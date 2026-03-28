import type { Order } from './order';
import type { User } from '@/features/users/types/user';
import type { PaymentMethod } from './paymentMethod';

export const PAYMENT_STATUS = {
  PENDING: 'pending',
  PARTIAL: 'partial',
  PAID: 'paid',
} as const;

export type PaymentStatus = (typeof PAYMENT_STATUS)[keyof typeof PAYMENT_STATUS];

export interface Payment {
  id: number;
  orderId: number;
  amount: number;
  paymentMethodId: number;
  reference?: string;
  notes?: string;
  paidAt: string; // ISO 8601
  receivedBy: number;
  createdAt: string;
  createdBy: number;

  // Relaciones pobladas (UI)
  order?: Order;
  paymentMethod?: PaymentMethod;
  receivedByUser?: User;
}

// DTOs
export interface CreatePaymentInput {
  orderId: number;
  amount: number;
  paymentMethodId: number;
  reference?: string;
  notes?: string;
  paidAt: string;
  receivedBy: number;
}

export interface UpdatePaymentInput {
  amount?: number;
  paymentMethodId?: number;
  reference?: string;
  notes?: string;
  paidAt?: string;
}

// Filtros
export interface PaymentFilters {
  orderId?: number;
  paymentMethodId?: number;
  startDate?: string;
  endDate?: string;
  receivedBy?: number;
}
