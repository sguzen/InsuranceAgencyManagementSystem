using static System.Runtime.InteropServices.JavaScript.JSType;

namespace IAMS.Domain.ValueObjects
{
    public class DateRange : IEquatable<DateRange>
    {
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }

        public DateRange(DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
                throw new ArgumentException("Start date cannot be greater than end date");

            StartDate = startDate.Date;
            EndDate = endDate.Date;
        }

        public int DurationInDays => (EndDate - StartDate).Days + 1;

        public int DurationInMonths
        {
            get
            {
                var months = (EndDate.Year - StartDate.Year) * 12 + EndDate.Month - StartDate.Month;
                if (EndDate.Day < StartDate.Day)
                    months--;
                return months;
            }
        }

        public int DurationInYears => DurationInMonths / 12;

        public bool IsActive => DateTime.Today.Date >= StartDate && DateTime.Today.Date <= EndDate;

        public bool Contains(DateTime date)
        {
            return date.Date >= StartDate && date.Date <= EndDate;
        }

        public bool Overlaps(DateRange other)
        {
            return StartDate <= other.EndDate && EndDate >= other.StartDate;
        }

        public bool IsExpired => EndDate < DateTime.Today.Date;

        public bool IsFuture => StartDate > DateTime.Today.Date;

        public DateRange Extend(int days)
        {
            return new DateRange(StartDate, EndDate.AddDays(days));
        }

        public DateRange ExtendToEndOfMonth()
        {
            var lastDayOfMonth = new DateTime(EndDate.Year, EndDate.Month, DateTime.DaysInMonth(EndDate.Year, EndDate.Month));
            return new DateRange(StartDate, lastDayOfMonth);
        }

        public DateRange ExtendToEndOfYear()
        {
            var lastDayOfYear = new DateTime(EndDate.Year, 12, 31);
            return new DateRange(StartDate, lastDayOfYear);
        }

        public IEnumerable<DateTime> GetDatesInRange()
        {
            for (var date = StartDate; date <= EndDate; date = date.AddDays(1))
            {
                yield return date;
            }
        }

        public IEnumerable<DateTime> GetMonthEndsInRange()
        {
            var current = new DateTime(StartDate.Year, StartDate.Month, DateTime.DaysInMonth(StartDate.Year, StartDate.Month));

            while (current <= EndDate)
            {
                yield return current;
                current = current.AddDays(1);
                current = new DateTime(current.Year, current.Month, DateTime.DaysInMonth(current.Year, current.Month));
            }
        }

        public bool Equals(DateRange? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            return StartDate == other.StartDate && EndDate == other.EndDate;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as DateRange);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(StartDate, EndDate);
        }

        public static bool operator ==(DateRange? left, DateRange? right)
        {
            return EqualityComparer<DateRange>.Default.Equals(left, right);
        }

        public static bool operator !=(DateRange? left, DateRange? right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return $"{StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd}";
        }

        public string ToString(string format)
        {
            return $"{StartDate.ToString(format)} to {EndDate.ToString(format)}";
        }

        public static DateRange CreateMonthRange(int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = new DateTime(year, month, DateTime.DaysInMonth(year, month));
            return new DateRange(startDate, endDate);
        }

        public static DateRange CreateYearRange(int year)
        {
            var startDate = new DateTime(year, 1, 1);
            var endDate = new DateTime(year, 12, 31);
            return new DateRange(startDate, endDate);
        }

        public static DateRange CreateCurrentMonth()
        {
            var today = DateTime.Today;
            return CreateMonthRange(today.Year, today.Month);
        }

        public static DateRange CreateCurrentYear()
        {
            return CreateYearRange(DateTime.Today.Year);
        }
    }
}