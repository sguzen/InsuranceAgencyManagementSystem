using System.Globalization;

namespace IAMS.Domain.ValueObjects
{
    public class Money : IEquatable<Money>, IComparable<Money>
    {
        public decimal Amount { get; private set; }
        public int CurrencyId { get; private set; }

        // Constructor with CurrencyId
        public Money(decimal amount, int currencyId)
        {
            if (amount < 0)
                throw new ArgumentException("Amount cannot be negative", nameof(amount));
            if (currencyId <= 0)
                throw new ArgumentException("CurrencyId must be greater than 0", nameof(currencyId));

            Amount = Math.Round(amount, 2);
            CurrencyId = currencyId;
        }

        // Backward compatibility: keep old constructor but marked as obsolete
        [Obsolete("Use Money(decimal amount, int currencyId) instead. This will be removed in future versions.")]
        public Money(decimal amount, string currency = "TRY")
        {
            // Default mapping: TRY=1, USD=2, EUR=3, GBP=4
            var currencyId = currency.ToUpperInvariant() switch
            {
                "TRY" => 1,
                "USD" => 2,
                "EUR" => 3,
                "GBP" => 4,
                _ => 1
            };

            if (amount < 0)
                throw new ArgumentException("Amount cannot be negative", nameof(amount));

            Amount = Math.Round(amount, 2);
            CurrencyId = currencyId;
        }

        public static Money Zero(int currencyId = 1) => new Money(0, currencyId);

        public Money Add(Money other)
        {
            if (CurrencyId != other.CurrencyId)
                throw new InvalidOperationException($"Cannot add different currencies: {CurrencyId} and {other.CurrencyId}");

            return new Money(Amount + other.Amount, CurrencyId);
        }

        public Money Subtract(Money other)
        {
            if (CurrencyId != other.CurrencyId)
                throw new InvalidOperationException($"Cannot subtract different currencies: {CurrencyId} and {other.CurrencyId}");

            return new Money(Amount - other.Amount, CurrencyId);
        }

        public Money Multiply(decimal multiplier)
        {
            return new Money(Amount * multiplier, CurrencyId);
        }

        public Money Divide(decimal divisor)
        {
            if (divisor == 0)
                throw new DivideByZeroException("Cannot divide by zero");

            return new Money(Amount / divisor, CurrencyId);
        }

        public decimal CalculatePercentage(decimal percentage)
        {
            return Amount * (percentage / 100);
        }

        public Money ApplyPercentage(decimal percentage)
        {
            return new Money(Amount * (percentage / 100), CurrencyId);
        }

        public bool Equals(Money? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            return Amount == other.Amount && CurrencyId == other.CurrencyId;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Money);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Amount, CurrencyId);
        }

        public int CompareTo(Money? other)
        {
            if (other is null) return 1;
            if (CurrencyId != other.CurrencyId)
                throw new InvalidOperationException($"Cannot compare different currencies: {CurrencyId} and {other.CurrencyId}");

            return Amount.CompareTo(other.Amount);
        }

        public static bool operator ==(Money? left, Money? right)
        {
            return EqualityComparer<Money>.Default.Equals(left, right);
        }

        public static bool operator !=(Money? left, Money? right)
        {
            return !(left == right);
        }

        public static bool operator <(Money left, Money right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator <=(Money left, Money right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >(Money left, Money right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator >=(Money left, Money right)
        {
            return left.CompareTo(right) >= 0;
        }

        public static Money operator +(Money left, Money right)
        {
            return left.Add(right);
        }

        public static Money operator -(Money left, Money right)
        {
            return left.Subtract(right);
        }

        public static Money operator *(Money money, decimal multiplier)
        {
            return money.Multiply(multiplier);
        }

        public static Money operator /(Money money, decimal divisor)
        {
            return money.Divide(divisor);
        }

        public override string ToString()
        {
            return $"{Amount:N2}"; // Currency symbol should be added by presentation layer
        }

        public string ToString(string format)
        {
            return Amount.ToString(format);
        }

        public string ToString(CultureInfo culture)
        {
            return Amount.ToString("N2", culture);
        }
    }
}