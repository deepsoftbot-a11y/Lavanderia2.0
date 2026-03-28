import { z } from 'zod';

export const loginSchema = z.object({
  username: z
    .string()
    .min(1, 'El usuario es requerido')
    .min(3, 'El usuario debe tener al menos 3 caracteres'),
  password: z
    .string()
    .min(1, 'La contraseña es requerida')
    .min(4, 'La contraseña debe tener al menos 4 caracteres'),
});

export type LoginFormData = z.infer<typeof loginSchema>;
