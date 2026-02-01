import { useState } from 'react';
import { Car, mockBids, User } from '@/app/data/mockData';
import { CountdownTimer } from '@/app/components/CountdownTimer';
import { BidHistory } from '@/app/components/BidHistory';
import { ArrowLeft, Gauge, Calendar, Award, ChevronLeft, ChevronRight, Lock } from 'lucide-react';
import { motion } from 'motion/react';

interface CarDetailProps {
  car: Car;
  onBack: () => void;
  user: User | null;
  onLoginRequired: () => void;
}

export function CarDetail({ car, onBack, user, onLoginRequired }: CarDetailProps) {
  const [currentImageIndex, setCurrentImageIndex] = useState(0);
  const [bidAmount, setBidAmount] = useState(car.currentBid + 500);
  const [showBidSuccess, setShowBidSuccess] = useState(false);
  
  const carBids = mockBids.filter((bid) => bid.carId === car.id);
  const minBid = car.currentBid + 100;

  const nextImage = () => {
    setCurrentImageIndex((prev) => (prev + 1) % car.images.length);
  };

  const prevImage = () => {
    setCurrentImageIndex((prev) => (prev - 1 + car.images.length) % car.images.length);
  };

  const handleBid = (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!user) {
      onLoginRequired();
      return;
    }
    
    if (bidAmount >= minBid) {
      setShowBidSuccess(true);
      setTimeout(() => setShowBidSuccess(false), 3000);
      setBidAmount(bidAmount + 500);
    }
  };

  return (
    <div className="min-h-screen bg-[#F9FAFB]">
      <div className="container mx-auto px-4 py-8">
        {/* Back button */}
        <button
          onClick={onBack}
          className="flex items-center gap-2 text-[#1E40AF] hover:text-[#1E40AF]/80 mb-6 transition-colors"
        >
          <ArrowLeft className="w-5 h-5" />
          <span className="font-medium">Volver a subastas</span>
        </button>

        <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
          {/* Left Column - Images */}
          <div>
            {/* Main Image */}
            <div className="relative bg-white rounded-xl shadow-md overflow-hidden mb-4">
              <div className="relative h-[400px]">
                <img
                  src={car.images[currentImageIndex]}
                  alt={`${car.brand} ${car.model}`}
                  className="w-full h-full object-cover"
                />
                
                {/* Image navigation */}
                {car.images.length > 1 && (
                  <>
                    <button
                      onClick={prevImage}
                      className="absolute left-4 top-1/2 -translate-y-1/2 w-10 h-10 bg-white/90 hover:bg-white rounded-full flex items-center justify-center shadow-lg transition-all"
                    >
                      <ChevronLeft className="w-6 h-6 text-[#111827]" />
                    </button>
                    <button
                      onClick={nextImage}
                      className="absolute right-4 top-1/2 -translate-y-1/2 w-10 h-10 bg-white/90 hover:bg-white rounded-full flex items-center justify-center shadow-lg transition-all"
                    >
                      <ChevronRight className="w-6 h-6 text-[#111827]" />
                    </button>
                    <div className="absolute bottom-4 left-1/2 -translate-x-1/2 flex gap-2">
                      {car.images.map((_, index) => (
                        <button
                          key={index}
                          onClick={() => setCurrentImageIndex(index)}
                          className={`w-2 h-2 rounded-full transition-all ${
                            index === currentImageIndex 
                              ? 'bg-white w-8' 
                              : 'bg-white/50'
                          }`}
                        />
                      ))}
                    </div>
                  </>
                )}
              </div>
            </div>

            {/* Thumbnail gallery */}
            {car.images.length > 1 && (
              <div className="grid grid-cols-3 gap-4">
                {car.images.map((image, index) => (
                  <button
                    key={index}
                    onClick={() => setCurrentImageIndex(index)}
                    className={`relative h-24 rounded-lg overflow-hidden border-2 transition-all ${
                      index === currentImageIndex 
                        ? 'border-[#1E40AF] scale-105' 
                        : 'border-transparent hover:border-gray-300'
                    }`}
                  >
                    <img
                      src={image}
                      alt={`${car.brand} ${car.model} - ${index + 1}`}
                      className="w-full h-full object-cover"
                    />
                  </button>
                ))}
              </div>
            )}

            {/* Car info cards - mobile */}
            <div className="lg:hidden mt-6 grid grid-cols-3 gap-4">
              <div className="bg-white rounded-lg p-4 text-center shadow-md">
                <Calendar className="w-6 h-6 text-[#1E40AF] mx-auto mb-2" />
                <p className="text-sm text-gray-600">Año</p>
                <p className="font-semibold text-[#111827]">{car.year}</p>
              </div>
              <div className="bg-white rounded-lg p-4 text-center shadow-md">
                <Gauge className="w-6 h-6 text-[#1E40AF] mx-auto mb-2" />
                <p className="text-sm text-gray-600">Kilometraje</p>
                <p className="font-semibold text-[#111827]">{car.mileage.toLocaleString()}</p>
              </div>
              <div className="bg-white rounded-lg p-4 text-center shadow-md">
                <Award className="w-6 h-6 text-[#1E40AF] mx-auto mb-2" />
                <p className="text-sm text-gray-600">Estado</p>
                <p className="font-semibold text-[#111827]">{car.condition}</p>
              </div>
            </div>
          </div>

          {/* Right Column - Details and Bidding */}
          <div className="space-y-6">
            {/* Title and condition */}
            <div className="bg-white rounded-xl shadow-md p-6">
              <div className="flex items-start justify-between mb-4">
                <div>
                  <h1 className="text-3xl font-bold text-[#111827] mb-2">
                    {car.brand} {car.model}
                  </h1>
                  <div className="flex items-center gap-4 text-gray-600">
                    <span className="flex items-center gap-1">
                      <Calendar className="w-4 h-4" />
                      {car.year}
                    </span>
                    <span className="flex items-center gap-1">
                      <Gauge className="w-4 h-4" />
                      {car.mileage.toLocaleString()} km
                    </span>
                  </div>
                </div>
                <span className={`px-4 py-2 rounded-lg text-sm font-medium ${
                  car.condition === 'Nuevo' 
                    ? 'bg-[#22C55E] text-white' 
                    : car.condition === 'Certificado'
                    ? 'bg-[#1E40AF] text-white'
                    : 'bg-gray-600 text-white'
                }`}>
                  {car.condition}
                </span>
              </div>

              <p className="text-gray-700 leading-relaxed">{car.description}</p>
            </div>

            {/* Price and countdown */}
            <div className="bg-white rounded-xl shadow-md p-6">
              <div className="flex items-center justify-between mb-6">
                <div>
                  <p className="text-sm text-gray-600 mb-1">Puja actual</p>
                  <p className="text-4xl font-bold text-[#1E40AF]">
                    ${car.currentBid.toLocaleString()}
                  </p>
                  <p className="text-sm text-gray-600 mt-1">
                    Precio inicial: ${car.startPrice.toLocaleString()}
                  </p>
                </div>
                <CountdownTimer endTime={car.endTime} size="large" />
              </div>

              {/* Bid form */}
              {!user ? (
                <div className="p-6 bg-gray-50 border-2 border-gray-200 rounded-lg text-center">
                  <Lock className="w-12 h-12 text-gray-400 mx-auto mb-3" />
                  <p className="text-gray-700 font-medium mb-3">
                    Debes iniciar sesión para realizar pujas
                  </p>
                  <button
                    onClick={onLoginRequired}
                    className="px-6 py-3 bg-[#1E40AF] hover:bg-[#1E40AF]/90 text-white font-semibold rounded-lg transition-colors"
                  >
                    Iniciar Sesión
                  </button>
                </div>
              ) : (
                <form onSubmit={handleBid} className="space-y-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Tu puja (mínimo ${minBid.toLocaleString()})
                    </label>
                    <div className="relative">
                      <span className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-600 font-medium">$</span>
                      <input
                        type="number"
                        value={bidAmount}
                        onChange={(e) => setBidAmount(Number(e.target.value))}
                        min={minBid}
                        step="100"
                        className="w-full pl-8 pr-4 py-3 border-2 border-gray-300 rounded-lg focus:border-[#1E40AF] focus:ring-2 focus:ring-[#1E40AF]/20 outline-none"
                      />
                    </div>
                  </div>
                  <button
                    type="submit"
                    disabled={bidAmount < minBid}
                    className="w-full py-4 bg-[#22C55E] hover:bg-[#22C55E]/90 disabled:bg-gray-300 disabled:cursor-not-allowed text-white font-semibold rounded-lg transition-colors"
                  >
                    Realizar Puja
                  </button>
                </form>
              )}

              {/* Success message */}
              {showBidSuccess && (
                <motion.div
                  initial={{ opacity: 0, y: -10 }}
                  animate={{ opacity: 1, y: 0 }}
                  className="mt-4 p-4 bg-[#22C55E]/10 border-2 border-[#22C55E] rounded-lg"
                >
                  <p className="text-[#22C55E] font-medium text-center">
                    ¡Puja realizada con éxito!
                  </p>
                </motion.div>
              )}
            </div>

            {/* Features */}
            <div className="bg-white rounded-xl shadow-md p-6">
              <h3 className="text-xl font-semibold text-[#111827] mb-4">Características</h3>
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                {car.features.map((feature, index) => (
                  <div key={index} className="flex items-center gap-2">
                    <div className="w-2 h-2 rounded-full bg-[#22C55E]" />
                    <span className="text-gray-700">{feature}</span>
                  </div>
                ))}
              </div>
            </div>
          </div>
        </div>

        {/* Bid History */}
        <div className="mt-8">
          <BidHistory bids={carBids} />
        </div>
      </div>
    </div>
  );
}