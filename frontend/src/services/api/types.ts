// ============================================
// API Response Types
// ============================================

// Generic API Response
export interface ApiResponse<T> {
  data: T;
  success: boolean;
  message?: string;
}

// Paginated Response
export interface PaginatedResponse<T> {
  items: T[];
  totalItems: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

// Error Response
export interface ApiError {
  message: string;
  errors?: Record<string, string[]>;
  statusCode: number;
}

// ============================================
// Auth Types
// ============================================

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  name: string;
  confirmPassword: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
  user: UserProfile;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

// ============================================
// User Types
// ============================================

export interface UserProfile {
  id: number;
  email: string;
  name: string;
  role: 'User' | 'Admin';
  createdAt: string;
  avatarUrl?: string;
}

export interface UpdateProfileRequest {
  name?: string;
  avatarUrl?: string;
}

// ============================================
// Car Types (matching backend DTOs)
// ============================================

export interface CarDto {
  id: number;
  brand: string;
  model: string;
  year: number;
  mileage: number;
  condition: 'Nuevo' | 'Usado' | 'Certificado';
  description: string;
  features: string[];
  images: string[];
  image: string; // Primary image
}

export interface CreateCarRequest {
  brand: string;
  model: string;
  year: number;
  mileage: number;
  condition: 'Nuevo' | 'Usado' | 'Certificado';
  description: string;
  features: string[];
  images: string[];
}

// ============================================
// Auction Types
// ============================================

export interface AuctionDto {
  id: number;
  carId: number;
  car: CarDto;
  startPrice: number;
  currentBid: number;
  startTime: string;
  endTime: string;
  status: 'pending' | 'active' | 'finished' | 'cancelled';
  winnerId?: number;
  winnerName?: string;
  totalBids: number;
}

export interface CreateAuctionRequest {
  carId: number;
  startPrice: number;
  startTime: string;
  endTime: string;
}

export interface AuctionFilters {
  status?: 'active' | 'finished' | 'pending';
  brand?: string;
  minPrice?: number;
  maxPrice?: number;
  page?: number;
  pageSize?: number;
  sortBy?: 'endTime' | 'currentBid' | 'createdAt';
  sortOrder?: 'asc' | 'desc';
}

// ============================================
// Bid Types
// ============================================

export interface BidDto {
  id: number;
  auctionId: number;
  userId: number;
  userName: string;
  amount: number;
  timestamp: string;
}

export interface CreateBidRequest {
  auctionId: number;
  amount: number;
}

// ============================================
// Notification Types
// ============================================

export interface NotificationDto {
  id: number;
  userId: number;
  title: string;
  message: string;
  type: 'bid' | 'auction' | 'system';
  isRead: boolean;
  createdAt: string;
}

// ============================================
// Admin Stats Types
// ============================================

export interface AdminStats {
  totalAuctions: number;
  activeAuctions: number;
  finishedAuctions: number;
  totalUsers: number;
  totalRevenue: number;
  auctionsThisMonth: { month: string; auctions: number }[];
  userGrowth: { month: string; users: number }[];
}
