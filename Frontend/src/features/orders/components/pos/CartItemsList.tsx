import { CartItem } from './CartItem';
import type { CartItem as CartItemType } from '@/features/orders/stores/cartStore';

interface CartItemsListProps {
  items: CartItemType[];
  onUpdateQuantity: (id: string, quantity: number, weightKilos: number) => void;
  onRemove: (id: string) => void;
}

export function CartItemsList({ items, onUpdateQuantity, onRemove }: CartItemsListProps) {
  if (items.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center py-12 text-center">
        <p className="text-muted-foreground text-sm">
          No hay servicios agregados
        </p>
        <p className="text-muted-foreground text-xs mt-1">
          Selecciona servicios del catálogo para comenzar
        </p>
      </div>
    );
  }

  return (
    <div className="space-y-3 pr-1">
      {items.map((item) => (
        <CartItem
          key={item.id}
          item={item}
          onUpdateQuantity={onUpdateQuantity}
          onRemove={onRemove}
        />
      ))}
    </div>
  );
}
