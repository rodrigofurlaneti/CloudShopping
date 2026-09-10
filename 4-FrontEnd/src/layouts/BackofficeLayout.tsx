import './backoffice.css';
import { Search, Menu, X, ArrowUpRight, ChevronRight, ShieldCheck } from 'lucide-react';
import { useSession } from '../services/sessionContext';
import { can, pagePermission } from '../services/permissions';
import { useState, type ReactNode } from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import {
    Home,
    ClipboardList,
    Package,
    Tags,
    Warehouse,
    Flag,
    Users,
    Settings,
    LogOut,
    Bell,
    ShoppingCart,
} from 'lucide-react';

interface NavItem {
    label: string;
    to: string;
    icon: React.ComponentType<{ size?: number; className?: string }>;
    badge?: number;
}

const NAV_ITEMS: NavItem[] = [
    { label: 'Perfis e permissões', to: '/admin/access', icon: Users },
    { label: 'Meu acesso', to: '/admin/security', icon: Users },
    { label: 'Atendimento', to: '/admin/support', icon: Users },
    { label: 'Notificações', to: '/admin/notifications', icon: Bell },
    { label: 'Cupons', to: '/admin/coupons', icon: Tags },
    { label: 'Importação', to: '/admin/imports', icon: Package },
    { label: 'Ficha e variantes', to: '/admin/catalog-details', icon: Tags },
    { label: 'Avaliações', to: '/admin/reviews', icon: ClipboardList },
    { label: 'Pagamentos Asaas', to: '/admin/asaas', icon: Settings },
    { label: 'Financeiro', to: '/admin/payments', icon: ClipboardList },
    { label: 'Dashboard', to: '/admin/dashboard', icon: Home },
    { label: 'Pedidos', to: '/admin/orders', icon: ClipboardList },
    { label: 'Setores Logísticos', to: '/admin/order-sectors', icon: Warehouse },
    { label: 'Status de Pedido', to: '/admin/order-statuses', icon: Flag },
    { label: 'Produtos', to: '/admin/products', icon: Package },
    { label: 'Departamentos', to: '/admin/departments', icon: Tags },
    { label: 'Banner', to: '/admin/banners', icon: Tags },
    { label: 'Entregas', to: '/admin/shipping', icon: Settings },
    { label: 'Clientes', to: '/admin/customers', icon: Users },
];

const NAV_GROUPS = [
    { label: 'Visão geral', paths: ['dashboard', 'orders', 'payments', 'customers'] },
    { label: 'Catálogo e vitrine', paths: ['products', 'departments', 'catalog-details', 'imports', 'banners', 'coupons'] },
    { label: 'Operação', paths: ['shipping', 'order-sectors', 'order-statuses', 'support', 'reviews', 'notifications'] },
    { label: 'Configurações', paths: ['asaas', 'access', 'security'] },
];

export function BackofficeLayout({ children }: { children: ReactNode }) {
    const location=useLocation();const {user,logout}=useSession();const navigate=useNavigate();
    const [menuOpen,setMenuOpen]=useState(false);const [error,setError]=useState('');
    const [search,setSearch]=useState('');const [leaving,setLeaving]=useState(false);
    const normalize=(value:string)=>value.normalize('NFD').replace(/[\u0300-\u036f]/g,'').toLowerCase();
    const visibleItems=NAV_ITEMS.filter(item=>!pagePermission(item.to)||can(user,pagePermission(item.to)!));
    const matches=visibleItems.filter(item=>normalize(item.label).includes(normalize(search.trim())));
    const current=NAV_ITEMS.find(item=>item.to===location.pathname)?.label || 'Painel administrativo';
    const initials=(user?.name || 'Administrador').trim().split(/\s+/).slice(0,2).map(part=>part[0]).join('').toUpperCase();
    return <div className="admin-shell">
        <a className="admin-skip" href="#admin-main">Pular para o conteúdo</a>
        <button className="admin-menu-toggle" aria-expanded={menuOpen} aria-controls="admin-navigation" onClick={()=>setMenuOpen(v=>!v)}>{menuOpen?<X size={20}/>:<Menu size={20}/>}<span>{menuOpen?'Fechar menu':'Menu administrativo'}</span><strong>CloudShopping<span>·</span></strong></button>
        <aside id="admin-navigation" className={'admin-sidebar '+(menuOpen?'is-open':'')} onKeyDown={e=>{if(e.key==='Escape'){setMenuOpen(false);document.querySelector<HTMLButtonElement>('.admin-menu-toggle')?.focus();}}}>
            <Link to="/admin/security" className="admin-sidebar-brand"><span className="admin-logo"><ShoppingCart size={22}/></span><span><strong>CloudShopping<span>·</span></strong><small>PAINEL DE GESTÃO</small></span></Link>
            <div className="admin-store-identity"><span className="admin-store-avatar">{user?.tenantId || '—'}</span><div><strong>Loja {user?.tenantId}</strong><small>Área administrativa</small></div><ShieldCheck size={16}/></div>
            <label className="admin-module-search"><Search size={16}/><span className="sr-only">Buscar módulo</span><input value={search} onChange={e=>setSearch(e.target.value)} placeholder="Buscar módulo…" type="search"/></label>
            <nav aria-label="Administração">{NAV_GROUPS.map(group=>{
                const items=group.paths.map(path=>matches.find(item=>item.to==='/admin/'+path)).filter((item):item is NavItem=>!!item);
                return items.length ? <section className="admin-nav-group" key={group.label}><h2>{group.label}</h2>{items.map(item=><Link key={item.to} to={item.to} aria-current={location.pathname===item.to?'page':undefined} onClick={()=>setMenuOpen(false)}><item.icon size={17}/><span>{item.label}</span>{location.pathname===item.to&&<ChevronRight size={14}/>}</Link>)}</section>:null;
            })}{!matches.length&&<p className="admin-no-modules" role="status">Nenhum módulo encontrado.</p>}</nav>
            <div className="admin-sidebar-bottom"><span className="admin-avatar">{initials}</span><div><strong>{user?.name || 'Administrador'}</strong><small>Conta administrativa</small></div><button title="Sair da conta" aria-label="Sair da conta" disabled={leaving} onClick={()=>{setLeaving(true);void logout().then(()=>navigate('/admin/login')).catch(e=>{setError(e.message);setLeaving(false);});}}><LogOut size={18}/></button></div>
        </aside>
        <div className="admin-content"><header className="admin-topbar"><div className="admin-breadcrumb"><span>Painel</span><ChevronRight size={14}/><strong>{current}</strong></div><div className="admin-topbar-actions"><Link className="admin-view-store" to="/">Ver loja <ArrowUpRight size={15}/></Link>{visibleItems.some(i=>i.to==='/admin/notifications')&&<Link className="admin-notification-link" to="/admin/notifications" aria-label="Fila de notificações"><Bell size={19}/></Link>}<Link className="admin-profile-link" to="/admin/security" aria-label="Meu acesso"><span className="admin-avatar">{initials}</span></Link></div></header>{error&&<p className="admin-layout-error" role="alert">{error}</p>}<div id="admin-main" className="admin-workspace" tabIndex={-1}>{children}</div><footer className="admin-footer"><span>CloudShopping · Gestão da sua loja</span><Link to="/admin/security">Minha conta <ArrowUpRight size={12}/></Link></footer></div>
    </div>;
}
