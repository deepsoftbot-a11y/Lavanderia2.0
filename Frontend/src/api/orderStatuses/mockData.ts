import type { OrderStatus } from '@/features/orders/types/order';

export const mockOrderStatuses: OrderStatus[] = [
  { id: 1, name: 'Recibido', color: '#4CAF50' },
  { id: 2, name: 'Listo', color: '#2196F3' },
  { id: 3, name: 'Entregado', color: '#9E9E9E' },
  { id: 4, name: 'Cancelado', color: '#FF0000' },
];
