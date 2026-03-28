import { z } from 'zod';

// Schema para pago inicial (sin orderId porque aún no existe la orden)
export const initialPaymentSchema = z.object({
  amount: z
    .number()
    .positive('El monto debe ser mayor a 0')
    .refine(
      (val) => Number(val.toFixed(2)) === val,
      'El monto no puede tener más de 2 decimales'
    ),
  paymentMethodId: z.number().min(1, 'Debe seleccionar un método de pago'),
  reference: z
    .string()
    .max(100, 'La referencia no puede exceder 100 caracteres')
    .optional(),
  notes: z
    .string()
    .max(500, 'Las notas no pueden exceder 500 caracteres')
    .optional(),
  paidAt: z.string().min(1, 'Debe especificar la fecha del pago'),
}).optional();

// Schema para item de orden
export const orderItemSchema = z.object({
  serviceId: z.number().min(1, 'Debe seleccionar un servicio'),
  serviceGarmentId: z.number().min(1, 'Debe seleccionar un tipo de prenda'),
  discountAmount: z.number().min(0, 'El descuento no puede ser negativo'),
  weightKilos: z.number().min(0, 'El peso no puede ser negativo'),
  quantity: z.number().int().min(0, 'La cantidad no puede ser negativa'),
  unitPrice: z.number().min(0, 'El precio unitario no puede ser negativo'),
  notes: z.string().max(500, 'Las notas no pueden exceder 500 caracteres').default(''),
}).refine(
  (data) => data.weightKilos > 0 || data.quantity > 0,
  {
    message: 'Debe especificar cantidad de piezas o peso en kilos',
    path: ['quantity'],
  }
);

// Schema para crear orden
export const createOrderSchema = z.object({
  clientId: z.number().min(1, 'Debe seleccionar un cliente'),
  promisedDate: z.string().min(1, 'La fecha prometida es obligatoria').refine(
    (date) => {
      const promisedDate = new Date(date);
      const tomorrow = new Date();
      tomorrow.setDate(tomorrow.getDate() + 1);
      tomorrow.setHours(0, 0, 0, 0);
      return promisedDate >= tomorrow;
    },
    {
      message: 'La fecha prometida debe ser mínimo mañana',
    }
  ),
  initialStatusId: z.number().min(1, 'Debe seleccionar un estado inicial'),
  notes: z.string().max(1000, 'Las notas no pueden exceder 1000 caracteres').default(''),
  storageLocation: z.string().min(1, 'La ubicación de almacenamiento es obligatoria').max(200, 'La ubicación no puede exceder 200 caracteres'),
  receivedBy: z.number().int().positive('El ID del usuario que recibe es obligatorio'),
  items: z.array(orderItemSchema).min(1, 'Debe agregar al menos un servicio'),
  initialPayment: initialPaymentSchema, // Pago inicial opcional
});

// Schema para actualizar orden
export const updateOrderSchema = z.object({
  promisedDate: z.string().optional(),
  initialStatusId: z.number().optional(),
  notes: z.string().max(1000).optional(),
  storageLocation: z.string().min(1).max(200).optional(),
});

// Tipos inferidos
export type CreateOrderFormData = z.infer<typeof createOrderSchema>;
export type OrderItemFormData = z.infer<typeof orderItemSchema>;
export type UpdateOrderFormData = z.infer<typeof updateOrderSchema>;
