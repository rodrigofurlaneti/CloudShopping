import { useState } from 'react';
import { BackofficeLayout } from '../../layouts/BackofficeLayout';
import {
    ShoppingCart,
    DollarSign,
    Users,
    Package,
    ArrowUp,
    ChevronRight,
    ChevronDown,
    Truck,
    CheckCircle2,
    Clock,
    ClipboardList,
    LineChart as LineChartIcon,
    Megaphone,
    BarChart3,
} from 'lucide-react';

// --- Dados de exemplo (mock) -------------------------------------------------
// TODO: substituir por chamadas reais assim que os endpoints de analytics
// (pedidos, vendas, clientes, estoque) existirem na API.

const KPIS = [
    { label: 'Pedidos', value: '126', delta: '+12%', icon: ShoppingCart, bg: 'bg-blue-100', color: 'text-blue-600' },
    { label: 'Vendas', value: 'R$ 8.423,50', delta: '+18%', icon: DollarSign, bg: 'bg-emerald-100', color: 'text-emerald-600' },
    { label: 'Clientes', value: '89', delta: '+7%', icon: Users, bg: 'bg-purple-100', color: 'text-purple-600' },
    { label: 'Produtos', value: '342', delta: '+5%', icon: Package, bg: 'bg-orange-100', color: 'text-orange-600' },
];

const CHART_LABELS = ['Qua', 'Qui', 'Sex', 'Sáb', 'Dom', 'Seg', 'Ter'];
const SALES_SERIES = [2600, 2950, 2750, 4150, 3350, 3600, 4600];
const ORDERS_SERIES = [15, 18, 20, 27, 18, 22, 33];

const RECENT_ORDERS = [
    { id: '#1265', client: 'Maria Silva', date: '12/09/2024', value: 'R$ 299,90', status: 'Em separação', tone: 'amber' },
    { id: '#1264', client: 'João Santos', date: '12/09/2024', value: 'R$ 189,90', status: 'Enviado', tone: 'blue' },
    { id: '#1263', client: 'Ana Costa', date: '11/09/2024', value: 'R$ 459,90', status: 'Entregue', tone: 'green' },
    { id: '#1262', client: 'Carlos Lima', date: '11/09/2024', value: 'R$ 129,90', status: 'Pendente', tone: 'amber' },
    { id: '#1261', client: 'Fernanda Rocha', date: '10/09/2024', value: 'R$ 349,90', status: 'Entregue', tone: 'green' },
];

const LATEST_ORDERS = [
    { id: '#1265', client: 'Maria Silva', value: 'R$ 299,90', status: 'Em separação', icon: Package, tone: 'amber' },
    { id: '#1264', client: 'João Santos', value: 'R$ 189,90', status: 'Enviado', icon: Truck, tone: 'blue' },
    { id: '#1263', client: 'Ana Costa', value: 'R$ 459,90', status: 'Entregue', icon: CheckCircle2, tone: 'green' },
    { id: '#1262', client: 'Carlos Lima', value: 'R$ 129,90', status: 'Pendente', icon: Clock, tone: 'amber' },
];

const TOP_PRODUCTS = [
    { name: 'Smartphone Galaxy S24', sales: 156 },
    { name: 'Notebook Dell Inspiron', sales: 142 },
    { name: 'Fone de Ouvido Bluetooth', sales: 98 },
    { name: 'Air Fryer 4L', sales: 87 },
    { name: 'Caixa de Som Bluetooth', sales: 76 },
];

const QUICK_ACTIONS = [
    { label: 'Cadastrar Produto', icon: Package },
    { label: 'Gerenciar Pedidos', icon: ClipboardList },
    { label: 'Ver Vendas', icon: LineChartIcon },
    { label: 'Configurar Promoções', icon: Megaphone },
    { label: 'Relatórios', icon: BarChart3 },
];

const STOCK_SUMMARY = [
    { label: 'Em estoque', value: 287, total: 342, tone: 'bg-emerald-500' },
    { label: 'Estoque baixo', value: 32, total: 342, tone: 'bg-amber-500' },
    { label: 'Sem estoque', value: 23, total: 342, tone: 'bg-red-500' },
];

const TONE_CLASSES: Record<string, string> = {
    amber: 'bg-amber-100 text-amber-700',
    blue: 'bg-blue-100 text-blue-700',
    green: 'bg-emerald-100 text-emerald-700',
};

const ICON_TONE_CLASSES: Record<string, string> = {
    amber: 'bg-amber-100 text-amber-600',
    blue: 'bg-blue-100 text-blue-600',
    green: 'bg-emerald-100 text-emerald-600',
};

function StatusBadge({ status, tone }: { status: string; tone: string }) {
    return (
        <span className={`text-xs font-semibold px-2.5 py-1 rounded-full ${TONE_CLASSES[tone]}`}>
            {status}
        </span>
    );
}

function SalesChart() {
    const width = 700;
    const height = 220;
    const maxSales = 5000;
    const maxOrders = 50;

    const toPath = (data: number[], max: number) =>
        data
            .map((v, i) => {
                const x = (i / (data.length - 1)) * width;
                const y = height - (v / max) * height;
                return `${i === 0 ? 'M' : 'L'}${x},${y}`;
            })
            .join(' ');

    const toPoints = (data: number[], max: number) =>
        data.map((v, i) => {
            const x = (i / (data.length - 1)) * width;
            const y = height - (v / max) * height;
            return { x, y };
        });

    return (
        <div className="w-full overflow-x-auto">
            <svg viewBox={`0 0 ${width} ${height + 30}`} className="w-full min-w-[560px]" preserveAspectRatio="none">
                {/* linhas de grade horizontais */}
                {[0, 0.25, 0.5, 0.75, 1].map((f) => (
                    <line
                        key={f}
                        x1={0}
                        x2={width}
                        y1={height * f}
                        y2={height * f}
                        stroke="#e2e8f0"
                        strokeWidth={1}
                    />
                ))}

                {/* área sob a linha de vendas */}
                <path
                    d={`${toPath(SALES_SERIES, maxSales)} L${width},${height} L0,${height} Z`}
                    fill="#3b82f6"
                    opacity={0.08}
                />

                <path d={toPath(SALES_SERIES, maxSales)} fill="none" stroke="#3b82f6" strokeWidth={2.5} />
                <path d={toPath(ORDERS_SERIES, maxOrders)} fill="none" stroke="#a855f7" strokeWidth={2.5} />

                {toPoints(SALES_SERIES, maxSales).map((p, i) => (
                    <circle key={`s-${i}`} cx={p.x} cy={p.y} r={4} fill="#3b82f6" />
                ))}
                {toPoints(ORDERS_SERIES, maxOrders).map((p, i) => (
                    <circle key={`o-${i}`} cx={p.x} cy={p.y} r={4} fill="#a855f7" />
                ))}

                {CHART_LABELS.map((label, i) => (
                    <text
                        key={label}
                        x={(i / (CHART_LABELS.length - 1)) * width}
                        y={height + 20}
                        textAnchor="middle"
                        fontSize={12}
                        fill="#64748b"
                    >
                        {label}
                    </text>
                ))}
            </svg>
        </div>
    );
}

export function Dashboard() {
    const [range, setRange] = useState<'7' | '30' | '365'>('7');

    return (
        <BackofficeLayout>
            {/* Breadcrumb */}
            <div className="text-sm text-slate-500 mb-2 flex items-center gap-1.5">
                <span>Início</span>
                <ChevronRight size={14} />
                <span className="text-slate-700 font-medium">Dashboard</span>
            </div>

            <div className="flex items-start justify-between mb-6">
                <div>
                    <h1 className="text-2xl font-bold text-slate-900">Dashboard</h1>
                    <p className="text-sm text-slate-500 mt-1">Visão geral da sua loja online</p>
                </div>

                <button className="flex items-center gap-2 bg-white border border-slate-200 rounded-lg px-4 py-2 text-sm font-medium text-slate-700 shadow-sm">
                    Últimos 7 dias
                    <ChevronDown size={16} className="text-slate-400" />
                </button>
            </div>

            {/* KPI cards */}
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
                {KPIS.map((kpi) => {
                    const Icon = kpi.icon;
                    return (
                        <div key={kpi.label} className="bg-white rounded-xl border border-slate-200 p-5 shadow-sm">
                            <div className="flex items-center gap-3 mb-3">
                                <div className={`w-10 h-10 rounded-lg flex items-center justify-center ${kpi.bg}`}>
                                    <Icon size={20} className={kpi.color} />
                                </div>
                                <p className="text-sm text-slate-500">{kpi.label}</p>
                            </div>
                            <p className="text-2xl font-bold text-slate-900">{kpi.value}</p>
                            <p className="text-xs text-emerald-600 font-medium flex items-center gap-1 mt-1">
                                <ArrowUp size={12} /> {kpi.delta} em relação ao período anterior
                            </p>
                        </div>
                    );
                })}
            </div>

            <div className="grid grid-cols-1 lg:grid-cols-3 gap-6 mb-6">
                {/* Chart */}
                <div className="lg:col-span-2 bg-white rounded-xl border border-slate-200 p-5 shadow-sm">
                    <div className="flex flex-wrap items-center justify-between gap-3 mb-4">
                        <div className="flex items-center gap-4">
                            <h2 className="font-bold text-slate-900">Vendas e Pedidos</h2>
                            <span className="flex items-center gap-1.5 text-xs text-slate-500">
                                <span className="w-2.5 h-2.5 rounded-full bg-blue-500" /> Vendas (R$)
                            </span>
                            <span className="flex items-center gap-1.5 text-xs text-slate-500">
                                <span className="w-2.5 h-2.5 rounded-full bg-purple-500" /> Pedidos
                            </span>
                        </div>

                        <div className="flex bg-slate-100 rounded-lg p-1 text-xs font-semibold">
                            {(['7', '30', '365'] as const).map((r) => (
                                <button
                                    key={r}
                                    onClick={() => setRange(r)}
                                    className={`px-3 py-1.5 rounded-md transition-colors ${
                                        range === r ? 'bg-blue-600 text-white' : 'text-slate-500 hover:text-slate-700'
                                    }`}
                                >
                                    {r === '7' ? '7 dias' : r === '30' ? '30 dias' : '12 meses'}
                                </button>
                            ))}
                        </div>
                    </div>

                    <SalesChart />
                </div>

                {/* Últimos Pedidos */}
                <div className="bg-white rounded-xl border border-slate-200 p-5 shadow-sm">
                    <div className="flex items-center justify-between mb-4">
                        <h2 className="font-bold text-slate-900">Últimos Pedidos</h2>
                        <a href="#" className="text-xs font-semibold text-blue-600 hover:underline">Ver todos</a>
                    </div>

                    <div className="space-y-3">
                        {LATEST_ORDERS.map((order) => {
                            const Icon = order.icon;
                            return (
                                <div key={order.id} className="flex items-center gap-3">
                                    <div className={`w-9 h-9 rounded-lg flex items-center justify-center shrink-0 ${ICON_TONE_CLASSES[order.tone]}`}>
                                        <Icon size={16} />
                                    </div>
                                    <div className="flex-1 min-w-0">
                                        <p className="text-sm font-semibold text-slate-800">{order.id}</p>
                                        <p className="text-xs text-slate-500 truncate">Cliente: {order.client}</p>
                                    </div>
                                    <div className="text-right shrink-0">
                                        <p className="text-sm font-semibold text-slate-800">{order.value}</p>
                                        <StatusBadge status={order.status} tone={order.tone} />
                                    </div>
                                </div>
                            );
                        })}
                    </div>
                </div>
            </div>

            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-6">
                {/* Pedidos Recentes */}
                <div className="bg-white rounded-xl border border-slate-200 p-5 shadow-sm">
                    <div className="flex items-center justify-between mb-4">
                        <h2 className="font-bold text-slate-900">Pedidos Recentes</h2>
                        <a href="#" className="text-xs font-semibold text-blue-600 hover:underline">Ver todos</a>
                    </div>

                    <div className="overflow-x-auto">
                        <table className="w-full text-sm">
                            <thead>
                                <tr className="text-left text-xs text-slate-400 uppercase tracking-wide">
                                    <th className="py-2 font-medium">Pedido</th>
                                    <th className="py-2 font-medium">Cliente</th>
                                    <th className="py-2 font-medium">Data</th>
                                    <th className="py-2 font-medium">Valor</th>
                                    <th className="py-2 font-medium">Status</th>
                                    <th className="py-2"></th>
                                </tr>
                            </thead>
                            <tbody>
                                {RECENT_ORDERS.map((order) => (
                                    <tr key={order.id} className="border-t border-slate-100">
                                        <td className="py-3 font-semibold text-blue-600">{order.id}</td>
                                        <td className="py-3 text-slate-700">{order.client}</td>
                                        <td className="py-3 text-slate-500">{order.date}</td>
                                        <td className="py-3 text-slate-700">{order.value}</td>
                                        <td className="py-3">
                                            <StatusBadge status={order.status} tone={order.tone} />
                                        </td>
                                        <td className="py-3 text-slate-300">
                                            <ChevronRight size={16} />
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    </div>
                </div>

                {/* Produtos Mais Vendidos */}
                <div className="bg-white rounded-xl border border-slate-200 p-5 shadow-sm">
                    <div className="flex items-center justify-between mb-4">
                        <h2 className="font-bold text-slate-900">Produtos Mais Vendidos</h2>
                        <a href="#" className="text-xs font-semibold text-blue-600 hover:underline">Ver todos</a>
                    </div>

                    <div className="space-y-3">
                        {TOP_PRODUCTS.map((product, i) => (
                            <div key={product.name} className="flex items-center gap-3">
                                <span className="w-6 h-6 rounded-full bg-orange-500 text-white text-xs font-bold flex items-center justify-center shrink-0">
                                    {i + 1}
                                </span>
                                <div className="w-9 h-9 rounded-lg bg-slate-100 flex items-center justify-center shrink-0">
                                    <Package size={16} className="text-slate-400" />
                                </div>
                                <p className="flex-1 text-sm text-slate-700 truncate">{product.name}</p>
                                <p className="text-sm text-slate-500 shrink-0">{product.sales} vendas</p>
                            </div>
                        ))}
                    </div>
                </div>
            </div>

            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                {/* Ações Rápidas */}
                <div className="bg-blue-600 rounded-xl p-5 shadow-sm text-white">
                    <div className="flex items-center justify-between mb-4">
                        <h2 className="font-bold">Ações Rápidas</h2>
                        <a href="#" className="text-xs font-semibold text-blue-100 hover:underline">Ver todos</a>
                    </div>

                    <div className="bg-white/10 rounded-lg divide-y divide-white/10">
                        {QUICK_ACTIONS.map((action) => {
                            const Icon = action.icon;
                            return (
                                <div key={action.label} className="flex items-center gap-3 px-4 py-3 hover:bg-white/10 rounded-lg cursor-pointer transition-colors">
                                    <Icon size={16} />
                                    <span className="text-sm font-medium">{action.label}</span>
                                </div>
                            );
                        })}
                    </div>
                </div>

                {/* Resumo do Estoque */}
                <div className="bg-white rounded-xl border border-slate-200 p-5 shadow-sm">
                    <div className="flex items-center justify-between mb-4">
                        <h2 className="font-bold text-slate-900">Resumo do Estoque</h2>
                        <a href="#" className="text-xs font-semibold text-blue-600 hover:underline">Ver detalhes</a>
                    </div>

                    <div className="space-y-4">
                        {STOCK_SUMMARY.map((item) => (
                            <div key={item.label}>
                                <div className="flex items-center justify-between text-sm mb-1.5">
                                    <span className="text-slate-600">{item.label}</span>
                                    <span className="font-semibold text-slate-800">{item.value} produtos</span>
                                </div>
                                <div className="w-full h-2 bg-slate-100 rounded-full overflow-hidden">
                                    <div
                                        className={`h-full rounded-full ${item.tone}`}
                                        style={{ width: `${(item.value / item.total) * 100}%` }}
                                    />
                                </div>
                            </div>
                        ))}
                    </div>
                </div>
            </div>
        </BackofficeLayout>
    );
}

export default Dashboard;
