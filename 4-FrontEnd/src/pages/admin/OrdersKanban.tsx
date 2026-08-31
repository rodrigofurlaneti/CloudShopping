import { useEffect, useMemo, useState, type DragEvent, type FormEvent } from 'react';
import { BackofficeLayout } from '../../layouts/BackofficeLayout';
import {
    OrderService,
    OrderSectorService,
    OrderStatusService,
    SYSTEM_ORDER_STATUS,
    type OrderSummary,
    type OrderDetail,
    type OrderSector,
    type OrderStatus,
} from '../../services/api';
import {
    ChevronRight,
    Search,
    X,
    AlertTriangle,
    CheckCircle2,
    RefreshCw,
    Package,
    User,
    CreditCard,
    MapPin,
    XCircle,
    RotateCcw,
    Ban,
    ArrowRight,
    Filter,
    GripVertical,
    Warehouse,
    Flag,
} from 'lucide-react';

// --- Helpers ------------------------------------------------------------

type ViewMode = 'sector' | 'status';

// Paleta cíclica para o topo colorido de cada coluna no modo "Por Setor" — aplicada
// por posição na lista de setores, não pelo id bruto, já que um tenant pode ter
// setores customizados com ids em qualquer ordem.
const SECTOR_PALETTE = ['#3b82f6', '#f59e0b', '#10b981', '#8b5cf6', '#14b8a6', '#ef4444', '#0ea5e9', '#d946ef'];
const FALLBACK_COLUMN_COLOR = '#64748b';

function sectorColor(index: number): string {
    if (index < 0) return FALLBACK_COLUMN_COLOR;
    return SECTOR_PALETTE[index % SECTOR_PALETTE.length];
}

// Cor por status específico do sistema (1-16, ver SYSTEM_ORDER_STATUS) — usada tanto
// no selo de status de cada card quanto no topo das colunas no modo "Por Status".
// Status customizados por tenant (id > 16) caem no fallback cinza.
const STATUS_HEX: Record<number, string> = {
    1: '#f59e0b', // Pending
    2: '#10b981', // Paid
    3: '#3b82f6', // Invoiced
    4: '#6366f1', // Processing
    5: '#06b6d4', // Separating
    6: '#14b8a6', // Packing
    7: '#8b5cf6', // GenerateLabel
    8: '#a855f7', // ReadyToShip
    9: '#0ea5e9', // Shipped
    10: '#0284c7', // TrackingNumber
    11: '#4f46e5', // Intransit
    12: '#22c55e', // Delivered
    13: '#f97316', // DeliveryFailed
    14: '#f43f5e', // Returning
    15: '#94a3b8', // Refunded
    16: '#ef4444', // Canceled
};

const STATUS_BADGE_CLASSES: Record<number, string> = {
    1: 'bg-amber-100 text-amber-700',
    2: 'bg-emerald-100 text-emerald-700',
    3: 'bg-blue-100 text-blue-700',
    4: 'bg-indigo-100 text-indigo-700',
    5: 'bg-cyan-100 text-cyan-700',
    6: 'bg-teal-100 text-teal-700',
    7: 'bg-violet-100 text-violet-700',
    8: 'bg-purple-100 text-purple-700',
    9: 'bg-sky-100 text-sky-700',
    10: 'bg-sky-100 text-sky-700',
    11: 'bg-indigo-100 text-indigo-700',
    12: 'bg-green-100 text-green-700',
    13: 'bg-orange-100 text-orange-700',
    14: 'bg-rose-100 text-rose-700',
    15: 'bg-slate-100 text-slate-500',
    16: 'bg-red-100 text-red-700',
};
const FALLBACK_STATUS_BADGE = 'bg-slate-100 text-slate-600';

function statusBadgeClasses(statusId: number): string {
    return STATUS_BADGE_CLASSES[statusId] ?? FALLBACK_STATUS_BADGE;
}

const PAYMENT_STATUS_LABEL: Record<number, { label: string; className: string }> = {
    1: { label: 'Processando', className: 'bg-amber-100 text-amber-700' },
    2: { label: 'Aprovado', className: 'bg-emerald-100 text-emerald-700' },
    3: { label: 'Recusado', className: 'bg-red-100 text-red-700' },
    4: { label: 'Estornado', className: 'bg-slate-100 text-slate-500' },
};

function currency(value: number) {
    return value.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
}

// Uma transição de status disponível a partir do status atual de um pedido — usada
// tanto pelos botões de ação no drawer quanto para validar destinos de arrastar-e-soltar
// entre colunas do Kanban (em qualquer um dos dois modos de visualização). resultStatusId
// é o que permite ao drop handler descobrir se soltar um card na coluna X corresponde a
// uma transição de fato permitida pelas regras de guarda do Order.cs.
interface OrderTransition {
    key: string;
    label: string;
    resultStatusId: number;
    run: () => Promise<void>;
}

// Uma coluna do board, já resolvida para o modo de visualização atual — o mesmo shape
// serve tanto para "coluna = setor" quanto para "coluna = status específico", o que
// permite ter um único bloco de renderização para os dois modos.
interface BoardColumn {
    id: number;
    title: string;
    color: string;
    orders: OrderSummary[];
}

const canCancel = (statusId: number) =>
    statusId < SYSTEM_ORDER_STATUS.Shipped || statusId > SYSTEM_ORDER_STATUS.Delivered;

// Transições de próxima etapa disponíveis para cada status padrão do sistema, seguindo
// exatamente as regras de guarda de Order.cs (cada comando só é aceito a partir de status
// específicos). Status customizados por tenant (id > 16) não têm transições automáticas.
// Devolução (Returning) fica de fora: exige um motivo digitado, então só é oferecida como
// formulário no drawer, nunca por arrasto.
function availableTransitions(order: OrderSummary): OrderTransition[] {
    const S = SYSTEM_ORDER_STATUS;
    const list: OrderTransition[] = [];
    switch (order.orderStatusId) {
        case S.Paid:
            list.push({ key: 'invoice', label: 'Emitir Nota Fiscal', resultStatusId: S.Invoiced, run: () => OrderService.markAsInvoiced(order.orderId) });
            list.push({ key: 'process', label: 'Iniciar Processamento', resultStatusId: S.Processing, run: () => OrderService.startProcessing(order.orderId) });
            break;
        case S.Invoiced:
            list.push({ key: 'process', label: 'Iniciar Processamento', resultStatusId: S.Processing, run: () => OrderService.startProcessing(order.orderId) });
            break;
        case S.Processing:
            list.push({ key: 'separate', label: 'Iniciar Separação', resultStatusId: S.Separating, run: () => OrderService.startSeparating(order.orderId) });
            break;
        case S.Separating:
            list.push({ key: 'pack', label: 'Iniciar Embalagem', resultStatusId: S.Packing, run: () => OrderService.startPacking(order.orderId) });
            break;
        case S.Packing:
            list.push({ key: 'label', label: 'Gerar Etiqueta', resultStatusId: S.GenerateLabel, run: () => OrderService.generateShippingLabel(order.orderId) });
            list.push({ key: 'ready', label: 'Pronto p/ Postagem', resultStatusId: S.ReadyToShip, run: () => OrderService.markAsReadyToShip(order.orderId) });
            break;
        case S.GenerateLabel:
            list.push({ key: 'ready', label: 'Pronto p/ Postagem', resultStatusId: S.ReadyToShip, run: () => OrderService.markAsReadyToShip(order.orderId) });
            break;
        case S.ReadyToShip:
            list.push({ key: 'ship', label: 'Despachar Pedido', resultStatusId: S.Shipped, run: () => OrderService.shipOrder(order.orderId) });
            break;
        case S.Shipped:
            list.push({ key: 'intransit', label: 'Marcar Em Trânsito', resultStatusId: S.Intransit, run: () => OrderService.markAsInTransit(order.orderId) });
            list.push({ key: 'delivered', label: 'Marcar Entregue', resultStatusId: S.Delivered, run: () => OrderService.markAsDelivered(order.orderId) });
            break;
        case S.TrackingNumber:
            list.push({ key: 'intransit', label: 'Marcar Em Trânsito', resultStatusId: S.Intransit, run: () => OrderService.markAsInTransit(order.orderId) });
            break;
        case S.Intransit:
            list.push({ key: 'delivered', label: 'Marcar Entregue', resultStatusId: S.Delivered, run: () => OrderService.markAsDelivered(order.orderId) });
            list.push({ key: 'failed', label: 'Falha na Entrega', resultStatusId: S.DeliveryFailed, run: () => OrderService.markDeliveryFailed(order.orderId) });
            break;
        default:
            break;
    }
    if (canCancel(order.orderStatusId) && order.orderStatusId !== S.Canceled && order.orderStatusId !== S.Refunded) {
        list.push({ key: 'cancel', label: 'Cancelar Pedido', resultStatusId: S.Canceled, run: () => OrderService.cancelOrder(order.orderId) });
    }
    return list;
}

// --- Card de pedido (compartilhado pelas colunas normais e pela coluna "órfã") -----

function OrderCard({
    order,
    draggableEnabled,
    isDragging,
    isBusy,
    onDragStart,
    onDragEnd,
    onOpen,
}: {
    order: OrderSummary;
    draggableEnabled: boolean;
    isDragging: boolean;
    isBusy: boolean;
    onDragStart: (e: DragEvent<HTMLButtonElement>) => void;
    onDragEnd: () => void;
    onOpen: () => void;
}) {
    return (
        <button
            draggable={draggableEnabled && !isBusy}
            onDragStart={onDragStart}
            onDragEnd={onDragEnd}
            onClick={onOpen}
            disabled={isBusy}
            className={`w-full text-left bg-white rounded-lg border border-slate-200 p-3 shadow-sm hover:shadow-md hover:border-blue-300 transition-all select-none ${
                isDragging ? 'opacity-40' : ''
            } ${isBusy ? 'opacity-60 cursor-wait' : draggableEnabled ? 'cursor-grab active:cursor-grabbing' : ''}`}
        >
            <div className="flex items-center justify-between mb-1.5">
                <span className="text-sm font-bold text-blue-600">#{order.orderId}</span>
                <div className="flex items-center gap-1.5">
                    <span className={`text-[11px] font-semibold px-2 py-0.5 rounded-full ${statusBadgeClasses(order.orderStatusId)}`}>
                        {order.statusName}
                    </span>
                    {draggableEnabled && <GripVertical size={13} className="text-slate-300 shrink-0" />}
                </div>
            </div>
            <p className="text-sm font-semibold text-slate-800 truncate">
                {order.customerDisplayName ?? order.customerEmail ?? `Cliente #${order.customerId}`}
            </p>
            <div className="flex items-center justify-between mt-2 text-xs text-slate-500">
                <span>{order.totalItems} item(ns)</span>
                <span className="font-bold text-slate-700">{currency(order.totalAmount)}</span>
            </div>
            <p className="text-[11px] text-slate-400 mt-1">{new Date(order.orderDate).toLocaleDateString('pt-BR')}</p>
        </button>
    );
}

// --- Página ---------------------------------------------------------------

export function OrdersKanban() {
    const [orders, setOrders] = useState<OrderSummary[]>([]);
    const [sectors, setSectors] = useState<OrderSector[]>([]);
    const [statuses, setStatuses] = useState<OrderStatus[]>([]);
    const [loading, setLoading] = useState(true);
    const [loadError, setLoadError] = useState('');
    const [searchTerm, setSearchTerm] = useState('');
    const [statusFilter, setStatusFilter] = useState<number | ''>('');
    const [hiddenSectorIds, setHiddenSectorIds] = useState<Set<number>>(new Set());
    const [viewMode, setViewMode] = useState<ViewMode>('sector');

    const [selectedOrder, setSelectedOrder] = useState<OrderSummary | null>(null);
    const [detail, setDetail] = useState<OrderDetail | null>(null);
    const [detailLoading, setDetailLoading] = useState(false);
    const [actionError, setActionError] = useState('');
    const [runningAction, setRunningAction] = useState('');

    const [trackingNumber, setTrackingNumber] = useState('');
    const [returnReason, setReturnReason] = useState('');
    const [pendingPaymentMethod, setPendingPaymentMethod] = useState('Pix');
    const [pendingPaymentAmount, setPendingPaymentAmount] = useState('');

    // Arrastar e soltar entre colunas
    const [draggingOrderId, setDraggingOrderId] = useState<number | null>(null);
    const [dragOverColumnId, setDragOverColumnId] = useState<number | null>(null);
    const [busyOrderId, setBusyOrderId] = useState<number | null>(null);
    const [boardMessage, setBoardMessage] = useState<{ type: 'success' | 'error'; text: string } | null>(null);

    const statusById = useMemo(() => {
        const map = new Map<number, OrderStatus>();
        statuses.forEach((s) => map.set(s.id, s));
        return map;
    }, [statuses]);

    const sortedSectors = useMemo(() => [...sectors].sort((a, b) => a.id - b.id), [sectors]);
    const sortedStatuses = useMemo(() => [...statuses].sort((a, b) => a.id - b.id), [statuses]);

    const sectorColorById = useMemo(() => {
        const map = new Map<number, string>();
        sortedSectors.forEach((sector, index) => map.set(sector.id, sectorColor(index)));
        return map;
    }, [sortedSectors]);

    async function loadBoard(statusOverride?: number | '') {
        const effectiveStatus = statusOverride !== undefined ? statusOverride : statusFilter;
        setLoading(true);
        setLoadError('');
        try {
            const [orderData, sectorData, statusData] = await Promise.all([
                OrderService.getAll(1, 200, effectiveStatus === '' ? undefined : effectiveStatus),
                OrderSectorService.getAll(false),
                OrderStatusService.getAll(false),
            ]);
            setOrders(orderData.items);
            setSectors(sectorData);
            setStatuses(statusData);
        } catch (error) {
            setLoadError(error instanceof Error ? error.message : 'Não foi possível carregar os pedidos.');
        } finally {
            setLoading(false);
        }
    }

    useEffect(() => {
        loadBoard();
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    function handleStatusFilterChange(value: string) {
        const parsed: number | '' = value === '' ? '' : Number(value);
        setStatusFilter(parsed);
        loadBoard(parsed);
    }

    function toggleSector(sectorId: number) {
        setHiddenSectorIds((prev) => {
            const next = new Set(prev);
            if (next.has(sectorId)) {
                next.delete(sectorId);
            } else {
                next.add(sectorId);
            }
            return next;
        });
    }

    // Busca por texto (id / e-mail / nome do cliente) — independe do modo de visualização.
    const searchFilteredOrders = useMemo(() => {
        const term = searchTerm.trim().toLowerCase();
        if (!term) return orders;
        return orders.filter(
            (o) =>
                String(o.orderId).includes(term) ||
                (o.customerEmail?.toLowerCase().includes(term) ?? false) ||
                (o.customerDisplayName?.toLowerCase().includes(term) ?? false)
        );
    }, [orders, searchTerm]);

    // Contagem por setor para os chips de filtro — não considera hiddenSectorIds (senão o
    // próprio chip perderia a contagem ao ser ocultado), mas considera a busca por texto.
    const sectorCounts = useMemo(() => {
        const map = new Map<number, number>();
        searchFilteredOrders.forEach((order) => {
            const sectorId = statusById.get(order.orderStatusId)?.orderSectorId;
            if (sectorId == null) return;
            map.set(sectorId, (map.get(sectorId) ?? 0) + 1);
        });
        return map;
    }, [searchFilteredOrders, statusById]);

    // Filtro cruzado por setor: funciona nos dois modos de visualização, então dá pra
    // ocultar um setor operacional inteiro mesmo estando na visão "Por Status".
    const filteredOrders = useMemo(() => {
        if (hiddenSectorIds.size === 0) return searchFilteredOrders;
        return searchFilteredOrders.filter((order) => {
            const sectorId = statusById.get(order.orderStatusId)?.orderSectorId;
            return sectorId == null || !hiddenSectorIds.has(sectorId);
        });
    }, [searchFilteredOrders, hiddenSectorIds, statusById]);

    const mappedFilteredOrders = useMemo(
        () => filteredOrders.filter((o) => statusById.has(o.orderStatusId)),
        [filteredOrders, statusById]
    );
    // Pedidos cujo status não corresponde a nenhum OrderStatus carregado — vão para uma
    // coluna à parte em qualquer um dos dois modos, para nunca sumirem silenciosamente.
    const orphanOrders = useMemo(
        () => filteredOrders.filter((o) => !statusById.has(o.orderStatusId)),
        [filteredOrders, statusById]
    );

    // Monta as colunas do board de acordo com o modo de visualização escolhido — a troca
    // de modo é 100% client-side (reagrupa os pedidos já carregados), sem nova chamada à API.
    const columns: BoardColumn[] = useMemo(() => {
        if (viewMode === 'sector') {
            const bySector = new Map<number, OrderSummary[]>();
            sortedSectors.forEach((s) => bySector.set(s.id, []));
            mappedFilteredOrders.forEach((order) => {
                const sectorId = statusById.get(order.orderStatusId)!.orderSectorId;
                if (!bySector.has(sectorId)) bySector.set(sectorId, []);
                bySector.get(sectorId)!.push(order);
            });
            for (const list of bySector.values()) {
                list.sort((a, b) => a.orderStatusId - b.orderStatusId || a.orderId - b.orderId);
            }
            return sortedSectors
                .filter((sector) => !hiddenSectorIds.has(sector.id))
                .map((sector) => ({
                    id: sector.id,
                    title: sector.name,
                    color: sectorColorById.get(sector.id) ?? FALLBACK_COLUMN_COLOR,
                    orders: bySector.get(sector.id) ?? [],
                }));
        }

        const byStatus = new Map<number, OrderSummary[]>();
        sortedStatuses.forEach((s) => byStatus.set(s.id, []));
        mappedFilteredOrders.forEach((order) => {
            if (!byStatus.has(order.orderStatusId)) byStatus.set(order.orderStatusId, []);
            byStatus.get(order.orderStatusId)!.push(order);
        });
        for (const list of byStatus.values()) {
            list.sort((a, b) => a.orderId - b.orderId);
        }
        return sortedStatuses.map((status) => ({
            id: status.id,
            title: status.name,
            color: STATUS_HEX[status.id] ?? FALLBACK_COLUMN_COLOR,
            orders: byStatus.get(status.id) ?? [],
        }));
    }, [viewMode, sortedSectors, sortedStatuses, mappedFilteredOrders, statusById, hiddenSectorIds, sectorColorById]);

    const stats = useMemo(() => {
        const total = orders.length;
        const totalValue = orders.reduce((sum, o) => sum + o.totalAmount, 0);
        const canceled = orders.filter((o) => o.orderStatusId === SYSTEM_ORDER_STATUS.Canceled).length;
        const delivered = orders.filter((o) => o.orderStatusId === SYSTEM_ORDER_STATUS.Delivered).length;
        return { total, totalValue, canceled, delivered };
    }, [orders]);

    async function openDetail(order: OrderSummary) {
        setSelectedOrder(order);
        setDetail(null);
        setActionError('');
        setTrackingNumber('');
        setReturnReason('');
        setPendingPaymentAmount('');
        setDetailLoading(true);
        try {
            const data = await OrderService.getById(order.orderId, order.customerId);
            setDetail(data);
        } catch (error) {
            setActionError(error instanceof Error ? error.message : 'Não foi possível carregar o pedido.');
        } finally {
            setDetailLoading(false);
        }
    }

    function closeDetail() {
        setSelectedOrder(null);
        setDetail(null);
    }

    async function runAction(key: string, action: () => Promise<void>) {
        setActionError('');
        setRunningAction(key);
        try {
            await action();
            if (selectedOrder) {
                const refreshed = await OrderService.getById(selectedOrder.orderId, selectedOrder.customerId);
                setDetail(refreshed);
            }
            await loadBoard();
        } catch (error) {
            setActionError(error instanceof Error ? error.message : 'Não foi possível concluir a ação.');
        } finally {
            setRunningAction('');
        }
    }

    async function handleSetTracking(e: FormEvent) {
        e.preventDefault();
        if (!selectedOrder || !trackingNumber.trim()) {
            setActionError('Informe o código de rastreio.');
            return;
        }
        await runAction('tracking', () => OrderService.setTrackingNumber(selectedOrder.orderId, trackingNumber.trim()));
    }

    async function handleRequestReturn(e: FormEvent) {
        e.preventDefault();
        if (!selectedOrder || !returnReason.trim()) {
            setActionError('Informe o motivo da devolução.');
            return;
        }
        await runAction('return', () => OrderService.requestReturn(selectedOrder.orderId, returnReason.trim()));
    }

    async function handleAddPendingPayment(e: FormEvent) {
        e.preventDefault();
        if (!selectedOrder) return;
        const amount = Number(pendingPaymentAmount.replace(',', '.'));
        if (!pendingPaymentMethod.trim() || !(amount > 0)) {
            setActionError('Informe o método e um valor válido para o pagamento.');
            return;
        }
        await runAction('add-payment', () =>
            OrderService.addPendingPayment(selectedOrder.orderId, pendingPaymentMethod.trim(), amount)
        );
        setPendingPaymentAmount('');
    }

    // Executa uma transição disparada pelo drawer (botão) ou pelo drop de um card — ambos
    // compartilham a mesma lógica de chamada de API + feedback + reload do board.
    async function performTransition(order: OrderSummary, transition: OrderTransition) {
        setBusyOrderId(order.orderId);
        setBoardMessage(null);
        try {
            await transition.run();
            setBoardMessage({ type: 'success', text: `Pedido #${order.orderId} movido para "${transition.label}".` });
            await loadBoard();
            if (selectedOrder?.orderId === order.orderId) {
                const refreshed = await OrderService.getById(order.orderId, order.customerId);
                setDetail(refreshed);
            }
        } catch (error) {
            setBoardMessage({
                type: 'error',
                text: error instanceof Error ? error.message : `Não foi possível mover o pedido #${order.orderId}.`,
            });
        } finally {
            setBusyOrderId(null);
        }
    }

    // No modo "Por Setor" uma coluna pode conter vários status (ex.: Armazém agrupa
    // Processing/Separating/Packing), então soltar um card ali casa com QUALQUER transição
    // cujo resultado caia naquele setor. No modo "Por Status" a coluna É o status exato, então
    // a checagem é uma igualdade direta — mais precisa, sem ambiguidade nenhuma.
    function isOrderInColumn(order: OrderSummary, column: BoardColumn): boolean {
        if (viewMode === 'sector') {
            return (statusById.get(order.orderStatusId)?.orderSectorId ?? -1) === column.id;
        }
        return order.orderStatusId === column.id;
    }

    function findMatchingTransition(order: OrderSummary, column: BoardColumn): OrderTransition | undefined {
        const transitions = availableTransitions(order);
        if (viewMode === 'sector') {
            return transitions.find((t) => statusById.get(t.resultStatusId)?.orderSectorId === column.id);
        }
        return transitions.find((t) => t.resultStatusId === column.id);
    }

    function handleCardDragStart(e: DragEvent<HTMLButtonElement>, order: OrderSummary) {
        e.dataTransfer.effectAllowed = 'move';
        e.dataTransfer.setData('text/plain', String(order.orderId));
        setDraggingOrderId(order.orderId);
    }

    function handleCardDragEnd() {
        setDraggingOrderId(null);
        setDragOverColumnId(null);
    }

    function handleColumnDragOver(e: DragEvent<HTMLDivElement>, columnId: number) {
        e.preventDefault();
        e.dataTransfer.dropEffect = 'move';
        if (dragOverColumnId !== columnId) setDragOverColumnId(columnId);
    }

    function handleColumnDragLeave(columnId: number) {
        setDragOverColumnId((prev) => (prev === columnId ? null : prev));
    }

    function handleColumnDrop(e: DragEvent<HTMLDivElement>, column: BoardColumn) {
        e.preventDefault();
        setDragOverColumnId(null);
        const orderId = draggingOrderId;
        setDraggingOrderId(null);
        if (orderId == null) return;

        const order = orders.find((o) => o.orderId === orderId);
        if (!order) return;
        if (isOrderInColumn(order, column)) return;

        const match = findMatchingTransition(order, column);
        if (!match) {
            setBoardMessage({
                type: 'error',
                text: `O pedido #${order.orderId} não pode ir direto para "${column.title}" a partir do status "${order.statusName}". Abra o pedido para ver as próximas etapas válidas.`,
            });
            return;
        }
        performTransition(order, match);
    }

    return (
        <BackofficeLayout>
            {/* Breadcrumb */}
            <div className="text-sm text-slate-500 mb-2 flex items-center gap-1.5">
                <span>Início</span>
                <ChevronRight size={14} />
                <span className="text-slate-700 font-medium">Pedidos</span>
            </div>

            <div className="flex items-start justify-between mb-4 gap-4 flex-wrap">
                <div>
                    <h1 className="text-2xl font-bold text-slate-900">Kanban de Gestão de Pedidos</h1>
                    <p className="text-sm text-slate-500 mt-1">
                        Arraste um pedido para avançar a etapa, ou abra o card para ver detalhes e ações
                    </p>
                </div>

                <button
                    onClick={() => loadBoard()}
                    className="flex items-center gap-2 bg-white border border-slate-200 hover:bg-slate-50 text-slate-600 text-sm font-semibold px-4 py-2.5 rounded-lg shadow-sm transition-colors"
                >
                    <RefreshCw size={16} /> Atualizar
                </button>
            </div>

            {/* Alternador de modo de visualização */}
            <div className="flex items-center gap-3 mb-6 flex-wrap">
                <div className="inline-flex items-center bg-slate-100 rounded-lg p-1 gap-1">
                    <button
                        onClick={() => setViewMode('sector')}
                        className={`flex items-center gap-1.5 text-sm font-semibold px-3.5 py-1.5 rounded-md transition-all ${
                            viewMode === 'sector' ? 'bg-white text-slate-900 shadow-sm' : 'text-slate-500 hover:text-slate-700'
                        }`}
                    >
                        <Warehouse size={15} /> Por Setor
                    </button>
                    <button
                        onClick={() => setViewMode('status')}
                        className={`flex items-center gap-1.5 text-sm font-semibold px-3.5 py-1.5 rounded-md transition-all ${
                            viewMode === 'status' ? 'bg-white text-slate-900 shadow-sm' : 'text-slate-500 hover:text-slate-700'
                        }`}
                    >
                        <Flag size={15} /> Por Status
                    </button>
                </div>
                <span className="text-xs text-slate-400">
                    {viewMode === 'sector'
                        ? 'Colunas agrupadas pelos setores operacionais (Armazém, Expedição...)'
                        : 'Colunas para cada status específico do pedido'}
                </span>
            </div>

            {/* Stat cards */}
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
                <div className="bg-white rounded-xl border border-slate-200 p-5 shadow-sm">
                    <div className="flex items-center gap-3 mb-3">
                        <div className="w-10 h-10 rounded-lg flex items-center justify-center bg-blue-100">
                            <Package size={20} className="text-blue-600" />
                        </div>
                        <p className="text-sm text-slate-500">Pedidos no quadro</p>
                    </div>
                    <p className="text-2xl font-bold text-slate-900">{stats.total}</p>
                </div>

                <div className="bg-white rounded-xl border border-slate-200 p-5 shadow-sm">
                    <div className="flex items-center gap-3 mb-3">
                        <div className="w-10 h-10 rounded-lg flex items-center justify-center bg-emerald-100">
                            <CreditCard size={20} className="text-emerald-600" />
                        </div>
                        <p className="text-sm text-slate-500">Valor total</p>
                    </div>
                    <p className="text-2xl font-bold text-slate-900">{currency(stats.totalValue)}</p>
                </div>

                <div className="bg-white rounded-xl border border-slate-200 p-5 shadow-sm">
                    <div className="flex items-center gap-3 mb-3">
                        <div className="w-10 h-10 rounded-lg flex items-center justify-center bg-emerald-100">
                            <CheckCircle2 size={20} className="text-emerald-600" />
                        </div>
                        <p className="text-sm text-slate-500">Entregues</p>
                    </div>
                    <p className="text-2xl font-bold text-slate-900">{stats.delivered}</p>
                </div>

                <div className="bg-white rounded-xl border border-slate-200 p-5 shadow-sm">
                    <div className="flex items-center gap-3 mb-3">
                        <div className="w-10 h-10 rounded-lg flex items-center justify-center bg-red-100">
                            <XCircle size={20} className="text-red-600" />
                        </div>
                        <p className="text-sm text-slate-500">Cancelados</p>
                    </div>
                    <p className="text-2xl font-bold text-slate-900">{stats.canceled}</p>
                </div>
            </div>

            {/* Filtros */}
            <div className="flex items-center gap-3 mb-3 flex-wrap">
                <div className="relative max-w-sm flex-1 min-w-[220px]">
                    <Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
                    <input
                        type="text"
                        value={searchTerm}
                        onChange={(e) => setSearchTerm(e.target.value)}
                        placeholder="Buscar por # do pedido, cliente ou e-mail..."
                        className="w-full bg-white border border-slate-200 rounded-lg pl-9 pr-4 py-2.5 text-sm text-slate-700 placeholder-slate-400 outline-none focus:ring-2 focus:ring-blue-500"
                    />
                </div>

                <div className="relative">
                    <Filter size={14} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
                    <select
                        value={statusFilter === '' ? '' : String(statusFilter)}
                        onChange={(e) => handleStatusFilterChange(e.target.value)}
                        className="bg-white border border-slate-200 rounded-lg pl-8 pr-8 py-2.5 text-sm text-slate-700 outline-none focus:ring-2 focus:ring-blue-500 appearance-none"
                    >
                        <option value="">Todos os status</option>
                        {sortedStatuses.map((status) => (
                            <option key={status.id} value={status.id}>
                                {status.name}
                            </option>
                        ))}
                    </select>
                </div>
            </div>

            {/* Chips de setor — filtro cruzado que funciona nos dois modos de visualização */}
            <div className="flex items-center gap-2 mb-4 flex-wrap">
                {sortedSectors.map((sector) => {
                    const color = sectorColorById.get(sector.id) ?? FALLBACK_COLUMN_COLOR;
                    const isHidden = hiddenSectorIds.has(sector.id);
                    const count = sectorCounts.get(sector.id) ?? 0;
                    return (
                        <button
                            key={sector.id}
                            onClick={() => toggleSector(sector.id)}
                            className={`flex items-center gap-1.5 text-xs font-semibold px-3 py-1.5 rounded-full border transition-all ${
                                isHidden
                                    ? 'border-slate-200 bg-slate-50 text-slate-400'
                                    : 'border-slate-200 bg-white text-slate-700 shadow-sm'
                            }`}
                        >
                            <span
                                className="w-2 h-2 rounded-full shrink-0"
                                style={{ backgroundColor: isHidden ? '#cbd5e1' : color }}
                            />
                            {sector.name} ({count})
                        </button>
                    );
                })}
            </div>

            {loadError && (
                <div className="mb-4 flex items-center gap-2 bg-red-50 border border-red-200 text-red-700 text-sm px-4 py-3 rounded-lg">
                    <AlertTriangle size={16} /> {loadError}
                </div>
            )}

            {boardMessage && (
                <div
                    className={`mb-4 flex items-center justify-between gap-2 text-sm px-4 py-3 rounded-lg border ${
                        boardMessage.type === 'success'
                            ? 'bg-emerald-50 border-emerald-200 text-emerald-700'
                            : 'bg-red-50 border-red-200 text-red-700'
                    }`}
                >
                    <span className="flex items-center gap-2">
                        {boardMessage.type === 'success' ? <CheckCircle2 size={16} /> : <AlertTriangle size={16} />}
                        {boardMessage.text}
                    </span>
                    <button onClick={() => setBoardMessage(null)} className="shrink-0 opacity-70 hover:opacity-100">
                        <X size={14} />
                    </button>
                </div>
            )}

            {loading && <p className="text-center text-slate-400 py-16">Carregando pedidos...</p>}

            {!loading && !loadError && (
                <div className="flex gap-4 overflow-x-auto pb-4">
                    {columns.map((column) => {
                        const isDragOver = dragOverColumnId === column.id;
                        return (
                            <div
                                key={`${viewMode}-${column.id}`}
                                onDragOver={(e) => handleColumnDragOver(e, column.id)}
                                onDragLeave={() => handleColumnDragLeave(column.id)}
                                onDrop={(e) => handleColumnDrop(e, column)}
                                className={`shrink-0 w-80 rounded-xl border bg-slate-50 overflow-hidden transition-all ${
                                    isDragOver ? 'border-blue-400 ring-2 ring-blue-200' : 'border-slate-200'
                                }`}
                            >
                                <div className="h-1.5" style={{ backgroundColor: column.color }} />
                                <div className="px-4 py-3 border-b border-slate-200/70 flex items-center justify-between bg-white gap-2">
                                    <h3 className="text-sm font-bold truncate" style={{ color: column.color }}>
                                        {column.title}
                                    </h3>
                                    <span
                                        className="text-xs font-bold px-2 py-0.5 rounded-full shrink-0"
                                        style={{ backgroundColor: `${column.color}22`, color: column.color }}
                                    >
                                        {column.orders.length}
                                    </span>
                                </div>
                                <div className="p-3 space-y-3 max-h-[65vh] overflow-y-auto">
                                    {column.orders.length === 0 && (
                                        <p className="text-xs text-slate-400 text-center py-6">
                                            {isDragOver ? 'Solte aqui' : 'Nenhum pedido'}
                                        </p>
                                    )}
                                    {column.orders.map((order) => (
                                        <OrderCard
                                            key={order.orderId}
                                            order={order}
                                            draggableEnabled
                                            isDragging={draggingOrderId === order.orderId}
                                            isBusy={busyOrderId === order.orderId}
                                            onDragStart={(e) => handleCardDragStart(e, order)}
                                            onDragEnd={handleCardDragEnd}
                                            onOpen={() => openDetail(order)}
                                        />
                                    ))}
                                </div>
                            </div>
                        );
                    })}

                    {orphanOrders.length > 0 && (
                        <div className="shrink-0 w-80 rounded-xl border border-slate-200 bg-slate-50 overflow-hidden">
                            <div className="h-1.5 bg-slate-400" />
                            <div className="px-4 py-3 border-b border-slate-200/70 flex items-center justify-between bg-white">
                                <h3 className="text-sm font-bold text-slate-600">
                                    {viewMode === 'sector' ? 'Sem setor mapeado' : 'Sem status mapeado'}
                                </h3>
                                <span className="text-xs font-bold px-2 py-0.5 rounded-full bg-slate-200 text-slate-600">
                                    {orphanOrders.length}
                                </span>
                            </div>
                            <div className="p-3 space-y-3 max-h-[65vh] overflow-y-auto">
                                {orphanOrders.map((order) => (
                                    <OrderCard
                                        key={order.orderId}
                                        order={order}
                                        draggableEnabled={false}
                                        isDragging={false}
                                        isBusy={busyOrderId === order.orderId}
                                        onDragStart={() => {}}
                                        onDragEnd={() => {}}
                                        onOpen={() => openDetail(order)}
                                    />
                                ))}
                            </div>
                        </div>
                    )}
                </div>
            )}

            {/* Drawer de detalhe do pedido */}
            {selectedOrder && (
                <div className="fixed inset-0 bg-slate-900/50 flex items-center justify-center px-4 z-50">
                    <div className="bg-white rounded-2xl shadow-xl w-full max-w-lg max-h-[90vh] flex flex-col">
                        <div className="flex items-center justify-between px-6 py-4 border-b border-slate-100">
                            <h3 className="font-bold text-slate-900">Pedido #{selectedOrder.orderId}</h3>
                            <button
                                onClick={closeDetail}
                                className="w-8 h-8 rounded-lg flex items-center justify-center text-slate-400 hover:bg-slate-100"
                                aria-label="Fechar"
                            >
                                <X size={18} />
                            </button>
                        </div>

                        {detailLoading && <p className="px-6 py-10 text-center text-slate-400">Carregando...</p>}

                        {actionError && (
                            <div className="mx-6 mt-4 flex items-center gap-2 bg-red-50 border border-red-200 text-red-700 text-sm px-4 py-3 rounded-lg">
                                <AlertTriangle size={16} /> {actionError}
                            </div>
                        )}

                        {detail && !detailLoading && (
                            <div className="px-6 py-5 overflow-y-auto space-y-5">
                                <div className="flex items-center justify-between">
                                    <div className="flex items-center gap-2 text-sm text-slate-600">
                                        <User size={14} />
                                        {selectedOrder.customerDisplayName ?? selectedOrder.customerEmail ?? `Cliente #${selectedOrder.customerId}`}
                                    </div>
                                    <span className={`text-xs font-semibold px-2.5 py-1 rounded-full ${statusBadgeClasses(detail.orderStatusId)}`}>
                                        {statusById.get(detail.orderStatusId)?.name ?? selectedOrder.statusName}
                                    </span>
                                </div>

                                {detail.address && (
                                    <div className="flex items-start gap-2 text-sm text-slate-500">
                                        <MapPin size={14} className="mt-0.5 shrink-0" />
                                        <span>
                                            {detail.address.street}, {detail.address.number}
                                            {detail.address.neighborhood ? ` - ${detail.address.neighborhood}` : ''} —{' '}
                                            {detail.address.city}/{detail.address.state}, {detail.address.zipCode}
                                        </span>
                                    </div>
                                )}

                                {/* Itens */}
                                <div>
                                    <p className="text-xs font-semibold uppercase tracking-wider text-slate-500 mb-2">
                                        Itens ({detail.items.length})
                                    </p>
                                    <div className="border border-slate-200 rounded-lg divide-y divide-slate-100">
                                        {detail.items.map((item, idx) => (
                                            <div key={idx} className="flex items-center justify-between px-3 py-2 text-sm">
                                                <span className="text-slate-600">
                                                    Produto #{item.productId} &times; {item.quantity}
                                                </span>
                                                <span className="font-semibold text-slate-800">
                                                    {currency(item.unitPrice * item.quantity)}
                                                </span>
                                            </div>
                                        ))}
                                        {detail.items.length === 0 && (
                                            <p className="px-3 py-4 text-center text-xs text-slate-400">Nenhum item.</p>
                                        )}
                                    </div>
                                    <p className="text-right text-sm font-bold text-slate-800 mt-2">
                                        Total: {currency(detail.totalAmount)}
                                    </p>
                                </div>

                                {/* Pagamentos */}
                                <div>
                                    <p className="text-xs font-semibold uppercase tracking-wider text-slate-500 mb-2">
                                        Pagamentos ({detail.payments.length})
                                    </p>
                                    <div className="space-y-2">
                                        {detail.payments.map((payment) => {
                                            const info = PAYMENT_STATUS_LABEL[payment.paymentStatusId] ?? {
                                                label: `Status ${payment.paymentStatusId}`,
                                                className: 'bg-slate-100 text-slate-500',
                                            };
                                            return (
                                                <div
                                                    key={payment.paymentId}
                                                    className="flex items-center justify-between border border-slate-200 rounded-lg px-3 py-2"
                                                >
                                                    <div className="text-sm">
                                                        <p className="font-semibold text-slate-800">{payment.paymentMethod}</p>
                                                        <p className="text-slate-500">{currency(payment.amount)}</p>
                                                    </div>
                                                    <div className="flex items-center gap-2">
                                                        <span className={`text-[11px] font-semibold px-2 py-0.5 rounded-full ${info.className}`}>
                                                            {info.label}
                                                        </span>
                                                        {payment.paymentStatusId === 1 && (
                                                            <>
                                                                <button
                                                                    onClick={() =>
                                                                        runAction(`approve-${payment.paymentId}`, () =>
                                                                            OrderService.approvePayment(detail.orderId, payment.paymentId)
                                                                        )
                                                                    }
                                                                    disabled={runningAction === `approve-${payment.paymentId}`}
                                                                    title="Aprovar pagamento"
                                                                    className="w-7 h-7 rounded-lg flex items-center justify-center text-emerald-600 bg-emerald-50 hover:bg-emerald-100 disabled:opacity-40"
                                                                >
                                                                    <CheckCircle2 size={13} />
                                                                </button>
                                                                <button
                                                                    onClick={() =>
                                                                        runAction(`decline-${payment.paymentId}`, () =>
                                                                            OrderService.declinePayment(detail.orderId, payment.paymentId)
                                                                        )
                                                                    }
                                                                    disabled={runningAction === `decline-${payment.paymentId}`}
                                                                    title="Recusar pagamento"
                                                                    className="w-7 h-7 rounded-lg flex items-center justify-center text-red-600 bg-red-50 hover:bg-red-100 disabled:opacity-40"
                                                                >
                                                                    <XCircle size={13} />
                                                                </button>
                                                            </>
                                                        )}
                                                        {payment.paymentStatusId === 2 && (
                                                            <button
                                                                onClick={() =>
                                                                    runAction(`refund-${payment.paymentId}`, () =>
                                                                        OrderService.refundPayment(detail.orderId, payment.paymentId)
                                                                    )
                                                                }
                                                                disabled={runningAction === `refund-${payment.paymentId}`}
                                                                title="Estornar pagamento"
                                                                className="w-7 h-7 rounded-lg flex items-center justify-center text-slate-500 bg-slate-100 hover:bg-slate-200 disabled:opacity-40"
                                                            >
                                                                <RotateCcw size={13} />
                                                            </button>
                                                        )}
                                                    </div>
                                                </div>
                                            );
                                        })}
                                        {detail.payments.length === 0 && (
                                            <p className="text-xs text-slate-400 text-center py-3 border border-dashed border-slate-200 rounded-lg">
                                                Nenhum pagamento registrado.
                                            </p>
                                        )}
                                    </div>

                                    <form onSubmit={handleAddPendingPayment} className="flex gap-2 mt-2">
                                        <input
                                            type="text"
                                            value={pendingPaymentMethod}
                                            onChange={(e) => setPendingPaymentMethod(e.target.value)}
                                            placeholder="Método"
                                            className="w-28 bg-slate-50 border border-slate-200 rounded-lg px-3 py-2 text-sm text-slate-800 outline-none focus:ring-2 focus:ring-blue-500"
                                        />
                                        <input
                                            type="text"
                                            inputMode="decimal"
                                            value={pendingPaymentAmount}
                                            onChange={(e) => setPendingPaymentAmount(e.target.value)}
                                            placeholder="Valor"
                                            className="flex-1 bg-slate-50 border border-slate-200 rounded-lg px-3 py-2 text-sm text-slate-800 outline-none focus:ring-2 focus:ring-blue-500"
                                        />
                                        <button
                                            type="submit"
                                            disabled={runningAction === 'add-payment'}
                                            className="px-3 py-2 rounded-lg text-xs font-semibold bg-slate-800 text-white hover:bg-slate-900 disabled:opacity-60"
                                        >
                                            Registrar
                                        </button>
                                    </form>
                                </div>

                                {/* Ações de fluxo / Kanban */}
                                <div className="border-t border-slate-100 pt-4 space-y-3">
                                    <p className="text-xs font-semibold uppercase tracking-wider text-slate-500">
                                        Avançar etapa
                                    </p>

                                    <div className="flex flex-wrap gap-2">
                                        {availableTransitions(selectedOrder)
                                            .filter((t) => t.key !== 'cancel')
                                            .map((action) => (
                                                <button
                                                    key={action.key}
                                                    onClick={() => runAction(action.key, action.run)}
                                                    disabled={runningAction === action.key}
                                                    className="flex items-center gap-1.5 bg-blue-600 hover:bg-blue-700 text-white text-xs font-semibold px-3 py-2 rounded-lg shadow-sm transition-colors disabled:opacity-60"
                                                >
                                                    {runningAction === action.key ? (
                                                        <span className="inline-block w-3 h-3 border-2 border-white border-t-transparent rounded-full animate-spin" />
                                                    ) : (
                                                        <ArrowRight size={13} />
                                                    )}
                                                    {action.label}
                                                </button>
                                            ))}
                                        {availableTransitions(selectedOrder).filter((t) => t.key !== 'cancel').length === 0 && (
                                            <p className="text-xs text-slate-400">
                                                Nenhuma ação automática disponível para este status.
                                            </p>
                                        )}
                                    </div>

                                    {(detail.orderStatusId === SYSTEM_ORDER_STATUS.Shipped ||
                                        detail.orderStatusId === SYSTEM_ORDER_STATUS.ReadyToShip) && (
                                        <form onSubmit={handleSetTracking} className="flex gap-2">
                                            <input
                                                type="text"
                                                value={trackingNumber}
                                                onChange={(e) => setTrackingNumber(e.target.value)}
                                                placeholder="Código de rastreio"
                                                className="flex-1 bg-slate-50 border border-slate-200 rounded-lg px-3 py-2 text-sm text-slate-800 outline-none focus:ring-2 focus:ring-blue-500"
                                            />
                                            <button
                                                type="submit"
                                                disabled={runningAction === 'tracking'}
                                                className="px-3 py-2 rounded-lg text-xs font-semibold bg-slate-800 text-white hover:bg-slate-900 disabled:opacity-60"
                                            >
                                                Definir rastreio
                                            </button>
                                        </form>
                                    )}

                                    {detail.orderStatusId === SYSTEM_ORDER_STATUS.Delivered && (
                                        <form onSubmit={handleRequestReturn} className="flex gap-2">
                                            <input
                                                type="text"
                                                value={returnReason}
                                                onChange={(e) => setReturnReason(e.target.value)}
                                                placeholder="Motivo da devolução"
                                                className="flex-1 bg-slate-50 border border-slate-200 rounded-lg px-3 py-2 text-sm text-slate-800 outline-none focus:ring-2 focus:ring-blue-500"
                                            />
                                            <button
                                                type="submit"
                                                disabled={runningAction === 'return'}
                                                className="px-3 py-2 rounded-lg text-xs font-semibold bg-amber-600 text-white hover:bg-amber-700 disabled:opacity-60"
                                            >
                                                Solicitar devolução
                                            </button>
                                        </form>
                                    )}

                                    {canCancel(detail.orderStatusId) &&
                                        detail.orderStatusId !== SYSTEM_ORDER_STATUS.Canceled &&
                                        detail.orderStatusId !== SYSTEM_ORDER_STATUS.Refunded && (
                                            <button
                                                onClick={() => runAction('cancel', () => OrderService.cancelOrder(detail.orderId))}
                                                disabled={runningAction === 'cancel'}
                                                className="flex items-center gap-1.5 text-red-600 hover:text-red-700 text-xs font-semibold disabled:opacity-60"
                                            >
                                                <Ban size={13} /> Cancelar pedido
                                            </button>
                                        )}
                                </div>
                            </div>
                        )}
                    </div>
                </div>
            )}
        </BackofficeLayout>
    );
}

export default OrdersKanban;
