export const API_BASE_URL = (import.meta.env.VITE_API_URL || '/api').replace(/\/$/, '');
export const STATIC_BASE_URL = (import.meta.env.VITE_STATIC_URL || '').replace(/\/$/, '');
const tenant = import.meta.env.DEV ? import.meta.env.VITE_DEV_TENANT_ID : undefined;
let csrf: string | undefined;
export class ApiError extends Error {
    status: number;
    constructor(status: number, message: string) { super(message); this.status = status; }
}
export function resetCsrf() { csrf = undefined; }
export function tenantHeaders(): Record<string, string> { return tenant ? { 'X-Tenant-Id': tenant } : {}; }
export async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
    const method = options.method || 'GET';
    if (!['GET', 'HEAD', 'OPTIONS'].includes(method) && !csrf) {
        const session = await request<{ csrfToken: string }>('/v1/session');
        csrf = session.csrfToken;
    }
    const headers = new Headers(options.headers);
    Object.entries(tenantHeaders()).forEach(([k,v]) => headers.set(k,v));
    if (options.body && !(options.body instanceof FormData)) headers.set('Content-Type', 'application/json');
    if (csrf && !['GET','HEAD','OPTIONS'].includes(method)) headers.set('X-CSRF-Token', csrf);
    const response = await fetch(API_BASE_URL + path, { ...options, credentials: 'include', headers });
    if (!response.ok) {
        const data = await response.json().catch(() => null);
        const fields = data?.errors ? (Object.values(data.errors).flat() as string[]).join(' ') : '';
        if (response.status === 401) window.dispatchEvent(new Event('session:expired'));
        throw new ApiError(response.status, fields || data?.message || data?.title || 'Não foi possível concluir a solicitação.');
    }
    if (response.status === 204) return undefined as T;
    const data = await response.json();
    if (path === '/v1/session') csrf = data.csrfToken;
    if (method !== 'GET' && path.startsWith('/v1/store/')) window.dispatchEvent(new Event('cart:changed'));
    return data as T;
}
export const post = <T>(path: string, body?: unknown) => request<T>(path, { method: 'POST', body: body === undefined ? undefined : JSON.stringify(body) });

