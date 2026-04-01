import { useState } from 'react';
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Plus, Pencil, Trash2, Loader2, Lock, Percent } from 'lucide-react';
import { format } from 'date-fns';
import { es } from 'date-fns/locale';

import { Button } from '@/shared/components/ui/button';
import { Input } from '@/shared/components/ui/input';
import { Label } from '@/shared/components/ui/label';
import { ClearableInput } from '@/shared/components/ui/field-input';
import { NumericInput } from '@/shared/components/common/NumericInput';
import { Checkbox } from '@/shared/components/ui/checkbox';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/shared/components/ui/dialog';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/shared/components/ui/alert-dialog';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/shared/components/ui/table';

import { useDiscountsStore } from '@/features/services/stores/discountsStore';
import { useToast } from '@/shared/hooks/use-toast';
import { createDiscountSchema, updateDiscountSchema } from '@/features/services/schemas/discount.schema';
import { TABLE_HEADER_CLASS as TH } from '@/shared/utils/constants';
import { StatusBadge } from '@/shared/components/ui/status-badge';
import type { Discount } from '@/features/services/types/discount';

const DEFAULT_DISCOUNT_ID = 1;

const DISCOUNT_TYPE_LABELS: Record<string, string> = {
  Percentage: 'Porcentaje',
  FixedAmount: 'Monto Fijo',
};

const DISCOUNT_TYPE_PILL: Record<string, string> = {
  Percentage: 'bg-violet-50 text-violet-700',
  FixedAmount: 'bg-amber-50 text-amber-700',
};

interface DiscountFormContentProps {
  discount?: Discount;
  onSubmit: (data: Record<string, unknown>) => Promise<void>;
  onCancel: () => void;
  isSubmitting: boolean;
}

function DiscountFormContent({
  discount,
  onSubmit,
  onCancel,
  isSubmitting,
}: DiscountFormContentProps) {
  const isEdit = !!discount;
  const schema = isEdit ? updateDiscountSchema : createDiscountSchema;

  const {
    register,
    handleSubmit,
    setValue,
    formState: { errors },
    control,
    watch,
  } = useForm({
    resolver: zodResolver(schema),
    defaultValues: isEdit
      ? {
          name: discount.name,
          type: discount.type,
          value: discount.value,
          startDate: discount.startDate ? discount.startDate.substring(0, 10) : '',
          endDate: discount.endDate ? discount.endDate.substring(0, 10) : '',
          isActive: discount.isActive,
        }
      : {
          name: '',
          type: 'Percentage' as const,
          value: 0,
          startDate: '',
          endDate: '',
        },
  });

  const watchedType = watch('type');
  const isPercentageType = watchedType === 'Percentage';

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col max-h-[82vh]">
      <div className="overflow-y-auto flex-1 px-6 py-4 space-y-4">
        <div className="space-y-1">
          <Label className="text-xs text-zinc-500 font-medium">Nombre *</Label>
          <ClearableInput {...register('name')} placeholder="Ej: Cliente Frecuente, Promo Lunes..." hasError={!!errors.name} onClear={() => setValue('name', '')} />
          {errors.name && (
            <p className="text-xs text-rose-500">{errors.name.message as string}</p>
          )}
        </div>

        <div className="space-y-1">
          <Label className="text-xs text-zinc-500 font-medium">Tipo *</Label>
          <Controller
            name="type"
            control={control}
            render={({ field }) => (
              <Select value={field.value ?? ''} onValueChange={field.onChange}>
                <SelectTrigger hasError={!!errors.type}>
                  <SelectValue placeholder="Selecciona tipo de descuento" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="Percentage">Porcentaje</SelectItem>
                  <SelectItem value="FixedAmount">Monto Fijo</SelectItem>
                </SelectContent>
              </Select>
            )}
          />
          {errors.type && (
            <p className="text-xs text-rose-500">{errors.type.message as string}</p>
          )}
        </div>

        <div className="space-y-1">
          <Label className="text-xs text-zinc-500 font-medium">
            Valor {isPercentageType ? '(%)' : '($)'}
          </Label>
          <Controller
            name="value"
            control={control}
            render={({ field }) => (
              <NumericInput
                prefix={isPercentageType ? '%' : '$'}
                step={isPercentageType ? 1 : 0.01}
                min={0}
                max={isPercentageType ? 100 : undefined}
                value={field.value ?? ''}
                onChange={field.onChange}
                onBlur={field.onBlur}
                className="text-right"
              />
            )}
          />
          {errors.value && (
            <p className="text-xs text-rose-500">{errors.value.message as string}</p>
          )}
        </div>

        <div className="grid grid-cols-2 gap-3">
          <div className="space-y-1">
            <Label className="text-xs text-zinc-500 font-medium">Válido desde *</Label>
            <Input type="date" {...register('startDate')} />
            {errors.startDate && (
              <p className="text-xs text-rose-500">{errors.startDate.message as string}</p>
            )}
          </div>
          <div className="space-y-1">
            <Label className="text-xs text-zinc-500 font-medium">Válido hasta</Label>
            <Input type="date" {...register('endDate')} />
            {errors.endDate && (
              <p className="text-xs text-rose-500">{errors.endDate.message as string}</p>
            )}
          </div>
        </div>

        {isEdit && (
          <div className="flex items-center gap-2 pt-1">
            <Controller
              name="isActive"
              control={control}
              render={({ field }) => (
                <Checkbox
                  id="dsc-isActive"
                  checked={field.value ?? true}
                  onCheckedChange={field.onChange}
                />
              )}
            />
            <Label
              htmlFor="dsc-isActive"
              className="text-xs text-zinc-500 font-medium cursor-pointer"
            >
              Descuento activo
            </Label>
          </div>
        )}
      </div>

      <div className="px-6 py-4 flex gap-3 justify-end border-t border-zinc-100 bg-zinc-50">
        <Button type="button" variant="outline" onClick={onCancel} disabled={isSubmitting}>
          Cancelar
        </Button>
        <Button
          type="submit"
          disabled={isSubmitting}
        >
          {isSubmitting && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
          Guardar
        </Button>
      </div>
    </form>
  );
}

function getValueDisplay(discount: Discount): string {
  if (discount.type === 'Percentage') return `${discount.value}%`;
  return `$${discount.value.toFixed(2)}`;
}

function getValidityDisplay(discount: Discount): string {
  if (!discount.startDate && !discount.endDate) return 'Siempre';
  const from = discount.startDate
    ? format(new Date(discount.startDate), 'MM/yy', { locale: es })
    : '?';
  const until = discount.endDate
    ? format(new Date(discount.endDate), 'MM/yy', { locale: es })
    : '∞';
  return `${from} — ${until}`;
}

export function DiscountsSection() {
  const { discounts, isLoading, createDiscount, updateDiscount, deleteDiscount } =
    useDiscountsStore();
  const { toast } = useToast();

  const [dialogOpen, setDialogOpen] = useState(false);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [selectedDiscount, setSelectedDiscount] = useState<Discount | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleOpenCreate = () => {
    setSelectedDiscount(null);
    setDialogOpen(true);
  };

  const handleOpenEdit = (discount: Discount) => {
    setSelectedDiscount(discount);
    setDialogOpen(true);
  };

  const handleSubmit = async (data: Record<string, unknown>) => {
    setIsSubmitting(true);
    const payload = {
      ...data,
      startDate: data.startDate || undefined,
      endDate: data.endDate || undefined,
    };

    try {
      if (selectedDiscount) {
        await updateDiscount(selectedDiscount.id, payload as any);
        toast({ title: 'Descuento actualizado correctamente' });
      } else {
        await createDiscount(payload as any);
        toast({ title: 'Descuento creado correctamente' });
      }
      setDialogOpen(false);
    } catch (err) {
      const message = err instanceof Error ? err.message : 'No se pudo guardar el descuento';
      toast({ title: 'Error', description: message, variant: 'destructive' });
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleDelete = async () => {
    if (!selectedDiscount) return;
    try {
      await deleteDiscount(selectedDiscount.id);
      toast({ title: 'Descuento eliminado' });
      setDeleteDialogOpen(false);
      setSelectedDiscount(null);
    } catch {
      toast({
        title: 'Error',
        description: 'No se pudo eliminar el descuento',
        variant: 'destructive',
      });
    }
  };

  return (
    <div>
      <div className="flex items-center justify-between px-6 py-5 border-b border-zinc-100">
        <h2 className="text-sm font-semibold text-zinc-900 tracking-tight">
          Descuentos
          <span className="ml-2 font-mono font-normal text-xs text-zinc-400">{discounts.length}</span>
        </h2>
        <Button
          onClick={handleOpenCreate}
          size="sm"
        >
          <Plus className="h-4 w-4 mr-1" />
          Nuevo Descuento
        </Button>
      </div>

      {isLoading && discounts.length === 0 ? (
        <div className="flex justify-center py-12">
          <Loader2 className="h-6 w-6 animate-spin text-zinc-400" />
        </div>
      ) : discounts.length === 0 ? (
        <div className="py-16 flex flex-col items-center gap-3">
          <div className="w-10 h-10 rounded-full bg-zinc-50 border border-zinc-100 flex items-center justify-center">
            <Percent className="h-4 w-4 text-zinc-300" />
          </div>
          <div className="text-center">
            <p className="text-sm font-medium text-zinc-400">Sin descuentos</p>
            <p className="text-xs text-zinc-300 mt-0.5">Crea promociones de porcentaje o monto fijo</p>
          </div>
        </div>
      ) : (
        <Table>
          <TableHeader>
            <TableRow className="bg-zinc-50 hover:bg-zinc-50">
              <TableHead className={TH}>Nombre</TableHead>
              <TableHead className={TH}>Tipo</TableHead>
              <TableHead className={`${TH} text-right`}>Valor</TableHead>
              <TableHead className={TH}>Vigencia</TableHead>
              <TableHead className={TH}>Estado</TableHead>
              <TableHead className="w-20" />
            </TableRow>
          </TableHeader>
          <TableBody>
            {discounts.map((discount) => {
              const isDefault = discount.id === DEFAULT_DISCOUNT_ID;
              return (
                <TableRow
                  key={discount.id}
                  className="border-b border-zinc-100 hover:bg-zinc-50 transition-colors"
                >
                  <TableCell className="text-sm font-medium text-zinc-900">
                    <div className="flex items-center gap-1.5">
                      {isDefault && <Lock className="h-3 w-3 text-zinc-400 shrink-0" />}
                      {discount.name}
                    </div>
                  </TableCell>

                  <TableCell>
                    <span
                      className={`inline-flex items-center px-2 py-0.5 rounded-full text-[10px] font-semibold ${
                        DISCOUNT_TYPE_PILL[discount.type] ?? 'bg-zinc-100 text-zinc-400'
                      }`}
                    >
                      {DISCOUNT_TYPE_LABELS[discount.type] ?? discount.type}
                    </span>
                  </TableCell>

                  <TableCell className="text-right font-mono tabular-nums text-sm font-medium text-zinc-900">
                    {getValueDisplay(discount)}
                  </TableCell>

                  <TableCell className="text-sm text-zinc-500">
                    {getValidityDisplay(discount)}
                  </TableCell>

                  <TableCell>
                    <StatusBadge active={discount.isActive} />
                  </TableCell>

                  <TableCell className="text-right">
                    <div className="flex justify-end gap-1">
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => handleOpenEdit(discount)}
                      >
                        <Pencil className="h-4 w-4 text-zinc-400" />
                      </Button>
                      <Button
                        variant="ghost"
                        size="sm"
                        disabled={isDefault}
                        onClick={() => {
                          setSelectedDiscount(discount);
                          setDeleteDialogOpen(true);
                        }}
                      >
                        <Trash2
                          className={`h-4 w-4 ${isDefault ? 'text-zinc-300' : 'text-rose-500'}`}
                        />
                      </Button>
                    </div>
                  </TableCell>
                </TableRow>
              );
            })}
          </TableBody>
        </Table>
      )}

      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent className="max-w-md p-0 gap-0 overflow-hidden">
          <DialogHeader className="px-6 pt-5 pb-4 border-b border-zinc-100">
            <DialogTitle className="text-sm font-semibold text-zinc-900 tracking-tight">
              {selectedDiscount ? 'Editar Descuento' : 'Nuevo Descuento'}
            </DialogTitle>
          </DialogHeader>
          <DiscountFormContent
            discount={selectedDiscount ?? undefined}
            onSubmit={handleSubmit}
            onCancel={() => setDialogOpen(false)}
            isSubmitting={isSubmitting}
          />
        </DialogContent>
      </Dialog>

      <AlertDialog open={deleteDialogOpen} onOpenChange={setDeleteDialogOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>¿Eliminar descuento?</AlertDialogTitle>
            <AlertDialogDescription>
              Esta acción no se puede deshacer. El descuento "{selectedDiscount?.name}" será
              eliminado permanentemente.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancelar</AlertDialogCancel>
            <AlertDialogAction
              onClick={handleDelete}
              className="bg-rose-600 hover:bg-rose-700 text-white"
            >
              Eliminar
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}
