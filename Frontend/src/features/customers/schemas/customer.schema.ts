import { z } from 'zod';

// Schema para crear cliente
export const createCustomerSchema = z.object({
  name: z
    .string()
    .min(2, 'El nombre debe tener al menos 2 caracteres')
    .max(150, 'El nombre no puede exceder 150 caracteres'),

  phone: z
    .string()
    .min(10, 'El teléfono debe tener al menos 10 dígitos')
    .max(20, 'El teléfono no puede exceder 20 dígitos')
    .regex(/^[0-9+\-\s()]+$/, 'Formato de teléfono inválido'),

  email: z
    .string()
    .email('Correo electrónico inválido')
    .max(100, 'El correo no puede exceder 100 caracteres')
    .optional()
    .or(z.literal('')),

  address: z
    .string()
    .max(255, 'La dirección no puede exceder 255 caracteres')
    .optional(),

  // NUEVO: Validación para RFC
  rfc: z
    .string()
    .length(13, 'El RFC debe tener exactamente 13 caracteres')
    .regex(/^[A-Z]{4}\d{6}[A-Z0-9]{3}$/, 'Formato de RFC inválido (ej: ABCD850101XYZ)')
    .optional()
    .or(z.literal('')),

  // NUEVO: Validación para límite de crédito
  creditLimit: z
    .number()
    .min(0, 'El límite de crédito no puede ser negativo')
    .max(999999.99, 'El límite de crédito excede el máximo permitido')
    .optional(),
});

// Schema para actualizar cliente
export const updateCustomerSchema = z.object({
  name: z.string().min(2).max(150).optional(),
  phone: z.string().min(10).max(20).regex(/^[0-9+\-\s()]+$/).optional(),
  email: z.string().email().max(100).optional().or(z.literal('')),
  address: z.string().max(255).optional(),
  rfc: z.string().length(13).regex(/^[A-Z]{4}\d{6}[A-Z0-9]{3}$/).optional().or(z.literal('')),
  creditLimit: z.number().min(0).max(999999.99).optional(),
  currentBalance: z.number().optional(), // Permitir ajustes manuales
  isActive: z.boolean().optional(),
});

// Tipos inferidos
export type CreateCustomerFormData = z.infer<typeof createCustomerSchema>;
export type UpdateCustomerFormData = z.infer<typeof updateCustomerSchema>;
