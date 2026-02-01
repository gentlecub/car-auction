import axiosInstance from './axiosInstance';
import type { BidDto, CreateBidRequest, PaginatedResponse } from './types';

const BID_ENDPOINTS = {
  BASE: '/bids',
  BY_AUCTION: (auctionId: number) => `/bids/auction/${auctionId}`,
  MY_BIDS: '/bids/my',
  DETAIL: (id: number) => `/bids/${id}`,
} as const;

/**
 * Place a new bid on an auction
 */
export const placeBid = async (data: CreateBidRequest): Promise<BidDto> => {
  const response = await axiosInstance.post<BidDto>(BID_ENDPOINTS.BASE, data);
  return response.data;
};

/**
 * Get all bids for a specific auction
 */
export const getBidsByAuction = async (
  auctionId: number,
  page = 1,
  pageSize = 20
): Promise<PaginatedResponse<BidDto>> => {
  const response = await axiosInstance.get<PaginatedResponse<BidDto>>(
    BID_ENDPOINTS.BY_AUCTION(auctionId),
    { params: { page, pageSize } }
  );
  return response.data;
};

/**
 * Get current user's bid history
 */
export const getMyBids = async (
  page = 1,
  pageSize = 20
): Promise<PaginatedResponse<BidDto>> => {
  const response = await axiosInstance.get<PaginatedResponse<BidDto>>(
    BID_ENDPOINTS.MY_BIDS,
    { params: { page, pageSize } }
  );
  return response.data;
};

/**
 * Get bid by ID
 */
export const getBidById = async (id: number): Promise<BidDto> => {
  const response = await axiosInstance.get<BidDto>(BID_ENDPOINTS.DETAIL(id));
  return response.data;
};

/**
 * Calculate minimum bid amount (current + increment)
 */
export const calculateMinBid = (currentBid: number): number => {
  // Bid increment rules based on current price
  if (currentBid < 10000) return currentBid + 100;
  if (currentBid < 50000) return currentBid + 250;
  if (currentBid < 100000) return currentBid + 500;
  return currentBid + 1000;
};

/**
 * Validate bid amount
 */
export const validateBidAmount = (
  amount: number,
  currentBid: number
): { valid: boolean; message?: string } => {
  const minBid = calculateMinBid(currentBid);

  if (amount < minBid) {
    return {
      valid: false,
      message: `La puja mínima es $${minBid.toLocaleString()}`,
    };
  }

  return { valid: true };
};

export default {
  placeBid,
  getBidsByAuction,
  getMyBids,
  getBidById,
  calculateMinBid,
  validateBidAmount,
};
