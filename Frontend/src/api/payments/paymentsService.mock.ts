import type {
  Payment,
  CreatePaymentInput,
  UpdatePaymentInput,
  PaymentFilters,
} from '@/features/orders/types/payment';
import { PAYMENT_STATUS } from '@/features/orders/types/payment';
import { mockPayments } from './mockData';
import { getPaymentMethodById } from '../paymentMethods';
import { updateOrderPaymentTotals, getOrderById } from '../orders';

const delay = (ms: number) => new Promise((resolve) => setTimeout(resolve, ms));

let payments = [...mockPayments];
let nextId = Math.max(...payments.map((p) => p.id), 0) + 1;

// Helper para calcular estado de pago de una orden
function calculatePaymentStatus(
  total: number,
  amountPaid: number
): 'pending' | 'partial' | 'paid' {
  const balance = total - amountPaid;

  if (balance <= 0) {
    return PAYMENT_STATUS.PAID;
  } else if (amountPaid > 0) {
    return PAYMENT_STATUS.PARTIAL;
  } else {
    return PAYMENT_STATUS.PENDING;
  }
}

// Helper para popular relaciones de un pago
async function populatePayment(payment: Payment): Promise<Payment> {
  const paymentMethod = await getPaymentMethodById(payment.paymentMethodId).catch(() => undefined);

  return {
    ...payment,
    paymentMethod,
  };
}

// Obtener todos los pagos con filtros
export async function getPayments(
  filters?: PaymentFilters
): Promise<Payment[]> {
  await delay(300);

  let filtered = [...payments];

  // Filtro por orden
  if (filters?.orderId) {
    filtered = filtered.filter((p) => p.orderId === filters.orderId);
  }

  // Filtro por método de pago
  if (filters?.paymentMethodId) {
    filtered = filtered.filter(
      (p) => p.paymentMethodId === filters.paymentMethodId
    );
  }

  // Filtro por usuario que recibió
  if (filters?.receivedBy) {
    filtered = filtered.filter((p) => p.receivedBy === filters.receivedBy);
  }

  // Filtro por rango de fechas
  if (filters?.startDate) {
    filtered = filtered.filter((p) => p.paidAt >= filters.startDate!);
  }
  if (filters?.endDate) {
    filtered = filtered.filter((p) => p.paidAt <= filters.endDate!);
  }

  // Popular relaciones
  const populated = await Promise.all(
    filtered.map((payment) => populatePayment(payment))
  );

  return populated;
}

// Obtener pago por ID
export async function getPaymentById(id: number): Promise<Payment> {
  await delay(200);

  const payment = payments.find((p) => p.id === id);
  if (!payment) {
    throw new Error(`Pago con ID ${id} no encontrado`);
  }

  return populatePayment(payment);
}

// Obtener pagos de una orden específica
export async function getPaymentsByOrderId(
  orderId: number
): Promise<Payment[]> {
  return getPayments({ orderId });
}

// Crear pago
export async function createPayment(
  input: CreatePaymentInput
): Promise<Payment> {
  await delay(400);

  // Obtener la orden para validar el balance
  console.log('[createPayment] Input:', input);
  const order = await getOrderById(input.orderId);
  console.log('[createPayment] Order obtenida:', {
    id: order.id,
    total: order.total,
    balance: order.balance,
    amountPaid: order.amountPaid
  });

  // Validaciones
  if (input.amount <= 0) {
    throw new Error('El monto debe ser mayor a 0');
  }

  if (!order.balance && order.balance !== 0) {
    throw new Error('La orden no tiene un balance válido. Por favor, recarga la página e intenta de nuevo.');
  }

  if (input.amount > order.balance) {
    throw new Error(
      `El monto del pago ($${input.amount}) excede el saldo pendiente ($${order.balance})`
    );
  }

  // Validar que la orden no esté cancelada (cuando se implemente cancelación)
  // if (order.cancelledAt) {
  //   throw new Error('No se puede registrar un pago en una orden cancelada');
  // }

  // Validar que existe el método de pago
  await getPaymentMethodById(input.paymentMethodId);

  const newPayment: Payment = {
    id: nextId++,
    orderId: input.orderId,
    amount: input.amount,
    paymentMethodId: input.paymentMethodId,
    reference: input.reference,
    notes: input.notes,
    paidAt: input.paidAt,
    receivedBy: input.receivedBy,
    createdAt: new Date().toISOString(),
    createdBy: input.receivedBy,
  };

  payments.push(newPayment);

  // Actualizar totales de pago en la orden
  await updateOrderPaymentTotals(input.orderId);

  return populatePayment(newPayment);
}

// Actualizar pago
export async function updatePayment(
  id: number,
  input: UpdatePaymentInput
): Promise<Payment> {
  await delay(400);

  const index = payments.findIndex((p) => p.id === id);
  if (index === -1) {
    throw new Error(`Pago con ID ${id} no encontrado`);
  }

  const existingPayment = payments[index];

  // Obtener la orden para validar el balance
  const order = await getOrderById(existingPayment.orderId);

  // Si se actualiza el monto, validar que no exceda el balance
  if (input.amount !== undefined && input.amount !== existingPayment.amount) {
    const difference = input.amount - existingPayment.amount;
    const newBalance = order.balance - difference;

    if (newBalance < 0) {
      throw new Error(
        `El nuevo monto excedería el total de la orden. Saldo disponible: $${order.balance + existingPayment.amount}`
      );
    }
  }

  // Validar método de pago si se actualiza
  if (input.paymentMethodId) {
    await getPaymentMethodById(input.paymentMethodId);
  }

  const updatedPayment: Payment = {
    ...existingPayment,
    ...input,
  };

  payments[index] = updatedPayment;

  // Actualizar totales de pago en la orden
  await updateOrderPaymentTotals(updatedPayment.orderId);

  return populatePayment(updatedPayment);
}

// Eliminar/Revertir pago
export async function deletePayment(id: number): Promise<void> {
  await delay(300);

  const index = payments.findIndex((p) => p.id === id);
  if (index === -1) {
    throw new Error(`Pago con ID ${id} no encontrado`);
  }

  const orderId = payments[index].orderId;
  payments.splice(index, 1);

  // Actualizar totales de pago en la orden
  await updateOrderPaymentTotals(orderId);
}

// Calcular totales de pago de una orden
export function calculateOrderPaymentTotals(orderPayments: Payment[]): {
  amountPaid: number;
  balance: number;
  paymentStatus: 'pending' | 'partial' | 'paid';
} {
  const amountPaid = orderPayments.reduce((sum, p) => sum + p.amount, 0);

  return {
    amountPaid: Math.round(amountPaid * 100) / 100,
    balance: 0, // Será calculado en el servicio de órdenes
    paymentStatus: PAYMENT_STATUS.PENDING, // Será calculado en el servicio de órdenes
  };
}

// Funciones helper para operaciones atómicas desde ordersService
// Estas funciones permiten crear pagos directamente sin llamadas async
export function addPaymentToArray(payment: Payment): void {
  payments.push(payment);
}

export function getNextPaymentId(): number {
  return nextId++;
}

export { calculatePaymentStatus };
