import { useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { StoreLayout } from '../layouts/StoreLayout';
import { useResource } from '../services/useResource';
import { type Product, type Page, money } from '../services/storeService';
import { resolveStaticUrl } from '../services/api';
export function StoreHome() {
    const [params, setParams] = useSearchParams();
    const [retry, setRetry] = useState(0);
    const query = params.toString();
    const {data,error:catalogError,loading} = useResource<Page<Product>>('/v1/store/products?' + query,retry);
    const {data:departments=[],error:departmentError} = useResource<{id:number;name:string}[]>('/v1/store/departments',retry);
    const error = catalogError || departmentError;
    return <StoreLayout>
        <section className="store-intro"><p className="eyebrow">Sua próxima escolha</p><h1>Encontre o que você precisa.</h1><p>Explore os produtos disponíveis nesta loja.</p></section>
        <div className="section-toolbar"><h2>Catálogo {data ? <small>({data.totalCount} produtos)</small> : ''}</h2>
            <label>Departamento <select value={params.get('departmentId') || ''} onChange={e => { const p = new URLSearchParams(params); if (e.target.value) p.set('departmentId', e.target.value); else p.delete('departmentId'); p.delete('page'); setParams(p); }}>
                <option value="">Todos</option>{departments.map(d => <option key={d.id} value={d.id}>{d.name}</option>)}
            </select></label></div>
        {loading ? <p role="status">Carregando catálogo…</p> : error ? <div role="alert" className="notice error">{error} <button onClick={() => setRetry(x=>x+1)}>Tentar novamente</button></div> :
            !data?.items.length ? <div className="empty-state"><h2>Nenhum produto encontrado</h2><p>Tente outra busca ou departamento.</p><Link to="/">Limpar filtros</Link></div> :
            <div className="product-grid">{data.items.map(p => <Link className="catalog-card" to={'/product/' + p.id} key={p.id}>
                <div className="product-image">{p.image ? <img src={resolveStaticUrl(p.image)} alt={p.name} loading="lazy" /> : <span>Sem imagem</span>}</div>
                <div className="catalog-copy"><small>{p.sku}</small><h3>{p.name}</h3><strong>{money(p.price)}</strong><p className={p.availableStock > 0 ? 'available' : 'unavailable'}>{p.availableStock > 0 ? 'Disponível' : 'Esgotado'}</p><span>Ver produto →</span></div>
            </Link>)}</div>}
        {data && data.totalPages > 1 && <nav className="pagination" aria-label="Paginação">
            <button disabled={data.page <= 1} onClick={() => { const p=new URLSearchParams(params);p.set('page',String(data.page-1));setParams(p); }}>Anterior</button>
            <span>Página {data.page} de {data.totalPages}</span>
            <button disabled={data.page >= data.totalPages} onClick={() => { const p=new URLSearchParams(params);p.set('page',String(data.page+1));setParams(p); }}>Próxima</button>
        </nav>}
    </StoreLayout>;
}
