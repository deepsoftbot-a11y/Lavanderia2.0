import { useState } from 'react';
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Plus, Pencil, Trash2, Loader2, Package } from 'lucide-react';

import { Button } from '@/shared/components/ui/button';
import { Input } from '@/shared/components/ui/input';
import { Label } from '@/shared/components/ui/label';
import { ClearableInput } from '@/shared/components/ui/field-input';
import { CurrencyInput } from '@/shared/components/ui/currency-input';
import { Textarea } from '@/shared/components/ui/textarea';
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

import { useServicesStore } from '@/features/services/stores/servicesStore';
import { useCategoriesStore } from '@/features/services/stores/categoriesStore';
import { useToast } from '@/shared/hooks/use-toast';
import { createServiceSchema, updateServiceSchema } from '@/features/services/schemas/service.schema';
import { CHARGE_TYPE } from '@/features/services/types/service';
import type { Service } from '@/features/services/types/service';
import { GarmentPricesSubsection } from './GarmentPricesSubsection';
import { NewGarmentDialog } from './NewGarmentDialog';
import { useGarmentTypesStore } from '@/features/services/stores/garmentTypesStore';
import { useServiceGarmentsStore } from '@/features/services/stores/serviceGarmentsStore';
import type { GarmentPriceInput } from '@/features/services/types/service';

interface ServiceFormContentProps {
  service?: Service;
  onSubmit: (data: Record<string, unknown>) => Promise<void>;
  onCancel: () => void;
  isSubmitting: boolean;
  initialGarmentPrices?: GarmentPriceInput[];
}

function ServiceFormContent({
  service,
  onSubmit,
  onCancel,
  isSubmitting,
  initialGarmentPrices,
}: ServiceFormContentProps) {
  const { categories } = useCategoriesStore();
  const { garmentTypes } = useGarmentTypesStore();
  const [garmentPrices, setGarmentPrices] = useState<GarmentPriceInput[]>(
    initialGarmentPrices ?? []
  );
  const [newGarmentOpen, setNewGarmentOpen] = useState(false);
  const isEdit = !!service;
  const schema = isEdit ? updateServiceSchema : createServiceSchema;

  const { register, handleSubmit, setValue, formState: { errors }, control, watch } = useForm({
    resolver: zodResolver(schema),
    defaultValues: isEdit
      ? {
          code: service.code,
          name: service.name,
          description: service.description ?? '',
          categoryId: service.categoryId,
          chargeType: service.chargeType,
          pricePerKg: service.pricePerKg,
          minWeight: service.minWeight,
          maxWeight: service.maxWeight,
          estimatedTime: service.estimatedTime,
          isActive: service.isActive,
        }
      : {
          name: '',
          description: '',
          chargeType: CHARGE_TYPE.PorPeso,
        },
  });

  const chargeType = watch('chargeType');

  return (
    <form onSubmit={handleSubmit((data) => onSubmit({ ...data, garmentPrices }))} className="flex flex-col max-h-[82vh]">
      <div className="overflow-y-auto flex-1 px-6 py-4 space-y-4">
        <div className={isEdit ? "grid grid-cols-2 gap-3" : ""}>
          {isEdit && (
            <div className="space-y-1">
              <Label className="text-xs text-zinc-500 font-medium">Código</Label>
              <div className="flex h-11 items-center rounded-xl bg-zinc-100 px-4 text-sm font-mono text-zinc-400 tracking-wide">
                {service?.code ?? '—'}
              </div>
            </div>
          )}

          <div className="space-y-1">
            <Label className="text-xs text-zinc-500 font-medium">Nombre *</Label>
            <ClearableInput {...register('name')} placeholder="Ej: Lavado, Planchado..." hasError={!!errors.name} onClear={() => setValue('name', '')} />
            {errors.name && (
              <p className="text-xs text-rose-500">{errors.name.message as string}</p>
            )}
          </div>
        </div>

        <div className="space-y-1">
          <Label className="text-xs text-zinc-500 font-medium">Descripción</Label>
          <Textarea {...register('description')} placeholder="Descripción opcional..." rows={2} />
        </div>

        <div className="space-y-1">
          <Label className="text-xs text-zinc-500 font-medium">Categoría *</Label>
          <Controller
            name="categoryId"
            control={control}
            render={({ field }) => (
              <Select
                value={field.value?.toString() ?? ''}
                onValueChange={(v) => field.onChange(parseInt(v, 10))}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Selecciona una categoría" />
                </SelectTrigger>
                <SelectContent>
                  {categories.map((cat) => (
                    <SelectItem key={cat.id} value={cat.id.toString()}>
                      {cat.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            )}
          />
          {errors.categoryId && (
            <p className="text-xs text-rose-500">{errors.categoryId.message as string}</p>
          )}
        </div>

        <div className="space-y-1">
          <Label className="text-xs text-zinc-500 font-medium">Tipo de cobro *</Label>
          <Controller
            name="chargeType"
            control={control}
            render={({ field }) => (
              <Select value={field.value ?? ''} onValueChange={field.onChange}>
                <SelectTrigger>
                  <SelectValue placeholder="Selecciona tipo de cobro" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="PorPeso">Por peso (kilo)</SelectItem>
                  <SelectItem value="PorPieza">Por pieza</SelectItem>
                </SelectContent>
              </Select>
            )}
          />
          {errors.chargeType && (
            <p className="text-xs text-rose-500">{errors.chargeType.message as string}</p>
          )}
        </div>

        {chargeType === CHARGE_TYPE.PorPeso && (
          <div className="grid grid-cols-3 gap-3">
            <div className="space-y-1">
              <Label className="text-xs text-zinc-500 font-medium">Precio / kg *</Label>
              <Controller
                name="pricePerKg"
                control={control}
                render={({ field }) => (
                  <CurrencyInput
                    value={field.value ?? ''}
                    onChange={field.onChange}
                    onBlur={field.onBlur}
                  />
                )}
              />
              {errors.pricePerKg && (
                <p className="text-xs text-rose-500">{errors.pricePerKg.message as string}</p>
              )}
            </div>

            <div className="space-y-1">
              <Label className="text-xs text-zinc-500 font-medium">Peso mín.</Label>
              <Input
                type="number"
                step="0.5"
                min="0"
                {...register('minWeight', {
                  setValueAs: (v) => (v === '' || v === undefined ? undefined : parseFloat(v)),
                })}
                placeholder="—"
                className="font-mono tabular-nums"
              />
            </div>

            <div className="space-y-1">
              <Label className="text-xs text-zinc-500 font-medium">Peso máx.</Label>
              <Input
                type="number"
                step="0.5"
                min="0"
                {...register('maxWeight', {
                  setValueAs: (v) => (v === '' || v === undefined ? undefined : parseFloat(v)),
                })}
                placeholder="—"
                className="font-mono tabular-nums"
              />
              {errors.maxWeight && (
                <p className="text-xs text-rose-500">{errors.maxWeight.message as string}</p>
              )}
            </div>
          </div>
        )}

        <div className="space-y-1">
          <Label className="text-xs text-zinc-500 font-medium">Tiempo estimado (min)</Label>
          <Input
            type="number"
            min="0"
            step="1"
            {...register('estimatedTime', {
              setValueAs: (v) => (v === '' || v === undefined ? undefined : parseInt(v, 10)),
            })}
            placeholder="—"
            className="font-mono tabular-nums"
          />
        </div>

        {chargeType === CHARGE_TYPE.PorPieza && (
          <div className="space-y-2 pt-3 border-t border-zinc-100">
            <div className="flex items-center justify-between">
              <Label className="text-xs text-zinc-500 font-medium">Prendas y Precios</Label>
              <button
                type="button"
                onClick={() => setNewGarmentOpen(true)}
                className="flex items-center gap-1 text-xs text-blue-600 hover:text-blue-700 font-medium"
              >
                <Plus className="h-3 w-3" />
                Nueva prenda
              </button>
            </div>
            <GarmentPricesSubsection
              garmentTypes={garmentTypes}
              value={garmentPrices}
              onChange={setGarmentPrices}
            />
            <NewGarmentDialog
              open={newGarmentOpen}
              onClose={() => setNewGarmentOpen(false)}
              onCreated={(entry) => setGarmentPrices((prev) => [...prev, entry])}
            />
          </div>
        )}

        {isEdit && (
          <div className="flex items-center gap-2 pt-1">
            <Controller
              name="isActive"
              control={control}
              render={({ field }) => (
                <Checkbox
                  id="svc-isActive"
                  checked={field.value ?? true}
                  onCheckedChange={field.onChange}
                />
              )}
            />
            <Label htmlFor="svc-isActive" className="text-xs text-zinc-500 font-medium cursor-pointer">
              Servicio activo
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

const CHARGE_TYPE_LABEL: Record<string, string> = {
  PorPeso: 'Por kilo',
  PorPieza: 'Por pieza',
};

const TH = 'text-[10px] font-semibold tracking-widest uppercase text-zinc-400';

export function ServicesSection() {
  const { services, isLoading, createService, updateService, deleteService } = useServicesStore();
  const { serviceGarments, fetchServiceGarments } = useServiceGarmentsStore();
  const { toast } = useToast();

  const [dialogOpen, setDialogOpen] = useState(false);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [selectedService, setSelectedService] = useState<Service | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [initialGarmentPrices, setInitialGarmentPrices] = useState<GarmentPriceInput[]>([]);

  const handleOpenCreate = () => {
    setSelectedService(null);
    setInitialGarmentPrices([]);
    setDialogOpen(true);
  };

  const handleOpenEdit = (service: Service) => {
    setSelectedService(service);
    const prices = serviceGarments
      .filter((sg) => sg.serviceId === service.id && sg.isActive)
      .map((sg) => ({ garmentTypeId: sg.garmentTypeId, unitPrice: sg.unitPrice }));
    setInitialGarmentPrices(prices);
    setDialogOpen(true);
  };

  const handleSubmit = async (data: Record<string, unknown>) => {
    setIsSubmitting(true);
    try {
      const { garmentPrices, ...serviceData } = data as Record<string, unknown> & { garmentPrices?: GarmentPriceInput[] };
      if (selectedService) {
        await updateService(selectedService.id, { ...serviceData, garmentPrices } as any);
        toast({ title: 'Servicio actualizado correctamente' });
      } else {
        await createService({ ...serviceData, garmentPrices } as any);
        toast({ title: 'Servicio creado correctamente' });
      }
      await fetchServiceGarments();
      setDialogOpen(false);
    } catch (err) {
      const message = err instanceof Error ? err.message : 'No se pudo guardar el servicio';
      toast({ title: 'Error', description: message, variant: 'destructive' });
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleDelete = async () => {
    if (!selectedService) return;
    try {
      await deleteService(selectedService.id);
      toast({ title: 'Servicio eliminado' });
      setDeleteDialogOpen(false);
      setSelectedService(null);
    } catch {
      toast({ title: 'Error', description: 'No se pudo eliminar el servicio', variant: 'destructive' });
    }
  };

  return (
    <div>
      <div className="flex items-center justify-between px-6 py-5 border-b border-zinc-100">
        <h2 className="text-sm font-semibold text-zinc-900 tracking-tight">
          Servicios
          <span className="ml-2 font-mono font-normal text-xs text-zinc-400">{services.length}</span>
        </h2>
        <Button onClick={handleOpenCreate} size="sm" >
          <Plus className="h-4 w-4 mr-1" />
          Nuevo Servicio
        </Button>
      </div>

      {isLoading && services.length === 0 ? (
        <div className="flex justify-center py-12">
          <Loader2 className="h-6 w-6 animate-spin text-zinc-400" />
        </div>
      ) : services.length === 0 ? (
        <div className="py-16 flex flex-col items-center gap-3">
          <div className="w-10 h-10 rounded-full bg-zinc-50 border border-zinc-100 flex items-center justify-center">
            <Package className="h-4 w-4 text-zinc-300" />
          </div>
          <div className="text-center">
            <p className="text-sm font-medium text-zinc-400">Sin servicios</p>
            <p className="text-xs text-zinc-300 mt-0.5">Crea el primer servicio para comenzar</p>
          </div>
        </div>
      ) : (
        <Table>
          <TableHeader>
            <TableRow className="bg-zinc-50 hover:bg-zinc-50">
              <TableHead className={TH}>Servicio</TableHead>
              <TableHead className={TH}>Categoría</TableHead>
              <TableHead className={TH}>Tarifa</TableHead>
              <TableHead className={TH}>Estado</TableHead>
              <TableHead className="w-20" />
            </TableRow>
          </TableHeader>
          <TableBody>
            {services.map((service) => (
              <TableRow key={service.id} className="border-b border-zinc-100 hover:bg-zinc-50 transition-colors">
                <TableCell>
                  <div>
                    <p className="text-[10px] font-mono text-zinc-400 uppercase tracking-wide mb-0.5">
                      {service.code}
                    </p>
                    <p className="text-sm font-medium text-zinc-900">{service.name}</p>
                  </div>
                </TableCell>

                <TableCell className="text-sm text-zinc-500">
                  {service.category?.name ?? '—'}
                </TableCell>

                <TableCell>
                  <div>
                    <p className="text-xs text-zinc-400 mb-0.5">
                      {CHARGE_TYPE_LABEL[service.chargeType] ?? service.chargeType}
                    </p>
                    {service.chargeType === CHARGE_TYPE.PorPeso && service.pricePerKg !== undefined ? (
                      <p className="text-sm font-mono font-medium text-zinc-900">
                        ${service.pricePerKg.toFixed(2)}
                        <span className="text-xs text-zinc-400 font-sans ml-0.5">/kg</span>
                      </p>
                    ) : (
                      <p className="text-xs text-zinc-400 italic">Ver Precios</p>
                    )}
                  </div>
                </TableCell>

                <TableCell>
                  <span className={`inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-[10px] font-semibold ${service.isActive ? 'bg-emerald-50 text-emerald-700' : 'bg-zinc-100 text-zinc-400'}`}>
                    <span className={`w-1.5 h-1.5 rounded-full shrink-0 ${service.isActive ? 'bg-emerald-500' : 'bg-zinc-400'}`} />
                    {service.isActive ? 'Activo' : 'Inactivo'}
                  </span>
                </TableCell>

                <TableCell className="text-right">
                  <div className="flex justify-end gap-1">
                    <Button variant="ghost" size="sm" onClick={() => handleOpenEdit(service)}>
                      <Pencil className="h-4 w-4 text-zinc-400" />
                    </Button>
                    <Button variant="ghost" size="sm" onClick={() => { setSelectedService(service); setDeleteDialogOpen(true); }}>
                      <Trash2 className="h-4 w-4 text-rose-500" />
                    </Button>
                  </div>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      )}

      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent className="max-w-2xl p-0 gap-0 overflow-hidden">
          <DialogHeader className="px-6 pt-5 pb-4 border-b border-zinc-100">
            <DialogTitle className="text-sm font-semibold text-zinc-900 tracking-tight">
              {selectedService ? 'Editar Servicio' : 'Nuevo Servicio'}
            </DialogTitle>
          </DialogHeader>
          <ServiceFormContent
            service={selectedService ?? undefined}
            onSubmit={handleSubmit}
            onCancel={() => setDialogOpen(false)}
            isSubmitting={isSubmitting}
            initialGarmentPrices={initialGarmentPrices}
          />
        </DialogContent>
      </Dialog>

      <AlertDialog open={deleteDialogOpen} onOpenChange={setDeleteDialogOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>¿Eliminar servicio?</AlertDialogTitle>
            <AlertDialogDescription>
              Esta acción no se puede deshacer. El servicio "{selectedService?.name}" será eliminado permanentemente.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancelar</AlertDialogCancel>
            <AlertDialogAction onClick={handleDelete} className="bg-rose-600 hover:bg-rose-700 text-white">
              Eliminar
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}
