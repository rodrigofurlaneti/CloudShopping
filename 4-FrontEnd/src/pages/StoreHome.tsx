import { useState } from 'react';
import { ArrowUpRight, ArrowRight, Package, SearchX, SlidersHorizontal, Truck, Headphones } from 'lucide-react';
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
    const featured = data?.items.find(p => p.image && p.availableStock > 0);
    const filtered = !!(params.get('search') || params.get('departmentId'));
    const selectDepartment = (id: string) => { const p = new URLSearchParams(params); if(id) p.set('departmentId',id); else p.delete('departmentId'); p.delete('page'); setParams(p); };
    return <StoreLayout>
        {!filtered && <section className="store-hero"><div className="hero-copy"><p className="eyebrow"><span/> ESCOLHAS PARA O SEU DIA A DIA</p><h1>Sua próxima<br/>boa escolha<span> está aqui.</span></h1><p>Descubra os produtos da nossa loja. Compare os detalhes e encontre o que faz sentido para você.</p><a className="hero-cta" href="#catalogo">Explorar produtos <ArrowRight size={19}/></a><div className="hero-caption"><span>01 / EXPLORE A LOJA</span><span>Do seu jeito. No seu tempo.</span></div></div><div className="hero-showcase"><div className="showcase-label">EM NOSSO CATÁLOGO <ArrowUpRight size={18}/></div>{featured ? <Link to={'/product/'+featured.id} className="featured-product"><img src={resolveStaticUrl(featured.image!)} alt={featured.name}/><div><span>{featured.brand || 'Conheça os detalhes'}</span><h2>{featured.name}</h2><strong>{money(featured.price)}</strong></div></Link> : <div className="hero-placeholder"><Package size={100} strokeWidth={.8}/><strong>Encontre novas possibilidades.</strong><span>Explore nosso catálogo abaixo.</span></div>}<span className="showcase-corner" aria-hidden="true">+</span></div></section>}
        <nav className="department-strip" aria-label="Departamentos"><button aria-pressed={!params.get('departmentId')} onClick={()=>selectDepartment('')}>Todos os produtos <ArrowUpRight size={15}/></button>{departments.map(d=><button key={d.id} aria-pressed={params.get('departmentId')===String(d.id)} onClick={()=>selectDepartment(String(d.id))}>{d.name}</button>)}</nav>
        <div id="catalogo" className="section-toolbar"><div><p className="eyebrow">EXPLORE E ENCONTRE</p><h2>{params.get('search') ? `Resultados para “${params.get('search')}”` : 'Escolha o seu próximo favorito'} {data ? <small>{data.totalCount} produtos</small> : ''}</h2></div>
            <label>Departamento <select value={params.get('departmentId') || ''} onChange={e => { const p = new URLSearchParams(params); if (e.target.value) p.set('departmentId', e.target.value); else p.delete('departmentId'); p.delete('page'); setParams(p); }}>
                <option value="">Todos</option>{departments.map(d => <option key={d.id} value={d.id}>{d.name}</option>)}
            </select></label></div>
        {filtered && <div className="active-filters"><SlidersHorizontal size={15}/><span>Catálogo filtrado</span><Link to="/">Limpar filtros ×</Link></div>}
        {loading ? <div role="status"><p>Carregando catálogo…</p><div className="product-grid" aria-hidden="true">{[1,2,3,4].map(i=><div className="product-skeleton" key={i}/>)}</div></div> : error ? <div role="alert" className="notice error">{error} <button onClick={() => setRetry(x=>x+1)}>Tentar novamente</button></div> :
            !data?.items.length ? <div className="empty-state"><SearchX size={36}/><h2>Nenhum produto encontrado</h2><p>Tente outra busca ou departamento.</p><Link to="/">Limpar filtros</Link></div> :
            <div className="product-grid">{data.items.map(p => <Link className="catalog-card" to={'/product/' + p.id} key={p.id}>
                <div className="product-image"><span className="stock-tag">{p.availableStock > 0 ? 'Disponível' : 'Esgotado'}</span>{p.image ? <img src={resolveStaticUrl(p.image)} alt={p.name} loading="lazy" /> : <div className="no-product-image"><Package size={48} strokeWidth={1}/><span>Imagem indisponível</span></div>}<span className="card-arrow"><ArrowUpRight size={20}/></span></div>
                <div className="catalog-copy"><small>{p.brand || p.sku}</small><h3>{p.name}</h3><div className="card-price"><strong>{money(p.price)}</strong><span>Ver detalhes <ArrowRight size={14}/></span></div></div>
            </Link>)}</div>}
        {data && data.totalPages > 1 && <nav className="pagination" aria-label="Paginação">
            <button disabled={data.page <= 1} onClick={() => { const p=new URLSearchParams(params);p.set('page',String(data.page-1));setParams(p); }}>Anterior</button>
            <span>Página {data.page} de {data.totalPages}</span>
            <button disabled={data.page >= data.totalPages} onClick={() => { const p=new URLSearchParams(params);p.set('page',String(data.page+1));setParams(p); }}>Próxima</button>
        </nav>}
        <section className="store-benefits" aria-label="Informações de compra"><div><Package size={25}/><span><strong>Todos os detalhes à mão</strong><small>Conheça cada produto antes de escolher.</small></span></div><div><Truck size={25}/><span><strong>Entrega para o seu endereço</strong><small>Consulte opções e valores no checkout.</small></span></div><Link to="/support"><Headphones size={25}/><span><strong>Conte com a nossa ajuda</strong><small>Fale com a loja pelo atendimento.</small></span><ArrowUpRight size={17}/></Link></section>
    </StoreLayout>;
}
