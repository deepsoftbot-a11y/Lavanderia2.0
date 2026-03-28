export interface Category {
  id: number;
  name: string;
  description?: string;
  isActive: boolean;
}

export interface CreateCategoryInput {
  name: string;
  description?: string;
}

export interface UpdateCategoryInput extends Partial<CreateCategoryInput> {
  isActive?: boolean;
}
