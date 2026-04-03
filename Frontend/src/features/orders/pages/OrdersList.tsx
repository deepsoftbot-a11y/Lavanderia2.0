import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Plus, DollarSign } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { useOrdersStore } from '@/features/orders/stores/ordersStore';
import { OrderSearchSheet } from '@/features/orders/components/orderSearch/OrderSearchSheet';
import { OrdersFiltersBar } from '@/features/orders/components/ordersHistory/OrdersFiltersBar';
import { OrdersTable } from '@/features/orders/components/ordersHistory/OrdersTable';
import { OrdersExportButton } from '@/features/orders/components/ordersHistory/OrdersExportButton';
import { useUIStore } from '@/shared/stores/uiStore';
import type { OrderHistoryFilters } from '@/features/orders/types/order';

export function OrdersList() {
  const navigate = useNavigate();
  const openCashClosing = useUIStore((state) => state.openCashClosing);
  const { orders, isLoading, error, pagination, activeFilters, fetchOrders, clearFilters } =
    useOrdersStore();

  const [searchSheetOpen, setSearchSheetOpen] = useState(false);
  const [selectedOrderId, setSelectedOrderId] = useState<number | undefined>(undefined);

  useEffect(() => {
    fetchOrders();
  }, [fetchOrders]);

  function handleFiltersChange(filters: OrderHistoryFilters) {
    fetchOrders(filters, 1);
  }

  function handlePageChange(page: number) {
    fetchOrders(activeFilters, page);
  }

  function handleViewOrder(orderId: number) {
    setSelectedOrderId(orderId);
    setSearchSheetOpen(true);
  }

  function handleRetry() {
    fetchOrders(activeFilters, pagination.page);
  }

  const hasActiveFilters =
    !!activeFilters.startDate ||
    !!activeFilters.endDate ||
    (activeFilters.statusIds?.length ?? 0) > 0 ||
    (activeFilters.paymentStatuses?.length ?? 0) > 0;

  return (
    <div className="bg-white border border-zinc-200 rounded-lg overflow-hidden">
      {/* Header */}
      <div className="flex items-center justify-between px-6 py-5 border-b border-zinc-100">
        <div>
          <h1 className="text-xl font-semibold text-zinc-900 tracking-tight">Órdenes</h1>
          <p className="text-xs text-zinc-400 mt-0.5">Gestión de órdenes de servicio</p>
        </div>
        <div className="flex gap-2">
          <Button
            variant="outline"
            size="sm"
            onClick={() => openCashClosing()}
            className="h-8 text-xs"
          >
            <DollarSign className="h-3.5 w-3.5 mr-1.5" />
            Corte de Caja
          </Button>
          <OrdersExportButton activeFilters={activeFilters} />
          <Button
            size="sm"
            onClick={() => navigate('/orders/new')}
            className="h-8 text-xs"
          >
            <Plus className="h-3.5 w-3.5 mr-1.5" />
            Nueva Venta
          </Button>
        </div>
      </div>

      {/* Filters */}
      <OrdersFiltersBar onFiltersChange={handleFiltersChange} />

      {/* Table */}
      <OrdersTable
        orders={orders}
        isLoading={isLoading}
        error={error}
        totalCount={pagination.totalCount}
        page={pagination.page}
        pageSize={pagination.pageSize}
        hasActiveFilters={hasActiveFilters}
        onPageChange={handlePageChange}
        onViewOrder={handleViewOrder}
        onClearFilters={clearFilters}
        onRetry={handleRetry}
      />

      <OrderSearchSheet
        open={searchSheetOpen}
        onOpenChange={setSearchSheetOpen}
        initialOrderId={selectedOrderId}
      />
    </div>
  );
}
