import { PaymentPanel } from '../components/PaymentPanel';
import { useResource } from '../services/useResource';
import { useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { StoreLayout } from '../layouts/StoreLayout';
import { post } from '../services/http';
import { type Order, money } from '../services/storeService';
export function OrdersPage() {
    const {id}=useParams();const [error,setError]=useState('');const [page,setPage]=useState(1);const [version,setVersion]=useState(0);const [busy,setBusy]=useState(false);
    const result=useResource<Order|Order[]>(id?'/v1/store/orders/'+id:'/v1/store/orders?page='+page,version);
    const loading=result.loading; const orders=Array.isArray(result.data)?result.data:[];const detail=!Array.isArray(result.data)?result.data:undefined;
    return <StoreLayout><h1>{id?'Pedido #'+id:'Meus pedidos'}</h1>{(error||result.error)&&<p role="alert" className="notice error">{error||result.error} <Link to="/account">Entrar na minha conta</Link></p>}
        {loading?<p role="status">Carregando pedidos…</p>:id&&detail?<section className="form-card">
            <p className="notice">{detail.orderStatusId===16?'Pedido cancelado.':detail.orderStatusId===1?'Pedido criado. Confira a situação financeira abaixo.':detail.orderStatusId===2?'Pedido pago.':detail.orderStatusId===15?'Pedido estornado.':'Status do pedido: '+detail.orderStatusId}</p>
            <PaymentPanel orderId={detail.id} pending={detail.orderStatusId===1} onChanged={()=>setVersion(v=>v+1)} />
            {detail.reservationExpiresAt&&<p>Reserva até {new Date(detail.reservationExpiresAt.endsWith('Z')?detail.reservationExpiresAt:detail.reservationExpiresAt+'Z').toLocaleString('pt-BR')} · {detail.reservationState==='Reserved'?'Reservado':detail.reservationState==='Released'?'Liberado':'Sem reserva ativa'}</p>}
            <ul className="order-items">{detail.items.map((i,n)=><li key={n}>{i.quantity} × {i.name||i.sku||'Produto '+i.productId}<strong>{money(i.unitPrice*i.quantity)}</strong></li>)}</ul>
            <p>Entrega: {detail.shippingMethod} · {money(detail.shippingAmount)}</p><p className="total">Total {money(detail.totalAmount)}</p>
            {detail.address&&<address>{detail.address.street}, {detail.address.number}<br/>{detail.address.city}/{detail.address.state} · CEP {detail.address.zipCode}</address>}
            {detail.orderStatusId===1&&detail.reservationState==='Reserved'&&<button disabled={busy} onClick={async()=>{setBusy(true);try{await post('/v1/store/orders/'+id+'/cancel');setVersion(v=>v+1);}catch(e){setError((e as Error).message);}finally{setBusy(false);}}}>Solicitar cancelamento do pedido</button>}
            <Link to="/orders">Ver todos os pedidos</Link>
        </section>:!id?<><div className="cart-lines">{orders.map(o=><Link className="cart-line" key={o.id} to={'/orders/'+o.id}><h2>Pedido #{o.id}</h2><span>{o.orderStatusId===1?'Aguardando pagamento':o.orderStatusId===16?'Cancelado':'Status '+o.orderStatusId}</span><strong>{money(o.totalAmount)}</strong></Link>)}</div>{!orders.length&&!error&&<p>Nenhum pedido nesta página.</p>}<nav className="pagination"><button disabled={page===1} onClick={()=>setPage(page-1)}>Anterior</button><span>Página {page}</span><button disabled={orders.length<20} onClick={()=>setPage(page+1)}>Próxima</button></nav></>:null}
    </StoreLayout>;
}

