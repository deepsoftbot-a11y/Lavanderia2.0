export type DiscountType = 'Percentage' | 'FixedAmount';

export interface Discount {
  id: number;
  name: string;
  type: DiscountType;
  value: number;
  startDate: string;
  endDate?: string;
  isActive: boolean;
}

export interface CreateDiscountInput {
  name: string;
  type: DiscountType;
  value: number;
  startDate: string;
  endDate?: string;
}

export interface UpdateDiscountInput extends Partial<CreateDiscountInput> {
  isActive?: boolean;
}
