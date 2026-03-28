export interface PaymentMethod {
  id: number;
  name: string;
  description?: string;
  isActive: boolean;
  createdAt: string;
}

// DTOs
export interface CreatePaymentMethodInput {
  name: string;
  description?: string;
}

export interface UpdatePaymentMethodInput {
  name?: string;
  description?: string;
  isActive?: boolean;
}
