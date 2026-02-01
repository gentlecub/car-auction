import { useState, useEffect, useCallback, useRef } from 'react';
import {
  HubConnectionBuilder,
  HubConnection,
  HubConnectionState,
  LogLevel,
} from '@microsoft/signalr';
import { getAccessToken } from '../api/authService';
import type { BidDto, AuctionDto } from '../api/types';

// ============================================
// Types
// ============================================

interface AuctionHubEvents {
  onBidPlaced?: (bid: BidDto) => void;
  onAuctionUpdated?: (auction: AuctionDto) => void;
  onAuctionEnded?: (auctionId: number, winnerId: number | null) => void;
  onConnected?: () => void;
  onDisconnected?: (error?: Error) => void;
  onReconnecting?: () => void;
}

interface UseAuctionHubReturn {
  isConnected: boolean;
  isConnecting: boolean;
  error: Error | null;
  joinAuction: (auctionId: number) => Promise<void>;
  leaveAuction: (auctionId: number) => Promise<void>;
  placeBid: (auctionId: number, amount: number) => Promise<void>;
}

// ============================================
// Constants
// ============================================

const WS_URL = import.meta.env.VITE_WS_URL || 'http://localhost:5000/hubs/auction';

// ============================================
// Hook
// ============================================

export const useAuctionHub = (events?: AuctionHubEvents): UseAuctionHubReturn => {
  const [isConnected, setIsConnected] = useState(false);
  const [isConnecting, setIsConnecting] = useState(false);
  const [error, setError] = useState<Error | null>(null);

  const connectionRef = useRef<HubConnection | null>(null);
  const eventsRef = useRef(events);

  // Keep events ref updated
  useEffect(() => {
    eventsRef.current = events;
  }, [events]);

  // Initialize connection
  useEffect(() => {
    const startConnection = async () => {
      if (connectionRef.current?.state === HubConnectionState.Connected) {
        return;
      }

      setIsConnecting(true);
      setError(null);

      try {
        const connection = new HubConnectionBuilder()
          .withUrl(WS_URL, {
            accessTokenFactory: () => getAccessToken() || '',
          })
          .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
          .configureLogging(
            import.meta.env.DEV ? LogLevel.Information : LogLevel.Warning
          )
          .build();

        // Event handlers
        connection.on('BidPlaced', (bid: BidDto) => {
          eventsRef.current?.onBidPlaced?.(bid);
        });

        connection.on('AuctionUpdated', (auction: AuctionDto) => {
          eventsRef.current?.onAuctionUpdated?.(auction);
        });

        connection.on('AuctionEnded', (auctionId: number, winnerId: number | null) => {
          eventsRef.current?.onAuctionEnded?.(auctionId, winnerId);
        });

        // Connection state handlers
        connection.onclose((err) => {
          setIsConnected(false);
          eventsRef.current?.onDisconnected?.(err);
        });

        connection.onreconnecting(() => {
          setIsConnected(false);
          eventsRef.current?.onReconnecting?.();
        });

        connection.onreconnected(() => {
          setIsConnected(true);
          eventsRef.current?.onConnected?.();
        });

        // Start connection
        await connection.start();

        connectionRef.current = connection;
        setIsConnected(true);
        setIsConnecting(false);
        eventsRef.current?.onConnected?.();
      } catch (err) {
        setError(err instanceof Error ? err : new Error('Connection failed'));
        setIsConnecting(false);
        eventsRef.current?.onDisconnected?.(
          err instanceof Error ? err : undefined
        );
      }
    };

    startConnection();

    // Cleanup on unmount
    return () => {
      if (connectionRef.current) {
        connectionRef.current.stop();
        connectionRef.current = null;
      }
    };
  }, []);

  // Join auction room
  const joinAuction = useCallback(async (auctionId: number) => {
    if (connectionRef.current?.state !== HubConnectionState.Connected) {
      throw new Error('Not connected to auction hub');
    }

    await connectionRef.current.invoke('JoinAuction', auctionId);
  }, []);

  // Leave auction room
  const leaveAuction = useCallback(async (auctionId: number) => {
    if (connectionRef.current?.state !== HubConnectionState.Connected) {
      return;
    }

    await connectionRef.current.invoke('LeaveAuction', auctionId);
  }, []);

  // Place bid via SignalR (real-time)
  const placeBid = useCallback(async (auctionId: number, amount: number) => {
    if (connectionRef.current?.state !== HubConnectionState.Connected) {
      throw new Error('Not connected to auction hub');
    }

    await connectionRef.current.invoke('PlaceBid', auctionId, amount);
  }, []);

  return {
    isConnected,
    isConnecting,
    error,
    joinAuction,
    leaveAuction,
    placeBid,
  };
};

export default useAuctionHub;
