CREATE DATABASE CarrentalsystemDB;
GO

USE CarrentalsystemDB;
GO

---------------------------------------------------
-- USERS TABLE
---------------------------------------------------

CREATE TABLE Users(
 Id INT IDENTITY PRIMARY KEY,

 FullName NVARCHAR(100) NOT NULL,

 Email NVARCHAR(100) NOT NULL UNIQUE,

 Password NVARCHAR(500) NOT NULL,

 Role NVARCHAR(20) NOT NULL DEFAULT 'User',

 IsBlocked BIT NOT NULL DEFAULT 0,

 IsVerified BIT NOT NULL DEFAULT 0,

 PhoneNumber NVARCHAR(20),

 Address NVARCHAR(300),

 City NVARCHAR(100),

 State NVARCHAR(100),

 Pincode NVARCHAR(10),

 DrivingLicenseNumber NVARCHAR(100) UNIQUE,

 DateOfBirth DATE,

 ProfileImage VARBINARY(MAX),

 LastLogin DATETIME NULL,

 CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),

 ModifiedAt DATETIME NULL
);
GO


---------------------------------------------------
-- CARS TABLE
---------------------------------------------------
CREATE TABLE Cars(
 Id INT IDENTITY PRIMARY KEY,

 CarName NVARCHAR(100) NOT NULL,

 CarModel NVARCHAR(100) NOT NULL,

 CarType NVARCHAR(50),

 ManufacturingYear INT,

 RegistrationNumber NVARCHAR(30) UNIQUE,

 SeatingCapacity INT,

 Transmission NVARCHAR(50) NOT NULL,

 FuelType NVARCHAR(50) NOT NULL,

 VehicleStatus NVARCHAR(30),

 PricePerDay DECIMAL(18,2) NOT NULL CHECK(PricePerDay>0),

 CarImage VARBINARY(MAX),

 Available BIT NOT NULL DEFAULT 1,

 PickupLocation NVARCHAR(200),

 Address NVARCHAR(500),

 Landmark NVARCHAR(200),

 Latitude DECIMAL(10,7),

 Longitude DECIMAL(10,7),

 ModifiedAt DATETIME NULL
);
GO

---------------------------------------------------
-- BOOKINGS TABLE
---------------------------------------------------
CREATE TABLE Bookings(
 Id INT IDENTITY PRIMARY KEY,

 UserId INT NOT NULL,

 CarId INT NOT NULL,

 StartDate DATE NOT NULL,

 EndDate DATE NOT NULL,

 TotalDays INT NOT NULL,

 TotalAmount DECIMAL(18,2) NOT NULL,

 BookingDate DATETIME NOT NULL DEFAULT GETDATE(),

 Status NVARCHAR(50) NOT NULL DEFAULT 'Pending',

 PaymentStatus NVARCHAR(30) NOT NULL DEFAULT 'Pending',

 PaymentMethod NVARCHAR(50) NULL,

 CancellationReason NVARCHAR(500) NULL,

 PickupLocation NVARCHAR(200),

 Address NVARCHAR(500),

 Landmark NVARCHAR(200),

 Latitude DECIMAL(10,7),

 Longitude DECIMAL(10,7),

 ReturnLocation NVARCHAR(200),

 ActualReturnDate DATETIME,

 CONSTRAINT CK_BookingDates CHECK(EndDate>=StartDate),

 CONSTRAINT FK_Bookings_Users FOREIGN KEY(UserId) REFERENCES Users(Id),

 CONSTRAINT FK_Bookings_Cars FOREIGN KEY(CarId) REFERENCES Cars(Id)
);
GO
---------------------------------------------------
-- USER VERIFICATIONS TABLE
---------------------------------------------------
CREATE TABLE UserVerifications(
 Id INT IDENTITY PRIMARY KEY,

 UserId INT NOT NULL,

 GovernmentId VARBINARY(MAX) NOT NULL,

 LicenseFront VARBINARY(MAX) NOT NULL,

 LicenseBack VARBINARY(MAX) NOT NULL,

 DigitalSignature VARBINARY(MAX) NOT NULL,

 Status NVARCHAR(50) NOT NULL DEFAULT 'Pending',

 Remarks NVARCHAR(500),

 UploadDate DATETIME NOT NULL DEFAULT GETDATE(),

 VerifiedDate DATETIME,

 ApprovedBy INT NULL,

 ModifiedAt DATETIME NULL,

 CONSTRAINT FK_UserVerifications_Users FOREIGN KEY(UserId) REFERENCES Users(Id)
);
GO

---------------------------------------------------
-- DEFAULT ADMIN
---------------------------------------------------

INSERT INTO Users
(
    FullName,
    Email,
    Password,
    Role,
    IsBlocked,
    IsVerified
)
VALUES
(
    'Admin',
    'admin@elitecar.com',
    '123456',
    'Admin',
    0,
    1
);
GO

/* ===========================================
    STORED PROCEDURES FILE
   =========================================== */

/* ===========================================
   USER MODULE
   =========================================== */

CREATE PROCEDURE sp_RegisterUser
(
    @FullName NVARCHAR(100),
    @Email NVARCHAR(100),
    @Password NVARCHAR(100),
    @Role NVARCHAR(20)
)
AS
BEGIN
    INSERT INTO Users
    (
        FullName,
        Email,
        Password,
        Role
    )
    VALUES
    (
        @FullName,
        @Email,
        @Password,
        @Role
    )
END
GO

CREATE PROCEDURE sp_Login
(
    @Email NVARCHAR(100),
    @Password NVARCHAR(100)
)
AS
BEGIN
    SELECT *
    FROM Users
    WHERE Email = @Email
    AND Password = @Password
END
GO

CREATE PROCEDURE sp_GetUsers
AS
BEGIN
    SELECT *
    FROM Users
    WHERE Role = 'User'
END
GO

CREATE PROCEDURE sp_BlockUser
(
    @Id INT
)
AS
BEGIN
    UPDATE Users
    SET IsBlocked = 1
    WHERE Id = @Id
END
GO

CREATE PROCEDURE sp_UnblockUser
(
    @Id INT
)
AS
BEGIN
    UPDATE Users
    SET IsBlocked = 0
    WHERE Id = @Id
END
GO

CREATE PROCEDURE sp_DeleteUser
(
    @Id INT
)
AS
BEGIN

    DELETE FROM Bookings
    WHERE UserId = @Id

    DELETE FROM UserVerifications
    WHERE UserId = @Id

    DELETE FROM Users
    WHERE Id = @Id

END
GO

/* ===========================================
   CAR MODULE
   =========================================== */

CREATE PROCEDURE sp_AddCar
(
    @CarName NVARCHAR(100),
    @CarModel NVARCHAR(100),
    @Transmission NVARCHAR(50),
    @FuelType NVARCHAR(50),
    @PricePerDay DECIMAL(18,2),
    @CarImage VARBINARY(MAX)
)
AS
BEGIN

    INSERT INTO Cars
    (
        CarName,
        CarModel,
        Transmission,
        FuelType,
        PricePerDay,
        CarImage,
        Available
    )
    VALUES
    (
        @CarName,
        @CarModel,
        @Transmission,
        @FuelType,
        @PricePerDay,
        @CarImage,
        1
    )

END
GO

CREATE PROCEDURE sp_GetCars
AS
BEGIN
    SELECT *
    FROM Cars
END
GO

CREATE PROCEDURE sp_GetAvailableCars
AS
BEGIN
    SELECT *
    FROM Cars
    WHERE Available = 1
END
GO

CREATE PROCEDURE sp_GetCarById
(
    @Id INT
)
AS
BEGIN
    SELECT *
    FROM Cars
    WHERE Id = @Id
END
GO

CREATE PROCEDURE sp_UpdateCar
(
    @Id INT,
    @CarName NVARCHAR(100),
    @CarModel NVARCHAR(100),
    @Transmission NVARCHAR(50),
    @FuelType NVARCHAR(50),
    @PricePerDay DECIMAL(18,2),
    @Available BIT
)
AS
BEGIN

    UPDATE Cars
    SET
    CarName = @CarName,
    CarModel = @CarModel,
    Transmission = @Transmission,
    FuelType = @FuelType,
    PricePerDay = @PricePerDay,
    Available = @Available
    WHERE Id = @Id

END
GO

CREATE PROCEDURE sp_DeleteCar
(
    @Id INT
)
AS
BEGIN

    DELETE FROM Bookings
    WHERE CarId = @Id

    DELETE FROM Cars
    WHERE Id = @Id

END
GO

/* ===========================================
   BOOKING MODULE
   =========================================== */

CREATE PROCEDURE sp_AddBooking
(
    @UserId INT,
    @CarId INT,
    @StartDate DATE,
    @EndDate DATE,
    @TotalDays INT,
    @TotalAmount DECIMAL(18,2)
)
AS
BEGIN

    INSERT INTO Bookings
    (
        UserId,
        CarId,
        StartDate,
        EndDate,
        TotalDays,
        TotalAmount,
        BookingDate,
        Status
    )
    VALUES
    (
        @UserId,
        @CarId,
        @StartDate,
        @EndDate,
        @TotalDays,
        @TotalAmount,
        GETDATE(),
        'Pending'
    )

END
GO

CREATE PROCEDURE sp_GetBookings
AS
BEGIN

    SELECT
        B.*,
        U.FullName,
        C.CarName
    FROM Bookings B
    INNER JOIN Users U
        ON B.UserId = U.Id
    INNER JOIN Cars C
        ON B.CarId = C.Id

END
GO

CREATE PROCEDURE sp_MyBookings
(
    @UserId INT
)
AS
BEGIN

    SELECT
        B.*,
        C.CarName
    FROM Bookings B
    INNER JOIN Cars C
        ON B.CarId = C.Id
    WHERE B.UserId = @UserId

END
GO

/* ===========================================
   VERIFICATION MODULE
   =========================================== */

CREATE PROCEDURE sp_AddVerification
(
    @UserId INT,
    @GovernmentId VARBINARY(MAX),
    @LicenseFront VARBINARY(MAX),
    @LicenseBack VARBINARY(MAX),
    @DigitalSignature VARBINARY(MAX)
)
AS
BEGIN

    INSERT INTO UserVerifications
    (
        UserId,
        GovernmentId,
        LicenseFront,
        LicenseBack,
        DigitalSignature,
        Status,
        UploadDate
    )
    VALUES
    (
        @UserId,
        @GovernmentId,
        @LicenseFront,
        @LicenseBack,
        @DigitalSignature,
        'Pending',
        GETDATE()
    )

END
GO

CREATE PROCEDURE sp_GetVerifications
AS
BEGIN

    SELECT
        V.*,
        U.FullName
    FROM UserVerifications V
    INNER JOIN Users U
        ON V.UserId = U.Id

END
GO

CREATE PROCEDURE sp_ApproveVerification
(
    @Id INT
)
AS
BEGIN

    UPDATE UserVerifications
    SET
        Status = 'Approved',
        VerifiedDate = GETDATE()
    WHERE Id = @Id

    UPDATE Users
    SET IsVerified = 1
    WHERE Id =
    (
        SELECT UserId
        FROM UserVerifications
        WHERE Id = @Id
    )

END
GO

CREATE PROCEDURE sp_RejectVerification
(
    @Id INT
)
AS
BEGIN

    UPDATE UserVerifications
    SET Status = 'Rejected'
    WHERE Id = @Id

    UPDATE Users
    SET IsVerified = 0
    WHERE Id =
    (
        SELECT UserId
        FROM UserVerifications
        WHERE Id = @Id
    )

END
GO

CREATE PROCEDURE sp_DeleteVerification
(
    @Id INT
)
AS
BEGIN

    UPDATE Users
    SET IsVerified = 0
    WHERE Id =
    (
        SELECT UserId
        FROM UserVerifications
        WHERE Id = @Id
    )

    DELETE FROM UserVerifications
    WHERE Id = @Id

END
GO

/* ===========================================
   DASHBOARD MODULE
   =========================================== */

CREATE PROCEDURE sp_Dashboard
AS
BEGIN

    SELECT
    (SELECT COUNT(*) FROM Cars) AS TotalCars,
    (SELECT COUNT(*) FROM Users WHERE Role='User') AS TotalUsers,
    (SELECT COUNT(*) FROM Bookings) AS TotalBookings,
    (SELECT COUNT(*) FROM Cars WHERE Available=1) AS AvailableCars,
    (SELECT COUNT(*) FROM Users WHERE IsBlocked=1) AS BlockedUsers,
    (SELECT COUNT(*) FROM Bookings WHERE Status='Approved') AS ApprovedBookings,
    (SELECT COUNT(*) FROM UserVerifications WHERE Status='Pending') AS PendingVerifications,
    (SELECT COUNT(*) FROM UserVerifications WHERE Status='Approved') AS ApprovedVerifications,
    (SELECT ISNULL(SUM(TotalAmount),0) FROM Bookings) AS TotalRevenue

END
GO
/* ===========================================
   Delete user 
   =========================================== */

CREATE OR ALTER PROCEDURE sp_DeleteUser
 @Id INT
AS
BEGIN
 SET NOCOUNT ON;
 BEGIN TRY
   IF EXISTS(SELECT 1 FROM Users WHERE Id=@Id AND Role='Admin')
   BEGIN
      RAISERROR('Admin account cannot be deleted.',16,1);
      RETURN;
   END

   BEGIN TRAN;

   DELETE FROM UserVerifications WHERE UserId=@Id;
   DELETE FROM Bookings WHERE UserId=@Id;
   DELETE FROM Users WHERE Id=@Id;

   COMMIT TRAN;
 END TRY
 BEGIN CATCH
   IF @@TRANCOUNT>0 ROLLBACK TRAN;
   THROW;
 END CATCH
END
GO