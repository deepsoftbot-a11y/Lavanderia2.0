import { memo } from 'react';
import { Eye } from 'lucide-react';
import { format } from 'date-fns';
import { es } from 'date-fns/locale';
import { Button } from '@/shared/components/ui/button';
import { cn } from '@/shared/utils/cn';
import type { OrderSummary } from '@/features/orders/types/order';

interface OrdersTableRowProps {
  order: OrderSummary;
  onView: (orderId: number) => void;
}

const PAYMENT_LABELS: Record<string, { label: string; className: string }> = {
  paid:    { label: 'Pagado',     className: 'text-emerald-700 bg-emerald-50 border border-emerald-100' },
  partial: { label: 'Parcial',    className: 'text-amber-700 bg-amber-50 border border-amber-100' },
  pending: { label: 'Pendiente',  className: 'text-rose-700 bg-rose-50 border border-rose-100' },
};

export const OrdersTableRow = memo(function OrdersTableRow({ order, onView }: OrdersTableRowProps) {
  const payment = PAYMENT_LABELS[order.paymentStatus] ?? PAYMENT_LABELS.pending;
  const registroDate = format(new Date(order.createdAt), "d MMM yyyy", { locale: es });

  return (
    <>
      {/* Desktop row */}
      <div className="hidden md:grid grid-cols-[140px_2fr_100px_1fr_1fr_120px_48px] gap-4 items-center px-6 py-3 border-b border-zinc-100 hover:bg-zinc-50 transition-colors">
        <span className="font-mono font-bold text-sm tracking-tight text-zinc-900 truncate">
          {order.folioOrden}
        </span>
        <p className="text-sm font-medium text-zinc-800 truncate">
          {order.client?.name ?? '—'}
        </p>
        <span
          className="inline-flex items-center px-2 py-0.5 rounded-full text-[10px] font-semibold tracking-wide leading-none w-fit"
          style={order.orderStatus?.color
            ? { color: order.orderStatus.color, backgroundColor: `${order.orderStatus.color}20` }
            : undefined}
        >
          {order.orderStatus?.name ?? '—'}
        </span>
        <span className={cn('text-[10px] font-semibold px-2 py-0.5 rounded-full w-fit', payment.className)}>
          {payment.label}
        </span>
        <span className="font-mono font-semibold tabular-nums text-sm text-zinc-900">
          ${order.total.toFixed(2)}
        </span>
        <p className="text-xs text-zinc-500 capitalize">
          {registroDate}
        </p>
        <div className="flex justify-end">
          <Button
            variant="ghost"
            size="sm"
            className="h-7 w-7 p-0"
            onClick={() => onView(order.id)}
          >
            <Eye className="h-3.5 w-3.5 text-zinc-400" />
          </Button>
        </div>
      </div>

      {/* Mobile card */}
      <div className="md:hidden px-6 py-4 border-b border-zinc-100">
        <div className="flex items-start justify-between gap-3 mb-1.5">
          <span className="font-mono font-bold text-sm tracking-tight text-zinc-900">
            {order.folioOrden}
          </span>
          <div className="flex items-center gap-2">
            <span className={cn('text-[10px] font-semibold px-2 py-0.5 rounded-full', payment.className)}>
              {payment.label}
            </span>
            <span className="font-mono font-semibold tabular-nums text-sm text-zinc-900">
              ${order.total.toFixed(2)}
            </span>
          </div>
        </div>
        <p className="text-sm font-medium text-zinc-800">{order.client?.name ?? '—'}</p>
        <p className="text-xs text-zinc-400 mt-0.5 capitalize">{registroDate}</p>
        {order.orderStatus && (
          <span
            className="inline-flex items-center px-2 py-0.5 rounded-full text-[10px] font-semibold tracking-wide leading-none w-fit mt-1"
            style={{ color: order.orderStatus.color, backgroundColor: `${order.orderStatus.color}20` }}
          >
            {order.orderStatus.name}
          </span>
        )}
        <div className="flex gap-2 mt-3 pt-3 border-t border-zinc-100">
          <Button
            size="sm"
            variant="outline"
            className="h-7 text-xs"
            onClick={() => onView(order.id)}
          >
            <Eye className="h-3 w-3 mr-1" />
            Ver detalle
          </Button>
        </div>
      </div>
    </>
  );
});
