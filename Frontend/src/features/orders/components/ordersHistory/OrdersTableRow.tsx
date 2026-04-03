import { Eye } from 'lucide-react';
import { format } from 'date-fns';
import { es } from 'date-fns/locale';
import { Button } from '@/shared/components/ui/button';
import { cn } from '@/shared/utils/cn';
import type { Order } from '@/features/orders/types/order';

interface OrdersTableRowProps {
  order: Order;
  onView: (orderId: number) => void;
}

const PAYMENT_LABELS: Record<string, { label: string; className: string }> = {
  paid:    { label: 'Pagado',     className: 'text-green-700 bg-green-50 border-green-200' },
  partial: { label: 'Parcial',    className: 'text-amber-700 bg-amber-50 border-amber-200' },
  pending: { label: 'Pendiente',  className: 'text-red-700 bg-red-50 border-red-200' },
};

export function OrdersTableRow({ order, onView }: OrdersTableRowProps) {
  const payment = PAYMENT_LABELS[order.paymentStatus] ?? PAYMENT_LABELS.pending;

  return (
    <>
      {/* Desktop row */}
      <div className="hidden md:grid grid-cols-[80px_2fr_1fr_80px_1fr_120px_48px] gap-4 items-center px-6 py-3 border-b border-zinc-100 hover:bg-zinc-50 transition-colors">
        <span className="font-mono font-bold text-sm tracking-tight text-zinc-900">
          #{order.id}
        </span>
        <p className="text-sm font-medium text-zinc-800 truncate">
          {order.client?.name ?? '—'}
        </p>
        <p className="text-xs text-zinc-500 capitalize">
          {format(new Date(order.promisedDate), "d 'de' MMM", { locale: es })}
        </p>
        <span className="text-xs text-zinc-500 font-mono">
          {order.items.length}
        </span>
        <span className="font-mono font-semibold tabular-nums text-sm text-zinc-900">
          ${order.total.toFixed(2)}
        </span>
        <span className={cn('text-[11px] font-medium px-2 py-0.5 rounded-full border w-fit', payment.className)}>
          {payment.label}
        </span>
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
            #{order.id}
          </span>
          <div className="flex items-center gap-2">
            <span className={cn('text-[11px] font-medium px-2 py-0.5 rounded-full border', payment.className)}>
              {payment.label}
            </span>
            <span className="font-mono font-semibold tabular-nums text-sm text-zinc-900">
              ${order.total.toFixed(2)}
            </span>
          </div>
        </div>
        <p className="text-sm font-medium text-zinc-800">{order.client?.name ?? '—'}</p>
        <p className="text-xs text-zinc-400 capitalize mt-0.5">
          Entrega: {format(new Date(order.promisedDate), "d 'de' MMM 'de' yyyy", { locale: es })}
        </p>
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
}
