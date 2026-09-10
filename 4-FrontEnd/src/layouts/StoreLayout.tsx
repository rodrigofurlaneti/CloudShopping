import { useEffect, useState, type ReactNode } from 'react';
import { Search, ShoppingBag, UserRound, ArrowUpRight, Package, Headphones, Bell, Heart } from 'lucide-react';
import './storefront.css';
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
        <a className="store-skip" href="#store-content">Pular para o conteúdo</a>
        <div className="store-topline"><span>Boas escolhas começam por aqui.</span><Link to="/support">Precisa de ajuda? <ArrowUpRight size={13}/></Link></div>
        <header className="store-header">
            <Link to="/" className="store-brand"><span className="brand-symbol" aria-hidden="true">C<span>·</span></span><span>{name}<small>SUA LOJA ONLINE</small></span></Link>
            <form className="store-search" role="search" onSubmit={e => { e.preventDefault(); navigate('/?search=' + encodeURIComponent(search.trim())); }}>
                <Search size={19} aria-hidden="true"/><label className="sr-only" htmlFor="search">Buscar produtos</label>
                <input id="search" value={search} onChange={e => setSearch(e.target.value)} placeholder="O que você está procurando?" />
                <button aria-label="Buscar produtos"><ArrowUpRight size={20}/></button>
            </form>
            <nav className="header-actions" aria-label="Conta e compras">
                <Link to="/account"><UserRound size={21}/><span>{user && !user.isGuest ? 'Minha conta' : 'Entrar'}</span></Link>
                <Link to="/cart" className="cart-access"><ShoppingBag size={21}/><span>Carrinho</span><b aria-label={`${count} itens no carrinho`}>{count}</b></Link>
            </nav>
        </header>
        <div className="store-nav"><nav aria-label="Navegação da loja"><Link to="/">Explorar catálogo</Link><Link to="/orders"><Package size={15}/>Meus pedidos</Link><Link to="/favorites"><Heart size={15}/>Favoritos</Link><Link to="/notifications"><Bell size={15}/>Notificações</Link><Link to="/support"><Headphones size={15}/>Atendimento</Link></nav>{user && <button onClick={() => void logout().catch(e => setError(e.message))}>Sair</button>}</div>
        <main id="store-content" className="store-main">{error && <p className="notice error" role="alert">{error}</p>}{children}</main>
        <footer className="store-footer"><div className="footer-intro"><strong>{name}<span>·</span></strong><p>Explore. Escolha. Encontre o que combina com você.</p></div><nav aria-label="Links do rodapé"><Link to="/account">Minha conta</Link><Link to="/orders">Acompanhar pedidos</Link><Link to="/support">Fale com a loja <ArrowUpRight size={14}/></Link></nav><div className="footer-bottom"><span>© {new Date().getFullYear()} {name} · CloudShopping</span><Link to="/admin/login">Área administrativa</Link></div></footer>
    </div>;
}
export default StoreLayout;
