import { formatDate } from '@/shared/utils/formatters';
import { PAYMENT_STATUS } from '@/features/orders/types/payment';
import type { OrderSummary } from '@/features/orders/types/order';

interface OrderSearchResultCardProps {
  order: OrderSummary;
  onClick: (order: OrderSummary) => void;
}

function getPaymentStatus(status: string): { label: string; className: string } {
  switch (status) {
    case PAYMENT_STATUS.PAID:
      return { label: 'Pagado', className: 'text-emerald-600' };
    case PAYMENT_STATUS.PARTIAL:
      return { label: 'Parcial', className: 'text-amber-600' };
    case PAYMENT_STATUS.PENDING:
    default:
      return { label: 'Pendiente', className: 'text-rose-600' };
  }
}

export function OrderSearchResultCard({ order, onClick }: OrderSearchResultCardProps) {
  const payment = getPaymentStatus(order.paymentStatus);

  return (
    <button
      type="button"
      className="w-full text-left px-6 py-3 border-b border-zinc-100 hover:bg-zinc-50 transition-colors"
      onClick={() => onClick(order)}
    >
      <div className="flex items-start justify-between gap-3">
        <div className="min-w-0 flex-1">
          {/* Folio como hero — lo primero que escanea el cajero */}
          <div className="flex items-center gap-2 mb-0.5">
            <span className="font-mono font-bold text-sm tracking-tight text-zinc-900">
              {order.folioOrden}
            </span>
            {order.orderStatus && (
              <span
                className="inline-flex items-center px-2 py-0.5 rounded-full text-[10px] font-semibold tracking-wide leading-none shrink-0"
                style={{
                  backgroundColor: `${order.orderStatus.color}20`,
                  color: order.orderStatus.color,
                }}
              >
                {order.orderStatus.name}
              </span>
            )}
          </div>
          <p className="text-xs font-medium text-zinc-800 truncate">
            {order.client?.name ?? `Cliente #${order.clientId}`}
          </p>
          <p className="text-[10px] text-zinc-400 mt-0.5">{formatDate(order.createdAt)}</p>
        </div>

        <div className="text-right shrink-0">
          <p className="font-mono font-semibold tabular-nums text-sm text-zinc-900">
            ${order.total.toFixed(2)}
          </p>
          <p className={`text-[10px] font-medium mt-0.5 ${payment.className}`}>
            {payment.label}
          </p>
        </div>
      </div>
    </button>
  );
}
