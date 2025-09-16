
-- Create Tables
CREATE TABLE StaffUser (
    StaffID INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(256) NOT NULL,
    Salt NVARCHAR(128) NOT NULL,
    [Role] NVARCHAR(20) NOT NULL CHECK ([Role] IN ('Teller', 'Manager', 'Admin')), -- Teller, Manager, Admin
    IsSuspended BIT NOT NULL DEFAULT 0,
    FirstName NVARCHAR(50), -- Added for greeting
    LastName NVARCHAR(50)  -- Added for potential future use
);
GO

CREATE TABLE Account (
    AccountID INT PRIMARY KEY IDENTITY(12345678, 1), -- Start with 8 digits
    FirstName NVARCHAR(100) NOT NULL,
    Surname NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100),
    Phone NVARCHAR(50),
    AddressLine1 NVARCHAR(150),
    AddressLine2 NVARCHAR(150),
    City NVARCHAR(100),
    County NVARCHAR(50) NOT NULL,
    AccountType NVARCHAR(20) NOT NULL CHECK (AccountType IN ('Current', 'Savings')), -- Current, Savings
    SortCode INT NOT NULL,
    Balance DECIMAL(18, 2) NOT NULL DEFAULT 0.00,
    OverdraftLimit DECIMAL(18, 2) NOT NULL DEFAULT 0.00,
    DateCreated DATETIME NOT NULL DEFAULT GETDATE()
);
GO

CREATE TABLE TransactionLog (
    TransactionID INT PRIMARY KEY IDENTITY(1,1),
    SourceAccountID INT NULL, -- Null for Deposits originating outside
    DestinationAccountID INT NULL, -- Null for Withdrawals or external transfers
    TransactionType NVARCHAR(50) NOT NULL, -- Deposit, Withdrawal, Transfer, Fee, Interest etc.
    Amount DECIMAL(18, 2) NOT NULL,
    TransactionDate DATETIME NOT NULL DEFAULT GETDATE(),
    Description NVARCHAR(255),
    RequiresApproval BIT NOT NULL DEFAULT 0,
    ApprovalStatus NVARCHAR(20) DEFAULT 'Approved', -- Pending, Approved, Rejected
    ApprovingManagerID INT NULL, -- FK to StaffUser table if needed
    ReferenceNumber NVARCHAR(100) UNIQUE -- For Transfers
    CONSTRAINT FK_Transaction_SourceAccount FOREIGN KEY (SourceAccountID) REFERENCES Account(AccountID),
    CONSTRAINT FK_Transaction_DestinationAccount FOREIGN KEY (DestinationAccountID) REFERENCES Account(AccountID)
    -- CONSTRAINT FK_Transaction_ApprovingManager FOREIGN KEY (ApprovingManagerID) REFERENCES StaffUser(StaffID) -- Optional FK
);
GO


-- Stored Procedures

-- StaffUser Procedures
CREATE PROCEDURE sp_GetStaffUserByUsername
    @Username NVARCHAR(50)
AS
BEGIN
    SELECT StaffID, Username, PasswordHash, Salt, Role, IsSuspended, FirstName, LastName
    FROM StaffUser
    WHERE Username = @Username;
END
GO

CREATE PROCEDURE sp_CreateStaffUser
    @Username NVARCHAR(50),
    @PasswordHash NVARCHAR(256),
    @Salt NVARCHAR(128),
    @Role NVARCHAR(20),
    @FirstName NVARCHAR(50),
    @LastName NVARCHAR(50)
AS
BEGIN
    INSERT INTO StaffUser (Username, PasswordHash, Salt, Role, IsSuspended, FirstName, LastName)
    VALUES (@Username, @PasswordHash, @Salt, @Role, 0, @FirstName, @LastName);
    SELECT SCOPE_IDENTITY(); -- Return the new StaffID
END
GO

CREATE PROCEDURE sp_UpdateStaffUserSuspension
    @StaffID INT,
    @IsSuspended BIT
AS
BEGIN
    UPDATE StaffUser
    SET IsSuspended = @IsSuspended
    WHERE StaffID = @StaffID;
END
GO

CREATE PROCEDURE sp_GetAllStaffUsers
AS
BEGIN
    SELECT StaffID, Username, Role, IsSuspended, FirstName, LastName
    FROM StaffUser;
END
GO

-- Account Procedures
CREATE PROCEDURE sp_CreateAccount
    @FirstName NVARCHAR(100),
    @Surname NVARCHAR(100),
    @Email NVARCHAR(100),
    @Phone NVARCHAR(50),
    @AddressLine1 NVARCHAR(150),
    @AddressLine2 NVARCHAR(150),
    @City NVARCHAR(100),
    @County NVARCHAR(50),
    @AccountType NVARCHAR(20),
    @SortCode INT,
    @Balance DECIMAL(18, 2),
    @OverdraftLimit DECIMAL(18, 2)
AS
BEGIN
    INSERT INTO Account (FirstName, Surname, Email, Phone, AddressLine1, AddressLine2, City, County, AccountType, SortCode, Balance, OverdraftLimit, DateCreated)
    VALUES (@FirstName, @Surname, @Email, @Phone, @AddressLine1, @AddressLine2, @City, @County, @AccountType, @SortCode, @Balance, @OverdraftLimit, GETDATE());
    SELECT SCOPE_IDENTITY(); -- Return the new AccountID (which is also the Account Number)
END
GO

CREATE PROCEDURE sp_UpdateAccountDetails
    @AccountID INT,
    @Email NVARCHAR(100),
    @Phone NVARCHAR(50),
    @AddressLine1 NVARCHAR(150),
    @AddressLine2 NVARCHAR(150),
    @City NVARCHAR(100),
    @County NVARCHAR(50),
    @OverdraftLimit DECIMAL(18, 2) -- Only Overdraft might be updatable for Current accounts via edit screen
AS
BEGIN
    UPDATE Account
    SET Email = @Email,
        Phone = @Phone,
        AddressLine1 = @AddressLine1,
        AddressLine2 = @AddressLine2,
        City = @City,
        County = @County,
        OverdraftLimit = @OverdraftLimit
    WHERE AccountID = @AccountID;
END
GO

CREATE PROCEDURE sp_GetAccountByID
    @AccountID INT
AS
BEGIN
    SELECT AccountID, FirstName, Surname, Email, Phone, AddressLine1, AddressLine2, City, County, AccountType, SortCode, Balance, OverdraftLimit, DateCreated
    FROM Account
    WHERE AccountID = @AccountID;
END
GO

CREATE PROCEDURE sp_GetAllAccounts
AS
BEGIN
    SELECT AccountID, FirstName, Surname, Email, Phone, AddressLine1, AddressLine2, City, County, AccountType, SortCode, Balance, OverdraftLimit, DateCreated
    FROM Account;
END
GO

CREATE PROCEDURE sp_UpdateAccountBalance
    @AccountID INT,
    @AmountChange DECIMAL(18, 2) -- Positive for deposit/credit, Negative for withdrawal/debit
AS
BEGIN
    UPDATE Account
    SET Balance = Balance + @AmountChange
    WHERE AccountID = @AccountID;
    -- Return new balance (optional)
    SELECT Balance FROM Account WHERE AccountID = @AccountID;
END
GO

-- Transaction Procedures
CREATE PROCEDURE sp_CreateTransaction
    @SourceAccountID INT,
    @DestinationAccountID INT,
    @TransactionType NVARCHAR(50),
    @Amount DECIMAL(18, 2),
    @Description NVARCHAR(255),
    @RequiresApproval BIT,
    @ApprovalStatus NVARCHAR(20),
    @ReferenceNumber NVARCHAR(100)
AS
BEGIN
    INSERT INTO TransactionLog (SourceAccountID, DestinationAccountID, TransactionType, Amount, TransactionDate, Description, RequiresApproval, ApprovalStatus, ReferenceNumber)
    VALUES (@SourceAccountID, @DestinationAccountID, @TransactionType, @Amount, GETDATE(), @Description, @RequiresApproval, @ApprovalStatus, @ReferenceNumber);
    SELECT SCOPE_IDENTITY(); -- Return the new TransactionID
END
GO

CREATE PROCEDURE sp_GetTransactionsByAccountID
    @AccountID INT
AS
BEGIN
    SELECT TransactionID, SourceAccountID, DestinationAccountID, TransactionType, Amount, TransactionDate, Description, RequiresApproval, ApprovalStatus, ReferenceNumber
    FROM TransactionLog
    WHERE SourceAccountID = @AccountID OR DestinationAccountID = @AccountID
    ORDER BY TransactionDate DESC;
END
GO

CREATE PROCEDURE sp_GetPendingTransactions
AS
BEGIN
    SELECT t.TransactionID, t.SourceAccountID, src.FirstName + ' ' + src.Surname AS SourceAccountName,
           t.DestinationAccountID, dest.FirstName + ' ' + dest.Surname AS DestinationAccountName,
           t.TransactionType, t.Amount, t.TransactionDate, t.Description, t.ReferenceNumber
    FROM TransactionLog t
    LEFT JOIN Account src ON t.SourceAccountID = src.AccountID
    LEFT JOIN Account dest ON t.DestinationAccountID = dest.AccountID
    WHERE t.RequiresApproval = 1 AND t.ApprovalStatus = 'Pending'
    ORDER BY t.TransactionDate ASC;
END
GO

CREATE PROCEDURE sp_UpdateTransactionApprovalStatus
    @TransactionID INT,
    @ApprovalStatus NVARCHAR(20), -- Approved, Rejected
    @ApprovingManagerID INT
AS
BEGIN
    UPDATE TransactionLog
    SET ApprovalStatus = @ApprovalStatus,
        ApprovingManagerID = @ApprovingManagerID
    WHERE TransactionID = @TransactionID AND RequiresApproval = 1 AND ApprovalStatus = 'Pending';

    -- Return 1 if successful, 0 otherwise (e.g., already processed)
    IF @@ROWCOUNT > 0
        SELECT 1;
    ELSE
        SELECT 0;
END
GO

CREATE PROCEDURE sp_GetTransactionByID -- Needed for processing after approval
    @TransactionID INT
AS
BEGIN
    SELECT TransactionID, SourceAccountID, DestinationAccountID, TransactionType, Amount, ApprovalStatus
    FROM TransactionLog
    WHERE TransactionID = @TransactionID;
END
GO

-- Add a new Admin to the staffUser Table (username:joe; password:test)
--INSERT INTO StaffUser (Username, PasswordHash, Salt, Role, IsSuspended, FirstName, LastName)
--VALUES ('joe', 'KyjbxxF4XAO1CIFIVHLq0NGHP1odeRfDDE85ZKwoK4A=', 'XmgR4VT8ukyJ71T/YFVONw==', 'Admin', 0, 'Joe', 'Bloggs');


SET IDENTITY_INSERT [dbo].[Account] ON 
GO

INSERT [dbo].[Account] ([AccountID], [FirstName], [Surname], [Email], [Phone], [AddressLine1], [AddressLine2], [City], [County], [AccountType], [SortCode], [Balance], [OverdraftLimit], [DateCreated]) 
VALUES (12345678, N'Matt', N'Philips', N'matt@phil.co', N'0980636483', N'24, Stealth Road', N'Dublin. Ireland', N'Dublin', N'Dublin', N'Current', 101010, CAST(63982.77 AS Decimal(18, 2)), CAST(1200.00 AS Decimal(18, 2)), CAST(N'2025-04-09T14:34:06.407' AS DateTime))
GO

INSERT [dbo].[Account] ([AccountID], [FirstName], [Surname], [Email], [Phone], [AddressLine1], [AddressLine2], [City], [County], [AccountType], [SortCode], [Balance], [OverdraftLimit], [DateCreated]) 
VALUES (12345679, N'Ryan', N'Breslow', N'ryan@microsoft.com', N'099567323', N'22, Back Avenue, Incihore', N'89, Dublin Rd, Dublin', N'Dublin', N'Dublin', N'Current', 101010, CAST(46417.00 AS Decimal(18, 2)), CAST(1200.00 AS Decimal(18, 2)), CAST(N'2025-04-09T14:39:35.070' AS DateTime))
GO

INSERT [dbo].[Account] ([AccountID], [FirstName], [Surname], [Email], [Phone], [AddressLine1], [AddressLine2], [City], [County], [AccountType], [SortCode], [Balance], [OverdraftLimit], [DateCreated]) 
VALUES (12345680, N'Sarah', N'Jones', N'sarah@gmail.com', N'08882374848', N'24 Airport Rd', N'15, Oldtown Drive', N'Dublin', N'Dublin', N'Current', 101010, CAST(66200.23 AS Decimal(18, 2)), CAST(10.00 AS Decimal(18, 2)), CAST(N'2025-04-09T21:09:14.337' AS DateTime))
GO

INSERT [dbo].[Account] ([AccountID], [FirstName], [Surname], [Email], [Phone], [AddressLine1], [AddressLine2], [City], [County], [AccountType], [SortCode], [Balance], [OverdraftLimit], [DateCreated]) 
VALUES (12345681, N'Nigel', N'Farage', N'nigel@london.gov.uk', N'08883647382', N'21, Downing Street', N'', N'Mayo', N'Mayo', N'Current', 101010, CAST(22670.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(N'2025-04-14T10:22:56.333' AS DateTime))
GO

INSERT [dbo].[Account] ([AccountID], [FirstName], [Surname], [Email], [Phone], [AddressLine1], [AddressLine2], [City], [County], [AccountType], [SortCode], [Balance], [OverdraftLimit], [DateCreated]) 
VALUES (12345682, N'Jacob', N'Daniels', N'jake@daniels.io', N'022934759', N'12, Simba Avenue', N'', N'cork', N'Cork', N'Savings', 101010, CAST(2300.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(N'2025-04-14T11:48:04.510' AS DateTime))
GO

SET IDENTITY_INSERT [dbo].[Account] OFF
GO

SET IDENTITY_INSERT [dbo].[TransactionLog] ON 
GO

INSERT [dbo].[TransactionLog] ([TransactionID], [SourceAccountID], [DestinationAccountID], [TransactionType], [Amount], [TransactionDate], [Description], [RequiresApproval], [ApprovalStatus], [ApprovingManagerID], [ReferenceNumber]) VALUES (1, 12345679, NULL, N'Withdrawal', CAST(400.00 AS Decimal(18, 2)), CAST(N'2025-04-09T14:40:56.137' AS DateTime), N'', 0, N'Approved', NULL, NULL)
GO

INSERT [dbo].[TransactionLog] ([TransactionID], [SourceAccountID], [DestinationAccountID], [TransactionType], [Amount], [TransactionDate], [Description], [RequiresApproval], [ApprovalStatus], [ApprovingManagerID], [ReferenceNumber]) VALUES (2, NULL, 12345678, N'Deposit', CAST(550.00 AS Decimal(18, 2)), CAST(N'2025-04-09T20:37:20.423' AS DateTime), N'extra cash', 0, N'Approved', NULL, N'7dd92da790af470d')
GO

INSERT [dbo].[TransactionLog] ([TransactionID], [SourceAccountID], [DestinationAccountID], [TransactionType], [Amount], [TransactionDate], [Description], [RequiresApproval], [ApprovalStatus], [ApprovingManagerID], [ReferenceNumber]) 
VALUES (3, NULL, 12345679, N'Deposit', CAST(550.00 AS Decimal(18, 2)), CAST(N'2025-04-09T20:38:48.800' AS DateTime), N'extra cash', 0, N'Approved', NULL, N'3fff89a0eedf4de8')
GO

INSERT [dbo].[TransactionLog] ([TransactionID], [SourceAccountID], [DestinationAccountID], [TransactionType], [Amount], [TransactionDate], [Description], [RequiresApproval], [ApprovalStatus], [ApprovingManagerID], [ReferenceNumber]) 
VALUES (4, NULL, 12345678, N'Deposit', CAST(500.00 AS Decimal(18, 2)), CAST(N'2025-04-09T21:10:50.460' AS DateTime), N'random fund', 0, N'Approved', NULL, N'd569c6b246114d1f')
GO

INSERT [dbo].[TransactionLog] ([TransactionID], [SourceAccountID], [DestinationAccountID], [TransactionType], [Amount], [TransactionDate], [Description], [RequiresApproval], [ApprovalStatus], [ApprovingManagerID], [ReferenceNumber]) 
VALUES (5, NULL, 12345678, N'Deposit', CAST(15000.00 AS Decimal(18, 2)), CAST(N'2025-04-14T08:31:57.890' AS DateTime), N'Lottery', 0, N'Approved', NULL, N'98be9d6098a44be7')
GO

INSERT [dbo].[TransactionLog] ([TransactionID], [SourceAccountID], [DestinationAccountID], [TransactionType], [Amount], [TransactionDate], [Description], [RequiresApproval], [ApprovalStatus], [ApprovingManagerID], [ReferenceNumber]) 
VALUES (6, NULL, 12345679, N'Deposit', CAST(19500.00 AS Decimal(18, 2)), CAST(N'2025-04-14T10:26:29.350' AS DateTime), N'Stock dividend payments', 1, N'Approved', 3, N'9de0789f0f5c438c')
GO

INSERT [dbo].[TransactionLog] ([TransactionID], [SourceAccountID], [DestinationAccountID], [TransactionType], [Amount], [TransactionDate], [Description], [RequiresApproval], [ApprovalStatus], [ApprovingManagerID], [ReferenceNumber]) 
VALUES (7, 12345681, NULL, N'Withdrawal', CAST(330.00 AS Decimal(18, 2)), CAST(N'2025-04-14T10:28:46.727' AS DateTime), N'', 0, N'Approved', NULL, N'89b7b85980a34f7b')
GO

INSERT [dbo].[TransactionLog] ([TransactionID], [SourceAccountID], [DestinationAccountID], [TransactionType], [Amount], [TransactionDate], [Description], [RequiresApproval], [ApprovalStatus], [ApprovingManagerID], [ReferenceNumber]) 
VALUES (8, 12345678, 12345680, N'Transfer', CAST(217.23 AS Decimal(18, 2)), CAST(N'2025-04-14T10:32:22.177' AS DateTime), N' to Acc 12345680', 0, N'Approved', NULL, N'5e9863d6b51140d8')
GO

INSERT [dbo].[TransactionLog] ([TransactionID], [SourceAccountID], [DestinationAccountID], [TransactionType], [Amount], [TransactionDate], [Description], [RequiresApproval], [ApprovalStatus], [ApprovingManagerID], [ReferenceNumber]) 
VALUES (9, 12345680, 12345679, N'Transfer', CAST(117.00 AS Decimal(18, 2)), CAST(N'2025-04-14T10:45:37.357' AS DateTime), N' to Acc 12345679', 0, N'Approved', NULL, N'0ea544478bfd4c01')
GO

INSERT [dbo].[TransactionLog] ([TransactionID], [SourceAccountID], [DestinationAccountID], [TransactionType], [Amount], [TransactionDate], [Description], [RequiresApproval], [ApprovalStatus], [ApprovingManagerID], [ReferenceNumber]) 
VALUES (10, NULL, 12345680, N'Deposit', CAST(75000.00 AS Decimal(18, 2)), CAST(N'2025-04-14T10:46:10.927' AS DateTime), N'Annual S&P500 Dividends', 0, N'Approved', NULL, N'18737748fcf54a43')
GO

INSERT [dbo].[TransactionLog] ([TransactionID], [SourceAccountID], [DestinationAccountID], [TransactionType], [Amount], [TransactionDate], [Description], [RequiresApproval], [ApprovalStatus], [ApprovingManagerID], [ReferenceNumber]) 
VALUES (11, NULL, 12345678, N'Deposit', CAST(50000.00 AS Decimal(18, 2)), CAST(N'2025-04-14T10:46:45.727' AS DateTime), N'Annual ROI', 1, N'Approved', 2, N'd2355c8fb4224a0e')
GO

INSERT [dbo].[TransactionLog] ([TransactionID], [SourceAccountID], [DestinationAccountID], [TransactionType], [Amount], [TransactionDate], [Description], [RequiresApproval], [ApprovalStatus], [ApprovingManagerID], [ReferenceNumber]) 
VALUES (12, NULL, 12345679, N'Deposit', CAST(15000.00 AS Decimal(18, 2)), CAST(N'2025-04-14T10:47:55.173' AS DateTime), N'lottery', 1, N'Approved', 2, N'52ffc8ca24ce4608')
GO

INSERT [dbo].[TransactionLog] ([TransactionID], [SourceAccountID], [DestinationAccountID], [TransactionType], [Amount], [TransactionDate], [Description], [RequiresApproval], [ApprovalStatus], [ApprovingManagerID], [ReferenceNumber]) 
VALUES (13, 12345679, 12345678, N'Transfer', CAST(200.00 AS Decimal(18, 2)), CAST(N'2025-04-14T11:33:02.147' AS DateTime), N' to Acc 12345678', 0, N'Approved', NULL, N'37e91ca7dbad4c6c')
GO

INSERT [dbo].[TransactionLog] ([TransactionID], [SourceAccountID], [DestinationAccountID], [TransactionType], [Amount], [TransactionDate], [Description], [RequiresApproval], [ApprovalStatus], [ApprovingManagerID], [ReferenceNumber]) 
VALUES (14, NULL, 12345678, N'Deposit', CAST(12000.00 AS Decimal(18, 2)), CAST(N'2025-04-14T11:39:39.990' AS DateTime), N'', 1, N'Approved', 2, N'789bc1af0b4548cb')
GO

INSERT [dbo].[TransactionLog] ([TransactionID], [SourceAccountID], [DestinationAccountID], [TransactionType], [Amount], [TransactionDate], [Description], [RequiresApproval], [ApprovalStatus], [ApprovingManagerID], [ReferenceNumber]) 
VALUES (15, NULL, 12345679, N'Deposit', CAST(11000.00 AS Decimal(18, 2)), CAST(N'2025-04-14T11:39:52.473' AS DateTime), N'', 1, N'Approved', 2, N'2704a61a72f74638')
GO

INSERT [dbo].[TransactionLog] ([TransactionID], [SourceAccountID], [DestinationAccountID], [TransactionType], [Amount], [TransactionDate], [Description], [RequiresApproval], [ApprovalStatus], [ApprovingManagerID], [ReferenceNumber]) 
VALUES (16, 12345679, NULL, N'Withdrawal', CAST(11000.00 AS Decimal(18, 2)), CAST(N'2025-04-14T11:40:02.077' AS DateTime), N'', 1, N'Rejected', 2, N'4ffc34d07d0f40f2')
GO

INSERT [dbo].[TransactionLog] ([TransactionID], [SourceAccountID], [DestinationAccountID], [TransactionType], [Amount], [TransactionDate], [Description], [RequiresApproval], [ApprovalStatus], [ApprovingManagerID], [ReferenceNumber]) 
VALUES (17, 12345680, 12345681, N'Transfer', CAST(10000.00 AS Decimal(18, 2)), CAST(N'2025-04-14T11:40:17.393' AS DateTime), N' to Acc 12345681', 1, N'Approved', 2, N'0954c18b9a5c490d')
GO

INSERT [dbo].[TransactionLog] ([TransactionID], [SourceAccountID], [DestinationAccountID], [TransactionType], [Amount], [TransactionDate], [Description], [RequiresApproval], [ApprovalStatus], [ApprovingManagerID], [ReferenceNumber]) 
VALUES (18, 12345681, NULL, N'Withdrawal', CAST(10000.00 AS Decimal(18, 2)), CAST(N'2025-04-14T11:40:27.407' AS DateTime), N'', 1, N'Approved', 2, N'1ca96a0af4fd4cb9')
GO

INSERT [dbo].[TransactionLog] ([TransactionID], [SourceAccountID], [DestinationAccountID], [TransactionType], [Amount], [TransactionDate], [Description], [RequiresApproval], [ApprovalStatus], [ApprovingManagerID], [ReferenceNumber]) 
VALUES (19, 12345680, NULL, N'Transfer', CAST(150.00 AS Decimal(18, 2)), CAST(N'2025-04-14T11:53:20.737' AS DateTime), N' to Sort Code 123456', 0, N'Approved', NULL, N'9ec164d1d6654c4c')
GO

SET IDENTITY_INSERT [dbo].[TransactionLog] OFF
GO

SET IDENTITY_INSERT [dbo].[StaffUser] ON 
GO

INSERT [dbo].[StaffUser] ([StaffID], [Username], [PasswordHash], [Salt], [Role], [IsSuspended], [FirstName], [LastName]) VALUES (2, N'joe', N'KyjbxxF4XAO1CIFIVHLq0NGHP1odeRfDDE85ZKwoK4A=', N'XmgR4VT8ukyJ71T/YFVONw==', N'Admin', 0, N'Joe', N'Bloggs')
GO

INSERT [dbo].[StaffUser] ([StaffID], [Username], [PasswordHash], [Salt], [Role], [IsSuspended], [FirstName], [LastName]) VALUES (3, N'sarah', N'H8FDlVJKng+EAhFPV80DzRY7z+EoWG2tbtztylUWYHE=', N'8eAisJHz+3vS+fy6DsfvNw==', N'Manager', 0, N'Sarah', N'Jones')
GO

INSERT [dbo].[StaffUser] ([StaffID], [Username], [PasswordHash], [Salt], [Role], [IsSuspended], [FirstName], [LastName]) VALUES (4, N'alfred', N'Rd77V/kbd9fekWJAc0S+MLOEM/aoaVzQOmn9fBi/5S8=', N'jdd4iPi9MgXfYoMp6HuAEQ==', N'Teller', 0, N'Alfie', N'Solmon')
GO

INSERT [dbo].[StaffUser] ([StaffID], [Username], [PasswordHash], [Salt], [Role], [IsSuspended], [FirstName], [LastName]) VALUES (5, N'mary', N'/Z6AKAonxAOXNV5PVSmuwT2+rJJ4RDaVYPv9yRQH7OY=', N'UHqwR1p46lW1UWyUqWOl7g==', N'Teller', 0, N'Mary', N'Lynch')
GO

SET IDENTITY_INSERT [dbo].[StaffUser] OFF
GO
