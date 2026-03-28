import { z } from 'zod';

export const createCashClosingSchema = z.object({
  fondoInicial: z
    .number()
    .min(0, 'El fondo inicial debe ser mayor o igual a 0')
    .default(0),
  actualCashAmount: z
    .number()
    .min(0, 'El efectivo contado debe ser mayor o igual a 0'),
  cashWithdrawal: z
    .number()
    .min(0, 'El retiro de efectivo debe ser mayor o igual a 0')
    .default(0),
  adjustment: z
    .number()
    .default(0),
  notes: z
    .string()
    .max(500, 'Las notas no pueden exceder 500 caracteres')
    .optional(),
});

export type CreateCashClosingFormData = z.infer<typeof createCashClosingSchema>;
