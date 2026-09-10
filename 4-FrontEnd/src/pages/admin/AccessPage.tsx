import { useCallback, useEffect, useState } from 'react';
import { BackofficeLayout } from '../../layouts/BackofficeLayout';
import { post, request } from '../../services/http';
import { useSession } from '../../services/sessionContext';
type Profile = { id: number; name: string; isActive: boolean; permissions: string[] };
type Access = { available: string[]; profiles: Profile[] };
const names: Record<string, string> = { catalog: 'Catálogo', orders: 'Pedidos', customers: 'Clientes', finance: 'Financeiro', settings: 'Configuração', support: 'Atendimento', moderation: 'Avaliações', promotions: 'Promoções', reports: 'Relatórios', stock: 'Estoque' };
function label(permission: string) {
    const [module, action] = permission.split('.');
    return `${names[module] || module}: ${action === 'read' ? 'consultar' : action === 'refund' ? 'estornar' : 'alterar'}`;
}
export function AccessPage() {
    const { user } = useSession();
    const [data, setData] = useState<Access>({ available: [], profiles: [] });
    const [selected, setSelected] = useState<Profile | null>(null);
    const [permissions, setPermissions] = useState<string[]>([]);
    const [error, setError] = useState(''); const [message, setMessage] = useState(''); const [busy, setBusy] = useState(false);
    const [users, setUsers] = useState<{id: number; username: string; isActive: boolean}[]>([]);
    const load = useCallback(async () => setData(await request<Access>('/v1/access')), []);
    useEffect(() => { void request<Access>('/v1/access').then(setData).catch(e => setError(e.message)); }, []);
    useEffect(() => { if (user) void request<{id: number; username: string; isActive: boolean}[]>(`/v1/tenants/${user.tenantId}/backoffice/users`).then(setUsers).catch(e => setError(e.message)); }, [user]);
    return <BackofficeLayout><div className="commerce-admin access-page"><h1>Perfis e permissões</h1>
        <p>As permissões se somam quando o usuário possui mais de um perfil ativo. Alterações valem na próxima requisição. O administrador geral mantém acesso integral.</p>
        <form onSubmit={async e => {
            e.preventDefault(); const form = e.currentTarget; const name = String(new FormData(form).get('name')).trim(); setBusy(true); setError('');
            try { await post(`/v1/tenants/${user!.tenantId}/backoffice/profiles`, { tenantId: user!.tenantId, name }); form.reset(); await load(); setMessage('Perfil criado sem permissões. Selecione-o para configurar.'); }
            catch (e) { setError((e as Error).message); } finally { setBusy(false); }
        }}><label>Nome do novo perfil<input name="name" maxLength={100} required /></label><button disabled={busy}>Criar perfil</button></form>
        <label>Perfil<select value={selected?.id ?? ''} disabled={busy} onChange={e => {
            const profile = data.profiles.find(p => p.id === Number(e.target.value)) ?? null;
            setSelected(profile); setPermissions(profile?.permissions ?? []); setMessage(''); setError('');
        }}><option value="">Selecione</option>{data.profiles.map(p => <option key={p.id} value={p.id}>{p.name}{p.isActive ? '' : ' (inativo)'}</option>)}</select></label>
        {selected && (selected.name === 'Administrador Geral' ? <p>Este perfil possui acesso integral.</p> : <form onSubmit={async e => {
            e.preventDefault(); setBusy(true); setError(''); setMessage('');
            try {
                await request('/v1/access/profiles/' + selected.id, { method: 'PUT', body: JSON.stringify({ expected: selected.permissions, permissions }) });
                await load(); setSelected(null); setMessage('Permissões salvas.');
            } catch (e) { setError((e as Error).message); }
            finally { setBusy(false); }
        }}><fieldset disabled={busy}><legend>Permissões de {selected.name}</legend>
            <div className="permissions-grid">{data.available.map(permission => <label key={permission}><input type="checkbox" checked={permissions.includes(permission)} onChange={e => setPermissions(old => e.target.checked ? [...old, permission] : old.filter(p => p !== permission))} />{label(permission)}</label>)}</div>
            <p>Para alterar um módulo, habilite também sua consulta. Importações e criação de produtos exigem permissão de estoque.</p>
            <button>Salvar permissões</button>
        </fieldset></form>)}
        {selected && selected.name !== 'Administrador Geral' && <form onSubmit={async e => {
            e.preventDefault(); const employeeUserId = Number(new FormData(e.currentTarget).get('userId')); setBusy(true); setError(''); setMessage('');
            try { await post(`/v1/tenants/${user!.tenantId}/backoffice/profile-users`, { tenantId: user!.tenantId, profileId: selected.id, employeeUserId }); setMessage('Perfil vinculado ao usuário.'); }
            catch (e) { setError((e as Error).message); } finally { setBusy(false); }
        }}><h2>Vincular usuário existente</h2><label>Usuário<select name="userId" required><option value="">Selecione</option>{users.filter(u => u.isActive).map(u => <option key={u.id} value={u.id}>{u.username}</option>)}</select></label><button disabled={busy}>Vincular a {selected.name}</button></form>}
        {error && <p role="alert">{error} <button onClick={() => { setSelected(null); void load().catch(e => setError(e.message)); }}>Atualizar perfis</button></p>}
        {message && <p role="status">{message}</p>}
    </div></BackofficeLayout>;
}
