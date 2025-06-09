using System.Collections.Generic;

namespace IAMS.Domain.ValueObjects
{
    public class Address : IEquatable<Address>
    {
        public string Street { get; private set; }
        public string City { get; private set; }
        public string District { get; private set; }
        public string PostalCode { get; private set; }
        public string Country { get; private set; }

        public Address(string street, string city, string district, string postalCode, string country)
        {
            if (string.IsNullOrWhiteSpace(street))
                throw new ArgumentException("Street cannot be empty", nameof(street));
            if (string.IsNullOrWhiteSpace(city))
                throw new ArgumentException("City cannot be empty", nameof(city));
            if (string.IsNullOrWhiteSpace(country))
                throw new ArgumentException("Country cannot be empty", nameof(country));

            Street = street;
            City = city;
            District = district ?? string.Empty;
            PostalCode = postalCode ?? string.Empty;
            Country = country;
        }

        public string GetFullAddress()
        {
            var parts = new List<string> { Street };

            if (!string.IsNullOrWhiteSpace(District))
                parts.Add(District);

            parts.Add(City);

            if (!string.IsNullOrWhiteSpace(PostalCode))
                parts.Add(PostalCode);

            parts.Add(Country);

            return string.Join(", ", parts);
        }

        public bool Equals(Address? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            return Street == other.Street &&
                   City == other.City &&
                   District == other.District &&
                   PostalCode == other.PostalCode &&
                   Country == other.Country;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Address);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Street, City, District, PostalCode, Country);
        }

        public static bool operator ==(Address? left, Address? right)
        {
            return EqualityComparer<Address>.Default.Equals(left, right);
        }

        public static bool operator !=(Address? left, Address? right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return GetFullAddress();
        }
    }
}