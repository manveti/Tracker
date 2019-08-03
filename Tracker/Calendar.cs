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

        public override bool Equals(object obj) {
            if (obj is null) { return false; }
            return this.value.Equals(((Interval)obj).value);
        }

        public static bool operator ==(Interval i1, Interval i2) {
            if (ReferenceEquals(i1, i2)) { return true; }
            if ((i1 is null) || (i2 is null)) { return false; }
            return i1.value == i2.value;
        }

        public static bool operator !=(Interval i1, Interval i2) {
            if (ReferenceEquals(i1, i2)) { return false; }
            if ((i1 is null) || (i2 is null)) { return true; }
            return i1.value != i2.value;
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
            if (obj is null) { return 1; }
            return this.value.CompareTo(((Timestamp)obj).value);
        }

        public override bool Equals(object obj) {
            if (obj is null) { return false; }
            return this.value.Equals(((Timestamp)obj).value);
        }

        public override int GetHashCode() {
            return this.value.GetHashCode();
        }

        public static bool operator <(Timestamp t1, Timestamp t2) {
            if ((t1 is null) || (t2 is null)) { return false; }
            return t1.CompareTo(t2) < 0;
        }

        public static bool operator <=(Timestamp t1, Timestamp t2) {
            if ((t1 is null) || (t2 is null)) { return false; }
            return t1.CompareTo(t2) <= 0;
        }

        public static bool operator ==(Timestamp t1, Timestamp t2) {
            if (ReferenceEquals(t1, t2)) { return true; }
            return t1.CompareTo(t2) == 0;
        }

        public static bool operator !=(Timestamp t1, Timestamp t2) {
            if (ReferenceEquals(t1, t2)) { return false; }
            return t1.CompareTo(t2) != 0;
        }

        public static bool operator >=(Timestamp t1, Timestamp t2) {
            if ((t1 is null) || (t2 is null)) { return false; }
            return t1.CompareTo(t2) >= 0;
        }

        public static bool operator >(Timestamp t1, Timestamp t2) {
            if ((t1 is null) || (t2 is null)) { return false; }
            return t1.CompareTo(t2) > 0;
        }

        public static Timestamp operator +(Timestamp t, Interval amount) {
            if ((t is null) || (amount is null)) { return null; }
            return new Timestamp(t.value + amount.value);
        }

        public static Timestamp operator -(Timestamp t, Interval amount) {
            if ((t is null) || (amount is null)) { return null; }
            return new Timestamp(t.value - amount.value);
        }

        public static Interval operator -(Timestamp t1, Timestamp t2) {
            if ((t1 is null) || (t2 is null)) { return null; }
            return new Interval(t1.value - t2.value);
        }
    }


    public abstract class Calendar {
    }
}
