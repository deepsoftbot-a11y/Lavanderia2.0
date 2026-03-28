import { useState, useEffect } from 'react';
import { ArrowLeft, Share2 } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { OrderItemsTable } from './OrderItemsTable';
import { OrderPaymentSection } from './OrderPaymentSection';
import { OrderStatusChanger } from './OrderStatusChanger';
import { useOrdersStore } from '@/features/orders/stores/ordersStore';
import { usePaymentMethodsStore } from '@/features/orders/stores/paymentMethodsStore';
import { useToast } from '@/shared/hooks/use-toast';
import { createPayment } from '@/api/payments';
import { mockOrderStatuses } from '@/api/orderStatuses';
import { formatDate, formatPhoneNumber } from '@/shared/utils/formatters';
import { formatOrderWhatsAppMessage, openWhatsAppShare } from '@/features/orders/utils/shareOrder';
import type { Order } from '@/features/orders/types/order';
import type { PaymentFormData } from '@/features/orders/components/payments/PaymentForm';

interface OrderDetailViewProps {
  order: Order;
  onBack: () => void;
}

export function OrderDetailView({ order, onBack }: OrderDetailViewProps) {
  const { toast } = useToast();
  const { updateOrderStatus, refreshSelectedOrder, isLoading } = useOrdersStore();
  const { fetchPaymentMethods } = usePaymentMethodsStore();
  const [isProcessingPayment, setIsProcessingPayment] = useState(false);
  const [localStatusId, setLocalStatusId] = useState(order.orderStatusId);
  const localStatus = mockOrderStatuses.find((s) => s.id === localStatusId) ?? order.orderStatus;

  useEffect(() => {
    fetchPaymentMethods();
  }, [fetchPaymentMethods]);

  const handleStatusChange = async (statusId: number): Promise<boolean> => {
    const result = await updateOrderStatus(order.id, statusId);
    if (result) {
      toast({
        title: 'Estado actualizado',
        description: `La orden se actualizó a "${mockOrderStatuses.find((s) => s.id === statusId)?.name}"`,
      });
      setLocalStatusId(statusId);
      return true;
    } else {
      toast({
        title: 'Error',
        description: 'No se pudo actualizar el estado de la orden',
        variant: 'destructive',
      });
      return false;
    }
  };

  const handlePaymentSubmit = async (data: PaymentFormData) => {
    setIsProcessingPayment(true);
    try {
      await createPayment({
        orderId: order.id,
        amount: data.amount,
        paymentMethodId: data.paymentMethodId,
        paidAt: data.paidAt,
      });
      await refreshSelectedOrder();
      toast({
        title: 'Pago registrado',
        description: `Pago de $${data.amount.toFixed(2)} registrado exitosamente`,
      });
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Error al registrar el pago';
      toast({
        title: 'Error',
        description: message,
        variant: 'destructive',
      });
    } finally {
      setIsProcessingPayment(false);
    }
  };

  return (
    <div className="flex flex-col h-full">

      {/* ── Header fijo — patrón system.md ───────────────────────────── */}
      <div className="px-6 pt-5 pb-4 border-b border-zinc-100 shrink-0 flex items-center gap-3">
        <Button
          variant="ghost"
          size="sm"
          className="h-7 w-7 p-0 -ml-1 text-zinc-400 hover:text-zinc-900"
          onClick={onBack}
        >
          <ArrowLeft className="h-4 w-4" />
        </Button>
        <div className="flex-1 min-w-0">
          <p className="font-mono font-bold text-sm tracking-tight text-zinc-900">
            {order.folioOrden}
          </p>
        </div>
        <Button
          variant="outline"
          size="sm"
          className="h-7 px-2.5 text-xs text-zinc-600 hover:text-zinc-900 shrink-0"
          onClick={() => openWhatsAppShare(order.client?.phone, formatOrderWhatsAppMessage(order))}
        >
          <Share2 className="h-3 w-3 mr-1.5" />
          WhatsApp
        </Button>
        {localStatus && (
          <span
            className="inline-flex items-center px-2 py-0.5 rounded-full text-[10px] font-semibold tracking-wide leading-none shrink-0"
            style={{
              backgroundColor: `${localStatus.color}20`,
              color: localStatus.color,
            }}
          >
            {localStatus.name}
          </span>
        )}
      </div>

      {/* ── Contenido scrollable ──────────────────────────────────────── */}
      <div className="flex-1 overflow-y-auto">

        {/* CLIENTE */}
        <div className="px-6 py-4 border-b border-zinc-100">
          <p className="text-[10px] font-semibold tracking-widest uppercase text-zinc-400 mb-2.5">
            Cliente
          </p>
          <div className="grid grid-cols-2 gap-y-3">
            <div>
              <p className="text-[10px] text-zinc-400">Nombre</p>
              <p className="text-xs font-medium text-zinc-800 mt-0.5">
                {order.client?.name ?? `#${order.clientId}`}
              </p>
            </div>
            {order.client?.phone && (
              <div>
                <p className="text-[10px] text-zinc-400">Teléfono</p>
                <p className="text-xs font-medium text-zinc-800 mt-0.5">
                  {formatPhoneNumber(order.client.phone)}
                </p>
              </div>
            )}
          </div>
        </div>

        {/* ORDEN */}
        <div className="px-6 py-4 border-b border-zinc-100">
          <p className="text-[10px] font-semibold tracking-widest uppercase text-zinc-400 mb-2.5">
            Orden
          </p>
          <div className="grid grid-cols-2 gap-y-3 mb-3">
            <div>
              <p className="text-[10px] text-zinc-400">Recepción</p>
              <p className="text-xs font-medium text-zinc-800 mt-0.5">{formatDate(order.createdAt)}</p>
            </div>
            <div>
              <p className="text-[10px] text-zinc-400">Entrega prometida</p>
              <p className="text-xs font-medium text-zinc-800 mt-0.5">{formatDate(order.promisedDate)}</p>
            </div>
            {order.storageLocation && (
              <div>
                <p className="text-[10px] text-zinc-400">Ubicación en estante</p>
                <p className="text-xs font-medium text-zinc-800 mt-0.5">{order.storageLocation}</p>
              </div>
            )}
          </div>
          {mockOrderStatuses.length > 0 && (
            <OrderStatusChanger
              currentStatusId={localStatusId}
              statuses={mockOrderStatuses}
              onStatusChange={handleStatusChange}
              isLoading={isLoading}
            />
          )}
        </div>

        {/* OBSERVACIONES */}
        {order.notes && (
          <div className="px-6 py-4 border-b border-zinc-100">
            <p className="text-[10px] font-semibold tracking-widest uppercase text-zinc-400 mb-2">
              Observaciones
            </p>
            <p className="text-xs text-zinc-700 leading-relaxed">{order.notes}</p>
          </div>
        )}

        {/* ITEMS */}
        <div className="px-6 py-4 border-b border-zinc-100">
          <p className="text-[10px] font-semibold tracking-widest uppercase text-zinc-400 mb-2.5">
            Items ({order.items.length})
          </p>
          <OrderItemsTable order={order} />
        </div>

        {/* PAGOS */}
        <div className="px-6 py-4">
          <p className="text-[10px] font-semibold tracking-widest uppercase text-zinc-400 mb-2.5">
            Pagos
          </p>
          <OrderPaymentSection
            order={order}
            onPaymentSubmit={handlePaymentSubmit}
            isProcessingPayment={isProcessingPayment}
          />
        </div>

      </div>
    </div>
  );
}
