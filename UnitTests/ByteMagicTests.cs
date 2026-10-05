namespace UnitTests;
[TestClass, TestCategory("Pure")]
public sealed class ByteMagicTests {
    public static IEnumerable<object[]> Bytes() => Enumerable.Range(0, 256).Select(n => new object[] { (byte)n, $"0x{n:X2}" });
    [DataTestMethod, DynamicData(nameof(Bytes), DynamicDataSourceType.Method)]
    public void SingleByteFormatsAsUppercasePrefixedHex(byte value, string expected) => Assert.AreEqual(expected, ByteMagic.ToHexString([value]));
}
