import {useState} from 'react';
import {useResource} from '../services/useResource';
import {post} from '../services/http';
import {type OperationDetail,label} from '../services/operations';

export function FulfillmentPanel({orderId,admin=false}:{orderId:number;admin?:boolean}) {
 const [revision,setRevision]=useState(0);const result=useResource<OperationDetail>(admin?`/v1/operations/orders/${orderId}`:`/v1/store/orders/${orderId}/fulfillment`,revision);
 const [busy,setBusy]=useState(false);const [error,setError]=useState('');const [notes,setNotes]=useState('');const [carrier,setCarrier]=useState('');const [service,setService]=useState('');const [tracking,setTracking]=useState('');
 const [quantities,setQuantities]=useState<Record<number,number>>({});const [restock,setRestock]=useState<Record<number,number>>({});
 const [pending,setPending]=useState<{signature:string;key:string}>();
 const d=result.data;
 async function run(path:string,body:object){
  if(!d||busy)return;const payload={...body,version:d.version};const signature=JSON.stringify({path,payload});
  const attempt=pending?.signature===signature?pending:{signature,key:crypto.randomUUID()};setPending(attempt);
  setBusy(true);setError('');
  try{await post(path,{...payload,key:attempt.key});setPending(undefined);setRevision(x=>x+1);setQuantities({});setRestock({});setNotes('');}
  catch(e){setError((e as Error).message);}finally{setBusy(false);}
 }
 const base=`/v1/operations/orders/${orderId}`;
 const selected=Object.entries(quantities).filter(([,q])=>q>0).map(([id,quantity])=>({orderItemId:Number(id),quantity}));
 return <section className="form-card"><h2>Atendimento e entrega</h2>{(error||result.error)&&<p role="alert" className="notice error">{error||result.error}</p>}
  <button disabled={busy} onClick={()=>setRevision(x=>x+1)}>Atualizar acompanhamento</button>
  {result.loading?<p>Carregando acompanhamento…</p>:d&&<>
   <p>{label(d.fulfillmentState)} · Financeiro: {label(d.financialState)}</p>{d.fulfillmentBlocked&&<p className="notice">Expedição bloqueada para análise financeira.</p>}
   {admin&&<div className="finance-actions">{d.allowedActions.filter(x=>x!=='Dispatch').map(action=><button key={action} disabled={busy} onClick={()=>void run(base+'/transition',{action,notes})}>{label(action)}</button>)}</div>}
   <h3>Itens do pedido</h3>{d.items.map(i=><p key={i.id}>{i.quantity} × {i.productName} · SKU {i.sku}</p>)}
   {(admin?d.allowedActions.includes('Dispatch'):d.shipments.some(s=>s.state==='Delivered'))&&<fieldset disabled={busy}><legend>{admin?'Registrar postagem já realizada':'Solicitar devolução dos itens entregues'}</legend>
    {d.items.map(i=><label key={i.id}>{i.productName} — quantidade<input type="number" min="0" max={i.quantity} value={quantities[i.id]||0} onChange={e=>setQuantities({...quantities,[i.id]:Number(e.target.value)})}/></label>)}
    {admin&&<><label>Transportadora<input value={carrier} maxLength={100} onChange={e=>setCarrier(e.target.value)}/></label><label>Serviço<input value={service} maxLength={100} onChange={e=>setService(e.target.value)}/></label><label>Código de rastreio<input value={tracking} maxLength={120} onChange={e=>setTracking(e.target.value)}/></label><p>Registro assistido de um volume. Não compra nem gera etiqueta.</p></>}
    {!admin&&<label>Motivo da devolução<textarea maxLength={1000} value={notes} onChange={e=>setNotes(e.target.value)}/></label>}
    <button disabled={!selected.length} onClick={()=>void run(admin?base+'/shipments':`/v1/store/orders/${orderId}/fulfillment/returns`,admin?{carrier,service,trackingCode:tracking,volumes:1,items:selected}:{reason:notes,items:selected})}>{admin?'Confirmar registro da postagem':'Enviar solicitação para análise'}</button>
   </fieldset>}
   {admin&&<label>Nota interna ou evidência de rastreio/parecer<textarea maxLength={1000} value={notes} onChange={e=>setNotes(e.target.value)}/><button disabled={busy||!notes.trim()} onClick={()=>void run(base+'/transition',{action:'Note',notes})}>Salvar nota interna</button></label>}
   <h3>Remessas</h3>{!d.shipments.length&&<p>Nenhuma remessa registrada.</p>}{d.shipments.map(s=><section className="form-card" key={s.id}><h4>{s.carrier} · {s.service}</h4><p>Rastreio: {s.trackingCode} · {label(s.state)} · {s.volumes} volume(s)</p>
    {d.shipmentItems.filter(i=>i.shipmentId===s.id).map(i=><p key={i.orderItemId}>{i.quantity} × {d.items.find(x=>x.id===i.orderItemId)?.productName}</p>)}
    {admin&&s.state!=='Delivered'&&<div className="finance-actions">{['InTransit','Delivered','DeliveryFailed'].filter(x=>x!==s.state).map(state=><button key={state} disabled={busy||!notes.trim()} onClick={()=>void run(base+`/shipments/${s.id}/tracking`,{state,notes})}>{label(state)}</button>)}</div>}
   </section>)}
   <h3>Devoluções</h3>{!d.returns.length&&<p>Nenhuma solicitação.</p>}{d.returns.map(r=><section className="form-card" key={r.id}><h4>Protocolo {r.id.slice(0,8)} · {label(r.state)}</h4><p>{r.reason}</p><p>{r.decision}</p>
    {d.returnItems.filter(i=>i.returnCaseId===r.id).map(i=><div key={i.orderItemId}><p>{i.quantity} × {d.items.find(x=>x.id===i.orderItemId)?.productName} · Reposto: {i.restockedQuantity}</p>{admin&&r.state==='Approved'&&<label>Quantidade recebida e aprovada para reposição<input type="number" min="0" max={i.quantity} value={restock[i.orderItemId]||0} onChange={e=>setRestock({...restock,[i.orderItemId]:Number(e.target.value)})}/></label>}</div>)}
    <p>O reembolso é acompanhado separadamente no pagamento.</p>
    {admin&&r.state==='Requested'&&['Approved','Rejected'].map(action=><button key={action} disabled={busy||!notes.trim()} onClick={()=>void run(base+`/returns/${r.id}`,{action,reason:notes,items:[]})}>{label(action)}</button>)}
    {admin&&r.state==='Approved'&&<button disabled={busy||!notes.trim()} onClick={()=>void run(base+`/returns/${r.id}`,{action:'Inspected',reason:notes,items:d.returnItems.filter(i=>i.returnCaseId===r.id).map(i=>({orderItemId:i.orderItemId,quantity:restock[i.orderItemId]||0}))})}>Confirmar inspeção física e reposição informada</button>}
   </section>)}
   <h3>Histórico</h3><ol>{d.timeline.map(e=><li key={e.id}>{new Date(e.createdAt.endsWith('Z')?e.createdAt:e.createdAt+'Z').toLocaleString('pt-BR')} · {label(e.kind)} · {label(e.newState)}{e.notes&&<p>{e.notes}</p>}{e.actor&&<small>{e.actor}</small>}</li>)}</ol>
  </>}
 </section>;
}
