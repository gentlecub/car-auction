import { useState } from 'react';
import { Car, mockCars } from '@/app/data/mockData';
import { CarCard } from '@/app/components/CarCard';
import { FilterBar, Filters } from '@/app/components/FilterBar';
import { Pagination } from '@/app/components/Pagination';
import { Search } from 'lucide-react';

interface HomeProps {
  onCarSelect: (car: Car) => void;
}

const ITEMS_PER_PAGE = 10;

export function Home({ onCarSelect }: HomeProps) {
  const [searchQuery, setSearchQuery] = useState('');
  const [currentPage, setCurrentPage] = useState(1);
  const [filters, setFilters] = useState<Filters>({
    brand: '',
    maxPrice: '',
    condition: '',
    sortBy: 'endingSoon',
  });

  const filteredCars = mockCars
    .filter((car) => car.status === 'active')
    .filter((car) => {
      // Search filter
      const query = searchQuery.toLowerCase();
      const matchesSearch = 
        car.brand.toLowerCase().includes(query) ||
        car.model.toLowerCase().includes(query);
      
      if (!matchesSearch) return false;

      // Brand filter
      if (filters.brand && car.brand !== filters.brand) return false;

      // Price filter
      if (filters.maxPrice && car.currentBid > parseInt(filters.maxPrice)) return false;

      // Condition filter
      if (filters.condition && car.condition !== filters.condition) return false;

      return true;
    })
    .sort((a, b) => {
      switch (filters.sortBy) {
        case 'endingSoon':
          return a.endTime.getTime() - b.endTime.getTime();
        case 'priceLow':
          return a.currentBid - b.currentBid;
        case 'priceHigh':
          return b.currentBid - a.currentBid;
        case 'newest':
          return b.year - a.year;
        default:
          return 0;
      }
    });

  // Calculate pagination
  const totalPages = Math.ceil(filteredCars.length / ITEMS_PER_PAGE);
  const startIndex = (currentPage - 1) * ITEMS_PER_PAGE;
  const endIndex = startIndex + ITEMS_PER_PAGE;
  const paginatedCars = filteredCars.slice(startIndex, endIndex);

  const handlePageChange = (page: number) => {
    setCurrentPage(page);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  const handleFilterChange = (newFilters: Filters) => {
    setFilters(newFilters);
    setCurrentPage(1); // Reset to first page when filters change
  };

  return (
    <div className="min-h-screen bg-[#F9FAFB]">
      <div className="container mx-auto px-4 py-8">
        {/* Hero Section */}
        <div className="bg-gradient-to-r from-[#1E40AF] to-[#3B82F6] rounded-2xl p-8 md:p-12 mb-8 text-white">
          <h1 className="text-3xl md:text-5xl font-bold mb-4">
            Online Car Auctions
          </h1>
          <p className="text-lg md:text-xl text-white/90 mb-6">
            Find your next vehicle at the best price. Real-time auctions.
          </p>
          
          {/* Search bar */}
          <div className="max-w-2xl">
            <div className="relative">
              <Search className="absolute left-4 top-1/2 transform -translate-y-1/2 w-5 h-5 text-gray-400" />
              <input
                type="text"
                placeholder="Search by brand or model..."
                value={searchQuery}
                onChange={(e) => {
                  setSearchQuery(e.target.value);
                  setCurrentPage(1); // Reset to first page when searching
                }}
                className="w-full pl-12 pr-4 py-4 rounded-xl text-[#111827] focus:ring-4 focus:ring-white/30 outline-none"
              />
            </div>
          </div>
        </div>

        {/* Filters */}
        <FilterBar onFilterChange={handleFilterChange} />

        {/* Results count and pagination info */}
        <div className="mb-6 flex items-center justify-between flex-wrap gap-2">
          <p className="text-gray-600">
            <span className="font-semibold text-[#111827]">{filteredCars.length}</span> active auction{filteredCars.length !== 1 ? 's' : ''}
            {totalPages > 1 && (
              <span className="ml-2">
                • Page {currentPage} of {totalPages}
              </span>
            )}
          </p>
        </div>

        {/* Car Grid */}
        {paginatedCars.length === 0 ? (
          <div className="text-center py-16">
            <p className="text-gray-500 text-lg">No auctions found with the selected filters</p>
          </div>
        ) : (
          <>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
              {paginatedCars.map((car) => (
                <CarCard key={car.id} car={car} onClick={() => onCarSelect(car)} />
              ))}
            </div>

            {/* Pagination */}
            {totalPages > 1 && (
              <Pagination
                currentPage={currentPage}
                totalPages={totalPages}
                onPageChange={handlePageChange}
              />
            )}
          </>
        )}
      </div>
    </div>
  );
}