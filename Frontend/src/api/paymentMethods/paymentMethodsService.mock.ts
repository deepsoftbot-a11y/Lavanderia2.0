import type {
  PaymentMethod,
  CreatePaymentMethodInput,
  UpdatePaymentMethodInput,
} from '@/features/orders/types/paymentMethod';
import { mockPaymentMethods } from './mockData';

const delay = (ms: number) => new Promise((resolve) => setTimeout(resolve, ms));

let paymentMethods = [...mockPaymentMethods];
let nextId = Math.max(...paymentMethods.map((pm) => pm.id)) + 1;

// Obtener todos los métodos de pago
export async function getPaymentMethods(): Promise<PaymentMethod[]> {
  await delay(200);
  return paymentMethods.filter((pm) => pm.isActive);
}

// Obtener todos los métodos de pago (incluyendo inactivos)
export async function getAllPaymentMethods(): Promise<PaymentMethod[]> {
  await delay(200);
  return paymentMethods;
}

// Obtener método de pago por ID
export async function getPaymentMethodById(
  id: number
): Promise<PaymentMethod> {
  await delay(200);

  const paymentMethod = paymentMethods.find((pm) => pm.id === id);
  if (!paymentMethod) {
    throw new Error(`Método de pago con ID ${id} no encontrado`);
  }

  return paymentMethod;
}

// Crear método de pago
export async function createPaymentMethod(
  input: CreatePaymentMethodInput
): Promise<PaymentMethod> {
  await delay(300);

  // Validar que no exista uno con el mismo nombre
  const exists = paymentMethods.find(
    (pm) => pm.name.toLowerCase() === input.name.toLowerCase()
  );
  if (exists) {
    throw new Error('Ya existe un método de pago con ese nombre');
  }

  const newPaymentMethod: PaymentMethod = {
    id: nextId++,
    name: input.name,
    description: input.description,
    isActive: true,
    createdAt: new Date().toISOString(),
  };

  paymentMethods.push(newPaymentMethod);

  return newPaymentMethod;
}

// Actualizar método de pago
export async function updatePaymentMethod(
  id: number,
  input: UpdatePaymentMethodInput
): Promise<PaymentMethod> {
  await delay(300);

  const index = paymentMethods.findIndex((pm) => pm.id === id);
  if (index === -1) {
    throw new Error(`Método de pago con ID ${id} no encontrado`);
  }

  // Validar que el nombre no esté duplicado
  if (input.name) {
    const exists = paymentMethods.find(
      (pm) =>
        pm.id !== id && pm.name.toLowerCase() === input.name!.toLowerCase()
    );
    if (exists) {
      throw new Error('Ya existe un método de pago con ese nombre');
    }
  }

  const updatedPaymentMethod: PaymentMethod = {
    ...paymentMethods[index],
    ...input,
  };

  paymentMethods[index] = updatedPaymentMethod;

  return updatedPaymentMethod;
}

// Desactivar método de pago
export async function deactivatePaymentMethod(
  id: number
): Promise<PaymentMethod> {
  return updatePaymentMethod(id, { isActive: false });
}

// Activar método de pago
export async function activatePaymentMethod(
  id: number
): Promise<PaymentMethod> {
  return updatePaymentMethod(id, { isActive: true });
}
