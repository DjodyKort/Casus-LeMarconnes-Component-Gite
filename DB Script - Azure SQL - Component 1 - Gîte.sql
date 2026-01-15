-- --------------------------------------------------------
-- DATABASE SCRIPT: LE MARCONNES - SPRINT 1 (DE GÎTE)
-- Scope: Enkel Gîte functionaliteit (Geheel vs. Slaapplek)
-- Dialect: Azure SQL / T-SQL
-- Converted from MySQL to Azure SQL
-- --------------------------------------------------------



-- ========================================================
-- 1. OPSCHONEN (Bestaande tabellen verwijderen)
-- ========================================================
-- Drop in juiste volgorde (child tables eerst)
IF OBJECT_ID('dbo.LOGBOEK', 'U') IS NOT NULL DROP TABLE dbo.LOGBOEK;
IF OBJECT_ID('dbo.RESERVERING_DETAIL', 'U') IS NOT NULL DROP TABLE dbo.RESERVERING_DETAIL;
IF OBJECT_ID('dbo.RESERVERING', 'U') IS NOT NULL DROP TABLE dbo.RESERVERING;
IF OBJECT_ID('dbo.TARIEF', 'U') IS NOT NULL DROP TABLE dbo.TARIEF;
IF OBJECT_ID('dbo.VERHUUR_EENHEID', 'U') IS NOT NULL DROP TABLE dbo.VERHUUR_EENHEID;
IF OBJECT_ID('dbo.GEBRUIKER', 'U') IS NOT NULL DROP TABLE dbo.GEBRUIKER;
IF OBJECT_ID('dbo.GAST', 'U') IS NOT NULL DROP TABLE dbo.GAST;
IF OBJECT_ID('dbo.TARIEF_CATEGORIE', 'U') IS NOT NULL DROP TABLE dbo.TARIEF_CATEGORIE;
IF OBJECT_ID('dbo.PLATFORM', 'U') IS NOT NULL DROP TABLE dbo.PLATFORM;
IF OBJECT_ID('dbo.ACCOMMODATIE_TYPE', 'U') IS NOT NULL DROP TABLE dbo.ACCOMMODATIE_TYPE;
GO

-- ========================================================
-- 2. TABELLEN AANMAKEN (DDL)
-- ========================================================

-- 2.1 Configuratie Tabellen
CREATE TABLE dbo.ACCOMMODATIE_TYPE (
    TypeID INT IDENTITY(1,1) PRIMARY KEY,
    Naam NVARCHAR(50) NOT NULL UNIQUE
);
GO

CREATE TABLE dbo.PLATFORM (
    PlatformID INT IDENTITY(1,1) PRIMARY KEY,
    Naam NVARCHAR(100) NOT NULL,
    CommissiePercentage DECIMAL(5,2) DEFAULT 0.00
);
GO

CREATE TABLE dbo.TARIEF_CATEGORIE (
    CategorieID INT IDENTITY(1,1) PRIMARY KEY,
    Naam NVARCHAR(100) NOT NULL
);
GO

-- 2.2 Gasten & Gebruikers
CREATE TABLE dbo.GAST (
    GastID INT IDENTITY(1,1) PRIMARY KEY,
    Naam NVARCHAR(100) NOT NULL,
    Email NVARCHAR(150) NOT NULL,
    Tel NVARCHAR(20) NULL,
    Straat NVARCHAR(100) NOT NULL,
    Huisnr NVARCHAR(20) NOT NULL,
    Postcode NVARCHAR(20) NOT NULL,
    Plaats NVARCHAR(100) NOT NULL,
    Land NVARCHAR(50) DEFAULT N'Nederland',
    IBAN NVARCHAR(34) NULL
);
GO

-- Index voor email lookups
CREATE NONCLUSTERED INDEX IX_GAST_Email ON dbo.GAST(Email);
GO

CREATE TABLE dbo.GEBRUIKER (
    GebruikerID INT IDENTITY(1,1) PRIMARY KEY,
    GastID INT NULL UNIQUE, -- 1:1 Relatie
    Email NVARCHAR(150) NOT NULL UNIQUE,
    WachtwoordHash NVARCHAR(255) NOT NULL,
    Rol NVARCHAR(20) DEFAULT N'Gast',
    CONSTRAINT FK_Gebruiker_Gast FOREIGN KEY (GastID) REFERENCES dbo.GAST(GastID) ON DELETE CASCADE
);
GO

-- Index voor email lookups
CREATE NONCLUSTERED INDEX IX_GEBRUIKER_Email ON dbo.GEBRUIKER(Email);
GO

-- 2.3 Inventaris (De Gîte Hiërarchie)
CREATE TABLE dbo.VERHUUR_EENHEID (
    EenheidID INT IDENTITY(1,1) PRIMARY KEY,
    Naam NVARCHAR(100) NOT NULL,
    TypeID INT NOT NULL,
    MaxCapaciteit INT NOT NULL CHECK (MaxCapaciteit > 0),
    ParentEenheidID INT NULL, -- Recursieve sleutel voor Gîte structuur
    CONSTRAINT FK_Eenheid_Type FOREIGN KEY (TypeID) REFERENCES dbo.ACCOMMODATIE_TYPE(TypeID),
    CONSTRAINT FK_Eenheid_Parent FOREIGN KEY (ParentEenheidID) REFERENCES dbo.VERHUUR_EENHEID(EenheidID)
);
GO

-- Index voor type filtering en parent lookups
CREATE NONCLUSTERED INDEX IX_VERHUUR_EENHEID_TypeID ON dbo.VERHUUR_EENHEID(TypeID);
CREATE NONCLUSTERED INDEX IX_VERHUUR_EENHEID_ParentID ON dbo.VERHUUR_EENHEID(ParentEenheidID);
GO

-- 2.4 Prijzen
CREATE TABLE dbo.TARIEF (
    TariefID INT IDENTITY(1,1) PRIMARY KEY,
    TypeID INT NOT NULL,
    CategorieID INT NOT NULL,
    PlatformID INT NULL,
    Prijs DECIMAL(10,2) NOT NULL,
    TaxStatus BIT DEFAULT 0, -- 0 = Excl, 1 = Incl (Voor Gîte is dit 1)
    TaxTarief DECIMAL(10,2) DEFAULT 0,
    GeldigVan DATE NOT NULL,
    GeldigTot DATE NULL,
    CONSTRAINT FK_Tarief_Type FOREIGN KEY (TypeID) REFERENCES dbo.ACCOMMODATIE_TYPE(TypeID),
    CONSTRAINT FK_Tarief_Cat FOREIGN KEY (CategorieID) REFERENCES dbo.TARIEF_CATEGORIE(CategorieID),
    CONSTRAINT FK_Tarief_Platform FOREIGN KEY (PlatformID) REFERENCES dbo.PLATFORM(PlatformID)
);
GO

-- Index voor tarief lookups (belangrijkste query pattern)
CREATE NONCLUSTERED INDEX IX_TARIEF_Lookup 
ON dbo.TARIEF(TypeID, PlatformID, GeldigVan, GeldigTot) 
INCLUDE (Prijs, TaxStatus, TaxTarief, CategorieID);
GO

-- 2.5 Transacties (Reserveringen)
CREATE TABLE dbo.RESERVERING (
    ReserveringID INT IDENTITY(1,1) PRIMARY KEY,
    GastID INT NOT NULL,
    EenheidID INT NOT NULL,
    PlatformID INT NOT NULL,
    Startdatum DATE NOT NULL,
    Einddatum DATE NOT NULL,
    Status NVARCHAR(20) DEFAULT N'Gereserveerd',
    CONSTRAINT CHK_RESERVERING_Dates CHECK (Einddatum > Startdatum),
    CONSTRAINT FK_Res_Gast FOREIGN KEY (GastID) REFERENCES dbo.GAST(GastID),
    CONSTRAINT FK_Res_Eenheid FOREIGN KEY (EenheidID) REFERENCES dbo.VERHUUR_EENHEID(EenheidID),
    CONSTRAINT FK_Res_Platform FOREIGN KEY (PlatformID) REFERENCES dbo.PLATFORM(PlatformID)
);
GO

-- Cruciale indexes voor beschikbaarheid checks en overlapping queries
CREATE NONCLUSTERED INDEX IX_RESERVERING_DateRange 
ON dbo.RESERVERING(Startdatum, Einddatum, Status) 
INCLUDE (EenheidID, GastID, PlatformID);

CREATE NONCLUSTERED INDEX IX_RESERVERING_Eenheid 
ON dbo.RESERVERING(EenheidID, Startdatum, Einddatum) 
INCLUDE (Status);

CREATE NONCLUSTERED INDEX IX_RESERVERING_Gast 
ON dbo.RESERVERING(GastID, Startdatum DESC);
GO

CREATE TABLE dbo.RESERVERING_DETAIL (
    DetailID INT IDENTITY(1,1) PRIMARY KEY,
    ReserveringID INT NOT NULL,
    CategorieID INT NOT NULL,
    Aantal INT DEFAULT 1,
    PrijsOpMoment DECIMAL(10,2) NOT NULL,
    CONSTRAINT FK_Detail_Res FOREIGN KEY (ReserveringID) REFERENCES dbo.RESERVERING(ReserveringID) ON DELETE CASCADE,
    CONSTRAINT FK_Detail_Cat FOREIGN KEY (CategorieID) REFERENCES dbo.TARIEF_CATEGORIE(CategorieID)
);
GO

-- Index voor detail lookups
CREATE NONCLUSTERED INDEX IX_RESERVERING_DETAIL_ReserveringID 
ON dbo.RESERVERING_DETAIL(ReserveringID);
GO

-- 2.6 Logging (Systeem Eis)
CREATE TABLE dbo.LOGBOEK (
    LogID INT IDENTITY(1,1) PRIMARY KEY,
    GebruikerID INT NULL,
    Tijdstip DATETIME2 DEFAULT SYSDATETIME(),
    Actie NVARCHAR(50) NOT NULL,
    TabelNaam NVARCHAR(50) NULL,
    RecordID INT NULL,
    OudeWaarde NVARCHAR(MAX) NULL,
    NieuweWaarde NVARCHAR(MAX) NULL,
    CONSTRAINT FK_Log_Gebruiker FOREIGN KEY (GebruikerID) REFERENCES dbo.GEBRUIKER(GebruikerID)
);
GO

-- Index voor tijdstip sorting (voor recente logs)
CREATE NONCLUSTERED INDEX IX_LOGBOEK_Tijdstip 
ON dbo.LOGBOEK(Tijdstip DESC);
GO

-- ========================================================
-- 3. VOORBEELDDATA LADEN (SEEDING)
-- ========================================================

-- 3.1 Basis Lookup Data
SET IDENTITY_INSERT dbo.ACCOMMODATIE_TYPE ON;
INSERT INTO dbo.ACCOMMODATIE_TYPE (TypeID, Naam) VALUES 
(1, N'Gîte-Geheel'), 
(2, N'Gîte-Slaapplek');
SET IDENTITY_INSERT dbo.ACCOMMODATIE_TYPE OFF;
GO

SET IDENTITY_INSERT dbo.PLATFORM ON;
INSERT INTO dbo.PLATFORM (PlatformID, Naam, CommissiePercentage) VALUES 
(1, N'Eigen Site', 0.00), 
(2, N'Booking.com', 15.00), 
(3, N'Airbnb', 3.00);
SET IDENTITY_INSERT dbo.PLATFORM OFF;
GO

SET IDENTITY_INSERT dbo.TARIEF_CATEGORIE ON;
INSERT INTO dbo.TARIEF_CATEGORIE (CategorieID, Naam) VALUES 
(1, N'Logies'), 
(2, N'Toeristenbelasting');
SET IDENTITY_INSERT dbo.TARIEF_CATEGORIE OFF;
GO

-- 3.2 De Gîte Hiérarchie
-- ID 1 is de Parent (Het gehele appartement)
SET IDENTITY_INSERT dbo.VERHUUR_EENHEID ON;
INSERT INTO dbo.VERHUUR_EENHEID (EenheidID, Naam, TypeID, MaxCapaciteit, ParentEenheidID) 
VALUES (1, N'Gîte Le Marconnès (Totaal)', 1, 9, NULL);
SET IDENTITY_INSERT dbo.VERHUUR_EENHEID OFF;
GO

-- ID 2 t/m 10 zijn de Children (De losse bedden/kamers)
INSERT INTO dbo.VERHUUR_EENHEID (Naam, TypeID, MaxCapaciteit, ParentEenheidID) VALUES 
(N'Slaapplek 1 (Kamer 1A)', 2, 1, 1),
(N'Slaapplek 2 (Kamer 1B)', 2, 1, 1),
(N'Slaapplek 3 (Kamer 2A)', 2, 1, 1),
(N'Slaapplek 4 (Kamer 2B)', 2, 1, 1),
(N'Slaapplek 5 (Kamer 2C)', 2, 1, 1),
(N'Slaapplek 6 (Kamer 3A)', 2, 1, 1),
(N'Slaapplek 7 (Kamer 3B)', 2, 1, 1),
(N'Slaapplek 8 (Kamer 4A)', 2, 1, 1),
(N'Slaapplek 9 (Kamer 4B)', 2, 1, 1);
GO

-- 3.3 Tarieven 2025
-- Model A: €200 voor gehele Gîte (Booking.com), Tax Inclusief (1)
INSERT INTO dbo.TARIEF (TypeID, PlatformID, CategorieID, Prijs, TaxStatus, GeldigVan) 
VALUES (1, 2, 1, 200.00, 1, '2025-01-01');

-- Model B: €27,50 per bed (Airbnb), Tax Inclusief (1)
INSERT INTO dbo.TARIEF (TypeID, PlatformID, CategorieID, Prijs, TaxStatus, GeldigVan) 
VALUES (2, 3, 1, 27.50, 1, '2025-01-01');
GO

-- 3.4 Gebruikers (Eigenaren + Testgast)
INSERT INTO dbo.GAST (Naam, Email, Tel, Straat, Huisnr, Postcode, Plaats, Land) VALUES 
(N'Elvire & Ed', N'info@lemarconnes.com', N'0612345678', N'Route de Barges', N'1', N'43420', N'St Arcons', N'Frankrijk'),
(N'Jan Jansen', N'jan@test.nl', N'0687654321', N'Kalverstraat', N'10', N'1012AB', N'Amsterdam', N'Nederland');
GO

-- Haal de GastID's op voor de gebruikers
DECLARE @Eigenaar_GastID INT = (SELECT GastID FROM dbo.GAST WHERE Email = N'info@lemarconnes.com');
DECLARE @TestGast_GastID INT = (SELECT GastID FROM dbo.GAST WHERE Email = N'jan@test.nl');

INSERT INTO dbo.GEBRUIKER (GastID, Email, WachtwoordHash, Rol) VALUES 
(@Eigenaar_GastID, N'info@lemarconnes.com', N'$2y$10$fakesecrethash', N'Eigenaar'),
(@TestGast_GastID, N'jan@test.nl', N'$2y$10$fakesecrethash', N'Gast');
GO

-- 3.5 Een Test Reservering (Gîte Geheel)
-- Gast 2 (Jan) boekt Eenheid 1 (Gîte Totaal) via Platform 2 (Booking.com)
DECLARE @TestGastID INT = (SELECT GastID FROM dbo.GAST WHERE Email = N'jan@test.nl');
DECLARE @NewReserveringID INT;

INSERT INTO dbo.RESERVERING (GastID, EenheidID, PlatformID, Startdatum, Einddatum, Status) 
VALUES (@TestGastID, 1, 2, '2025-06-01', '2025-06-08', N'Gereserveerd');

-- Gebruik SCOPE_IDENTITY() om het nieuwe ReserveringID op te halen
SET @NewReserveringID = SCOPE_IDENTITY();

-- Detail: Het kostte €200 per nacht op dat moment
INSERT INTO dbo.RESERVERING_DETAIL (ReserveringID, CategorieID, Aantal, PrijsOpMoment) 
VALUES (@NewReserveringID, 1, 7, 200.00);
GO

-- ========================================================
-- 4. VERIFICATIE QUERIES (Optioneel - voor validatie)
-- ========================================================
/*
-- Controleer alle data
SELECT 'ACCOMMODATIE_TYPE' AS Tabel, COUNT(*) AS Aantal FROM dbo.ACCOMMODATIE_TYPE
UNION ALL SELECT 'PLATFORM', COUNT(*) FROM dbo.PLATFORM
UNION ALL SELECT 'TARIEF_CATEGORIE', COUNT(*) FROM dbo.TARIEF_CATEGORIE
UNION ALL SELECT 'GAST', COUNT(*) FROM dbo.GAST
UNION ALL SELECT 'GEBRUIKER', COUNT(*) FROM dbo.GEBRUIKER
UNION ALL SELECT 'VERHUUR_EENHEID', COUNT(*) FROM dbo.VERHUUR_EENHEID
UNION ALL SELECT 'TARIEF', COUNT(*) FROM dbo.TARIEF
UNION ALL SELECT 'RESERVERING', COUNT(*) FROM dbo.RESERVERING
UNION ALL SELECT 'RESERVERING_DETAIL', COUNT(*) FROM dbo.RESERVERING_DETAIL;

-- Controleer de hiërarchie
SELECT 
    Parent.EenheidID AS ParentID,
    Parent.Naam AS ParentNaam,
    Child.EenheidID AS ChildID,
    Child.Naam AS ChildNaam
FROM dbo.VERHUUR_EENHEID Parent
LEFT JOIN dbo.VERHUUR_EENHEID Child ON Parent.EenheidID = Child.ParentEenheidID
WHERE Parent.TypeID = 1
ORDER BY Child.EenheidID;

-- Controleer reservering met details
SELECT 
    r.ReserveringID,
    g.Naam AS Gast,
    ve.Naam AS Eenheid,
    p.Naam AS Platform,
    r.Startdatum,
    r.Einddatum,
    r.Status,
    rd.Aantal,
    rd.PrijsOpMoment,
    tc.Naam AS Categorie
FROM dbo.RESERVERING r
INNER JOIN dbo.GAST g ON r.GastID = g.GastID
INNER JOIN dbo.VERHUUR_EENHEID ve ON r.EenheidID = ve.EenheidID
INNER JOIN dbo.PLATFORM p ON r.PlatformID = p.PlatformID
LEFT JOIN dbo.RESERVERING_DETAIL rd ON r.ReserveringID = rd.ReserveringID
LEFT JOIN dbo.TARIEF_CATEGORIE tc ON rd.CategorieID = tc.CategorieID;
*/

-- Einde Script
PRINT N'✅ Database script succesvol uitgevoerd!';
PRINT N'📊 Tabellen aangemaakt: 10';
PRINT N'🏠 Verhuur eenheden: 10 (1 Parent + 9 Children)';
PRINT N'📅 Test reservering aangemaakt voor 2025-06-01 t/m 2025-06-08';
GO
