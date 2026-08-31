import { useEffect, useMemo, useState, type FormEvent } from 'react';
import { BackofficeLayout } from '../../layouts/BackofficeLayout';
import {
    StoreBannerService,
    type StoreBanner,
    type CreateStoreBannerPayload,
    type UpdateStoreBannerPayload,
} from '../../services/api';
import {
    ChevronRight,
    Plus,
    Search,
    Pencil,
    Trash2,
    Image as ImageIcon,
    Percent,
    ArrowUpDown,
    X,
    AlertTriangle,
} from 'lucide-react';

// --- Helpers ------------------------------------------------------------

const HEX_COLOR_REGEX = /^#([0-9a-f]{3}|[0-9a-f]{6})$/i;

interface BannerFormState {
    title: string;
    subtitle: string;
    discountPercentage: string;
    buttonText: string;
    buttonLink: string;
    backgroundColor: string;
    displayOrder: string;
    isActive: boolean;
}

const EMPTY_FORM: BannerFormState = {
    title: '',
    subtitle: '',
    buttonText: 'Ver ofertas',
    buttonLink: '#',
    discountPercentage: '',
    backgroundColor: '#f95d00',
    displayOrder: '1',
    isActive: true,
};

// Mesma lógica visual usada na loja (BannerRow.tsx): título quebra linha nos
// espaços, e o desconto substitui o subtítulo quando presente.
function BannerPreview({ form }: { form: BannerFormState }) {
    return (
        <div
            style={{ backgroundColor: HEX_COLOR_REGEX.test(form.backgroundColor) ? form.backgroundColor : '#94a3b8' }}
            className="h-[150px] w-full rounded-2xl p-4 text-white shadow-sm relative overflow-hidden flex flex-col justify-between"
        >
            <div>
                <h2 className="text-lg font-extrabold leading-tight whitespace-pre-line">
                    {(form.title || 'Título do banner').replace(/ /g, '\n')}
                </h2>

                {form.discountPercentage ? (
                    <div className="mt-1">
                        <span className="text-[10px] font-bold block mb-[-6px]">Até</span>
                        <div className="flex items-start">
                            <span className="text-4xl font-black tracking-tighter">{form.discountPercentage}</span>
                            <div className="flex flex-col ml-1 mt-0.5">
                                <span className="text-sm font-black leading-none">%</span>
                                <span className="text-sm font-black leading-none">OFF</span>
                            </div>
                        </div>
                    </div>
                ) : (
                    <p className="text-[11px] mt-1 font-medium opacity-90 leading-snug">
                        {form.subtitle || 'Subtítulo de apoio do banner'}
                    </p>
                )}
            </div>

            <span className="absolute bottom-4 right-4 bg-[#0f172a] text-white text-[11px] font-bold py-1.5 px-4 rounded-full z-10 shadow-md text-center">
                {form.buttonText || 'Ver ofertas'}
            </span>
        </div>
    );
}

// --- Página ---------------------------------------------------------------

export function StoreBanners() {
    const [banners, setBanners] = useState<StoreBanner[]>([]);
    const [loading, setLoading] = useState(true);
    const [loadError, setLoadError] = useState('');

    const [searchTerm, setSearchTerm] = useState('');

    const [isModalOpen, setIsModalOpen] = useState(false);
    const [editingBanner, setEditingBanner] = useState<StoreBanner | null>(null);
    const [form, setForm] = useState<BannerFormState>(EMPTY_FORM);
    const [saving, setSaving] = useState(false);
    const [formError, setFormError] = useState('');

    const [bannerToDelete, setBannerToDelete] = useState<StoreBanner | null>(null);
    const [deleting, setDeleting] = useState(false);
    const [deleteError, setDeleteError] = useState('');

    async function loadBanners() {
        setLoading(true);
        setLoadError('');
        try {
            const data = await StoreBannerService.getAll();
            setBanners([...data].sort((a, b) => a.displayOrder - b.displayOrder));
        } catch (error) {
            setLoadError(error instanceof Error ? error.message : 'Não foi possível carregar os banners.');
        } finally {
            setLoading(false);
        }
    }

    useEffect(() => {
        loadBanners();
    }, []);

    const filteredBanners = useMemo(() => {
        const term = searchTerm.trim().toLowerCase();
        if (!term) return banners;
        return banners.filter(
            (banner) =>
                banner.title.toLowerCase().includes(term) ||
                (banner.subtitle ?? '').toLowerCase().includes(term)
        );
    }, [banners, searchTerm]);

    const stats = useMemo(() => {
        const total = banners.length;
        const withDiscount = banners.filter((b) => !!b.discountPercentage).length;
        const maxOrder = banners.reduce((max, b) => Math.max(max, b.displayOrder), 0);
        return { total, withDiscount, maxOrder };
    }, [banners]);

    function openCreateModal() {
        setEditingBanner(null);
        setForm({ ...EMPTY_FORM, displayOrder: String(stats.maxOrder + 1) });
        setFormError('');
        setIsModalOpen(true);
    }

    function openEditModal(banner: StoreBanner) {
        setEditingBanner(banner);
        setForm({
            title: banner.title,
            subtitle: banner.subtitle ?? '',
            discountPercentage: banner.discountPercentage ?? '',
            buttonText: banner.buttonText,
            buttonLink: banner.buttonLink,
            backgroundColor: banner.backgroundColor,
            displayOrder: String(banner.displayOrder),
            // A listagem da API não devolve o status Ativo/Inativo do banner,
            // então o formulário assume "Ativo" ao abrir uma edição — ajuste
            // manualmente se este banner estiver pausado.
            isActive: true,
        });
        setFormError('');
        setIsModalOpen(true);
    }

    function closeModal() {
        if (saving) return;
        setIsModalOpen(false);
    }

    function updateField<K extends keyof BannerFormState>(key: K, value: BannerFormState[K]) {
        setForm((prev) => ({ ...prev, [key]: value }));
    }

    function validateForm(): string | null {
        const title = form.title.trim();
        const buttonText = form.buttonText.trim();
        const buttonLink = form.buttonLink.trim();
        const backgroundColor = form.backgroundColor.trim();
        const displayOrder = Number(form.displayOrder);

        if (!title) return 'Informe o título do banner.';
        if (title.length > 150) return 'O título pode ter no máximo 150 caracteres.';
        if (form.subtitle.trim().length > 250) return 'O subtítulo pode ter no máximo 250 caracteres.';
        if (!buttonText) return 'Informe o texto do botão.';
        if (buttonText.length > 50) return 'O texto do botão pode ter no máximo 50 caracteres.';
        if (!buttonLink) return 'Informe o link do botão.';
        if (buttonLink.length > 250) return 'O link pode ter no máximo 250 caracteres.';
        if (!backgroundColor) return 'Informe a cor de fundo.';
        if (backgroundColor.length > 30) return 'A cor de fundo pode ter no máximo 30 caracteres.';
        if (!Number.isInteger(displayOrder) || displayOrder <= 0) return 'A ordem de exibição deve ser um número inteiro maior que zero.';

        return null;
    }

    async function handleSubmit(e: FormEvent) {
        e.preventDefault();
        setFormError('');

        const validationMessage = validateForm();
        if (validationMessage) {
            setFormError(validationMessage);
            return;
        }

        const basePayload: CreateStoreBannerPayload = {
            title: form.title.trim(),
            subtitle: form.subtitle.trim() || undefined,
            discountPercentage: form.discountPercentage.trim() || undefined,
            buttonText: form.buttonText.trim(),
            buttonLink: form.buttonLink.trim(),
            backgroundColor: form.backgroundColor.trim(),
            displayOrder: Number(form.displayOrder),
        };

        setSaving(true);
        try {
            if (editingBanner) {
                const updatePayload: UpdateStoreBannerPayload = { ...basePayload, isActive: form.isActive };
                await StoreBannerService.update(editingBanner.id, updatePayload);
            } else {
                await StoreBannerService.create(basePayload);
            }
            setIsModalOpen(false);
            await loadBanners();
        } catch (error) {
            setFormError(error instanceof Error ? error.message : 'Não foi possível salvar o banner.');
        } finally {
            setSaving(false);
        }
    }

    function requestDelete(banner: StoreBanner) {
        setDeleteError('');
        setBannerToDelete(banner);
    }

    async function confirmDelete() {
        if (!bannerToDelete) return;
        setDeleting(true);
        setDeleteError('');
        try {
            await StoreBannerService.remove(bannerToDelete.id);
            setBannerToDelete(null);
            await loadBanners();
        } catch (error) {
            setDeleteError(error instanceof Error ? error.message : 'Não foi possível excluir o banner.');
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
                <span className="text-slate-700 font-medium">Banners</span>
            </div>

            <div className="flex items-start justify-between mb-6 gap-4 flex-wrap">
                <div>
                    <h1 className="text-2xl font-bold text-slate-900">Banners da Loja</h1>
                    <p className="text-sm text-slate-500 mt-1">
                        Gerencie os banners promocionais exibidos na vitrine da sua loja online
                    </p>
                </div>

                <button
                    onClick={openCreateModal}
                    className="flex items-center gap-2 bg-blue-600 hover:bg-blue-700 text-white text-sm font-semibold px-4 py-2.5 rounded-lg shadow-sm transition-colors"
                >
                    <Plus size={16} /> Novo Banner
                </button>
            </div>

            {/* Stat cards */}
            <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 mb-6">
                <div className="bg-white rounded-xl border border-slate-200 p-5 shadow-sm">
                    <div className="flex items-center gap-3 mb-3">
                        <div className="w-10 h-10 rounded-lg flex items-center justify-center bg-blue-100">
                            <ImageIcon size={20} className="text-blue-600" />
                        </div>
                        <p className="text-sm text-slate-500">Total de Banners</p>
                    </div>
                    <p className="text-2xl font-bold text-slate-900">{stats.total}</p>
                </div>

                <div className="bg-white rounded-xl border border-slate-200 p-5 shadow-sm">
                    <div className="flex items-center gap-3 mb-3">
                        <div className="w-10 h-10 rounded-lg flex items-center justify-center bg-orange-100">
                            <Percent size={20} className="text-orange-600" />
                        </div>
                        <p className="text-sm text-slate-500">Com Desconto</p>
                    </div>
                    <p className="text-2xl font-bold text-slate-900">{stats.withDiscount}</p>
                </div>

                <div className="bg-white rounded-xl border border-slate-200 p-5 shadow-sm">
                    <div className="flex items-center gap-3 mb-3">
                        <div className="w-10 h-10 rounded-lg flex items-center justify-center bg-purple-100">
                            <ArrowUpDown size={20} className="text-purple-600" />
                        </div>
                        <p className="text-sm text-slate-500">Maior Ordem de Exibição</p>
                    </div>
                    <p className="text-2xl font-bold text-slate-900">{stats.maxOrder}</p>
                </div>
            </div>

            {/* Lista */}
            <div className="bg-white rounded-xl border border-slate-200 shadow-sm">
                <div className="p-5 border-b border-slate-100 flex flex-wrap items-center justify-between gap-3">
                    <h2 className="font-bold text-slate-900">Banners cadastrados</h2>

                    <div className="relative">
                        <Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
                        <input
                            type="text"
                            value={searchTerm}
                            onChange={(e) => setSearchTerm(e.target.value)}
                            placeholder="Buscar por título ou subtítulo..."
                            className="bg-slate-50 border border-slate-200 rounded-lg pl-9 pr-4 py-2 text-sm text-slate-700 placeholder-slate-400 outline-none focus:ring-2 focus:ring-blue-500 w-64"
                        />
                    </div>
                </div>

                {loadError && (
                    <div className="m-5 flex items-center gap-2 bg-red-50 border border-red-200 text-red-700 text-sm px-4 py-3 rounded-lg">
                        <AlertTriangle size={16} /> {loadError}
                    </div>
                )}

                {!loadError && loading && (
                    <div className="py-14 text-center text-slate-400 text-sm">Carregando banners...</div>
                )}

                {!loadError && !loading && filteredBanners.length === 0 && (
                    <div className="py-14 text-center text-slate-400 text-sm">Nenhum banner encontrado.</div>
                )}

                {!loadError && !loading && filteredBanners.length > 0 && (
                    <div className="p-5 grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-3 gap-4">
                        {filteredBanners.map((banner) => (
                            <div key={banner.id} className="group relative rounded-2xl overflow-hidden shadow-sm">
                                <div
                                    style={{
                                        backgroundColor: HEX_COLOR_REGEX.test(banner.backgroundColor)
                                            ? banner.backgroundColor
                                            : '#94a3b8',
                                    }}
                                    className="h-[150px] w-full p-4 text-white relative overflow-hidden flex flex-col justify-between"
                                >
                                    <div>
                                        <h3 className="text-lg font-extrabold leading-tight whitespace-pre-line">
                                            {banner.title.replace(/ /g, '\n')}
                                        </h3>

                                        {banner.discountPercentage ? (
                                            <div className="mt-1">
                                                <span className="text-[10px] font-bold block mb-[-6px]">Até</span>
                                                <div className="flex items-start">
                                                    <span className="text-4xl font-black tracking-tighter">
                                                        {banner.discountPercentage}
                                                    </span>
                                                    <div className="flex flex-col ml-1 mt-0.5">
                                                        <span className="text-sm font-black leading-none">%</span>
                                                        <span className="text-sm font-black leading-none">OFF</span>
                                                    </div>
                                                </div>
                                            </div>
                                        ) : (
                                            <p className="text-[11px] mt-1 font-medium opacity-90 leading-snug">
                                                {banner.subtitle}
                                            </p>
                                        )}
                                    </div>

                                    <span className="absolute bottom-4 right-4 bg-[#0f172a] text-white text-[11px] font-bold py-1.5 px-4 rounded-full z-10 shadow-md text-center">
                                        {banner.buttonText}
                                    </span>

                                    <span className="absolute top-3 left-3 bg-black/30 text-white text-[10px] font-bold px-2 py-1 rounded-full backdrop-blur-sm">
                                        Ordem {banner.displayOrder}
                                    </span>
                                </div>

                                {/* Overlay de ações (aparece no hover) */}
                                <div className="absolute inset-0 bg-slate-900/0 group-hover:bg-slate-900/40 transition-colors flex items-start justify-end p-3 gap-2 opacity-0 group-hover:opacity-100">
                                    <button
                                        onClick={() => openEditModal(banner)}
                                        title="Editar banner"
                                        className="w-8 h-8 rounded-lg flex items-center justify-center bg-white text-blue-600 hover:bg-blue-50 shadow-sm transition-colors"
                                    >
                                        <Pencil size={14} />
                                    </button>
                                    <button
                                        onClick={() => requestDelete(banner)}
                                        title="Excluir banner"
                                        className="w-8 h-8 rounded-lg flex items-center justify-center bg-white text-red-600 hover:bg-red-50 shadow-sm transition-colors"
                                    >
                                        <Trash2 size={14} />
                                    </button>
                                </div>
                            </div>
                        ))}
                    </div>
                )}

                {!loadError && filteredBanners.length > 0 && (
                    <p className="px-5 pb-5 text-sm text-slate-500">
                        Mostrando {filteredBanners.length} de {banners.length} banners
                    </p>
                )}
            </div>

            {/* Modal de criação/edição */}
            {isModalOpen && (
                <div className="fixed inset-0 bg-slate-900/50 flex items-center justify-center px-4 py-8 z-50 overflow-y-auto">
                    <div className="bg-white rounded-2xl shadow-xl w-full max-w-3xl my-auto">
                        <div className="flex items-center justify-between px-6 py-4 border-b border-slate-100">
                            <h3 className="font-bold text-slate-900">
                                {editingBanner ? 'Editar Banner' : 'Novo Banner'}
                            </h3>
                            <button
                                onClick={closeModal}
                                className="w-8 h-8 rounded-lg flex items-center justify-center text-slate-400 hover:bg-slate-100"
                                aria-label="Fechar"
                            >
                                <X size={18} />
                            </button>
                        </div>

                        <form onSubmit={handleSubmit} className="px-6 py-5">
                            {formError && (
                                <div className="mb-4 bg-red-50 border border-red-200 text-red-700 text-xs px-3 py-2.5 rounded-lg">
                                    {formError}
                                </div>
                            )}

                            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                                <div className="space-y-4">
                                    <div>
                                        <label className="block text-xs font-semibold uppercase tracking-wider text-slate-500 mb-1.5">
                                            Título
                                        </label>
                                        <input
                                            type="text"
                                            required
                                            autoFocus
                                            maxLength={150}
                                            value={form.title}
                                            onChange={(e) => updateField('title', e.target.value)}
                                            placeholder="Ex: Mega Promoção de Verão"
                                            className="w-full bg-slate-50 border border-slate-200 rounded-lg px-4 py-2.5 text-sm text-slate-800 placeholder-slate-400 outline-none focus:ring-2 focus:ring-blue-500"
                                        />
                                    </div>

                                    <div>
                                        <label className="block text-xs font-semibold uppercase tracking-wider text-slate-500 mb-1.5">
                                            Subtítulo
                                        </label>
                                        <input
                                            type="text"
                                            maxLength={250}
                                            value={form.subtitle}
                                            onChange={(e) => updateField('subtitle', e.target.value)}
                                            placeholder="Ex: Aproveite condições especiais"
                                            className="w-full bg-slate-50 border border-slate-200 rounded-lg px-4 py-2.5 text-sm text-slate-800 placeholder-slate-400 outline-none focus:ring-2 focus:ring-blue-500"
                                        />
                                        <p className="text-[11px] text-slate-400 mt-1.5">
                                            Exibido apenas quando o banner não tiver um desconto configurado.
                                        </p>
                                    </div>

                                    <div>
                                        <label className="block text-xs font-semibold uppercase tracking-wider text-slate-500 mb-1.5">
                                            Desconto (%)
                                        </label>
                                        <input
                                            type="text"
                                            value={form.discountPercentage}
                                            onChange={(e) => updateField('discountPercentage', e.target.value)}
                                            placeholder="Ex: 40 (deixe em branco para usar o subtítulo)"
                                            className="w-full bg-slate-50 border border-slate-200 rounded-lg px-4 py-2.5 text-sm text-slate-800 placeholder-slate-400 outline-none focus:ring-2 focus:ring-blue-500"
                                        />
                                    </div>

                                    <div className="grid grid-cols-2 gap-4">
                                        <div>
                                            <label className="block text-xs font-semibold uppercase tracking-wider text-slate-500 mb-1.5">
                                                Texto do botão
                                            </label>
                                            <input
                                                type="text"
                                                required
                                                maxLength={50}
                                                value={form.buttonText}
                                                onChange={(e) => updateField('buttonText', e.target.value)}
                                                placeholder="Ver ofertas"
                                                className="w-full bg-slate-50 border border-slate-200 rounded-lg px-4 py-2.5 text-sm text-slate-800 placeholder-slate-400 outline-none focus:ring-2 focus:ring-blue-500"
                                            />
                                        </div>

                                        <div>
                                            <label className="block text-xs font-semibold uppercase tracking-wider text-slate-500 mb-1.5">
                                                Ordem de exibição
                                            </label>
                                            <input
                                                type="number"
                                                required
                                                min={1}
                                                step={1}
                                                value={form.displayOrder}
                                                onChange={(e) => updateField('displayOrder', e.target.value)}
                                                className="w-full bg-slate-50 border border-slate-200 rounded-lg px-4 py-2.5 text-sm text-slate-800 placeholder-slate-400 outline-none focus:ring-2 focus:ring-blue-500"
                                            />
                                        </div>
                                    </div>

                                    <div>
                                        <label className="block text-xs font-semibold uppercase tracking-wider text-slate-500 mb-1.5">
                                            Link do botão
                                        </label>
                                        <input
                                            type="text"
                                            required
                                            maxLength={250}
                                            value={form.buttonLink}
                                            onChange={(e) => updateField('buttonLink', e.target.value)}
                                            placeholder="/produtos/promocoes"
                                            className="w-full bg-slate-50 border border-slate-200 rounded-lg px-4 py-2.5 text-sm text-slate-800 placeholder-slate-400 outline-none focus:ring-2 focus:ring-blue-500"
                                        />
                                    </div>

                                    <div>
                                        <label className="block text-xs font-semibold uppercase tracking-wider text-slate-500 mb-1.5">
                                            Cor de fundo
                                        </label>
                                        <div className="flex items-center gap-2">
                                            <input
                                                type="color"
                                                value={HEX_COLOR_REGEX.test(form.backgroundColor) ? form.backgroundColor : '#94a3b8'}
                                                onChange={(e) => updateField('backgroundColor', e.target.value)}
                                                className="w-11 h-11 rounded-lg border border-slate-200 cursor-pointer shrink-0 bg-white p-1"
                                            />
                                            <input
                                                type="text"
                                                required
                                                maxLength={30}
                                                value={form.backgroundColor}
                                                onChange={(e) => updateField('backgroundColor', e.target.value)}
                                                placeholder="#f95d00"
                                                className="flex-1 bg-slate-50 border border-slate-200 rounded-lg px-4 py-2.5 text-sm text-slate-800 placeholder-slate-400 outline-none focus:ring-2 focus:ring-blue-500"
                                            />
                                        </div>
                                    </div>

                                    {editingBanner && (
                                        <label className="flex items-center gap-2.5 pt-1 cursor-pointer select-none">
                                            <input
                                                type="checkbox"
                                                checked={form.isActive}
                                                onChange={(e) => updateField('isActive', e.target.checked)}
                                                className="w-4 h-4 rounded border-slate-300 text-blue-600 focus:ring-blue-500"
                                            />
                                            <span className="text-sm text-slate-700">Banner ativo na loja</span>
                                        </label>
                                    )}
                                </div>

                                {/* Preview ao vivo */}
                                <div>
                                    <p className="text-xs font-semibold uppercase tracking-wider text-slate-500 mb-1.5">
                                        Pré-visualização
                                    </p>
                                    <BannerPreview form={form} />
                                    <p className="text-[11px] text-slate-400 mt-2">
                                        É assim que o banner vai aparecer na home da loja.
                                    </p>
                                </div>
                            </div>

                            <div className="flex items-center justify-end gap-3 pt-6 mt-2 border-t border-slate-100">
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
                                    {editingBanner ? 'Salvar alterações' : 'Criar banner'}
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            )}

            {/* Confirmação de exclusão */}
            {bannerToDelete && (
                <div className="fixed inset-0 bg-slate-900/50 flex items-center justify-center px-4 z-50">
                    <div className="bg-white rounded-2xl shadow-xl w-full max-w-sm p-6">
                        <div className="w-11 h-11 rounded-full bg-red-100 flex items-center justify-center mb-4">
                            <AlertTriangle size={20} className="text-red-600" />
                        </div>
                        <h3 className="font-bold text-slate-900 mb-1.5">Excluir banner?</h3>
                        <p className="text-sm text-slate-500 mb-4">
                            Tem certeza que deseja excluir{' '}
                            <span className="font-semibold text-slate-700">{bannerToDelete.title}</span>? Esta ação
                            não pode ser desfeita.
                        </p>

                        {deleteError && (
                            <div className="mb-4 bg-red-50 border border-red-200 text-red-700 text-xs px-3 py-2.5 rounded-lg">
                                {deleteError}
                            </div>
                        )}

                        <div className="flex items-center justify-end gap-3">
                            <button
                                onClick={() => setBannerToDelete(null)}
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

export default StoreBanners;
