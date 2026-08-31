// Configurações globais da API
const API_BASE_URL = 'http://localhost:5147/api';
const STATIC_BASE_URL = 'http://localhost:5147'; // wwwroot (app.UseStaticFiles()), sem o prefixo /api
const DEFAULT_TENANT_ID = '1'; // Em produção, isso viria de um AuthContext ou do subdomínio

// Monta a URL pública de um arquivo salvo em wwwroot (ex: imagens de produto),
// a partir do caminho relativo devolvido pela API (ex: "uploads/1/products/45/foto.jpg").
export function resolveStaticUrl(relativePath: string): string {
    return `${STATIC_BASE_URL}/${relativePath.replace(/^\//, '')}`;
}

// Interfaces de Tipagem
export interface Department {
    id: number;
    name: string;
    slug: string;
    isSystemDefault: boolean;
}

export interface CreateDepartmentPayload {
    name: string;
    slug: string;
}

export interface UpdateDepartmentPayload {
    name: string;
    slug: string;
}

export interface StoreBanner {
    id: number;
    title: string;
    subtitle?: string;
    discountPercentage?: string;
    buttonText: string;
    buttonLink: string;
    backgroundColor: string;
    displayOrder: number;
}

// Campos aceitos por POST /v1/store-banners (CreateStoreBannerCommand)
export interface CreateStoreBannerPayload {
    title: string;
    subtitle?: string;
    discountPercentage?: string;
    buttonText: string;
    buttonLink: string;
    backgroundColor: string;
    displayOrder: number;
}

// Campos aceitos por PUT /v1/store-banners/{id} (UpdateStoreBannerCommand),
// sem id/tenantId: o service preenche os dois automaticamente.
export interface UpdateStoreBannerPayload {
    title: string;
    subtitle?: string;
    discountPercentage?: string;
    buttonText: string;
    buttonLink: string;
    backgroundColor: string;
    displayOrder: number;
    isActive: boolean;
}

export interface OrderSector {
    id: number;
    name: string;
    isActive: boolean;
}

// Campos aceitos por POST /v1/order-sectors (CreateOrderSectorCommand)
export interface CreateOrderSectorPayload {
    name: string;
}

// Campo aceito por PUT /v1/order-sectors/{id} (UpdateOrderSectorNameCommand);
// o Id vem da URL, então o corpo só carrega o novo nome.
export interface UpdateOrderSectorPayload {
    newName: string;
}

export interface OrderStatus {
    id: number;
    orderSectorId: number;
    name: string;
    isSystemDefault: boolean;
    isActive: boolean;
}

// Campos aceitos por POST /v1/order-statuses (CreateOrderStatusCommand)
export interface CreateOrderStatusPayload {
    orderSectorId: number;
    name: string;
}

// Campos aceitos por PUT /v1/order-statuses/{id} (UpdateOrderStatusCommand); o Id vem da URL.
export interface UpdateOrderStatusPayload {
    orderSectorId: number;
    name: string;
}

// ---- Customers ----

export type CustomerType = 'Guest' | 'Lead' | 'B2C' | 'B2B';

export interface CustomerSummary {
    id: number;
    email?: string | null;
    customerType: CustomerType;
    createdAt: string;
    isActive: boolean;
}

export interface CustomerAddress {
    id: number;
    street: string;
    number: string;
    neighborhood?: string | null;
    city: string;
    state: string;
    zipCode: string;
    isDefault: boolean;
}

export interface CustomerDetail {
    id: number;
    email?: string | null;
    customerTypeId: CustomerType;
    fullName?: string | null;
    companyName?: string | null;
    addresses: CustomerAddress[];
}

export interface PagedResult<T> {
    items: T[];
    totalCount: number;
    page: number;
    pageSize: number;
    totalPages: number;
}

export type AddressType = 'Shipping' | 'Billing';

export interface RegisterLeadPayload {
    email: string;
}

export interface RegisterB2CPayload {
    taxId: string;
    fullName: string;
    birthDate?: string | null;
}

export interface RegisterB2BPayload {
    businessTaxId: string;
    companyName: string;
    stateTaxId?: string | null;
}

export interface UpdateB2CProfilePayload {
    fullName: string;
    birthDate?: string | null;
}

export interface UpdateB2BProfilePayload {
    companyName: string;
    stateTaxId?: string | null;
}

export interface ChangeEmailPayload {
    newEmail: string;
}

export interface AddCustomerAddressPayload {
    addressTypeId: AddressType;
    street: string;
    number: string;
    city: string;
    state: string;
    zipCode: string;
    isDefault: boolean;
}

export type UpdateCustomerAddressPayload = AddCustomerAddressPayload;

// ---- Products ----

export interface ProductImage {
    id: number;
    fileName: string;
    filePath: string;
    isPrimary: boolean;
    displayOrder: number;
}

export interface ProductSummary {
    id: number;
    departmentId: number;
    sku: string;
    name: string;
    price: number;
    physicalStock: number;
    reservedStock: number;
    availableStock: number;
    hasLocation: boolean;
    primaryImagePath?: string | null;
}

export interface ProductDetail {
    id: number;
    departmentId: number;
    sku: string;
    name: string;
    price: number;
    physicalStock: number;
    reservedStock: number;
    availableStock: number;
    aisle?: string | null;
    rack?: string | null;
    level?: string | null;
    position?: string | null;
    images: ProductImage[];
}

export interface CreateProductPayload {
    departmentId: number;
    sku: string;
    name: string;
    price: number;
    initialStock: number;
    aisle?: string;
    rack?: string;
    level?: string;
    position?: string;
}

export interface UpdateProductDetailsPayload {
    name: string;
    price: number;
}

export interface UpdateProductLocationPayload {
    aisle: string;
    rack: string;
    level: string;
    position: string;
}

export interface AddProductStockPayload {
    quantity: number;
    reason: string;
}

export interface AdjustInventoryPayload {
    newPhysicalQuantity: number;
    reason: string;
}

// Corpo de erro retornado pela API no formato Result Pattern (Error { Code, Message })
interface ApiErrorBody {
    code?: string;
    message?: string;
}

// Wrapper genérico para o fetch (injeta o X-Tenant-Id automaticamente)
async function fetchClient<T>(endpoint: string, options?: RequestInit): Promise<T> {
    const headers = {
        'Content-Type': 'application/json',
        'X-Tenant-Id': DEFAULT_TENANT_ID,
        ...options?.headers,
    };

    const response = await fetch(`${API_BASE_URL}${endpoint}`, {
        ...options,
        headers,
    });

    if (!response.ok) {
        const errorBody: ApiErrorBody | null = await response.json().catch(() => null);
        const message = errorBody?.message || `Erro na requisição: ${response.status} - ${response.statusText}`;
        throw new Error(message);
    }

    // Respostas 204 No Content (PUT/DELETE) não possuem corpo para converter em JSON
    if (response.status === 204) {
        return undefined as T;
    }

    return response.json();
}

// Variante do fetchClient para envio de arquivos (multipart/form-data). Não define
// Content-Type manualmente: o navegador precisa gerar o boundary automaticamente.
async function fetchMultipart<T>(endpoint: string, formData: FormData, method: 'POST' | 'PUT' = 'POST'): Promise<T> {
    const response = await fetch(`${API_BASE_URL}${endpoint}`, {
        method,
        headers: {
            'X-Tenant-Id': DEFAULT_TENANT_ID,
        },
        body: formData,
    });

    if (!response.ok) {
        const errorBody: ApiErrorBody | null = await response.json().catch(() => null);
        const message = errorBody?.message || `Erro na requisição: ${response.status} - ${response.statusText}`;
        throw new Error(message);
    }

    if (response.status === 204) {
        return undefined as T;
    }

    return response.json();
}

// Serviços organizados por domínio
export const DepartmentService = {
    getAll: () => fetchClient<Department[]>('/Departments'),

    create: (payload: CreateDepartmentPayload) =>
        fetchClient<number>('/Departments', {
            method: 'POST',
            body: JSON.stringify(payload),
        }),

    update: (id: number, payload: UpdateDepartmentPayload) =>
        fetchClient<void>(`/Departments/${id}`, {
            method: 'PUT',
            body: JSON.stringify(payload),
        }),

    remove: (id: number) =>
        fetchClient<void>(`/Departments/${id}`, {
            method: 'DELETE',
        }),
};

export const StoreBannerService = {
    getAll: () => fetchClient<StoreBanner[]>('/v1/store-banners'),

    create: (payload: CreateStoreBannerPayload) =>
        fetchClient<{ id: number }>('/v1/store-banners', {
            method: 'POST',
            body: JSON.stringify(payload),
        }),

    // O UpdateStoreBannerCommand exige Id e TenantId no corpo, mas a listagem (GET)
    // não devolve nenhum dos dois — por isso o service injeta o id da URL e o
    // tenant padrão do painel (mesmo usado no header X-Tenant-Id) automaticamente.
    update: (id: number, payload: UpdateStoreBannerPayload) =>
        fetchClient<void>(`/v1/store-banners/${id}`, {
            method: 'PUT',
            body: JSON.stringify({
                id,
                tenantId: Number(DEFAULT_TENANT_ID),
                ...payload,
            }),
        }),

    remove: (id: number) =>
        fetchClient<void>(`/v1/store-banners/${id}`, {
            method: 'DELETE',
        }),
};

export const OrderSectorService = {
    // onlyActive=false (padrão) traz também os setores inativos, para que o
    // painel consiga reativá-los; a rota GET foi adicionada ao controller
    // reaproveitando a query/handler que já existiam na Application.
    getAll: (onlyActive = false) => fetchClient<OrderSector[]>(`/v1/order-sectors?onlyActive=${onlyActive}`),

    create: (payload: CreateOrderSectorPayload) =>
        fetchClient<{ id: number }>('/v1/order-sectors', {
            method: 'POST',
            body: JSON.stringify(payload),
        }),

    update: (id: number, payload: UpdateOrderSectorPayload) =>
        fetchClient<void>(`/v1/order-sectors/${id}`, {
            method: 'PUT',
            body: JSON.stringify(payload),
        }),

    // Não existe endpoint de exclusão: setores são desativados/reativados
    // (soft toggle), nunca removidos.
    toggleStatus: (id: number, activate: boolean) =>
        fetchClient<void>(`/v1/order-sectors/${id}/status?activate=${activate}`, {
            method: 'PATCH',
        }),
};

export const OrderStatusService = {
    // onlyActive=false (padrão) traz também os status inativos, para o painel poder
    // reativá-los. A rota GET e o toggle de status foram adicionados ao controller
    // (só existiam Create/Update); Create/Update devolvem exceções não tratadas em vez
    // de Result em caso de erro, então uma falha de validação pode chegar aqui como um
    // 500 genérico em vez de uma mensagem específica — por isso o formulário valida os
    // mesmos limites do domínio (nome obrigatório, até 50 caracteres) antes de enviar.
    getAll: (onlyActive = false) => fetchClient<OrderStatus[]>(`/v1/order-statuses?onlyActive=${onlyActive}`),

    create: (payload: CreateOrderStatusPayload) =>
        fetchClient<{ id: number }>('/v1/order-statuses', {
            method: 'POST',
            body: JSON.stringify(payload),
        }),

    update: (id: number, payload: UpdateOrderStatusPayload) =>
        fetchClient<void>(`/v1/order-statuses/${id}`, {
            method: 'PUT',
            body: JSON.stringify(payload),
        }),

    toggleStatus: (id: number, activate: boolean) =>
        fetchClient<void>(`/v1/order-statuses/${id}/status?activate=${activate}`, {
            method: 'PATCH',
        }),
};

export const CustomerService = {
    // A rota GET (lista paginada) foi adicionada ao controller reaproveitando a
    // GetPaginatedCustomersQuery/Handler que já existiam na Application mas não
    // estavam expostos por nenhum endpoint.
    getAll: (page = 1, pageSize = 10, searchTerm?: string) =>
        fetchClient<PagedResult<CustomerSummary>>(
            `/v1/customers?page=${page}&pageSize=${pageSize}${searchTerm ? `&searchTerm=${encodeURIComponent(searchTerm)}` : ''}`
        ),

    getById: (id: number) => fetchClient<CustomerDetail>(`/v1/customers/${id}`),

    registerGuest: () =>
        fetchClient<{ id: number }>('/v1/customers/guest', { method: 'POST' }),

    registerLead: (id: number, payload: RegisterLeadPayload) =>
        fetchClient<void>(`/v1/customers/${id}/lead`, {
            method: 'POST',
            body: JSON.stringify(payload),
        }),

    registerB2C: (id: number, payload: RegisterB2CPayload) =>
        fetchClient<void>(`/v1/customers/${id}/register-b2c`, {
            method: 'POST',
            body: JSON.stringify(payload),
        }),

    registerB2B: (id: number, payload: RegisterB2BPayload) =>
        fetchClient<void>(`/v1/customers/${id}/register-b2b`, {
            method: 'POST',
            body: JSON.stringify(payload),
        }),

    changeEmail: (id: number, payload: ChangeEmailPayload) =>
        fetchClient<void>(`/v1/customers/${id}/email`, {
            method: 'PATCH',
            body: JSON.stringify(payload),
        }),

    updateB2CProfile: (id: number, payload: UpdateB2CProfilePayload) =>
        fetchClient<void>(`/v1/customers/${id}/profile/b2c`, {
            method: 'PUT',
            body: JSON.stringify(payload),
        }),

    updateB2BProfile: (id: number, payload: UpdateB2BProfilePayload) =>
        fetchClient<void>(`/v1/customers/${id}/profile/b2b`, {
            method: 'PUT',
            body: JSON.stringify(payload),
        }),

    addAddress: (id: number, payload: AddCustomerAddressPayload) =>
        fetchClient<void>(`/v1/customers/${id}/addresses`, {
            method: 'POST',
            body: JSON.stringify(payload),
        }),

    updateAddress: (id: number, addressId: number, payload: UpdateCustomerAddressPayload) =>
        fetchClient<void>(`/v1/customers/${id}/addresses/${addressId}`, {
            method: 'PUT',
            body: JSON.stringify(payload),
        }),
};

export const ProductService = {
    // Lista paginada — endpoint novo (GetPaginatedProductsQuery), já que o controller
    // original só tinha GetById/GetBySku (sem listagem).
    getAll: (page = 1, pageSize = 12, searchTerm?: string) =>
        fetchClient<PagedResult<ProductSummary>>(
            `/v1/products?page=${page}&pageSize=${pageSize}${searchTerm ? `&searchTerm=${encodeURIComponent(searchTerm)}` : ''}`
        ),

    getById: (id: number) => fetchClient<ProductDetail>(`/v1/products/${id}`),

    getBySku: (sku: string) => fetchClient<ProductDetail>(`/v1/products/sku/${encodeURIComponent(sku)}`),

    create: (payload: CreateProductPayload) =>
        fetchClient<{ id: number }>('/v1/products', {
            method: 'POST',
            body: JSON.stringify(payload),
        }),

    remove: (id: number) =>
        fetchClient<void>(`/v1/products/${id}`, { method: 'DELETE' }),

    updateDetails: (id: number, payload: UpdateProductDetailsPayload) =>
        fetchClient<void>(`/v1/products/${id}`, {
            method: 'PUT',
            body: JSON.stringify(payload),
        }),

    updateLocation: (id: number, payload: UpdateProductLocationPayload) =>
        fetchClient<void>(`/v1/products/${id}/location`, {
            method: 'PUT',
            body: JSON.stringify(payload),
        }),

    addStock: (id: number, payload: AddProductStockPayload) =>
        fetchClient<void>(`/v1/products/${id}/stock/add`, {
            method: 'POST',
            body: JSON.stringify(payload),
        }),

    adjustInventory: (id: number, payload: AdjustInventoryPayload) =>
        fetchClient<void>(`/v1/products/${id}/stock/adjust`, {
            method: 'POST',
            body: JSON.stringify(payload),
        }),

    // multipart/form-data: File + IsPrimary + DisplayOrder (nomes de campo em
    // PascalCase-insensitive são aceitos pelo model binder do ASP.NET Core).
    uploadImage: (id: number, file: File, isPrimary: boolean, displayOrder: number) => {
        const formData = new FormData();
        formData.append('file', file);
        formData.append('isPrimary', String(isPrimary));
        formData.append('displayOrder', String(displayOrder));
        return fetchMultipart<{ path: string }>(`/v1/products/${id}/images`, formData, 'POST');
    },
};
