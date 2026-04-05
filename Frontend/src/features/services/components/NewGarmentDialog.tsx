import { useState } from 'react';
import { Loader2 } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { Input } from '@/shared/components/ui/input';
import { Label } from '@/shared/components/ui/label';
import { CurrencyInput } from '@/shared/components/ui/currency-input';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/shared/components/ui/dialog';
import { useGarmentTypesStore } from '@/features/services/stores/garmentTypesStore';
import { useToast } from '@/shared/hooks/use-toast';
import type { GarmentPriceInput } from '@/features/services/types/service';

interface NewGarmentDialogProps {
  open: boolean;
  onClose: () => void;
  onCreated: (entry: GarmentPriceInput) => void;
}

export function NewGarmentDialog({ open, onClose, onCreated }: NewGarmentDialogProps) {
  const { createGarmentType } = useGarmentTypesStore();
  const { toast } = useToast();
  const [name, setName] = useState('');
  const [price, setPrice] = useState<number | ''>('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  const canSave = name.trim().length > 0 && typeof price === 'number' && price > 0 && !isSubmitting;

  const handleClose = () => {
    setName('');
    setPrice('');
    onClose();
  };

  const handleSave = async () => {
    if (!canSave) return;
    setIsSubmitting(true);
    try {
      const newGarment = await createGarmentType({ name: name.trim(), description: '' });
      if (!newGarment) throw new Error('No se recibió respuesta del servidor');
      onCreated({ garmentTypeId: newGarment.id, unitPrice: price as number });
      toast({ title: `Prenda "${name.trim()}" creada` });
      handleClose();
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Error al crear la prenda';
      toast({ title: 'Error', description: message, variant: 'destructive' });
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Dialog open={open} onOpenChange={(o) => { if (!o) handleClose(); }}>
      <DialogContent className="max-w-sm p-0 gap-0 overflow-hidden">
        <DialogHeader className="px-6 pt-5 pb-4 border-b border-zinc-100">
          <DialogTitle className="text-sm font-semibold text-zinc-900 tracking-tight">
            Nueva prenda
          </DialogTitle>
        </DialogHeader>
        <div className="px-6 py-4 space-y-4">
          <div className="space-y-1">
            <Label className="text-xs text-zinc-500 font-medium">Nombre *</Label>
            <Input
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="Ej: Camisa, Edredón..."
              autoFocus
            />
          </div>
          <div className="space-y-1">
            <Label className="text-xs text-zinc-500 font-medium">Precio unitario *</Label>
            <CurrencyInput value={price} onChange={setPrice} />
          </div>
        </div>
        <div className="px-6 py-4 flex gap-3 justify-end border-t border-zinc-100 bg-zinc-50">
          <Button variant="outline" onClick={handleClose} disabled={isSubmitting}>
            Cancelar
          </Button>
          <Button onClick={handleSave} disabled={!canSave}>
            {isSubmitting && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
            Guardar
          </Button>
        </div>
      </DialogContent>
    </Dialog>
  );
}
