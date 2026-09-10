import {useState} from 'react';
import {BackofficeLayout} from '../../layouts/BackofficeLayout';
import {useResource} from '../../services/useResource';
import {post} from '../../services/http';
export function ReviewModeration(){
 const [revision,setRevision]=useState(0);const [page,setPage]=useState(1);const [state,setState]=useState('Pending');const [reason,setReason]=useState('');const [error,setError]=useState('');const [busy,setBusy]=useState(false);
 const result=useResource<{id:string;productId:number;rating:number;content:string;version:number}[]>(`/v1/engagement/reviews?page=${page}&state=${state}`,revision);
 return <BackofficeLayout><main className="commerce-admin"><h1>Moderação de avaliações</h1><p>Avaliações negativas seguem os mesmos critérios das positivas. Registre o motivo de cada decisão.</p><label>Fila<select value={state} onChange={e=>{setState(e.target.value);setPage(1);}}><option value="Pending">Pendentes</option><option value="Approved">Publicadas</option><option value="Rejected">Recusadas</option></select></label><label>Motivo da decisão<textarea maxLength={1000} value={reason} onChange={e=>setReason(e.target.value)}/></label>{(error||result.error)&&<p role="alert">{error||result.error}</p>}
 {result.data?.map(r=><section key={r.id} className="form-card"><h2>Produto #{r.productId} · Nota {r.rating}/5</h2><p style={{whiteSpace:'pre-wrap'}}>{r.content}</p>{['Approved','Rejected'].filter(x=>x!==state).map(decision=><button key={decision} disabled={busy||!reason.trim()} onClick={()=>{setBusy(true);void post('/v1/engagement/reviews/'+r.id,{version:r.version,state:decision,reason}).then(()=>{setRevision(v=>v+1);setReason('');}).catch(e=>setError(e.message)).finally(()=>setBusy(false));}}>{decision==='Approved'?'Publicar':'Recusar publicação'}</button>)}</section>)}
 {!result.loading&&!result.data?.length&&<p>Nenhuma avaliação nesta página.</p>}<nav className="pagination"><button disabled={page===1} onClick={()=>setPage(p=>p-1)}>Anterior</button><span>Página {page}</span><button disabled={(result.data?.length||0)<20} onClick={()=>setPage(p=>p+1)}>Próxima</button></nav>
 </main></BackofficeLayout>;
}
