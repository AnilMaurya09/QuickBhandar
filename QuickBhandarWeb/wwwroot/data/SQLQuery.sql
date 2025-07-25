create database QuickBhandarDB
use QuickBhandarDB


CREATE TABLE Orders (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    FirstName NVARCHAR(100),
    LastName NVARCHAR(100),
    Email NVARCHAR(150),
    Phone NVARCHAR(20),
    Address NVARCHAR(250),
    City NVARCHAR(100),
    Zip NVARCHAR(20),
    PaymentMethod NVARCHAR(50),
    TotalAmount DECIMAL(18,2),
    Status NVARCHAR(50) DEFAULT 'Pending', -- Pending, Completed, Cancelled
    CreatedAt DATETIME DEFAULT GETDATE(),

    FOREIGN KEY (UserId) REFERENCES Users(Id)
);
CREATE TABLE OrderItems (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL,
    Price DECIMAL(18,2),

    FOREIGN KEY (OrderId) REFERENCES Orders(Id),
    FOREIGN KEY (ProductId) REFERENCES Products(Id)
);

CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    MobileNumber NVARCHAR(15) NOT NULL,
    Role NVARCHAR(20) NOT NULL, -- 'Admin' or 'User'
    OTP NVARCHAR(100) NOT NULL,
    OTPExpiry datetime
);
CREATE TABLE Products (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(150) NOT NULL,
    ImageUrl NVARCHAR(500),
    Price DECIMAL(18,2) NOT NULL,
    Description NVARCHAR(MAX),
    Category NVARCHAR(100),
    Stock INT NOT NULL,
    IsTrending BIT NOT NULL DEFAULT 0,
    IsBestSelling BIT NOT NULL DEFAULT 0,
    IsJustArrived BIT NOT NULL DEFAULT 0,
    IsMostPopular BIT NOT NULL DEFAULT 0
);

CREATE TABLE Cart (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (ProductId) REFERENCES Products(Id)
);
CREATE TABLE Wishlist (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    ProductId INT NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),

    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (ProductId) REFERENCES Products(Id)
);

