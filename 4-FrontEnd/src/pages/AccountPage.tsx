import { useState } from 'react';
import { AccountSecurityPanel } from '../components/AccountSecurityPanel';
import { Link, useNavigate } from 'react-router-dom';
import { StoreLayout } from '../layouts/StoreLayout';
import { post, resetCsrf } from '../services/http';
import { useSession } from '../services/sessionContext';
export function AccountPage() {
    const {user,refresh,logout}=useSession(); const [register,setRegister]=useState(false);
    const [error,setError]=useState('');const [busy,setBusy]=useState(false); const navigate=useNavigate();
    return <StoreLayout><div className="form-card"><h1>Minha conta</h1>
        {user && !user.isGuest ? <><p>Sessão de {user.name}</p><p>Loja {user.tenantId}</p><Link to="/orders">Ver meus pedidos</Link><button onClick={()=>void logout().catch(e=>setError(e.message))}>Sair desta conta</button>
        <AccountSecurityPanel /></> :
        <form onSubmit={async e=>{e.preventDefault();setBusy(true);setError('');const data=new FormData(e.currentTarget);try{
            const email=String(data.get('email'));const password=String(data.get('password'));
            await post('/v1/session/'+(register?'register':'login'),register?{email,password}:{username:email,password});resetCsrf();await refresh();navigate('/cart');
        }catch(e){setError((e as Error).message);}finally{setBusy(false);}}}>
            <label>Email<input name="email" type="email" maxLength={100} autoComplete="email" required /></label>
            <label>Senha<input name="password" type="password" minLength={register?12:1} maxLength={72} autoComplete={register?'new-password':'current-password'} required /></label>
            {register&&<small>Use pelo menos 12 caracteres.</small>}<button className="primary" disabled={busy}>{busy?'Aguarde…':register?'Criar conta':'Entrar'}</button>
            <button type="button" onClick={()=>setRegister(!register)}>{register?'Já tenho conta':'Quero criar uma conta'}</button>
        </form>}
        {error&&<p className="notice error" role="alert">{error}</p>}</div></StoreLayout>;
}

