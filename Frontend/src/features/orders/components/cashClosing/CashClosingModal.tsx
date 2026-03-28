import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { format } from 'date-fns';
import { es } from 'date-fns/locale';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/shared/components/ui/dialog';
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from '@/shared/components/ui/form';
import { NumericInput } from '@/shared/components/common/NumericInput';
import { Textarea } from '@/shared/components/ui/textarea';
import { Button } from '@/shared/components/ui/button';
import { useCashClosingsStore } from '@/features/orders/stores/cashClosingsStore';
import { useToast } from '@/shared/hooks/use-toast';
import { createCashClosingSchema } from '@/features/orders/schemas/cashClosing.schema';
import type { CreateCashClosingInput } from '@/features/orders/types/cashClosing';
import { cn } from '@/shared/utils/cn';

// ─── Modal principal ──────────────────────────────────────────────────────────

interface CashClosingModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function CashClosingModal({ open, onOpenChange }: CashClosingModalProps) {
  const { toast } = useToast();
  const {
    todayTotals,
    isLoadingTotals,
    isLoading,
    fetchTodayTotals,
    createCashClosing,
  } = useCashClosingsStore();

  const form = useForm({
    resolver: zodResolver(createCashClosingSchema),
    defaultValues: {
      fondoInicial: 0,
      actualCashAmount: 0,
      cashWithdrawal: 0,
      adjustment: 0,
      notes: '',
    },
  });

  useEffect(() => {
    if (open) {
      fetchTodayTotals();
      form.reset({
        fondoInicial: 0,
        actualCashAmount: 0,
        cashWithdrawal: 0,
        adjustment: 0,
        notes: '',
      });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, fetchTodayTotals]);

  const fondoInicial = form.watch('fondoInicial') || 0;
  const actualCash = form.watch('actualCashAmount') || 0;
  const adjustment = form.watch('adjustment') || 0;
  const expectedCash = todayTotals?.cashAmount ?? 0;
  const cashDiscrepancy = actualCash - fondoInicial + adjustment - expectedCash;

  const handleSubmit = async (data: CreateCashClosingInput) => {
    const result = await createCashClosing(data);
    if (result) {
      toast({
        title: 'Corte de caja registrado',
        description: 'El corte se ha guardado exitosamente',
      });
      onOpenChange(false);
    } else {
      const currentError = useCashClosingsStore.getState().error;
      toast({
        title: 'Error',
        description: currentError ?? 'Error al registrar el corte',
        variant: 'destructive',
      });
    }
  };

  const todayLabel = format(new Date(), "EEEE d 'de' MMMM, yyyy", { locale: es });

  const paymentMethods = [
    { label: 'Efectivo',   value: todayTotals?.cashAmount ?? 0 },
    { label: 'Tarjeta',    value: todayTotals?.cardAmount ?? 0 },
    { label: 'Transfer.',  value: todayTotals?.transferAmount ?? 0 },
    { label: 'Cheque',     value: todayTotals?.checkAmount ?? 0 },
  ];

  const discrepancySign = cashDiscrepancy > 0 ? '+' : cashDiscrepancy < 0 ? '−' : '';
  const discrepancyDisplay = `${discrepancySign}$${Math.abs(cashDiscrepancy).toFixed(2)}`;

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-md p-0 gap-0 overflow-hidden">

        {/* ── Header ─────────────────────────────────────────────────────── */}
        <DialogHeader className="px-6 pt-5 pb-4 border-b border-zinc-100">
          <DialogTitle className="text-sm font-semibold text-zinc-900 tracking-tight">
            Corte de Caja
          </DialogTitle>
          <p className="text-xs text-zinc-400 capitalize mt-0.5">{todayLabel}</p>
        </DialogHeader>

        {isLoadingTotals ? (
          <div className="py-16 text-center text-zinc-400 text-sm">Cargando...</div>
        ) : (
          <Form {...form}>
            <form
              onSubmit={form.handleSubmit(handleSubmit)}
              className="flex flex-col max-h-[82vh]"
            >
              {/* ── Contenido scrollable ──────────────────────────────────── */}
              <div className="overflow-y-auto flex-1">

                {/* Resumen del día */}
                <div className="px-6 py-4 bg-zinc-50 border-b border-zinc-100">
                  <p className="text-[10px] font-semibold tracking-widest text-zinc-400 uppercase mb-3">
                    Resumen del Día
                  </p>

                  <div className="grid grid-cols-4 gap-2 mb-3">
                    {paymentMethods.map((method) => (
                      <div key={method.label} className="text-center">
                        <p className="text-[10px] text-zinc-400 mb-1">{method.label}</p>
                        <p className="text-xs font-mono font-semibold tabular-nums text-zinc-700">
                          ${method.value.toFixed(2)}
                        </p>
                      </div>
                    ))}
                  </div>

                  <div className="flex items-center justify-between pt-3 border-t border-zinc-200">
                    <span className="text-xs text-zinc-500">Total del día</span>
                    <span className="font-mono font-bold tabular-nums text-zinc-900 text-lg tracking-tight">
                      ${(todayTotals?.totalSales ?? 0).toFixed(2)}
                    </span>
                  </div>
                </div>

                {/* Conteo de caja */}
                <div className="px-6 py-4 border-b border-zinc-100">
                  <p className="text-[10px] font-semibold tracking-widest text-zinc-400 uppercase mb-3">
                    Conteo de Caja
                  </p>

                  {/* Efectivo esperado — referencia para el cajero */}
                  <div className="flex items-center justify-between px-3 py-2.5 rounded-md bg-zinc-100 mb-4">
                    <span className="text-xs text-zinc-500">Efectivo esperado</span>
                    <span className="font-mono font-semibold tabular-nums text-zinc-800 text-sm">
                      ${expectedCash.toFixed(2)}
                    </span>
                  </div>

                  {/* Grid 2×2 de inputs */}
                  <div className="grid grid-cols-2 gap-3">
                    <FormField
                      control={form.control}
                      name="fondoInicial"
                      render={({ field }) => (
                        <FormItem className="space-y-1">
                          <FormLabel className="text-xs text-zinc-500 font-medium">
                            Fondo Inicial
                          </FormLabel>
                          <FormControl>
                            <NumericInput
                              value={field.value ?? 0}
                              onChange={field.onChange}
                              onBlur={field.onBlur}
                              ref={field.ref}
                              min={0}
                              step={0.01}
                              prefix="$"
                              placeholder="0.00"
                              className="text-right"
                            />
                          </FormControl>
                          <FormMessage className="text-[10px]" />
                        </FormItem>
                      )}
                    />

                    <FormField
                      control={form.control}
                      name="actualCashAmount"
                      render={({ field }) => (
                        <FormItem className="space-y-1">
                          <FormLabel className="text-xs text-zinc-500 font-medium">
                            Efectivo Contado
                          </FormLabel>
                          <FormControl>
                            <NumericInput
                              value={field.value ?? 0}
                              onChange={field.onChange}
                              onBlur={field.onBlur}
                              ref={field.ref}
                              min={0}
                              step={0.01}
                              prefix="$"
                              placeholder="0.00"
                              className="text-right"
                            />
                          </FormControl>
                          <FormMessage className="text-[10px]" />
                        </FormItem>
                      )}
                    />

                    <FormField
                      control={form.control}
                      name="adjustment"
                      render={({ field }) => (
                        <FormItem className="space-y-1">
                          <FormLabel className="text-xs text-zinc-500 font-medium">
                            Ajuste
                          </FormLabel>
                          <FormControl>
                            <NumericInput
                              value={field.value ?? 0}
                              onChange={field.onChange}
                              onBlur={field.onBlur}
                              ref={field.ref}
                              step={0.01}
                              prefix="$"
                              placeholder="0.00"
                              className="text-right"
                            />
                          </FormControl>
                          <FormMessage className="text-[10px]" />
                        </FormItem>
                      )}
                    />

                    <FormField
                      control={form.control}
                      name="cashWithdrawal"
                      render={({ field }) => (
                        <FormItem className="space-y-1">
                          <FormLabel className="text-xs text-zinc-500 font-medium">
                            Retiro
                          </FormLabel>
                          <FormControl>
                            <NumericInput
                              value={field.value ?? 0}
                              onChange={field.onChange}
                              onBlur={field.onBlur}
                              ref={field.ref}
                              min={0}
                              step={0.01}
                              prefix="$"
                              placeholder="0.00"
                              className="text-right"
                            />
                          </FormControl>
                          <FormMessage className="text-[10px]" />
                        </FormItem>
                      )}
                    />
                  </div>
                </div>

                {/* ── Diferencia — hero element ─────────────────────────── */}
                <div
                  className={cn(
                    'px-6 py-5 border-b transition-colors duration-150',
                    cashDiscrepancy === 0 && 'bg-white border-zinc-100',
                    cashDiscrepancy > 0 && 'bg-emerald-50 border-emerald-100',
                    cashDiscrepancy < 0 && 'bg-rose-50 border-rose-100',
                  )}
                >
                  <div className="flex items-end justify-between">
                    <div>
                      <p className="text-[10px] font-semibold tracking-widest uppercase text-zinc-400 mb-1.5">
                        Diferencia
                      </p>
                      <p
                        className={cn(
                          'font-mono font-bold tabular-nums text-3xl tracking-tight leading-none',
                          cashDiscrepancy === 0 && 'text-zinc-300',
                          cashDiscrepancy > 0 && 'text-emerald-600',
                          cashDiscrepancy < 0 && 'text-rose-600',
                        )}
                      >
                        {discrepancyDisplay}
                      </p>
                    </div>

                    <span
                      className={cn(
                        'text-xs font-semibold px-3 py-1.5 rounded-full',
                        cashDiscrepancy === 0 && 'bg-zinc-100 text-zinc-400',
                        cashDiscrepancy > 0 && 'bg-emerald-100 text-emerald-700',
                        cashDiscrepancy < 0 && 'bg-rose-100 text-rose-700',
                      )}
                    >
                      {cashDiscrepancy === 0
                        ? 'Cuadrado'
                        : cashDiscrepancy > 0
                          ? 'Sobrante'
                          : 'Faltante'}
                    </span>
                  </div>
                </div>

                {/* Notas */}
                <div className="px-6 py-4">
                  <FormField
                    control={form.control}
                    name="notes"
                    render={({ field }) => (
                      <FormItem className="space-y-1">
                        <FormLabel className="text-xs text-zinc-500 font-medium">
                          Notas{' '}
                          <span className="font-normal text-zinc-400">(opcional)</span>
                        </FormLabel>
                        <FormControl>
                          <Textarea
                            placeholder="Observaciones del corte..."
                            className="resize-none h-16 text-sm"
                            {...field}
                          />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />
                </div>

              </div>

              {/* ── Footer — acciones ──────────────────────────────────────── */}
              <div className="px-6 py-4 flex gap-3 justify-end border-t border-zinc-100 bg-zinc-50">
                <Button
                  type="button"
                  variant="outline"
                  onClick={() => onOpenChange(false)}
                  disabled={isLoading}
                >
                  Cancelar
                </Button>
                <Button
                  type="submit"
                  disabled={isLoading || isLoadingTotals}
                  className="bg-zinc-900 hover:bg-zinc-800 text-white"
                >
                  {isLoading ? 'Registrando...' : 'Registrar Corte'}
                </Button>
              </div>

            </form>
          </Form>
        )}
      </DialogContent>
    </Dialog>
  );
}
