import { useCallback, useEffect, useState } from 'react';
import { post, request, resetCsrf } from '../services/http';
import { useSession } from '../services/sessionContext';

type SessionEntry = { id: string; expiresAt: string; current: boolean };
export function AccountSecurityPanel() {
    const { refresh } = useSession();
    const [sessions, setSessions] = useState<SessionEntry[]>([]);
    const [error, setError] = useState('');
    const [message, setMessage] = useState('');
    const [busy, setBusy] = useState(false);
    const load = useCallback(async () => setSessions(await request<SessionEntry[]>('/v1/session/security/sessions')), []);
    useEffect(() => { void request<SessionEntry[]>('/v1/session/security/sessions').then(setSessions).catch(e => setError(e.message)); }, []);
    async function revoke(sessionId: string | null) {
        setBusy(true); setError(''); setMessage('');
        try {
            await post('/v1/session/security/sessions/revoke', { sessionId });
            await load(); setMessage('Sessões encerradas.');
        } catch (e) { setError((e as Error).message); }
        finally { setBusy(false); }
    }
    return <section aria-label="Segurança da conta">
        <h2>Segurança da conta</h2>
        <h3>Sessões ativas</h3>
        <p>Os acessos duram até oito horas. Encerre os que você não deseja manter.</p>
        <button disabled={busy} onClick={() => void revoke(null)}>Encerrar todas as outras sessões</button>
        <ul>{sessions.map((session, index) => <li key={session.id}>
            {session.current ? 'Esta sessão' : `Outra sessão ${index + 1}`} · Expira em {new Date(session.expiresAt + (session.expiresAt.endsWith('Z') ? '' : 'Z')).toLocaleString('pt-BR')}
            {!session.current && <button disabled={busy} onClick={() => void revoke(session.id)}>Encerrar sessão</button>}
        </li>)}</ul>
        <h3>Alterar senha</h3>
        <p>Após a alteração, todos os acessos serão encerrados. Entre novamente com a nova senha.</p>
        <form onSubmit={async e => {
            e.preventDefault(); const form = e.currentTarget; const data = new FormData(form);
            setError(''); setMessage('');
            if (data.get('newPassword') !== data.get('confirmation')) { setError('A confirmação da senha não confere.'); return; }
            setBusy(true);
            try {
                await post('/v1/session/security/password', { currentPassword: data.get('currentPassword'), newPassword: data.get('newPassword') });
                form.reset(); resetCsrf(); await refresh();
            } catch (e) { setError((e as Error).message); }
            finally { setBusy(false); }
        }}>
            <label>Senha atual<input name="currentPassword" type="password" autoComplete="current-password" maxLength={72} required /></label>
            <label>Nova senha<input name="newPassword" type="password" autoComplete="new-password" minLength={12} maxLength={72} required /></label>
            <label>Confirme a nova senha<input name="confirmation" type="password" autoComplete="new-password" minLength={12} maxLength={72} required /></label>
            <button disabled={busy}>Alterar senha e encerrar acessos</button>
        </form>
        {error && <p role="alert">{error}</p>}{message && <p role="status">{message}</p>}
    </section>;
}
