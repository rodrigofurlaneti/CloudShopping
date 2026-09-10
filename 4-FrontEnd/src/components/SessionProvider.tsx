import { useEffect, useState, useCallback, type ReactNode } from 'react';
import { request, post, resetCsrf } from '../services/http';
import type { Session, SessionUser } from '../services/storeService';
import { Navigate, useLocation } from 'react-router-dom';
import { can, pagePermission } from '../services/permissions';

import { SessionContext, useSession } from '../services/sessionContext';
export function SessionProvider({ children }: { children: ReactNode }) {
    const [user, setUser] = useState<SessionUser | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');
    const refresh = useCallback(async () => {
        try { const data = await request<Session>('/v1/session'); setUser(data.user); setError(''); }
        catch (e) { setUser(null); setError((e as Error).message); }
        finally { setLoading(false); }
    }, []);
    useEffect(() => {
        const changed = () => { resetCsrf(); void refresh(); };
        const expired = () => { setUser(null); resetCsrf(); };
        request<Session>('/v1/session').then(data => { setUser(data.user); setError(''); }).catch(e => setError(e.message)).finally(() => setLoading(false));
        window.addEventListener('session:changed', changed);
        window.addEventListener('session:expired', expired);
        return () => { window.removeEventListener('session:changed', changed); window.removeEventListener('session:expired', expired); };
    }, [refresh]);
    const logout = async () => {
        await post('/v1/session/logout'); resetCsrf(); setUser(null);
        sessionStorage.removeItem('checkout:pending'); window.dispatchEvent(new Event('cart:changed'));
        await refresh();
    };
    return <SessionContext.Provider value={{ user, loading, error, refresh, logout }}>{children}</SessionContext.Provider>;
}
export function AdminOnly({ children }: { children: ReactNode }) {
    const { user, loading, error, refresh } = useSession();
    const location = useLocation();
    if (loading) return <p className="p-8" role="status">Verificando sessão…</p>;
    if (error) return <div className="p-8" role="alert">{error} <button onClick={() => void refresh()}>Tentar novamente</button></div>;
    if (user?.role !== 'Administrator') return <Navigate to="/admin/login" state={{ from: location.pathname }} replace />;
    const permission = pagePermission(location.pathname);
    if (permission && !can(user, permission)) return <Navigate to="/admin/security" replace />;
    return <>{children}</>;
}

