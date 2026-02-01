import { useEffect, useState } from 'react';
import { Clock } from 'lucide-react';

interface CountdownTimerProps {
  endTime: Date;
  size?: 'small' | 'large';
}

interface TimeLeft {
  days: number;
  hours: number;
  minutes: number;
  seconds: number;
}

export function CountdownTimer({ endTime, size = 'small' }: CountdownTimerProps) {
  const [timeLeft, setTimeLeft] = useState<TimeLeft>(calculateTimeLeft());

  function calculateTimeLeft(): TimeLeft {
    const difference = endTime.getTime() - Date.now();
    
    if (difference <= 0) {
      return { days: 0, hours: 0, minutes: 0, seconds: 0 };
    }

    return {
      days: Math.floor(difference / (1000 * 60 * 60 * 24)),
      hours: Math.floor((difference / (1000 * 60 * 60)) % 24),
      minutes: Math.floor((difference / 1000 / 60) % 60),
      seconds: Math.floor((difference / 1000) % 60),
    };
  }

  useEffect(() => {
    const timer = setInterval(() => {
      setTimeLeft(calculateTimeLeft());
    }, 1000);

    return () => clearInterval(timer);
  }, [endTime]);

  const isExpired = timeLeft.days === 0 && timeLeft.hours === 0 && timeLeft.minutes === 0 && timeLeft.seconds === 0;
  const isUrgent = !isExpired && timeLeft.days === 0 && timeLeft.hours < 1;

  if (size === 'small') {
    return (
      <div className={`flex items-center gap-1.5 px-3 py-1.5 rounded-lg ${
        isExpired 
          ? 'bg-gray-200 text-gray-600' 
          : isUrgent 
          ? 'bg-[#EF4444] text-white animate-pulse' 
          : 'bg-[#F9FAFB] text-[#111827]'
      }`}>
        <Clock className="w-4 h-4" />
        <span className="text-sm font-medium">
          {isExpired ? 'Finalizada' : `${timeLeft.hours}h ${timeLeft.minutes}m ${timeLeft.seconds}s`}
        </span>
      </div>
    );
  }

  return (
    <div className={`inline-flex flex-col items-center p-6 rounded-xl ${
      isExpired 
        ? 'bg-gray-200' 
        : isUrgent 
        ? 'bg-[#EF4444] animate-pulse' 
        : 'bg-[#1E40AF]'
    }`}>
      <div className="flex items-center gap-2 mb-3">
        <Clock className={`w-6 h-6 ${isExpired ? 'text-gray-600' : 'text-white'}`} />
        <span className={`text-sm font-medium ${isExpired ? 'text-gray-600' : 'text-white/80'}`}>
          {isExpired ? 'Subasta finalizada' : 'Tiempo restante'}
        </span>
      </div>
      <div className="flex gap-3">
        {!isExpired && timeLeft.days > 0 && (
          <div className="flex flex-col items-center">
            <span className="text-3xl font-bold text-white">{String(timeLeft.days).padStart(2, '0')}</span>
            <span className="text-xs text-white/70 mt-1">días</span>
          </div>
        )}
        <div className="flex flex-col items-center">
          <span className={`text-3xl font-bold ${isExpired ? 'text-gray-600' : 'text-white'}`}>
            {String(timeLeft.hours).padStart(2, '0')}
          </span>
          <span className={`text-xs mt-1 ${isExpired ? 'text-gray-600' : 'text-white/70'}`}>horas</span>
        </div>
        <span className={`text-3xl font-bold ${isExpired ? 'text-gray-600' : 'text-white'}`}>:</span>
        <div className="flex flex-col items-center">
          <span className={`text-3xl font-bold ${isExpired ? 'text-gray-600' : 'text-white'}`}>
            {String(timeLeft.minutes).padStart(2, '0')}
          </span>
          <span className={`text-xs mt-1 ${isExpired ? 'text-gray-600' : 'text-white/70'}`}>min</span>
        </div>
        <span className={`text-3xl font-bold ${isExpired ? 'text-gray-600' : 'text-white'}`}>:</span>
        <div className="flex flex-col items-center">
          <span className={`text-3xl font-bold ${isExpired ? 'text-gray-600' : 'text-white'}`}>
            {String(timeLeft.seconds).padStart(2, '0')}
          </span>
          <span className={`text-xs mt-1 ${isExpired ? 'text-gray-600' : 'text-white/70'}`}>seg</span>
        </div>
      </div>
    </div>
  );
}
