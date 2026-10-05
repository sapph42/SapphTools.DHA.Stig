namespace UnitTests;
[TestClass, TestCategory("Pure")]
public sealed class NativeMemoryTests {
    [TestMethod] public void SafeBufferRoundTripsBytesWithinAllocatedBoundsAndClosesIdempotently() {
        SafeHBuffer buffer = SafeHBuffer.AllocFactory(8);
        try {
            byte[] expected = [0, 1, 2, 3, 127, 128, 254, 255], actual = new byte[8];
            Assert.AreEqual(8UL, buffer.ByteLength); Assert.IsFalse(buffer.IsInvalid);
            buffer.WriteArray(0, expected, 0, expected.Length); buffer.ReadArray(0, actual, 0, actual.Length);
            CollectionAssert.AreEqual(expected, actual);
            Assert.ThrowsException<ArgumentException>(() => buffer.Read<byte>(8));
        } finally { buffer.Dispose(); }
        buffer.Dispose(); Assert.IsTrue(buffer.IsClosed); Assert.IsTrue(buffer.IsInvalid);
    }
    [TestMethod] public void UnicodeWrapperUsesIdentityEqualityAndReleasesRepeatedly() {
        using LsaUnicodeStringWrapper a = new("a"), b = new("a");
        Assert.IsTrue(a.Equals(a)); Assert.IsFalse(a.Equals(b)); Assert.IsFalse(a.Equals(null)); Assert.IsFalse(a.Equals("a"));
        Assert.AreEqual(a.GetHashCode(), a.GetHashCode());
        bool acquired = false; LsaUnicodeString first = a.DangerousGetStruct(ref acquired);
        try { Assert.IsTrue(acquired); Assert.AreNotEqual(IntPtr.Zero, first.Buffer); }
        finally { a.DangerousReleaseStructBuffer(); }
        a.DangerousReleaseStructBuffer(); acquired = false;
        LsaUnicodeString second = a.DangerousGetStruct(ref acquired);
        try { Assert.IsTrue(acquired); Assert.AreNotEqual(IntPtr.Zero, second.Buffer); }
        finally { a.DangerousReleaseStructBuffer(); }
        a.Dispose(); a.Dispose();
    }
}
