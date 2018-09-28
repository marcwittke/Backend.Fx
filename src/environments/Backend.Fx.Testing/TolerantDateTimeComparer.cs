namespace Backend.Fx.Testing
{
    using System;
    using System.Collections.Generic;

    public class TolerantDateTimeOffsetComparer : IEqualityComparer<DateTimeOffset?>
    {
        private readonly TimeSpan _epsilon;

        public TolerantDateTimeOffsetComparer(TimeSpan epsilon)
        {
            this._epsilon = epsilon;
        }

        public bool Equals(DateTimeOffset? x, DateTimeOffset? y)
        {
            if (x == null && y == null) return true;

            if (x == null || y == null) return false;

            return Math.Abs((x.Value - y.Value).TotalMilliseconds) < _epsilon.TotalMilliseconds;
        }

        public int GetHashCode(DateTimeOffset? obj)
        {
            return obj?.GetHashCode() ?? 0;
        }
    }

    public class TolerantDateTimeComparer : IEqualityComparer<DateTime?>
    {
        private readonly TimeSpan _epsilon;

        public TolerantDateTimeComparer(TimeSpan epsilon)
        {
            this._epsilon = epsilon;
        }

        public bool Equals(DateTime? x, DateTime? y)
        {
            if (x == null && y == null) return true;

            if (x == null || y == null) return false;

            return Math.Abs((x.Value - y.Value).TotalMilliseconds) < _epsilon.TotalMilliseconds;
        }

        public int GetHashCode(DateTime? obj)
        {
            return obj?.GetHashCode() ?? 0;
        }
    }
}
