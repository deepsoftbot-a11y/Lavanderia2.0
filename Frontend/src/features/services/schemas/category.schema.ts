import { z } from 'zod';

export const createCategorySchema = z.object({
  name: z
    .string()
    .min(2, 'El nombre debe tener al menos 2 caracteres')
    .max(100, 'El nombre no puede exceder 100 caracteres'),
  description: z.preprocess(
    (v) => (v === '' ? undefined : v),
    z.string().max(255, 'La descripción no puede exceder 255 caracteres').optional()
  ),
});

export const updateCategorySchema = z.object({
  name: z.string().min(2).max(100).optional(),
  description: z.string().max(255).optional(),
  isActive: z.boolean().optional(),
});

export type CreateCategoryFormData = z.infer<typeof createCategorySchema>;
export type UpdateCategoryFormData = z.infer<typeof updateCategorySchema>;
