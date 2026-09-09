import {useEffect,useState} from 'react';
import {post,request} from '../services/http';
import {type PaymentView,paymentLabels,paymentUrl} from '../services/payments';
import {money} from '../services/storeService';
export function PaymentPanel({orderId,pending,onChanged}:{orderId:number;pending:boolean;onChanged:()=>void}){
    const [data,setData]=useState<PaymentView>();const [method,setMethod]=useState('PIX');const [busy,setBusy]=useState(false);const [error,setError]=useState('');
    useEffect(()=>{
        let active=true;
        const load=()=>request<PaymentView>('/v1/asaas/orders/'+orderId).then(x=>{if(active)setData(x);}).catch(e=>{if(active)setError(e.message);});
        void load();const timer=window.setInterval(()=>void load(),15000);
        return()=>{active=false;clearInterval(timer);};
    },[orderId]);
    const run=async(action:string)=>{setBusy(true);setError('');try{
        setData(await post<PaymentView>('/v1/asaas/orders/'+orderId+action,action===''?{method}:undefined));onChanged();
    }catch(e){setError((e as Error).message);}finally{setBusy(false);}};
    const a=data?.attempt;
    const payable=a?.state==='AwaitingPayment'&&!a.cancelRequested&&!a.refundRequested;
    return <section className="form-card payment-panel"><h2>Pagamento</h2>
        {error&&<p className="notice error" role="alert">{error}</p>}
        {!data?<p>Consultando pagamento…</p>:<>
            {data.fulfillmentBlocked&&<p className="notice error">A loja precisa analisar este pagamento antes de liberar a entrega.</p>}
            {!a?data.configured&&pending?<>
                <label>Como deseja pagar?<select value={method} onChange={e=>setMethod(e.target.value)}><option value="PIX">Pix</option><option value="BOLETO">Boleto</option><option value="CREDIT_CARD">Cartão de crédito à vista</option></select></label>
                <p>O pagamento pertence a este pedido. Em caso de falha, recupere a mesma tentativa.</p>
                {method==='BOLETO'&&<p>Boleto com vencimento em dois dias. A confirmação depende da compensação bancária.</p>}
                {method==='CREDIT_CARD'&&<p>Você informará os dados do cartão na página segura do Asaas.</p>}
                <button className="primary" disabled={busy} onClick={()=>void run('')}>{busy?'Preparando…':'Continuar para pagamento'}</button>
            </>:<p>{data.configured?'Este pedido não aceita uma nova cobrança.':'A loja ainda não habilitou os pagamentos online.'}</p>:<>
                <p className="notice" role="status">{paymentLabels[a.state]||a.state} · {money(a.amount)}</p>
                {a.lastError&&<p role="status">{a.lastError}</p>}
                {payable&&a.method==='PIX'&&<>
                    {a.pixImage&&/^[A-Za-z0-9+/=\r\n]+$/.test(a.pixImage)&&<img className="pix-code" src={'data:image/png;base64,'+a.pixImage} alt="QR Code Pix do pedido"/>}
                    {a.pixPayload&&<label>Pix copia e cola<textarea readOnly value={a.pixPayload}/><button disabled={busy} onClick={()=>void navigator.clipboard.writeText(a.pixPayload!).catch(()=>setError('Selecione e copie o código Pix manualmente.'))}>Copiar código Pix</button></label>}
                </>}
                {payable&&paymentUrl(a.method==='BOLETO'?a.bankSlipUrl||a.paymentUrl:a.paymentUrl)&&<a className="primary button-link" href={paymentUrl(a.method==='BOLETO'?a.bankSlipUrl||a.paymentUrl:a.paymentUrl)} target="_blank" rel="noopener noreferrer">{a.method==='BOLETO'?'Abrir boleto':a.method==='CREDIT_CARD'?'Pagar no Asaas':'Abrir cobrança Asaas'}</a>}
                {a.refundRequested&&a.method==='BOLETO'&&paymentUrl(a.refundRequestUrl)&&<p>Para continuar o estorno do boleto, <a href={paymentUrl(a.refundRequestUrl)} target="_blank" rel="noopener noreferrer">preencha os dados solicitados pelo Asaas</a>. O valor ainda não foi devolvido.</p>}
                <p>Abrir a página ou retornar à loja não confirma pagamento. O resultado é atualizado após a confirmação do Asaas.</p>
                <button disabled={busy} onClick={()=>void run('/refresh')}>{busy?'Consultando…':'Atualizar pagamento'}</button>
                {['Queued','Unknown','AwaitingPayment','Creating','CancelPending','Failed'].includes(a.state)&&<button disabled={busy} onClick={()=>void run('/cancel')}>Solicitar cancelamento</button>}
                {['Paid','Refunded','PaidReview'].includes(a.state)&&<p>{a.state==='Refunded'?'O estorno foi confirmado. A reposição física depende da devolução.':'Consulte a loja se precisar cancelar uma compra já paga.'}</p>}
            </>}
        </>}
    </section>;
}
