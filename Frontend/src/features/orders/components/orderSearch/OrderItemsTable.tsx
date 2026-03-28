import {
  Table,
  TableBody,
  TableCell,
  TableFooter,
  TableHead,
  TableHeader,
  TableRow,
} from '@/shared/components/ui/table';

import type { Order } from '@/features/orders/types/order';

interface OrderItemsTableProps {
  order: Order;
}

export function OrderItemsTable({ order }: OrderItemsTableProps) {
  return (
    <div className="border border-zinc-100 rounded-md overflow-hidden">
      <Table>
        <TableHeader>
          <TableRow className="bg-zinc-50">
            <TableHead className="text-[10px] font-semibold tracking-widest uppercase text-zinc-400 h-8">Servicio</TableHead>
            <TableHead className="text-[10px] font-semibold tracking-widest uppercase text-zinc-400 h-8 text-center">Cant/Peso</TableHead>
            <TableHead className="text-[10px] font-semibold tracking-widest uppercase text-zinc-400 h-8 text-right">P. Unit.</TableHead>
            <TableHead className="text-[10px] font-semibold tracking-widest uppercase text-zinc-400 h-8 text-right">Desc.</TableHead>
            <TableHead className="text-[10px] font-semibold tracking-widest uppercase text-zinc-400 h-8 text-right">Total</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {order.items.map((item) => {
            const serviceName = item.service?.name ?? `Servicio #${item.serviceId}`;
            const garmentName = item.garmentType?.name;
            const quantityDisplay = item.quantity > 0
              ? `${item.quantity} pz`
              : `${item.weightKilos} kg`;

            return (
              <TableRow key={item.id} className="border-zinc-100">
                <TableCell className="text-xs py-2">
                  <span className="font-medium text-zinc-800">{serviceName}</span>
                  {garmentName && (
                    <span className="text-zinc-400 block text-[10px]">{garmentName}</span>
                  )}
                </TableCell>
                <TableCell className="text-xs py-2 text-center text-zinc-600">{quantityDisplay}</TableCell>
                <TableCell className="text-xs py-2 text-right font-mono tabular-nums text-zinc-600">
                  ${item.unitPrice.toFixed(2)}
                </TableCell>
                <TableCell className="text-xs py-2 text-right font-mono tabular-nums text-zinc-600">
                  {item.discountAmount > 0 ? `-$${item.discountAmount.toFixed(2)}` : '—'}
                </TableCell>
                <TableCell className="text-xs py-2 text-right font-mono font-medium tabular-nums text-zinc-800">
                  ${item.total.toFixed(2)}
                </TableCell>
              </TableRow>
            );
          })}
        </TableBody>
        <TableFooter className="bg-zinc-50">
          <TableRow className="border-zinc-100">
            <TableCell colSpan={4} className="text-xs text-right text-zinc-500 font-medium">Subtotal</TableCell>
            <TableCell className="text-xs text-right font-mono font-medium tabular-nums text-zinc-700">
              ${order.subtotal.toFixed(2)}
            </TableCell>
          </TableRow>
          {order.totalDiscount > 0 && (
            <TableRow className="border-zinc-100">
              <TableCell colSpan={4} className="text-xs text-right font-medium text-rose-600">Descuento</TableCell>
              <TableCell className="text-xs text-right font-mono font-medium tabular-nums text-rose-600">
                -${order.totalDiscount.toFixed(2)}
              </TableCell>
            </TableRow>
          )}
          <TableRow className="border-zinc-100">
            <TableCell colSpan={4} className="text-xs text-right font-bold text-zinc-800">Total</TableCell>
            <TableCell className="text-xs text-right font-mono font-bold tabular-nums text-zinc-900">
              ${order.total.toFixed(2)}
            </TableCell>
          </TableRow>
        </TableFooter>
      </Table>
    </div>
  );
}
