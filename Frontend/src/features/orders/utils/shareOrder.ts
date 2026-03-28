import { format } from 'date-fns';
import { es } from 'date-fns/locale';
import type { Order } from '@/features/orders/types/order';

export function formatOrderWhatsAppMessage(order: Order): string {
  const lines: string[] = [];

  lines.push(`*Orden de Lavandería — ${order.folioOrden}*`);
  lines.push('');
  lines.push(`Cliente: ${order.client?.name ?? `#${order.clientId}`}`);
  lines.push(
    `Entrega: ${format(new Date(order.promisedDate), "d 'de' MMMM 'de' yyyy", { locale: es })}`
  );

  lines.push('');
  lines.push('*Detalle:*');
  for (const item of order.items) {
    const name = item.service?.name ?? 'Servicio';
    const garment = item.garmentType?.name ? ` — ${item.garmentType.name}` : '';
    const qty = item.quantity > 0 ? `${item.quantity} pz` : `${item.weightKilos} kg`;
    lines.push(`• ${name}${garment} (${qty}) — $${item.total.toFixed(2)}`);
  }

  lines.push('');
  if (order.totalDiscount > 0) {
    lines.push(`Subtotal: $${order.subtotal.toFixed(2)}`);
    lines.push(`Descuento: -$${order.totalDiscount.toFixed(2)}`);
  }
  lines.push(`*Total: $${order.total.toFixed(2)}*`);

  if (order.paymentStatus === 'paid') {
    lines.push('Pagado');
  } else if (order.balance > 0) {
    lines.push(`Saldo pendiente: $${order.balance.toFixed(2)}`);
  }

  if (order.notes) {
    lines.push('');
    lines.push(`Notas: ${order.notes}`);
  }

  return lines.join('\n');
}

export function openWhatsAppShare(phone: string | undefined, message: string): void {
  const cleanPhone = phone?.replace(/\D/g, '') ?? '';
  const encoded = encodeURIComponent(message);
  const url = cleanPhone
    ? `https://wa.me/${cleanPhone}?text=${encoded}`
    : `https://wa.me/?text=${encoded}`;
  window.open(url, '_blank', 'noopener,noreferrer');
}
