// Serviço de autenticação / onboarding do Backoffice
const API_BASE_URL = 'http://localhost:5147/api';

export interface RegisterCompanyRequest {
    companyName: string;
    domain?: string;
    adminName: string;
    adminCpf: string;
    adminEmail: string;
    adminPhone?: string;
    adminUsername: string;
    adminPassword: string;
}

export interface RegisterCompanyResponse {
    tenantId: number;
    companyName: string;
    employeeUserId: number;
    username: string;
}

async function postJson<TResponse>(endpoint: string, body: unknown): Promise<TResponse> {
    const response = await fetch(`${API_BASE_URL}${endpoint}`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(body),
    });

    const data = await response.json().catch(() => null);

    if (!response.ok) {
        const message = (data && data.message) || `Erro na requisição: ${response.status} - ${response.statusText}`;
        throw new Error(message);
    }

    return data as TResponse;
}

export const TenantAuthService = {
    // Auto-cadastro público: cria a empresa (Tenant) e já retorna o primeiro
    // usuário administrador criado para ela.
    registerCompany: (payload: RegisterCompanyRequest) =>
        postJson<RegisterCompanyResponse>('/v1/tenants/register', payload),
};
