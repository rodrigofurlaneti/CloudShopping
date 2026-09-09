import { useEffect,useState } from 'react';
import { BackofficeLayout } from '../../layouts/BackofficeLayout';
import { post,request } from '../../services/http';
import { type Shipping,money } from '../../services/storeService';
export function ShippingSettings(){
    const [items,setItems]=useState<Shipping[]>([]);const [error,setError]=useState('');const [busy,setBusy]=useState(false);
    const load=()=>request<Shipping[]>('/v1/store/admin/shipping-options').then(setItems).catch(e=>setError(e.message));
    useEffect(()=>{void load();},[]);
    return <BackofficeLayout><div className="commerce-admin"><h1>Opções de entrega</h1><p>Cadastre preço e cobertura por prefixo de CEP. Prefixo vazio atende todos os CEPs. Para retirada, identifique o local no nome da opção.</p>
        {error&&<p role="alert" className="notice error">{error}</p>}<form className="form-card" onSubmit={async e=>{e.preventDefault();const f=new FormData(e.currentTarget);const form=e.currentTarget;setBusy(true);setError('');try{await post('/v1/store/admin/shipping-options',{name:f.get('name'),amount:Number(f.get('amount')),postalCodePrefix:f.get('postalCodePrefix'),estimatedDays:Number(f.get('estimatedDays'))});form.reset();await load();}catch(e){setError((e as Error).message);}finally{setBusy(false);}}}>
            <label>Nome da opção<input name="name" required maxLength={100}/></label><label>Valor (R$)<input name="amount" type="number" min="0" max="999999.99" step="0.01" required/></label>
            <label>Prefixo do CEP<input name="postalCodePrefix" pattern="[0-9]{0,8}" maxLength={8}/></label><label>Prazo em dias<input name="estimatedDays" type="number" min="0" max="365" required/></label>
            <button className="primary" disabled={busy}>Cadastrar entrega</button>
        </form><div className="cart-lines">{items.map(s=><article className="cart-line" key={s.id}><div><h2>{s.name}</h2><p>{money(s.amount)} · CEP {s.postalCodePrefix||'todos'} · {s.estimatedDays} dia(s)</p></div><span>{s.isActive?'Ativa':'Desativada'}</span>{s.isActive&&<button disabled={busy} onClick={async()=>{setBusy(true);try{await request('/v1/store/admin/shipping-options/'+s.id,{method:'DELETE'});await load();}catch(e){setError((e as Error).message);}finally{setBusy(false);}}}>Desativar</button>}</article>)}</div></div></BackofficeLayout>;
}

