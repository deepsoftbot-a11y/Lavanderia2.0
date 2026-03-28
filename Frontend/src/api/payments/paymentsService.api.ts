import api from '@/api/axiosConfig';
import type { Payment, CreatePaymentInput } from '@/features/orders/types/payment';

function getUserId(): number {
  const userJson = localStorage.getItem('auth_user');
  return userJson ? Number.parseInt(JSON.parse(userJson).id, 10) : 0;
}

export async function createPayment(input: Omit<CreatePaymentInput, 'receivedBy'>): Promise<Payment> {
  try {
    const payload: CreatePaymentInput = {
      ...input,
      receivedBy: getUserId(),
    };
    const response = await api.post<Payment>('/payments', payload);
    return response.data;
  } catch (error) {
    console.error('Create payment API error:', error);
    throw new Error('Error al registrar el pago');
  }
}

export async function getPaymentsByOrderId(orderId: number): Promise<Payment[]> {
  try {
    const response = await api.get<Payment[]>(`/orders/${orderId}/payments`);
    return response.data;
  } catch (error) {
    console.error('Get payments by order API error:', error);
    throw new Error('Error al obtener los pagos de la orden');
  }
}
