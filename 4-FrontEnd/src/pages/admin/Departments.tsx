import { useEffect, useMemo, useState, type FormEvent } from 'react';
import { BackofficeLayout } from '../../layouts/BackofficeLayout';
import { DepartmentService, type Department } from '../../services/api';
import {
    ChevronRight,
    Plus,
    Search,
    Pencil,
    Trash2,
    Tags,
    Layers,
    Lock,
    X,
    AlertTriangle,
} from 'lucide-react';

// --- Helpers ------------------------------------------------------------

type TypeFilter = 'all' | 'system' | 'custom';

const PAGE_SIZE = 7;

const DIACRITICS_REGEX = new RegExp('[\\u0300-\\u036f]', 'g');

function slugify(value: string): string {
    return value
        .normalize('NFD')
        .replace(DIACRITICS_REGEX, '')
        .toLowerCase()
        .trim()
        .replace(/[^a-z0-9]+/g, '-')
        .replace(/(^-+|-+$)/g, '');
}

interface DepartmentFormState {
    name: string;
    slug: string;
    slugTouched: boolean;
}

const EMPTY_FORM: DepartmentFormState = { name: '', slug: '', slugTouched: false };

function TypeBadge({ isSystemDefault }: { isSystemDefault: boolean }) {
    return isSystemDefault ? (
        <span className="inline-flex items-center gap-1 text-xs font-semibold px-2.5 py-1 rounded-full bg-slate-100 text-slate-600">
            <Lock size={11} /> Sistema
        </span>
    ) : (
        <span className="inline-flex items-center gap-1 text-xs font-semibold px-2.5 py-1 rounded-full bg-emerald-100 text-emerald-700">
            Personalizado
        </span>
    );
}

// --- Página ---------------------------------------------------------------

export function Departments() {
    const [departments, setDepartments] = useState<Department[]>([]);
    const [loading, setLoading] = useState(true);
    const [loadError, setLoadError] = useState('');

    const [searchTerm, setSearchTerm] = useState('');
    const [typeFilter, setTypeFilter] = useState<TypeFilter>('all');
    const [page, setPage] = useState(1);

    const [isModalOpen, setIsModalOpen] = useState(false);
    const [editingDepartment, setEditingDepartment] = useState<Department | null>(null);
    const [form, setForm] = useState<DepartmentFormState>(EMPTY_FORM);
    const [saving, setSaving] = useState(false);
    const [formError, setFormError] = useState('');

    const [departmentToDelete, setDepartmentToDelete] = useState<Department | null>(null);
    const [deleting, setDeleting] = useState(false);
    const [deleteError, setDeleteError] = useState('');

    async function loadDepartments() {
        setLoading(true);
        setLoadError('');
        try {
            const data = await DepartmentService.getAll();
            setDepartments(data);
        } catch (error) {
            setLoadError(error instanceof Error ? error.message : 'Não foi possível carregar os departamentos.');
        } finally {
            setLoading(false);
        }
    }

    useEffect(() => {
        loadDepartments();
    }, []);

    useEffect(() => {
        setPage(1);
    }, [searchTerm, typeFilter]);

    const filteredDepartments = useMemo(() => {
        const term = searchTerm.trim().toLowerCase();
        return departments.filter((dept) => {
            const matchesSearch =
                !term || dept.name.toLowerCase().includes(term) || dept.slug.toLowerCase().includes(term);
            const matchesType =
                typeFilter === 'all' ||
                (typeFilter === 'system' && dept.isSystemDefault) ||
                (typeFilter === 'custom' && !dept.isSystemDefault);
            return matchesSearch && matchesType;
        });
    }, [departments, searchTerm, typeFilter]);

    const totalPages = Math.max(1, Math.ceil(filteredDepartments.length / PAGE_SIZE));
    const currentPage = Math.min(page, totalPages);
    const pagedDepartments = filteredDepartments.slice(
        (currentPage - 1) * PAGE_SIZE,
        (currentPage - 1) * PAGE_SIZE + PAGE_SIZE
    );

    const stats = useMemo(() => {
        const total = departments.length;
        const system = departments.filter((d) => d.isSystemDefault).length;
        return { total, system, custom: total - system };
    }, [departments]);

    function openCreateModal() {
        setEditingDepartment(null);
        setForm(EMPTY_FORM);
        setFormError('');
        setIsModalOpen(true);
    }

    function openEditModal(department: Department) {
        if (department.isSystemDefault) return;
        setEditingDepartment(department);
        setForm({ name: department.name, slug: department.slug, slugTouched: true });
        setFormError('');
        setIsModalOpen(true);
    }

    function closeModal() {
        if (saving) return;
        setIsModalOpen(false);
    }

    function handleNameChange(value: string) {
        setForm((prev) => ({
            ...prev,
            name: value,
            slug: prev.slugTouched ? prev.slug : slugify(value),
        }));
    }

    function handleSlugChange(value: string) {
        setForm((prev) => ({ ...prev, slug: value, slugTouched: true }));
    }

    async function handleSubmit(e: FormEvent) {
        e.preventDefault();
        setFormError('');

        const name = form.name.trim();
        const slug = slugify(form.slug);

        if (!name) {
            setFormError('Informe o nome do departamento.');
            return;
        }
        if (!slug) {
            setFormError('Informe um slug válido.');
            return;
        }

        setSaving(true);
        try {
            if (editingDepartment) {
                await DepartmentService.update(editingDepartment.id, { name, slug });
            } else {
                await DepartmentService.create({ name, slug });
            }
            setIsModalOpen(false);
            await loadDepartments();
        } catch (error) {
            setFormError(error instanceof Error ? error.message : 'Não foi possível salvar o departamento.');
        } finally {
            setSaving(false);
        }
    }

    function requestDelete(department: Department) {
        if (department.isSystemDefault) return;
        setDeleteError('');
        setDepartmentToDelete(department);
    }

    async function confirmDelete() {
        if (!departmentToDelete) return;
        setDeleting(true);
        setDeleteError('');
        try {
            await DepartmentService.remove(departmentToDelete.id);
            setDepartmentToDelete(null);
            await loadDepartments();
        } catch (error) {
            setDeleteError(error instanceof Error ? error.message : 'Não foi possível excluir o departamento.');
        } finally {
            setDeleting(false);
        }
    }

    return (
        <BackofficeLayout>
            {/* Breadcrumb */}
            <div className="text-sm text-slate-500 mb-2 flex items-center gap-1.5">
                <span>Início</span>
                <ChevronRight size={14} />
                <span className="text-slate-700 font-medium">Departamentos</span>
            </div>

            <div className="flex items-start justify-between mb-6 gap-4 flex-wrap">
                <div>
                    <h1 className="text-2xl font-bold text-slate-900">Departamentos</h1>
                    <p className="text-sm text-slate-500 mt-1">Gerencie as categorias dos produtos da sua loja online</p>
                </div>

                <button
                    onClick={openCreateModal}
                    className="flex items-center gap-2 bg-blue-600 hover:bg-blue-700 text-white text-sm font-semibold px-4 py-2.5 rounded-lg shadow-sm transition-colors"
                >
                    <Plus size={16} /> Novo Departamento
                </button>
            </div>

            {/* Stat cards */}
            <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 mb-6">
                <div className="bg-white rounded-xl border border-slate-200 p-5 shadow-sm">
                    <div className="flex items-center gap-3 mb-3">
                        <div className="w-10 h-10 rounded-lg flex items-center justify-center bg-blue-100">
                            <Tags size={20} className="text-blue-600" />
                        </div>
                        <p className="text-sm text-slate-500">Total de Departamentos</p>
                    </div>
                    <p className="text-2xl font-bold text-slate-900">{stats.total}</p>
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

                <div className="bg-white rounded-xl border border-slate-200 p-5 shadow-sm">
                    <div className="flex items-center gap-3 mb-3">
                        <div className="w-10 h-10 rounded-lg flex items-center justify-center bg-emerald-100">
                            <Layers size={20} className="text-emerald-600" />
                        </div>
                        <p className="text-sm text-slate-500">Personalizados</p>
                    </div>
                    <p className="text-2xl font-bold text-slate-900">{stats.custom}</p>
                </div>
            </div>

            {/* Lista */}
            <div className="bg-white rounded-xl border border-slate-200 shadow-sm">
                <div className="p-5 border-b border-slate-100 flex flex-wrap items-center justify-between gap-3">
                    <h2 className="font-bold text-slate-900">Lista de Departamentos</h2>

                    <div className="flex flex-wrap items-center gap-3">
                        <div className="relative">
                            <Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
                            <input
                                type="text"
                                value={searchTerm}
                                onChange={(e) => setSearchTerm(e.target.value)}
                                placeholder="Buscar por nome ou slug..."
                                className="bg-slate-50 border border-slate-200 rounded-lg pl-9 pr-4 py-2 text-sm text-slate-700 placeholder-slate-400 outline-none focus:ring-2 focus:ring-blue-500 w-64"
                            />
                        </div>

                        <select
                            value={typeFilter}
                            onChange={(e) => setTypeFilter(e.target.value as TypeFilter)}
                            className="bg-slate-50 border border-slate-200 rounded-lg px-3 py-2 text-sm text-slate-700 outline-none focus:ring-2 focus:ring-blue-500"
                        >
                            <option value="all">Todos</option>
                            <option value="system">Sistema</option>
                            <option value="custom">Personalizado</option>
                        </select>
                    </div>
                </div>

                {loadError && (
                    <div className="m-5 flex items-center gap-2 bg-red-50 border border-red-200 text-red-700 text-sm px-4 py-3 rounded-lg">
                        <AlertTriangle size={16} /> {loadError}
                    </div>
                )}

                {!loadError && (
                    <div className="overflow-x-auto">
                        <table className="w-full text-sm">
                            <thead>
                                <tr className="text-left text-xs text-slate-400 uppercase tracking-wide bg-slate-50">
                                    <th className="py-3 px-5 font-medium">Nome</th>
                                    <th className="py-3 px-5 font-medium">Slug</th>
                                    <th className="py-3 px-5 font-medium">Tipo</th>
                                    <th className="py-3 px-5 font-medium text-right">Ações</th>
                                </tr>
                            </thead>
                            <tbody>
                                {loading && (
                                    <tr>
                                        <td colSpan={4} className="py-10 text-center text-slate-400">
                                            Carregando departamentos...
                                        </td>
                                    </tr>
                                )}

                                {!loading && pagedDepartments.length === 0 && (
                                    <tr>
                                        <td colSpan={4} className="py-10 text-center text-slate-400">
                                            Nenhum departamento encontrado.
                                        </td>
                                    </tr>
                                )}

                                {!loading &&
                                    pagedDepartments.map((department) => (
                                        <tr key={department.id} className="border-t border-slate-100 hover:bg-slate-50/60">
                                            <td className="py-3 px-5">
                                                <div className="flex items-center gap-3">
                                                    <div className="w-9 h-9 rounded-lg bg-blue-50 flex items-center justify-center shrink-0">
                                                        <Tags size={16} className="text-blue-500" />
                                                    </div>
                                                    <span className="font-semibold text-slate-800">{department.name}</span>
                                                </div>
                                            </td>
                                            <td className="py-3 px-5 text-slate-500">{department.slug}</td>
                                            <td className="py-3 px-5">
                                                <TypeBadge isSystemDefault={department.isSystemDefault} />
                                            </td>
                                            <td className="py-3 px-5">
                                                <div className="flex items-center justify-end gap-2">
                                                    <button
                                                        onClick={() => openEditModal(department)}
                                                        disabled={department.isSystemDefault}
                                                        title={
                                                            department.isSystemDefault
                                                                ? 'Departamento padrão do sistema não pode ser editado'
                                                                : 'Editar departamento'
                                                        }
                                                        className="w-8 h-8 rounded-lg flex items-center justify-center text-blue-600 bg-blue-50 hover:bg-blue-100 disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
                                                    >
                                                        <Pencil size={14} />
                                                    </button>
                                                    <button
                                                        onClick={() => requestDelete(department)}
                                                        disabled={department.isSystemDefault}
                                                        title={
                                                            department.isSystemDefault
                                                                ? 'Departamento padrão do sistema não pode ser excluído'
                                                                : 'Excluir departamento'
                                                        }
                                                        className="w-8 h-8 rounded-lg flex items-center justify-center text-red-600 bg-red-50 hover:bg-red-100 disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
                                                    >
                                                        <Trash2 size={14} />
                                                    </button>
                                                </div>
                                            </td>
                                        </tr>
                                    ))}
                            </tbody>
                        </table>
                    </div>
                )}

                {!loadError && filteredDepartments.length > 0 && (
                    <div className="flex items-center justify-between px-5 py-4 border-t border-slate-100">
                        <p className="text-sm text-slate-500">
                            Mostrando {pagedDepartments.length} de {filteredDepartments.length} departamentos
                        </p>

                        {totalPages > 1 && (
                            <div className="flex items-center gap-2">
                                <button
                                    onClick={() => setPage((p) => Math.max(1, p - 1))}
                                    disabled={currentPage === 1}
                                    className="w-8 h-8 rounded-lg border border-slate-200 flex items-center justify-center text-slate-500 hover:bg-slate-50 disabled:opacity-40 disabled:cursor-not-allowed"
                                    aria-label="Página anterior"
                                >
                                    <ChevronRight size={14} className="rotate-180" />
                                </button>
                                {Array.from({ length: totalPages }, (_, i) => i + 1).map((p) => (
                                    <button
                                        key={p}
                                        onClick={() => setPage(p)}
                                        className={`w-8 h-8 rounded-lg text-sm font-medium flex items-center justify-center transition-colors ${
                                            p === currentPage
                                                ? 'bg-blue-600 text-white'
                                                : 'border border-slate-200 text-slate-600 hover:bg-slate-50'
                                        }`}
                                    >
                                        {p}
                                    </button>
                                ))}
                                <button
                                    onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                                    disabled={currentPage === totalPages}
                                    className="w-8 h-8 rounded-lg border border-slate-200 flex items-center justify-center text-slate-500 hover:bg-slate-50 disabled:opacity-40 disabled:cursor-not-allowed"
                                    aria-label="Próxima página"
                                >
                                    <ChevronRight size={14} />
                                </button>
                            </div>
                        )}
                    </div>
                )}
            </div>

            {/* Modal de criação/edição */}
            {isModalOpen && (
                <div className="fixed inset-0 bg-slate-900/50 flex items-center justify-center px-4 z-50">
                    <div className="bg-white rounded-2xl shadow-xl w-full max-w-md">
                        <div className="flex items-center justify-between px-6 py-4 border-b border-slate-100">
                            <h3 className="font-bold text-slate-900">
                                {editingDepartment ? 'Editar Departamento' : 'Novo Departamento'}
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
                                    Nome
                                </label>
                                <input
                                    type="text"
                                    required
                                    autoFocus
                                    value={form.name}
                                    onChange={(e) => handleNameChange(e.target.value)}
                                    placeholder="Ex: Eletrônicos"
                                    className="w-full bg-slate-50 border border-slate-200 rounded-lg px-4 py-2.5 text-sm text-slate-800 placeholder-slate-400 outline-none focus:ring-2 focus:ring-blue-500"
                                />
                            </div>

                            <div>
                                <label className="block text-xs font-semibold uppercase tracking-wider text-slate-500 mb-1.5">
                                    Slug
                                </label>
                                <input
                                    type="text"
                                    required
                                    value={form.slug}
                                    onChange={(e) => handleSlugChange(e.target.value)}
                                    placeholder="ex-eletronicos"
                                    className="w-full bg-slate-50 border border-slate-200 rounded-lg px-4 py-2.5 text-sm text-slate-800 placeholder-slate-400 outline-none focus:ring-2 focus:ring-blue-500"
                                />
                                <p className="text-[11px] text-slate-400 mt-1.5">
                                    Usado na URL da loja. Apenas letras minúsculas, números e hífens.
                                </p>
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
                                    {editingDepartment ? 'Salvar alterações' : 'Criar departamento'}
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            )}

            {/* Confirmação de exclusão */}
            {departmentToDelete && (
                <div className="fixed inset-0 bg-slate-900/50 flex items-center justify-center px-4 z-50">
                    <div className="bg-white rounded-2xl shadow-xl w-full max-w-sm p-6">
                        <div className="w-11 h-11 rounded-full bg-red-100 flex items-center justify-center mb-4">
                            <AlertTriangle size={20} className="text-red-600" />
                        </div>
                        <h3 className="font-bold text-slate-900 mb-1.5">Excluir departamento?</h3>
                        <p className="text-sm text-slate-500 mb-4">
                            Tem certeza que deseja excluir{' '}
                            <span className="font-semibold text-slate-700">{departmentToDelete.name}</span>? Esta ação
                            não pode ser desfeita.
                        </p>

                        {deleteError && (
                            <div className="mb-4 bg-red-50 border border-red-200 text-red-700 text-xs px-3 py-2.5 rounded-lg">
                                {deleteError}
                            </div>
                        )}

                        <div className="flex items-center justify-end gap-3">
                            <button
                                onClick={() => setDepartmentToDelete(null)}
                                disabled={deleting}
                                className="px-4 py-2.5 rounded-lg text-sm font-semibold text-slate-600 hover:bg-slate-100 transition-colors disabled:opacity-50"
                            >
                                Cancelar
                            </button>
                            <button
                                onClick={confirmDelete}
                                disabled={deleting}
                                className="flex items-center gap-2 bg-red-600 hover:bg-red-700 text-white text-sm font-semibold px-4 py-2.5 rounded-lg shadow-sm transition-colors disabled:opacity-60"
                            >
                                {deleting && (
                                    <span className="inline-block w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin" />
                                )}
                                Excluir
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </BackofficeLayout>
    );
}

export default Departments;
