// ============================================
// API Services - Barrel Export
// ============================================

// Axios instance (for custom requests)
export { default as axiosInstance } from './axiosInstance';

// Services
export { default as authService } from './authService';
export { default as auctionService } from './auctionService';
export { default as bidService } from './bidService';
export { default as carService } from './carService';
export { default as userService } from './userService';

// Individual exports for tree-shaking
export * from './authService';
export * from './auctionService';
export * from './bidService';
export * from './carService';
export * from './userService';

// Types
export * from './types';
