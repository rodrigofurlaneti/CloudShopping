import { useResource } from '../services/useResource';
import { useEffect, useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { StoreLayout } from '../layouts/StoreLayout';
import { request, post, ApiError } from '../services/http';
import { type Address, type Profile, type Shipping, type Preview, type Order, ensureCustomer, money } from '../services/storeService';
export function CheckoutPage() {
    const [profile,setProfile]=useState<Profile>({email:'',name:'',type:'B2C',taxId:''});
    const [addresses,setAddresses]=useState<Address[]>([]);const [addressId,setAddressId]=useState(0);
    const [selectedShippingId,setShippingId]=useState(0);
    const shippingResource=useResource<Shipping[]>(addressId?'/v1/store/shipping-options?addressId='+addressId:null);
    const shipping=shippingResource.data||[];const shippingId=shipping.some(s=>s.id===selectedShippingId)?selectedShippingId:shipping[0]?.id||0;
    const [preview,setPreview]=useState<Preview>();const [error,setError]=useState('');const [status,setStatus]=useState('');
    const [busy,setBusy]=useState(false);const [ready,setReady]=useState(false);
    const [pending,setPending]=useState<{key:string;token:string}|null>(()=>{try{return JSON.parse(sessionStorage.getItem('checkout:pending')||'null');}catch{return null;}});
    const navigate=useNavigate();
    useEffect(()=>{ensureCustomer().then(()=>Promise.all([request<Profile>('/v1/store/profile'),request<Address[]>('/v1/store/addresses')])).then(([p,a])=>{setProfile({...p,email:p.email||''});setAddresses(a);setAddressId(a[0]?.id||0);setReady(true);}).catch(e=>setError(e.message));},[]);
    const run=async(action:()=>Promise<void>)=>{setBusy(true);setError('');setStatus('');try{await action();}catch(e){setError((e as Error).message);}finally{setBusy(false);}};
    const confirm=async(attempt:{key:string;token:string})=>{
        sessionStorage.setItem('checkout:pending',JSON.stringify(attempt));setPending(attempt);
        let order:Order;
        try { order=await post<Order>('/v1/store/checkout/confirm',attempt); }
        catch (e) {
            // A definitive conflict is returned after the transaction is rolled back.
            // Network failures retain the key for safe recovery, never a new order.
            if(e instanceof ApiError && e.status===409) { sessionStorage.removeItem('checkout:pending');setPending(null);setPreview(undefined); }
            throw e;
        }
        sessionStorage.removeItem('checkout:pending');setPending(null);navigate('/orders/'+order.id);
    };
    return <StoreLayout><h1>Finalizar pedido</h1><p className="muted">Revise seus dados e a entrega. Esta etapa cria o pedido; não realiza pagamento.</p>
        {error&&<p className="notice error" role="alert">{error}</p>}{status&&<p className="notice" role="status">{status}</p>}
        {pending&&<div className="notice"><p>Há uma confirmação anterior para recuperar. Consulte o resultado antes de tentar novamente.</p><button disabled={busy} onClick={()=>void run(()=>confirm(pending))}>Recuperar confirmação</button><Link to="/orders">Consultar meus pedidos</Link></div>}
        {!ready ? !error&&<p role="status">Preparando checkout…</p> : <div className="checkout-grid"><div>
            <section className="form-card"><h2>1. Dados do comprador</h2><form onSubmit={e=>{e.preventDefault();void run(async()=>{await request('/v1/store/profile',{method:'PUT',body:JSON.stringify(profile)});setPreview(undefined);setStatus('Dados do comprador salvos.');});}}>
                <label>Tipo de pessoa<select value={profile.type} onChange={e=>{setProfile({...profile,type:e.target.value as Profile['type']});setPreview(undefined);}}><option value="B2C">Pessoa física</option><option value="B2B">Pessoa jurídica</option></select></label>
                <label>{profile.type==='B2C'?'Nome completo':'Razão social'}<input required maxLength={150} value={profile.name} onChange={e=>{setProfile({...profile,name:e.target.value});setPreview(undefined);}} /></label>
                <label>Email<input required type="email" maxLength={100} value={profile.email} onChange={e=>{setProfile({...profile,email:e.target.value});setPreview(undefined);}} /></label>
                <label>{profile.type==='B2C'?'CPF':'CNPJ'} (somente números)<input required inputMode="numeric" pattern={profile.type==='B2C'?'[0-9]{11}':'[0-9]{14}'} maxLength={14} value={profile.taxId} onChange={e=>{setProfile({...profile,taxId:e.target.value.replace(/\D/g,'')});setPreview(undefined);}} /></label>
                <button disabled={busy}>Salvar dados</button>
            </form></section>
            <section className="form-card"><h2>2. Endereço de entrega</h2>
                {addresses.length>0&&<label>Endereço cadastrado<select value={addressId} onChange={e=>{setAddressId(Number(e.target.value));setShippingId(0);setPreview(undefined);}}>{addresses.map(a=><option key={a.id} value={a.id}>{a.street}, {a.number} — {a.city}/{a.state}</option>)}</select></label>}
                <details open={!addresses.length}><summary>Adicionar endereço</summary><form onSubmit={e=>{e.preventDefault();const f=new FormData(e.currentTarget);const form=e.currentTarget;void run(async()=>{const result=await post<{id:number}>('/v1/store/addresses',Object.fromEntries(f));setAddresses(await request<Address[]>('/v1/store/addresses'));setAddressId(result.id);setShippingId(0);setPreview(undefined);form.reset();setStatus('Endereço salvo.');});}}>
                    <label>CEP<input name="zipCode" required inputMode="numeric" pattern="[0-9]{8}" maxLength={8} placeholder="Somente 8 números" /></label>
                    <label>Rua<input name="street" required maxLength={150}/></label><label>Número<input name="number" required maxLength={10}/></label>
                    <label>Bairro<input name="neighborhood" maxLength={50}/></label><label>Cidade<input name="city" required maxLength={50}/></label><label>UF<input name="state" required pattern="[A-Za-z]{2}" maxLength={2}/></label>
                    <button disabled={busy}>Salvar endereço</button>
                </form></details>
            </section>
            <section className="form-card"><h2>3. Entrega</h2>{shippingResource.loading?<p>Consultando entrega…</p>:shippingResource.error?<p role="alert">{shippingResource.error}</p>:addressId&&!shipping.length?<p>Nenhuma opção disponível para este CEP. A loja precisa configurar a entrega.</p>:shipping.map(s=><label className="radio-option" key={s.id}><input type="radio" name="shipping" checked={shippingId===s.id} onChange={()=>{setShippingId(s.id);setPreview(undefined);}}/>{s.name} · {money(s.amount)} · até {s.estimatedDays} dia(s)</label>)}
                <button disabled={busy||!shippingId||!addressId} onClick={()=>void run(async()=>{setPreview(await post<Preview>('/v1/store/checkout/preview',{addressId,shippingId}));})}>Revisar pedido</button>
            </section>
        </div><aside className="form-card order-review"><h2>4. Revisão</h2>{preview?<><ul>{preview.cart.items.map(i=><li key={i.productId}>{i.quantity} × {i.name}<strong>{money(i.price*i.quantity)}</strong></li>)}</ul><p>Subtotal <strong>{money(preview.cart.subtotal)}</strong></p><p>{preview.shippingName} <strong>{money(preview.shippingAmount)}</strong></p><p className="total">Total <strong>{money(preview.total)}</strong></p><p className="muted">Valores válidos até {new Date(preview.expiresAt.endsWith('Z')?preview.expiresAt:preview.expiresAt+'Z').toLocaleTimeString('pt-BR')}.</p><button className="primary" disabled={busy||!!pending} onClick={()=>void run(()=>confirm({key:crypto.randomUUID(),token:preview.token}))}>{busy?'Confirmando…':'Confirmar pedido'}</button></>:<p>Salve seus dados, selecione endereço e entrega e clique em revisar.</p>}<Link to="/cart">Voltar ao carrinho</Link></aside></div>}
    </StoreLayout>;
}

