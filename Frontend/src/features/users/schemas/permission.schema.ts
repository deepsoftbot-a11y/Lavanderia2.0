import { z } from 'zod';

export const permissionSchema = z.object({
  name: z
    .string()
    .min(1, 'El nombre es requerido')
    .regex(/^[a-z]+\.[a-z_]+$/, 'Formato inválido. Usa: modulo.accion (ej: pedidos.crear)'),

  module: z
    .string()
    .min(1, 'El módulo es requerido')
    .regex(/^[a-z]+$/, 'Solo letras minúsculas sin espacios'),

  description: z.string().optional(),
});

export type PermissionFormData = z.infer<typeof permissionSchema>;
