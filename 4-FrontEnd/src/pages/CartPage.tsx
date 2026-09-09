import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { StoreLayout } from '../layouts/StoreLayout';
import { request } from '../services/http';
import { type Cart, ensureCustomer, money } from '../services/storeService';
export function CartPage() {
    const [cart,setCart]=useState<Cart>(); const [error,setError]=useState(''); const [busy,setBusy]=useState(false);
    useEffect(()=>{ensureCustomer().then(()=>request<Cart>('/v1/store/cart')).then(setCart).catch(e=>setError(e.message));},[]);
    const mutate=async(path:string,method:string,body?:unknown)=>{setBusy(true);setError('');try{setCart(await request<Cart>(path,{method,body:body===undefined?undefined:JSON.stringify(body)}));}catch(e){setError((e as Error).message);}finally{setBusy(false);}};
    return <StoreLayout><h1>Meu carrinho</h1>{error&&<p className="notice error" role="alert">{error}</p>}
        {!cart ? !error&&<p role="status">Carregando…</p> : !cart.items.length ? <div className="empty-state"><h2>Seu carrinho está vazio</h2><Link to="/">Explorar produtos</Link></div> :
            <><div className="cart-lines">{cart.items.map(item=><article key={item.productId} className="cart-line">
                <div><Link to={'/product/'+item.productId}><h2>{item.name}</h2></Link><small>{item.sku}</small><p>{money(item.price)} por unidade</p>{item.quantity>item.availableStock&&<p className="error">Quantidade indisponível. Atualize o item.</p>}</div>
                <div className="quantity-control"><button aria-label={'Diminuir '+item.name} disabled={busy||item.quantity<=1} onClick={()=>void mutate('/v1/store/cart/items/'+item.productId,'PUT',{quantity:item.quantity-1})}>−</button><span aria-label="Quantidade">{item.quantity}</span><button aria-label={'Aumentar '+item.name} disabled={busy||item.quantity>=item.availableStock} onClick={()=>void mutate('/v1/store/cart/items/'+item.productId,'PUT',{quantity:item.quantity+1})}>+</button></div>
                <strong>{money(item.price*item.quantity)}</strong><button disabled={busy} onClick={()=>void mutate('/v1/store/cart/items/'+item.productId,'DELETE')}>Remover</button>
            </article>)}</div><div className="cart-total"><p>Subtotal <strong>{money(cart.subtotal)}</strong></p><p className="muted">Entrega calculada no checkout. Adicionar ao carrinho não reserva estoque.</p><Link className="primary button" to="/checkout">Continuar para o checkout</Link></div></>}
    </StoreLayout>;
}

