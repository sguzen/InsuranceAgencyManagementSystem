-- =====================================================
-- Migrate Existing PolicyPayments to CustomerPayments
-- =====================================================
-- This script creates CustomerPayment records for all
-- existing PolicyPayments to implement the dual-write
-- pattern (Option B).
--
-- Run this ONCE to sync historical data.
-- =====================================================

BEGIN TRANSACTION;

-- Insert CustomerPayments from PolicyPayments
INSERT INTO CustomerPayments (
    CustomerId,
    Amount,
    PaymentDate,
    DueDate,
    PaymentMethod,
    Status,
    CurrencyId,
    Reference,
    Notes,
    AllocationStatus,
    CreatedOn,
    ModifiedOn,
    CreatedBy,
    ModifiedBy,
    IsDeleted
)
SELECT
    p.CustomerId,
    pp.Amount,
    pp.PaymentDate,
    pp.DueDate,
    pp.PaymentMethod,
    pp.Status,
    pp.CurrencyId,
    pp.Reference,
    CONCAT('Migrated: Payment for Policy ', pol.PolicyNumber,
           CASE WHEN pp.Notes IS NOT NULL THEN CONCAT(' - ', pp.Notes) ELSE '' END),
    1, -- AllocationStatus: FullyAllocated (since it's from a PolicyPayment)
    pp.CreatedOn,
    pp.ModifiedOn,
    pp.CreatedBy,
    pp.ModifiedBy,
    pp.IsDeleted
FROM PolicyPayments pp
INNER JOIN Policies pol ON pp.PolicyId = pol.Id
INNER JOIN Customers p ON pol.CustomerId = p.Id
WHERE NOT EXISTS (
    -- Don't insert duplicates if script is run multiple times
    SELECT 1 FROM CustomerPayments cp
    WHERE cp.CustomerId = p.CustomerId
      AND cp.Amount = pp.Amount
      AND cp.PaymentDate = pp.PaymentDate
      AND cp.CurrencyId = pp.CurrencyId
      AND cp.Reference = pp.Reference
);

-- Show migration summary
SELECT
    COUNT(*) AS TotalPolicyPayments,
    SUM(CASE WHEN pp.Status = 1 THEN 1 ELSE 0 END) AS CompletedPayments,
    SUM(pp.Amount) AS TotalAmount
FROM PolicyPayments pp
WHERE pp.IsDeleted = 0;

SELECT
    COUNT(*) AS TotalCustomerPayments,
    SUM(CASE WHEN cp.Status = 1 THEN 1 ELSE 0 END) AS CompletedPayments,
    SUM(cp.Amount) AS TotalAmount
FROM CustomerPayments cp
WHERE cp.IsDeleted = 0;

COMMIT TRANSACTION;

PRINT 'Migration completed successfully!';
PRINT 'PolicyPayments have been synced to CustomerPayments.';
PRINT 'New payments will automatically create both records.';
