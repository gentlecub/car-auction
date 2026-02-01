import { adminStats, mockCars } from '@/app/data/mockData';
import { BarChart, Bar, LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';
import { TrendingUp, Users, DollarSign, Plus, CheckCircle, XCircle } from 'lucide-react';
import { useState } from 'react';

export function AdminPanel() {
  const [showAddCarModal, setShowAddCarModal] = useState(false);

  return (
    <div className="min-h-screen bg-[#F9FAFB]">
      <div className="container mx-auto px-4 py-8">
        {/* Header */}
        <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between mb-8 gap-4">
          <div>
            <h1 className="text-3xl font-bold text-[#111827] mb-2">Panel de Administración</h1>
            <p className="text-gray-600">Gestiona subastas y revisa estadísticas</p>
          </div>
          <button
            onClick={() => setShowAddCarModal(true)}
            className="flex items-center gap-2 px-6 py-3 bg-[#22C55E] hover:bg-[#22C55E]/90 text-white font-semibold rounded-lg transition-colors shadow-md"
          >
            <Plus className="w-5 h-5" />
            Agregar Carro
          </button>
        </div>

        {/* Stats Cards */}
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
          <div className="bg-white rounded-xl shadow-md p-6">
            <div className="flex items-center justify-between mb-4">
              <div className="w-12 h-12 bg-[#1E40AF]/10 rounded-lg flex items-center justify-center">
                <TrendingUp className="w-6 h-6 text-[#1E40AF]" />
              </div>
              <span className="text-sm font-medium text-[#22C55E]">+12%</span>
            </div>
            <p className="text-gray-600 text-sm mb-1">Subastas Activas</p>
            <p className="text-3xl font-bold text-[#111827]">{adminStats.activeAuctions}</p>
          </div>

          <div className="bg-white rounded-xl shadow-md p-6">
            <div className="flex items-center justify-between mb-4">
              <div className="w-12 h-12 bg-[#22C55E]/10 rounded-lg flex items-center justify-center">
                <Users className="w-6 h-6 text-[#22C55E]" />
              </div>
              <span className="text-sm font-medium text-[#22C55E]">+18%</span>
            </div>
            <p className="text-gray-600 text-sm mb-1">Usuarios Totales</p>
            <p className="text-3xl font-bold text-[#111827]">{adminStats.totalUsers}</p>
          </div>

          <div className="bg-white rounded-xl shadow-md p-6">
            <div className="flex items-center justify-between mb-4">
              <div className="w-12 h-12 bg-[#1E40AF]/10 rounded-lg flex items-center justify-center">
                <DollarSign className="w-6 h-6 text-[#1E40AF]" />
              </div>
              <span className="text-sm font-medium text-[#22C55E]">+25%</span>
            </div>
            <p className="text-gray-600 text-sm mb-1">Ingresos Totales</p>
            <p className="text-3xl font-bold text-[#111827]">${(adminStats.totalRevenue / 1000).toFixed(0)}k</p>
          </div>

          <div className="bg-white rounded-xl shadow-md p-6">
            <div className="flex items-center justify-between mb-4">
              <div className="w-12 h-12 bg-gray-100 rounded-lg flex items-center justify-center">
                <CheckCircle className="w-6 h-6 text-gray-600" />
              </div>
              <span className="text-sm font-medium text-gray-500">Este mes</span>
            </div>
            <p className="text-gray-600 text-sm mb-1">Subastas Finalizadas</p>
            <p className="text-3xl font-bold text-[#111827]">{adminStats.finishedAuctions}</p>
          </div>
        </div>

        {/* Charts */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-8">
          {/* Auctions Chart */}
          <div className="bg-white rounded-xl shadow-md p-6">
            <h3 className="text-xl font-semibold text-[#111827] mb-6">Subastas por Mes</h3>
            <ResponsiveContainer width="100%" height={300}>
              <BarChart data={adminStats.auctionsThisMonth}>
                <CartesianGrid strokeDasharray="3 3" stroke="#E5E7EB" />
                <XAxis dataKey="month" stroke="#6B7280" />
                <YAxis stroke="#6B7280" />
                <Tooltip 
                  contentStyle={{ 
                    backgroundColor: '#fff', 
                    border: '1px solid #E5E7EB',
                    borderRadius: '8px'
                  }}
                />
                <Bar dataKey="auctions" fill="#1E40AF" radius={[8, 8, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </div>

          {/* Users Chart */}
          <div className="bg-white rounded-xl shadow-md p-6">
            <h3 className="text-xl font-semibold text-[#111827] mb-6">Crecimiento de Usuarios</h3>
            <ResponsiveContainer width="100%" height={300}>
              <LineChart data={adminStats.userGrowth}>
                <CartesianGrid strokeDasharray="3 3" stroke="#E5E7EB" />
                <XAxis dataKey="month" stroke="#6B7280" />
                <YAxis stroke="#6B7280" />
                <Tooltip 
                  contentStyle={{ 
                    backgroundColor: '#fff', 
                    border: '1px solid #E5E7EB',
                    borderRadius: '8px'
                  }}
                />
                <Line 
                  type="monotone" 
                  dataKey="users" 
                  stroke="#22C55E" 
                  strokeWidth={3}
                  dot={{ fill: '#22C55E', r: 5 }}
                />
              </LineChart>
            </ResponsiveContainer>
          </div>
        </div>

        {/* Auctions Table */}
        <div className="bg-white rounded-xl shadow-md overflow-hidden">
          <div className="p-6 border-b border-gray-200">
            <h3 className="text-xl font-semibold text-[#111827]">Todas las Subastas</h3>
          </div>
          
          {/* Desktop Table */}
          <div className="hidden md:block overflow-x-auto">
            <table className="w-full">
              <thead className="bg-[#F9FAFB]">
                <tr>
                  <th className="px-6 py-4 text-left text-sm font-semibold text-gray-600">Vehículo</th>
                  <th className="px-6 py-4 text-left text-sm font-semibold text-gray-600">Año</th>
                  <th className="px-6 py-4 text-left text-sm font-semibold text-gray-600">Estado</th>
                  <th className="px-6 py-4 text-left text-sm font-semibold text-gray-600">Puja Actual</th>
                  <th className="px-6 py-4 text-left text-sm font-semibold text-gray-600">Estado</th>
                  <th className="px-6 py-4 text-left text-sm font-semibold text-gray-600">Acción</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-200">
                {mockCars.map((car) => (
                  <tr key={car.id} className="hover:bg-[#F9FAFB] transition-colors">
                    <td className="px-6 py-4">
                      <div className="flex items-center gap-3">
                        <img 
                          src={car.image} 
                          alt={`${car.brand} ${car.model}`}
                          className="w-12 h-12 rounded-lg object-cover"
                        />
                        <div>
                          <p className="font-medium text-[#111827]">{car.brand} {car.model}</p>
                          <p className="text-sm text-gray-600">{car.mileage.toLocaleString()} km</p>
                        </div>
                      </div>
                    </td>
                    <td className="px-6 py-4 text-gray-700">{car.year}</td>
                    <td className="px-6 py-4">
                      <span className={`px-3 py-1 rounded-full text-xs font-medium ${
                        car.condition === 'Nuevo' 
                          ? 'bg-[#22C55E]/10 text-[#22C55E]' 
                          : car.condition === 'Certificado'
                          ? 'bg-[#1E40AF]/10 text-[#1E40AF]'
                          : 'bg-gray-100 text-gray-700'
                      }`}>
                        {car.condition}
                      </span>
                    </td>
                    <td className="px-6 py-4 font-semibold text-[#1E40AF]">
                      ${car.currentBid.toLocaleString()}
                    </td>
                    <td className="px-6 py-4">
                      {car.status === 'active' ? (
                        <span className="flex items-center gap-1 text-[#22C55E]">
                          <CheckCircle className="w-4 h-4" />
                          Activa
                        </span>
                      ) : (
                        <span className="flex items-center gap-1 text-gray-500">
                          <XCircle className="w-4 h-4" />
                          Finalizada
                        </span>
                      )}
                    </td>
                    <td className="px-6 py-4">
                      <button className="text-[#1E40AF] hover:text-[#1E40AF]/80 font-medium">
                        Ver detalles
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {/* Mobile Cards */}
          <div className="md:hidden divide-y divide-gray-200">
            {mockCars.map((car) => (
              <div key={car.id} className="p-4 hover:bg-[#F9FAFB] transition-colors">
                <div className="flex gap-3 mb-3">
                  <img 
                    src={car.image} 
                    alt={`${car.brand} ${car.model}`}
                    className="w-20 h-20 rounded-lg object-cover"
                  />
                  <div className="flex-1">
                    <p className="font-medium text-[#111827] mb-1">{car.brand} {car.model}</p>
                    <p className="text-sm text-gray-600 mb-2">{car.year} • {car.mileage.toLocaleString()} km</p>
                    <span className={`px-3 py-1 rounded-full text-xs font-medium ${
                      car.condition === 'Nuevo' 
                        ? 'bg-[#22C55E]/10 text-[#22C55E]' 
                        : car.condition === 'Certificado'
                        ? 'bg-[#1E40AF]/10 text-[#1E40AF]'
                        : 'bg-gray-100 text-gray-700'
                    }`}>
                      {car.condition}
                    </span>
                  </div>
                </div>
                <div className="flex items-center justify-between">
                  <div>
                    <p className="text-sm text-gray-600">Puja actual</p>
                    <p className="font-semibold text-[#1E40AF]">${car.currentBid.toLocaleString()}</p>
                  </div>
                  {car.status === 'active' ? (
                    <span className="flex items-center gap-1 text-[#22C55E] text-sm">
                      <CheckCircle className="w-4 h-4" />
                      Activa
                    </span>
                  ) : (
                    <span className="flex items-center gap-1 text-gray-500 text-sm">
                      <XCircle className="w-4 h-4" />
                      Finalizada
                    </span>
                  )}
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>

      {/* Add Car Modal */}
      {showAddCarModal && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-xl max-w-md w-full p-6">
            <h3 className="text-xl font-semibold text-[#111827] mb-4">Agregar Nuevo Carro</h3>
            <p className="text-gray-600 mb-6">
              Esta es una vista de demostración. En producción, aquí se mostraría un formulario completo para agregar un nuevo vehículo.
            </p>
            <button
              onClick={() => setShowAddCarModal(false)}
              className="w-full px-4 py-3 bg-[#1E40AF] hover:bg-[#1E40AF]/90 text-white font-semibold rounded-lg transition-colors"
            >
              Cerrar
            </button>
          </div>
        </div>
      )}
    </div>
  );
}
