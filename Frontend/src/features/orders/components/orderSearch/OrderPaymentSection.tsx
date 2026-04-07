import { useState } from 'react';
import { Plus, ChevronDown, ChevronUp } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { PaymentForm, type PaymentFormData } from '@/features/orders/components/payments/PaymentForm';
import { formatDate } from '@/shared/utils/formatters';
import { PAYMENT_STATUS } from '@/features/orders/types/payment';
import { cn } from '@/shared/utils/cn';
import type { Order } from '@/features/orders/types/order';

interface OrderPaymentSectionProps {
  order: Order;
  onPaymentSubmit: (data: PaymentFormData) => Promise<void>;
  isProcessingPayment: boolean;
}

function getPaymentStatus(status: string): { label: string; className: string } {
  switch (status) {
    case PAYMENT_STATUS.PAID:
      return { label: 'Pagado',    className: 'bg-emerald-50 border border-emerald-100 text-emerald-700' };
    case PAYMENT_STATUS.PARTIAL:
      return { label: 'Parcial',   className: 'bg-amber-50 border border-amber-100 text-amber-700' };
    case PAYMENT_STATUS.PENDING:
    default:
      return { label: 'Pendiente', className: 'bg-rose-50 border border-rose-100 text-rose-700' };
  }
}

export function OrderPaymentSection({ order, onPaymentSubmit, isProcessingPayment }: OrderPaymentSectionProps) {
  const [showPaymentForm, setShowPaymentForm] = useState(false);
  const [showHistory, setShowHistory] = useState(false);
  const statusInfo = getPaymentStatus(order.paymentStatus);
  const hasPayments = order.payments && order.payments.length > 0;
  const hasBalance = order.balance > 0;

  const handlePaymentSubmit = async (data: PaymentFormData) => {
    await onPaymentSubmit(data);
    setShowPaymentForm(false);
  };

  return (
    <div>
      {/* Resumen financiero */}
      <div className="grid grid-cols-3 gap-2 mb-3">
        <div className="p-2.5 bg-zinc-50 rounded-md text-center">
          <p className="text-[10px] text-zinc-400 mb-0.5">Total</p>
          <p className="font-mono font-semibold tabular-nums text-sm text-zinc-900">
            ${order.total.toFixed(2)}
          </p>
        </div>
        <div className="p-2.5 bg-zinc-50 rounded-md text-center">
          <p className="text-[10px] text-zinc-400 mb-0.5">Pagado</p>
          <p className="font-mono font-semibold tabular-nums text-sm text-emerald-600">
            ${order.amountPaid.toFixed(2)}
          </p>
        </div>
        <div className="p-2.5 bg-zinc-50 rounded-md text-center">
          <p className="text-[10px] text-zinc-400 mb-0.5">Saldo</p>
          <p className={cn(
            'font-mono font-semibold tabular-nums text-sm',
            order.balance > 0 ? 'text-rose-600' : 'text-zinc-400',
          )}>
            ${order.balance.toFixed(2)}
          </p>
        </div>
      </div>

      {/* Estado de pago */}
      <span className={cn('inline-flex items-center px-2 py-0.5 rounded-full text-[10px] font-semibold', statusInfo.className)}>
        {statusInfo.label}
      </span>

      {/* Historial de pagos */}
      {hasPayments && (
        <div className="pt-3 mt-3 border-t border-zinc-100">
          <button
            type="button"
            className="flex items-center justify-between w-full text-[10px] font-semibold tracking-widest uppercase text-zinc-400 hover:text-zinc-600 transition-colors"
            onClick={() => setShowHistory(!showHistory)}
          >
            <span>Historial ({order.payments!.length})</span>
            {showHistory
              ? <ChevronUp className="h-3 w-3" />
              : <ChevronDown className="h-3 w-3" />
            }
          </button>

          {showHistory && (
            <div className="mt-2 space-y-0">
              {order.payments!.map((payment) => (
                <div
                  key={payment.id}
                  className="flex items-center justify-between py-1.5 border-b border-zinc-50 last:border-0"
                >
                  <div>
                    <span className="text-[10px] text-zinc-400">{formatDate(payment.paidAt)}</span>
                    {payment.paymentMethod && (
                      <span className="text-[10px] text-zinc-400 ml-1.5">
                        · {payment.paymentMethod.name}
                      </span>
                    )}
                  </div>
                  <span className="font-mono font-medium tabular-nums text-xs text-zinc-800">
                    ${payment.amount.toFixed(2)}
                  </span>
                </div>
              ))}
            </div>
          )}
        </div>
      )}

      {/* Registrar pago */}
      {hasBalance && (
        <div className="pt-3 mt-3 border-t border-zinc-100">
          {!showPaymentForm ? (
            <Button
              size="sm"
              className="w-full text-xs"
              onClick={() => setShowPaymentForm(true)}
            >
              <Plus className="h-3.5 w-3.5 mr-1.5" />
              Registrar Pago
            </Button>
          ) : (
            <div className="space-y-2">
              <p className="text-[10px] font-semibold tracking-widest uppercase text-zinc-400">
                Nuevo Pago
              </p>
              <PaymentForm
                maxAmount={order.balance}
                defaultAmount={order.balance}
                onSubmit={handlePaymentSubmit}
                isLoading={isProcessingPayment}
                showCancelButton={true}
                onCancel={() => setShowPaymentForm(false)}
                autoSubmit={false}
              />
            </div>
          )}
        </div>
      )}
    </div>
  );
}
