-- ============================================
-- CarAuction Database - Initial Schema
-- MySQL 8.0+
-- ============================================

-- Roles table
CREATE TABLE IF NOT EXISTS Roles (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(50) NOT NULL UNIQUE,
    Description VARCHAR(255),
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Users table
CREATE TABLE IF NOT EXISTS Users (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Email VARCHAR(255) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    FirstName VARCHAR(100) NOT NULL,
    LastName VARCHAR(100) NOT NULL,
    PhoneNumber VARCHAR(20) NULL,
    Status TINYINT NOT NULL DEFAULT 0 COMMENT '0=Pending, 1=Active, 2=Suspended, 3=Deleted',
    EmailVerified BOOLEAN NOT NULL DEFAULT FALSE,
    EmailVerificationToken VARCHAR(255) NULL,
    EmailVerificationExpires DATETIME NULL,
    PasswordResetToken VARCHAR(255) NULL,
    PasswordResetExpires DATETIME NULL,
    LastLoginAt DATETIME NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NULL,
    INDEX idx_users_email (Email),
    INDEX idx_users_status (Status)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- User Roles (many-to-many)
CREATE TABLE IF NOT EXISTS UserRoles (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL,
    RoleId INT NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (RoleId) REFERENCES Roles(Id) ON DELETE CASCADE,
    UNIQUE KEY uk_user_role (UserId, RoleId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Refresh Tokens
CREATE TABLE IF NOT EXISTS RefreshTokens (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL,
    Token VARCHAR(500) NOT NULL,
    ExpiresAt DATETIME NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    RevokedAt DATETIME NULL,
    ReplacedByToken VARCHAR(500) NULL,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    INDEX idx_refreshtokens_token (Token(255)),
    INDEX idx_refreshtokens_userid (UserId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Cars table
CREATE TABLE IF NOT EXISTS Cars (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Brand VARCHAR(100) NOT NULL,
    Model VARCHAR(100) NOT NULL,
    Year INT NOT NULL,
    Mileage INT NOT NULL DEFAULT 0,
    Color VARCHAR(50) NULL,
    TransmissionType TINYINT NOT NULL DEFAULT 0 COMMENT '0=Manual, 1=Automatic, 2=CVT',
    FuelType TINYINT NOT NULL DEFAULT 0 COMMENT '0=Gasoline, 1=Diesel, 2=Electric, 3=Hybrid',
    EngineSize DECIMAL(3,1) NULL,
    Horsepower INT NULL,
    VIN VARCHAR(17) NULL UNIQUE,
    Description TEXT NULL,
    Features JSON NULL,
    Condition TINYINT NOT NULL DEFAULT 0 COMMENT '0=New, 1=LikeNew, 2=Good, 3=Fair, 4=Poor',
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NULL,
    INDEX idx_cars_brand (Brand),
    INDEX idx_cars_year (Year)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Car Images
CREATE TABLE IF NOT EXISTS CarImages (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    CarId INT NOT NULL,
    ImageUrl VARCHAR(500) NOT NULL,
    ThumbnailUrl VARCHAR(500) NULL,
    IsPrimary BOOLEAN NOT NULL DEFAULT FALSE,
    DisplayOrder INT NOT NULL DEFAULT 0,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (CarId) REFERENCES Cars(Id) ON DELETE CASCADE,
    INDEX idx_carimages_carid (CarId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Auctions table
CREATE TABLE IF NOT EXISTS Auctions (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    CarId INT NOT NULL,
    StartingPrice DECIMAL(18,2) NOT NULL,
    ReservePrice DECIMAL(18,2) NULL,
    CurrentBid DECIMAL(18,2) NOT NULL,
    CurrentBidderId INT NULL,
    MinimumBidIncrement DECIMAL(18,2) NOT NULL DEFAULT 100.00,
    StartTime DATETIME NOT NULL,
    EndTime DATETIME NOT NULL,
    OriginalEndTime DATETIME NOT NULL,
    ExtensionMinutes INT NOT NULL DEFAULT 5,
    ExtensionThresholdMinutes INT NOT NULL DEFAULT 2,
    TotalBids INT NOT NULL DEFAULT 0,
    Status TINYINT NOT NULL DEFAULT 0 COMMENT '0=Pending, 1=Active, 2=Completed, 3=Cancelled',
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NULL,
    FOREIGN KEY (CarId) REFERENCES Cars(Id) ON DELETE RESTRICT,
    FOREIGN KEY (CurrentBidderId) REFERENCES Users(Id) ON DELETE SET NULL,
    INDEX idx_auctions_status (Status),
    INDEX idx_auctions_endtime (EndTime),
    INDEX idx_auctions_carid (CarId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Bids table
CREATE TABLE IF NOT EXISTS Bids (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    AuctionId INT NOT NULL,
    UserId INT NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    IpAddress VARCHAR(45) NULL,
    IsWinningBid BOOLEAN NOT NULL DEFAULT FALSE,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (AuctionId) REFERENCES Auctions(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    INDEX idx_bids_auctionid (AuctionId),
    INDEX idx_bids_userid (UserId),
    INDEX idx_bids_amount (Amount DESC)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Auction History (completed auctions)
CREATE TABLE IF NOT EXISTS AuctionHistories (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    AuctionId INT NOT NULL,
    WinnerId INT NULL,
    FinalPrice DECIMAL(18,2) NULL,
    TotalBids INT NOT NULL DEFAULT 0,
    UniqueParticipants INT NOT NULL DEFAULT 0,
    CompletedAt DATETIME NULL,
    ReserveMet BOOLEAN NOT NULL DEFAULT FALSE,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (AuctionId) REFERENCES Auctions(Id) ON DELETE CASCADE,
    FOREIGN KEY (WinnerId) REFERENCES Users(Id) ON DELETE SET NULL,
    INDEX idx_auctionhistories_auctionid (AuctionId),
    INDEX idx_auctionhistories_winnerid (WinnerId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Notifications
CREATE TABLE IF NOT EXISTS Notifications (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL,
    Type TINYINT NOT NULL COMMENT '0=General, 1=Outbid, 2=AuctionWon, 3=AuctionEnding, 4=AuctionCancelled',
    Title VARCHAR(255) NOT NULL,
    Message TEXT NOT NULL,
    AuctionId INT NULL,
    IsRead BOOLEAN NOT NULL DEFAULT FALSE,
    ReadAt DATETIME NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (AuctionId) REFERENCES Auctions(Id) ON DELETE SET NULL,
    INDEX idx_notifications_userid (UserId),
    INDEX idx_notifications_isread (IsRead),
    INDEX idx_notifications_createdat (CreatedAt DESC)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================
-- Insert default data
-- ============================================

-- Insert default roles
INSERT INTO Roles (Name, Description) VALUES
    ('Admin', 'Administrator with full access'),
    ('User', 'Regular user')
ON DUPLICATE KEY UPDATE Description = VALUES(Description);

-- Insert admin user (password: Admin123!)
INSERT INTO Users (Email, PasswordHash, FirstName, LastName, Status, EmailVerified) VALUES
    ('admin@carauction.com', '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/X4.A1vSVsxsj0smPi', 'Admin', 'User', 1, TRUE)
ON DUPLICATE KEY UPDATE Email = VALUES(Email);

-- Assign admin role to admin user
INSERT INTO UserRoles (UserId, RoleId)
SELECT u.Id, r.Id FROM Users u, Roles r
WHERE u.Email = 'admin@carauction.com' AND r.Name = 'Admin'
ON DUPLICATE KEY UPDATE UserId = VALUES(UserId);
