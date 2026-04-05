import { z } from 'zod';
import { CHARGE_TYPE } from '@/features/services/types/service';

export const createServiceSchema = z
  .object({
    name: z
      .string()
      .min(2, 'El nombre debe tener al menos 2 caracteres')
      .max(100, 'El nombre no puede exceder 100 caracteres'),
    categoryId: z
      .number({ message: 'Debe seleccionar una categoría' })
      .int()
      .positive('Debe seleccionar una categoría'),
    chargeType: z.enum([CHARGE_TYPE.PorPeso, CHARGE_TYPE.PorPieza], {
      message: 'Tipo de cobro inválido',
    }),
    pricePerKg: z.number().positive('El precio por kilo debe ser mayor a 0').optional(),
    minWeight: z.number().min(0, 'El peso mínimo no puede ser negativo').optional(),
    maxWeight: z.number().min(0, 'El peso máximo no puede ser negativo').optional(),
    description: z.preprocess(
      (v) => (v === '' ? undefined : v),
      z.string().max(500, 'La descripción no puede exceder 500 caracteres').optional()
    ),
    estimatedTime: z.number().min(0, 'El tiempo estimado no puede ser negativo').optional(),
  })
  .refine(
    (data) => {
      if (data.chargeType === CHARGE_TYPE.PorPeso) {
        return data.pricePerKg !== undefined && data.pricePerKg > 0;
      }
      return true;
    },
    {
      message: 'El precio por kilo es requerido para servicios por peso',
      path: ['pricePerKg'],
    }
  )
  .refine(
    (data) => {
      if (data.minWeight !== undefined && data.maxWeight !== undefined) {
        return data.maxWeight >= data.minWeight;
      }
      return true;
    },
    {
      message: 'El peso máximo debe ser mayor o igual al peso mínimo',
      path: ['maxWeight'],
    }
  );

export const updateServiceSchema = z
  .object({
    code: z.string().min(1).max(20).optional(),
    name: z.string().min(2).max(100).optional(),
    categoryId: z.number().int().positive().optional(),
    chargeType: z.enum([CHARGE_TYPE.PorPeso, CHARGE_TYPE.PorPieza]).optional(),
    pricePerKg: z.number().positive().optional(),
    minWeight: z.number().min(0).optional(),
    maxWeight: z.number().min(0).optional(),
    description: z.preprocess(
      (v) => (v === '' ? undefined : v),
      z.string().max(500).optional()
    ),
    estimatedTime: z.number().min(0).optional(),
    isActive: z.boolean().optional(),
  })
  .refine(
    (data) => {
      if (data.minWeight !== undefined && data.maxWeight !== undefined) {
        return data.maxWeight >= data.minWeight;
      }
      return true;
    },
    {
      message: 'El peso máximo debe ser mayor o igual al peso mínimo',
      path: ['maxWeight'],
    }
  );

export type CreateServiceFormData = z.infer<typeof createServiceSchema>;
export type UpdateServiceFormData = z.infer<typeof updateServiceSchema>;
