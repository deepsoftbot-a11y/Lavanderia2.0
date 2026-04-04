import api from '@/api/axiosConfig';
import type {
  Order,
  OrderSummary,
  PagedResult,
  OrderHistoryFilters,
  CreateOrderInput,
  UpdateOrderInput,
  OrderSearchFilters,
  UpdateOrderStatusInput,
} from '@/features/orders/types/order';
import { mockOrderStatuses } from '@/api/orderStatuses';

function withOrderStatus<T extends { orderStatusId: number }>(order: T): T {
  return {
    ...order,
    orderStatus: mockOrderStatuses.find((s) => s.id === order.orderStatusId),
  };
}

export async function getOrders(
  filters: OrderHistoryFilters = {},
  page = 1,
  pageSize = 20
): Promise<PagedResult<OrderSummary>> {
  try {
    const params: Record<string, unknown> = { ...filters, page, pageSize };
    const response = await api.get<PagedResult<OrderSummary>>('/orders', { params });
    return {
      ...response.data,
      data: response.data.data.map(withOrderStatus),
    };
  } catch (error) {
    console.error('Get orders API error:', error);
    throw new Error('Error al obtener órdenes desde el servidor');
  }
}

export async function getOrderById(id: number): Promise<Order> {
  try {
    const response = await api.get<Order>(`/orders/${id}`);
    return withOrderStatus(response.data);
  } catch (error) {
    console.error('Get order by ID API error:', error);
    throw new Error('Error al obtener la orden desde el servidor');
  }
}

export async function createOrder(input: CreateOrderInput): Promise<Order> {
  try {
    const response = await api.post<Order>('/ordenes/v2', input);
    console.log('[createOrder API] Respuesta recibida:', response.data);
    return withOrderStatus(response.data);
  } catch (error) {
    console.error('[createOrder API] Error completo:', error);
    if (error instanceof Error) {
      console.error('[createOrder API] Error message:', error.message);
      console.error('[createOrder API] Error stack:', error.stack);
    }
    throw new Error('Error al crear la orden en el servidor');
  }
}

export async function updateOrder(id: number, input: UpdateOrderInput): Promise<Order> {
  try {
    const response = await api.put<Order>(`/orders/${id}`, input);
    return withOrderStatus(response.data);
  } catch (error) {
    console.error('Update order API error:', error);
    throw new Error('Error al actualizar la orden en el servidor');
  }
}

export async function deleteOrder(id: number): Promise<void> {
  try {
    await api.delete(`/orders/${id}`);
  } catch (error) {
    console.error('Delete order API error:', error);
    throw new Error('Error al eliminar la orden del servidor');
  }
}

export async function updateOrderPaymentTotals(orderId: number): Promise<Order> {
  try {
    const response = await api.patch<Order>(`/orders/${orderId}/payment-totals`);
    return withOrderStatus(response.data);
  } catch (error) {
    console.error('Update order payment totals API error:', error);
    throw new Error('Error al actualizar los totales de pago');
  }
}

export async function searchOrders(filters: OrderSearchFilters): Promise<OrderSummary[]> {
  try {
    const response = await api.get<OrderSummary[]>('/orders/search', { params: filters });
    return response.data.map(withOrderStatus);
  } catch (error) {
    console.error('Search orders API error:', error);
    throw new Error('Error al buscar órdenes');
  }
}

export async function updateOrderStatus(orderId: number, input: UpdateOrderStatusInput): Promise<Order> {
  try {
    const response = await api.patch<Order>(`/orders/${orderId}/status`, input);
    return withOrderStatus(response.data);
  } catch (error) {
    console.error('Update order status API error:', error);
    throw new Error('Error al actualizar el estado de la orden');
  }
}

export async function exportOrders(
  format: 'xlsx' | 'pdf',
  filters: OrderHistoryFilters = {}
): Promise<void> {
  try {
    const params: Record<string, unknown> = { ...filters, format };
    const response = await api.get('/reportes/ordenes/export', {
      params,
      responseType: 'blob',
    });
    const ext = format === 'xlsx' ? 'xlsx' : 'pdf';
    const today = new Date().toISOString().slice(0, 10);
    const fileName = `ordenes-${today}.${ext}`;
    const url = URL.createObjectURL(response.data as Blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = fileName;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
  } catch (error) {
    console.error('Export orders API error:', error);
    throw new Error('Error al generar el reporte');
  }
}
