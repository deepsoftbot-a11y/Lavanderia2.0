import { OrderSearchResultCard } from './OrderSearchResultCard';
import type { OrderSummary } from '@/features/orders/types/order';

interface OrderSearchResultsListProps {
  results: OrderSummary[];
  isSearching: boolean;
  searchError: string | null;
  hasSearched: boolean;
  onSelectOrder: (order: OrderSummary) => void;
}

export function OrderSearchResultsList({
  results,
  isSearching,
  searchError,
  hasSearched,
  onSelectOrder,
}: OrderSearchResultsListProps) {
  if (searchError) {
    return (
      <div className="px-6 py-10 text-center">
        <p className="text-xs text-rose-600">{searchError}</p>
      </div>
    );
  }

  if (isSearching) {
    return (
      <div className="flex flex-col items-center justify-center py-12 gap-3">
        <div className="h-5 w-5 animate-spin rounded-full border-2 border-zinc-200 border-t-zinc-600" />
        <p className="text-xs text-zinc-400">Buscando órdenes...</p>
      </div>
    );
  }

  if (!hasSearched) {
    return (
      <div className="px-6 py-10 text-center">
        <p className="text-xs font-medium text-zinc-500">Escribe al menos 2 caracteres para buscar</p>
        <p className="text-[10px] text-zinc-400 mt-1">Folio, nombre de cliente o teléfono</p>
      </div>
    );
  }

  if (results.length === 0) {
    return (
      <div className="px-6 py-10 text-center">
        <p className="text-xs font-medium text-zinc-500">Sin resultados</p>
        <p className="text-[10px] text-zinc-400 mt-1">Intenta con otro término de búsqueda</p>
      </div>
    );
  }

  return (
    <div>
      <p className="text-[10px] font-semibold tracking-widest uppercase text-zinc-400 px-6 py-2 border-b border-zinc-100">
        {results.length} resultado{results.length !== 1 ? 's' : ''}
      </p>
      {results.map((order) => (
        <OrderSearchResultCard
          key={order.id}
          order={order}
          onClick={onSelectOrder}
        />
      ))}
    </div>
  );
}
