import type { GarmentType } from './garmentType';

export interface ServiceGarment {
  id: number;
  serviceId: number;
  garmentTypeId: number;
  garmentType: GarmentType;
  unitPrice: number;
  isActive: boolean;
}

export interface CreateServiceGarmentInput {
  serviceId: number;
  garmentTypeId: number;
  unitPrice: number;
}

export interface UpdateServiceGarmentInput {
  unitPrice?: number;
  isActive?: boolean;
}
