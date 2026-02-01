import axiosInstance from './axiosInstance';
import type {
  LoginRequest,
  RegisterRequest,
  AuthResponse,
  RefreshTokenRequest,
  UserProfile,
} from './types';

const AUTH_ENDPOINTS = {
  LOGIN: '/auth/login',
  REGISTER: '/auth/register',
  REFRESH: '/auth/refresh',
  LOGOUT: '/auth/logout',
  ME: '/auth/me',
} as const;

/**
 * Login user with email and password
 */
export const login = async (credentials: LoginRequest): Promise<AuthResponse> => {
  const response = await axiosInstance.post<AuthResponse>(
    AUTH_ENDPOINTS.LOGIN,
    credentials
  );

  const { accessToken, refreshToken, user } = response.data;

  // Store tokens and user in localStorage
  localStorage.setItem('accessToken', accessToken);
  localStorage.setItem('refreshToken', refreshToken);
  localStorage.setItem('user', JSON.stringify(user));

  return response.data;
};

/**
 * Register new user
 */
export const register = async (data: RegisterRequest): Promise<AuthResponse> => {
  const response = await axiosInstance.post<AuthResponse>(
    AUTH_ENDPOINTS.REGISTER,
    data
  );

  const { accessToken, refreshToken, user } = response.data;

  // Store tokens and user in localStorage
  localStorage.setItem('accessToken', accessToken);
  localStorage.setItem('refreshToken', refreshToken);
  localStorage.setItem('user', JSON.stringify(user));

  return response.data;
};

/**
 * Refresh access token using refresh token
 */
export const refreshToken = async (token: string): Promise<AuthResponse> => {
  const request: RefreshTokenRequest = { refreshToken: token };
  const response = await axiosInstance.post<AuthResponse>(
    AUTH_ENDPOINTS.REFRESH,
    request
  );

  const { accessToken, refreshToken: newRefreshToken } = response.data;

  localStorage.setItem('accessToken', accessToken);
  localStorage.setItem('refreshToken', newRefreshToken);

  return response.data;
};

/**
 * Logout user - clear tokens and optionally notify backend
 */
export const logout = async (): Promise<void> => {
  try {
    // Notify backend to invalidate refresh token
    await axiosInstance.post(AUTH_ENDPOINTS.LOGOUT);
  } catch {
    // Ignore errors - we'll clear local storage anyway
  } finally {
    // Clear local storage
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
  }
};

/**
 * Get current user profile from token
 */
export const getCurrentUser = async (): Promise<UserProfile> => {
  const response = await axiosInstance.get<UserProfile>(AUTH_ENDPOINTS.ME);
  return response.data;
};

/**
 * Check if user is authenticated (has valid token in storage)
 */
export const isAuthenticated = (): boolean => {
  const token = localStorage.getItem('accessToken');
  if (!token) return false;

  try {
    // Decode JWT payload to check expiration
    const payload = JSON.parse(atob(token.split('.')[1]));
    const expirationTime = payload.exp * 1000; // Convert to milliseconds
    return Date.now() < expirationTime;
  } catch {
    return false;
  }
};

/**
 * Get stored user from localStorage
 */
export const getStoredUser = (): UserProfile | null => {
  const userJson = localStorage.getItem('user');
  if (!userJson) return null;

  try {
    return JSON.parse(userJson) as UserProfile;
  } catch {
    return null;
  }
};

/**
 * Get stored access token
 */
export const getAccessToken = (): string | null => {
  return localStorage.getItem('accessToken');
};

export default {
  login,
  register,
  refreshToken,
  logout,
  getCurrentUser,
  isAuthenticated,
  getStoredUser,
  getAccessToken,
};
