using Microsoft.VisualStudio.TestTools.UnitTesting;
using Opal;

namespace OpalTests
{
    [TestClass]
    public class ContainerTests
    {
        [TestMethod]
        public void ImmutableQueue()
        {
            var stack = ImmutableStack<int>.Empty;
            var s2 = stack.Push(1);
            var s3 = s2.Pop(out var item);
            Assert.AreEqual(item, 1);
            var s4 = s3.Push(2)
                .Push(3);
            var s5 = s4.Pop(out item);
            Assert.AreEqual(3, item);
            Assert.AreEqual(s5.Peek(), 2);
        }
    }
}
