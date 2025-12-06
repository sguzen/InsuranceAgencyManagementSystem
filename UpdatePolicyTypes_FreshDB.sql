-- Simple migration script for FRESH databases with no existing policies
-- WARNING: This script will delete ALL policies and commission rates
-- Only use this if you're okay with losing existing data

BEGIN TRANSACTION;

-- Step 1: Delete all dependent records first
DELETE FROM CommissionRates;
DELETE FROM Policies;

-- Step 2: Delete old PolicyTypes
DELETE FROM PolicyTypes;

-- Step 3: Reset identity seed
DBCC CHECKIDENT ('PolicyTypes', RESEED, 0);

-- Step 4: Insert new PolicyTypes
SET IDENTITY_INSERT PolicyTypes ON;

INSERT INTO PolicyTypes (Id, Code, Name, IsActive, CreatedOn, CreatedBy) VALUES
    (1, '01', 'YANGIN', 1, '2024-01-01', 'System'),
    (2, '02', 'TİCARİ PAKET', 1, '2024-01-01', 'System'),
    (3, '10', 'NAKLİYAT EMTEA', 1, '2024-01-01', 'System'),
    (4, '15', 'ZORUNLU TRAFİK', 1, '2024-01-01', 'System'),
    (5, '16', 'KISMI KASKO', 1, '2024-01-01', 'System'),
    (6, '17', 'KASKO', 1, '2024-01-01', 'System'),
    (7, '19', 'FERDİ KAZA', 1, '2024-01-01', 'System'),
    (8, '20', 'İŞVEREN MALİ SORUMLULUK', 1, '2024-01-01', 'System'),
    (9, '21', '3.ŞAHIS MALİ SORUMLULUK', 1, '2024-01-01', 'System'),
    (10, '22', 'ASANSÖR MALİ SORUMLULUK', 1, '2024-01-01', 'System'),
    (11, '23', 'CAM KIRILMASI', 1, '2024-01-01', 'System'),
    (12, '24', 'HIRSIZLIK', 1, '2024-01-01', 'System'),
    (13, '32', 'EMNİYETİ SUİSTİMAL', 1, '2024-01-01', 'System'),
    (14, '33', 'TAŞINAN PARA', 1, '2024-01-01', 'System'),
    (15, '34', 'SEYAHAT SAĞLIK', 1, '2024-01-01', 'System'),
    (16, '35', 'KREDİ KARTI', 1, '2024-01-01', 'System'),
    (17, '36', 'HAYAT KREDİ', 1, '2024-01-01', 'System'),
    (18, '37', 'MESLEKİ SORUMLULUK', 1, '2024-01-01', 'System'),
    (19, '39', 'TEHLİKELİ HASTALIKLAR', 1, '2024-01-01', 'System'),
    (20, '46', 'EĞİTİM GÜVENCESİ', 1, '2024-01-01', 'System'),
    (21, '65', 'MAKİNA KIRILMASI', 1, '2024-01-01', 'System'),
    (22, '66', 'MONTAJ', 1, '2024-01-01', 'System'),
    (23, '67', 'İNŞAAT', 1, '2024-01-01', 'System'),
    (24, '68', 'ELEKTRONİK CİHAZ', 1, '2024-01-01', 'System');

SET IDENTITY_INSERT PolicyTypes OFF;

PRINT 'Migration completed successfully!';
PRINT 'New PolicyTypes have been inserted.';

COMMIT TRANSACTION;
