import axiosInstance from './axiosInstance';
import type { CarDto, CreateCarRequest, PaginatedResponse } from './types';

const CAR_ENDPOINTS = {
  BASE: '/cars',
  DETAIL: (id: number) => `/cars/${id}`,
  BRANDS: '/cars/brands',
} as const;

/**
 * Get paginated list of cars
 */
export const getCars = async (
  page = 1,
  pageSize = 12,
  brand?: string
): Promise<PaginatedResponse<CarDto>> => {
  const params = new URLSearchParams({
    page: String(page),
    pageSize: String(pageSize),
  });

  if (brand) {
    params.append('brand', brand);
  }

  const response = await axiosInstance.get<PaginatedResponse<CarDto>>(
    CAR_ENDPOINTS.BASE,
    { params }
  );
  return response.data;
};

/**
 * Get car by ID
 */
export const getCarById = async (id: number): Promise<CarDto> => {
  const response = await axiosInstance.get<CarDto>(CAR_ENDPOINTS.DETAIL(id));
  return response.data;
};

/**
 * Create new car (Admin only)
 */
export const createCar = async (data: CreateCarRequest): Promise<CarDto> => {
  const response = await axiosInstance.post<CarDto>(CAR_ENDPOINTS.BASE, data);
  return response.data;
};

/**
 * Update car (Admin only)
 */
export const updateCar = async (
  id: number,
  data: Partial<CreateCarRequest>
): Promise<CarDto> => {
  const response = await axiosInstance.put<CarDto>(
    CAR_ENDPOINTS.DETAIL(id),
    data
  );
  return response.data;
};

/**
 * Delete car (Admin only)
 */
export const deleteCar = async (id: number): Promise<void> => {
  await axiosInstance.delete(CAR_ENDPOINTS.DETAIL(id));
};

/**
 * Get list of available car brands
 */
export const getBrands = async (): Promise<string[]> => {
  const response = await axiosInstance.get<string[]>(CAR_ENDPOINTS.BRANDS);
  return response.data;
};

/**
 * Upload car images (Admin only)
 */
export const uploadCarImages = async (
  carId: number,
  files: File[]
): Promise<string[]> => {
  const formData = new FormData();
  files.forEach((file) => {
    formData.append('images', file);
  });

  const response = await axiosInstance.post<string[]>(
    `${CAR_ENDPOINTS.DETAIL(carId)}/images`,
    formData,
    {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    }
  );
  return response.data;
};

export default {
  getCars,
  getCarById,
  createCar,
  updateCar,
  deleteCar,
  getBrands,
  uploadCarImages,
};
