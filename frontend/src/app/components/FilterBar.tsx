import { SlidersHorizontal, X } from 'lucide-react';
import { useState } from 'react';

interface FilterBarProps {
  onFilterChange: (filters: Filters) => void;
}

export interface Filters {
  brand: string;
  maxPrice: string;
  condition: string;
  sortBy: string;
}

export function FilterBar({ onFilterChange }: FilterBarProps) {
  const [showMobileFilters, setShowMobileFilters] = useState(false);
  const [filters, setFilters] = useState<Filters>({
    brand: '',
    maxPrice: '',
    condition: '',
    sortBy: 'endingSoon',
  });

  const handleFilterChange = (key: keyof Filters, value: string) => {
    const newFilters = { ...filters, [key]: value };
    setFilters(newFilters);
    onFilterChange(newFilters);
  };

  const resetFilters = () => {
    const defaultFilters: Filters = {
      brand: '',
      maxPrice: '',
      condition: '',
      sortBy: 'endingSoon',
    };
    setFilters(defaultFilters);
    onFilterChange(defaultFilters);
  };

  const FilterContent = () => (
    <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
      {/* Brand */}
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-2">Brand</label>
        <select
          value={filters.brand}
          onChange={(e) => handleFilterChange('brand', e.target.value)}
          className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-[#1E40AF] focus:border-transparent bg-white"
        >
          <option value="">All brands</option>
          <option value="Tesla">Tesla</option>
          <option value="BMW">BMW</option>
          <option value="Mercedes-Benz">Mercedes-Benz</option>
          <option value="Audi">Audi</option>
          <option value="Porsche">Porsche</option>
          <option value="Ford">Ford</option>
        </select>
      </div>

      {/* Max Price */}
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-2">Max price</label>
        <select
          value={filters.maxPrice}
          onChange={(e) => handleFilterChange('maxPrice', e.target.value)}
          className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-[#1E40AF] focus:border-transparent bg-white"
        >
          <option value="">No limit</option>
          <option value="50000">Up to $50,000</option>
          <option value="75000">Up to $75,000</option>
          <option value="100000">Up to $100,000</option>
          <option value="150000">Up to $150,000</option>
        </select>
      </div>

      {/* Condition */}
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-2">Condition</label>
        <select
          value={filters.condition}
          onChange={(e) => handleFilterChange('condition', e.target.value)}
          className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-[#1E40AF] focus:border-transparent bg-white"
        >
          <option value="">All</option>
          <option value="New">New</option>
          <option value="Certified">Certified</option>
          <option value="Used">Used</option>
        </select>
      </div>

      {/* Sort */}
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-2">Sort by</label>
        <select
          value={filters.sortBy}
          onChange={(e) => handleFilterChange('sortBy', e.target.value)}
          className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-[#1E40AF] focus:border-transparent bg-white"
        >
          <option value="endingSoon">Ending soon</option>
          <option value="priceLow">Price: Low to High</option>
          <option value="priceHigh">Price: High to Low</option>
          <option value="newest">Newest</option>
        </select>
      </div>
    </div>
  );

  return (
    <div className="bg-white rounded-xl shadow-md p-4 md:p-6 mb-6">
      {/* Mobile filter toggle */}
      <div className="md:hidden">
        <button
          onClick={() => setShowMobileFilters(!showMobileFilters)}
          className="w-full flex items-center justify-between px-4 py-3 bg-[#F9FAFB] rounded-lg"
        >
          <span className="flex items-center gap-2 font-medium text-[#111827]">
            <SlidersHorizontal className="w-5 h-5" />
            Filters
          </span>
          {showMobileFilters ? <X className="w-5 h-5" /> : <SlidersHorizontal className="w-5 h-5" />}
        </button>
        {showMobileFilters && (
          <div className="mt-4">
            <FilterContent />
            <button
              onClick={resetFilters}
              className="mt-4 w-full px-4 py-2 text-sm text-[#EF4444] hover:bg-red-50 rounded-lg transition-colors"
            >
              Clear filters
            </button>
          </div>
        )}
      </div>

      {/* Desktop filters */}
      <div className="hidden md:block">
        <div className="flex items-center justify-between mb-4">
          <div className="flex items-center gap-2">
            <SlidersHorizontal className="w-5 h-5 text-[#111827]" />
            <h3 className="font-semibold text-[#111827]">Filtros</h3>
          </div>
          <button
            onClick={resetFilters}
            className="text-sm text-[#EF4444] hover:underline"
          >
            Clear filters
          </button>
        </div>
        <FilterContent />
      </div>
    </div>
  );
}
