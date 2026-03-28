import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Plus, DollarSign, Eye, Package } from 'lucide-react';
import { format } from 'date-fns';
import { es } from 'date-fns/locale';

import { Button } from '@/shared/components/ui/button';
import { useOrdersStore } from '@/features/orders/stores/ordersStore';
import { CashClosingModal } from '@/features/orders/components/cashClosing/CashClosingModal';
import { OrderSearchSheet } from '@/features/orders/components/orderSearch/OrderSearchSheet';

const TH = 'text-[10px] font-semibold tracking-widest uppercase text-zinc-400';

export function OrdersList() {
  const navigate = useNavigate();
  const { orders, fetchOrders, isLoading } = useOrdersStore();
  const [cashClosingOpen, setCashClosingOpen] = useState(false);
  const [searchSheetOpen, setSearchSheetOpen] = useState(false);
  const [selectedOrderId, setSelectedOrderId] = useState<number | undefined>(undefined);

  const handleViewOrder = (orderId: number) => {
    setSelectedOrderId(orderId);
    setSearchSheetOpen(true);
  };

  useEffect(() => {
    fetchOrders();
  }, [fetchOrders]);

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
            onClick={() => setCashClosingOpen(true)}
            className="h-8 text-xs"
          >
            <DollarSign className="h-3.5 w-3.5 mr-1.5" />
            Corte de Caja
          </Button>
          <Button
            size="sm"
            onClick={() => navigate('/orders/new')}
            className="bg-zinc-900 hover:bg-zinc-800 text-white h-8 text-xs"
          >
            <Plus className="h-3.5 w-3.5 mr-1.5" />
            Nueva Venta
          </Button>
        </div>
      </div>

      {isLoading ? (
        <div className="flex justify-center py-12">
          <div className="animate-spin rounded-full h-6 w-6 border-b-2 border-zinc-900" />
        </div>
      ) : orders.length === 0 ? (
        <div className="py-16 flex flex-col items-center gap-3">
          <div className="w-10 h-10 rounded-full bg-zinc-50 border border-zinc-100 flex items-center justify-center">
            <Package className="h-4 w-4 text-zinc-300" />
          </div>
          <div className="text-center">
            <p className="text-sm font-medium text-zinc-400">Sin órdenes</p>
            <p className="text-xs text-zinc-300 mt-0.5">Crea la primera venta para comenzar</p>
          </div>
          <Button
            size="sm"
            onClick={() => navigate('/orders/new')}
            className="bg-zinc-900 hover:bg-zinc-800 text-white mt-2"
          >
            <Plus className="h-3.5 w-3.5 mr-1.5" />
            Nueva Venta
          </Button>
        </div>
      ) : (
        <>
          {/* Table headers */}
          <div className="hidden md:grid grid-cols-[80px_2fr_1fr_80px_1fr_48px] gap-4 px-6 py-2 border-b border-zinc-100 bg-zinc-50">
            <p className={TH}>Folio</p>
            <p className={TH}>Cliente</p>
            <p className={TH}>Entrega</p>
            <p className={TH}>Items</p>
            <p className={TH}>Total</p>
            <p className={TH} />
          </div>

          {/* Desktop rows */}
          <div className="hidden md:block">
            {orders.map((order) => (
              <div
                key={order.id}
                className="grid grid-cols-[80px_2fr_1fr_80px_1fr_48px] gap-4 items-center px-6 py-3 border-b border-zinc-100 hover:bg-zinc-50 transition-colors"
              >
                <span className="font-mono font-bold text-sm tracking-tight text-zinc-900">
                  #{order.id}
                </span>

                <p className="text-sm font-medium text-zinc-800 truncate">
                  {order.client?.name ?? '—'}
                </p>

                <p className="text-xs text-zinc-500 capitalize">
                  {format(new Date(order.promisedDate), "d 'de' MMM", { locale: es })}
                </p>

                <span className="text-xs text-zinc-500 font-mono">
                  {order.items.length}
                </span>

                <span className="font-mono font-semibold tabular-nums text-sm text-zinc-900">
                  ${order.total.toFixed(2)}
                </span>

                <div className="flex justify-end">
                  <Button
                    variant="ghost"
                    size="sm"
                    className="h-7 w-7 p-0"
                    onClick={() => handleViewOrder(order.id)}
                  >
                    <Eye className="h-3.5 w-3.5 text-zinc-400" />
                  </Button>
                </div>
              </div>
            ))}
          </div>

          {/* Mobile rows */}
          <div className="md:hidden">
            {orders.map((order) => (
              <div
                key={order.id}
                className="px-6 py-4 border-b border-zinc-100"
              >
                <div className="flex items-start justify-between gap-3 mb-1.5">
                  <span className="font-mono font-bold text-sm tracking-tight text-zinc-900">
                    #{order.id}
                  </span>
                  <span className="font-mono font-semibold tabular-nums text-sm text-zinc-900">
                    ${order.total.toFixed(2)}
                  </span>
                </div>
                <p className="text-sm font-medium text-zinc-800">{order.client?.name ?? '—'}</p>
                <p className="text-xs text-zinc-400 capitalize mt-0.5">
                  Entrega: {format(new Date(order.promisedDate), "d 'de' MMM 'de' yyyy", { locale: es })}
                </p>
                <div className="flex gap-2 mt-3 pt-3 border-t border-zinc-100">
                  <Button
                    size="sm"
                    variant="outline"
                    className="h-7 text-xs"
                    onClick={() => handleViewOrder(order.id)}
                  >
                    <Eye className="h-3 w-3 mr-1" />
                    Ver detalle
                  </Button>
                </div>
              </div>
            ))}
          </div>

          {/* Footer counter */}
          <div className="px-6 py-3 bg-zinc-50 border-t border-zinc-100">
            <p className="text-[10px] font-semibold tracking-widest uppercase text-zinc-400">
              {orders.length} orden{orders.length !== 1 ? 'es' : ''}
            </p>
          </div>
        </>
      )}

      <CashClosingModal open={cashClosingOpen} onOpenChange={setCashClosingOpen} />
      <OrderSearchSheet
        open={searchSheetOpen}
        onOpenChange={setSearchSheetOpen}
        initialOrderId={selectedOrderId}
      />
    </div>
  );
}
