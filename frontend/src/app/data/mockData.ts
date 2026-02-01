export interface Car {
  id: number;
  brand: string;
  model: string;
  year: number;
  currentBid: number;
  startPrice: number;
  endTime: Date;
  image: string;
  images: string[];
  mileage: number;
  condition: 'Nuevo' | 'Usado' | 'Certificado';
  description: string;
  features: string[];
  status: 'active' | 'finished';
}

export interface Bid {
  id: number;
  carId: number;
  userName: string;
  amount: number;
  timestamp: Date;
}

export interface User {
  id: number;
  email: string;
  password: string;
  name: string;
}

export const mockUsers: User[] = [
  { id: 1, email: 'admin@autosubastas.com', password: 'admin123', name: 'Administrador' },
  { id: 2, email: 'usuario@ejemplo.com', password: 'usuario123', name: 'Juan Pérez' },
  { id: 3, email: 'maria@ejemplo.com', password: 'maria123', name: 'María García' },
];

export const mockCars: Car[] = [
  {
    id: 1,
    brand: 'Tesla',
    model: 'Model 3',
    year: 2023,
    currentBid: 45000,
    startPrice: 40000,
    endTime: new Date(Date.now() + 2 * 60 * 60 * 1000), // 2 hours
    image: 'https://images.unsplash.com/photo-1560958089-b8a1929cea89?w=800',
    images: [
      'https://images.unsplash.com/photo-1560958089-b8a1929cea89?w=800',
      'https://images.unsplash.com/photo-1617788138017-80ad40651399?w=800',
      'https://images.unsplash.com/photo-1617814076367-b759c7d7e738?w=800',
    ],
    mileage: 15000,
    condition: 'Certificado',
    description: 'Tesla Model 3 en excelente estado, con todas las revisiones al día. Incluye piloto automático y carga rápida.',
    features: ['Piloto Automático', 'Carga Rápida', 'Pantalla Touch 15"', 'Asientos de Cuero'],
    status: 'active',
  },
  {
    id: 2,
    brand: 'BMW',
    model: 'Serie 5',
    year: 2022,
    currentBid: 52000,
    startPrice: 48000,
    endTime: new Date(Date.now() + 5 * 60 * 60 * 1000), // 5 hours
    image: 'https://images.unsplash.com/photo-1555215695-3004980ad54e?w=800',
    images: [
      'https://images.unsplash.com/photo-1555215695-3004980ad54e?w=800',
      'https://images.unsplash.com/photo-1617531653332-bd46c24f2068?w=800',
    ],
    mileage: 22000,
    condition: 'Certificado',
    description: 'BMW Serie 5 con paquete deportivo M, interior de lujo y tecnología de última generación.',
    features: ['Paquete M Sport', 'Techo Panorámico', 'Sistema de Audio Harman Kardon', 'Asientos Ventilados'],
    status: 'active',
  },
  {
    id: 3,
    brand: 'Mercedes-Benz',
    model: 'GLE 350',
    year: 2023,
    currentBid: 68000,
    startPrice: 65000,
    endTime: new Date(Date.now() + 1 * 60 * 60 * 1000), // 1 hour
    image: 'https://images.unsplash.com/photo-1618843479313-40f8afb4b4d8?w=800',
    images: [
      'https://images.unsplash.com/photo-1618843479313-40f8afb4b4d8?w=800',
      'https://images.unsplash.com/photo-1606664515524-ed2f786a0bd6?w=800',
    ],
    mileage: 8000,
    condition: 'Certificado',
    description: 'Mercedes-Benz GLE 350 SUV de lujo con tecnología AMG y máximo confort.',
    features: ['Paquete AMG', 'Asientos Masajeadores', 'Tracción 4MATIC', 'Cámara 360°'],
    status: 'active',
  },
  {
    id: 4,
    brand: 'Audi',
    model: 'A4',
    year: 2021,
    currentBid: 38000,
    startPrice: 35000,
    endTime: new Date(Date.now() + 8 * 60 * 60 * 1000), // 8 hours
    image: 'https://images.unsplash.com/photo-1606664515524-ed2f786a0bd6?w=800',
    images: [
      'https://images.unsplash.com/photo-1606664515524-ed2f786a0bd6?w=800',
    ],
    mileage: 28000,
    condition: 'Usado',
    description: 'Audi A4 en excelente estado, mantenimiento completo y único dueño.',
    features: ['Virtual Cockpit', 'Quattro AWD', 'LED Matrix', 'Asientos Deportivos'],
    status: 'active',
  },
  {
    id: 5,
    brand: 'Porsche',
    model: '911 Carrera',
    year: 2023,
    currentBid: 125000,
    startPrice: 120000,
    endTime: new Date(Date.now() + 12 * 60 * 60 * 1000), // 12 hours
    image: 'https://images.unsplash.com/photo-1503376780353-7e6692767b70?w=800',
    images: [
      'https://images.unsplash.com/photo-1503376780353-7e6692767b70?w=800',
      'https://images.unsplash.com/photo-1614200187524-dc4b892acf16?w=800',
    ],
    mileage: 3000,
    condition: 'Nuevo',
    description: 'Porsche 911 Carrera nuevo, edición limitada con especificaciones deportivas.',
    features: ['Motor Turbo', 'Suspensión Deportiva', 'Sistema PCM', 'Escape Deportivo'],
    status: 'active',
  },
  {
    id: 6,
    brand: 'Ford',
    model: 'Mustang GT',
    year: 2022,
    currentBid: 55000,
    startPrice: 52000,
    endTime: new Date(Date.now() - 2 * 60 * 60 * 1000), // Finished
    image: 'https://images.unsplash.com/photo-1584345604476-8ec5f12d2923?w=800',
    images: [
      'https://images.unsplash.com/photo-1584345604476-8ec5f12d2923?w=800',
    ],
    mileage: 12000,
    condition: 'Certificado',
    description: 'Ford Mustang GT con motor V8, edición especial.',
    features: ['Motor V8 5.0L', 'Performance Pack', 'Sistema SYNC', 'Escape Activo'],
    status: 'finished',
  },
  {
    id: 7,
    brand: 'Toyota',
    model: 'Camry Hybrid',
    year: 2023,
    currentBid: 32000,
    startPrice: 30000,
    endTime: new Date(Date.now() + 6 * 60 * 60 * 1000),
    image: 'https://images.unsplash.com/photo-1621007947382-bb3c3994e3fb?w=800',
    images: ['https://images.unsplash.com/photo-1621007947382-bb3c3994e3fb?w=800'],
    mileage: 5000,
    condition: 'Certificado',
    description: 'Toyota Camry Hybrid con tecnología híbrida avanzada y bajo consumo.',
    features: ['Sistema Híbrido', 'Cámara Reversa', 'Sensores de Parking', 'Bluetooth'],
    status: 'active',
  },
  {
    id: 8,
    brand: 'Chevrolet',
    model: 'Corvette C8',
    year: 2023,
    currentBid: 95000,
    startPrice: 90000,
    endTime: new Date(Date.now() + 3 * 60 * 60 * 1000),
    image: 'https://images.unsplash.com/photo-1552519507-da3b142c6e3d?w=800',
    images: ['https://images.unsplash.com/photo-1552519507-da3b142c6e3d?w=800'],
    mileage: 2000,
    condition: 'Nuevo',
    description: 'Chevrolet Corvette C8, motor central V8, diseño deportivo revolucionario.',
    features: ['Motor V8 6.2L', 'Transmisión de 8 Velocidades', 'Suspensión Magnética', 'Performance Exhaust'],
    status: 'active',
  },
  {
    id: 9,
    brand: 'Lexus',
    model: 'RX 350',
    year: 2022,
    currentBid: 48000,
    startPrice: 45000,
    endTime: new Date(Date.now() + 7 * 60 * 60 * 1000),
    image: 'https://images.unsplash.com/photo-1549317661-bd32c8ce0db2?w=800',
    images: ['https://images.unsplash.com/photo-1549317661-bd32c8ce0db2?w=800'],
    mileage: 18000,
    condition: 'Certificado',
    description: 'Lexus RX 350 SUV de lujo, confiabilidad japonesa con acabados premium.',
    features: ['Asientos de Cuero', 'Sistema Mark Levinson', 'Techo Panorámico', 'Safety Sense 2.0'],
    status: 'active',
  },
  {
    id: 10,
    brand: 'Honda',
    model: 'Accord Sport',
    year: 2023,
    currentBid: 28000,
    startPrice: 26000,
    endTime: new Date(Date.now() + 4 * 60 * 60 * 1000),
    image: 'https://images.unsplash.com/photo-1590362891991-f776e747a588?w=800',
    images: ['https://images.unsplash.com/photo-1590362891991-f776e747a588?w=800'],
    mileage: 8500,
    condition: 'Certificado',
    description: 'Honda Accord Sport, sedán deportivo con excelente rendimiento de combustible.',
    features: ['Turbo Engine', 'Honda Sensing', 'Apple CarPlay', 'Asientos Deportivos'],
    status: 'active',
  },
  {
    id: 11,
    brand: 'Mazda',
    model: 'CX-5 Signature',
    year: 2023,
    currentBid: 36000,
    startPrice: 34000,
    endTime: new Date(Date.now() + 9 * 60 * 60 * 1000),
    image: 'https://images.unsplash.com/photo-1617531653520-bd466c5d2c9e?w=800',
    images: ['https://images.unsplash.com/photo-1617531653520-bd466c5d2c9e?w=800'],
    mileage: 12000,
    condition: 'Certificado',
    description: 'Mazda CX-5 Signature, SUV premium con tecnología Skyactiv.',
    features: ['Skyactiv-G Turbo', 'i-Activsense', 'Bose Audio', 'Asientos Nappa'],
    status: 'active',
  },
  {
    id: 12,
    brand: 'Volkswagen',
    model: 'Tiguan R-Line',
    year: 2022,
    currentBid: 33000,
    startPrice: 31000,
    endTime: new Date(Date.now() + 10 * 60 * 60 * 1000),
    image: 'https://images.unsplash.com/photo-1622353219448-46a5c61a72b6?w=800',
    images: ['https://images.unsplash.com/photo-1622353219448-46a5c61a72b6?w=800'],
    mileage: 20000,
    condition: 'Usado',
    description: 'Volkswagen Tiguan R-Line, SUV compacto con línea deportiva.',
    features: ['Paquete R-Line', 'Digital Cockpit', 'Frenova App-Connect', '4Motion AWD'],
    status: 'active',
  },
  {
    id: 13,
    brand: 'Nissan',
    model: 'GT-R Nismo',
    year: 2023,
    currentBid: 185000,
    startPrice: 180000,
    endTime: new Date(Date.now() + 15 * 60 * 60 * 1000),
    image: 'https://images.unsplash.com/photo-1549399542-7e3f8b79c341?w=800',
    images: ['https://images.unsplash.com/photo-1549399542-7e3f8b79c341?w=800'],
    mileage: 500,
    condition: 'Nuevo',
    description: 'Nissan GT-R Nismo, superdeportivo japonés de edición especial.',
    features: ['Motor V6 Twin-Turbo', 'Aerodinámica Nismo', 'Suspensión Bilstein', 'Frenos Brembo'],
    status: 'active',
  },
  {
    id: 14,
    brand: 'Subaru',
    model: 'WRX STI',
    year: 2022,
    currentBid: 42000,
    startPrice: 40000,
    endTime: new Date(Date.now() + 11 * 60 * 60 * 1000),
    image: 'https://images.unsplash.com/photo-1605559424843-9e4c228bf1c2?w=800',
    images: ['https://images.unsplash.com/photo-1605559424843-9e4c228bf1c2?w=800'],
    mileage: 16000,
    condition: 'Certificado',
    description: 'Subaru WRX STI, deportivo rally con tracción AWD y alto rendimiento.',
    features: ['Turbo Boxer Engine', 'Symmetrical AWD', 'Brembo Brakes', 'Recaro Seats'],
    status: 'active',
  },
  {
    id: 15,
    brand: 'Hyundai',
    model: 'Ioniq 5',
    year: 2023,
    currentBid: 46000,
    startPrice: 44000,
    endTime: new Date(Date.now() + 13 * 60 * 60 * 1000),
    image: 'https://images.unsplash.com/photo-1593941707882-a5bba14938c7?w=800',
    images: ['https://images.unsplash.com/photo-1593941707882-a5bba14938c7?w=800'],
    mileage: 3000,
    condition: 'Nuevo',
    description: 'Hyundai Ioniq 5 eléctrico, diseño futurista con ultra rápida carga.',
    features: ['Motor Eléctrico Dual', 'Carga Ultra Rápida 800V', 'V2L', 'Augmented Reality HUD'],
    status: 'active',
  },
  {
    id: 16,
    brand: 'Kia',
    model: 'Stinger GT',
    year: 2022,
    currentBid: 44000,
    startPrice: 42000,
    endTime: new Date(Date.now() + 14 * 60 * 60 * 1000),
    image: 'https://images.unsplash.com/photo-1542362567-b07e54358753?w=800',
    images: ['https://images.unsplash.com/photo-1542362567-b07e54358753?w=800'],
    mileage: 14000,
    condition: 'Certificado',
    description: 'Kia Stinger GT, Gran Turismo deportivo con diseño europeo.',
    features: ['Motor V6 Twin-Turbo', 'Limited Slip Diff', 'Harman Kardon', 'Adaptive Suspension'],
    status: 'active',
  },
];

export const mockBids: Bid[] = [
  { id: 1, carId: 1, userName: 'Juan Pérez', amount: 45000, timestamp: new Date(Date.now() - 5 * 60 * 1000) },
  { id: 2, carId: 1, userName: 'María García', amount: 43500, timestamp: new Date(Date.now() - 15 * 60 * 1000) },
  { id: 3, carId: 1, userName: 'Carlos López', amount: 42000, timestamp: new Date(Date.now() - 30 * 60 * 1000) },
  { id: 4, carId: 2, userName: 'Ana Martínez', amount: 52000, timestamp: new Date(Date.now() - 10 * 60 * 1000) },
  { id: 5, carId: 2, userName: 'Roberto Silva', amount: 50000, timestamp: new Date(Date.now() - 25 * 60 * 1000) },
  { id: 6, carId: 3, userName: 'Laura Torres', amount: 68000, timestamp: new Date(Date.now() - 8 * 60 * 1000) },
];

export const adminStats = {
  totalAuctions: 6,
  activeAuctions: 5,
  finishedAuctions: 1,
  totalUsers: 245,
  totalRevenue: 425000,
  auctionsThisMonth: [
    { month: 'Ene', auctions: 12 },
    { month: 'Feb', auctions: 19 },
    { month: 'Mar', auctions: 15 },
    { month: 'Abr', auctions: 22 },
    { month: 'May', auctions: 28 },
    { month: 'Jun', auctions: 24 },
  ],
  userGrowth: [
    { month: 'Ene', users: 120 },
    { month: 'Feb', users: 145 },
    { month: 'Mar', users: 167 },
    { month: 'Abr', users: 189 },
    { month: 'May', users: 215 },
    { month: 'Jun', users: 245 },
  ],
};