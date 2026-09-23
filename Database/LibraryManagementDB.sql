IF DB_ID(N'LibraryManagementDB') IS NULL
BEGIN
    CREATE DATABASE LibraryManagementDB;
END
GO
USE LibraryManagementDB;
GO

IF OBJECT_ID('dbo.LoanDetails', 'U') IS NOT NULL DROP TABLE dbo.LoanDetails;
IF OBJECT_ID('dbo.Loans', 'U') IS NOT NULL DROP TABLE dbo.Loans;
IF OBJECT_ID('dbo.Books', 'U') IS NOT NULL DROP TABLE dbo.Books;
IF OBJECT_ID('dbo.Readers', 'U') IS NOT NULL DROP TABLE dbo.Readers;
IF OBJECT_ID('dbo.Categories', 'U') IS NOT NULL DROP TABLE dbo.Categories;
GO

CREATE TABLE dbo.Categories (
    CategoryId INT IDENTITY(1,1) CONSTRAINT PK_Categories PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL CONSTRAINT UQ_Categories_Name UNIQUE,
    Description NVARCHAR(500) NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Categories_IsActive DEFAULT 1
);
GO

CREATE TABLE dbo.Books (
    BookId INT IDENTITY(1,1) CONSTRAINT PK_Books PRIMARY KEY,
    ISBN NVARCHAR(20) NOT NULL CONSTRAINT UQ_Books_ISBN UNIQUE,
    Title NVARCHAR(200) NOT NULL,
    Author NVARCHAR(160) NOT NULL,
    Publisher NVARCHAR(160) NULL,
    PublishYear INT NOT NULL,
    Quantity INT NOT NULL,
    AvailableQuantity INT NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    ShelfLocation NVARCHAR(50) NOT NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Books_IsActive DEFAULT 1,
    CategoryId INT NOT NULL CONSTRAINT FK_Books_Categories REFERENCES dbo.Categories(CategoryId)
);
GO
CREATE INDEX IX_Books_Title_Author ON dbo.Books(Title, Author);
GO

CREATE TABLE dbo.Readers (
    ReaderId INT IDENTITY(1,1) CONSTRAINT PK_Readers PRIMARY KEY,
    CardNumber NVARCHAR(30) NOT NULL CONSTRAINT UQ_Readers_CardNumber UNIQUE,
    FullName NVARCHAR(150) NOT NULL,
    DateOfBirth DATE NOT NULL,
    Phone NVARCHAR(30) NULL,
    Email NVARCHAR(150) NULL,
    Address NVARCHAR(300) NULL,
    RegisteredAt DATETIME2 NOT NULL CONSTRAINT DF_Readers_RegisteredAt DEFAULT SYSDATETIME(),
    IsActive BIT NOT NULL CONSTRAINT DF_Readers_IsActive DEFAULT 1
);
GO

CREATE TABLE dbo.Loans (
    LoanId INT IDENTITY(1,1) CONSTRAINT PK_Loans PRIMARY KEY,
    LoanCode NVARCHAR(30) NOT NULL CONSTRAINT UQ_Loans_LoanCode UNIQUE,
    ReaderId INT NOT NULL CONSTRAINT FK_Loans_Readers REFERENCES dbo.Readers(ReaderId),
    BorrowedAt DATETIME2 NOT NULL,
    DueDate DATETIME2 NOT NULL,
    ReturnedAt DATETIME2 NULL,
    Status NVARCHAR(20) NOT NULL,
    FineAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    Note NVARCHAR(500) NULL
);
GO

CREATE TABLE dbo.LoanDetails (
    LoanDetailId INT IDENTITY(1,1) CONSTRAINT PK_LoanDetails PRIMARY KEY,
    LoanId INT NOT NULL CONSTRAINT FK_LoanDetails_Loans REFERENCES dbo.Loans(LoanId) ON DELETE CASCADE,
    BookId INT NOT NULL CONSTRAINT FK_LoanDetails_Books REFERENCES dbo.Books(BookId),
    Quantity INT NOT NULL DEFAULT 1,
    FineAmount DECIMAL(18,2) NOT NULL DEFAULT 0
);
GO

INSERT dbo.Categories(Name, Description) VALUES
(N'Công nghệ', N'Programming, software, computer science'),
(N'Kinh tế', N'Business, finance and management'),
(N'Văn học', N'Novels, short stories and literature'),
(N'Khoa học', N'Science and research'),
(N'Kỹ năng sống', N'Personal growth and soft skills');
GO

INSERT dbo.Books(ISBN,Title,Author,Publisher,PublishYear,Quantity,AvailableQuantity,Price,ShelfLocation,CategoryId) VALUES
('9780135957059',N'The Pragmatic Programmer',N'David Thomas',N'Addison-Wesley',2019,8,8,39.99,N'A-01',1),
('9780134685991',N'Effective Java',N'Joshua Bloch',N'Addison-Wesley',2018,6,5,49.90,N'A-02',1),
('9781491950357',N'Designing Data-Intensive Applications',N'Martin Kleppmann',N'O''Reilly',2017,5,4,59.00,N'A-03',1),
('9780134494166',N'Clean Architecture',N'Robert C. Martin',N'Prentice Hall',2017,7,7,55.00,N'A-04',1),
('9780062316110',N'Sapiens',N'Yuval Noah Harari',N'Harper',2015,10,10,24.90,N'B-01',4),
('9780316346627',N'Atomic Habits',N'James Clear',N'Avery',2018,9,9,20.00,N'C-01',5),
('9780131103627',N'The C Programming Language',N'Brian Kernighan',N'Prentice Hall',1988,4,4,32.50,N'A-05',1),
('9780061120084',N'To Kill a Mockingbird',N'Harper Lee',N'Harper Perennial',2006,6,6,16.50,N'D-01',3),
('9780307474278',N'The Lean Startup',N'Eric Ries',N'Crown Business',2011,5,5,22.90,N'B-03',2);
GO

INSERT dbo.Readers(CardNumber,FullName,DateOfBirth,Phone,Email,Address) VALUES
('DG-0001',N'Nguyễn Minh Anh','2004-05-12','0901001001','minhanh@example.com',N'TP. Hồ Chí Minh'),
('DG-0002',N'Trần Hoàng Nam','2003-09-03','0902002002','hoangnam@example.com',N'Bình Dương'),
('DG-0003',N'Lê Gia Hân','2005-02-21','0903003003','giahan@example.com',N'Đồng Nai'),
('DG-0004',N'Phạm Quốc Bảo','2002-11-08','0904004004','quocbao@example.com',N'TP. Hồ Chí Minh');
GO

INSERT dbo.Loans(LoanCode,ReaderId,BorrowedAt,DueDate,ReturnedAt,Status,FineAmount,Note) VALUES
('LN-2026-0001',1,DATEADD(DAY,-3,SYSDATETIME()),DATEADD(DAY,11,CAST(GETDATE() AS DATE)),NULL,'Borrowed',0,N'Khách mượn bình thường'),
('LN-2026-0002',2,DATEADD(DAY,-18,SYSDATETIME()),DATEADD(DAY,-4,CAST(GETDATE() AS DATE)),NULL,'Overdue',20000,N'Phiếu quá hạn');
GO
INSERT dbo.LoanDetails(LoanId,BookId,Quantity,FineAmount) VALUES
(1,2,1,0),(2,3,1,20000);
GO

SELECT N'LibraryManagementDB ready.' AS Result;
