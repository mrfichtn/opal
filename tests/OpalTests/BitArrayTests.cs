using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using BitArray = Opal.Containers.BitArray;

namespace OpalTests
{
    [TestClass]
    public class BitArrayTests
    {
        [TestMethod]
        public void BitArray_Set()
        {
            var array = new BitArray(32);
            array.SetBit(0);
            Assert.IsTrue(array.GetBit(0));
            Assert.IsFalse(array.GetBit(1));
        }

        [TestMethod]
        public void BitArray_Clr()
        {
            var array = new BitArray(32);
            array.SetBit(0);
            array.SetBit(1);
            Assert.IsTrue(array.GetBit(0));
            Assert.IsTrue(array.GetBit(1));
            array.ClrBit(0);
            Assert.IsFalse(array.GetBit(0));
            Assert.IsTrue(array.GetBit(1));
        }

        [TestMethod]
        public void BitArray_Invert()
        {
            var array = new BitArray(32);
            array.Invert();
            Assert.IsTrue(array.GetBit(0));
            Assert.IsTrue(array.GetBit(1));

            array = ~array;
            Assert.IsFalse(array.GetBit(0));
            Assert.IsFalse(array.GetBit(1));
        }

        [TestMethod]
        public void BitArray_OrFrom()
        {
            var left = Stripe1();
            var right = Stripe2();

            left.OrFrom(right);
            Assert.IsFalse(left[0]);
            Assert.IsTrue(left[1]);
            Assert.IsTrue(left[2]);
            Assert.IsTrue(left[3]);
        }

        [TestMethod]
        public void BitArray_OrNotFrom()
        {
            var left = Stripe1();
            var right = Stripe2();

            left.OrNotFrom(right);
            Assert.IsTrue(left[0]);
            Assert.IsTrue(left[1]);
            Assert.IsFalse(left[2]);
            Assert.IsTrue(left[3]);
        }

        [TestMethod]
        public void BitArray_AndFrom()
        {
            var left = Stripe1();
            var right = Stripe2();

            left.AndFrom(right);
            Assert.IsFalse(left[0]);
            Assert.IsFalse(left[1]);
            Assert.IsFalse(left[2]);
            Assert.IsTrue(left[3]);
        }

        [TestMethod]
        public void BitArray_AndNotFrom()
        {
            var left = Stripe1();
            var right = Stripe2();

            left.AndNotFrom(right);
            Assert.IsFalse(left[0]);
            Assert.IsTrue(left[1]);
            Assert.IsFalse(left[2]);
            Assert.IsFalse(left[3]);
        }

        [TestMethod]
        public void BitArray_BitCount()
        {
            var array = new BitArray(32);
            Assert.AreEqual(0, array.BitCount());
            for (var i = 0; i < array.Length; i++)
            {
                array.SetBit(i);
                Assert.AreEqual(i + 1, array.BitCount());
            }

            for (var i = array.Length-1; i >= 0; i--)
            {
                array.ClrBit(i);
                Assert.AreEqual(i, array.BitCount());
            }
        }

        [TestMethod]
        public void BitArray_SetAll()
        {
            var array = new BitArray(32);
            array.SetAll();
            Assert.IsTrue(array[0]);
            Assert.IsTrue(array[1]);
            Assert.IsTrue(array[2]);
        }

        [TestMethod]
        public void BitArray_SetAddresses()
        {
            var array = new BitArray(64);
            array[0] = true;
            array[34] = true;

            var result = array.SetAddresses.ToArray();
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual(0, result[0]);
            Assert.AreEqual(34, result[1]);
        }

        [TestMethod]
        public void BitArray_ClearedAddresses()
        {
            var array = new BitArray(32);
            array[1] = true;

            var result = array.ClearedAddresses.ToArray();
            Assert.AreEqual(31, result.Length);
            Assert.AreEqual(0, result[0]);
            Assert.AreEqual(2, result[1]);
        }

        [TestMethod]
        public void BitArray_Equals()
        {
            var array = new BitArray(32);
            array[1] = true;

            var right = new BitArray(32);

            Assert.IsFalse(array.Equals(right));
            
            right[1] = true;

            Assert.IsTrue(array.Equals(right));
        }

        [TestMethod]
        public void BitArray_IsInverseOf()
        {
            var array = new BitArray(32);
            array[1] = true;

            var right = new BitArray(32);
            Assert.IsFalse(array.IsInverseOf(right));
            
            right[1] = true;
            Assert.IsFalse(array.IsInverseOf(right));
            right.Invert();
            Assert.IsTrue(array.IsInverseOf(right));
        }


        private static BitArray Stripe1()
        {
            var left = new BitArray(32);
            left[1] = true;
            left[3] = true;
            return left;
        }

        private static BitArray Stripe2()
        {
            var right = new BitArray(32);
            right[2] = true;
            right[3] = true;
            return right;
        }

    }
}
