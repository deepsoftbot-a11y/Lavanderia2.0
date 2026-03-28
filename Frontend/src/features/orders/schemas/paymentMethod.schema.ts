import { z } from 'zod';

export const createPaymentMethodSchema = z.object({
  name: z.string().min(1, 'El nombre es obligatorio').max(100, 'El nombre no puede exceder 100 caracteres'),
  description: z.string().max(500, 'La descripción no puede exceder 500 caracteres').optional(),
});

export const updatePaymentMethodSchema = z.object({
  name: z.string().min(1).max(100).optional(),
  description: z.string().max(500).optional(),
  isActive: z.boolean().optional(),
});
