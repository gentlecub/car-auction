import { useState } from 'react';
import { Car, Lock, Mail, AlertCircle } from 'lucide-react';
import { mockUsers, User } from '@/app/data/mockData';

interface LoginProps {
  onLogin: (user: User) => void;
  onSkip: () => void;
}

export function Login({ onLogin, onSkip }: LoginProps) {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [showDemo, setShowDemo] = useState(false);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    const user = mockUsers.find(
      (u) => u.email === email && u.password === password
    );

    if (user) {
      onLogin(user);
    } else {
      setError('Invalid credentials. Please try again.');
    }
  };

  const handleDemoLogin = (userEmail: string) => {
    const user = mockUsers.find((u) => u.email === userEmail);
    if (user) {
      onLogin(user);
    }
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-[#1E40AF] to-[#3B82F6] flex items-center justify-center p-4">
      <div className="w-full max-w-md">
        {/* Logo and title */}
        <div className="text-center mb-8">
          <div className="flex items-center justify-center gap-3 mb-4">
            <div className="w-16 h-16 bg-white rounded-2xl flex items-center justify-center shadow-lg">
              <Car className="w-10 h-10 text-[#1E40AF]" />
            </div>
          </div>
          <h1 className="text-4xl font-bold text-white mb-2">AutoAuctions</h1>
          <p className="text-white/80">Sign in to participate in auctions</p>
        </div>

        {/* Login card */}
        <div className="bg-white rounded-2xl shadow-2xl p-8">
          <form onSubmit={handleSubmit} className="space-y-6">
            {/* Email */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Email
              </label>
              <div className="relative">
                <Mail className="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
                <input
                  type="email"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  placeholder="you@email.com"
                  className="w-full pl-11 pr-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-[#1E40AF] focus:border-transparent outline-none"
                  required
                />
              </div>
            </div>

            {/* Password */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Password
              </label>
              <div className="relative">
                <Lock className="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
                <input
                  type="password"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  placeholder="••••••••"
                  className="w-full pl-11 pr-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-[#1E40AF] focus:border-transparent outline-none"
                  required
                />
              </div>
            </div>

            {/* Error message */}
            {error && (
              <div className="flex items-center gap-2 p-3 bg-red-50 border border-red-200 rounded-lg text-red-700">
                <AlertCircle className="w-5 h-5 flex-shrink-0" />
                <p className="text-sm">{error}</p>
              </div>
            )}

            {/* Submit button */}
            <button
              type="submit"
              className="w-full py-3 bg-[#1E40AF] hover:bg-[#1E40AF]/90 text-white font-semibold rounded-lg transition-colors shadow-md"
            >
              Sign In
            </button>
          </form>

          {/* Demo accounts */}
          <div className="mt-6">
            <button
              onClick={() => setShowDemo(!showDemo)}
              className="w-full text-sm text-[#1E40AF] hover:underline"
            >
              {showDemo ? 'Hide' : 'Show'} demo accounts
            </button>

            {showDemo && (
              <div className="mt-4 p-4 bg-[#F9FAFB] rounded-lg space-y-3">
                <p className="text-sm font-medium text-gray-700 mb-2">
                  Test accounts:
                </p>
                {mockUsers.map((user) => (
                  <div
                    key={user.id}
                    className="flex items-center justify-between p-3 bg-white rounded-lg border border-gray-200"
                  >
                    <div className="text-sm">
                      <p className="font-medium text-gray-900">{user.name}</p>
                      <p className="text-gray-600">{user.email}</p>
                      <p className="text-gray-500 text-xs mt-1">
                        Password: {user.password}
                      </p>
                    </div>
                    <button
                      onClick={() => handleDemoLogin(user.email)}
                      className="px-3 py-1.5 bg-[#22C55E] hover:bg-[#22C55E]/90 text-white text-sm font-medium rounded transition-colors"
                    >
                      Use
                    </button>
                  </div>
                ))}
              </div>
            )}
          </div>

          {/* Skip login */}
          <div className="mt-6 pt-6 border-t border-gray-200">
            <button
              onClick={onSkip}
              className="w-full py-3 text-gray-600 hover:text-gray-800 font-medium transition-colors"
            >
              Continue as guest
            </button>
          </div>
        </div>

        {/* Footer info */}
        <div className="mt-6 text-center">
          <p className="text-white/70 text-sm">
            This is a demo application with simulated data
          </p>
        </div>
      </div>
    </div>
  );
}
