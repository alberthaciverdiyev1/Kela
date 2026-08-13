/**
 * Controller — Qeydlər (Google Keep üslubu).
 *
 * Üst kompakt "tez qeyd" qutusu, rəngli not kartları şəbəkəsi (masonry),
 * sabitləmə (pin), rəng palitrası, redaktə modulu və çöp qutusu.
 * Hər dəyişiklik /api/v1/notes endpointləri ilə avtomatik saxlanılır.
 */
import Alpine from 'alpinejs';

const PALETTE = {
    default: { bg: '#ffffff', label: 'Ağ' },
    yellow:  { bg: '#fef08a', label: 'Sarı' },
    blue:    { bg: '#bfdbfe', label: 'Mavi' },
    green:   { bg: '#bbf7d0', label: 'Yaşıl' },
    red:     { bg: '#fecaca', label: 'Qırmızı' },
    purple:  { bg: '#ddd6fe', label: 'Bənövşəyi' },
    teal:    { bg: '#99f6e4', label: 'Firuzə' },
    orange:  { bg: '#fed7aa', label: 'Narıncı' },
    gray:    { bg: '#e5e7eb', label: 'Boz' },
};

export default function notesApp(config) {
    return {
        // ── Vəziyyət ───────────────────────────────────────────────────────
        notes: [],
        trashed: [],
        showTrash: false,
        loading: false,
        saveState: 'idle',

        // Tez qeyd qutusu
        composerOpen: false,
        composerTitle: '',
        composerBody: '',
        composerColor: 'default',

        // Redaktə modulu (aktiv qeyd)
        editing: null,
        dirty: false,

        init() {
            this.loadNotes();
            window.addEventListener('keydown', (e) => {
                if (e.key === 'Escape' && this.editing) this.closeEditor();
            });
        },

        // ── Rəng palitrası ────────────────────────────────────────────────

        colorKeys() { return Object.keys(PALETTE); },
        colorOf(key) { return PALETTE[key] || PALETTE.default; },
        cardStyle(note) {
            return `background-color:${this.colorOf(note.color).bg}; color:#1f2937;`;
        },
        swatchStyle(key) {
            return `background-color:${this.colorOf(key).bg};`;
        },

        // ── Məlumat ───────────────────────────────────────────────────────

        async loadNotes() {
            this.loading = true;
            try {
                const res = await KelaApi('GET', '/api/v1/notes');
                this.notes = res?.data ?? [];
            } catch (err) {
                window.alert(err.message);
            } finally {
                this.loading = false;
            }
        },

        async loadTrash() {
            try {
                const res = await KelaApi('GET', '/api/v1/notes/trashed');
                this.trashed = res?.data ?? [];
            } catch (err) {
                window.alert(err.message);
            }
        },

        // Sabitlənmişlər əvvəl görünsün
        sortedNotes() {
            return [...this.notes].sort((a, b) => (b.is_pinned ? 1 : 0) - (a.is_pinned ? 1 : 0));
        },

        toggleTrash() {
            this.showTrash = !this.showTrash;
            if (this.showTrash) this.loadTrash();
        },

        // ── Tez qeyd qutusu ───────────────────────────────────────────────

        openComposer() { this.composerOpen = true; },

        async closeComposer() {
            const title = this.composerTitle.trim();
            const body = this.composerBody.trim();
            if (title || body) {
                try {
                    const res = await KelaApi('POST', '/api/v1/notes', {
                        title: title || null,
                        body: body || null,
                        color: this.composerColor,
                    });
                    this.notes.unshift(res.data);
                } catch (err) {
                    window.alert(err.message);
                }
            }
            this.composerOpen = false;
            this.composerTitle = '';
            this.composerBody = '';
            this.composerColor = 'default';
        },

        composerKeydown(e) {
            if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                this.closeComposer();
            }
        },

        // ── Redaktə modulu (avtomatik kayıt) ──────────────────────────────

        openEditor(note) {
            this.editing = note;
            this.dirty = false;
        },

        async closeEditor() {
            if (!this.editing) return;
            await this.saveEditor(true);
            this.editing = null;
            this.dirty = false;
        },

        scheduleSave() {
            this.dirty = true;
            clearTimeout(this._saveTimer);
            this._saveTimer = setTimeout(() => this.saveEditor(false), 600);
        },

        async saveEditor(force = true) {
            if (!this.editing) return;
            if (!force && !this.dirty) return;
            this.saveState = 'saving';
            try {
                const res = await KelaApi('PUT', `/api/v1/notes/${this.editing.id}`, {
                    title: this.editing.title || null,
                    body: this.editing.body || null,
                    color: this.editing.color,
                    is_pinned: this.editing.is_pinned,
                });
                Object.assign(this.editing, res.data);
                this.dirty = false;
                this.saveState = 'saved';
                clearTimeout(this._saveTimer);
                this._saveResetTimer = setTimeout(() => { this.saveState = 'idle'; }, 1200);
            } catch (err) {
                this.saveState = 'idle';
                window.alert(err.message);
            }
        },

        // ── Kart əməliyyatları ────────────────────────────────────────────

        async setColor(note, color) {
            note.color = color;
            if (this.editing === note) {
                this.dirty = true;
                this.scheduleSave();
            } else {
                try {
                    const res = await KelaApi('PUT', `/api/v1/notes/${note.id}`, { color });
                    Object.assign(note, res.data);
                } catch (err) {
                    window.alert(err.message);
                }
            }
        },

        async togglePin(note) {
            note.is_pinned = !note.is_pinned;
            if (this.editing === note) {
                this.dirty = true;
                this.scheduleSave();
            } else {
                try {
                    const res = await KelaApi('PUT', `/api/v1/notes/${note.id}`, { is_pinned: note.is_pinned });
                    Object.assign(note, res.data);
                } catch (err) {
                    window.alert(err.message);
                }
            }
        },

        async deleteNote(note) {
            const wasEditing = this.editing === note;
            try {
                await KelaApi('DELETE', `/api/v1/notes/${note.id}`);
                if (wasEditing) this.editing = null;
                this.notes = this.notes.filter((n) => n.id !== note.id);
                this.trashed = this.trashed.filter((n) => n.id !== note.id);
                if (this.showTrash) await this.loadTrash();
            } catch (err) {
                window.alert(err.message);
            }
        },

        async restoreNote(note) {
            try {
                const res = await KelaApi('POST', `/api/v1/notes/${note.id}/restore`);
                this.trashed = this.trashed.filter((n) => n.id !== note.id);
                this.notes.unshift(res.data);
            } catch (err) {
                window.alert(err.message);
            }
        },
    };
}

// Alpine-də qeydiyyat: x-data="notesApp(...)" işləsin.
Alpine.data('notesApp', notesApp);
// Bu entry yalnız qeydlər səhifələrində yüklənir. start() idempotentdir.
Alpine.start();
