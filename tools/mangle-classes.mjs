#!/usr/bin/env node
// Rewrites the class selectors of the built Tailwind bundles to random names and
// applies the same mapping to the markup sources, so production HTML/CSS carries no
// readable class names. Runs only inside deploy.sh (dev builds keep the real names).
//
//   node tools/mangle-classes.mjs --css <bundle.css>... --root <dir>... --map <out.json> --backup <dir>
//   node tools/mangle-classes.mjs --restore <backup dir>
//
// Discovery is conservative: a class is renamed only when it is both a selector in the
// bundle and a word in the sources, and never when it can be reached from code we do
// not rewrite (vendored libraries, dynamic prefix fragments, lowercase member access).

import fs from 'node:fs';
import path from 'node:path';
import crypto from 'node:crypto';

const SOURCE_EXTENSIONS = new Set(['.cshtml', '.razor', '.html', '.js', '.cs']);
const SKIP_DIRS = new Set(['node_modules', 'bin', 'obj', '.git', '.vscode', 'uploads']);
// Files whose classes are produced or consumed by code outside the rewrite:
// compiled third-party runtimes and the certificate templates (Tailwind Play reads
// the class names from the markup at runtime).
const SKIP_PATHS = [
    `${path.sep}js${path.sep}library${path.sep}`,
    `${path.sep}wwwroot${path.sep}lib${path.sep}`,
    `${path.sep}certificate-templates${path.sep}`,
    '.min.js',
];

// Tailwind utilities that double as C# keywords (`static`, `fixed`, `inline`, `table`,
// `block`, `hidden`). Inline `@code` blocks are not parsed, so these keep their names.
const CSHARP_KEYWORDS = new Set([
    'abstract', 'as', 'base', 'bool', 'break', 'byte', 'case', 'catch', 'char', 'checked',
    'class', 'const', 'continue', 'decimal', 'default', 'delegate', 'do', 'double', 'else',
    'enum', 'event', 'explicit', 'extern', 'false', 'finally', 'fixed', 'float', 'for',
    'foreach', 'goto', 'if', 'implicit', 'in', 'int', 'interface', 'internal', 'is', 'lock',
    'long', 'namespace', 'new', 'null', 'object', 'operator', 'out', 'override', 'params',
    'private', 'protected', 'public', 'readonly', 'ref', 'return', 'sbyte', 'sealed',
    'short', 'sizeof', 'stackalloc', 'static', 'string', 'struct', 'switch', 'this', 'throw',
    'true', 'try', 'typeof', 'uint', 'ulong', 'unchecked', 'unsafe', 'ushort', 'using',
    'virtual', 'void', 'volatile', 'while',
]);

const args = process.argv.slice(2);
const options = { css: [], roots: [], map: null, backup: null, restore: null, explain: [], seed: crypto.randomBytes(8).toString('hex') };

for (let i = 0; i < args.length; i++) {
    const arg = args[i];
    const next = () => {
        const value = args[++i];
        if (value === undefined) throw new Error(`${arg} needs a value`);
        return value;
    };
    switch (arg) {
        case '--css': options.css.push(next()); break;
        case '--root': options.roots.push(next()); break;
        case '--map': options.map = next(); break;
        case '--backup': options.backup = next(); break;
        case '--seed': options.seed = next(); break;
        case '--restore': options.restore = next(); break;
        case '--explain': options.explain.push(next()); break;
        default: throw new Error(`unknown argument: ${arg}`);
    }
}

if (options.restore) {
    restore(options.restore);
    process.exit(0);
}

if (!options.css.length || !options.roots.length) {
    console.error('usage: mangle-classes.mjs --css <bundle.css>... --root <dir>... [--map <out.json>] [--backup <dir>] [--seed <value>]');
    process.exit(2);
}

// ---------------------------------------------------------------- CSS parsing

function skipString(css, i) {
    const quote = css[i];
    i++;
    while (i < css.length) {
        if (css[i] === '\\') i += 2;
        else if (css[i] === quote) return i + 1;
        else i++;
    }
    return i;
}

// Reads a CSS identifier starting at `i`, unescaping it. Escapes are written as `\X`
// (e.g. `hover\:flex`) or as `\HHHHHH ` hex codes (leading digits, e.g. `\32 xl`).
function readIdentifier(text, i) {
    let decoded = '';
    while (i < text.length) {
        const ch = text[i];
        if (ch === '\\') {
            const hex = /^\\([0-9a-fA-F]{1,6})[ \t]?/.exec(text.slice(i, i + 8));
            if (hex) {
                decoded += String.fromCodePoint(parseInt(hex[1], 16));
                i += hex[0].length;
            } else if (i + 1 < text.length) {
                decoded += text[i + 1];
                i += 2;
            } else {
                i++;
            }
            continue;
        }
        if (/[A-Za-z0-9_-]/.test(ch)) {
            decoded += ch;
            i++;
            continue;
        }
        break;
    }
    return { decoded, end: i };
}

// Collects the class names of every selector prelude. Segment boundaries are the
// characters that end a declaration or a block, so declarations (`overflow:hidden`)
// are never mistaken for selectors.
function cssClassNames(css, into) {
    let segmentStart = 0;
    let i = 0;
    while (i < css.length) {
        const ch = css[i];
        // Escaped characters belong to an identifier (`font-\[\'DM_Sans\'\]`).
        if (ch === '\\') { i += 2; continue; }
        if (ch === '"' || ch === "'") { i = skipString(css, i); continue; }
        if (ch === '/' && css[i + 1] === '*') { const end = css.indexOf('*/', i + 2); i = end < 0 ? css.length : end + 2; continue; }
        if (ch === '{') {
            const prelude = css.slice(segmentStart, i);
            if (!prelude.trimStart().startsWith('@')) {
                for (let p = 0; p < prelude.length; p++) {
                    if (prelude[p] !== '.') continue;
                    const { decoded, end } = readIdentifier(prelude, p + 1);
                    if (decoded && !/^\d+$/.test(decoded)) into.add(decoded);
                    p = Math.max(p, end - 1);
                }
            }
            i++;
            segmentStart = i;
            continue;
        }
        if (ch === ';' || ch === '}') { i++; segmentStart = i; continue; }
        i++;
    }
    return into;
}

// ------------------------------------------------------------- source walking

function walk(dir, files) {
    for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
        if (entry.name.startsWith('.') && entry.isDirectory()) continue;
        const full = path.join(dir, entry.name);
        if (entry.isDirectory()) {
            if (SKIP_DIRS.has(entry.name)) continue;
            walk(full, files);
            continue;
        }
        if (!SOURCE_EXTENSIONS.has(path.extname(entry.name))) continue;
        if (SKIP_PATHS.some(p => full.includes(p) || entry.name.endsWith(p))) continue;
        files.push(full);
    }
    return files;
}

// Same shape the rewriter accepts, so `lg:shrink-0` counts as used, not `lg` + `shrink-0`.
const classToken = /[A-Za-z0-9_!:/[\].%#&>~*-]+/g;

function scanDiscovery(texts, classNames) {
    const used = new Set();
    const protectedNames = new Map();

    for (const { file, text } of texts) {
        for (const match of text.matchAll(classToken)) used.add(match[0].replace(/^[[.]+/, ''));

        // A declared variable that is also a class name keeps the class name: the rewriter
        // replaces bare words, so `const collapse = ...` would be renamed while
        // `collapse.classList` (one token, `collapse.classList`) stayed intact.
        if (file.endsWith('.js')) {
            for (const match of text.matchAll(/(?:\b(?:const|let|var)\s+|\bfunction\s+)([A-Za-z_$][\w$]*)/g)) {
                if (classNames.has(match[1]) && !protectedNames.has(match[1])) {
                    protectedNames.set(match[1], `declared variable in ${file}`);
                }
            }
        }

        // Lowercase member access (`el.hidden`) would become a renamed property;
        // the same name as a method call (`AdminUI.badge(`) is unrelated.
        for (const match of text.matchAll(/(?<=[A-Za-z0-9_)\]?])\s*\.\s*([a-z][a-z0-9_-]*)(?!\s*\()/g)) {
            if (!protectedNames.has(match[1])) protectedNames.set(match[1], `member access in ${file}`);
        }

        // Fragments whose class is assembled at runtime: `badge-${variant}`.
        for (const match of text.matchAll(/([A-Za-z][A-Za-z0-9_-]*-)(?:\$\{|\{)/g)) {
            protectedNames.set(`${match[1]}*`, `dynamic fragment in ${file}`);
        }
    }

    return { used, protectedNames };
}

// ------------------------------------------------------------------ renaming

const alphabet = 'abcdefghijklmnopqrstuvwxyz0123456789';

function newName(taken, random) {
    for (;;) {
        let name = 'x';
        for (let i = 0; i < 5; i++) name += alphabet[Math.floor(random() * alphabet.length)];
        if (!taken.has(name)) {
            taken.add(name);
            return name;
        }
    }
}

function mulberry32(seed) {
    let a = 0;
    for (const ch of seed) a = (Math.imul(a ^ ch.charCodeAt(0), 0x01000193) >>> 0);
    return () => {
        a = (a + 0x6d2b79f5) >>> 0;
        let t = a;
        t = Math.imul(t ^ (t >>> 15), t | 1);
        t ^= t + Math.imul(t ^ (t >>> 7), t | 61);
        return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
    };
}

// ---------------------------------------------------------------- rewriting

function escapeRegExp(value) {
    return value.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
}

function classPattern(map) {
    const names = [...map.keys()].sort((a, b) => b.length - a.length);
    // Longest first: `hover:py-2` must win over `py-2` at the same position.
    // The trailing `.` is part of a class (`mt-0.5`); the leading one is left free so
    // that selector strings (`querySelector('.admin-tab')`) are rewritten too.
    return new RegExp(`(?<![\\w-])(${names.map(escapeRegExp).join('|')})(?![\\w.-])`, 'g');
}

// C# sources: only string literal contents may be rewritten — a CSS name can also be an
// identifier or keyword in code (`private static readonly`, `var fixed`). Interpolation
// holes (`$"{value}"`) are expressions and stay untouched.
function rewriteCsStrings(text, pattern, map) {
    const parts = [];
    let segmentStart = 0;

    const emit = (start, end, replaceable) => {
        const body = text.slice(start, end);
        parts.push(text.slice(segmentStart, start),
            replaceable ? body.replace(pattern, (_, name) => map.get(name)) : body);
        segmentStart = end;
    };

    let i = 0;
    while (i < text.length) {
        const ch = text[i];

        if (ch === '/' && text[i + 1] === '/') {
            const end = text.indexOf('\n', i);
            i = end < 0 ? text.length : end + 1;
            continue;
        }
        if (ch === '/' && text[i + 1] === '*') {
            const end = text.indexOf('*/', i + 2);
            i = end < 0 ? text.length : end + 2;
            continue;
        }
        if (ch === "'") {
            i = skipCsQuoted(text, i, "'");
            continue;
        }

        let j = i;
        let verbatim = false;
        let interpolated = false;
        while (j < text.length && (text[j] === '$' || text[j] === '@')) {
            if (text[j] === '@') verbatim = true; else interpolated = true;
            j++;
        }
        if (text[j] !== '"') {
            i = j > i ? j : i + 1;
            continue;
        }

        let quotes = 0;
        while (text[j + quotes] === '"') quotes++;

        if (quotes >= 3) {
            const close = text.indexOf('"'.repeat(quotes), j + quotes);
            const end = close < 0 ? text.length : close;
            emit(j + quotes, end, true);
            i = close < 0 ? text.length : close + quotes;
            continue;
        }

        const contentStart = j + 1;
        const close = skipCsQuoted(text, contentStart - 1, '"', verbatim);
        const contentEnd = Math.min(close, text.length);

        // Class lists never start with `/`; route strings (`$"/api/x/{id}/contents"`) do,
        // and renaming a path segment would break the request.
        const firstHole = text.indexOf('{', contentStart);
        const headEnd = interpolated && firstHole >= 0 && firstHole < contentEnd ? firstHole : contentEnd;
        if (/^\s*(\/|https?:\/\/)/.test(text.slice(contentStart, headEnd))) {
            emit(contentStart, contentEnd, false);
            i = close >= text.length ? text.length : close + 1;
            continue;
        }

        if (!interpolated) {
            emit(contentStart, contentEnd, true);
        } else {
            // Split the literal into plain runs and `{...}` holes.
            let runStart = contentStart;
            let k = contentStart;
            while (k < contentEnd) {
                if (!verbatim && text[k] === '\\') { k += 2; continue; }
                if (verbatim && text[k] === '"' && text[k + 1] === '"') { k += 2; continue; }
                if (text[k] === '{' && text[k + 1] === '{') { k += 2; continue; }
                if (text[k] !== '{') { k++; continue; }
                let depth = 1;
                let h = k + 1;
                while (h < contentEnd && depth > 0) {
                    const c = text[h];
                    if (c === '"') { h = skipCsQuoted(text, h, '"') + 1; continue; }
                    if (c === "'") { h = skipCsQuoted(text, h, "'") + 1; continue; }
                    if (c === '{') depth++;
                    else if (c === '}') depth--;
                    h++;
                }
                emit(runStart, k, true);
                emit(k, h, false);
                runStart = h;
                k = h;
            }
            emit(runStart, contentEnd, true);
        }

        i = close >= text.length ? text.length : close + 1;
    }

    parts.push(text.slice(segmentStart));
    return parts.join('');
}

// Returns the index of the delimiter closing the literal opened at `open`
// (`open` itself may be the quote or the first content char index passed as quote-1).
function skipCsQuoted(text, open, quote, verbatim = false) {
    let i = open + 1;
    while (i < text.length) {
        const ch = text[i];
        if (ch === '\\' && !verbatim) { i += 2; continue; }
        if (ch === quote) {
            if (quote === '"' && verbatim && text[i + 1] === '"') { i += 2; continue; }
            return i;
        }
        i++;
    }
    return text.length;
}

// Everything between two boundaries is emitted verbatim except selector preludes, so
// strings and comments only need to be stepped over — never emitted separately.
function rewriteCss(css, map) {
    const parts = [];
    let segmentStart = 0;
    let i = 0;
    while (i < css.length) {
        const ch = css[i];
        if (ch === '\\') { i += 2; continue; }
        if (ch === '"' || ch === "'") { i = skipString(css, i); continue; }
        if (ch === '/' && css[i + 1] === '*') {
            const end = css.indexOf('*/', i + 2);
            i = end < 0 ? css.length : end + 2;
            continue;
        }
        if (ch === '{') {
            parts.push(rewritePrelude(css.slice(segmentStart, i), map), '{');
            i++;
            segmentStart = i;
            continue;
        }
        if (ch === ';' || ch === '}') {
            parts.push(css.slice(segmentStart, i), ch);
            i++;
            segmentStart = i;
            continue;
        }
        i++;
    }
    parts.push(css.slice(segmentStart));
    return parts.join('');
}

function rewritePrelude(prelude, map) {
    if (prelude.trimStart().startsWith('@')) return prelude;
    let out = '';
    let i = 0;
    while (i < prelude.length) {
        if (prelude[i] !== '.') { out += prelude[i++]; continue; }
        const { decoded, end } = readIdentifier(prelude, i + 1);
        if (!decoded) { out += '.'; i++; continue; }
        out += '.' + (map.get(decoded) ?? prelude.slice(i + 1, end));
        i = end;
    }
    return out;
}

const backupPaths = [];

function backupFile(dir, file) {
    fs.mkdirSync(path.join(dir, 'files'), { recursive: true });
    fs.copyFileSync(file, path.join(dir, 'files', `${backupPaths.length}.bak`));
    backupPaths.push(file);
}

function restore(dir) {
    const index = JSON.parse(fs.readFileSync(path.join(dir, 'index.json'), 'utf8'));
    for (const [i, file] of index.entries()) {
        fs.copyFileSync(path.join(dir, 'files', `${i}.bak`), file);
    }
    fs.rmSync(dir, { recursive: true, force: true });
    console.log(`restored ${index.length} files`);
}

// ------------------------------------------------------------------- driver

const cssFiles = options.css.map(file => path.resolve(file));
const rootDirs = options.roots.map(dir => path.resolve(dir));

const bundles = cssFiles.map(file => {
    const css = fs.readFileSync(file, 'utf8');
    return { file, css, classes: cssClassNames(css, new Set()) };
});

const sourceFiles = rootDirs.flatMap(dir => walk(dir, []));
const sources = sourceFiles.map(file => ({ file, text: fs.readFileSync(file, 'utf8') }));

const classNames = new Set(bundles.flatMap(b => [...b.classes]));
const { used, protectedNames } = scanDiscovery(sources, classNames);

const dynamicPrefixes = [...protectedNames.keys()].filter(name => name.endsWith('*')).map(name => name.slice(0, -1));

const candidates = [...classNames].filter(name => {
    if (!used.has(name)) return false;
    if (protectedNames.has(name)) return false;
    if (CSHARP_KEYWORDS.has(name)) return false;
    return !dynamicPrefixes.some(prefix => name.startsWith(prefix));
});

const random = mulberry32(options.seed);
const taken = new Set([...classNames, ...used]);
const map = new Map();
for (const name of candidates.sort()) map.set(name, newName(taken, random));

const backupDir = options.backup ? path.resolve(options.backup) : null;
if (backupDir) fs.rmSync(backupDir, { recursive: true, force: true });
const written = [];

for (const bundle of bundles) {
    const rewritten = rewriteCss(bundle.css, map);
    if (backupDir) backupFile(backupDir, bundle.file);
    fs.writeFileSync(bundle.file, rewritten);
    written.push(bundle.file);
}

const pattern = classPattern(map);

for (const source of sources) {
    const rewritten = source.file.endsWith('.cs')
        ? rewriteCsStrings(source.text, pattern, map)
        : source.text.replace(pattern, (_, name) => map.get(name));
    if (rewritten === source.text) continue;
    if (backupDir) backupFile(backupDir, source.file);
    fs.writeFileSync(source.file, rewritten);
    written.push(source.file);
}

if (backupDir) {
    fs.writeFileSync(path.join(backupDir, 'index.json'), JSON.stringify(backupPaths, null, 2));
}

if (options.map) {
    fs.writeFileSync(path.resolve(options.map), JSON.stringify(Object.fromEntries([...map].sort()), null, 2));
}

if (options.explain) {
    for (const name of options.explain) {
        const prefix = dynamicPrefixes.find(p => name.startsWith(p));
        const reason = !classNames.has(name) ? 'not a selector in the bundle'
            : !used.has(name) ? 'not used in sources'
                : protectedNames.get(name) ?? (prefix ? `dynamic prefix ${prefix}` : 'renamed');
        console.log(`explain ${name}: ${reason}${map.has(name) ? ` -> ${map.get(name)}` : ''}`);
    }
}

const untouched = [...classNames].filter(name => !map.has(name));
console.log(`classes in bundles: ${classNames.size} | renamed: ${map.size} | kept: ${untouched.length} | files rewritten: ${written.length}`);
if (untouched.length) {
    const sample = untouched.slice(0, 15).join(' ');
    console.log(`kept (dynamic/vendored/unused): ${sample}${untouched.length > 15 ? ' …' : ''}`);
}
