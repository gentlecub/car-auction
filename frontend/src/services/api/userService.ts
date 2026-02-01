import axiosInstance from './axiosInstance';
import type {
  UserProfile,
  UpdateProfileRequest,
  NotificationDto,
  PaginatedResponse,
} from './types';

const USER_ENDPOINTS = {
  PROFILE: '/users/me',
  UPDATE_PROFILE: '/users/me',
  CHANGE_PASSWORD: '/users/me/password',
  NOTIFICATIONS: '/notifications',
  MARK_READ: (id: number) => `/notifications/${id}/read`,
  MARK_ALL_READ: '/notifications/read-all',
} as const;

/**
 * Get current user profile
 */
export const getProfile = async (): Promise<UserProfile> => {
  const response = await axiosInstance.get<UserProfile>(USER_ENDPOINTS.PROFILE);
  return response.data;
};

/**
 * Update user profile
 */
export const updateProfile = async (
  data: UpdateProfileRequest
): Promise<UserProfile> => {
  const response = await axiosInstance.put<UserProfile>(
    USER_ENDPOINTS.UPDATE_PROFILE,
    data
  );

  // Update stored user in localStorage
  const updatedUser = response.data;
  localStorage.setItem('user', JSON.stringify(updatedUser));

  return updatedUser;
};

/**
 * Change user password
 */
export const changePassword = async (
  currentPassword: string,
  newPassword: string
): Promise<void> => {
  await axiosInstance.post(USER_ENDPOINTS.CHANGE_PASSWORD, {
    currentPassword,
    newPassword,
  });
};

/**
 * Get user notifications
 */
export const getNotifications = async (
  page = 1,
  pageSize = 20,
  unreadOnly = false
): Promise<PaginatedResponse<NotificationDto>> => {
  const response = await axiosInstance.get<PaginatedResponse<NotificationDto>>(
    USER_ENDPOINTS.NOTIFICATIONS,
    { params: { page, pageSize, unreadOnly } }
  );
  return response.data;
};

/**
 * Mark notification as read
 */
export const markNotificationAsRead = async (id: number): Promise<void> => {
  await axiosInstance.put(USER_ENDPOINTS.MARK_READ(id));
};

/**
 * Mark all notifications as read
 */
export const markAllNotificationsAsRead = async (): Promise<void> => {
  await axiosInstance.put(USER_ENDPOINTS.MARK_ALL_READ);
};

/**
 * Get unread notification count
 */
export const getUnreadCount = async (): Promise<number> => {
  const response = await axiosInstance.get<{ count: number }>(
    `${USER_ENDPOINTS.NOTIFICATIONS}/unread-count`
  );
  return response.data.count;
};

export default {
  getProfile,
  updateProfile,
  changePassword,
  getNotifications,
  markNotificationAsRead,
  markAllNotificationsAsRead,
  getUnreadCount,
};
