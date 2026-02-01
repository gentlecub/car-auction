import { Car, LayoutDashboard, Menu, X, User, LogOut } from 'lucide-react';
import { useState } from 'react';
import { User as UserType } from '@/app/data/mockData';

interface HeaderProps {
  currentView: 'home' | 'detail' | 'admin';
  onNavigate: (view: 'home' | 'admin') => void;
  user: UserType | null;
  onLogout: () => void;
}

export function Header({ currentView, onNavigate, user, onLogout }: HeaderProps) {
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  const [showUserMenu, setShowUserMenu] = useState(false);

  return (
    <header className="bg-[#111827] text-white sticky top-0 z-50 shadow-lg">
      <div className="container mx-auto px-4 py-4">
        <div className="flex items-center justify-between">
          {/* Logo */}
          <button
            onClick={() => onNavigate('home')}
            className="flex items-center gap-2 hover:opacity-80 transition-opacity"
          >
            <Car className="w-8 h-8 text-[#22C55E]" />
            <span className="text-xl font-bold">AutoAuctions</span>
          </button>

          {/* Desktop Navigation */}
          <div className="hidden md:flex items-center gap-6">
            <button
              onClick={() => onNavigate('home')}
              className={`px-4 py-2 rounded-lg transition-colors ${
                currentView === 'home' || currentView === 'detail'
                  ? 'bg-[#1E40AF] text-white'
                  : 'text-gray-300 hover:text-white'
              }`}
            >
              Auctions
            </button>
            <button
              onClick={() => onNavigate('admin')}
              className={`px-4 py-2 rounded-lg transition-colors flex items-center gap-2 ${
                currentView === 'admin'
                  ? 'bg-[#1E40AF] text-white'
                  : 'text-gray-300 hover:text-white'
              }`}
            >
              <LayoutDashboard className="w-4 h-4" />
              Admin Panel
            </button>

            {/* User menu */}
            {user ? (
              <div className="relative">
                <button
                  onClick={() => setShowUserMenu(!showUserMenu)}
                  className="flex items-center gap-2 px-4 py-2 bg-[#22C55E] hover:bg-[#22C55E]/90 rounded-lg transition-colors"
                >
                  <User className="w-4 h-4" />
                  <span>{user.name}</span>
                </button>
                {showUserMenu && (
                  <div className="absolute right-0 mt-2 w-48 bg-white rounded-lg shadow-xl py-2 border border-gray-200">
                    <button
                      onClick={() => {
                        onLogout();
                        setShowUserMenu(false);
                      }}
                      className="w-full px-4 py-2 text-left text-gray-700 hover:bg-gray-100 flex items-center gap-2"
                    >
                      <LogOut className="w-4 h-4" />
                      Log Out
                    </button>
                  </div>
                )}
              </div>
            ) : (
              <div className="flex items-center gap-2 px-4 py-2 bg-gray-700 rounded-lg">
                <User className="w-4 h-4" />
                <span className="text-sm">Guest</span>
              </div>
            )}
          </div>

          {/* Mobile Menu Button */}
          <button
            className="md:hidden text-white"
            onClick={() => setMobileMenuOpen(!mobileMenuOpen)}
          >
            {mobileMenuOpen ? <X className="w-6 h-6" /> : <Menu className="w-6 h-6" />}
          </button>
        </div>

        {/* Mobile Navigation */}
        {mobileMenuOpen && (
          <nav className="md:hidden mt-4 space-y-2 pb-2">
            {/* User info mobile */}
            {user ? (
              <div className="p-4 bg-[#22C55E] rounded-lg mb-2">
                <div className="flex items-center gap-2 mb-2">
                  <User className="w-5 h-5" />
                  <span className="font-medium">{user.name}</span>
                </div>
                <button
                  onClick={() => {
                    onLogout();
                    setMobileMenuOpen(false);
                  }}
                  className="w-full px-3 py-2 bg-white/20 hover:bg-white/30 rounded text-sm flex items-center gap-2 transition-colors"
                >
                  <LogOut className="w-4 h-4" />
                  Log Out
                </button>
              </div>
            ) : (
              <div className="p-4 bg-gray-700 rounded-lg mb-2 flex items-center gap-2">
                <User className="w-5 h-5" />
                <span>Guest</span>
              </div>
            )}

            <button
              onClick={() => {
                onNavigate('home');
                setMobileMenuOpen(false);
              }}
              className={`w-full px-4 py-3 rounded-lg transition-colors text-left ${
                currentView === 'home' || currentView === 'detail'
                  ? 'bg-[#1E40AF] text-white'
                  : 'text-gray-300 hover:bg-gray-800'
              }`}
            >
              Auctions
            </button>
            <button
              onClick={() => {
                onNavigate('admin');
                setMobileMenuOpen(false);
              }}
              className={`w-full px-4 py-3 rounded-lg transition-colors text-left flex items-center gap-2 ${
                currentView === 'admin'
                  ? 'bg-[#1E40AF] text-white'
                  : 'text-gray-300 hover:bg-gray-800'
              }`}
            >
              <LayoutDashboard className="w-4 h-4" />
              Admin Panel
            </button>
          </nav>
        )}
      </div>
    </header>
  );
}