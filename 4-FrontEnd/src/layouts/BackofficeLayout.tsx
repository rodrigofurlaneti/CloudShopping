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
    LineChart,
    Megaphone,
    BarChart3,
    Wallet,
    Settings,
    HelpCircle,
    LogOut,
    Search,
    Bell,
    ChevronDown,
    ShoppingCart,
} from 'lucide-react';

interface NavItem {
    label: string;
    to: string;
    icon: React.ComponentType<{ size?: number; className?: string }>;
    badge?: number;
}

const NAV_ITEMS: NavItem[] = [
    { label: 'Dashboard', to: '/admin/dashboard', icon: Home },
    { label: 'Pedidos', to: '/admin/orders', icon: ClipboardList, badge: 12 },
    { label: 'Setores Logísticos', to: '/admin/order-sectors', icon: Warehouse },
    { label: 'Status de Pedido', to: '/admin/order-statuses', icon: Flag },
    { label: 'Produtos', to: '/admin/products', icon: Package },
    { label: 'Departamentos', to: '/admin/departments', icon: Tags },
    { label: 'Banner', to: '/admin/banners', icon: Tags },
    { label: 'Categorias', to: '/admin/categories', icon: Tags },
    { label: 'Clientes', to: '/admin/customers', icon: Users },
    { label: 'Vendas', to: '/admin/sales', icon: LineChart },
    { label: 'Promoções', to: '/admin/promotions', icon: Megaphone },
    { label: 'Relatórios', to: '/admin/reports', icon: BarChart3 },
    { label: 'Financeiro', to: '/admin/finance', icon: Wallet },
    { label: 'Configurações', to: '/admin/settings', icon: Settings },
    { label: 'Suporte', to: '/admin/support', icon: HelpCircle },
];

export function BackofficeLayout({ children }: { children: ReactNode }) {
    const location = useLocation();
    const navigate = useNavigate();
    const [searchTerm, setSearchTerm] = useState('');

    return (
        <div className="min-h-screen bg-slate-50 flex">
            {/* Sidebar */}
            <aside className="w-64 shrink-0 bg-slate-900 text-white flex flex-col">
                <div className="px-5 py-5 border-b border-slate-800">
                    <div className="flex items-center gap-2">
                        <div className="w-9 h-9 rounded-lg bg-orange-500 flex items-center justify-center">
                            <ShoppingCart size={18} className="text-white" />
                        </div>
                        <div>
                            <p className="text-lg font-bold leading-tight">MinhaLoja</p>
                            <p className="text-[11px] text-slate-400 leading-tight">Painel Administrativo</p>
                        </div>
                    </div>
                </div>

                <nav className="flex-1 px-3 py-4 space-y-1 overflow-y-auto">
                    {NAV_ITEMS.map((item) => {
                        const isActive = location.pathname === item.to;
                        const Icon = item.icon;
                        return (
                            <Link
                                key={item.to}
                                to={item.to}
                                className={`flex items-center justify-between gap-2 px-3 py-2.5 rounded-lg text-sm font-medium transition-colors ${
                                    isActive
                                        ? 'bg-blue-600 text-white'
                                        : 'text-slate-300 hover:bg-slate-800 hover:text-white'
                                }`}
                            >
                                <span className="flex items-center gap-3">
                                    <Icon size={18} />
                                    {item.label}
                                </span>
                                {item.badge !== undefined && (
                                    <span className="bg-orange-500 text-white text-[11px] font-bold rounded-full min-w-[20px] h-5 flex items-center justify-center px-1.5">
                                        {item.badge}
                                    </span>
                                )}
                            </Link>
                        );
                    })}
                </nav>

                <div className="px-3 py-4 border-t border-slate-800">
                    <button
                        onClick={() => navigate('/admin/login')}
                        className="w-full flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium text-slate-300 hover:bg-slate-800 hover:text-white transition-colors"
                    >
                        <LogOut size={18} />
                        Sair
                    </button>
                </div>
            </aside>

            {/* Main column */}
            <div className="flex-1 flex flex-col min-w-0">
                {/* Top header */}
                <header className="bg-white border-b border-slate-200 px-8 py-3 flex items-center gap-6">
                    <div className="flex-1 max-w-xl">
                        <div className="relative">
                            <Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
                            <input
                                type="text"
                                value={searchTerm}
                                onChange={(e) => setSearchTerm(e.target.value)}
                                placeholder="Buscar pedidos, produtos, clientes..."
                                className="w-full bg-slate-100 rounded-lg pl-9 pr-4 py-2.5 text-sm text-slate-700 placeholder-slate-400 outline-none focus:ring-2 focus:ring-blue-500"
                            />
                        </div>
                    </div>

                    <div className="flex items-center gap-5">
                        <button className="relative text-slate-500 hover:text-slate-700">
                            <Bell size={20} />
                            <span className="absolute -top-1.5 -right-1.5 bg-orange-500 text-white text-[10px] font-bold rounded-full w-4 h-4 flex items-center justify-center">
                                3
                            </span>
                        </button>

                        <div className="flex items-center gap-2 cursor-pointer">
                            <div className="w-9 h-9 rounded-full bg-blue-600 text-white flex items-center justify-center font-bold text-sm">
                                R
                            </div>
                            <div className="text-sm leading-tight">
                                <p className="font-semibold text-slate-800">Rodrigo Admin</p>
                                <p className="text-xs text-slate-400">Administrador</p>
                            </div>
                            <ChevronDown size={16} className="text-slate-400" />
                        </div>
                    </div>
                </header>

                <main className="flex-1 px-8 py-6 overflow-y-auto">{children}</main>
            </div>
        </div>
    );
}

export default BackofficeLayout;
