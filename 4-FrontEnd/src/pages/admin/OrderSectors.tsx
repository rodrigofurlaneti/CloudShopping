import { useEffect, useMemo, useState, type FormEvent } from 'react';
import { BackofficeLayout } from '../../layouts/BackofficeLayout';
import { OrderSectorService, type OrderSector } from '../../services/api';
import {
    ChevronRight,
    Plus,
    Search,
    Pencil,
    Power,
    PowerOff,
    Warehouse,
    CheckCircle2,
    XCircle,
    X,
    AlertTriangle,
} from 'lucide-react';

// --- Helpers ------------------------------------------------------------

type StatusFilter = 'all' | 'active' | 'inactive';

const MAX_NAME_LENGTH = 100;

function StatusBadge({ isActive }: { isActive: boolean }) {
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
}

const EMPTY_FORM: FormState = { name: '' };

// --- Página ---------------------------------------------------------------

export function OrderSectors() {
    const [sectors, setSectors] = useState<OrderSector[]>([]);
    const [loading, setLoading] = useState(true);
    const [loadError, setLoadError] = useState('');

    const [searchTerm, setSearchTerm] = useState('');
    const [statusFilter, setStatusFilter] = useState<StatusFilter>('all');

    const [isModalOpen, setIsModalOpen] = useState(false);
    const [editingSector, setEditingSector] = useState<OrderSector | null>(null);
    const [form, setForm] = useState<FormState>(EMPTY_FORM);
    const [saving, setSaving] = useState(false);
    const [formError, setFormError] = useState('');

    const [togglingId, setTogglingId] = useState<number | null>(null);
    const [toggleError, setToggleError] = useState('');

    async function loadSectors() {
        setLoading(true);
        setLoadError('');
        try {
            const data = await OrderSectorService.getAll(false);
            setSectors(data);
        } catch (error) {
            setLoadError(error instanceof Error ? error.message : 'Não foi possível carregar os setores.');
        } finally {
            setLoading(false);
        }
    }

    useEffect(() => {
        loadSectors();
    }, []);

    const filteredSectors = useMemo(() => {
        const term = searchTerm.trim().toLowerCase();
        return sectors.filter((sector) => {
            const matchesSearch = !term || sector.name.toLowerCase().includes(term);
            const matchesStatus =
                statusFilter === 'all' ||
                (statusFilter === 'active' && sector.isActive) ||
                (statusFilter === 'inactive' && !sector.isActive);
            return matchesSearch && matchesStatus;
        });
    }, [sectors, searchTerm, statusFilter]);

    const stats = useMemo(() => {
        const total = sectors.length;
        const active = sectors.filter((s) => s.isActive).length;
        return { total, active, inactive: total - active };
    }, [sectors]);

    function openCreateModal() {
        setEditingSector(null);
        setForm(EMPTY_FORM);
        setFormError('');
        setIsModalOpen(true);
    }

    function openEditModal(sector: OrderSector) {
        setEditingSector(sector);
        setForm({ name: sector.name });
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
        if (!name) {
            setFormError('Informe o nome do setor.');
            return;
        }
        if (name.length > MAX_NAME_LENGTH) {
            setFormError(`O nome do setor pode ter no máximo ${MAX_NAME_LENGTH} caracteres.`);
            return;
        }

        setSaving(true);
        try {
            if (editingSector) {
                await OrderSectorService.update(editingSector.id, { newName: name });
            } else {
                await OrderSectorService.create({ name });
            }
            setIsModalOpen(false);
            await loadSectors();
        } catch (error) {
            setFormError(error instanceof Error ? error.message : 'Não foi possível salvar o setor.');
        } finally {
            setSaving(false);
        }
    }

    async function handleToggle(sector: OrderSector) {
        setToggleError('');
        setTogglingId(sector.id);
        try {
            await OrderSectorService.toggleStatus(sector.id, !sector.isActive);
            await loadSectors();
        } catch (error) {
            setToggleError(error instanceof Error ? error.message : 'Não foi possível alterar o status do setor.');
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
                <span className="text-slate-700 font-medium">Setores Logísticos</span>
            </div>

            <div className="flex items-start justify-between mb-6 gap-4 flex-wrap">
                <div>
                    <h1 className="text-2xl font-bold text-slate-900">Setores Logísticos</h1>
                    <p className="text-sm text-slate-500 mt-1">
                        Gerencie os setores usados no fluxo de separação e envio dos pedidos
                    </p>
                </div>

                <button
                    onClick={openCreateModal}
                    className="flex items-center gap-2 bg-blue-600 hover:bg-blue-700 text-white text-sm font-semibold px-4 py-2.5 rounded-lg shadow-sm transition-colors"
                >
                    <Plus size={16} /> Novo Setor
                </button>
            </div>

            {/* Stat cards */}
            <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 mb-6">
                <div className="bg-white rounded-xl border border-slate-200 p-5 shadow-sm">
                    <div className="flex items-center gap-3 mb-3">
                        <div className="w-10 h-10 rounded-lg flex items-center justify-center bg-blue-100">
                            <Warehouse size={20} className="text-blue-600" />
                        </div>
                        <p className="text-sm text-slate-500">Total de Setores</p>
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
                            <XCircle size={20} className="text-slate-500" />
                        </div>
                        <p className="text-sm text-slate-500">Inativos</p>
                    </div>
                    <p className="text-2xl font-bold text-slate-900">{stats.inactive}</p>
                </div>
            </div>

            {/* Lista */}
            <div className="bg-white rounded-xl border border-slate-200 shadow-sm">
                <div className="p-5 border-b border-slate-100 flex flex-wrap items-center justify-between gap-3">
                    <h2 className="font-bold text-slate-900">Lista de Setores</h2>

                    <div className="flex flex-wrap items-center gap-3">
                        <div className="relative">
                            <Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
                            <input
                                type="text"
                                value={searchTerm}
                                onChange={(e) => setSearchTerm(e.target.value)}
                                placeholder="Buscar por nome..."
                                className="bg-slate-50 border border-slate-200 rounded-lg pl-9 pr-4 py-2 text-sm text-slate-700 placeholder-slate-400 outline-none focus:ring-2 focus:ring-blue-500 w-64"
                            />
                        </div>

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
                                    <th className="py-3 px-5 font-medium">Status</th>
                                    <th className="py-3 px-5 font-medium text-right">Ações</th>
                                </tr>
                            </thead>
                            <tbody>
                                {loading && (
                                    <tr>
                                        <td colSpan={3} className="py-10 text-center text-slate-400">
                                            Carregando setores...
                                        </td>
                                    </tr>
                                )}

                                {!loading && filteredSectors.length === 0 && (
                                    <tr>
                                        <td colSpan={3} className="py-10 text-center text-slate-400">
                                            Nenhum setor encontrado.
                                        </td>
                                    </tr>
                                )}

                                {!loading &&
                                    filteredSectors.map((sector) => (
                                        <tr key={sector.id} className="border-t border-slate-100 hover:bg-slate-50/60">
                                            <td className="py-3 px-5">
                                                <div className="flex items-center gap-3">
                                                    <div className="w-9 h-9 rounded-lg bg-blue-50 flex items-center justify-center shrink-0">
                                                        <Warehouse size={16} className="text-blue-500" />
                                                    </div>
                                                    <span className="font-semibold text-slate-800">{sector.name}</span>
                                                </div>
                                            </td>
                                            <td className="py-3 px-5">
                                                <StatusBadge isActive={sector.isActive} />
                                            </td>
                                            <td className="py-3 px-5">
                                                <div className="flex items-center justify-end gap-2">
                                                    <button
                                                        onClick={() => openEditModal(sector)}
                                                        title="Editar nome do setor"
                                                        className="w-8 h-8 rounded-lg flex items-center justify-center text-blue-600 bg-blue-50 hover:bg-blue-100 transition-colors"
                                                    >
                                                        <Pencil size={14} />
                                                    </button>
                                                    <button
                                                        onClick={() => handleToggle(sector)}
                                                        disabled={togglingId === sector.id}
                                                        title={sector.isActive ? 'Desativar setor' : 'Ativar setor'}
                                                        className={`w-8 h-8 rounded-lg flex items-center justify-center transition-colors disabled:opacity-40 disabled:cursor-not-allowed ${
                                                            sector.isActive
                                                                ? 'text-slate-500 bg-slate-100 hover:bg-slate-200'
                                                                : 'text-emerald-600 bg-emerald-50 hover:bg-emerald-100'
                                                        }`}
                                                    >
                                                        {togglingId === sector.id ? (
                                                            <span className="inline-block w-3.5 h-3.5 border-2 border-current border-t-transparent rounded-full animate-spin" />
                                                        ) : sector.isActive ? (
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

                {!loadError && filteredSectors.length > 0 && (
                    <p className="px-5 py-4 border-t border-slate-100 text-sm text-slate-500">
                        Mostrando {filteredSectors.length} de {sectors.length} setores
                    </p>
                )}
            </div>

            {/* Modal de criação/edição */}
            {isModalOpen && (
                <div className="fixed inset-0 bg-slate-900/50 flex items-center justify-center px-4 z-50">
                    <div className="bg-white rounded-2xl shadow-xl w-full max-w-md">
                        <div className="flex items-center justify-between px-6 py-4 border-b border-slate-100">
                            <h3 className="font-bold text-slate-900">
                                {editingSector ? 'Editar Setor' : 'Novo Setor'}
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
                                    Nome do setor
                                </label>
                                <input
                                    type="text"
                                    required
                                    autoFocus
                                    maxLength={MAX_NAME_LENGTH}
                                    value={form.name}
                                    onChange={(e) => setForm({ name: e.target.value })}
                                    placeholder="Ex: Separação, Expedição, Transporte"
                                    className="w-full bg-slate-50 border border-slate-200 rounded-lg px-4 py-2.5 text-sm text-slate-800 placeholder-slate-400 outline-none focus:ring-2 focus:ring-blue-500"
                                />
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
                                    {editingSector ? 'Salvar alterações' : 'Criar setor'}
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            )}
        </BackofficeLayout>
    );
}

export default OrderSectors;
