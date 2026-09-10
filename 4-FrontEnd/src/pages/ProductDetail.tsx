import { useState } from 'react';
import { ProductReviews } from '../components/ProductReviews';
import { Link, useParams } from 'react-router-dom';
import { StoreLayout } from '../layouts/StoreLayout';
import { useResource } from '../services/useResource';
import { type Product, addToCart, money } from '../services/storeService';
import { resolveStaticUrl } from '../services/api';
export function ProductDetail() {
    const { id, slug } = useParams(); const {data:product,error:loadError}=useResource<Product>(slug?'/v1/store/products/by-slug/'+encodeURIComponent(slug):'/v1/store/products/'+id); const [error,setError]=useState(''); const [busy,setBusy]=useState(false); const [quantity,setQuantity]=useState(1); const [added,setAdded]=useState(false);
    return <StoreLayout><Link to="/">← Catálogo</Link>{(error||loadError)&&<p className="notice error" role="alert">{error||loadError}</p>}
        {!product ? !(error||loadError)&&<p role="status">Carregando produto…</p> : <div className="product-detail">
            <div className="product-gallery">{product.images?.length ? product.images.map((img,i)=><img key={img} src={resolveStaticUrl(img)} alt={product.name + ' — imagem ' + (i+1)} />) : <div className="empty-state">Sem imagem cadastrada</div>}</div>
            <section><p className="eyebrow">SKU {product.sku}</p><h1>{product.name}</h1><p className="price">{money(product.price)}</p><p>{product.availableStock > 0 ? product.availableStock+' unidades disponíveis' : 'Produto esgotado'}</p>
                <form onSubmit={async e=>{e.preventDefault();setBusy(true);setAdded(false);setError('');try{await addToCart(product.id,quantity);setAdded(true);}catch(e){setError((e as Error).message);}finally{setBusy(false);}}}>
                    <label>Quantidade<input type="number" min="1" max={Math.min(product.availableStock,999)} value={quantity} onChange={e=>setQuantity(Number(e.target.value))} required /></label>
                    <button className="primary" disabled={busy||product.availableStock<=0}>{busy?'Adicionando…':'Adicionar ao carrinho'}</button>
                </form>
                {added&&<p className="notice" role="status">Produto adicionado. <Link to="/cart">Ver carrinho →</Link></p>}
                {product.description&&<p style={{whiteSpace:'pre-wrap'}}>{product.description}</p>}{product.brand&&<p>Marca: {product.brand}</p>}<dl>{Object.entries(product.attributes||{}).map(([name,value])=><div key={name}><dt>{name}</dt><dd>{value}</dd></div>)}</dl>{!!product.variants?.length&&<nav aria-label="Variantes do produto">{product.variants.map(v=><Link className="button-link" key={v.id} to={'/p/'+v.slug} aria-current={v.id===product.id?'page':undefined}>{v.variantLabel} · {money(v.price)} · {v.availableStock>0?'Disponível':'Esgotado'}</Link>)}</nav>}<p className="muted">O preço e a disponibilidade serão conferidos ao finalizar o pedido.</p>
            </section>
        </div>}
        {product&&<ProductReviews key={product.id} productId={product.id}/>}
    </StoreLayout>;
}
