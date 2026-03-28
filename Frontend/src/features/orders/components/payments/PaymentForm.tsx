import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { usePaymentMethodsStore } from '@/features/orders/stores/paymentMethodsStore';
import { createPaymentSchema } from '@/features/orders/schemas/payment.schema';
import { Button } from '@/shared/components/ui/button';
import {
  Form,
  FormControl,
  FormDescription,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from '@/shared/components/ui/form';
import { NumericInput } from '@/shared/components/common/NumericInput';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select';
import { useEffect } from 'react';

export interface PaymentFormData {
  amount: number;
  paymentMethodId: number;
  reference?: string;
  notes?: string;
  paidAt: string;
}

interface PaymentFormProps {
  maxAmount: number;
  defaultAmount?: number;
  defaultPaymentMethodId?: number;
  onSubmit: (data: PaymentFormData) => void;
  isLoading?: boolean;
  showCancelButton?: boolean;
  onCancel?: () => void;
  autoSubmit?: boolean;
}

export function PaymentForm({
  maxAmount,
  defaultAmount,
  defaultPaymentMethodId = 1,
  onSubmit,
  isLoading = false,
  showCancelButton = false,
  onCancel,
  autoSubmit = true,
}: PaymentFormProps) {
  const { paymentMethods } = usePaymentMethodsStore();

  const form = useForm<PaymentFormData>({
    resolver: zodResolver(
      createPaymentSchema.omit({ orderId: true, receivedBy: true })
    ),
    defaultValues: {
      amount: defaultAmount || maxAmount,
      paymentMethodId: defaultPaymentMethodId,
      paidAt: new Date().toISOString(),
      reference: '',
      notes: '',
    },
  });

  // Sincroniza el monto cuando cambia el total de la orden (ej. al agregar items al carrito)
  useEffect(() => {
    if (defaultAmount !== undefined && defaultAmount > 0) {
      form.setValue('amount', defaultAmount, { shouldValidate: true });
    }
  }, [defaultAmount, form]);

  // Validación: monto no debe exceder máximo
  const validateAmount = (amount: number): boolean => {
    if (amount > maxAmount) {
      form.setError('amount', {
        type: 'manual',
        message: `El monto no puede exceder $${maxAmount.toFixed(2)}`,
      });
      return false;
    }
    return true;
  };

  const handleSubmit = (data: PaymentFormData) => {
    if (!validateAmount(data.amount)) return;
    onSubmit(data);
  };

  // Auto-submit on form change to update parent state (only when autoSubmit is enabled)
  useEffect(() => {
    if (!autoSubmit) return;
    const subscription = form.watch(() => {
      const values = form.getValues();
      const amount = values.amount;

      if (amount > 0 && amount > maxAmount) {
        form.setError('amount', {
          type: 'manual',
          message: `El monto no puede exceder $${maxAmount.toFixed(2)}`,
        });
        return;
      }

      if (form.formState.isValid && amount > 0 && amount <= maxAmount) {
        form.clearErrors('amount');
        onSubmit(values);
      }
    });
    return () => subscription.unsubscribe();
  }, [form, maxAmount, onSubmit, autoSubmit]);

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(handleSubmit)} className="space-y-4">
        {/* Campo: Monto */}
        <FormField
          control={form.control}
          name="amount"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Monto del pago</FormLabel>
              <FormControl>
                <NumericInput
                  value={field.value}
                  onChange={field.onChange}
                  onBlur={field.onBlur}
                  ref={field.ref}
                  min={0.01}
                  max={maxAmount}
                  step={0.01}
                  prefix="$"
                  placeholder="0.00"
                />
              </FormControl>
              <FormDescription>
                Máximo: ${maxAmount.toFixed(2)}
              </FormDescription>
              <FormMessage />
            </FormItem>
          )}
        />

        {/* Campo: Método de pago */}
        <FormField
          control={form.control}
          name="paymentMethodId"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Método de pago</FormLabel>
              <Select
                onValueChange={(value) => field.onChange(parseInt(value))}
                defaultValue={field.value.toString()}
              >
                <FormControl>
                  <SelectTrigger>
                    <SelectValue placeholder="Seleccione un método" />
                  </SelectTrigger>
                </FormControl>
                <SelectContent>
                  {paymentMethods
                    .filter((pm) => pm.isActive)
                    .map((method) => (
                      <SelectItem key={method.id} value={method.id.toString()}>
                        {method.name}
                      </SelectItem>
                    ))}
                </SelectContent>
              </Select>
              <FormMessage />
            </FormItem>
          )}
        />


        {/* Botones - Solo mostrar si se requiere submit manual */}
        {showCancelButton && (
          <div className="flex gap-2 justify-end">
            {onCancel && (
              <Button
                type="button"
                variant="outline"
                onClick={onCancel}
                disabled={isLoading}
              >
                Cancelar
              </Button>
            )}
            <Button type="submit" disabled={isLoading}>
              {isLoading ? 'Procesando...' : 'Confirmar Pago'}
            </Button>
          </div>
        )}
      </form>
    </Form>
  );
}
