import { Car } from '@/app/data/mockData';
import { CountdownTimer } from './CountdownTimer';
import { TrendingUp } from 'lucide-react';

interface CarCardProps {
  car: Car;
  onClick: () => void;
}

export function CarCard({ car, onClick }: CarCardProps) {
  const priceIncrease = ((car.currentBid - car.startPrice) / car.startPrice * 100).toFixed(1);

  return (
    <div 
      onClick={onClick}
      className="bg-white rounded-xl shadow-md hover:shadow-xl transition-all duration-300 cursor-pointer overflow-hidden group"
    >
      {/* Image */}
      <div className="relative h-48 overflow-hidden">
        <img 
          src={car.image} 
          alt={`${car.brand} ${car.model}`}
          className="w-full h-full object-cover group-hover:scale-110 transition-transform duration-300"
        />
        <div className="absolute top-3 right-3">
          <span className={`px-3 py-1 rounded-full text-xs font-medium ${
            car.condition === 'New'
              ? 'bg-[#22C55E] text-white'
              : car.condition === 'Certified'
              ? 'bg-[#1E40AF] text-white'
              : 'bg-gray-600 text-white'
          }`}>
            {car.condition}
          </span>
        </div>
      </div>

      {/* Content */}
      <div className="p-5">
        <h3 className="text-xl font-semibold text-[#111827] mb-1">
          {car.brand} {car.model}
        </h3>
        <p className="text-sm text-gray-600 mb-4">
          {car.year} • {car.mileage.toLocaleString()} km
        </p>

        {/* Price */}
        <div className="mb-4">
          <p className="text-sm text-gray-600 mb-1">Current bid</p>
          <div className="flex items-end gap-2">
            <span className="text-3xl font-bold text-[#1E40AF]">
              ${car.currentBid.toLocaleString()}
            </span>
            <div className="flex items-center gap-1 text-[#22C55E] mb-1">
              <TrendingUp className="w-4 h-4" />
              <span className="text-sm font-medium">+{priceIncrease}%</span>
            </div>
          </div>
        </div>

        {/* Countdown */}
        <CountdownTimer endTime={car.endTime} size="small" />
      </div>
    </div>
  );
}
