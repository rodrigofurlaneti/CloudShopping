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

export function BackofficeLayout({ children }: { children: ReactNode }) {
    const location=useLocation();const {user,logout}=useSession();const navigate=useNavigate();
    const [menuOpen,setMenuOpen]=useState(false);const [error,setError]=useState('');
    return <div className="admin-shell">
        <button className="admin-menu-toggle" aria-expanded={menuOpen} aria-controls="admin-navigation" onClick={()=>setMenuOpen(v=>!v)}>Menu administrativo</button>
        <aside id="admin-navigation" className={'admin-sidebar '+(menuOpen?'is-open':'')} onKeyDown={e=>{if(e.key==='Escape')setMenuOpen(false);}}>
            <div className="admin-sidebar-brand"><ShoppingCart size={24}/><strong>Loja {user?.tenantId}</strong></div>
            <nav aria-label="Administração">{NAV_ITEMS.filter(item=>!pagePermission(item.to)||can(user,pagePermission(item.to)!)).map(item=><Link key={item.to} to={item.to} aria-current={location.pathname===item.to?'page':undefined} onClick={()=>setMenuOpen(false)}><item.icon size={18}/>{item.label}</Link>)}</nav>
            <button onClick={()=>void logout().then(()=>navigate('/admin/login')).catch(e=>setError(e.message))}><LogOut size={18}/> Sair</button>
        </aside>
        <div className="admin-content"><header><Link to="/">Ver loja</Link><span>{user?.name}</span><Link to="/admin/notifications">Fila de notificações</Link></header>{error&&<p role="alert">{error}</p>}<div className="admin-workspace">{children}</div></div>
    </div>;
}
