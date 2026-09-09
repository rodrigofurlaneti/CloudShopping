import {useState} from 'react';
import {BackofficeLayout} from '../../layouts/BackofficeLayout';
import {useResource} from '../../services/useResource';
import {request} from '../../services/http';
interface Connection {configured:boolean;mode?:string;environment?:string;walletId?:string;webhookPath?:string;productionAllowed:boolean;merchantWalletId?:string;merchantPercent?:string}
export function AsaasSettings(){
    const [version,setVersion]=useState(0);const current=useResource<Connection>('/v1/asaas/connection',version);
    const [mode,setMode]=useState('Direct');const [environment,setEnvironment]=useState('Sandbox');const [apiKey,setApiKey]=useState('');const [token,setToken]=useState('');const [busy,setBusy]=useState(false);const [message,setMessage]=useState('');
    return <BackofficeLayout><main className="commerce-admin"><h1>Pagamentos Asaas</h1>
        <p>Escolha a conta emissora das novas cobranças. Tentativas já criadas mantêm a conta e as regras originais.</p>
        {(message||current.error)&&<p className="notice" role="status">{message||current.error}</p>}
        {current.data?.configured&&<section className="form-card"><h2>Conexão atual</h2><p>{current.data.mode==='Direct'?'Conta própria do lojista':'Plataforma com split'} · {current.data.environment}</p><p>Carteira emissora: {current.data.walletId}</p><label>Caminho do webhook<input readOnly value={current.data.webhookPath||''}/></label><p>Cadastre este caminho junto à URL HTTPS pública da API no Asaas. Use o token de webhook informado abaixo.</p></section>}
        <form className="form-card" onSubmit={async e=>{e.preventDefault();setBusy(true);setMessage('');try{
            await request('/v1/asaas/connection',{method:'PUT',body:JSON.stringify({mode,environment,apiKey:mode==='Direct'?apiKey:null,webhookToken:mode==='Direct'?token:null})});
            setApiKey('');setToken('');setVersion(v=>v+1);setMessage('Conta verificada. Configure os webhooks no Asaas antes de iniciar a homologação.');
        }catch(err){setMessage((err as Error).message);}finally{setBusy(false);}}}>
            <h2>Configurar recebimento</h2><label>Modelo<select value={mode} onChange={e=>setMode(e.target.value)}><option value="Direct">Cada lojista recebe em sua conta</option><option value="PlatformSplit">Plataforma recebe e divide os valores</option></select></label>
            <label>Ambiente<select value={environment} onChange={e=>setEnvironment(e.target.value)}><option value="Sandbox">Sandbox — testes</option>{current.data?.productionAllowed&&<option value="Production">Produção — dinheiro real</option>}</select></label>
            {mode==='Direct'?<><label>API Key<input type="password" autoComplete="new-password" required value={apiKey} onChange={e=>setApiKey(e.target.value)} maxLength={1000}/></label><label>Token de autenticação do webhook<input type="password" autoComplete="new-password" required minLength={32} maxLength={255} value={token} onChange={e=>setToken(e.target.value)}/></label><p>Use um token independente, de 32 a 255 caracteres sem espaços. As credenciais não são exibidas após salvar.</p></>:<p>A administração da plataforma deve configurar no servidor a API Key, o token do webhook, a carteira deste lojista e seu percentual do valor líquido. A diferença líquida fica com a plataforma.</p>}
            <button disabled={busy||current.loading} className="primary">{busy?'Verificando conta…':'Verificar e salvar conexão'}</button>
        </form><section className="form-card"><h2>Eventos necessários</h2><p>PAYMENT_CREATED, PAYMENT_UPDATED, PAYMENT_CONFIRMED, PAYMENT_RECEIVED, PAYMENT_DELETED, PAYMENT_OVERDUE, PAYMENT_REFUNDED, PAYMENT_REFUND_IN_PROGRESS e eventos de chargeback. Para cartão: CHECKOUT_CREATED, CHECKOUT_PAID, CHECKOUT_CANCELED e CHECKOUT_EXPIRED.</p><p>A integração valida o token e registra os eventos antes de responder. Retornos de navegador não alteram o estado financeiro.</p></section>
    </main></BackofficeLayout>;
}
