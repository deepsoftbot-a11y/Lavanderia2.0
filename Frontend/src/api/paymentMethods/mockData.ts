import type { PaymentMethod } from '@/features/orders/types/paymentMethod';

export const mockPaymentMethods: PaymentMethod[] = [
  {
    id: 1,
    name: 'Efectivo',
    description: 'Pago en efectivo',
    isActive: true,
    createdAt: '2024-01-01T00:00:00Z',
  },
  {
    id: 2,
    name: 'Tarjeta',
    description: 'Pago con tarjeta de crédito o débito',
    isActive: true,
    createdAt: '2024-01-01T00:00:00Z',
  },
  {
    id: 3,
    name: 'Transferencia',
    description: 'Transferencia bancaria',
    isActive: true,
    createdAt: '2024-01-01T00:00:00Z',
  },
  {
    id: 4,
    name: 'Cheque',
    description: 'Pago con cheque',
    isActive: false,
    createdAt: '2024-01-01T00:00:00Z',
  },
];
