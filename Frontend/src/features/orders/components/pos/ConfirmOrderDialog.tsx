import { useState, useEffect } from 'react';
import { format } from 'date-fns';
import { es } from 'date-fns/locale';
import { Loader2 } from 'lucide-react';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/shared/components/ui/dialog';
import { Button } from '@/shared/components/ui/button';
import { Label } from '@/shared/components/ui/label';
import { Switch } from '@/shared/components/ui/switch';
import { CurrencyInput } from '@/shared/components/ui/currency-input';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select';
import { usePaymentMethodsStore } from '@/features/orders/stores/paymentMethodsStore';
import type { PaymentFormData } from '@/features/orders/components/payments/PaymentForm';
import type { Customer } from '@/features/customers/types/customer';

interface ConfirmOrderDialogProps {
  open: boolean;
  customer: Customer | null;
  itemCount: number;
  subtotal: number;
  totalDiscount: number;
  total: number;
  promisedDate: string;
  storageLocation: string;
  paymentData: PaymentFormData | null;
  isLoading: boolean;
  onConfirm: (paymentData: PaymentFormData | null) => void;
  onCancel: () => void;
}

export function ConfirmOrderDialog({
  open,
  customer,
  itemCount,
  subtotal,
  totalDiscount,
  total,
  promisedDate,
  storageLocation,
  paymentData,
  isLoading,
  onConfirm,
  onCancel,
}: ConfirmOrderDialogProps) {
  const { paymentMethods } = usePaymentMethodsStore();
  const activePaymentMethods = paymentMethods.filter((pm) => pm.isActive);

  const [registerPaymentNow, setRegisterPaymentNow] = useState(false);
  const [amount, setAmount] = useState<number | ''>(total);
  const [paymentMethodId, setPaymentMethodId] = useState<string>('');

  useEffect(() => {
    if (open) {
      setRegisterPaymentNow(total > 0);
      setAmount(paymentData?.amount ?? total);
      setPaymentMethodId(
        paymentData?.paymentMethodId
          ? paymentData.paymentMethodId.toString()
          : activePaymentMethods[0]?.id.toString() ?? ''
      );
    }
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open]);

  const formattedDate = promisedDate
    ? format(new Date(promisedDate + 'T12:00:00'), "d 'de' MMMM yyyy", { locale: es })
    : '—';

  const handleConfirm = () => {
    if (!registerPaymentNow) {
      onConfirm(null);
      return;
    }

    if (!paymentMethodId || !amount || amount <= 0) return;

    onConfirm({
      amount,
      paymentMethodId: parseInt(paymentMethodId),
      paidAt: new Date().toISOString(),
      reference: paymentData?.reference ?? '',
      notes: paymentData?.notes ?? '',
    });
  };

  const selectedMethod = activePaymentMethods.find(
    (pm) => pm.id.toString() === paymentMethodId
  );

  const paymentFieldsValid = !registerPaymentNow || (!!paymentMethodId && !!amount && amount > 0);

  return (
    <Dialog open={open} onOpenChange={(isOpen) => { if (!isOpen) onCancel(); }}>
      <DialogContent className="max-w-sm p-0 gap-0 overflow-hidden">
        <DialogHeader className="px-6 pt-5 pb-4 border-b border-zinc-100">
          <DialogTitle className="text-sm font-semibold text-zinc-900 tracking-tight">
            Confirmar Venta
          </DialogTitle>
          {customer && (
            <p className="text-xs text-zinc-400 mt-0.5">
              {customer.name}
              {customer.phone && (
                <> · <span className="font-mono">{customer.phone}</span></>
              )}
            </p>
          )}
        </DialogHeader>

        <div className="overflow-y-auto max-h-[80vh]">
          {/* Detalles del pedido */}
          <div className="px-6 py-4 border-b border-zinc-100">
            <p className="text-[10px] font-semibold tracking-widest uppercase text-zinc-400 mb-3">
              Detalles del pedido
            </p>
            <div className="space-y-0">
              <div className="flex justify-between items-center py-1.5 border-b border-zinc-100">
                <span className="text-xs text-zinc-500">Entrega prometida</span>
                <span className="text-xs text-zinc-700">{formattedDate}</span>
              </div>
              {storageLocation && (
                <div className="flex justify-between items-center py-1.5 border-b border-zinc-100">
                  <span className="text-xs text-zinc-500">Ubicación</span>
                  <span className="text-xs font-mono text-zinc-700">{storageLocation}</span>
                </div>
              )}
              <div className="flex justify-between items-center py-1.5">
                <span className="text-xs text-zinc-500">Servicios</span>
                <span className="text-xs text-zinc-700">
                  {itemCount} {itemCount === 1 ? 'item' : 'items'}
                </span>
              </div>
            </div>
          </div>

          {/* Totales */}
          <div className="px-6 py-4 border-b border-zinc-100">
            <p className="text-[10px] font-semibold tracking-widest uppercase text-zinc-400 mb-3">
              Totales
            </p>
            <div className="space-y-0">
              <div className="flex justify-between items-center py-1.5 border-b border-zinc-100">
                <span className="text-xs text-zinc-500">Subtotal</span>
                <span className="text-xs font-mono tabular-nums text-zinc-700">
                  ${subtotal.toFixed(2)}
                </span>
              </div>
              {totalDiscount > 0 && (
                <div className="flex justify-between items-center py-1.5 border-b border-zinc-100">
                  <span className="text-xs text-emerald-600">Descuento</span>
                  <span className="text-xs font-mono tabular-nums text-emerald-600">
                    -${totalDiscount.toFixed(2)}
                  </span>
                </div>
              )}
              <div className="flex justify-between items-center py-2">
                <span className="text-sm font-semibold text-zinc-900">Total</span>
                <span className="font-mono font-bold tabular-nums text-zinc-900">
                  ${total.toFixed(2)}
                </span>
              </div>
            </div>
          </div>

          {/* Pago inicial */}
          <div className="px-6 py-4">
            {/* Toggle */}
            <div className="flex items-center justify-between mb-4">
              <p className="text-[10px] font-semibold tracking-widest uppercase text-zinc-400">
                Pago inicial
              </p>
              <div className="flex items-center gap-2">
                <span className="text-xs text-zinc-500">
                  {registerPaymentNow ? 'Cobrar ahora' : 'Sin pago por ahora'}
                </span>
                <Switch
                  checked={registerPaymentNow}
                  onCheckedChange={setRegisterPaymentNow}
                  disabled={isLoading}
                />
              </div>
            </div>

            {registerPaymentNow ? (
              <div className="space-y-3">
                <div className="space-y-1">
                  <Label className="text-xs text-zinc-500 font-medium">
                    Método de pago
                  </Label>
                  <Select value={paymentMethodId} onValueChange={setPaymentMethodId}>
                    <SelectTrigger>
                      <SelectValue placeholder="Seleccionar método" />
                    </SelectTrigger>
                    <SelectContent>
                      {activePaymentMethods.map((method) => (
                        <SelectItem key={method.id} value={method.id.toString()}>
                          {method.name}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>

                <div className="space-y-1">
                  <Label className="text-xs text-zinc-500 font-medium">
                    Monto a cobrar
                  </Label>
                  <CurrencyInput
                    value={amount}
                    onChange={setAmount}
                    max={total}
                  />
                  {!!amount && amount > 0 && amount < total && (
                    <p className="text-[11px] text-amber-600 mt-1">
                      Pago parcial — saldo pendiente: ${(total - amount).toFixed(2)}
                    </p>
                  )}
                </div>

                {selectedMethod && (
                  <div className="flex items-center gap-1.5 py-1.5 px-3 rounded-md bg-zinc-50 border border-zinc-100">
                    <span className="text-xs text-zinc-500">
                      {!!amount && amount >= total ? 'Pago completo con' : 'Pago parcial con'}
                    </span>
                    <span className="text-xs font-semibold text-zinc-800">{selectedMethod.name}</span>
                  </div>
                )}
              </div>
            ) : (
              <div className="flex items-center gap-2 py-2 px-3 rounded-md bg-zinc-50 border border-zinc-100">
                <span className="text-xs text-zinc-500">
                  La orden quedará con saldo pendiente de{' '}
                  <span className="font-mono font-semibold text-zinc-700">${total.toFixed(2)}</span>
                </span>
              </div>
            )}
          </div>
        </div>

        {/* Footer */}
        <div className="px-6 py-4 flex gap-3 justify-end border-t border-zinc-100 bg-zinc-50">
          <Button variant="outline" onClick={onCancel} disabled={isLoading}>
            Cancelar
          </Button>
          <Button
            onClick={handleConfirm}
            disabled={isLoading || !paymentFieldsValid}
          >
            {isLoading ? (
              <span className="flex items-center gap-2">
                <Loader2 className="w-4 h-4 animate-spin" />
                Registrando...
              </span>
            ) : (
              'Confirmar Venta'
            )}
          </Button>
        </div>
      </DialogContent>
    </Dialog>
  );
}
