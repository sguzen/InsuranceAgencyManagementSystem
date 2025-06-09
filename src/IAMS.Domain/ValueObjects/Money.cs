using System.Globalization;

namespace IAMS.Domain.ValueObjects
{
    public class Money : IEquatable<Money>, IComparable<Money>
    {
        public decimal Amount { get; private set; }
        public string Currency { get; private set; }

        public Money(decimal amount, string currency = "TRY")
        {
            if (amount < 0)
                throw new ArgumentException("Amount cannot be negative", nameof(amount));
            if (string.IsNullOrWhiteSpace(currency))
                throw new ArgumentException("Currency cannot be empty", nameof(currency));

            Amount = Math.Round(amount, 2);
            Currency = currency.ToUpperInvariant();
        }

        public static Money Zero(string currency = "TRY") => new Money(0, currency);

        public Money Add(Money other)
        {
            if (Currency != other.Currency)
                throw new InvalidOperationException($"Cannot add different currencies: {Currency} and {other.Currency}");

            return new Money(Amount + other.Amount, Currency);
        }

        public Money Subtract(Money other)
        {
            if (Currency != other.Currency)
                throw new InvalidOperationException($"Cannot subtract different currencies: {Currency} and {other.Currency}");

            return new Money(Amount - other.Amount, Currency);
        }

        public Money Multiply(decimal multiplier)
        {
            return new Money(Amount * multiplier, Currency);
        }

        public Money Divide(decimal divisor)
        {
            if (divisor == 0)
                throw new DivideByZeroException("Cannot divide by zero");

            return new Money(Amount / divisor, Currency);
        }

        public decimal CalculatePercentage(decimal percentage)
        {
            return Amount * (percentage / 100);
        }

        public Money ApplyPercentage(decimal percentage)
        {
            return new Money(Amount * (percentage / 100), Currency);
        }

        public bool Equals(Money? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            return Amount == other.Amount && Currency == other.Currency;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Money);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Amount, Currency);
        }

        public int CompareTo(Money? other)
        {
            if (other is null) return 1;
            if (Currency != other.Currency)
                throw new InvalidOperationException($"Cannot compare different currencies: {Currency} and {other.Currency}");

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
            return $"{Amount:N2} {Currency}";
        }

        public string ToString(string format)
        {
            return $"{Amount.ToString(format)} {Currency}";
        }

        public string ToString(CultureInfo culture)
        {
            return $"{Amount.ToString("N2", culture)} {Currency}";
        }
    }
}