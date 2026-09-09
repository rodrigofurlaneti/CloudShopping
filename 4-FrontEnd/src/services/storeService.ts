import { post, request, resetCsrf } from './http';
export interface SessionUser { id: number; name: string; role: 'Customer' | 'Administrator'; tenantId: number; isGuest: boolean }
export interface Session { user: SessionUser | null; csrfToken: string }
export interface Product { id: number; name: string; sku: string; price: number; departmentId: number; availableStock: number; image?: string; images?: string[] }
export interface Page<T> { items: T[]; page: number; totalCount: number; totalPages: number }
export interface CartLine { productId: number; name: string; sku: string; price: number; quantity: number; availableStock: number; image?: string }
export interface Cart { id: number; version: number; expiresAt: string; items: CartLine[]; subtotal: number }
export interface Address { id: number; street: string; number: string; neighborhood?: string; city: string; state: string; zipCode: string }
export interface Profile { email: string; name: string; type: 'B2C' | 'B2B'; taxId: string }
export interface Shipping { id: number; name: string; amount: number; estimatedDays: number; postalCodePrefix?: string; isActive?: boolean }
export interface Preview { token: string; cart: Cart; shippingName: string; shippingAmount: number; total: number; expiresAt: string }
export interface Order { id: number; totalAmount: number; shippingAmount: number; shippingMethod: string; orderStatusId: number; reservationState: string; reservationExpiresAt: string; items: { productId: number; name: string; sku: string; quantity: number; unitPrice: number }[]; address: Address }
export const money = (n: number) => new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(n);
let guestPromise: Promise<void> | undefined;
export function ensureCustomer() {
    if (!guestPromise) guestPromise = (async () => {
        const session = await request<Session>('/v1/session');
        if (session.user?.role === 'Administrator') throw new Error('Saia do painel administrativo para iniciar uma compra.');
        if (!session.user) { await post('/v1/session/guest'); resetCsrf(); window.dispatchEvent(new Event('session:changed')); }
    })().finally(() => { guestPromise = undefined; });
    return guestPromise;
}
export async function addToCart(productId: number, quantity: number) {
    await ensureCustomer();
    return post<Cart>('/v1/store/cart/items', { productId, quantity });
}

