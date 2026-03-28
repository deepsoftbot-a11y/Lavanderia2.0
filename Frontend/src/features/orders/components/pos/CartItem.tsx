import { useCallback } from 'react';
import { Minus, Plus, Trash2 } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { Card, CardContent } from '@/shared/components/ui/card';
import { NumericInput } from '@/shared/components/common/NumericInput';
import type { CartItem as CartItemType } from '@/features/orders/stores/cartStore';

interface CartItemProps {
  item: CartItemType;
  onUpdateQuantity: (id: string, quantity: number, weightKilos: number) => void;
  onRemove: (id: string) => void;
}

export function CartItem({ item, onUpdateQuantity, onRemove }: CartItemProps) {
  const isPieceType = item.quantity > 0;

  const handleIncrement = useCallback(() => {
    if (isPieceType) {
      onUpdateQuantity(item.id, item.quantity + 1, 0);
    } else {
      onUpdateQuantity(item.id, 0, item.weightKilos + 0.5);
    }
  }, [isPieceType, item.id, item.quantity, item.weightKilos, onUpdateQuantity]);

  const handleDecrement = useCallback(() => {
    if (isPieceType) {
      if (item.quantity > 1) onUpdateQuantity(item.id, item.quantity - 1, 0);
    } else {
      if (item.weightKilos > 0.5) onUpdateQuantity(item.id, 0, item.weightKilos - 0.5);
    }
  }, [isPieceType, item.id, item.quantity, item.weightKilos, onUpdateQuantity]);

  const handleRemove = useCallback(() => onRemove(item.id), [item.id, onRemove]);

  return (
    <Card>
      <CardContent className="p-3">
        <div className="space-y-3">
          {/* Header: Service + Garment */}
          <div className="flex items-start justify-between">
            <div className="flex-1">
              <h4 className="font-medium text-sm">
                {item.serviceName}
                {/* Solo mostrar nombre de prenda si existe (servicios por pieza) */}
                {item.serviceGarmentId && ` - ${item.garmentName}`}
              </h4>
              {item.notes && (
                <p className="text-xs text-muted-foreground italic mt-1">{item.notes}</p>
              )}
            </div>
            <Button
              variant="ghost"
              size="icon"
              onClick={handleRemove}
              className="h-8 w-8 text-destructive hover:text-destructive hover:bg-destructive/10"
            >
              <Trash2 className="h-4 w-4" />
            </Button>
          </div>

          {/* Quantity Controls */}
          <div className="flex items-center justify-between">
            <span className="text-sm text-muted-foreground">Cantidad:</span>
            <div className="flex items-center gap-2">
              <Button
                variant="outline"
                size="icon"
                onClick={handleDecrement}
                className="h-8 w-8"
                disabled={isPieceType ? item.quantity <= 1 : item.weightKilos <= 0.5}
              >
                <Minus className="h-4 w-4" />
              </Button>

              <NumericInput
                value={isPieceType ? item.quantity : item.weightKilos}
                onChange={(val) => {
                  if (isPieceType) onUpdateQuantity(item.id, val, 0);
                  else onUpdateQuantity(item.id, 0, val);
                }}
                min={isPieceType ? 1 : 0.5}
                step={isPieceType ? 1 : 0.5}
                integer={isPieceType}
                className="h-8 w-20 text-center"
              />

              <Button
                variant="outline"
                size="icon"
                onClick={handleIncrement}
                className="h-8 w-8"
              >
                <Plus className="h-4 w-4" />
              </Button>

              <span className="text-sm text-muted-foreground min-w-[40px]">
                {isPieceType ? 'pzas' : 'kg'}
              </span>
            </div>
          </div>

          {/* Price Breakdown */}
          <div className="space-y-1 pt-2 border-t border-zinc-100">
            <div className="flex justify-between text-sm">
              <span className="text-zinc-500">Precio unitario:</span>
              <span className="font-mono tabular-nums text-zinc-700">
                ${item.unitPrice.toFixed(2)}
              </span>
            </div>

            <div className="flex justify-between text-sm">
              <span className="text-zinc-500">Subtotal:</span>
              <span className="font-mono tabular-nums text-zinc-700">
                ${item.subtotal.toFixed(2)}
              </span>
            </div>

            {item.discountAmount > 0 && (
              <div className="flex justify-between text-sm text-emerald-600">
                <span>Descuento:</span>
                <span className="font-mono tabular-nums">
                  -${item.discountAmount.toFixed(2)}
                </span>
              </div>
            )}

            <div className="flex justify-between text-base font-semibold pt-1">
              <span className="text-xs font-semibold text-zinc-900">Total:</span>
              <span className="font-mono font-bold tabular-nums text-zinc-900">
                ${item.total.toFixed(2)}
              </span>
            </div>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
