import { useEffect, useMemo, useState, type FormEvent } from 'react';
import { BackofficeLayout } from '../../layouts/BackofficeLayout';
import {
    CustomerService,
    type CustomerSummary,
    type CustomerDetail,
    type CustomerType,
    type AddressType,
} from '../../services/api';
import {
    ChevronRight,
    Plus,
    Search,
    Pencil,
    Users,
    Building2,
    UserCheck,
    Mail,
    MapPin,
    X,
    AlertTriangle,
    CheckCircle2,
    XCircle,
    ChevronLeft,
} from 'lucide-react';

// --- Helpers ------------------------------------------------------------

const PAGE_SIZE = 10;

const TYPE_LABEL: Record<CustomerType, string> = {
    Guest: 'Visitante',
    Lead: 'Lead',
    B2C: 'Pessoa Física',
    B2B: 'Pessoa Jurídica',
};

const TYPE_BADGE: Record<CustomerType, string> = {
    Guest: 'bg-slate-100 text-slate-500',
    Lead: 'bg-amber-100 text-amber-700',
    B2C: 'bg-blue-100 text-blue-700',
    B2B: 'bg-purple-100 text-purple-700',
};

function TypeBadge({ type }: { type: CustomerType }) {
    return (
        <span className={`inline-flex items-center text-xs font-semibold px-2.5 py-1 rounded-full ${TYPE_BADGE[type]}`}>
            {TYPE_LABEL[type]}
        </span>
    );
}

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

type DetailTab = 'profile' | 'addresses';

interface AddressFormState {
    id: number | null;
    addressTypeId: AddressType;
    street: string;
    number: string;
    city: string;
    state: string;
    zipCode: string;
    isDefault: boolean;
}

const EMPTY_ADDRESS_FORM: AddressFormState = {
    id: null,
    addressTypeId: 'Shipping',
    street: '',
    number: '',
    city: '',
    state: '',
    zipCode: '',
    isDefault: false,
};

// --- Página ---------------------------------------------------------------

export function Customers() {
    const [customers, setCustomers] = useState<CustomerSummary[]>([]);
    const [totalCount, setTotalCount] = useState(0);
    const [page, setPage] = useState(1);
    const [loading, setLoading] = useState(true);
    const [loadError, setLoadError] = useState('');

    const [searchTerm, setSearchTerm] = useState('');
    const [searchInput, setSearchInput] = useState('');

    const [selectedId, setSelectedId] = useState<number | null>(null);
    const [detail, setDetail] = useState<CustomerDetail | null>(null);
    const [detailLoading, setDetailLoading] = useState(false);
    const [detailError, setDetailError] = useState('');
    const [detailTab, setDetailTab] = useState<DetailTab>('profile');

    // Formulários de perfil
    const [profileType, setProfileType] = useState<'B2C' | 'B2B'>('B2C');
    const [fullName, setFullName] = useState('');
    const [birthDate, setBirthDate] = useState('');
    const [taxId, setTaxId] = useState('');
    const [companyName, setCompanyName] = useState('');
    const [businessTaxId, setBusinessTaxId] = useState('');
    const [stateTaxId, setStateTaxId] = useState('');
    const [email, setEmail] = useState('');
    const [savingProfile, setSavingProfile] = useState(false);
    const [profileError, setProfileError] = useState('');
    const [savingEmail, setSavingEmail] = useState(false);
    const [emailError, setEmailError] = useState('');

    // Endereços
    const [isAddressModalOpen, setIsAddressModalOpen] = useState(false);
    const [addressForm, setAddressForm] = useState<AddressFormState>(EMPTY_ADDRESS_FORM);
    const [savingAddress, setSavingAddress] = useState(false);
    const [addressError, setAddressError] = useState('');

    async function loadList(targetPage = page, term = searchTerm) {
        setLoading(true);
        setLoadError('');
        try {
            const result = await CustomerService.getAll(targetPage, PAGE_SIZE, term || undefined);
            setCustomers(result.items);
            setTotalCount(result.totalCount);
        } catch (error) {
            setLoadError(error instanceof Error ? error.message : 'Não foi possível carregar os clientes.');
        } finally {
            setLoading(false);
        }
    }

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
        const b2c = customers.filter((c) => c.customerType === 'B2C').length;
        const b2b = customers.filter((c) => c.customerType === 'B2B').length;
        const active = customers.filter((c) => c.isActive).length;
        return { total: totalCount, b2c, b2b, active };
    }, [customers, totalCount]);

    async function openDetail(customerId: number) {
        setSelectedId(customerId);
        setDetail(null);
        setDetailError('');
        setDetailTab('profile');
        setDetailLoading(true);
        try {
            const data = await CustomerService.getById(customerId);
            setDetail(data);
            setEmail(data.email ?? '');
            if (data.customerTypeId === 'B2B') {
                setProfileType('B2B');
                setCompanyName(data.companyName ?? '');
            } else {
                setProfileType('B2C');
                setFullName(data.fullName ?? '');
            }
        } catch (error) {
            setDetailError(error instanceof Error ? error.message : 'Não foi possível carregar o cliente.');
        } finally {
            setDetailLoading(false);
        }
    }

    function closeDetail() {
        setSelectedId(null);
        setDetail(null);
        setProfileError('');
        setEmailError('');
    }

    async function refreshDetail() {
        if (selectedId == null) return;
        const data = await CustomerService.getById(selectedId);
        setDetail(data);
    }

    async function handleSaveProfile(e: FormEvent) {
        e.preventDefault();
        if (!detail) return;
        setProfileError('');
        setSavingProfile(true);
        try {
            if (detail.customerTypeId === 'Guest' || detail.customerTypeId === 'Lead') {
                // Ainda não convertido: registra como B2C ou B2B pela primeira vez.
                if (profileType === 'B2C') {
                    if (!taxId.trim() || taxId.trim().length !== 11) {
                        setProfileError('O CPF deve conter exatamente 11 dígitos.');
                        setSavingProfile(false);
                        return;
                    }
                    if (!fullName.trim()) {
                        setProfileError('Informe o nome completo.');
                        setSavingProfile(false);
                        return;
                    }
                    await CustomerService.registerB2C(detail.id, {
                        taxId: taxId.trim(),
                        fullName: fullName.trim(),
                        birthDate: birthDate || null,
                    });
                } else {
                    if (!businessTaxId.trim() || businessTaxId.trim().length !== 14) {
                        setProfileError('O CNPJ deve conter exatamente 14 dígitos.');
                        setSavingProfile(false);
                        return;
                    }
                    if (!companyName.trim()) {
                        setProfileError('Informe a razão social.');
                        setSavingProfile(false);
                        return;
                    }
                    await CustomerService.registerB2B(detail.id, {
                        businessTaxId: businessTaxId.trim(),
                        companyName: companyName.trim(),
                        stateTaxId: stateTaxId.trim() || null,
                    });
                }
            } else if (detail.customerTypeId === 'B2C') {
                if (!fullName.trim()) {
                    setProfileError('Informe o nome completo.');
                    setSavingProfile(false);
                    return;
                }
                await CustomerService.updateB2CProfile(detail.id, {
                    fullName: fullName.trim(),
                    birthDate: birthDate || null,
                });
            } else {
                if (!companyName.trim()) {
                    setProfileError('Informe a razão social.');
                    setSavingProfile(false);
                    return;
                }
                await CustomerService.updateB2BProfile(detail.id, {
                    companyName: companyName.trim(),
                    stateTaxId: stateTaxId.trim() || null,
                });
            }
            await refreshDetail();
            await loadList();
        } catch (error) {
            setProfileError(error instanceof Error ? error.message : 'Não foi possível salvar o perfil.');
        } finally {
            setSavingProfile(false);
        }
    }

    async function handleSaveEmail(e: FormEvent) {
        e.preventDefault();
        if (!detail) return;
        setEmailError('');
        if (!email.trim()) {
            setEmailError('Informe o e-mail.');
            return;
        }
        setSavingEmail(true);
        try {
            if (detail.customerTypeId === 'Guest') {
                await CustomerService.registerLead(detail.id, { email: email.trim() });
            } else {
                await CustomerService.changeEmail(detail.id, { newEmail: email.trim() });
            }
            await refreshDetail();
            await loadList();
        } catch (error) {
            setEmailError(error instanceof Error ? error.message : 'Não foi possível alterar o e-mail.');
        } finally {
            setSavingEmail(false);
        }
    }

    function openAddAddress() {
        setAddressForm(EMPTY_ADDRESS_FORM);
        setAddressError('');
        setIsAddressModalOpen(true);
    }

    function openEditAddress(address: CustomerDetail['addresses'][number]) {
        setAddressForm({
            id: address.id,
            addressTypeId: 'Shipping',
            street: address.street,
            number: address.number,
            city: address.city,
            state: address.state,
            zipCode: address.zipCode,
            isDefault: address.isDefault,
        });
        setAddressError('');
        setIsAddressModalOpen(true);
    }

    async function handleSaveAddress(e: FormEvent) {
        e.preventDefault();
        if (!detail) return;
        setAddressError('');

        const zip = addressForm.zipCode.replace(/\D/g, '');
        if (zip.length !== 8) {
            setAddressError('O CEP deve conter 8 dígitos.');
            return;
        }
        if (addressForm.state.trim().length !== 2) {
            setAddressError('O Estado deve ter 2 caracteres (UF).');
            return;
        }
        if (!addressForm.street.trim()) {
            setAddressError('Informe a rua.');
            return;
        }
        if (!addressForm.number.trim()) {
            setAddressError('Informe o número.');
            return;
        }

        setSavingAddress(true);
        try {
            const payload = {
                addressTypeId: addressForm.addressTypeId,
                street: addressForm.street.trim(),
                number: addressForm.number.trim(),
                city: addressForm.city.trim(),
                state: addressForm.state.trim().toUpperCase(),
                zipCode: zip,
                isDefault: addressForm.isDefault,
            };
            if (addressForm.id != null) {
                await CustomerService.updateAddress(detail.id, addressForm.id, payload);
            } else {
                await CustomerService.addAddress(detail.id, payload);
            }
            setIsAddressModalOpen(false);
            await refreshDetail();
        } catch (error) {
            setAddressError(error instanceof Error ? error.message : 'Não foi possível salvar o endereço.');
        } finally {
            setSavingAddress(false);
        }
    }

    return (
        <BackofficeLayout>
            {/* Breadcrumb */}
            <div className="text-sm text-slate-500 mb-2 flex items-center gap-1.5">
                <span>Início</span>
                <ChevronRight size={14} />
                <span className="text-slate-700 font-medium">Clientes</span>
            </div>

            <div className="flex items-start justify-between mb-6 gap-4 flex-wrap">
                <div>
                    <h1 className="text-2xl font-bold text-slate-900">Clientes</h1>
                    <p className="text-sm text-slate-500 mt-1">
                        Consulte e edite o cadastro de clientes (visitantes, leads, pessoa física e pessoa jurídica)
                    </p>
                </div>
            </div>

            {/* Stat cards */}
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
                <div className="bg-white rounded-xl border border-slate-200 p-5 shadow-sm">
                    <div className="flex items-center gap-3 mb-3">
                        <div className="w-10 h-10 rounded-lg flex items-center justify-center bg-blue-100">
                            <Users size={20} className="text-blue-600" />
                        </div>
                        <p className="text-sm text-slate-500">Total de Clientes</p>
                    </div>
                    <p className="text-2xl font-bold text-slate-900">{stats.total}</p>
                </div>

                <div className="bg-white rounded-xl border border-slate-200 p-5 shadow-sm">
                    <div className="flex items-center gap-3 mb-3">
                        <div className="w-10 h-10 rounded-lg flex items-center justify-center bg-blue-100">
                            <UserCheck size={20} className="text-blue-600" />
                        </div>
                        <p className="text-sm text-slate-500">Pessoa Física (página)</p>
                    </div>
                    <p className="text-2xl font-bold text-slate-900">{stats.b2c}</p>
                </div>

                <div className="bg-white rounded-xl border border-slate-200 p-5 shadow-sm">
                    <div className="flex items-center gap-3 mb-3">
                        <div className="w-10 h-10 rounded-lg flex items-center justify-center bg-purple-100">
                            <Building2 size={20} className="text-purple-600" />
                        </div>
                        <p className="text-sm text-slate-500">Pessoa Jurídica (página)</p>
                    </div>
                    <p className="text-2xl font-bold text-slate-900">{stats.b2b}</p>
                </div>

                <div className="bg-white rounded-xl border border-slate-200 p-5 shadow-sm">
                    <div className="flex items-center gap-3 mb-3">
                        <div className="w-10 h-10 rounded-lg flex items-center justify-center bg-emerald-100">
                            <CheckCircle2 size={20} className="text-emerald-600" />
                        </div>
                        <p className="text-sm text-slate-500">Ativos (página)</p>
                    </div>
                    <p className="text-2xl font-bold text-slate-900">{stats.active}</p>
                </div>
            </div>

            {/* Lista */}
            <div className="bg-white rounded-xl border border-slate-200 shadow-sm">
                <div className="p-5 border-b border-slate-100 flex flex-wrap items-center justify-between gap-3">
                    <h2 className="font-bold text-slate-900">Lista de Clientes</h2>

                    <div className="relative">
                        <Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
                        <input
                            type="text"
                            value={searchInput}
                            onChange={(e) => setSearchInput(e.target.value)}
                            placeholder="Buscar por e-mail..."
                            className="bg-slate-50 border border-slate-200 rounded-lg pl-9 pr-4 py-2 text-sm text-slate-700 placeholder-slate-400 outline-none focus:ring-2 focus:ring-blue-500 w-64"
                        />
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
                                    <th className="py-3 px-5 font-medium">Cliente</th>
                                    <th className="py-3 px-5 font-medium">Tipo</th>
                                    <th className="py-3 px-5 font-medium">Cadastrado em</th>
                                    <th className="py-3 px-5 font-medium">Status</th>
                                    <th className="py-3 px-5 font-medium text-right">Ações</th>
                                </tr>
                            </thead>
                            <tbody>
                                {loading && (
                                    <tr>
                                        <td colSpan={5} className="py-10 text-center text-slate-400">
                                            Carregando clientes...
                                        </td>
                                    </tr>
                                )}

                                {!loading && customers.length === 0 && (
                                    <tr>
                                        <td colSpan={5} className="py-10 text-center text-slate-400">
                                            Nenhum cliente encontrado.
                                        </td>
                                    </tr>
                                )}

                                {!loading &&
                                    customers.map((customer) => (
                                        <tr key={customer.id} className="border-t border-slate-100 hover:bg-slate-50/60">
                                            <td className="py-3 px-5">
                                                <div className="flex items-center gap-3">
                                                    <div className="w-9 h-9 rounded-lg bg-blue-50 flex items-center justify-center shrink-0">
                                                        <Mail size={16} className="text-blue-500" />
                                                    </div>
                                                    <div>
                                                        <p className="font-semibold text-slate-800">
                                                            {customer.email ?? `Cliente #${customer.id}`}
                                                        </p>
                                                        <p className="text-xs text-slate-400">#{customer.id}</p>
                                                    </div>
                                                </div>
                                            </td>
                                            <td className="py-3 px-5">
                                                <TypeBadge type={customer.customerType} />
                                            </td>
                                            <td className="py-3 px-5 text-slate-500">
                                                {new Date(customer.createdAt).toLocaleDateString('pt-BR')}
                                            </td>
                                            <td className="py-3 px-5">
                                                <ActiveBadge isActive={customer.isActive} />
                                            </td>
                                            <td className="py-3 px-5">
                                                <div className="flex items-center justify-end gap-2">
                                                    <button
                                                        onClick={() => openDetail(customer.id)}
                                                        title="Ver / editar cliente"
                                                        className="w-8 h-8 rounded-lg flex items-center justify-center text-blue-600 bg-blue-50 hover:bg-blue-100 transition-colors"
                                                    >
                                                        <Pencil size={14} />
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
                            Página {page} de {totalPages} &middot; {totalCount} clientes
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

            {/* Drawer de detalhe/edição */}
            {selectedId != null && (
                <div className="fixed inset-0 bg-slate-900/50 flex items-center justify-center px-4 z-50">
                    <div className="bg-white rounded-2xl shadow-xl w-full max-w-lg max-h-[90vh] flex flex-col">
                        <div className="flex items-center justify-between px-6 py-4 border-b border-slate-100">
                            <h3 className="font-bold text-slate-900">
                                {detail?.email ?? `Cliente #${selectedId}`}
                            </h3>
                            <button
                                onClick={closeDetail}
                                className="w-8 h-8 rounded-lg flex items-center justify-center text-slate-400 hover:bg-slate-100"
                                aria-label="Fechar"
                            >
                                <X size={18} />
                            </button>
                        </div>

                        {detailLoading && <p className="px-6 py-10 text-center text-slate-400">Carregando...</p>}

                        {detailError && (
                            <div className="mx-6 mt-4 flex items-center gap-2 bg-red-50 border border-red-200 text-red-700 text-sm px-4 py-3 rounded-lg">
                                <AlertTriangle size={16} /> {detailError}
                            </div>
                        )}

                        {detail && !detailLoading && (
                            <>
                                <div className="px-6 pt-4 flex items-center gap-2 border-b border-slate-100">
                                    <button
                                        onClick={() => setDetailTab('profile')}
                                        className={`px-3 py-2 text-sm font-semibold border-b-2 -mb-px transition-colors ${
                                            detailTab === 'profile'
                                                ? 'border-blue-600 text-blue-600'
                                                : 'border-transparent text-slate-500 hover:text-slate-700'
                                        }`}
                                    >
                                        Perfil
                                    </button>
                                    <button
                                        onClick={() => setDetailTab('addresses')}
                                        className={`px-3 py-2 text-sm font-semibold border-b-2 -mb-px transition-colors ${
                                            detailTab === 'addresses'
                                                ? 'border-blue-600 text-blue-600'
                                                : 'border-transparent text-slate-500 hover:text-slate-700'
                                        }`}
                                    >
                                        Endereços ({detail.addresses.length})
                                    </button>
                                </div>

                                <div className="px-6 py-5 overflow-y-auto space-y-6">
                                    {detailTab === 'profile' && (
                                        <>
                                            {/* E-mail */}
                                            <form onSubmit={handleSaveEmail} className="space-y-2">
                                                <label className="block text-xs font-semibold uppercase tracking-wider text-slate-500">
                                                    E-mail
                                                </label>
                                                <div className="flex gap-2">
                                                    <input
                                                        type="email"
                                                        value={email}
                                                        onChange={(e) => setEmail(e.target.value)}
                                                        placeholder="cliente@email.com"
                                                        className="flex-1 bg-slate-50 border border-slate-200 rounded-lg px-4 py-2.5 text-sm text-slate-800 placeholder-slate-400 outline-none focus:ring-2 focus:ring-blue-500"
                                                    />
                                                    <button
                                                        type="submit"
                                                        disabled={savingEmail}
                                                        className="px-4 py-2.5 rounded-lg text-sm font-semibold bg-slate-800 text-white hover:bg-slate-900 disabled:opacity-60"
                                                    >
                                                        Salvar
                                                    </button>
                                                </div>
                                                {emailError && <p className="text-xs text-red-600">{emailError}</p>}
                                            </form>

                                            <div className="border-t border-slate-100 pt-5">
                                                {/* Tipo de conta / dados do perfil */}
                                                {(detail.customerTypeId === 'Guest' || detail.customerTypeId === 'Lead') && (
                                                    <div className="flex items-center gap-2 mb-4">
                                                        <button
                                                            type="button"
                                                            onClick={() => setProfileType('B2C')}
                                                            className={`px-3 py-1.5 rounded-lg text-xs font-semibold ${
                                                                profileType === 'B2C'
                                                                    ? 'bg-blue-600 text-white'
                                                                    : 'bg-slate-100 text-slate-600'
                                                            }`}
                                                        >
                                                            Pessoa Física
                                                        </button>
                                                        <button
                                                            type="button"
                                                            onClick={() => setProfileType('B2B')}
                                                            className={`px-3 py-1.5 rounded-lg text-xs font-semibold ${
                                                                profileType === 'B2B'
                                                                    ? 'bg-purple-600 text-white'
                                                                    : 'bg-slate-100 text-slate-600'
                                                            }`}
                                                        >
                                                            Pessoa Jurídica
                                                        </button>
                                                    </div>
                                                )}

                                                <form onSubmit={handleSaveProfile} className="space-y-3">
                                                    {profileError && (
                                                        <div className="bg-red-50 border border-red-200 text-red-700 text-xs px-3 py-2.5 rounded-lg">
                                                            {profileError}
                                                        </div>
                                                    )}

                                                    {(detail.customerTypeId === 'B2C' ||
                                                        ((detail.customerTypeId === 'Guest' || detail.customerTypeId === 'Lead') &&
                                                            profileType === 'B2C')) && (
                                                        <>
                                                            {(detail.customerTypeId === 'Guest' || detail.customerTypeId === 'Lead') && (
                                                                <div>
                                                                    <label className="block text-xs font-semibold uppercase tracking-wider text-slate-500 mb-1.5">
                                                                        CPF
                                                                    </label>
                                                                    <input
                                                                        type="text"
                                                                        value={taxId}
                                                                        onChange={(e) => setTaxId(e.target.value.replace(/\D/g, ''))}
                                                                        maxLength={11}
                                                                        placeholder="Somente números"
                                                                        className="w-full bg-slate-50 border border-slate-200 rounded-lg px-4 py-2.5 text-sm text-slate-800 placeholder-slate-400 outline-none focus:ring-2 focus:ring-blue-500"
                                                                    />
                                                                </div>
                                                            )}
                                                            <div>
                                                                <label className="block text-xs font-semibold uppercase tracking-wider text-slate-500 mb-1.5">
                                                                    Nome completo
                                                                </label>
                                                                <input
                                                                    type="text"
                                                                    value={fullName}
                                                                    onChange={(e) => setFullName(e.target.value)}
                                                                    maxLength={100}
                                                                    className="w-full bg-slate-50 border border-slate-200 rounded-lg px-4 py-2.5 text-sm text-slate-800 outline-none focus:ring-2 focus:ring-blue-500"
                                                                />
                                                            </div>
                                                            <div>
                                                                <label className="block text-xs font-semibold uppercase tracking-wider text-slate-500 mb-1.5">
                                                                    Data de nascimento
                                                                </label>
                                                                <input
                                                                    type="date"
                                                                    value={birthDate}
                                                                    onChange={(e) => setBirthDate(e.target.value)}
                                                                    className="w-full bg-slate-50 border border-slate-200 rounded-lg px-4 py-2.5 text-sm text-slate-800 outline-none focus:ring-2 focus:ring-blue-500"
                                                                />
                                                            </div>
                                                        </>
                                                    )}

                                                    {(detail.customerTypeId === 'B2B' ||
                                                        ((detail.customerTypeId === 'Guest' || detail.customerTypeId === 'Lead') &&
                                                            profileType === 'B2B')) && (
                                                        <>
                                                            {(detail.customerTypeId === 'Guest' || detail.customerTypeId === 'Lead') && (
                                                                <div>
                                                                    <label className="block text-xs font-semibold uppercase tracking-wider text-slate-500 mb-1.5">
                                                                        CNPJ
                                                                    </label>
                                                                    <input
                                                                        type="text"
                                                                        value={businessTaxId}
                                                                        onChange={(e) => setBusinessTaxId(e.target.value.replace(/\D/g, ''))}
                                                                        maxLength={14}
                                                                        placeholder="Somente números"
                                                                        className="w-full bg-slate-50 border border-slate-200 rounded-lg px-4 py-2.5 text-sm text-slate-800 placeholder-slate-400 outline-none focus:ring-2 focus:ring-blue-500"
                                                                    />
                                                                </div>
                                                            )}
                                                            <div>
                                                                <label className="block text-xs font-semibold uppercase tracking-wider text-slate-500 mb-1.5">
                                                                    Razão social
                                                                </label>
                                                                <input
                                                                    type="text"
                                                                    value={companyName}
                                                                    onChange={(e) => setCompanyName(e.target.value)}
                                                                    maxLength={150}
                                                                    className="w-full bg-slate-50 border border-slate-200 rounded-lg px-4 py-2.5 text-sm text-slate-800 outline-none focus:ring-2 focus:ring-blue-500"
                                                                />
                                                            </div>
                                                            <div>
                                                                <label className="block text-xs font-semibold uppercase tracking-wider text-slate-500 mb-1.5">
                                                                    Inscrição estadual
                                                                </label>
                                                                <input
                                                                    type="text"
                                                                    value={stateTaxId}
                                                                    onChange={(e) => setStateTaxId(e.target.value)}
                                                                    maxLength={15}
                                                                    className="w-full bg-slate-50 border border-slate-200 rounded-lg px-4 py-2.5 text-sm text-slate-800 outline-none focus:ring-2 focus:ring-blue-500"
                                                                />
                                                            </div>
                                                        </>
                                                    )}

                                                    <button
                                                        type="submit"
                                                        disabled={savingProfile}
                                                        className="w-full flex items-center justify-center gap-2 bg-blue-600 hover:bg-blue-700 text-white text-sm font-semibold px-4 py-2.5 rounded-lg shadow-sm transition-colors disabled:opacity-60"
                                                    >
                                                        {savingProfile && (
                                                            <span className="inline-block w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin" />
                                                        )}
                                                        Salvar perfil
                                                    </button>
                                                </form>
                                            </div>
                                        </>
                                    )}

                                    {detailTab === 'addresses' && (
                                        <div className="space-y-3">
                                            <button
                                                onClick={openAddAddress}
                                                className="flex items-center gap-2 text-sm font-semibold text-blue-600 hover:text-blue-700"
                                            >
                                                <Plus size={14} /> Adicionar endereço
                                            </button>

                                            {detail.addresses.length === 0 && (
                                                <p className="text-sm text-slate-400 py-6 text-center">
                                                    Nenhum endereço cadastrado.
                                                </p>
                                            )}

                                            {detail.addresses.map((address) => (
                                                <div
                                                    key={address.id}
                                                    className="border border-slate-200 rounded-lg p-4 flex items-start justify-between gap-3"
                                                >
                                                    <div className="flex items-start gap-3">
                                                        <MapPin size={16} className="text-slate-400 mt-0.5" />
                                                        <div className="text-sm">
                                                            <p className="font-semibold text-slate-800">
                                                                {address.street}, {address.number}
                                                                {address.isDefault && (
                                                                    <span className="ml-2 text-xs font-semibold text-emerald-600">
                                                                        Padrão
                                                                    </span>
                                                                )}
                                                            </p>
                                                            <p className="text-slate-500">
                                                                {address.neighborhood ? `${address.neighborhood}, ` : ''}
                                                                {address.city} - {address.state}, {address.zipCode}
                                                            </p>
                                                        </div>
                                                    </div>
                                                    <button
                                                        onClick={() => openEditAddress(address)}
                                                        className="w-8 h-8 rounded-lg flex items-center justify-center text-blue-600 bg-blue-50 hover:bg-blue-100 transition-colors shrink-0"
                                                    >
                                                        <Pencil size={14} />
                                                    </button>
                                                </div>
                                            ))}
                                        </div>
                                    )}
                                </div>
                            </>
                        )}
                    </div>
                </div>
            )}

            {/* Modal de endereço */}
            {isAddressModalOpen && (
                <div className="fixed inset-0 bg-slate-900/60 flex items-center justify-center px-4 z-[60]">
                    <div className="bg-white rounded-2xl shadow-xl w-full max-w-md">
                        <div className="flex items-center justify-between px-6 py-4 border-b border-slate-100">
                            <h3 className="font-bold text-slate-900">
                                {addressForm.id != null ? 'Editar Endereço' : 'Novo Endereço'}
                            </h3>
                            <button
                                onClick={() => setIsAddressModalOpen(false)}
                                className="w-8 h-8 rounded-lg flex items-center justify-center text-slate-400 hover:bg-slate-100"
                                aria-label="Fechar"
                            >
                                <X size={18} />
                            </button>
                        </div>

                        <form onSubmit={handleSaveAddress} className="px-6 py-5 space-y-3">
                            {addressError && (
                                <div className="bg-red-50 border border-red-200 text-red-700 text-xs px-3 py-2.5 rounded-lg">
                                    {addressError}
                                </div>
                            )}

                            <div>
                                <label className="block text-xs font-semibold uppercase tracking-wider text-slate-500 mb-1.5">
                                    Tipo
                                </label>
                                <select
                                    value={addressForm.addressTypeId}
                                    onChange={(e) =>
                                        setAddressForm((prev) => ({ ...prev, addressTypeId: e.target.value as AddressType }))
                                    }
                                    className="w-full bg-slate-50 border border-slate-200 rounded-lg px-4 py-2.5 text-sm text-slate-800 outline-none focus:ring-2 focus:ring-blue-500"
                                >
                                    <option value="Shipping">Entrega</option>
                                    <option value="Billing">Cobrança</option>
                                </select>
                            </div>

                            <div className="grid grid-cols-3 gap-3">
                                <div className="col-span-2">
                                    <label className="block text-xs font-semibold uppercase tracking-wider text-slate-500 mb-1.5">
                                        Rua
                                    </label>
                                    <input
                                        type="text"
                                        value={addressForm.street}
                                        onChange={(e) => setAddressForm((prev) => ({ ...prev, street: e.target.value }))}
                                        className="w-full bg-slate-50 border border-slate-200 rounded-lg px-4 py-2.5 text-sm text-slate-800 outline-none focus:ring-2 focus:ring-blue-500"
                                    />
                                </div>
                                <div>
                                    <label className="block text-xs font-semibold uppercase tracking-wider text-slate-500 mb-1.5">
                                        Número
                                    </label>
                                    <input
                                        type="text"
                                        value={addressForm.number}
                                        onChange={(e) => setAddressForm((prev) => ({ ...prev, number: e.target.value }))}
                                        className="w-full bg-slate-50 border border-slate-200 rounded-lg px-4 py-2.5 text-sm text-slate-800 outline-none focus:ring-2 focus:ring-blue-500"
                                    />
                                </div>
                            </div>

                            <div className="grid grid-cols-3 gap-3">
                                <div className="col-span-2">
                                    <label className="block text-xs font-semibold uppercase tracking-wider text-slate-500 mb-1.5">
                                        Cidade
                                    </label>
                                    <input
                                        type="text"
                                        value={addressForm.city}
                                        onChange={(e) => setAddressForm((prev) => ({ ...prev, city: e.target.value }))}
                                        className="w-full bg-slate-50 border border-slate-200 rounded-lg px-4 py-2.5 text-sm text-slate-800 outline-none focus:ring-2 focus:ring-blue-500"
                                    />
                                </div>
                                <div>
                                    <label className="block text-xs font-semibold uppercase tracking-wider text-slate-500 mb-1.5">
                                        UF
                                    </label>
                                    <input
                                        type="text"
                                        value={addressForm.state}
                                        onChange={(e) =>
                                            setAddressForm((prev) => ({ ...prev, state: e.target.value.toUpperCase() }))
                                        }
                                        maxLength={2}
                                        className="w-full bg-slate-50 border border-slate-200 rounded-lg px-4 py-2.5 text-sm text-slate-800 outline-none focus:ring-2 focus:ring-blue-500"
                                    />
                                </div>
                            </div>

                            <div>
                                <label className="block text-xs font-semibold uppercase tracking-wider text-slate-500 mb-1.5">
                                    CEP
                                </label>
                                <input
                                    type="text"
                                    value={addressForm.zipCode}
                                    onChange={(e) =>
                                        setAddressForm((prev) => ({ ...prev, zipCode: e.target.value.replace(/\D/g, '') }))
                                    }
                                    maxLength={8}
                                    placeholder="Somente números"
                                    className="w-full bg-slate-50 border border-slate-200 rounded-lg px-4 py-2.5 text-sm text-slate-800 placeholder-slate-400 outline-none focus:ring-2 focus:ring-blue-500"
                                />
                            </div>

                            <label className="flex items-center gap-2 text-sm text-slate-600">
                                <input
                                    type="checkbox"
                                    checked={addressForm.isDefault}
                                    onChange={(e) => setAddressForm((prev) => ({ ...prev, isDefault: e.target.checked }))}
                                    className="rounded border-slate-300"
                                />
                                Definir como endereço padrão
                            </label>

                            <div className="flex items-center justify-end gap-3 pt-2">
                                <button
                                    type="button"
                                    onClick={() => setIsAddressModalOpen(false)}
                                    disabled={savingAddress}
                                    className="px-4 py-2.5 rounded-lg text-sm font-semibold text-slate-600 hover:bg-slate-100 transition-colors disabled:opacity-50"
                                >
                                    Cancelar
                                </button>
                                <button
                                    type="submit"
                                    disabled={savingAddress}
                                    className="flex items-center gap-2 bg-blue-600 hover:bg-blue-700 text-white text-sm font-semibold px-4 py-2.5 rounded-lg shadow-sm transition-colors disabled:opacity-60"
                                >
                                    {savingAddress && (
                                        <span className="inline-block w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin" />
                                    )}
                                    Salvar endereço
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            )}
        </BackofficeLayout>
    );
}

export default Customers;
