import { z } from 'zod';

export const createServiceGarmentSchema = z.object({
  garmentTypeId: z
    .number({ message: 'Debe seleccionar un tipo de prenda' })
    .int()
    .positive('Debe seleccionar un tipo de prenda'),
  unitPrice: z.number().positive('El precio unitario debe ser mayor a 0'),
});

export const updateServiceGarmentSchema = z.object({
  unitPrice: z.number().positive('El precio unitario debe ser mayor a 0').optional(),
  isActive: z.boolean().optional(),
});

export type CreateServiceGarmentFormData = z.infer<typeof createServiceGarmentSchema>;
export type UpdateServiceGarmentFormData = z.infer<typeof updateServiceGarmentSchema>;
