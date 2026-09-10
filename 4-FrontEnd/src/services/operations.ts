export const fulfillmentLabels: Record<string,string>={Unstarted:'A iniciar',Processing:'Em processamento',Picking:'Em separação',Packed:'Embalado',PartiallyShipped:'Parcialmente enviado',Shipped:'Enviado',Delivered:'Entregue',Dispatched:'Postado',InTransit:'Em trânsito',DeliveryFailed:'Falha de entrega',Requested:'Solicitado',Approved:'Aprovado',Rejected:'Recusado',Inspected:'Inspecionado',Note:'Nota interna',Dispatch:'Registrar postagem',Unpaid:'Não pago',Paid:'Pago',PaidReview:'Pago — análise necessária',Review:'Em análise financeira',Refunded:'Estornado',Preview:'Prévia',Pending:'Pendente',Queued:'Na fila',Completed:'Concluído',CompletedWithErrors:'Concluído com erros',Invalid:'Inválido',Failed:'Falha',ReturnRequested:'Devolução solicitada',ReturnApproved:'Devolução aprovada',ReturnRejected:'Devolução recusada',ReturnInspected:'Inspeção concluída',Tracking:'Atualização de rastreio'};
export const label=(s:string)=>fulfillmentLabels[s]||s;
export interface OperationDetail {
 id:number; version:number; financialState:string; fulfillmentState:string; fulfillmentBlocked:boolean; allowedActions:string[];
 items:{id:number;productId:number;sku:string;productName:string;quantity:number;unitPrice:number}[];
 shipments:{id:string;carrier:string;service:string;trackingCode:string;volumes:number;state:string}[];
 shipmentItems:{shipmentId:string;orderItemId:number;quantity:number}[];
 returns:{id:string;reason:string;decision:string;state:string}[];
 returnItems:{returnCaseId:string;orderItemId:number;quantity:number;restockedQuantity:number}[];
 timeline:{id:string;kind:string;previousState:string;newState:string;notes:string;actor?:string;createdAt:string}[];
}
