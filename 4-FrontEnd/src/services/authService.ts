// Serviço de autenticação / onboarding do Backoffice
import { post } from './http';

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

async function postJson<TResponse>(endpoint: string, body: unknown): Promise<TResponse> { return post<TResponse>(endpoint, body); }

export const TenantAuthService = {
    // Auto-cadastro público: cria a empresa (Tenant) e já retorna o primeiro
    // usuário administrador criado para ela.
    registerCompany: (payload: RegisterCompanyRequest) =>
        postJson<RegisterCompanyResponse>('/v1/tenants/register', payload),
};
