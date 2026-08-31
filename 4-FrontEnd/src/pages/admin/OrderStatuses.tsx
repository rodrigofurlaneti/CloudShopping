import { useEffect, useMemo, useState, type FormEvent } from 'react';
import { BackofficeLayout } from '../../layouts/BackofficeLayout';
import { OrderStatusService, OrderSectorService, type OrderStatus, type OrderSector } from '../../services/api';
import {
    ChevronRight,
    Plus,
    Search,
    Pencil,
    Power,
    PowerOff,
    Flag,
    Lock,
    CheckCircle2,
    XCircle,
    X,
    AlertTriangle,
} from 'lucide-react';

// --- Helpers ------------------------------------------------------------

type StatusFilter = 'all' | 'active' | 'inactive';

const MAX_NAME_LENGTH = 50;

function ActiveBadge({ isActive }: { isActive: boolean }) {
    return isActive ? (
        <span className="inline-flex items-center gap-1 text-xs font-semibold px-2.5 py-1 rounded-full bg-emerald-100 text-emerald-700">
            <CheckCircle2 size={11} /> Ativo
        </span>
    ) : (
        <span className="inline-flex items-center gap-1 text-xs font-semibold px-2.5 py-1 rounded-full bg-slate-100 text-slate-500">
            <XCircle size={11} /> Inativo
        </span>
    );
}

interface FormState {
    name: string;
    orderSectorId: string;
}

const EMPTY_FORM: FormState = { name: '', orderSectorId: '' };

// --- Página ---------------------------------------------------------------

export function OrderStatuses() {
    const [statuses, setStatuses] = useState<OrderStatus[]>([]);
    const [sectors, setSectors] = useState<OrderSector[]>([]);
    const [loading, setLoading] = useState(true);
    const [loadError, setLoadError] = useState('');

    const [searchTerm, setSearchTerm] = useState('');
    const [statusFilter, setStatusFilter] = useState<StatusFilter>('all');
    const [sectorFilter, setSectorFilter] = useState<'all' | number>('all');

    const [isModalOpen, setIsModalOpen] = useState(false);
    const [editingStatus, setEditingStatus] = useState<OrderStatus | null>(null);
    const [form, setForm] = useState<FormState>(EMPTY_FORM);
    const [saving, setSaving] = useState(false);
    const [formError, setFormError] = useState('');

    const [togglingId, setTogglingId] = useState<number | null>(null);
    const [toggleError, setToggleError] = useState('');

    const sectorNameById = useMemo(() => {
        const map = new Map<number, string>();
        sectors.forEach((sector) => map.set(sector.id, sector.name));
        return map;
    }, [sectors]);

    const activeSectors = useMemo(() => sectors.filter((s) => s.isActive), [sectors]);

    async function loadData() {
        setLoading(true);
        setLoadError('');
        try {
            const [statusData, sectorData] = await Promise.all([
                OrderStatusService.getAll(false),
                OrderSectorService.getAll(false),
            ]);
            setStatuses(statusData);
            setSectors(sectorData);
        } catch (error) {
            setLoadError(error instanceof Error ? error.message : 'Não foi possível carregar os status de pedido.');
        } finally {
            setLoading(false);
        }
    }

    useEffect(() => {
        loadData();
    }, []);

    const filteredStatuses = useMemo(() => {
        const term = searchTerm.trim().toLowerCase();
        return statuses.filter((status) => {
            const matchesSearch = !term || status.name.toLowerCase().includes(term);
            const matchesStatus =
                statusFilter === 'all' ||
                (statusFilter === 'active' && status.isActive) ||
                (statusFilter === 'inactive' && !status.isActive);
            const matchesSector = sectorFilter === 'all' || status.orderSectorId === sectorFilter;
            return matchesSearch && matchesStatus && matchesSector;
        });
    }, [statuses, searchTerm, statusFilter, sectorFilter]);

    const stats = useMemo(() => {
        const total = statuses.length;
        const active = statuses.filter((s) => s.isActive).length;
        const system = statuses.filter((s) => s.isSystemDefault).length;
        return { total, active, system };
    }, [statuses]);

    function openCreateModal() {
        setEditingStatus(null);
        setForm({ name: '', orderSectorId: activeSectors[0] ? String(activeSectors[0].id) : '' });
        setFormError('');
        setIsModalOpen(true);
    }

    function openEditModal(status: OrderStatus) {
        setEditingStatus(status);
        setForm({ name: status.name, orderSectorId: String(status.orderSectorId) });
        setFormError('');
        setIsModalOpen(true);
    }

    function closeModal() {
        if (saving) return;
        setIsModalOpen(false);
    }

    async function handleSubmit(e: FormEvent) {
        e.preventDefault();
        setFormError('');

        const name = form.name.trim();
        const orderSectorId = Number(form.orderSectorId);

        if (!name) {
            setFormError('Informe o nome do status.');
            return;
        }
        if (name.length > MAX_NAME_LENGTH) {
            setFormError(`O nome do status pode ter no máximo ${MAX_NAME_LENGTH} caracteres.`);
            return;
        }
        if (!orderSectorId) {
            setFormError('Selecione o setor logístico deste status.');
            return;
        }

        setSaving(true);
        try {
            if (editingStatus) {
                await OrderStatusService.update(editingStatus.id, { name, orderSectorId });
            } else {
                await OrderStatusService.create({ name, orderSectorId });
            }
            setIsModalOpen(false);
            await loadData();
        } catch (error) {
            setFormError(error instanceof Error ? error.message : 'Não foi possível salvar o status.');
        } finally {
            setSaving(false);
        }
    }

    async function handleToggle(status: OrderStatus) {
        setToggleError('');
        setTogglingId(status.id);
        try {
            await OrderStatusService.toggleStatus(status.id, !status.isActive);
            await loadData();
        } catch (error) {
            setToggleError(error instanceof Error ? error.message : 'Não foi possível alterar o status.');
        } finally {
            setTogglingId(null);
        }
    }

    return (
        <BackofficeLayout>
            {/* Breadcrumb */}
            <div className="text-sm text-slate-500 mb-2 flex items-center gap-1.5">
                <span>Início</span>
                <ChevronRight size={14} />
                <span className="text-slate-700 font-medium">Status de Pedido</span>
            </div>

            <div className="flex items-start justify-between mb-6 gap-4 flex-wrap">
                <div>
                    <h1 className="text-2xl font-bold text-slate-900">Status de Pedido</h1>
                    <p className="text-sm text-slate-500 mt-1">
                        Gerencie as etapas usadas para acompanhar o pedido dentro de cada setor logístico
                    </p>
                </div>

                <button
                    onClick={openCreateModal}
                    disabled={activeSectors.length === 0}
                    title={activeSectors.length === 0 ? 'Cadastre um setor logístico ativo primeiro' : undefined}
                    className="flex items-center gap-2 bg-blue-600 hover:bg-blue-700 text-white text-sm font-semibold px-4 py-2.5 rounded-lg shadow-sm transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                >
                    <Plus size={16} /> Novo Status
                </button>
            </div>

            {!loading && activeSectors.length === 0 && (
                <div className="mb-6 flex items-center gap-2 bg-amber-50 border border-amber-200 text-amber-800 text-sm px-4 py-3 rounded-lg">
                    <AlertTriangle size={16} />
                    Nenhum setor logístico ativo. Cadastre um setor em "Setores Logísticos" antes de criar um status.
                </div>
            )}

            {/* Stat cards */}
            <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 mb-6">
                <div className="bg-white rounded-xl border border-slate-200 p-5 shadow-sm">
                    <div className="flex items-center gap-3 mb-3">
                        <div className="w-10 h-10 rounded-lg flex items-center justify-center bg-blue-100">
                            <Flag size={20} className="text-blue-600" />
                        </div>
                        <p className="text-sm text-slate-500">Total de Status</p>
                    </div>
                    <p className="text-2xl font-bold text-slate-900">{stats.total}</p>
                </div>

                <div className="bg-white rounded-xl border border-slate-200 p-5 shadow-sm">
                    <div className="flex items-center gap-3 mb-3">
                        <div className="w-10 h-10 rounded-lg flex items-center justify-center bg-emerald-100">
                            <CheckCircle2 size={20} className="text-emerald-600" />
                        </div>
                        <p className="text-sm text-slate-500">Ativos</p>
                    </div>
                    <p className="text-2xl font-bold text-slate-900">{stats.active}</p>
                </div>

                <div className="bg-white rounded-xl border border-slate-200 p-5 shadow-sm">
                    <div className="flex items-center gap-3 mb-3">
                        <div className="w-10 h-10 rounded-lg flex items-center justify-center bg-slate-100">
                            <Lock size={20} className="text-slate-500" />
                        </div>
                        <p className="text-sm text-slate-500">Padrão do Sistema</p>
                    </div>
                    <p className="text-2xl font-bold text-slate-900">{stats.system}</p>
                </div>
            </div>

            {/* Lista */}
            <div className="bg-white rounded-xl border border-slate-200 shadow-sm">
                <div className="p-5 border-b border-slate-100 flex flex-wrap items-center justify-between gap-3">
                    <h2 className="font-bold text-slate-900">Lista de Status</h2>

                    <div className="flex flex-wrap items-center gap-3">
                        <div className="relative">
                            <Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
                            <input
                                type="text"
                                value={searchTerm}
                                onChange={(e) => setSearchTerm(e.target.value)}
                                placeholder="Buscar por nome..."
                                className="bg-slate-50 border border-slate-200 rounded-lg pl-9 pr-4 py-2 text-sm text-slate-700 placeholder-slate-400 outline-none focus:ring-2 focus:ring-blue-500 w-56"
                            />
                        </div>

                        <select
                            value={sectorFilter}
                            onChange={(e) => setSectorFilter(e.target.value === 'all' ? 'all' : Number(e.target.value))}
                            className="bg-slate-50 border border-slate-200 rounded-lg px-3 py-2 text-sm text-slate-700 outline-none focus:ring-2 focus:ring-blue-500"
                        >
                            <option value="all">Todos os setores</option>
                            {sectors.map((sector) => (
                                <option key={sector.id} value={sector.id}>
                                    {sector.name}
                                </option>
                            ))}
                        </select>

                        <select
                            value={statusFilter}
                            onChange={(e) => setStatusFilter(e.target.value as StatusFilter)}
                            className="bg-slate-50 border border-slate-200 rounded-lg px-3 py-2 text-sm text-slate-700 outline-none focus:ring-2 focus:ring-blue-500"
                        >
                            <option value="all">Todos</option>
                            <option value="active">Ativos</option>
                            <option value="inactive">Inativos</option>
                        </select>
                    </div>
                </div>

                {(loadError || toggleError) && (
                    <div className="m-5 flex items-center gap-2 bg-red-50 border border-red-200 text-red-700 text-sm px-4 py-3 rounded-lg">
                        <AlertTriangle size={16} /> {loadError || toggleError}
                    </div>
                )}

                {!loadError && (
                    <div className="overflow-x-auto">
                        <table className="w-full text-sm">
                            <thead>
                                <tr className="text-left text-xs text-slate-400 uppercase tracking-wide bg-slate-50">
                                    <th className="py-3 px-5 font-medium">Nome</th>
                                    <th className="py-3 px-5 font-medium">Setor</th>
                                    <th className="py-3 px-5 font-medium">Tipo</th>
                                    <th className="py-3 px-5 font-medium">Status</th>
                                    <th className="py-3 px-5 font-medium text-right">Ações</th>
                                </tr>
                            </thead>
                            <tbody>
                                {loading && (
                                    <tr>
                                        <td colSpan={5} className="py-10 text-center text-slate-400">
                                            Carregando status de pedido...
                                        </td>
                                    </tr>
                                )}

                                {!loading && filteredStatuses.length === 0 && (
                                    <tr>
                                        <td colSpan={5} className="py-10 text-center text-slate-400">
                                            Nenhum status encontrado.
                                        </td>
                                    </tr>
                                )}

                                {!loading &&
                                    filteredStatuses.map((status) => (
                                        <tr key={status.id} className="border-t border-slate-100 hover:bg-slate-50/60">
                                            <td className="py-3 px-5">
                                                <div className="flex items-center gap-3">
                                                    <div className="w-9 h-9 rounded-lg bg-blue-50 flex items-center justify-center shrink-0">
                                                        <Flag size={16} className="text-blue-500" />
                                                    </div>
                                                    <span className="font-semibold text-slate-800">{status.name}</span>
                                                </div>
                                            </td>
                                            <td className="py-3 px-5 text-slate-500">
                                                {sectorNameById.get(status.orderSectorId) ?? `Setor #${status.orderSectorId}`}
                                            </td>
                                            <td className="py-3 px-5">
                                                {status.isSystemDefault ? (
                                                    <span className="inline-flex items-center gap-1 text-xs font-semibold px-2.5 py-1 rounded-full bg-slate-100 text-slate-600">
                                                        <Lock size={11} /> Sistema
                                                    </span>
                                                ) : (
                                                    <span className="inline-flex items-center gap-1 text-xs font-semibold px-2.5 py-1 rounded-full bg-purple-100 text-purple-700">
                                                        Personalizado
                                                    </span>
                                                )}
                                            </td>
                                            <td className="py-3 px-5">
                                                <ActiveBadge isActive={status.isActive} />
                                            </td>
                                            <td className="py-3 px-5">
                                                <div className="flex items-center justify-end gap-2">
                                                    <button
                                                        onClick={() => openEditModal(status)}
                                                        title="Editar status"
                                                        className="w-8 h-8 rounded-lg flex items-center justify-center text-blue-600 bg-blue-50 hover:bg-blue-100 transition-colors"
                                                    >
                                                        <Pencil size={14} />
                                                    </button>
                                                    <button
                                                        onClick={() => handleToggle(status)}
                                                        disabled={togglingId === status.id || (status.isSystemDefault && status.isActive)}
                                                        title={
                                                            status.isSystemDefault && status.isActive
                                                                ? 'Status padrão do sistema não pode ser desativado'
                                                                : status.isActive
                                                                  ? 'Desativar status'
                                                                  : 'Ativar status'
                                                        }
                                                        className={`w-8 h-8 rounded-lg flex items-center justify-center transition-colors disabled:opacity-40 disabled:cursor-not-allowed ${
                                                            status.isActive
                                                                ? 'text-slate-500 bg-slate-100 hover:bg-slate-200'
                                                                : 'text-emerald-600 bg-emerald-50 hover:bg-emerald-100'
                                                        }`}
                                                    >
                                                        {togglingId === status.id ? (
                                                            <span className="inline-block w-3.5 h-3.5 border-2 border-current border-t-transparent rounded-full animate-spin" />
                                                        ) : status.isActive ? (
                                                            <PowerOff size={14} />
                                                        ) : (
                                                            <Power size={14} />
                                                        )}
                                                    </button>
                                                </div>
                                            </td>
                                        </tr>
                                    ))}
                            </tbody>
                        </table>
                    </div>
                )}

                {!loadError && filteredStatuses.length > 0 && (
                    <p className="px-5 py-4 border-t border-slate-100 text-sm text-slate-500">
                        Mostrando {filteredStatuses.length} de {statuses.length} status
                    </p>
                )}
            </div>

            {/* Modal de criação/edição */}
            {isModalOpen && (
                <div className="fixed inset-0 bg-slate-900/50 flex items-center justify-center px-4 z-50">
                    <div className="bg-white rounded-2xl shadow-xl w-full max-w-md">
                        <div className="flex items-center justify-between px-6 py-4 border-b border-slate-100">
                            <h3 className="font-bold text-slate-900">
                                {editingStatus ? 'Editar Status' : 'Novo Status'}
                            </h3>
                            <button
                                onClick={closeModal}
                                className="w-8 h-8 rounded-lg flex items-center justify-center text-slate-400 hover:bg-slate-100"
                                aria-label="Fechar"
                            >
                                <X size={18} />
                            </button>
                        </div>

                        <form onSubmit={handleSubmit} className="px-6 py-5 space-y-4">
                            {formError && (
                                <div className="bg-red-50 border border-red-200 text-red-700 text-xs px-3 py-2.5 rounded-lg">
                                    {formError}
                                </div>
                            )}

                            <div>
                                <label className="block text-xs font-semibold uppercase tracking-wider text-slate-500 mb-1.5">
                                    Nome do status
                                </label>
                                <input
                                    type="text"
                                    required
                                    autoFocus
                                    maxLength={MAX_NAME_LENGTH}
                                    value={form.name}
                                    onChange={(e) => setForm((prev) => ({ ...prev, name: e.target.value }))}
                                    placeholder="Ex: Aguardando pagamento, Em separação"
                                    className="w-full bg-slate-50 border border-slate-200 rounded-lg px-4 py-2.5 text-sm text-slate-800 placeholder-slate-400 outline-none focus:ring-2 focus:ring-blue-500"
                                />
                            </div>

                            <div>
                                <label className="block text-xs font-semibold uppercase tracking-wider text-slate-500 mb-1.5">
                                    Setor logístico
                                </label>
                                <select
                                    required
                                    value={form.orderSectorId}
                                    onChange={(e) => setForm((prev) => ({ ...prev, orderSectorId: e.target.value }))}
                                    className="w-full bg-slate-50 border border-slate-200 rounded-lg px-4 py-2.5 text-sm text-slate-800 outline-none focus:ring-2 focus:ring-blue-500"
                                >
                                    <option value="" disabled>
                                        Selecione um setor
                                    </option>
                                    {activeSectors.map((sector) => (
                                        <option key={sector.id} value={sector.id}>
                                            {sector.name}
                                        </option>
                                    ))}
                                </select>
                            </div>

                            <div className="flex items-center justify-end gap-3 pt-2">
                                <button
                                    type="button"
                                    onClick={closeModal}
                                    disabled={saving}
                                    className="px-4 py-2.5 rounded-lg text-sm font-semibold text-slate-600 hover:bg-slate-100 transition-colors disabled:opacity-50"
                                >
                                    Cancelar
                                </button>
                                <button
                                    type="submit"
                                    disabled={saving}
                                    className="flex items-center gap-2 bg-blue-600 hover:bg-blue-700 text-white text-sm font-semibold px-4 py-2.5 rounded-lg shadow-sm transition-colors disabled:opacity-60"
                                >
                                    {saving && (
                                        <span className="inline-block w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin" />
                                    )}
                                    {editingStatus ? 'Salvar alterações' : 'Criar status'}
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            )}
        </BackofficeLayout>
    );
}

export default OrderStatuses;
