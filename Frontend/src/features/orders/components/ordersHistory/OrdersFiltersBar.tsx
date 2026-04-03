import { useState } from 'react';
import { X } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { Input } from '@/shared/components/ui/input';
import { Label } from '@/shared/components/ui/label';
import { cn } from '@/shared/utils/cn';
import { mockOrderStatuses } from '@/api/orderStatuses';
import type { OrderHistoryFilters } from '@/features/orders/types/order';
import type { PaymentStatus } from '@/features/orders/types/payment';

interface OrdersFiltersBarProps {
  onFiltersChange: (filters: OrderHistoryFilters) => void;
}

const PAYMENT_STATUS_OPTIONS: { value: PaymentStatus; label: string }[] = [
  { value: 'paid',    label: 'Pagado'    },
  { value: 'partial', label: 'Parcial'   },
  { value: 'pending', label: 'Pendiente' },
];

export function OrdersFiltersBar({ onFiltersChange }: OrdersFiltersBarProps) {
  const [startDate, setStartDate]                 = useState('');
  const [endDate, setEndDate]                     = useState('');
  const [selectedStatusIds, setSelectedStatusIds] = useState<number[]>([]);
  const [selectedPayments, setSelectedPayments]   = useState<PaymentStatus[]>([]);
  const [dateError, setDateError]                 = useState('');

  const hasFilters =
    startDate !== '' || endDate !== '' ||
    selectedStatusIds.length > 0 || selectedPayments.length > 0;

  function buildFilters(
    sd: string,
    ed: string,
    statuses: number[],
    payments: PaymentStatus[]
  ): OrderHistoryFilters {
    return {
      startDate:       sd || undefined,
      endDate:         ed || undefined,
      statusIds:       statuses.length > 0 ? statuses : undefined,
      paymentStatuses: payments.length > 0 ? payments : undefined,
    };
  }

  function handleStartDate(value: string) {
    setStartDate(value);
    if (endDate && value > endDate) {
      setDateError('La fecha de inicio no puede ser posterior a la fecha de fin');
      return;
    }
    setDateError('');
    onFiltersChange(buildFilters(value, endDate, selectedStatusIds, selectedPayments));
  }

  function handleEndDate(value: string) {
    setEndDate(value);
    if (startDate && value < startDate) {
      setDateError('La fecha de fin no puede ser anterior a la fecha de inicio');
      return;
    }
    setDateError('');
    onFiltersChange(buildFilters(startDate, value, selectedStatusIds, selectedPayments));
  }

  function handleStatusChange(id: number) {
    const next = selectedStatusIds.includes(id)
      ? selectedStatusIds.filter((s) => s !== id)
      : [...selectedStatusIds, id];
    setSelectedStatusIds(next);
    onFiltersChange(buildFilters(startDate, endDate, next, selectedPayments));
  }

  function handlePaymentToggle(status: PaymentStatus) {
    const next = selectedPayments.includes(status)
      ? selectedPayments.filter((s) => s !== status)
      : [...selectedPayments, status];
    setSelectedPayments(next);
    onFiltersChange(buildFilters(startDate, endDate, selectedStatusIds, next));
  }

  function handleClear() {
    setStartDate('');
    setEndDate('');
    setSelectedStatusIds([]);
    setSelectedPayments([]);
    setDateError('');
    onFiltersChange({});
  }

  return (
    <div className="border-b border-zinc-100 bg-zinc-50 px-6 py-4 space-y-3">
      <div className="flex flex-wrap gap-4 items-end">
        {/* Rango de fechas */}
        <div className="flex gap-2 items-end">
          <div className="space-y-1">
            <Label className="text-xs text-zinc-500">Desde</Label>
            <Input
              type="date"
              value={startDate}
              onChange={(e) => handleStartDate(e.target.value)}
              className="h-8 text-xs w-36"
            />
          </div>
          <div className="space-y-1">
            <Label className="text-xs text-zinc-500">Hasta</Label>
            <Input
              type="date"
              value={endDate}
              onChange={(e) => handleEndDate(e.target.value)}
              className="h-8 text-xs w-36"
            />
          </div>
        </div>

        {/* Estado de orden */}
        <div className="space-y-1">
          <Label className="text-xs text-zinc-500">Estado de orden</Label>
          <div className="flex gap-1 flex-wrap">
            {mockOrderStatuses.map((status) => (
              <button
                key={status.id}
                type="button"
                onClick={() => handleStatusChange(status.id)}
                className={cn(
                  'px-2.5 py-1 rounded-full text-xs font-medium border transition-colors',
                  selectedStatusIds.includes(status.id)
                    ? 'bg-zinc-900 text-white border-zinc-900'
                    : 'bg-white text-zinc-600 border-zinc-200 hover:border-zinc-400'
                )}
              >
                {status.name}
              </button>
            ))}
          </div>
        </div>

        {/* Estado de pago */}
        <div className="space-y-1">
          <Label className="text-xs text-zinc-500">Estado de pago</Label>
          <div className="flex gap-1">
            {PAYMENT_STATUS_OPTIONS.map(({ value, label }) => (
              <button
                key={value}
                type="button"
                onClick={() => handlePaymentToggle(value)}
                className={cn(
                  'px-2.5 py-1 rounded-full text-xs font-medium border transition-colors',
                  selectedPayments.includes(value)
                    ? 'bg-zinc-900 text-white border-zinc-900'
                    : 'bg-white text-zinc-600 border-zinc-200 hover:border-zinc-400'
                )}
              >
                {label}
              </button>
            ))}
          </div>
        </div>

        {/* Limpiar */}
        {hasFilters && (
          <Button
            variant="ghost"
            size="sm"
            onClick={handleClear}
            className="h-8 text-xs text-zinc-400 hover:text-zinc-600"
          >
            <X className="h-3 w-3 mr-1" />
            Limpiar
          </Button>
        )}
      </div>

      {dateError && (
        <p className="text-xs text-red-500">{dateError}</p>
      )}
    </div>
  );
}
