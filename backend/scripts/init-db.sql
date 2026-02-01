-- ============================================
-- CarAuction Database Initialization Script
-- Runs on first MySQL container startup
-- ============================================

-- Use the carauction database
USE carauction;

-- ============================================
-- Insert default roles
-- ============================================
INSERT INTO Roles (Id, Name, Description, CreatedAt)
VALUES
    (1, 'Admin', 'Administrador del sistema con acceso completo', UTC_TIMESTAMP()),
    (2, 'User', 'Usuario estándar del sistema', UTC_TIMESTAMP())
ON DUPLICATE KEY UPDATE
    Name = VALUES(Name),
    Description = VALUES(Description);

-- ============================================
-- Insert default admin user
-- Email: admin@carauction.com
-- Password: Admin123! (BCrypt hash with cost 12)
-- ============================================
INSERT INTO Users (
    Id,
    Email,
    PasswordHash,
    FirstName,
    LastName,
    PhoneNumber,
    Status,
    EmailVerified,
    CreatedAt
)
VALUES (
    1,
    'admin@carauction.com',
    '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/X4.4HqoQz4L4XK/Oi',
    'Admin',
    'System',
    NULL,
    1,
    1,
    UTC_TIMESTAMP()
)
ON DUPLICATE KEY UPDATE
    Email = VALUES(Email);

-- ============================================
-- Assign admin role to admin user
-- ============================================
INSERT INTO UserRoles (UserId, RoleId)
VALUES (1, 1)
ON DUPLICATE KEY UPDATE
    RoleId = VALUES(RoleId);

-- ============================================
-- Insert sample test user (for development)
-- Email: user@carauction.com
-- Password: User123! (BCrypt hash with cost 12)
-- ============================================
INSERT INTO Users (
    Id,
    Email,
    PasswordHash,
    FirstName,
    LastName,
    PhoneNumber,
    Status,
    EmailVerified,
    CreatedAt
)
VALUES (
    2,
    'user@carauction.com',
    '$2a$12$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi',
    'Usuario',
    'Demo',
    '+52 555 123 4567',
    1,
    1,
    UTC_TIMESTAMP()
)
ON DUPLICATE KEY UPDATE
    Email = VALUES(Email);

-- Assign user role to test user
INSERT INTO UserRoles (UserId, RoleId)
VALUES (2, 2)
ON DUPLICATE KEY UPDATE
    RoleId = VALUES(RoleId);

-- ============================================
-- Sample Cars (for development/testing)
-- ============================================
INSERT INTO Cars (
    Id, Brand, Model, Year, VIN, Mileage, Color,
    EngineType, Transmission, FuelType, Horsepower,
    Description, `Condition`, Features, IsActive, CreatedAt
)
VALUES
(
    1,
    'Toyota',
    'Corolla',
    2022,
    '1HGBH41JXMN109186',
    25000,
    'Blanco Perla',
    '2.0L 4-Cylinder',
    'Automático CVT',
    'Gasolina',
    169,
    'Toyota Corolla 2022 en excelente estado. Un solo dueño, servicio de agencia completo. Interior impecable, sin accidentes.',
    'Excelente',
    '["Aire acondicionado", "Bluetooth", "Cámara de reversa", "Apple CarPlay", "Android Auto", "Control crucero", "Sensores de estacionamiento"]',
    1,
    UTC_TIMESTAMP()
),
(
    2,
    'Honda',
    'Civic',
    2023,
    '2HGFC2F59NH123456',
    12000,
    'Negro Cristal',
    '1.5L Turbo',
    'Automático CVT',
    'Gasolina',
    180,
    'Honda Civic Touring 2023. Totalmente equipado con paquete de seguridad Honda Sensing. Excelente rendimiento de combustible.',
    'Excelente',
    '["Aire acondicionado dual", "Bluetooth", "Cámara 360°", "Honda Sensing", "Asientos de piel", "Techo solar", "Sistema de navegación"]',
    1,
    UTC_TIMESTAMP()
),
(
    3,
    'BMW',
    '330i',
    2021,
    'WBA5R1C55LA123789',
    35000,
    'Azul Melbourne',
    '2.0L TwinPower Turbo',
    'Automático 8 velocidades',
    'Gasolina',
    255,
    'BMW 330i M Sport 2021. Paquete M Sport completo, asientos deportivos, llantas 19". Mantenimiento al día en agencia BMW.',
    'Muy Bueno',
    '["M Sport Package", "Asientos deportivos", "Head-up display", "Harman Kardon", "Navegación profesional", "Parking assistant", "Live Cockpit Professional"]',
    1,
    UTC_TIMESTAMP()
)
ON DUPLICATE KEY UPDATE
    Brand = VALUES(Brand);

-- ============================================
-- Sample Car Images
-- ============================================
INSERT INTO CarImages (Id, CarId, ImageUrl, ThumbnailUrl, IsPrimary, DisplayOrder, CreatedAt)
VALUES
    (1, 1, 'https://images.unsplash.com/photo-1621007947382-bb3c3994e3fb?w=800', 'https://images.unsplash.com/photo-1621007947382-bb3c3994e3fb?w=200', 1, 0, UTC_TIMESTAMP()),
    (2, 1, 'https://images.unsplash.com/photo-1619682817481-e994891cd1f5?w=800', 'https://images.unsplash.com/photo-1619682817481-e994891cd1f5?w=200', 0, 1, UTC_TIMESTAMP()),
    (3, 2, 'https://images.unsplash.com/photo-1606664515524-ed2f786a0bd6?w=800', 'https://images.unsplash.com/photo-1606664515524-ed2f786a0bd6?w=200', 1, 0, UTC_TIMESTAMP()),
    (4, 2, 'https://images.unsplash.com/photo-1609521263047-f8f205293f24?w=800', 'https://images.unsplash.com/photo-1609521263047-f8f205293f24?w=200', 0, 1, UTC_TIMESTAMP()),
    (5, 3, 'https://images.unsplash.com/photo-1555215695-3004980ad54e?w=800', 'https://images.unsplash.com/photo-1555215695-3004980ad54e?w=200', 1, 0, UTC_TIMESTAMP()),
    (6, 3, 'https://images.unsplash.com/photo-1617531653332-bd46c24f2068?w=800', 'https://images.unsplash.com/photo-1617531653332-bd46c24f2068?w=200', 0, 1, UTC_TIMESTAMP())
ON DUPLICATE KEY UPDATE
    ImageUrl = VALUES(ImageUrl);

-- ============================================
-- Sample Auctions (Active)
-- ============================================
INSERT INTO Auctions (
    Id, CarId, StartingPrice, ReservePrice, MinimumBidIncrement,
    CurrentBid, CurrentBidderId, StartTime, EndTime, OriginalEndTime,
    ExtensionMinutes, ExtensionThresholdMinutes, TotalBids, Status, CreatedAt
)
VALUES
(
    1,
    1,
    180000.00,
    220000.00,
    1000.00,
    180000.00,
    NULL,
    DATE_SUB(UTC_TIMESTAMP(), INTERVAL 2 DAY),
    DATE_ADD(UTC_TIMESTAMP(), INTERVAL 5 DAY),
    DATE_ADD(UTC_TIMESTAMP(), INTERVAL 5 DAY),
    5,
    2,
    0,
    1,
    DATE_SUB(UTC_TIMESTAMP(), INTERVAL 3 DAY)
),
(
    2,
    2,
    280000.00,
    320000.00,
    2000.00,
    280000.00,
    NULL,
    DATE_SUB(UTC_TIMESTAMP(), INTERVAL 1 DAY),
    DATE_ADD(UTC_TIMESTAMP(), INTERVAL 7 DAY),
    DATE_ADD(UTC_TIMESTAMP(), INTERVAL 7 DAY),
    5,
    2,
    0,
    1,
    DATE_SUB(UTC_TIMESTAMP(), INTERVAL 2 DAY)
),
(
    3,
    3,
    450000.00,
    520000.00,
    5000.00,
    450000.00,
    NULL,
    UTC_TIMESTAMP(),
    DATE_ADD(UTC_TIMESTAMP(), INTERVAL 10 DAY),
    DATE_ADD(UTC_TIMESTAMP(), INTERVAL 10 DAY),
    5,
    2,
    0,
    1,
    DATE_SUB(UTC_TIMESTAMP(), INTERVAL 1 DAY)
)
ON DUPLICATE KEY UPDATE
    CarId = VALUES(CarId);

-- ============================================
-- Verification
-- ============================================
SELECT 'Database initialization completed successfully!' AS Status;
SELECT COUNT(*) AS RolesCount FROM Roles;
SELECT COUNT(*) AS UsersCount FROM Users;
SELECT COUNT(*) AS CarsCount FROM Cars;
SELECT COUNT(*) AS AuctionsCount FROM Auctions;
