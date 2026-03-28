import { z } from 'zod';

export const createUserSchema = z.object({
  username: z
    .string()
    .min(1, 'El usuario es requerido')
    .min(4, 'El usuario debe tener al menos 4 caracteres')
    .max(50, 'El usuario no puede exceder 50 caracteres')
    .regex(/^[a-zA-Z0-9_]+$/, 'El usuario solo puede contener letras, números y guiones bajos'),

  fullName: z
    .string()
    .min(1, 'El nombre es requerido')
    .min(3, 'El nombre debe tener al menos 3 caracteres')
    .max(150, 'El nombre no puede exceder 150 caracteres'),

  email: z
    .string()
    .min(1, 'El email es requerido')
    .email('Email inválido'),

  password: z
    .string()
    .min(1, 'La contraseña es requerida')
    .min(6, 'La contraseña debe tener al menos 6 caracteres')
    .max(100, 'La contraseña no puede exceder 100 caracteres'),

  confirmPassword: z
    .string()
    .min(1, 'Confirme la contraseña'),

  roleId: z
    .number()
    .int()
    .positive('El rol es requerido'),

  isActive: z.boolean(),
}).refine((data) => data.password === data.confirmPassword, {
  message: 'Las contraseñas no coinciden',
  path: ['confirmPassword'],
});

export const updateUserSchema = z.object({
  fullName: z
    .string()
    .min(3, 'El nombre debe tener al menos 3 caracteres')
    .max(150, 'El nombre no puede exceder 150 caracteres')
    .optional(),

  email: z
    .string()
    .email('Email inválido')
    .optional(),

  roleId: z.number().int().positive().nullable().optional(),

  isActive: z.boolean().optional(),

  password: z
    .string()
    .min(6, 'La contraseña debe tener al menos 6 caracteres')
    .max(100, 'La contraseña no puede exceder 100 caracteres')
    .optional(),

  confirmPassword: z.string().optional(),
}).refine(
  (data) => {
    if (data.password && !data.confirmPassword) return false;
    if (data.confirmPassword && !data.password) return false;
    if (data.password && data.confirmPassword) {
      return data.password === data.confirmPassword;
    }
    return true;
  },
  {
    message: 'Las contraseñas no coinciden',
    path: ['confirmPassword'],
  }
);

export type CreateUserFormData = z.infer<typeof createUserSchema>;
export type UpdateUserFormData = z.infer<typeof updateUserSchema>;
