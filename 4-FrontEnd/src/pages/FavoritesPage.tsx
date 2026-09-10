import {useState} from 'react';
import {Link} from 'react-router-dom';
import {StoreLayout} from '../layouts/StoreLayout';
import {useResource} from '../services/useResource';
import {request} from '../services/http';
import {money} from '../services/storeService';
export function FavoritesPage(){
 const [revision,setRevision]=useState(0);const [page,setPage]=useState(1);const [error,setError]=useState('');
 const result=useResource<{id:number;name:string;price:number;available:number}[]>('/v1/store/favorites?page='+page,revision);
 return <StoreLayout><h1>Meus favoritos</h1>{(error||result.error)&&<p role="alert">{error||result.error}</p>}{result.loading?<p>Carregando…</p>:result.data?.length?result.data.map(p=><section className="form-card" key={p.id}><Link to={'/product/'+p.id}>{p.name}</Link><p>{money(p.price)} · {p.available>0?'Disponível':'Esgotado'}</p><button onClick={()=>void request('/v1/store/favorites/'+p.id,{method:'DELETE'}).then(()=>setRevision(x=>x+1)).catch(e=>setError(e.message))}>Remover favorito</button></section>):<p>Nenhum favorito disponível nesta página.</p>}<nav className="pagination"><button disabled={page===1} onClick={()=>setPage(p=>p-1)}>Anterior</button><span>Página {page}</span><button disabled={(result.data?.length||0)<20} onClick={()=>setPage(p=>p+1)}>Próxima</button></nav></StoreLayout>;
}
