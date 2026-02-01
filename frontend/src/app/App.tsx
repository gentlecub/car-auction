import { useState } from 'react';
import { Header } from '@/app/components/Header';
import { Home } from '@/app/views/Home';
import { CarDetail } from '@/app/views/CarDetail';
import { AdminPanel } from '@/app/views/AdminPanel';
import { Login } from '@/app/views/Login';
import { Car, User } from '@/app/data/mockData';

type View = 'login' | 'home' | 'detail' | 'admin';

export default function App() {
  const [currentView, setCurrentView] = useState<View>('login');
  const [selectedCar, setSelectedCar] = useState<Car | null>(null);
  const [user, setUser] = useState<User | null>(null);

  const handleCarSelect = (car: Car) => {
    setSelectedCar(car);
    setCurrentView('detail');
  };

  const handleNavigate = (view: 'home' | 'admin') => {
    setCurrentView(view);
    if (view === 'home') {
      setSelectedCar(null);
    }
  };

  const handleBackToHome = () => {
    setCurrentView('home');
    setSelectedCar(null);
  };

  const handleLogin = (loggedInUser: User) => {
    setUser(loggedInUser);
    setCurrentView('home');
  };

  const handleSkipLogin = () => {
    setUser(null);
    setCurrentView('home');
  };

  const handleLogout = () => {
    setUser(null);
    setCurrentView('login');
    setSelectedCar(null);
  };

  const handleLoginRequired = () => {
    setCurrentView('login');
  };

  if (currentView === 'login') {
    return <Login onLogin={handleLogin} onSkip={handleSkipLogin} />;
  }

  return (
    <div className="min-h-screen">
      <Header 
        currentView={currentView} 
        onNavigate={handleNavigate} 
        user={user}
        onLogout={handleLogout}
      />
      
      {currentView === 'home' && <Home onCarSelect={handleCarSelect} />}
      
      {currentView === 'detail' && selectedCar && (
        <CarDetail 
          car={selectedCar} 
          onBack={handleBackToHome}
          user={user}
          onLoginRequired={handleLoginRequired}
        />
      )}
      
      {currentView === 'admin' && <AdminPanel />}
    </div>
  );
}