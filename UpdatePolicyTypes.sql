-- Migration script to update PolicyTypes table with new insurance types
-- This script handles foreign key constraints by mapping old policy types to new ones where applicable

BEGIN TRANSACTION;

-- Step 1: Create a temporary mapping table for old to new policy types
CREATE TABLE #PolicyTypeMapping (
    OldId INT,
    OldCode NVARCHAR(50),
    NewId INT,
    NewCode NVARCHAR(50)
);

-- Insert mappings for policy types that exist in both old and new structure
INSERT INTO #PolicyTypeMapping (OldId, OldCode, NewId, NewCode) VALUES
    (14, 'YAN', 1, '01'),      -- Yangın Sigortası
    (1, 'TRF', 4, '15'),        -- Trafik Sigortası -> Zorunlu Trafik
    (2, 'KAS', 6, '17'),        -- Kasko Sigortası
    (10, 'FK', 7, '19'),        -- Ferdi Kaza Sigortası
    (15, 'HIR', 12, '24'),      -- Hırsızlık Sigortası
    (11, 'SEY', 15, '34'),      -- Seyahat Sağlık Sigortası
    (12, 'MMS', 18, '37'),      -- Mesleki Mali Sorumluluk
    (13, 'USS', 9, '21');       -- Üçüncü Şahıs Mali Sorumluluk

-- Step 2: Update Policies table to use new PolicyType IDs
UPDATE P
SET P.PolicyTypeId = M.NewId
FROM Policies P
INNER JOIN #PolicyTypeMapping M ON P.PolicyTypeId = M.OldId;

-- Step 3: Update CommissionRates table to use new PolicyType IDs
UPDATE CR
SET CR.PolicyTypeId = M.NewId
FROM CommissionRates CR
INNER JOIN #PolicyTypeMapping M ON CR.PolicyTypeId = M.OldId;

-- Step 4: Handle policies and commission rates that don't have a mapping
-- Option A: Delete them (uncomment if you want to delete orphaned records)
-- DELETE FROM CommissionRates WHERE PolicyTypeId NOT IN (SELECT NewId FROM #PolicyTypeMapping);
-- DELETE FROM Policies WHERE PolicyTypeId NOT IN (SELECT NewId FROM #PolicyTypeMapping);

-- Option B: Set them to NULL temporarily (uncomment if you want to preserve records)
-- UPDATE Policies SET PolicyTypeId = NULL WHERE PolicyTypeId NOT IN (SELECT NewId FROM #PolicyTypeMapping);
-- UPDATE CommissionRates SET PolicyTypeId = NULL WHERE PolicyTypeId NOT IN (SELECT NewId FROM #PolicyTypeMapping);

-- Option C: Display warning about unmapped records (recommended for first run)
PRINT 'Checking for unmapped Policies...';
SELECT COUNT(*) AS UnmappedPolicies
FROM Policies
WHERE PolicyTypeId NOT IN (SELECT NewId FROM #PolicyTypeMapping);

PRINT 'Checking for unmapped CommissionRates...';
SELECT COUNT(*) AS UnmappedCommissionRates
FROM CommissionRates
WHERE PolicyTypeId NOT IN (SELECT NewId FROM #PolicyTypeMapping);

-- Step 5: Delete old PolicyTypes
DELETE FROM PolicyTypes;

-- Step 6: Reset identity seed
DBCC CHECKIDENT ('PolicyTypes', RESEED, 0);

-- Step 7: Insert new PolicyTypes
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

-- Step 8: Clean up temporary table
DROP TABLE #PolicyTypeMapping;

-- Review changes before committing
PRINT 'Migration completed successfully!';
PRINT 'Review the changes and if everything looks good, run: COMMIT TRANSACTION';
PRINT 'If there are issues, run: ROLLBACK TRANSACTION';

-- Uncomment the following line to automatically commit
-- COMMIT TRANSACTION;

