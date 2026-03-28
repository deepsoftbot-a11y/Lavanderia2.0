import { z } from 'zod';

export const roleSchema = z.object({
  name: z
    .string()
    .min(1, 'El nombre es requerido')
    .min(3, 'El nombre debe tener al menos 3 caracteres')
    .max(50, 'El nombre no puede exceder 50 caracteres'),

  description: z.string().optional(),

  isActive: z.boolean(),

  permissionIds: z
    .array(z.number())
    .min(1, 'Selecciona al menos un permiso'),
});

export type RoleFormData = z.infer<typeof roleSchema>;
