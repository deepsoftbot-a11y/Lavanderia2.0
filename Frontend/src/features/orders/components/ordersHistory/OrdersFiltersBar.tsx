import { useState } from 'react';
import { X } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { Input } from '@/shared/components/ui/input';
import { Label } from '@/shared/components/ui/label';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select';
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

const SELECT_TRIGGER_CLASS =
  'h-8 text-xs bg-zinc-100 border-2 border-transparent rounded-lg focus:border-blue-600 focus:bg-blue-50 w-36';

export function OrdersFiltersBar({ onFiltersChange }: OrdersFiltersBarProps) {
  const [startDate,      setStartDate]      = useState('');
  const [endDate,        setEndDate]        = useState('');
  const [selectedStatus, setSelectedStatus] = useState<string>('');
  const [selectedPayment,setSelectedPayment]= useState<string>('');
  const [dateError,      setDateError]      = useState('');

  const hasFilters =
    startDate !== '' || endDate !== '' ||
    selectedStatus !== '' || selectedPayment !== '';

  function buildFilters(
    sd: string,
    ed: string,
    status: string,
    payment: string,
  ): OrderHistoryFilters {
    return {
      startDate:       sd      || undefined,
      endDate:         ed      || undefined,
      statusIds:       status  ? [Number(status)] : undefined,
      paymentStatuses: payment ? [payment as PaymentStatus] : undefined,
    };
  }

  function handleStartDate(value: string) {
    setStartDate(value);
    if (endDate && value > endDate) {
      setDateError('La fecha de inicio no puede ser posterior a la fecha de fin');
      return;
    }
    setDateError('');
    onFiltersChange(buildFilters(value, endDate, selectedStatus, selectedPayment));
  }

  function handleEndDate(value: string) {
    setEndDate(value);
    if (startDate && value < startDate) {
      setDateError('La fecha de fin no puede ser anterior a la fecha de inicio');
      return;
    }
    setDateError('');
    onFiltersChange(buildFilters(startDate, value, selectedStatus, selectedPayment));
  }

  function handleStatusChange(value: string) {
    const next = value === 'all' ? '' : value;
    setSelectedStatus(next);
    onFiltersChange(buildFilters(startDate, endDate, next, selectedPayment));
  }

  function handlePaymentChange(value: string) {
    const next = value === 'all' ? '' : value;
    setSelectedPayment(next);
    onFiltersChange(buildFilters(startDate, endDate, selectedStatus, next));
  }

  function handleClear() {
    setStartDate('');
    setEndDate('');
    setSelectedStatus('');
    setSelectedPayment('');
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
              className="h-8 text-xs w-36 bg-zinc-100 border-2 border-transparent rounded-lg focus:border-blue-600 focus:bg-blue-50"
            />
          </div>
          <div className="space-y-1">
            <Label className="text-xs text-zinc-500">Hasta</Label>
            <Input
              type="date"
              value={endDate}
              onChange={(e) => handleEndDate(e.target.value)}
              className="h-8 text-xs w-36 bg-zinc-100 border-2 border-transparent rounded-lg focus:border-blue-600 focus:bg-blue-50"
            />
          </div>
        </div>

        {/* Estado de orden */}
        <div className="space-y-1">
          <Label className="text-xs text-zinc-500">Estado</Label>
          <Select value={selectedStatus || 'all'} onValueChange={handleStatusChange}>
            <SelectTrigger className={SELECT_TRIGGER_CLASS}>
              <SelectValue placeholder="Todos" />
            </SelectTrigger>
            <SelectContent className="rounded-xl border border-zinc-200 bg-white shadow-sm">
              <SelectItem value="all" className="text-xs">Todos</SelectItem>
              {mockOrderStatuses.map((s) => (
                <SelectItem key={s.id} value={String(s.id)} className="text-xs">
                  {s.name}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>

        {/* Estado de pago */}
        <div className="space-y-1">
          <Label className="text-xs text-zinc-500">Pago</Label>
          <Select value={selectedPayment || 'all'} onValueChange={handlePaymentChange}>
            <SelectTrigger className={SELECT_TRIGGER_CLASS}>
              <SelectValue placeholder="Todos" />
            </SelectTrigger>
            <SelectContent className="rounded-xl border border-zinc-200 bg-white shadow-sm">
              <SelectItem value="all" className="text-xs">Todos</SelectItem>
              {PAYMENT_STATUS_OPTIONS.map(({ value, label }) => (
                <SelectItem key={value} value={value} className="text-xs">
                  {label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
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
        <p className="text-xs text-rose-500">{dateError}</p>
      )}
    </div>
  );
}
