import { z } from 'zod';

export const createDiscountSchema = z
  .object({
    name: z
      .string()
      .min(1, 'El nombre es requerido')
      .max(100, 'El nombre no puede exceder 100 caracteres'),
    type: z.enum(['Percentage', 'FixedAmount'], {
      message: 'Tipo de descuento inválido',
    }),
    value: z.number().positive('El valor debe ser mayor a 0'),
    startDate: z.string().min(1, 'La fecha de inicio es requerida'),
    endDate: z.string().optional(),
  })
  .refine(
    (data) => {
      if (data.type === 'Percentage') {
        return data.value <= 100;
      }
      return true;
    },
    {
      message: 'El porcentaje de descuento no puede superar 100',
      path: ['value'],
    }
  )
  .refine(
    (data) => {
      if (data.startDate && data.endDate) {
        return new Date(data.endDate) >= new Date(data.startDate);
      }
      return true;
    },
    {
      message: 'La fecha de fin debe ser posterior o igual a la fecha de inicio',
      path: ['endDate'],
    }
  );

export const updateDiscountSchema = z
  .object({
    name: z.string().min(1).max(100).optional(),
    type: z.enum(['Percentage', 'FixedAmount']).optional(),
    value: z.number().positive().optional(),
    startDate: z.string().optional(),
    endDate: z.string().optional(),
    isActive: z.boolean().optional(),
  })
  .refine(
    (data) => {
      if (data.type === 'Percentage' && data.value !== undefined) {
        return data.value <= 100;
      }
      return true;
    },
    {
      message: 'El porcentaje de descuento no puede superar 100',
      path: ['value'],
    }
  );

export type CreateDiscountFormData = z.infer<typeof createDiscountSchema>;
export type UpdateDiscountFormData = z.infer<typeof updateDiscountSchema>;
