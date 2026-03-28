import { useState, useEffect } from 'react';
import { Check, ChevronsUpDown } from 'lucide-react';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/shared/components/ui/dialog';
import { Button } from '@/shared/components/ui/button';
import { Label } from '@/shared/components/ui/label';
import { NumericInput } from '@/shared/components/common/NumericInput';
import { Textarea } from '@/shared/components/ui/textarea';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select';
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from '@/shared/components/ui/popover';
import {
  Command,
  CommandEmpty,
  CommandInput,
  CommandItem,
  CommandList,
} from '@/shared/components/ui/command';
import { cn } from '@/shared/utils/cn';
import { getServiceGarments } from '@/api/serviceGarments';
import { CHARGE_TYPE } from '@/features/services/types/service';
import type { Service } from '@/features/services/types/service';
import type { ServiceGarment } from '@/features/services/types/serviceGarment';
import type { Discount } from '@/features/services/types/discount';

interface AddServiceDialogProps {
  open: boolean;
  service: Service | null;
  discounts: Discount[];
  onClose: () => void;
  onAdd: (
    service: Service,
    garment: ServiceGarment | null,
    discount: Discount | null,
    quantity: number,
    weightKilos: number,
    notes: string,
    specificPrice: number | null
  ) => void;
}

export function AddServiceDialog({
  open,
  service,
  discounts,
  onClose,
  onAdd,
}: AddServiceDialogProps) {
  const [selectedGarmentId, setSelectedGarmentId] = useState<string>('');
  const [garmentPopoverOpen, setGarmentPopoverOpen] = useState(false);
  const [selectedDiscountId, setSelectedDiscountId] = useState<string>('0');
  const [quantity, setQuantity] = useState<number>(1);
  const [weightKilos, setWeightKilos] = useState<number>(1);
  const [notes, setNotes] = useState<string>('');
  const [availableGarments, setAvailableGarments] = useState<ServiceGarment[]>([]);

  const isPieceType = service?.chargeType === CHARGE_TYPE.PorPieza;

  useEffect(() => {
    const fetchGarments = async () => {
      if (service && open && service.chargeType === CHARGE_TYPE.PorPieza) {
        try {
          const garments = await getServiceGarments(service.id);
          setAvailableGarments(garments.filter((g) => g.isActive));
        } catch (error) {
          console.error('Error al cargar prendas del servicio:', error);
          setAvailableGarments([]);
        }
      } else {
        setAvailableGarments([]);
      }
    };

    fetchGarments();
  }, [service, open]);

  useEffect(() => {
    if (open && service) {
      setSelectedGarmentId('');
      setGarmentPopoverOpen(false);
      setSelectedDiscountId('0');
      setQuantity(1);
      setWeightKilos(1);
      setNotes('');
    }
  }, [open, service]);

  const selectedGarment = availableGarments.find(
    (g) => g.id === parseInt(selectedGarmentId)
  ) ?? null;

  const displayPrice = isPieceType
    ? selectedGarment?.unitPrice
    : service?.pricePerKg;

  const handleAdd = () => {
    if (!service) return;

    const discount =
      selectedDiscountId && selectedDiscountId !== '0'
        ? discounts.find((d) => d.id === parseInt(selectedDiscountId))
        : null;

    if (service.chargeType === CHARGE_TYPE.PorPeso) {
      onAdd(service, null, discount ?? null, 0, weightKilos, notes, displayPrice ?? null);
      onClose();
      return;
    }

    if (!selectedGarmentId || !selectedGarment) return;

    onAdd(
      service,
      selectedGarment,
      discount ?? null,
      quantity,
      0,
      notes,
      displayPrice ?? null
    );

    onClose();
  };

  const activeDiscounts = discounts.filter((d) => d.isActive);

  return (
    <Dialog open={open} onOpenChange={onClose}>
      <DialogContent className="max-w-md p-0 gap-0 overflow-hidden">
        <DialogHeader className="px-6 pt-5 pb-4 border-b border-zinc-100">
          <DialogTitle className="text-sm font-semibold text-zinc-900 tracking-tight">
            Agregar Servicio
          </DialogTitle>
          {service && (
            <p className="text-xs text-zinc-400 mt-0.5">
              {service.name}
              {!isPieceType && service.pricePerKg !== undefined && (
                <>
                  {' '}—{' '}
                  <span className="font-mono tabular-nums">${service.pricePerKg}</span>
                  {' '}por kilo
                </>
              )}
              {isPieceType && !selectedGarmentId && <> — selecciona el tipo de prenda</>}
              {isPieceType && selectedGarmentId && displayPrice !== undefined && (
                <>
                  {' '}—{' '}
                  <span className="font-mono tabular-nums">${displayPrice}</span>
                  {' '}por pieza
                </>
              )}
            </p>
          )}
        </DialogHeader>

        <div className="flex flex-col max-h-[82vh]">
          <div className="overflow-y-auto flex-1">
            {/* Prenda y cantidad */}
            <div className="px-6 py-4 border-b border-zinc-100">
              <p className="text-[10px] font-semibold tracking-widest uppercase text-zinc-400 mb-3">
                {isPieceType ? 'Prenda y cantidad' : 'Cantidad'}
              </p>
              <div className="space-y-3">
                {isPieceType && (
                  <div className="space-y-1">
                    <Label className="text-xs text-zinc-500 font-medium">
                      Tipo de prenda *
                    </Label>
                    <Popover open={garmentPopoverOpen} onOpenChange={setGarmentPopoverOpen}>
                      <PopoverTrigger asChild>
                        <button
                          type="button"
                          role="combobox"
                          aria-expanded={garmentPopoverOpen}
                          className={cn(
                            'flex h-9 w-full items-center justify-between rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-xs ring-offset-background',
                            'focus:outline-none focus:ring-1 focus:ring-ring',
                            'disabled:cursor-not-allowed disabled:opacity-50',
                            !selectedGarmentId && 'text-muted-foreground'
                          )}
                        >
                          <span className="truncate">
                            {selectedGarmentId
                              ? (() => {
                                  const g = availableGarments.find(
                                    (g) => g.id.toString() === selectedGarmentId
                                  );
                                  return g
                                    ? `${g.garmentType.name} — $${g.unitPrice}`
                                    : 'Seleccionar tipo de prenda';
                                })()
                              : 'Seleccionar tipo de prenda'}
                          </span>
                          <ChevronsUpDown className="ml-2 h-4 w-4 shrink-0 text-zinc-400" />
                        </button>
                      </PopoverTrigger>
                      <PopoverContent className="w-[--radix-popover-trigger-width] p-0" align="start">
                        <Command>
                          <CommandInput placeholder="Buscar prenda..." className="h-9" />
                          <CommandList>
                            <CommandEmpty className="py-6 text-center text-sm text-zinc-400">
                              No se encontraron prendas
                            </CommandEmpty>
                            {availableGarments.map((garment) => (
                              <CommandItem
                                key={garment.id}
                                value={garment.garmentType.name}
                                onSelect={() => {
                                  setSelectedGarmentId(garment.id.toString());
                                  setGarmentPopoverOpen(false);
                                }}
                                className="flex items-center justify-between"
                              >
                                <div className="flex items-center gap-2">
                                  <Check
                                    className={cn(
                                      'h-4 w-4 text-zinc-900',
                                      selectedGarmentId === garment.id.toString()
                                        ? 'opacity-100'
                                        : 'opacity-0'
                                    )}
                                  />
                                  <span>{garment.garmentType.name}</span>
                                </div>
                                <span className="font-mono tabular-nums text-xs text-zinc-400">
                                  ${garment.unitPrice}
                                </span>
                              </CommandItem>
                            ))}
                          </CommandList>
                        </Command>
                      </PopoverContent>
                    </Popover>
                  </div>
                )}

                {isPieceType ? (
                  <div className="space-y-1">
                    <Label htmlFor="quantity" className="text-xs text-zinc-500 font-medium">
                      Piezas *
                    </Label>
                    <NumericInput
                      id="quantity"
                      value={quantity}
                      onChange={setQuantity}
                      min={1}
                      step={1}
                      integer
                    />
                  </div>
                ) : (
                  <div className="space-y-1">
                    <Label htmlFor="weight" className="text-xs text-zinc-500 font-medium">
                      Peso en kilos *
                    </Label>
                    <NumericInput
                      id="weight"
                      value={weightKilos}
                      onChange={setWeightKilos}
                      min={0.5}
                      step={0.5}
                    />
                  </div>
                )}
              </div>
            </div>

            {/* Descuento y notas */}
            <div className="px-6 py-4">
              <p className="text-[10px] font-semibold tracking-widest uppercase text-zinc-400 mb-3">
                Opciones
              </p>
              <div className="space-y-3">
                <div className="space-y-1">
                  <Label htmlFor="discount" className="text-xs text-zinc-500 font-medium">
                    Descuento
                  </Label>
                  <Select value={selectedDiscountId} onValueChange={setSelectedDiscountId}>
                    <SelectTrigger id="discount">
                      <SelectValue placeholder="Sin descuento" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="0">Sin descuento</SelectItem>
                      {activeDiscounts.map((discount) => (
                        <SelectItem key={discount.id} value={discount.id.toString()}>
                          {discount.name}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>

                <div className="space-y-1">
                  <Label htmlFor="notes" className="text-xs text-zinc-500 font-medium">
                    Notas
                  </Label>
                  <Textarea
                    id="notes"
                    placeholder="Observaciones sobre este item..."
                    value={notes}
                    onChange={(e) => setNotes(e.target.value)}
                    rows={3}
                  />
                </div>
              </div>
            </div>
          </div>

          <div className="px-6 py-4 flex gap-3 justify-end border-t border-zinc-100 bg-zinc-50">
            <Button variant="outline" onClick={onClose}>
              Cancelar
            </Button>
            <Button
              onClick={handleAdd}
              disabled={isPieceType && !selectedGarmentId}
              className="bg-zinc-900 hover:bg-zinc-800 text-white"
            >
              Agregar al carrito
            </Button>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
}
