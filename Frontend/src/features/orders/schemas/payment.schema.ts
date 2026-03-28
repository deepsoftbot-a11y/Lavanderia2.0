import { z } from 'zod';

export const createPaymentSchema = z.object({
  orderId: z.number().min(1, 'Debe especificar la orden'),
  amount: z
    .number()
    .positive('El monto debe ser mayor a 0')
    .refine(
      (val) => Number(val.toFixed(2)) === val,
      'El monto no puede tener más de 2 decimales'
    ),
  paymentMethodId: z.number().min(1, 'Debe seleccionar un método de pago'),
  reference: z
    .string()
    .max(100, 'La referencia no puede exceder 100 caracteres')
    .optional(),
  notes: z
    .string()
    .max(500, 'Las notas no pueden exceder 500 caracteres')
    .optional(),
  paidAt: z.string().min(1, 'Debe especificar la fecha del pago'),
  receivedBy: z.number().min(1, 'Debe especificar el usuario que registra el pago'),
});

export const updatePaymentSchema = z.object({
  amount: z.number().positive().optional(),
  paymentMethodId: z.number().min(1).optional(),
  reference: z.string().max(100).optional(),
  notes: z.string().max(500).optional(),
  paidAt: z.string().optional(),
});
