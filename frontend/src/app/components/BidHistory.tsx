import { Bid } from '@/app/data/mockData';
import { TrendingUp, User } from 'lucide-react';
import { motion } from 'motion/react';

interface BidHistoryProps {
  bids: Bid[];
}

export function BidHistory({ bids }: BidHistoryProps) {
  const sortedBids = [...bids].sort((a, b) => b.timestamp.getTime() - a.timestamp.getTime());

  const formatTime = (date: Date) => {
    const now = new Date();
    const diff = now.getTime() - date.getTime();
    const minutes = Math.floor(diff / 60000);
    
    if (minutes < 1) return 'Just now';
    if (minutes === 1) return '1 minute ago';
    if (minutes < 60) return `${minutes} minutes ago`;

    const hours = Math.floor(minutes / 60);
    if (hours === 1) return '1 hour ago';
    if (hours < 24) return `${hours} hours ago`;
    
    return date.toLocaleDateString();
  };

  return (
    <div className="bg-white rounded-xl shadow-md p-6">
      <div className="flex items-center gap-2 mb-6">
        <TrendingUp className="w-6 h-6 text-[#1E40AF]" />
        <h3 className="text-xl font-semibold text-[#111827]">Bid History</h3>
      </div>

      {sortedBids.length === 0 ? (
        <p className="text-gray-500 text-center py-8">No bids yet on this auction</p>
      ) : (
        <div className="space-y-3 max-h-96 overflow-y-auto">
          {sortedBids.map((bid, index) => (
            <motion.div
              key={bid.id}
              initial={{ opacity: 0, x: -20 }}
              animate={{ opacity: 1, x: 0 }}
              transition={{ delay: index * 0.05 }}
              className={`p-4 rounded-lg border-2 transition-all ${
                index === 0 
                  ? 'border-[#22C55E] bg-green-50' 
                  : 'border-gray-200 bg-[#F9FAFB]'
              }`}
            >
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-3">
                  <div className="w-10 h-10 rounded-full bg-[#1E40AF] flex items-center justify-center">
                    <User className="w-5 h-5 text-white" />
                  </div>
                  <div>
                    <p className="font-medium text-[#111827]">{bid.userName}</p>
                    <p className="text-sm text-gray-600">{formatTime(bid.timestamp)}</p>
                  </div>
                </div>
                <div className="text-right">
                  <p className={`text-xl font-bold ${
                    index === 0 ? 'text-[#22C55E]' : 'text-[#1E40AF]'
                  }`}>
                    ${bid.amount.toLocaleString()}
                  </p>
                  {index === 0 && (
                    <p className="text-xs text-[#22C55E] font-medium mt-1">Highest bid</p>
                  )}
                </div>
              </div>
            </motion.div>
          ))}
        </div>
      )}
    </div>
  );
}
