import api from '@/api/axiosConfig';
import type { Category, CreateCategoryInput, UpdateCategoryInput } from '@/features/services/types/category';

export async function getCategories(): Promise<Category[]> {
  try {
    const response = await api.get<Category[]>('/categories');
    return response.data;
  } catch (error) {
    console.error('Get categories API error:', error);
    throw new Error('Error al obtener categorías desde el servidor');
  }
}

export async function getCategoryById(id: number): Promise<Category> {
  try {
    const response = await api.get<Category>(`/categories/${id}`);
    return response.data;
  } catch (error) {
    console.error('Get category by ID API error:', error);
    throw new Error('Error al obtener la categoría desde el servidor');
  }
}

export async function createCategory(input: CreateCategoryInput): Promise<Category> {
  try {
    const response = await api.post<Category>('/categories', input);
    return response.data;
  } catch (error) {
    console.error('Create category API error:', error);
    throw new Error('Error al crear la categoría en el servidor');
  }
}

export async function updateCategory(id: number, input: UpdateCategoryInput): Promise<Category> {
  try {
    const response = await api.put<Category>(`/categories/${id}`, input);
    return response.data;
  } catch (error) {
    console.error('Update category API error:', error);
    throw new Error('Error al actualizar la categoría en el servidor');
  }
}

export async function deleteCategory(id: number): Promise<void> {
  try {
    await api.delete(`/categories/${id}`);
  } catch (error) {
    console.error('Delete category API error:', error);
    throw new Error('Error al eliminar la categoría del servidor');
  }
}

export async function toggleCategoryStatus(id: number): Promise<Category> {
  try {
    const response = await api.patch<Category>(`/categories/${id}/status`);
    return response.data;
  } catch (error) {
    console.error('Toggle category status API error:', error);
    throw new Error('Error al cambiar el estado de la categoría');
  }
}
