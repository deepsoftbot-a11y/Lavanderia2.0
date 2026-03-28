export interface GarmentType {
  id: number;
  name: string;
  description?: string;
  isActive: boolean;
}

export interface CreateGarmentTypeInput {
  name: string;
  description?: string;
}

export interface UpdateGarmentTypeInput extends Partial<CreateGarmentTypeInput> {
  isActive?: boolean;
}
