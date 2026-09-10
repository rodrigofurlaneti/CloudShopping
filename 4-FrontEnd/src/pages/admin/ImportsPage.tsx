import {useState} from 'react';
import {BackofficeLayout} from '../../layouts/BackofficeLayout';
import {useResource} from '../../services/useResource';
import {post} from '../../services/http';
import {label} from '../../services/operations';
import {money} from '../../services/storeService';
type Job={id:string;state:string;createdAt:string};
type Preview={job:Job;rows:{id:string;lineNumber:number;sku:string;name:string;price:number;physicalStock:number;expectedProductId?:number;state:string;error:string}[]};
export function ImportsPage(){
 const [revision,setRevision]=useState(0);const [page,setPage]=useState(1);const [id,setId]=useState<string>();const [csv,setCsv]=useState('');const [error,setError]=useState('');const [busy,setBusy]=useState(false);
 const list=useResource<Job[]>('/v1/catalog-imports?page='+page,revision);const detail=useResource<Preview>(id?'/v1/catalog-imports/'+id:null,revision);
 return <BackofficeLayout><main className="commerce-admin"><h1>Importação de catálogo</h1><p>CSV com cabeçalho <code>sku,name,departmentId,price,physicalStock</code>. Use ponto decimal no preço. Máximo de 200 produtos por lote. Cada linha é aplicada em uma transação; falhas não desfazem linhas já concluídas.</p>
 <p>O estoque informado é o físico total. Reservas não são apagadas. Alterações posteriores à prévia impedem sobrescrita do produto.</p>{(error||list.error||detail.error)&&<p role="alert">{error||list.error||detail.error}</p>}
 <form className="form-card" onSubmit={e=>{e.preventDefault();setBusy(true);void post<Preview>('/v1/catalog-imports',{csv}).then(r=>{setId(r.job.id);setRevision(v=>v+1);}).catch(e=>setError(e.message)).finally(()=>setBusy(false));}}><label>Arquivo CSV<input type="file" accept=".csv,text/csv" onChange={e=>{const file=e.target.files?.[0];if(file){if(file.size>200000){setError('Arquivo muito grande.');return;}void file.text().then(setCsv).catch(()=>setError('Não foi possível ler o arquivo.'));}}}/></label><label>Conteúdo para prévia<textarea required maxLength={200000} rows={8} value={csv} onChange={e=>setCsv(e.target.value)}/></label><button disabled={busy}>Validar sem alterar produtos</button></form>
 <button onClick={()=>setRevision(v=>v+1)}>Atualizar andamento</button>{list.data?.map(j=><button key={j.id} className="form-card" onClick={()=>setId(j.id)}>Lote {j.id.slice(0,8)} · {label(j.state)}</button>)}
 <nav className="pagination"><button disabled={page===1} onClick={()=>setPage(p=>p-1)}>Anterior</button><span>Página {page}</span><button disabled={(list.data?.length||0)<20} onClick={()=>setPage(p=>p+1)}>Próxima</button></nav>
 {detail.data&&<section className="form-card"><h2>Prévia e resultado: {label(detail.data.job.state)}</h2><p>{detail.data.rows.filter(r=>!r.expectedProductId).length} criações · {detail.data.rows.filter(r=>r.expectedProductId).length} atualizações propostas</p>{detail.data.job.state==='Preview'&&<button disabled={busy||detail.data.rows.some(r=>r.state==='Invalid')} onClick={()=>{setBusy(true);void post('/v1/catalog-imports/'+id+'/queue').then(()=>setRevision(v=>v+1)).catch(e=>setError(e.message)).finally(()=>setBusy(false));}}>Confirmar execução deste lote</button>}<div className="table-scroll"><table><thead><tr><th>Linha</th><th>SKU / Nome</th><th>Preço</th><th>Físico</th><th>Resultado</th></tr></thead><tbody>{detail.data.rows.map(r=><tr key={r.id}><td>{r.lineNumber}</td><td>{r.sku} / {r.name}</td><td>{money(r.price)}</td><td>{r.physicalStock}</td><td>{label(r.state)} {r.error}</td></tr>)}</tbody></table></div></section>}
 </main></BackofficeLayout>;
}
