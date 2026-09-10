import {useRef,useState} from 'react';
import {StoreLayout} from '../layouts/StoreLayout';
import {BackofficeLayout} from '../layouts/BackofficeLayout';
import {useResource} from '../services/useResource';
import {post} from '../services/http';
type Ticket={id:string;subject:string;category:string;state:string;version:number;orderId?:number};
type TicketDetail={ticket:Ticket;messages:{id:string;content:string;sender:string;createdAt:string}[]};
export function SupportPage({admin=false}:{admin?:boolean}){
 const [version,setVersion]=useState(0);const [page,setPage]=useState(1);const [id,setId]=useState<string>();const [subject,setSubject]=useState('');const [category,setCategory]=useState('Question');const [content,setContent]=useState('');const [order,setOrder]=useState('');const [error,setError]=useState('');const [busy,setBusy]=useState(false);
 const key=useRef(crypto.randomUUID());const base=admin?'/v1/engagement/support':'/v1/store/support';const list=useResource<Ticket[]>(base+'?page='+page,version);
 const detail=useResource<TicketDetail>(id?base+'/'+id:null,version);
 async function submit(close=false){setBusy(true);setError('');try{const r=await post<TicketDetail>(id?base+'/'+id+'/messages':base,id?{key:key.current,version:detail.data?.ticket.version,content,close}:{key:key.current,subject,category,orderId:order?Number(order):null,content});key.current=crypto.randomUUID();setId(r.ticket.id);setContent('');setVersion(v=>v+1);}catch(e){setError((e as Error).message);}finally{setBusy(false);}}
 const Layout=admin?BackofficeLayout:StoreLayout;
 return <Layout><main className="commerce-admin"><h1>{admin?'Central de atendimento':'Ajuda e atendimento'}</h1><p>Acompanhe respostas pelo protocolo nesta página. Solicitações de privacidade passam por análise e não removem dados automaticamente.</p>
 {(error||list.error||detail.error)&&<p role="alert" className="notice error">{error||list.error||detail.error}</p>}
 <div className="finance-list">{list.data?.map(t=><button key={t.id} className="form-card" onClick={()=>{setId(t.id);setContent('');key.current=crypto.randomUUID();}}>{t.subject} · {t.state==='Closed'?'Encerrado':'Aberto'}<br/>Protocolo {t.id.slice(0,8)}</button>)}</div>
 {!list.loading&&!list.data?.length&&<p>Nenhum protocolo nesta página.</p>}<nav className="pagination"><button disabled={page===1} onClick={()=>setPage(p=>p-1)}>Anterior</button><span>Página {page}</span><button disabled={(list.data?.length||0)<20} onClick={()=>setPage(p=>p+1)}>Próxima</button></nav>
 {id&&<button onClick={()=>{setId(undefined);setContent('');key.current=crypto.randomUUID();}}>Fechar protocolo</button>}
 {detail.data&&id&&<section className="form-card"><h2>{detail.data.ticket.subject}</h2>{detail.data.messages.map(m=><article key={m.id}><strong>{m.sender==='Support'?'Atendimento':'Cliente'}</strong><p style={{whiteSpace:'pre-wrap'}}>{m.content}</p></article>)}</section>}
 {(!admin||id)&&<form className="form-card" onSubmit={e=>{e.preventDefault();void submit();}}><h2>{id?'Responder ao protocolo':'Abrir solicitação'}</h2>{!id&&<><label>Assunto<input required maxLength={150} value={subject} onChange={e=>setSubject(e.target.value)}/></label><label>Categoria<select value={category} onChange={e=>setCategory(e.target.value)}><option value="Question">Dúvida</option><option value="Order">Pedido</option><option value="Privacy">Privacidade e dados pessoais</option></select></label><label>Pedido relacionado (opcional)<input type="number" min="1" value={order} onChange={e=>setOrder(e.target.value)}/></label></>}
 <label>Mensagem<textarea required maxLength={4000} value={content} onChange={e=>setContent(e.target.value)}/></label><button disabled={busy||(!!id&&!detail.data)}>Enviar mensagem</button>{admin&&id&&<button type="button" disabled={busy||!content.trim()} onClick={()=>void submit(true)}>Responder e encerrar</button>}</form>}
 </main></Layout>;
}
