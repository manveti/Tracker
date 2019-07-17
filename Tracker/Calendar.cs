using System;


namespace Tracker {
    /// <summary>
    /// Generic time interval, to be interpreted by a <see cref="Calendar"/>.
    /// </summary>
    [Serializable]
    public class Interval {
        public readonly Decimal value;

        public Interval(Decimal value) {
            this.value = value;
        }
    }


    /// <summary>
    /// Generic timestamp, to be interpreted by a <see cref="Calendar"/>.
    /// </summary>
    [Serializable]
    public class Timestamp : IComparable {
        public readonly Decimal value;

        public Timestamp(Decimal value) {
            this.value = value;
        }

        public virtual int CompareTo(Object obj) {
            if (obj == null) { return 1; }
            return this.value.CompareTo(((Timestamp)obj).value);
        }

        public override bool Equals(object obj) {
            if (obj == null) { return false; }
            return this.value.Equals(((Timestamp)obj).value);
        }

        public override int GetHashCode() {
            return this.value.GetHashCode();
        }

        public static bool operator <(Timestamp t1, Timestamp t2) {
            if ((t1 == null) || (t2 == null)) { return false; }
            return t1.CompareTo(t2) < 0;
        }

        public static bool operator <=(Timestamp t1, Timestamp t2) {
            if ((t1 == null) || (t2 == null)) { return false; }
            return t1.CompareTo(t2) <= 0;
        }

        public static bool operator ==(Timestamp t1, Timestamp t2) {
            if (ReferenceEquals(t1, t2)) { return true; }
            return t1.CompareTo(t2) == 0;
        }

        public static bool operator !=(Timestamp t1, Timestamp t2) {
            if (ReferenceEquals(t1, t2)) { return true; }
            return t1.CompareTo(t2) != 0;
        }

        public static bool operator >=(Timestamp t1, Timestamp t2) {
            if ((t1 == null) || (t2 == null)) { return false; }
            return t1.CompareTo(t2) >= 0;
        }

        public static bool operator >(Timestamp t1, Timestamp t2) {
            if ((t1 == null) || (t2 == null)) { return false; }
            return t1.CompareTo(t2) > 0;
        }

        public static Timestamp operator +(Timestamp t, Interval amount) {
            if ((t == null) || (amount == null)) { return null; }
            return new Timestamp(t.value + amount.value);
        }

        public static Timestamp operator -(Timestamp t, Interval amount) {
            if ((t == null) || (amount == null)) { return null; }
            return new Timestamp(t.value - amount.value);
        }

        public static Interval operator -(Timestamp t1, Timestamp t2) {
            if ((t1 == null) || (t2 == null)) { return null; }
            return new Interval(t1.value - t2.value);
        }
    }


    public abstract class Calendar {
    }
}
