import { useEffect, useMemo, useRef, useState, type FormEvent } from 'react';
import { BackofficeLayout } from '../../layouts/BackofficeLayout';
import {
    ProductService,
    DepartmentService,
    resolveStaticUrl,
    type ProductSummary,
    type ProductDetail,
    type Department,
} from '../../services/api';
import {
    ChevronRight,
    ChevronLeft,
    Plus,
    Search,
    Pencil,
    Trash2,
    Package,
    Boxes,
    MapPin,
    ImagePlus,
    X,
    AlertTriangle,
    ImageOff,
    Star,
    Upload,
} from 'lucide-react';

// --- Helpers ------------------------------------------------------------

const PAGE_SIZE = 12;

function StockBadge({ available }: { available: number }) {
    if (available <= 0) {
        return (
            <span className="inline-flex items-center text-xs font-semibold px-2.5 py-1 rounded-full bg-red-100 text-red-700">
                Sem estoque
            </span>
        );
    }
    if (available <= 5) {
        return (
            <span className="inline-flex items-center text-xs font-semibold px-2.5 py-1 rounded-full bg-amber-100 text-amber-700">
                {available} un. (baixo)
            </span>
        );
    }
    return (
        <span className="inline-flex items-center text-xs font-semibold px-2.5 py-1 rounded-full bg-emerald-100 text-emerald-700">
            {available} un.
        </span>
    );
}

interface CreateFormState {
    departmentId: string;
    sku: string;
    name: string;
    price: string;
    initialStock: string;
    aisle: string;
    rack: string;
    level: string;
    position: string;
}

const EMPTY_CREATE_FORM: CreateFormState = {
    departmentId: '',
    sku: '',
    name: '',
    price: '',
    initialStock: '0',
    aisle: '',
    rack: '',
    level: '',
    position: '',
};

type DetailTab = 'details' | 'location' | 'stock' | 'images';

// --- Página ---------------------------------------------------------------

export function Products() {
    const [products, setProducts] = useState<ProductSummary[]>([]);
    const [departments, setDepartments] = useState<Department[]>([]);
    const [totalCount, setTotalCount] = useState(0);
    const [page, setPage] = useState(1);
    const [loading, setLoading] = useState(true);
    const [loadError, setLoadError] = useState('');

    const [searchTerm, setSearchTerm] = useState('');
    const [searchInput, setSearchInput] = useState('');

    const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
    const [createForm, setCreateForm] = useState<CreateFormState>(EMPTY_CREATE_FORM);
    const [saving, setSaving] = useState(false);
    const [formError, setFormError] = useState('');

    const [deletingId, setDeletingId] = useState<number | null>(null);
    const [deleteError, setDeleteError] = useState('');

    const [selectedId, setSelectedId] = useState<number | null>(null);
    const [detail, setDetail] = useState<ProductDetail | null>(null);
    const [detailLoading, setDetailLoading] = useState(false);
    const [detailTab, setDetailTab] = useState<DetailTab>('details');

    const [detailsForm, setDetailsForm] = useState({ name: '', price: '' });
    const [savingDetails, setSavingDetails] = useState(false);
    const [detailsError, setDetailsError] = useState('');

    const [locationForm, setLocationForm] = useState({ aisle: '', rack: '', level: '', position: '' });
    const [savingLocation, setSavingLocation] = useState(false);
    const [locationError, setLocationError] = useState('');

    const [stockForm, setStockForm] = useState({ quantity: '', reason: '' });
    const [adjustForm, setAdjustForm] = useState({ newPhysicalQuantity: '', reason: '' });
    const [savingStock, setSavingStock] = useState(false);
    const [stockError, setStockError] = useState('');

    const [uploadingImage, setUploadingImage] = useState(false);
    const [imageError, setImageError] = useState('');
    const [uploadIsPrimary, setUploadIsPrimary] = useState(true);
    const fileInputRef = useRef<HTMLInputElement>(null);

    const departmentNameById = useMemo(() => {
        const map = new Map<number, string>();
        departments.forEach((d) => map.set(d.id, d.name));
        return map;
    }, [departments]);

    async function loadDepartments() {
        try {
            const data = await DepartmentService.getAll();
            setDepartments(data);
        } catch {
            // Departamentos são carregados de forma auxiliar; falha aqui não bloqueia a lista de produtos.
        }
    }

    async function loadList(targetPage = page, term = searchTerm) {
        setLoading(true);
        setLoadError('');
        try {
            const result = await ProductService.getAll(targetPage, PAGE_SIZE, term || undefined);
            setProducts(result.items);
            setTotalCount(result.totalCount);
        } catch (error) {
            setLoadError(error instanceof Error ? error.message : 'Não foi possível carregar os produtos.');
        } finally {
            setLoading(false);
        }
    }

    useEffect(() => {
        loadDepartments();
    }, []);

    useEffect(() => {
        loadList(page, searchTerm);
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [page, searchTerm]);

    useEffect(() => {
        const timeout = setTimeout(() => {
            setPage(1);
            setSearchTerm(searchInput.trim());
        }, 400);
        return () => clearTimeout(timeout);
    }, [searchInput]);

    const totalPages = useMemo(() => Math.max(1, Math.ceil(totalCount / PAGE_SIZE)), [totalCount]);

    const stats = useMemo(() => {
        const totalStock = products.reduce((sum, p) => sum + p.availableStock, 0);
        const lowStock = products.filter((p) => p.availableStock > 0 && p.availableStock <= 5).length;
        const outOfStock = products.filter((p) => p.availableStock <= 0).length;
        return { total: totalCount, totalStock, lowStock, outOfStock };
    }, [products, totalCount]);

    function openCreateModal() {
        setCreateForm({ ...EMPTY_CREATE_FORM, departmentId: departments[0] ? String(departments[0].id) : '' });
        setFormError('');
        setIsCreateModalOpen(true);
    }

    async function handleCreateSubmit(e: FormEvent) {
        e.preventDefault();
        setFormError('');

        const departmentId = Number(createForm.departmentId);
        const price = Number(createForm.price.replace(',', '.'));
        const initialStock = Number(createForm.initialStock);

        if (!departmentId) {
            setFormError('Selecione um departamento.');
            return;
        }
        if (!createForm.sku.trim()) {
            setFormError('Informe o SKU do produto.');
            return;
        }
        if (!createForm.name.trim()) {
            setFormError('Informe o nome do produto.');
            return;
        }
        if (!(price > 0)) {
            setFormError('O preço deve ser maior que zero.');
            return;
        }
        if (initialStock < 0) {
            setFormError('O estoque inicial não pode ser negativo.');
            return;
        }
        const hasPartialLocation =
            [createForm.aisle, createForm.rack, createForm.level, createForm.position].some((v) => v.trim()) &&
            [createForm.aisle, createForm.rack, createForm.level, createForm.position].some((v) => !v.trim());
        if (hasPartialLocation) {
            setFormError('Se informar a localização, preencha corredor, estante, nível e posição.');
            return;
        }

        setSaving(true);
        try {
            await ProductService.create({
                departmentId,
                sku: createForm.sku.trim(),
                name: createForm.name.trim(),
                price,
                initialStock,
                aisle: createForm.aisle.trim() || undefined,
                rack: createForm.rack.trim() || undefined,
                level: createForm.level.trim() || undefined,
                position: createForm.position.trim() || undefined,
            });
            setIsCreateModalOpen(false);
            await loadList(1, searchTerm);
            setPage(1);
        } catch (error) {
            setFormError(error instanceof Error ? error.message : 'Não foi possível criar o produto.');
        } finally {
            setSaving(false);
        }
    }

    async function handleDelete(product: ProductSummary) {
        setDeleteError('');
        setDeletingId(product.id);
        try {
            await ProductService.remove(product.id);
            await loadList();
        } catch (error) {
            setDeleteError(error instanceof Error ? error.message : 'Não foi possível excluir o produto.');
        } finally {
            setDeletingId(null);
        }
    }

    async function openDetail(productId: number) {
        setSelectedId(productId);
        setDetail(null);
        setDetailTab('details');
        setDetailLoading(true);
        setDetailsError('');
        setLocationError('');
        setStockError('');
        setImageError('');
        try {
            const data = await ProductService.getById(productId);
            setDetail(data);
            setDetailsForm({ name: data.name, price: String(data.price) });
            setLocationForm({
                aisle: data.aisle ?? '',
                rack: data.rack ?? '',
                level: data.level ?? '',
                position: data.position ?? '',
            });
        } catch (error) {
            setDetailsError(error instanceof Error ? error.message : 'Não foi possível carregar o produto.');
        } finally {
            setDetailLoading(false);
        }
    }

    function closeDetail() {
        setSelectedId(null);
        setDetail(null);
    }

    async function refreshDetail() {
        if (selectedId == null) return;
        const data = await ProductService.getById(selectedId);
        setDetail(data);
    }

    async function handleSaveDetails(e: FormEvent) {
        e.preventDefault();
        if (!detail) return;
        setDetailsError('');

        const price = Number(detailsForm.price.replace(',', '.'));
        if (!detailsForm.name.trim()) {
            setDetailsError('Informe o nome do produto.');
            return;
        }
        if (!(price > 0)) {
            setDetailsError('O preço deve ser maior que zero.');
            return;
        }

        setSavingDetails(true);
        try {
            await ProductService.updateDetails(detail.id, { name: detailsForm.name.trim(), price });
            await refreshDetail();
            await loadList();
        } catch (error) {
            setDetailsError(error instanceof Error ? error.message : 'Não foi possível salvar os detalhes.');
        } finally {
            setSavingDetails(false);
        }
    }

    async function handleSaveLocation(e: FormEvent) {
        e.preventDefault();
        if (!detail) return;
        setLocationError('');

        if (![locationForm.aisle, locationForm.rack, locationForm.level, locationForm.position].every((v) => v.trim())) {
            setLocationError('Preencha corredor, estante, nível e posição.');
            return;
        }

        setSavingLocation(true);
        try {
            await ProductService.updateLocation(detail.id, {
                aisle: locationForm.aisle.trim(),
                rack: locationForm.rack.trim(),
                level: locationForm.level.trim(),
                position: locationForm.position.trim(),
            });
            await refreshDetail();
            await loadList();
        } catch (error) {
            setLocationError(error instanceof Error ? error.message : 'Não foi possível salvar a localização.');
        } finally {
            setSavingLocation(false);
        }
    }

    async function handleAddStock(e: FormEvent) {
        e.preventDefault();
        if (!detail) return;
        setStockError('');

        const quantity = Number(stockForm.quantity);
        if (!(quantity > 0)) {
            setStockError('A quantidade deve ser maior que zero.');
            return;
        }
        if (!stockForm.reason.trim()) {
            setStockError('Informe o motivo da entrada de estoque.');
            return;
        }

        setSavingStock(true);
        try {
            await ProductService.addStock(detail.id, { quantity, reason: stockForm.reason.trim() });
            setStockForm({ quantity: '', reason: '' });
            await refreshDetail();
            await loadList();
        } catch (error) {
            setStockError(error instanceof Error ? error.message : 'Não foi possível adicionar estoque.');
        } finally {
            setSavingStock(false);
        }
    }

    async function handleAdjustInventory(e: FormEvent) {
        e.preventDefault();
        if (!detail) return;
        setStockError('');

        const newPhysicalQuantity = Number(adjustForm.newPhysicalQuantity);
        if (newPhysicalQuantity < 0) {
            setStockError('A quantidade física não pode ser negativa.');
            return;
        }
        if (!adjustForm.reason.trim()) {
            setStockError('Informe o motivo do ajuste de inventário.');
            return;
        }

        setSavingStock(true);
        try {
            await ProductService.adjustInventory(detail.id, { newPhysicalQuantity, reason: adjustForm.reason.trim() });
            setAdjustForm({ newPhysicalQuantity: '', reason: '' });
            await refreshDetail();
            await loadList();
        } catch (error) {
            setStockError(error instanceof Error ? error.message : 'Não foi possível ajustar o inventário.');
        } finally {
            setSavingStock(false);
        }
    }

    async function handleFileSelected(e: React.ChangeEvent<HTMLInputElement>) {
        const file = e.target.files?.[0];
        if (!file || !detail) return;
        setImageError('');
        setUploadingImage(true);
        try {
            const nextOrder = detail.images.length;
            await ProductService.uploadImage(detail.id, file, uploadIsPrimary, nextOrder);
            await refreshDetail();
        } catch (error) {
            setImageError(error instanceof Error ? error.message : 'Não foi possível enviar a imagem.');
        } finally {
            setUploadingImage(false);
            if (fileInputRef.current) fileInputRef.current.value = '';
        }
    }

    return (
        <BackofficeLayout>
            {/* Breadcrumb */}
            <div className="text-sm text-slate-500 mb-2 flex items-center gap-1.5">
                <span>Início</span>
                <ChevronRight size={14} />
                <span className="text-slate-700 font-medium">Produtos</span>
            </div>

            <div className="flex items-start justify-between mb-6 gap-4 flex-wrap">
                <div>
                    <h1 className="text-2xl font-bold text-slate-900">Produtos</h1>
                    <p className="text-sm text-slate-500 mt-1">
                        Cadastre produtos, gerencie estoque, endereçamento logístico e fotos
                    </p>
                </div>

                <button
                    onClick={openCreateModal}
                    disabled={departments.length === 0}
                    title={departments.length === 0 ? 'Cadastre um departamento primeiro' : undefined}
                    className="flex items-center gap-2 bg-blue-600 hover:bg-blue-700 text-white text-sm font-semibold px-4 py-2.5 rounded-lg shadow-sm transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                >
                    <Plus size={16} /> Novo Produto
                </button>
            </div>

            {/* Stat cards */}
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
                <div className="bg-white rounded-xl border border-slate-200 p-5 shadow-sm">
                    <div className="flex items-center gap-3 mb-3">
                        <div className="w-10 h-10 rounded-lg flex items-center justify-center bg-blue-100">
                            <Package size={20} className="text-blue-600" />
                        </div>
                        <p className="text-sm text-slate-500">Total de Produtos</p>
                    </div>
                    <p className="text-2xl font-bold text-slate-900">{stats.total}</p>
                </div>

                <div className="bg-white rounded-xl border border-slate-200 p-5 shadow-sm">
                    <div className="flex items-center gap-3 mb-3">
                        <div className="w-10 h-10 rounded-lg flex items-center justify-center bg-emerald-100">
                            <Boxes size={20} className="text-emerald-600" />
                        </div>
                        <p className="text-sm text-slate-500">Estoque disponível (página)</p>
                    </div>
                    <p className="text-2xl font-bold text-slate-900">{stats.totalStock}</p>
                </div>

                <div className="bg-white rounded-xl border border-slate-200 p-5 shadow-sm">
                    <div className="flex items-center gap-3 mb-3">
                        <div className="w-10 h-10 rounded-lg flex items-center justify-center bg-amber-100">
                            <AlertTriangle size={20} className="text-amber-600" />
                        </div>
                        <p className="text-sm text-slate-500">Estoque baixo (página)</p>
                    </div>
                    <p className="text-2xl font-bold text-slate-900">{stats.lowStock}</p>
                </div>

                <div className="bg-white rounded-xl border border-slate-200 p-5 shadow-sm">
                    <div className="flex items-center gap-3 mb-3">
                        <div className="w-10 h-10 rounded-lg flex items-center justify-center bg-red-100">
                            <ImageOff size={20} className="text-red-600" />
                        </div>
                        <p className="text-sm text-slate-500">Sem estoque (página)</p>
                    </div>
                    <p className="text-2xl font-bold text-slate-900">{stats.outOfStock}</p>
                </div>
            </div>

            {/* Lista */}
            <div className="bg-white rounded-xl border border-slate-200 shadow-sm">
                <div className="p-5 border-b border-slate-100 flex flex-wrap items-center justify-between gap-3">
                    <h2 className="font-bold text-slate-900">Lista de Produtos</h2>

                    <div className="relative">
                        <Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
                        <input
                            type="text"
                            value={searchInput}
                            onChange={(e) => setSearchInput(e.target.value)}
                            placeholder="Buscar por nome ou SKU..."
                            className="bg-slate-50 border border-slate-200 rounded-lg pl-9 pr-4 py-2 text-sm text-slate-700 placeholder-slate-400 outline-none focus:ring-2 focus:ring-blue-500 w-64"
                        />
                    </div>
                </div>

                {(loadError || deleteError) && (
                    <div className="m-5 flex items-center gap-2 bg-red-50 border border-red-200 text-red-700 text-sm px-4 py-3 rounded-lg">
                        <AlertTriangle size={16} /> {loadError || deleteError}
                    </div>
                )}

                {!loadError && (
                    <div className="overflow-x-auto">
                        <table className="w-full text-sm">
                            <thead>
                                <tr className="text-left text-xs text-slate-400 uppercase tracking-wide bg-slate-50">
                                    <th className="py-3 px-5 font-medium">Produto</th>
                                    <th className="py-3 px-5 font-medium">Departamento</th>
                                    <th className="py-3 px-5 font-medium">Preço</th>
                                    <th className="py-3 px-5 font-medium">Estoque</th>
                                    <th className="py-3 px-5 font-medium">Localização</th>
                                    <th className="py-3 px-5 font-medium text-right">Ações</th>
                                </tr>
                            </thead>
                            <tbody>
                                {loading && (
                                    <tr>
                                        <td colSpan={6} className="py-10 text-center text-slate-400">
                                            Carregando produtos...
                                        </td>
                                    </tr>
                                )}

                                {!loading && products.length === 0 && (
                                    <tr>
                                        <td colSpan={6} className="py-10 text-center text-slate-400">
                                            Nenhum produto encontrado.
                                        </td>
                                    </tr>
                                )}

                                {!loading &&
                                    products.map((product) => (
                                        <tr key={product.id} className="border-t border-slate-100 hover:bg-slate-50/60">
                                            <td className="py-3 px-5">
                                                <div className="flex items-center gap-3">
                                                    <div className="w-10 h-10 rounded-lg bg-slate-100 flex items-center justify-center shrink-0 overflow-hidden">
                                                        {product.primaryImagePath ? (
                                                            <img
                                                                src={resolveStaticUrl(product.primaryImagePath)}
                                                                alt={product.name}
                                                                className="w-full h-full object-cover"
                                                            />
                                                        ) : (
                                                            <ImageOff size={16} className="text-slate-400" />
                                                        )}
                                                    </div>
                                                    <div>
                                                        <p className="font-semibold text-slate-800">{product.name}</p>
                                                        <p className="text-xs text-slate-400">SKU: {product.sku}</p>
                                                    </div>
                                                </div>
                                            </td>
                                            <td className="py-3 px-5 text-slate-500">
                                                {departmentNameById.get(product.departmentId) ?? `#${product.departmentId}`}
                                            </td>
                                            <td className="py-3 px-5 text-slate-700 font-medium">
                                                {product.price.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}
                                            </td>
                                            <td className="py-3 px-5">
                                                <StockBadge available={product.availableStock} />
                                            </td>
                                            <td className="py-3 px-5">
                                                {product.hasLocation ? (
                                                    <span className="inline-flex items-center gap-1 text-xs font-semibold px-2.5 py-1 rounded-full bg-blue-100 text-blue-700">
                                                        <MapPin size={11} /> Endereçado
                                                    </span>
                                                ) : (
                                                    <span className="inline-flex items-center text-xs font-semibold px-2.5 py-1 rounded-full bg-slate-100 text-slate-500">
                                                        Sem local
                                                    </span>
                                                )}
                                            </td>
                                            <td className="py-3 px-5">
                                                <div className="flex items-center justify-end gap-2">
                                                    <button
                                                        onClick={() => openDetail(product.id)}
                                                        title="Editar produto"
                                                        className="w-8 h-8 rounded-lg flex items-center justify-center text-blue-600 bg-blue-50 hover:bg-blue-100 transition-colors"
                                                    >
                                                        <Pencil size={14} />
                                                    </button>
                                                    <button
                                                        onClick={() => handleDelete(product)}
                                                        disabled={deletingId === product.id}
                                                        title="Excluir produto"
                                                        className="w-8 h-8 rounded-lg flex items-center justify-center text-red-600 bg-red-50 hover:bg-red-100 transition-colors disabled:opacity-40"
                                                    >
                                                        {deletingId === product.id ? (
                                                            <span className="inline-block w-3.5 h-3.5 border-2 border-current border-t-transparent rounded-full animate-spin" />
                                                        ) : (
                                                            <Trash2 size={14} />
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

                {!loadError && (
                    <div className="px-5 py-4 border-t border-slate-100 flex items-center justify-between">
                        <p className="text-sm text-slate-500">
                            Página {page} de {totalPages} &middot; {totalCount} produtos
                        </p>
                        <div className="flex items-center gap-2">
                            <button
                                onClick={() => setPage((p) => Math.max(1, p - 1))}
                                disabled={page <= 1}
                                className="w-8 h-8 rounded-lg flex items-center justify-center text-slate-500 bg-slate-100 hover:bg-slate-200 disabled:opacity-40 disabled:cursor-not-allowed"
                            >
                                <ChevronLeft size={14} />
                            </button>
                            <button
                                onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                                disabled={page >= totalPages}
                                className="w-8 h-8 rounded-lg flex items-center justify-center text-slate-500 bg-slate-100 hover:bg-slate-200 disabled:opacity-40 disabled:cursor-not-allowed"
                            >
                                <ChevronRight size={14} />
                            </button>
                        </div>
                    </div>
                )}
            </div>

            {/* Modal de criação */}
            {isCreateModalOpen && (
                <div className="fixed inset-0 bg-slate-900/50 flex items-center justify-center px-4 z-50">
                    <div className="bg-white rounded-2xl shadow-xl w-full max-w-lg max-h-[90vh] overflow-y-auto">
                        <div className="flex items-center justify-between px-6 py-4 border-b border-slate-100">
                            <h3 className="font-bold text-slate-900">Novo Produto</h3>
                            <button
                                onClick={() => !saving && setIsCreateModalOpen(false)}
                                className="w-8 h-8 rounded-lg flex items-center justify-center text-slate-400 hover:bg-slate-100"
                                aria-label="Fechar"
                            >
                                <X size={18} />
                            </button>
                        </div>

                        <form onSubmit={handleCreateSubmit} className="px-6 py-5 space-y-4">
                            {formError && (
                                <div className="bg-red-50 border border-red-200 text-red-700 text-xs px-3 py-2.5 rounded-lg">
                                    {formError}
                                </div>
                            )}

                            <div>
                                <label className="block text-xs font-semibold uppercase tracking-wider text-slate-500 mb-1.5">
                                    Departamento
                                </label>
                                <select
                                    required
                                    value={createForm.departmentId}
                                    onChange={(e) => setCreateForm((p) => ({ ...p, departmentId: e.target.value }))}
                                    className="w-full bg-slate-50 border border-slate-200 rounded-lg px-4 py-2.5 text-sm text-slate-800 outline-none focus:ring-2 focus:ring-blue-500"
                                >
                                    <option value="" disabled>
                                        Selecione um departamento
                                    </option>
                                    {departments.map((d) => (
                                        <option key={d.id} value={d.id}>
                                            {d.name}
                                        </option>
                                    ))}
                                </select>
                            </div>

                            <div className="grid grid-cols-2 gap-3">
                                <div>
                                    <label className="block text-xs font-semibold uppercase tracking-wider text-slate-500 mb-1.5">
                                        SKU
                                    </label>
                                    <input
                                        type="text"
                                        required
                                        maxLength={50}
                                        value={createForm.sku}
                                        onChange={(e) => setCreateForm((p) => ({ ...p, sku: e.target.value }))}
                                        className="w-full bg-slate-50 border border-slate-200 rounded-lg px-4 py-2.5 text-sm text-slate-800 outline-none focus:ring-2 focus:ring-blue-500"
                                    />
                                </div>
                                <div>
                                    <label className="block text-xs font-semibold uppercase tracking-wider text-slate-500 mb-1.5">
                                        Preço
                                    </label>
                                    <input
                                        type="text"
                                        required
                                        inputMode="decimal"
                                        placeholder="0,00"
                                        value={createForm.price}
                                        onChange={(e) => setCreateForm((p) => ({ ...p, price: e.target.value }))}
                                        className="w-full bg-slate-50 border border-slate-200 rounded-lg px-4 py-2.5 text-sm text-slate-800 outline-none focus:ring-2 focus:ring-blue-500"
                                    />
                                </div>
                            </div>

                            <div>
                                <label className="block text-xs font-semibold uppercase tracking-wider text-slate-500 mb-1.5">
                                    Nome do produto
                                </label>
                                <input
                                    type="text"
                                    required
                                    maxLength={150}
                                    value={createForm.name}
                                    onChange={(e) => setCreateForm((p) => ({ ...p, name: e.target.value }))}
                                    className="w-full bg-slate-50 border border-slate-200 rounded-lg px-4 py-2.5 text-sm text-slate-800 outline-none focus:ring-2 focus:ring-blue-500"
                                />
                            </div>

                            <div>
                                <label className="block text-xs font-semibold uppercase tracking-wider text-slate-500 mb-1.5">
                                    Estoque inicial
                                </label>
                                <input
                                    type="number"
                                    min={0}
                                    value={createForm.initialStock}
                                    onChange={(e) => setCreateForm((p) => ({ ...p, initialStock: e.target.value }))}
                                    className="w-full bg-slate-50 border border-slate-200 rounded-lg px-4 py-2.5 text-sm text-slate-800 outline-none focus:ring-2 focus:ring-blue-500"
                                />
                            </div>

                            <div className="border-t border-slate-100 pt-4">
                                <p className="text-xs font-semibold uppercase tracking-wider text-slate-500 mb-2">
                                    Endereçamento logístico (opcional)
                                </p>
                                <div className="grid grid-cols-4 gap-2">
                                    <input
                                        type="text"
                                        placeholder="Corredor"
                                        value={createForm.aisle}
                                        onChange={(e) => setCreateForm((p) => ({ ...p, aisle: e.target.value }))}
                                        className="bg-slate-50 border border-slate-200 rounded-lg px-3 py-2 text-sm text-slate-800 outline-none focus:ring-2 focus:ring-blue-500"
                                    />
                                    <input
                                        type="text"
                                        placeholder="Estante"
                                        value={createForm.rack}
                                        onChange={(e) => setCreateForm((p) => ({ ...p, rack: e.target.value }))}
                                        className="bg-slate-50 border border-slate-200 rounded-lg px-3 py-2 text-sm text-slate-800 outline-none focus:ring-2 focus:ring-blue-500"
                                    />
                                    <input
                                        type="text"
                                        placeholder="Nível"
                                        value={createForm.level}
                                        onChange={(e) => setCreateForm((p) => ({ ...p, level: e.target.value }))}
                                        className="bg-slate-50 border border-slate-200 rounded-lg px-3 py-2 text-sm text-slate-800 outline-none focus:ring-2 focus:ring-blue-500"
                                    />
                                    <input
                                        type="text"
                                        placeholder="Posição"
                                        value={createForm.position}
                                        onChange={(e) => setCreateForm((p) => ({ ...p, position: e.target.value }))}
                                        className="bg-slate-50 border border-slate-200 rounded-lg px-3 py-2 text-sm text-slate-800 outline-none focus:ring-2 focus:ring-blue-500"
                                    />
                                </div>
                            </div>

                            <div className="flex items-center justify-end gap-3 pt-2">
                                <button
                                    type="button"
                                    onClick={() => setIsCreateModalOpen(false)}
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
                                    Criar produto
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            )}

            {/* Drawer de edição */}
            {selectedId != null && (
                <div className="fixed inset-0 bg-slate-900/50 flex items-center justify-center px-4 z-50">
                    <div className="bg-white rounded-2xl shadow-xl w-full max-w-lg max-h-[90vh] flex flex-col">
                        <div className="flex items-center justify-between px-6 py-4 border-b border-slate-100">
                            <h3 className="font-bold text-slate-900">{detail?.name ?? `Produto #${selectedId}`}</h3>
                            <button
                                onClick={closeDetail}
                                className="w-8 h-8 rounded-lg flex items-center justify-center text-slate-400 hover:bg-slate-100"
                                aria-label="Fechar"
                            >
                                <X size={18} />
                            </button>
                        </div>

                        {detailLoading && <p className="px-6 py-10 text-center text-slate-400">Carregando...</p>}

                        {detail && !detailLoading && (
                            <>
                                <div className="px-6 pt-4 flex items-center gap-2 border-b border-slate-100 overflow-x-auto">
                                    {(
                                        [
                                            ['details', 'Detalhes'],
                                            ['location', 'Localização'],
                                            ['stock', 'Estoque'],
                                            ['images', `Imagens (${detail.images.length})`],
                                        ] as [DetailTab, string][]
                                    ).map(([tab, label]) => (
                                        <button
                                            key={tab}
                                            onClick={() => setDetailTab(tab)}
                                            className={`px-3 py-2 text-sm font-semibold border-b-2 -mb-px whitespace-nowrap transition-colors ${
                                                detailTab === tab
                                                    ? 'border-blue-600 text-blue-600'
                                                    : 'border-transparent text-slate-500 hover:text-slate-700'
                                            }`}
                                        >
                                            {label}
                                        </button>
                                    ))}
                                </div>

                                <div className="px-6 py-5 overflow-y-auto space-y-4">
                                    {detailTab === 'details' && (
                                        <form onSubmit={handleSaveDetails} className="space-y-3">
                                            {detailsError && (
                                                <div className="bg-red-50 border border-red-200 text-red-700 text-xs px-3 py-2.5 rounded-lg">
                                                    {detailsError}
                                                </div>
                                            )}
                                            <p className="text-xs text-slate-400">
                                                SKU: <span className="font-semibold text-slate-600">{detail.sku}</span> &middot;
                                                Departamento:{' '}
                                                <span className="font-semibold text-slate-600">
                                                    {departmentNameById.get(detail.departmentId) ?? `#${detail.departmentId}`}
                                                </span>
                                            </p>
                                            <div>
                                                <label className="block text-xs font-semibold uppercase tracking-wider text-slate-500 mb-1.5">
                                                    Nome
                                                </label>
                                                <input
                                                    type="text"
                                                    maxLength={150}
                                                    value={detailsForm.name}
                                                    onChange={(e) => setDetailsForm((p) => ({ ...p, name: e.target.value }))}
                                                    className="w-full bg-slate-50 border border-slate-200 rounded-lg px-4 py-2.5 text-sm text-slate-800 outline-none focus:ring-2 focus:ring-blue-500"
                                                />
                                            </div>
                                            <div>
                                                <label className="block text-xs font-semibold uppercase tracking-wider text-slate-500 mb-1.5">
                                                    Preço
                                                </label>
                                                <input
                                                    type="text"
                                                    inputMode="decimal"
                                                    value={detailsForm.price}
                                                    onChange={(e) => setDetailsForm((p) => ({ ...p, price: e.target.value }))}
                                                    className="w-full bg-slate-50 border border-slate-200 rounded-lg px-4 py-2.5 text-sm text-slate-800 outline-none focus:ring-2 focus:ring-blue-500"
                                                />
                                            </div>
                                            <button
                                                type="submit"
                                                disabled={savingDetails}
                                                className="w-full flex items-center justify-center gap-2 bg-blue-600 hover:bg-blue-700 text-white text-sm font-semibold px-4 py-2.5 rounded-lg shadow-sm transition-colors disabled:opacity-60"
                                            >
                                                {savingDetails && (
                                                    <span className="inline-block w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin" />
                                                )}
                                                Salvar detalhes
                                            </button>
                                        </form>
                                    )}

                                    {detailTab === 'location' && (
                                        <form onSubmit={handleSaveLocation} className="space-y-3">
                                            {locationError && (
                                                <div className="bg-red-50 border border-red-200 text-red-700 text-xs px-3 py-2.5 rounded-lg">
                                                    {locationError}
                                                </div>
                                            )}
                                            <div className="grid grid-cols-2 gap-3">
                                                <div>
                                                    <label className="block text-xs font-semibold uppercase tracking-wider text-slate-500 mb-1.5">
                                                        Corredor
                                                    </label>
                                                    <input
                                                        type="text"
                                                        value={locationForm.aisle}
                                                        onChange={(e) => setLocationForm((p) => ({ ...p, aisle: e.target.value }))}
                                                        className="w-full bg-slate-50 border border-slate-200 rounded-lg px-4 py-2.5 text-sm text-slate-800 outline-none focus:ring-2 focus:ring-blue-500"
                                                    />
                                                </div>
                                                <div>
                                                    <label className="block text-xs font-semibold uppercase tracking-wider text-slate-500 mb-1.5">
                                                        Estante
                                                    </label>
                                                    <input
                                                        type="text"
                                                        value={locationForm.rack}
                                                        onChange={(e) => setLocationForm((p) => ({ ...p, rack: e.target.value }))}
                                                        className="w-full bg-slate-50 border border-slate-200 rounded-lg px-4 py-2.5 text-sm text-slate-800 outline-none focus:ring-2 focus:ring-blue-500"
                                                    />
                                                </div>
                                                <div>
                                                    <label className="block text-xs font-semibold uppercase tracking-wider text-slate-500 mb-1.5">
                                                        Nível
                                                    </label>
                                                    <input
                                                        type="text"
                                                        value={locationForm.level}
                                                        onChange={(e) => setLocationForm((p) => ({ ...p, level: e.target.value }))}
                                                        className="w-full bg-slate-50 border border-slate-200 rounded-lg px-4 py-2.5 text-sm text-slate-800 outline-none focus:ring-2 focus:ring-blue-500"
                                                    />
                                                </div>
                                                <div>
                                                    <label className="block text-xs font-semibold uppercase tracking-wider text-slate-500 mb-1.5">
                                                        Posição
                                                    </label>
                                                    <input
                                                        type="text"
                                                        value={locationForm.position}
                                                        onChange={(e) => setLocationForm((p) => ({ ...p, position: e.target.value }))}
                                                        className="w-full bg-slate-50 border border-slate-200 rounded-lg px-4 py-2.5 text-sm text-slate-800 outline-none focus:ring-2 focus:ring-blue-500"
                                                    />
                                                </div>
                                            </div>
                                            <button
                                                type="submit"
                                                disabled={savingLocation}
                                                className="w-full flex items-center justify-center gap-2 bg-blue-600 hover:bg-blue-700 text-white text-sm font-semibold px-4 py-2.5 rounded-lg shadow-sm transition-colors disabled:opacity-60"
                                            >
                                                {savingLocation && (
                                                    <span className="inline-block w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin" />
                                                )}
                                                Salvar localização
                                            </button>
                                        </form>
                                    )}

                                    {detailTab === 'stock' && (
                                        <div className="space-y-6">
                                            {stockError && (
                                                <div className="bg-red-50 border border-red-200 text-red-700 text-xs px-3 py-2.5 rounded-lg">
                                                    {stockError}
                                                </div>
                                            )}

                                            <div className="grid grid-cols-3 gap-3 text-center">
                                                <div className="bg-slate-50 rounded-lg p-3">
                                                    <p className="text-xs text-slate-400">Físico</p>
                                                    <p className="text-lg font-bold text-slate-800">{detail.physicalStock}</p>
                                                </div>
                                                <div className="bg-slate-50 rounded-lg p-3">
                                                    <p className="text-xs text-slate-400">Reservado</p>
                                                    <p className="text-lg font-bold text-slate-800">{detail.reservedStock}</p>
                                                </div>
                                                <div className="bg-slate-50 rounded-lg p-3">
                                                    <p className="text-xs text-slate-400">Disponível</p>
                                                    <p className="text-lg font-bold text-emerald-600">{detail.availableStock}</p>
                                                </div>
                                            </div>

                                            <form onSubmit={handleAddStock} className="space-y-2 border-t border-slate-100 pt-4">
                                                <p className="text-xs font-semibold uppercase tracking-wider text-slate-500">
                                                    Adicionar entrada de estoque
                                                </p>
                                                <div className="flex gap-2">
                                                    <input
                                                        type="number"
                                                        min={1}
                                                        placeholder="Quantidade"
                                                        value={stockForm.quantity}
                                                        onChange={(e) => setStockForm((p) => ({ ...p, quantity: e.target.value }))}
                                                        className="w-28 bg-slate-50 border border-slate-200 rounded-lg px-3 py-2 text-sm text-slate-800 outline-none focus:ring-2 focus:ring-blue-500"
                                                    />
                                                    <input
                                                        type="text"
                                                        placeholder="Motivo (ex: reposição de fornecedor)"
                                                        value={stockForm.reason}
                                                        onChange={(e) => setStockForm((p) => ({ ...p, reason: e.target.value }))}
                                                        className="flex-1 bg-slate-50 border border-slate-200 rounded-lg px-3 py-2 text-sm text-slate-800 outline-none focus:ring-2 focus:ring-blue-500"
                                                    />
                                                </div>
                                                <button
                                                    type="submit"
                                                    disabled={savingStock}
                                                    className="w-full bg-emerald-600 hover:bg-emerald-700 text-white text-sm font-semibold px-4 py-2.5 rounded-lg shadow-sm transition-colors disabled:opacity-60"
                                                >
                                                    Adicionar
                                                </button>
                                            </form>

                                            <form onSubmit={handleAdjustInventory} className="space-y-2 border-t border-slate-100 pt-4">
                                                <p className="text-xs font-semibold uppercase tracking-wider text-slate-500">
                                                    Ajustar inventário (contagem física)
                                                </p>
                                                <div className="flex gap-2">
                                                    <input
                                                        type="number"
                                                        min={0}
                                                        placeholder="Novo total físico"
                                                        value={adjustForm.newPhysicalQuantity}
                                                        onChange={(e) =>
                                                            setAdjustForm((p) => ({ ...p, newPhysicalQuantity: e.target.value }))
                                                        }
                                                        className="w-32 bg-slate-50 border border-slate-200 rounded-lg px-3 py-2 text-sm text-slate-800 outline-none focus:ring-2 focus:ring-blue-500"
                                                    />
                                                    <input
                                                        type="text"
                                                        placeholder="Motivo (ex: balanço de estoque)"
                                                        value={adjustForm.reason}
                                                        onChange={(e) => setAdjustForm((p) => ({ ...p, reason: e.target.value }))}
                                                        className="flex-1 bg-slate-50 border border-slate-200 rounded-lg px-3 py-2 text-sm text-slate-800 outline-none focus:ring-2 focus:ring-blue-500"
                                                    />
                                                </div>
                                                <button
                                                    type="submit"
                                                    disabled={savingStock}
                                                    className="w-full bg-slate-800 hover:bg-slate-900 text-white text-sm font-semibold px-4 py-2.5 rounded-lg shadow-sm transition-colors disabled:opacity-60"
                                                >
                                                    Ajustar inventário
                                                </button>
                                            </form>
                                        </div>
                                    )}

                                    {detailTab === 'images' && (
                                        <div className="space-y-4">
                                            {imageError && (
                                                <div className="bg-red-50 border border-red-200 text-red-700 text-xs px-3 py-2.5 rounded-lg">
                                                    {imageError}
                                                </div>
                                            )}

                                            <div className="grid grid-cols-3 gap-3">
                                                {detail.images.map((image) => (
                                                    <div
                                                        key={image.id}
                                                        className="relative aspect-square rounded-lg overflow-hidden border border-slate-200 bg-slate-50"
                                                    >
                                                        <img
                                                            src={resolveStaticUrl(image.filePath)}
                                                            alt={image.fileName}
                                                            className="w-full h-full object-cover"
                                                        />
                                                        {image.isPrimary && (
                                                            <span className="absolute top-1.5 right-1.5 bg-amber-400 text-white rounded-full p-1">
                                                                <Star size={10} fill="currentColor" />
                                                            </span>
                                                        )}
                                                    </div>
                                                ))}

                                                {detail.images.length === 0 && (
                                                    <div className="col-span-3 py-8 text-center text-slate-400 text-sm">
                                                        Nenhuma imagem cadastrada.
                                                    </div>
                                                )}
                                            </div>

                                            <div className="border-t border-slate-100 pt-4 space-y-2">
                                                <label className="flex items-center gap-2 text-sm text-slate-600">
                                                    <input
                                                        type="checkbox"
                                                        checked={uploadIsPrimary}
                                                        onChange={(e) => setUploadIsPrimary(e.target.checked)}
                                                        className="rounded border-slate-300"
                                                    />
                                                    Definir como imagem principal
                                                </label>
                                                <button
                                                    type="button"
                                                    onClick={() => fileInputRef.current?.click()}
                                                    disabled={uploadingImage}
                                                    className="w-full flex items-center justify-center gap-2 border-2 border-dashed border-slate-300 hover:border-blue-400 text-slate-500 hover:text-blue-600 text-sm font-semibold px-4 py-4 rounded-lg transition-colors disabled:opacity-60"
                                                >
                                                    {uploadingImage ? (
                                                        <>
                                                            <span className="inline-block w-4 h-4 border-2 border-current border-t-transparent rounded-full animate-spin" />
                                                            Enviando...
                                                        </>
                                                    ) : (
                                                        <>
                                                            <Upload size={16} /> Enviar foto
                                                        </>
                                                    )}
                                                </button>
                                                <input
                                                    ref={fileInputRef}
                                                    type="file"
                                                    accept="image/*"
                                                    className="hidden"
                                                    onChange={handleFileSelected}
                                                />
                                                <p className="text-xs text-slate-400 flex items-center gap-1">
                                                    <ImagePlus size={12} /> JPG/PNG — a imagem é redimensionada e comprimida
                                                    automaticamente pelo servidor.
                                                </p>
                                            </div>
                                        </div>
                                    )}
                                </div>
                            </>
                        )}
                    </div>
                </div>
            )}
        </BackofficeLayout>
    );
}

export default Products;
