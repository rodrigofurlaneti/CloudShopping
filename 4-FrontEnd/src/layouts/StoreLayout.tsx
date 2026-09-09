import { useEffect, useState, type ReactNode } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { request } from '../services/http';
import { useSession } from '../services/sessionContext';
import type { Cart } from '../services/storeService';
export function StoreLayout({ children }: { children: ReactNode }) {
    const [name, setName] = useState('CloudShopping');
    const [count, setCount] = useState(0);
    const [search, setSearch] = useState('');
    const [error, setError] = useState('');
    const navigate = useNavigate();
    const { user, logout } = useSession();
    useEffect(() => { request<{ companyName: string }>('/v1/store/context').then(x => setName(x.companyName)).catch(e => setError(e.message)); }, []);
    useEffect(() => {
        const update = () => {
            if (user?.role === 'Customer') request<Cart>('/v1/store/cart').then(x => setCount(x.items.reduce((n,i) => n+i.quantity,0))).catch(() => setCount(0));
            else setCount(0);
        };
        update(); window.addEventListener('cart:changed', update);
        return () => window.removeEventListener('cart:changed', update);
    }, [user]);
    return <div className="store-shell">
        <header className="store-header">
            <Link to="/" className="store-brand">{name}</Link>
            <form className="store-search" onSubmit={e => { e.preventDefault(); navigate('/?search=' + encodeURIComponent(search)); }}>
                <label className="sr-only" htmlFor="search">Buscar produtos</label>
                <input id="search" value={search} onChange={e => setSearch(e.target.value)} placeholder="Buscar produtos ou SKU" />
                <button>Buscar</button>
            </form>
            <nav aria-label="Conta e compras">
                <Link to="/account">{user ? 'Minha conta' : 'Entrar'}</Link>
                <Link to="/orders">Meus pedidos</Link>
                <Link to="/cart">Carrinho ({count})</Link>
                {user && <button onClick={() => void logout().catch(e => setError(e.message))}>Sair</button>}
            </nav>
        </header>
        <main className="store-main">{error && <p className="notice error" role="alert">{error}</p>}{children}</main>
        <footer className="store-footer"><span>© CloudShopping · {name}</span><Link to="/admin/login">Área administrativa</Link></footer>
    </div>;
}
export default StoreLayout;
