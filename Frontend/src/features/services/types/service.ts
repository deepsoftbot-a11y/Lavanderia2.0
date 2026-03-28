import type { Category } from './category';

export const CHARGE_TYPE = {
  PorPeso: 'PorPeso',
  PorPieza: 'PorPieza',
} as const;

export type ChargeType = typeof CHARGE_TYPE[keyof typeof CHARGE_TYPE];

export interface Service {
  id: number;
  categoryId: number;
  category: Category;
  code: string;
  name: string;
  chargeType: ChargeType;
  pricePerKg?: number;
  minWeight?: number;
  maxWeight?: number;
  description?: string;
  estimatedTime?: number;
  isActive: boolean;
}

export interface CreateServiceInput {
  categoryId: number;
  code: string;
  name: string;
  chargeType: ChargeType;
  pricePerKg?: number;
  minWeight?: number;
  maxWeight?: number;
  description?: string;
  estimatedTime?: number;
}

export interface UpdateServiceInput extends Partial<CreateServiceInput> {
  isActive?: boolean;
}
