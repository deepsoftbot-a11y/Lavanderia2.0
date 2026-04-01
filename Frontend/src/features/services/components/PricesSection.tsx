import { useState } from 'react';
import { Plus, Pencil, Trash2, Loader2, Receipt, Check, ChevronsUpDown } from 'lucide-react';

import { Button } from '@/shared/components/ui/button';
import { Label } from '@/shared/components/ui/label';
import { CurrencyInput } from '@/shared/components/ui/currency-input';
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
  Command,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList,
} from '@/shared/components/ui/command';
import { Popover, PopoverContent, PopoverTrigger } from '@/shared/components/ui/popover';
import { cn } from '@/shared/utils/cn';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/shared/components/ui/table';

import { useServicesStore } from '@/features/services/stores/servicesStore';
import { CHARGE_TYPE } from '@/features/services/types/service';
import { useGarmentTypesStore } from '@/features/services/stores/garmentTypesStore';
import { useServiceGarmentsStore } from '@/features/services/stores/serviceGarmentsStore';
import { useToast } from '@/shared/hooks/use-toast';
import type { ServiceGarment } from '@/features/services/types/serviceGarment';

// --- Create form ---

interface CreatePriceContentProps {
  onClose: () => void;
}

function CreatePriceContent({ onClose }: CreatePriceContentProps) {
  const { services } = useServicesStore();
  const { garmentTypes } = useGarmentTypesStore();
  const { serviceGarments, createServiceGarment } = useServiceGarmentsStore();
  const { toast } = useToast();

  const [selectedServiceId, setSelectedServiceId] = useState('');
  const [selectedGarmentTypeId, setSelectedGarmentTypeId] = useState('');
  const [unitPrice, setUnitPrice] = useState<number | ''>('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [serviceOpen, setServiceOpen] = useState(false);
  const [serviceSearch, setServiceSearch] = useState('');
  const [garmentOpen, setGarmentOpen] = useState(false);
  const [garmentSearch, setGarmentSearch] = useState('');

  const pieceServices = services.filter((s) => s.chargeType === CHARGE_TYPE.PorPieza && s.isActive);

  const usedGarmentTypeIds = serviceGarments
    .filter((sg) => sg.serviceId === parseInt(selectedServiceId, 10))
    .map((sg) => sg.garmentTypeId);

  const availableGarmentTypes = garmentTypes.filter(
    (gt) => gt.isActive && !usedGarmentTypeIds.includes(gt.id)
  );

  const handleSave = async () => {
    if (!unitPrice || unitPrice <= 0) {
      toast({
        title: 'Precio inválido',
        description: 'Ingresa un precio mayor a 0',
        variant: 'destructive',
      });
      return;
    }
    setIsSubmitting(true);
    try {
      await createServiceGarment({
        serviceId: parseInt(selectedServiceId, 10),
        garmentTypeId: parseInt(selectedGarmentTypeId, 10),
        unitPrice,
      });
      toast({ title: 'Precio establecido' });
      onClose();
    } catch (err) {
      const message = err instanceof Error ? err.message : 'No se pudo guardar el precio';
      toast({ title: 'Error', description: message, variant: 'destructive' });
    } finally {
      setIsSubmitting(false);
    }
  };

  const canSave = !!selectedServiceId && !!selectedGarmentTypeId && !!unitPrice && unitPrice > 0 && !isSubmitting;

  return (
    <>
      <div className="px-6 py-4 space-y-4">
        <div className="space-y-1">
          <Label className="text-xs text-zinc-500 font-medium">Servicio *</Label>
          <Popover open={serviceOpen} onOpenChange={setServiceOpen}>
            <PopoverTrigger asChild>
              <button className="flex h-11 w-full items-center justify-between whitespace-nowrap rounded-xl border-2 border-transparent bg-zinc-100 px-4 text-sm text-indigo-900 outline-none transition-all duration-150 hover:bg-zinc-200 focus:border-blue-600 focus:bg-blue-50 disabled:cursor-not-allowed disabled:opacity-50 cursor-pointer">
                {selectedServiceId
                  ? pieceServices.find((s) => s.id.toString() === selectedServiceId)?.name
                  : <span className="text-zinc-400">Seleccionar servicio</span>}
                <ChevronsUpDown className="h-4 w-4 shrink-0 text-zinc-400" />
              </button>
            </PopoverTrigger>
            <PopoverContent className="w-[--radix-popover-trigger-width] p-0 rounded-xl border border-zinc-200 shadow-sm" align="start">
              <Command shouldFilter={false}>
                <CommandInput
                  placeholder="Buscar servicio..."
                  value={serviceSearch}
                  onValueChange={setServiceSearch}
                />
                <CommandList>
                  <CommandEmpty>Sin resultados</CommandEmpty>
                  <CommandGroup>
                    {pieceServices
                      .filter((s) => s.name.toLowerCase().includes(serviceSearch.toLowerCase()))
                      .map((s) => (
                        <CommandItem
                          key={s.id}
                          value={s.id.toString()}
                          onSelect={(v) => {
                            setSelectedServiceId(v);
                            setSelectedGarmentTypeId('');
                            setServiceSearch('');
                            setServiceOpen(false);
                          }}
                        >
                          <Check className={cn('mr-2 h-4 w-4', selectedServiceId === s.id.toString() ? 'opacity-100' : 'opacity-0')} />
                          {s.name}
                        </CommandItem>
                      ))}
                  </CommandGroup>
                </CommandList>
              </Command>
            </PopoverContent>
          </Popover>
        </div>

        <div className="space-y-1">
          <Label className="text-xs text-zinc-500 font-medium">Tipo de prenda *</Label>
          <Popover open={garmentOpen} onOpenChange={setGarmentOpen}>
            <PopoverTrigger asChild>
              <button
                disabled={!selectedServiceId}
                className="flex h-11 w-full items-center justify-between whitespace-nowrap rounded-xl border-2 border-transparent bg-zinc-100 px-4 text-sm text-indigo-900 outline-none transition-all duration-150 hover:bg-zinc-200 focus:border-blue-600 focus:bg-blue-50 disabled:cursor-not-allowed disabled:opacity-50 cursor-pointer"
              >
                {selectedGarmentTypeId
                  ? availableGarmentTypes.find((gt) => gt.id.toString() === selectedGarmentTypeId)?.name
                  : <span className="text-zinc-400">Seleccionar tipo de prenda</span>}
                <ChevronsUpDown className="h-4 w-4 shrink-0 text-zinc-400" />
              </button>
            </PopoverTrigger>
            <PopoverContent className="w-[--radix-popover-trigger-width] p-0 rounded-xl border border-zinc-200 shadow-sm" align="start">
              <Command shouldFilter={false}>
                <CommandInput
                  placeholder="Buscar tipo de prenda..."
                  value={garmentSearch}
                  onValueChange={setGarmentSearch}
                />
                <CommandList>
                  <CommandEmpty>Sin resultados</CommandEmpty>
                  <CommandGroup>
                    {availableGarmentTypes
                      .filter((gt) => gt.name.toLowerCase().includes(garmentSearch.toLowerCase()))
                      .map((gt) => (
                        <CommandItem
                          key={gt.id}
                          value={gt.id.toString()}
                          onSelect={(v) => {
                            setSelectedGarmentTypeId(v);
                            setGarmentSearch('');
                            setGarmentOpen(false);
                          }}
                        >
                          <Check className={cn('mr-2 h-4 w-4', selectedGarmentTypeId === gt.id.toString() ? 'opacity-100' : 'opacity-0')} />
                          {gt.name}
                        </CommandItem>
                      ))}
                  </CommandGroup>
                </CommandList>
              </Command>
            </PopoverContent>
          </Popover>
        </div>

        <div className="space-y-1">
          <Label className="text-xs text-zinc-500 font-medium">Precio unitario *</Label>
          <CurrencyInput
            value={unitPrice}
            onChange={setUnitPrice}
          />
        </div>
      </div>

      <div className="px-6 py-4 flex gap-3 justify-end border-t border-zinc-100 bg-zinc-50">
        <Button variant="outline" onClick={onClose} disabled={isSubmitting}>
          Cancelar
        </Button>
        <Button
          onClick={handleSave}
          disabled={!canSave}
        >
          {isSubmitting && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
          Guardar
        </Button>
      </div>
    </>
  );
}

// --- Edit form ---

interface EditPriceContentProps {
  entry: ServiceGarment;
  onClose: () => void;
}

function EditPriceContent({ entry, onClose }: EditPriceContentProps) {
  const { updateServiceGarment } = useServiceGarmentsStore();
  const { toast } = useToast();

  const [unitPrice, setUnitPrice] = useState<number | ''>(entry.unitPrice);
  const [isActive, setIsActive] = useState(entry.isActive);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSave = async () => {
    if (!unitPrice || unitPrice <= 0) {
      toast({
        title: 'Precio inválido',
        description: 'Ingresa un precio mayor a 0',
        variant: 'destructive',
      });
      return;
    }
    setIsSubmitting(true);
    try {
      await updateServiceGarment(entry.id, { unitPrice, isActive });
      toast({ title: 'Precio actualizado' });
      onClose();
    } catch (err) {
      const message = err instanceof Error ? err.message : 'No se pudo actualizar el precio';
      toast({ title: 'Error', description: message, variant: 'destructive' });
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <>
      <div className="px-6 py-4 space-y-4">
        <div className="space-y-1">
          <Label className="text-xs text-zinc-500 font-medium">Precio unitario *</Label>
          <CurrencyInput
            value={unitPrice}
            onChange={setUnitPrice}
            autoFocus
          />
        </div>

        <div className="flex items-center gap-2 pt-1">
          <Checkbox
            id="price-isActive"
            checked={isActive}
            onCheckedChange={(v) => setIsActive(!!v)}
          />
          <Label
            htmlFor="price-isActive"
            className="text-xs text-zinc-500 font-medium cursor-pointer"
          >
            Precio activo
          </Label>
        </div>
      </div>

      <div className="px-6 py-4 flex gap-3 justify-end border-t border-zinc-100 bg-zinc-50">
        <Button variant="outline" onClick={onClose} disabled={isSubmitting}>
          Cancelar
        </Button>
        <Button
          onClick={handleSave}
          disabled={isSubmitting}
        >
          {isSubmitting && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
          Guardar
        </Button>
      </div>
    </>
  );
}

// --- Main section ---

const TH = 'text-[10px] font-semibold tracking-widest uppercase text-zinc-400';

export function PricesSection() {
  const { services } = useServicesStore();
  const { serviceGarments, isLoading, deleteServiceGarment } = useServiceGarmentsStore();
  const { toast } = useToast();

  const [filterServiceId, setFilterServiceId] = useState('all');
  const [createDialogOpen, setCreateDialogOpen] = useState(false);
  const [editDialogOpen, setEditDialogOpen] = useState(false);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [editingEntry, setEditingEntry] = useState<ServiceGarment | undefined>();

  const pieceServices = services.filter((s) => s.chargeType === CHARGE_TYPE.PorPieza && s.isActive);

  const filteredEntries =
    filterServiceId === 'all'
      ? serviceGarments
      : serviceGarments.filter((sg) => sg.serviceId === parseInt(filterServiceId, 10));

  // Group entries by service for display
  const groupedEntries = pieceServices
    .filter((s) => filterServiceId === 'all' || s.id === parseInt(filterServiceId, 10))
    .map((service) => ({
      service,
      entries: filteredEntries.filter((e) => e.serviceId === service.id),
    }))
    .filter((g) => g.entries.length > 0);

  const handleDelete = async () => {
    if (!editingEntry) return;
    try {
      await deleteServiceGarment(editingEntry.id);
      toast({ title: 'Precio eliminado' });
      setDeleteDialogOpen(false);
      setEditingEntry(undefined);
    } catch {
      toast({
        title: 'Error',
        description: 'No se pudo eliminar el precio',
        variant: 'destructive',
      });
    }
  };

  return (
    <div>
      <div className="flex items-center justify-between px-6 py-5 border-b border-zinc-100">
        <h2 className="text-sm font-semibold text-zinc-900 tracking-tight">
          Precios por Prenda
          <span className="ml-2 font-mono font-normal text-xs text-zinc-400">{serviceGarments.length}</span>
        </h2>
        <Button
          onClick={() => setCreateDialogOpen(true)}
          size="sm"
        >
          <Plus className="h-4 w-4 mr-1" />
          Nuevo Precio
        </Button>
      </div>

      {pieceServices.length > 1 && (
        <div className="px-6 py-3 border-b border-zinc-100">
          <Select value={filterServiceId} onValueChange={setFilterServiceId}>
            <SelectTrigger className="w-64 h-8 text-sm">
              <SelectValue placeholder="Filtrar por servicio" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">Todos los servicios</SelectItem>
              {pieceServices.map((s) => (
                <SelectItem key={s.id} value={s.id.toString()}>
                  {s.name}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
      )}

      {isLoading && serviceGarments.length === 0 ? (
        <div className="flex justify-center py-12">
          <Loader2 className="h-6 w-6 animate-spin text-zinc-400" />
        </div>
      ) : filteredEntries.length === 0 ? (
        <div className="py-16 flex flex-col items-center gap-3">
          <div className="w-10 h-10 rounded-full bg-zinc-50 border border-zinc-100 flex items-center justify-center">
            <Receipt className="h-4 w-4 text-zinc-300" />
          </div>
          <div className="text-center">
            <p className="text-sm font-medium text-zinc-400">
              {pieceServices.length === 0 ? 'Sin servicios por pieza' : 'Sin precios configurados'}
            </p>
            <p className="text-xs text-zinc-300 mt-0.5">
              {pieceServices.length === 0
                ? 'Crea primero un servicio de tipo "por pieza"'
                : 'Agrega precios para las prendas de cada servicio'}
            </p>
          </div>
        </div>
      ) : (
        <Table>
          <TableHeader>
            <TableRow className="bg-zinc-50 hover:bg-zinc-50">
              <TableHead className={`${TH} pl-10`}>Tipo de Prenda</TableHead>
              <TableHead className={`${TH} text-right`}>Precio Unitario</TableHead>
              <TableHead className={TH}>Estado</TableHead>
              <TableHead className="w-20" />
            </TableRow>
          </TableHeader>
          <TableBody>
            {groupedEntries.flatMap(({ service, entries }) => [
              <TableRow
                key={`group-${service.id}`}
                className="bg-zinc-50 hover:bg-zinc-50 border-b border-zinc-200"
              >
                <TableCell colSpan={4} className="py-2.5 px-6">
                  <p className="text-[10px] font-semibold tracking-widest uppercase text-zinc-500">
                    {service.name}
                  </p>
                </TableCell>
              </TableRow>,
              ...entries.map((entry) => (
                <TableRow
                  key={entry.id}
                  className="border-b border-zinc-100 hover:bg-zinc-50 transition-colors"
                >
                  <TableCell className="pl-10 text-sm font-medium text-zinc-900">
                    {entry.garmentType.name}
                  </TableCell>
                  <TableCell className="text-right font-mono tabular-nums text-sm font-medium text-zinc-900">
                    ${entry.unitPrice.toFixed(2)}
                  </TableCell>
                  <TableCell>
                    <span
                      className={`inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-[10px] font-semibold ${
                        entry.isActive
                          ? 'bg-emerald-50 text-emerald-700'
                          : 'bg-zinc-100 text-zinc-400'
                      }`}
                    >
                      <span className={`w-1.5 h-1.5 rounded-full shrink-0 ${entry.isActive ? 'bg-emerald-500' : 'bg-zinc-400'}`} />
                      {entry.isActive ? 'Activo' : 'Inactivo'}
                    </span>
                  </TableCell>
                  <TableCell className="text-right">
                    <div className="flex justify-end gap-1">
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => {
                          setEditingEntry(entry);
                          setEditDialogOpen(true);
                        }}
                      >
                        <Pencil className="h-4 w-4 text-zinc-400" />
                      </Button>
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => {
                          setEditingEntry(entry);
                          setDeleteDialogOpen(true);
                        }}
                      >
                        <Trash2 className="h-4 w-4 text-rose-500" />
                      </Button>
                    </div>
                  </TableCell>
                </TableRow>
              )),
            ])}
          </TableBody>
        </Table>
      )}

      <Dialog open={createDialogOpen} onOpenChange={setCreateDialogOpen}>
        <DialogContent className="max-w-sm p-0 gap-0 overflow-hidden">
          <DialogHeader className="px-6 pt-5 pb-4 border-b border-zinc-100">
            <DialogTitle className="text-sm font-semibold text-zinc-900 tracking-tight">
              Nuevo Precio de Prenda
            </DialogTitle>
          </DialogHeader>
          <CreatePriceContent onClose={() => setCreateDialogOpen(false)} />
        </DialogContent>
      </Dialog>

      <Dialog open={editDialogOpen} onOpenChange={setEditDialogOpen}>
        <DialogContent className="max-w-sm p-0 gap-0 overflow-hidden">
          <DialogHeader className="px-6 pt-5 pb-4 border-b border-zinc-100">
            <DialogTitle className="text-sm font-semibold text-zinc-900 tracking-tight">
              Editar Precio
            </DialogTitle>
          </DialogHeader>
          {editingEntry && (
            <EditPriceContent
              entry={editingEntry}
              onClose={() => setEditDialogOpen(false)}
            />
          )}
        </DialogContent>
      </Dialog>

      <AlertDialog open={deleteDialogOpen} onOpenChange={setDeleteDialogOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>¿Eliminar precio?</AlertDialogTitle>
            <AlertDialogDescription>
              Esta acción no se puede deshacer. Se eliminará el precio de "
              {editingEntry?.garmentType.name}".
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
