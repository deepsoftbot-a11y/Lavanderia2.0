import { useState } from 'react';
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Plus, Pencil, Trash2, Loader2, Shirt } from 'lucide-react';

import { Button } from '@/shared/components/ui/button';
import { Input } from '@/shared/components/ui/input';
import { Label } from '@/shared/components/ui/label';
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
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/shared/components/ui/table';

import { useGarmentTypesStore } from '@/features/services/stores/garmentTypesStore';
import { useToast } from '@/shared/hooks/use-toast';
import {
  createGarmentTypeSchema,
  updateGarmentTypeSchema,
} from '@/features/services/schemas/garmentType.schema';
import type { GarmentType } from '@/features/services/types/garmentType';

interface GarmentTypeFormContentProps {
  garmentType?: GarmentType;
  onSubmit: (data: Record<string, unknown>) => Promise<void>;
  onCancel: () => void;
  isSubmitting: boolean;
}

function GarmentTypeFormContent({
  garmentType,
  onSubmit,
  onCancel,
  isSubmitting,
}: GarmentTypeFormContentProps) {
  const isEdit = !!garmentType;
  const schema = isEdit ? updateGarmentTypeSchema : createGarmentTypeSchema;

  const {
    register,
    handleSubmit,
    formState: { errors },
    control,
  } = useForm({
    resolver: zodResolver(schema),
    defaultValues: isEdit
      ? {
          name: garmentType.name,
          description: garmentType.description ?? '',
          isActive: garmentType.isActive,
        }
      : {
          name: '',
          description: '',
        },
  });

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col max-h-[82vh]">
      <div className="overflow-y-auto flex-1 px-6 py-4 space-y-4">
        <div className="space-y-1">
          <Label className="text-xs text-zinc-500 font-medium">Nombre *</Label>
          <Input {...register('name')} placeholder="Ej: Camisa, Edredón, Cortina..." />
          {errors.name && (
            <p className="text-xs text-rose-500">{errors.name.message as string}</p>
          )}
        </div>

        <div className="space-y-1">
          <Label className="text-xs text-zinc-500 font-medium">Descripción</Label>
          <Textarea
            {...register('description')}
            placeholder="Descripción opcional..."
            rows={2}
          />
        </div>

        {isEdit && (
          <div className="flex items-center gap-2 pt-1">
            <Controller
              name="isActive"
              control={control}
              render={({ field }) => (
                <Checkbox
                  id="gmt-isActive"
                  checked={field.value ?? true}
                  onCheckedChange={field.onChange}
                />
              )}
            />
            <Label
              htmlFor="gmt-isActive"
              className="text-xs text-zinc-500 font-medium cursor-pointer"
            >
              Tipo de prenda activo
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
          className="bg-zinc-900 hover:bg-zinc-800 text-white"
          disabled={isSubmitting}
        >
          {isSubmitting && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
          Guardar
        </Button>
      </div>
    </form>
  );
}

const TH = 'text-[10px] font-semibold tracking-widest uppercase text-zinc-400';

export function GarmentTypesSection() {
  const {
    garmentTypes,
    isLoading,
    createGarmentType,
    updateGarmentType,
    deleteGarmentType,
  } = useGarmentTypesStore();
  const { toast } = useToast();

  const [dialogOpen, setDialogOpen] = useState(false);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [selectedGarmentType, setSelectedGarmentType] = useState<GarmentType | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleOpenCreate = () => {
    setSelectedGarmentType(null);
    setDialogOpen(true);
  };

  const handleOpenEdit = (garmentType: GarmentType) => {
    setSelectedGarmentType(garmentType);
    setDialogOpen(true);
  };

  const handleSubmit = async (data: Record<string, unknown>) => {
    setIsSubmitting(true);
    try {
      if (selectedGarmentType) {
        await updateGarmentType(selectedGarmentType.id, data as any);
        toast({ title: 'Tipo de prenda actualizado correctamente' });
      } else {
        await createGarmentType(data as any);
        toast({ title: 'Tipo de prenda creado correctamente' });
      }
      setDialogOpen(false);
    } catch (err) {
      const message = err instanceof Error ? err.message : 'No se pudo guardar el tipo de prenda';
      toast({ title: 'Error', description: message, variant: 'destructive' });
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleDelete = async () => {
    if (!selectedGarmentType) return;
    try {
      await deleteGarmentType(selectedGarmentType.id);
      toast({ title: 'Tipo de prenda eliminado' });
      setDeleteDialogOpen(false);
      setSelectedGarmentType(null);
    } catch {
      toast({
        title: 'Error',
        description: 'No se pudo eliminar el tipo de prenda',
        variant: 'destructive',
      });
    }
  };

  return (
    <div>
      <div className="flex items-center justify-between px-6 py-5 border-b border-zinc-100">
        <h2 className="text-sm font-semibold text-zinc-900 tracking-tight">
          Tipos de Prenda
          <span className="ml-2 font-mono font-normal text-xs text-zinc-400">{garmentTypes.length}</span>
        </h2>
        <Button
          onClick={handleOpenCreate}
          size="sm"
          className="bg-zinc-900 hover:bg-zinc-800 text-white"
        >
          <Plus className="h-4 w-4 mr-1" />
          Nuevo Tipo de Prenda
        </Button>
      </div>

      {isLoading && garmentTypes.length === 0 ? (
        <div className="flex justify-center py-12">
          <Loader2 className="h-6 w-6 animate-spin text-zinc-400" />
        </div>
      ) : garmentTypes.length === 0 ? (
        <div className="py-16 flex flex-col items-center gap-3">
          <div className="w-10 h-10 rounded-full bg-zinc-50 border border-zinc-100 flex items-center justify-center">
            <Shirt className="h-4 w-4 text-zinc-300" />
          </div>
          <div className="text-center">
            <p className="text-sm font-medium text-zinc-400">Sin tipos de prenda</p>
            <p className="text-xs text-zinc-300 mt-0.5">Define las prendas disponibles para los servicios por pieza</p>
          </div>
        </div>
      ) : (
        <Table>
          <TableHeader>
            <TableRow className="bg-zinc-50 hover:bg-zinc-50">
              <TableHead className={TH}>Nombre</TableHead>
              <TableHead className={TH}>Descripción</TableHead>
              <TableHead className={TH}>Estado</TableHead>
              <TableHead className="w-20" />
            </TableRow>
          </TableHeader>
          <TableBody>
            {garmentTypes.map((garmentType) => (
              <TableRow
                key={garmentType.id}
                className="border-b border-zinc-100 hover:bg-zinc-50 transition-colors"
              >
                <TableCell className="text-sm font-medium text-zinc-900">
                  {garmentType.name}
                </TableCell>
                <TableCell className="text-sm text-zinc-500">
                  {garmentType.description ?? '—'}
                </TableCell>
                <TableCell>
                  <span
                    className={`inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-[10px] font-semibold ${
                      garmentType.isActive
                        ? 'bg-emerald-50 text-emerald-700'
                        : 'bg-zinc-100 text-zinc-400'
                    }`}
                  >
                    <span className={`w-1.5 h-1.5 rounded-full shrink-0 ${garmentType.isActive ? 'bg-emerald-500' : 'bg-zinc-400'}`} />
                    {garmentType.isActive ? 'Activo' : 'Inactivo'}
                  </span>
                </TableCell>
                <TableCell className="text-right">
                  <div className="flex justify-end gap-1">
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => handleOpenEdit(garmentType)}
                    >
                      <Pencil className="h-4 w-4 text-zinc-400" />
                    </Button>
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => {
                        setSelectedGarmentType(garmentType);
                        setDeleteDialogOpen(true);
                      }}
                    >
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
        <DialogContent className="max-w-sm p-0 gap-0 overflow-hidden">
          <DialogHeader className="px-6 pt-5 pb-4 border-b border-zinc-100">
            <DialogTitle className="text-sm font-semibold text-zinc-900 tracking-tight">
              {selectedGarmentType ? 'Editar Tipo de Prenda' : 'Nuevo Tipo de Prenda'}
            </DialogTitle>
          </DialogHeader>
          <GarmentTypeFormContent
            garmentType={selectedGarmentType ?? undefined}
            onSubmit={handleSubmit}
            onCancel={() => setDialogOpen(false)}
            isSubmitting={isSubmitting}
          />
        </DialogContent>
      </Dialog>

      <AlertDialog open={deleteDialogOpen} onOpenChange={setDeleteDialogOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>¿Eliminar tipo de prenda?</AlertDialogTitle>
            <AlertDialogDescription>
              Esta acción no se puede deshacer. El tipo de prenda "{selectedGarmentType?.name}"
              será eliminado permanentemente.
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
