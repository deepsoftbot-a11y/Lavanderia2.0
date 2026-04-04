import { ChevronLeft, ChevronRight, Package } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { TABLE_HEADER_CLASS as TH } from '@/shared/utils/constants';
import { OrdersTableRow } from './OrdersTableRow';
import type { OrderSummary } from '@/features/orders/types/order';

interface OrdersTableProps {
  orders: OrderSummary[];
  isLoading: boolean;
  error: string | null;
  totalCount: number;
  page: number;
  pageSize: number;
  hasActiveFilters: boolean;
  onPageChange: (page: number) => void;
  onViewOrder: (orderId: number) => void;
  onClearFilters: () => void;
  onRetry: () => void;
}

function SkeletonRow() {
  return (
    <div className="hidden md:grid grid-cols-[80px_2fr_1fr_120px_48px] gap-4 items-center px-6 py-3 border-b border-zinc-100">
      {[80, 160, 80, 70, 28].map((w, i) => (
        <div key={i} className="h-4 bg-zinc-100 rounded animate-pulse" style={{ width: w }} />
      ))}
    </div>
  );
}

export function OrdersTable({
  orders,
  isLoading,
  error,
  totalCount,
  page,
  pageSize,
  hasActiveFilters,
  onPageChange,
  onViewOrder,
  onClearFilters,
  onRetry,
}: OrdersTableProps) {
  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));

  if (error) {
    return (
      <div className="py-16 flex flex-col items-center gap-3">
        <p className="text-sm text-zinc-500">{error}</p>
        <Button size="sm" variant="outline" onClick={onRetry}>
          Reintentar
        </Button>
      </div>
    );
  }

  if (isLoading) {
    return (
      <>
        <div className="hidden md:grid grid-cols-[80px_2fr_1fr_120px_48px] gap-4 px-6 py-2 border-b border-zinc-100 bg-zinc-50">
          {['Folio', 'Cliente', 'Total', 'Pago', ''].map((h) => (
            <p key={h} className={TH}>{h}</p>
          ))}
        </div>
        {Array.from({ length: 5 }).map((_, i) => <SkeletonRow key={i} />)}
      </>
    );
  }

  if (orders.length === 0) {
    return (
      <div className="py-16 flex flex-col items-center gap-3">
        <div className="w-10 h-10 rounded-full bg-zinc-50 border border-zinc-100 flex items-center justify-center">
          <Package className="h-4 w-4 text-zinc-300" />
        </div>
        <div className="text-center">
          <p className="text-sm font-medium text-zinc-400">
            {hasActiveFilters ? 'No hay órdenes con estos filtros' : 'Sin órdenes'}
          </p>
        </div>
        {hasActiveFilters && (
          <Button size="sm" variant="outline" onClick={onClearFilters}>
            Limpiar filtros
          </Button>
        )}
      </div>
    );
  }

  return (
    <>
      <div className="hidden md:grid grid-cols-[80px_2fr_1fr_120px_48px] gap-4 px-6 py-2 border-b border-zinc-100 bg-zinc-50">
        {['Folio', 'Cliente', 'Entrega', 'Items', 'Total', 'Pago', ''].map((h) => (
          <p key={h} className={TH}>{h}</p>
        ))}
      </div>

      {orders.map((order) => (
        <OrdersTableRow key={order.id} order={order} onView={onViewOrder} />
      ))}

      <div className="px-6 py-3 bg-zinc-50 border-t border-zinc-100 flex items-center justify-between">
        <p className="text-[10px] font-semibold tracking-widest uppercase text-zinc-400">
          {totalCount} orden{totalCount !== 1 ? 'es' : ''}
        </p>
        <div className="flex items-center gap-2">
          <Button
            variant="outline"
            size="sm"
            className="h-7 w-7 p-0"
            disabled={page <= 1}
            onClick={() => onPageChange(page - 1)}
          >
            <ChevronLeft className="h-3.5 w-3.5" />
          </Button>
          <span className="text-xs text-zinc-500 tabular-nums">
            Página {page} de {totalPages}
          </span>
          <Button
            variant="outline"
            size="sm"
            className="h-7 w-7 p-0"
            disabled={page >= totalPages}
            onClick={() => onPageChange(page + 1)}
          >
            <ChevronRight className="h-3.5 w-3.5" />
          </Button>
        </div>
      </div>
    </>
  );
}
