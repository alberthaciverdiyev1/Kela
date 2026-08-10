import { writable } from 'svelte/store';
import { api } from '$lib/api.js';

export const user = writable(null);
export const ready = writable(false);

export async function loadUser() {
    try {
        const me = await api('/users/me');
        user.set(me);
    } catch (e) {
        user.set(null);
    } finally {
        ready.set(true);
    }
}

export async function login(email, password) {
    await api('/auth/login', {
        method: 'POST',
        body: JSON.stringify({ email, password })
    });
    const me = await api('/users/me');
    user.set(me);
    ready.set(true);
    return me;
}

export function logout() {
    user.set(null);
    ready.set(false);
}

export function homeFor(role) {
    switch (role) {
        case 'Teacher':
        case 'Admin':
            return '/teacher/dashboard';
        case 'Student':
            return '/student';
        case 'Parent':
            return '/parent';
        default:
            return '/auth/login';
    }
}
