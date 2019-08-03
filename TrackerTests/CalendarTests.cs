using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tracker;
using System;


namespace Tracker.Tests {
    [TestClass]
    public class IntervalTests {
        [TestMethod]
        public void test_compare() {
            Decimal[] nums = { -1000, -5.823m, 0, 83.4m, 90210 };
            Interval nada = null;

            for (int i = 0; i < nums.Length; i++) {
                Interval i1 = new Interval(nums[i]);

                Assert.IsFalse(i1 == nada);
                Assert.IsTrue(i1 != nada);

                for (int j = 0; j < nums.Length; j++) {
                    Interval i2 = new Interval(nums[j]);

                    Assert.AreEqual(i1 == i2, nums[i] == nums[j]);
                    Assert.AreEqual(i1 != i2, nums[i] != nums[j]);
                }
            }
        }
    }


    [TestClass]
    public class TimestampTests {
        [TestMethod]
        public void test_compare() {
            Decimal[] nums = { 42, 0, -100, 10.7m, -1.23m };
            Timestamp nada = null;

            for (int i = 0; i < nums.Length; i++) {
                Timestamp t1 = new Timestamp(nums[i]);

                Assert.IsFalse(t1 > nada);
                Assert.IsFalse(t1 >= nada);
                Assert.IsFalse(t1 == nada);
                Assert.IsTrue(t1 != nada);
                Assert.IsFalse(t1 <= nada);
                Assert.IsFalse(t1 < nada);

#pragma warning disable CS1718 // Comparison made to same variable
                Assert.IsFalse(t1 > t1);
                Assert.IsTrue(t1 >= t1);
                Assert.IsTrue(t1 == t1);
                Assert.IsFalse(t1 != t1);
                Assert.IsTrue(t1 <= t1);
                Assert.IsFalse(t1 < t1);
#pragma warning restore CS1718 // Comparison made to same variable

                for (int j = 0; j < nums.Length; j++) {
                    Timestamp t2 = new Timestamp(nums[j]);

                    Assert.AreEqual(t1 > t2, nums[i] > nums[j]);
                    Assert.AreEqual(t1 >= t2, nums[i] >= nums[j]);
                    Assert.AreEqual(t1 == t2, nums[i] == nums[j]);
                    Assert.AreEqual(t1 != t2, nums[i] != nums[j]);
                    Assert.AreEqual(t1 <= t2, nums[i] <= nums[j]);
                    Assert.AreEqual(t1 < t2, nums[i] < nums[j]);
                    Assert.AreEqual(t1.CompareTo(t2), nums[i].CompareTo(nums[j]));
                }
            }
        }

        [TestMethod]
        public void test_arithmetic() {
            Decimal[] nums = { 1.23m, -456, 7890, 0, -3.14m };
            Timestamp nada = null;
            Interval nullInterval = null;

            for (int i = 0; i < nums.Length; i++) {
                Timestamp t1 = new Timestamp(nums[i]);
                Interval i1 = new Interval(nums[i]);

                Assert.IsNull(t1 + nullInterval);
                Assert.IsNull(t1 - nullInterval);
                Assert.IsNull(nada + i1);
                Assert.IsNull(nada - i1);
                Assert.IsNull(t1 - nada);
                Assert.IsNull(nada - t1);

                for (int j = 0; j < nums.Length; j++) {
                    Timestamp t2 = new Timestamp(nums[j]);
                    Interval i2 = new Interval(nums[j]);

                    Assert.AreEqual(t1 + i2, new Timestamp(nums[i] + nums[j]));
                    Assert.AreEqual(t1 - i2, new Timestamp(nums[i] - nums[j]));
                    Interval isub = t1 - t2, newi = new Interval(nums[i] - nums[j]);
                    Assert.AreEqual(isub.value, newi.value);
                    Assert.IsTrue(isub.value == newi.value);
                    Assert.AreEqual(isub, newi);
                    Assert.AreEqual(t1 - t2, new Interval(nums[i] - nums[j]));
                    Assert.AreEqual(t2 - t1, new Interval(nums[j] - nums[i]));
                }
            }
        }
    }
}