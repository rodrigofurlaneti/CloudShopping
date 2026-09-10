import type { SessionUser } from './storeService';
export function can(user: SessionUser | null, permission: string) {
    return !!user?.permissions?.some(p => p === '*' || p === permission);
}
export function pagePermission(path: string): string | null {
    if (path === '/admin/security') return null;
    const module = ({ products: 'catalog', departments: 'catalog', imports: 'catalog', 'catalog-details': 'catalog', orders: 'orders', customers: 'customers', payments: 'finance', dashboard: 'reports', coupons: 'promotions', support: 'support', reviews: 'moderation', asaas: 'settings', shipping: 'settings', notifications: 'settings', banners: 'settings', 'order-sectors': 'settings', 'order-statuses': 'settings' } as Record<string, string>)[path.split('/')[2]];
    return module ? module + '.read' : '*';
}
