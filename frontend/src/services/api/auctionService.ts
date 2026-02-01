import axiosInstance from './axiosInstance';
import type {
  AuctionDto,
  CreateAuctionRequest,
  AuctionFilters,
  PaginatedResponse,
} from './types';

const AUCTION_ENDPOINTS = {
  BASE: '/auctions',
  DETAIL: (id: number) => `/auctions/${id}`,
  MY_AUCTIONS: '/auctions/my',
  ACTIVE: '/auctions/active',
} as const;

/**
 * Get paginated list of auctions with optional filters
 */
export const getAuctions = async (
  filters?: AuctionFilters
): Promise<PaginatedResponse<AuctionDto>> => {
  const params = new URLSearchParams();

  if (filters) {
    Object.entries(filters).forEach(([key, value]) => {
      if (value !== undefined && value !== null) {
        params.append(key, String(value));
      }
    });
  }

  const response = await axiosInstance.get<PaginatedResponse<AuctionDto>>(
    AUCTION_ENDPOINTS.BASE,
    { params }
  );
  return response.data;
};

/**
 * Get active auctions only
 */
export const getActiveAuctions = async (
  page = 1,
  pageSize = 12
): Promise<PaginatedResponse<AuctionDto>> => {
  return getAuctions({ status: 'active', page, pageSize });
};

/**
 * Get auction by ID with full details
 */
export const getAuctionById = async (id: number): Promise<AuctionDto> => {
  const response = await axiosInstance.get<AuctionDto>(
    AUCTION_ENDPOINTS.DETAIL(id)
  );
  return response.data;
};

/**
 * Create new auction (Admin only)
 */
export const createAuction = async (
  data: CreateAuctionRequest
): Promise<AuctionDto> => {
  const response = await axiosInstance.post<AuctionDto>(
    AUCTION_ENDPOINTS.BASE,
    data
  );
  return response.data;
};

/**
 * Update auction (Admin only)
 */
export const updateAuction = async (
  id: number,
  data: Partial<CreateAuctionRequest>
): Promise<AuctionDto> => {
  const response = await axiosInstance.put<AuctionDto>(
    AUCTION_ENDPOINTS.DETAIL(id),
    data
  );
  return response.data;
};

/**
 * Delete/Cancel auction (Admin only)
 */
export const deleteAuction = async (id: number): Promise<void> => {
  await axiosInstance.delete(AUCTION_ENDPOINTS.DETAIL(id));
};

/**
 * Get auctions where current user has bid
 */
export const getMyAuctions = async (): Promise<AuctionDto[]> => {
  const response = await axiosInstance.get<AuctionDto[]>(
    AUCTION_ENDPOINTS.MY_AUCTIONS
  );
  return response.data;
};

export default {
  getAuctions,
  getActiveAuctions,
  getAuctionById,
  createAuction,
  updateAuction,
  deleteAuction,
  getMyAuctions,
};
