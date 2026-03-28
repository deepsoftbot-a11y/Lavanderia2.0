import axios from 'axios';
import api from '@/api/axiosConfig';
import type { Discount, CreateDiscountInput, UpdateDiscountInput } from '@/features/services/types/discount';

export async function getDiscounts(): Promise<Discount[]> {
  try {
    const response = await api.get<Discount[]>('/discounts');
    return response.data;
  } catch (error) {
    console.error('Get discounts API error:', error);
    throw new Error('Error al obtener descuentos desde el servidor');
  }
}

export async function getDiscountById(id: number): Promise<Discount> {
  try {
    const response = await api.get<Discount>(`/discounts/${id}`);
    return response.data;
  } catch (error) {
    console.error('Get discount by ID API error:', error);
    throw new Error('Error al obtener el descuento desde el servidor');
  }
}

export async function createDiscount(input: CreateDiscountInput): Promise<Discount> {
  try {
    const response = await api.post<Discount>('/discounts', input);
    return response.data;
  } catch (error) {
    console.error('Create discount API error:', error);
    if (axios.isAxiosError(error) && error.response?.status === 409) {
      throw new Error('Ya existe un descuento con ese nombre');
    }
    throw new Error('Error al crear el descuento en el servidor');
  }
}

export async function updateDiscount(id: number, input: UpdateDiscountInput): Promise<Discount> {
  try {
    const response = await api.put<Discount>(`/discounts/${id}`, input);
    return response.data;
  } catch (error) {
    console.error('Update discount API error:', error);
    if (axios.isAxiosError(error) && error.response?.status === 409) {
      throw new Error('Ya existe un descuento con ese nombre');
    }
    throw new Error('Error al actualizar el descuento en el servidor');
  }
}

export async function deleteDiscount(id: number): Promise<void> {
  try {
    await api.delete(`/discounts/${id}`);
  } catch (error) {
    console.error('Delete discount API error:', error);
    throw new Error('Error al eliminar el descuento del servidor');
  }
}

export async function toggleDiscountStatus(id: number): Promise<Discount> {
  try {
    const response = await api.patch<Discount>(`/discounts/${id}/status`);
    return response.data;
  } catch (error) {
    console.error('Toggle discount status API error:', error);
    throw new Error('Error al cambiar el estado del descuento');
  }
}
