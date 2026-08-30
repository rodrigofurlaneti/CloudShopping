// Configurações globais da API
const API_BASE_URL = 'http://localhost:5147/api';
const DEFAULT_TENANT_ID = '1'; // Em produção, isso viria de um AuthContext ou do subdomínio

// Interfaces de Tipagem
export interface Department {
    id: number;
    name: string;
    slug: string;
    isSystemDefault: boolean;
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
        throw new Error(`Erro na requisição: ${response.status} - ${response.statusText}`);
    }

    return response.json();
}

// Serviços organizados por domínio
export const DepartmentService = {
    getAll: () => fetchClient<Department[]>('/Departments'),
};

export const StoreBannerService = {
    getAll: () => fetchClient<StoreBanner[]>('/v1/store-banners'),
};