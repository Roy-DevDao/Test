IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'DocCare')
BEGIN
    CREATE DATABASE DocCare;
END;
GO

USE DocCare;
GO

CREATE TABLE Contact (
    ContactId NVARCHAR(50) PRIMARY KEY,    -- checked
    Name NVARCHAR(MAX),  -- Changed to MAX
    Email NVARCHAR(MAX), -- Changed to MAX
    Title NVARCHAR(MAX), -- Changed to MAX
    Description NVARCHAR(MAX),
    Status NVARCHAR(50)
);

-- Account table
CREATE TABLE Account (                  -- checked
    Id NVARCHAR(255) PRIMARY KEY,
    Username NVARCHAR(MAX), -- Changed to MAX
    Password NVARCHAR(MAX), -- Changed to MAX
    Email NVARCHAR(MAX),    -- Changed to MAX
    Role INT,
    Status BIT
);

-- Patient table - Using Id from Account
CREATE TABLE Patient (   -- checked
    PId NVARCHAR(255) PRIMARY KEY,
    Name NVARCHAR(MAX),      -- Changed to MAX
    PatientImg NVARCHAR(MAX), -- Changed to MAX
    Phone NVARCHAR(MAX),     -- Changed to MAX
    Gender NVARCHAR(50),
    DOB DATE,
    FOREIGN KEY (PId) REFERENCES Account(Id) -- Using Id as foreign key
);

-- Specialty table
CREATE TABLE Specialty (   
    SpecialtyId NVARCHAR(255) PRIMARY KEY,
    SpecialtyName NVARCHAR(MAX), -- Changed to MAX
    SpecialtyImg NVARCHAR(MAX),  -- Changed to MAX
    ShortDescription NVARCHAR(MAX), -- Changed to MAX
);

-- DetailSpecialty
CREATE TABLE DetailSpecialty (
    DetailId NVARCHAR(50) PRIMARY KEY,
    SpecialtyId NVARCHAR(255),
    Title NVARCHAR(MAX),
    Content NVARCHAR(MAX),
    FOREIGN KEY (SpecialtyId) REFERENCES Specialty(SpecialtyId)
);

-- Doctor table - Using Id from Account
CREATE TABLE Doctor (
    DId NVARCHAR(255) PRIMARY KEY,
    Name NVARCHAR(MAX),        -- Changed to MAX
    DoctorImg NVARCHAR(MAX),   -- Changed to MAX
    Position NVARCHAR(MAX),    -- Changed to MAX
    Phone NVARCHAR(MAX),       -- Changed to MAX
    Gender NVARCHAR(50),
    DOB DATE,
    Description NVARCHAR(MAX),
    Price FLOAT,
    SpecialtyId NVARCHAR(255),
    FOREIGN KEY (DId) REFERENCES Account(Id), -- Using Id as foreign key
    FOREIGN KEY (SpecialtyId) REFERENCES Specialty(SpecialtyId)
);

-- DetailDoctor table
CREATE TABLE DetailDoctor (
    DetailId NVARCHAR(255) PRIMARY KEY,
    DId NVARCHAR(255),
    Title NVARCHAR(MAX),     -- Changed to MAX
    Content NVARCHAR(MAX),
    FOREIGN KEY (DId) REFERENCES Doctor(DId)
);

-- Feedback table
CREATE TABLE Feedback (
    FeedbackId NVARCHAR(255) PRIMARY KEY,
    DId NVARCHAR(255),
    PId NVARCHAR(255),
    Name NVARCHAR(MAX),   -- Changed to MAX
    DateCmt DATETIME,
    Star INT,
    Description NVARCHAR(MAX),
    FOREIGN KEY (DId) REFERENCES Doctor(DId),
    FOREIGN KEY (PId) REFERENCES Patient(PId)
);

-- Schedule table
-- none

-- Option table
CREATE TABLE [Option] (
    OptionId NVARCHAR(255) PRIMARY KEY,
    DId NVARCHAR(255),
    Status NVARCHAR(50),
	DateWork DATETIME,
    FOREIGN KEY (DId) REFERENCES Doctor(DId)
);

-- Order table
CREATE TABLE [Order] (
    OId NVARCHAR(255) PRIMARY KEY,
    PId NVARCHAR(255),
    OptionId NVARCHAR(255),
    Status NVARCHAR(MAX),  -- Changed to MAX
    DateOrder DATETIME,
    Symptom NVARCHAR(MAX),
    FOREIGN KEY (PId) REFERENCES Patient(PId),
    FOREIGN KEY (OptionId) REFERENCES [Option](OptionId)
);

-- HealthRecord table
CREATE TABLE HealthRecord (
    RecordId NVARCHAR(255) PRIMARY KEY,
    PId NVARCHAR(255),
    DId NVARCHAR(255),
    OId NVARCHAR(255),
    Diagnosis NVARCHAR(MAX),
    Description NVARCHAR(MAX),
    Note NVARCHAR(MAX),
    DateExam DATETIME,
    FOREIGN KEY (PId) REFERENCES Patient(PId),
    FOREIGN KEY (DId) REFERENCES Doctor(DId),
    FOREIGN KEY (OId) REFERENCES [Order](OId)
);



-- Payment table (updated)
CREATE TABLE Payment (
    PayId NVARCHAR(255) PRIMARY KEY,
    OId NVARCHAR(255),
    Method NVARCHAR(MAX),  -- Changed to MAX
    PayImg NVARCHAR(MAX),  -- Changed to MAX
    DatePay DATETIME,
    FOREIGN KEY (OId) REFERENCES [Order](OId)
);

CREATE TABLE Staff (   -- checked
    StaffId NVARCHAR(255) PRIMARY KEY,
    Name NVARCHAR(MAX),      -- Changed to MAX
    StaffImg NVARCHAR(MAX), -- Changed to MAX
    Phone NVARCHAR(MAX),     -- Changed to MAX
    Gender NVARCHAR(50),
    DOB DATE,
    FOREIGN KEY (StaffId) REFERENCES Account(Id) -- Using Id as foreign key
);