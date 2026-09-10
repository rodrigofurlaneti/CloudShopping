import {useState} from 'react';
import {BackofficeLayout} from '../../layouts/BackofficeLayout';
import {FulfillmentPanel} from '../../components/FulfillmentPanel';
import {useResource} from '../../services/useResource';
import {fulfillmentLabels,label} from '../../services/operations';
import {money} from '../../services/storeService';
export function OrderOperationsPage(){
 const [page,setPage]=useState(1);const [state,setState]=useState('');const [orderId,setOrderId]=useState('');const [selected,setSelected]=useState<number>();const [revision,setRevision]=useState(0);
 const result=useResource<{total:number;items:{id:number;totalAmount:number;financialState:string;fulfillmentState:string;fulfillmentBlocked:boolean}[]}>(`/v1/operations/orders?page=${page}${state?'&state='+state:''}${orderId?'&orderId='+orderId:''}`,revision);
 return <BackofficeLayout><main className="commerce-admin"><h1>Operação de pedidos</h1><p>Etapas de atendimento, remessas e devoluções com acompanhamento financeiro separado.</p>
  <div className="form-card"><label>Número do pedido<input type="number" min="1" value={orderId} onChange={e=>{setOrderId(e.target.value);setPage(1);}}/></label><label>Etapa<select value={state} onChange={e=>{setState(e.target.value);setPage(1);}}><option value="">Todas</option>{Object.keys(fulfillmentLabels).slice(0,7).map(s=><option key={s} value={s}>{label(s)}</option>)}</select></label><button onClick={()=>setRevision(x=>x+1)}>Atualizar lista</button></div>
  {result.error&&<p role="alert">{result.error}</p>}{result.loading?<p>Carregando pedidos…</p>:<><p>{result.data?.total||0} pedidos encontrados</p><div className="finance-list">{result.data?.items.map(o=><button className="form-card" key={o.id} onClick={()=>setSelected(o.id)}>Pedido #{o.id} · {money(o.totalAmount)}<br/>{label(o.fulfillmentState)} · {label(o.financialState)}{o.fulfillmentBlocked?' · Expedição bloqueada':''}</button>)}</div></>}
  <nav className="pagination"><button disabled={page===1} onClick={()=>setPage(p=>p-1)}>Anterior</button><span>Página {page}</span><button disabled={page*20>=(result.data?.total||0)} onClick={()=>setPage(p=>p+1)}>Próxima</button></nav>
  {selected&&<><h2>Pedido #{selected}</h2><button onClick={()=>{setSelected(undefined);setRevision(x=>x+1);}}>Fechar detalhe e atualizar lista</button><FulfillmentPanel key={selected} orderId={selected} admin/></>}
 </main></BackofficeLayout>;
}
