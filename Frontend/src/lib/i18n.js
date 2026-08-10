import { writable, derived } from 'svelte/store';
import tr from '$lib/locales/messages_tr.json';
import az from '$lib/locales/messages_az.json';
import en from '$lib/locales/messages_en.json';
import ru from '$lib/locales/messages_ru.json';

const dicts = { tr, az, en, ru };
const supported = ['tr', 'az', 'en', 'ru'];
const DEFAULT = 'az';

function detect() {
    if (typeof localStorage !== 'undefined') {
        const saved = localStorage.getItem('kela-lang');
        if (saved && supported.includes(saved)) return saved;
    }
    if (typeof navigator !== 'undefined') {
        const nav = (navigator.language || '').split('-')[0];
        if (supported.includes(nav)) return nav;
    }
    return DEFAULT;
}

export const locale = writable(detect());

export function setLocale(lang) {
    if (!supported.includes(lang)) return;
    locale.set(lang);
    if (typeof localStorage !== 'undefined') localStorage.setItem('kela-lang', lang);
}

export const t = derived(locale, ($l) => {
    const dict = dicts[$l] || dicts[DEFAULT];
    return (key, params) => {
        let value = dict[key] != null ? dict[key] : key;
        if (params) {
            for (const k of Object.keys(params)) {
                value = value.split('{' + k + '}').join(params[k]);
            }
        }
        return value;
    };
});
