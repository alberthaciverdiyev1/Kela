import axios from 'axios';

export class ApiError extends Error {
    constructor(message, status, errors) {
        super(message);
        this.status = status;
        this.errors = errors || [];
    }
}

export const http = axios.create({
    baseURL: '/api',
    withCredentials: true,
    headers: {
        'Content-Type': 'application/json',
        'X-Requested-With': 'XMLHttpRequest'
    }
});

http.interceptors.response.use(
    (res) => res,
    (err) => {
        if (err.response && err.response.status === 401) {
            const onAuth = window.location.pathname.startsWith('/auth/');
            if (!onAuth) {
                window.location.href = '/auth/login';
            }
        }
        return Promise.reject(err);
    }
);

export async function api(path, options = {}) {
    const method = (options.method || 'GET').toLowerCase();
    try {
        const res = await http.request({
            url: path,
            method,
            data: options.body ? JSON.parse(options.body) : undefined
        });
        if (res.status === 204) return null;
        const body = res.data;
        if (body && body.success === false) {
            throw new ApiError(body.message || 'Beklenmeyen bir hata oluştu.', res.status, body.errors || []);
        }
        return body ? body.data : null;
    } catch (err) {
        if (err instanceof ApiError) throw err;
        if (err.response) {
            const body = err.response.data;
            throw new ApiError((body && body.message) || 'Beklenmeyen bir hata oluştu.', err.response.status, body ? body.errors : []);
        }
        throw new ApiError('Sunucuya ulaşılamadı.', 0);
    }
}

export default http;
