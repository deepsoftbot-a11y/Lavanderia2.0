import type { OrderStatus } from '@/features/orders/types/order';
import { mockOrderStatuses } from './mockData';

const delay = (ms: number) => new Promise((resolve) => setTimeout(resolve, ms));

const orderStatuses = [...mockOrderStatuses];

export async function getOrderStatuses(): Promise<OrderStatus[]> {
  await delay(200);
  return orderStatuses;
}

export async function getOrderStatusById(id: number): Promise<OrderStatus> {
  await delay(100);
  const status = orderStatuses.find((s) => s.id === id);
  if (!status) {
    throw new Error('Estado de orden no encontrado');
  }
  return status;
}
