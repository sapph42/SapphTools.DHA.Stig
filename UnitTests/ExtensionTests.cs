using System.Globalization;
using SapphTools.DHA.Stig.Common.Extensions;
using SapphTools.DHA.Stig.Remediator.Extensions;

namespace UnitTests;
[TestClass, TestCategory("Pure")]
public sealed class ExtensionTests {
    [DataTestMethod] [DataRow(0L, "0 ticks")] [DataRow(1L, "1 tick")] [DataRow(2L, "2 ticks")]
    [DataRow(10000L, "1ms")] [DataRow(10000000L, "1s")] [DataRow(600000000L, "1m")]
    [DataRow(36000000000L, "1h")] [DataRow(864000000000L, "1d")]
    public void SmartDurationFormatsEachScale(long ticks, string expected) {
        CultureInfo original = CultureInfo.CurrentCulture;
        try { CultureInfo.CurrentCulture = CultureInfo.InvariantCulture; Assert.AreEqual(expected, new TimeSpan(ticks).ToSmartString()); }
        finally { CultureInfo.CurrentCulture = original; }
    }
    [TestMethod] public void SmartDurationSplitsLayersAndRoundsFinalLayer() {
        CultureInfo original = CultureInfo.CurrentCulture;
        try {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            Assert.AreEqual("1h, 30m", TimeSpan.FromMinutes(90).ToSmartString(layers: 2));
            Assert.AreEqual("1.5h", TimeSpan.FromMinutes(90).ToSmartString());
            Assert.AreEqual("1s, 234ms, 5 ticks", new TimeSpan(12340005).ToSmartString(layers: 6));
        } finally { CultureInfo.CurrentCulture = original; }
    }
    [DataTestMethod] [DataRow(0, 1, "fracDigits")] [DataRow(-1, 1, "fracDigits")]
    [DataRow(1, 0, "layers")] [DataRow(1, -1, "layers")]
    public void SmartDurationRejectsInvalidArguments(int digits, int layers, string parameter) {
        ArgumentOutOfRangeException error = Assert.ThrowsException<ArgumentOutOfRangeException>(() => TimeSpan.Zero.ToSmartString(digits, layers));
        Assert.AreEqual(parameter, error.ParamName);
    }
    [DataTestMethod] [DataRow(0)] [DataRow(7)] [DataRow(-1)]
    public void LayerHelpersRejectOutOfRangeLayers(int layer) {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => TimeSpan.Zero.GetValueAtLayer(layer));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => TimeSpan.Zero.GetTotalValueAtLayer(layer));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => TimeSpan.Zero.GetValueStringAtLayer(layer));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => TimeSpan.Zero.GetTotalValueStringAtLayer(layer, 1));
    }
    [DataTestMethod] [DataRow(1, 1)] [DataRow(2, 2)] [DataRow(3, 3)] [DataRow(4, 4)] [DataRow(5, 5)] [DataRow(6, 6)]
    public void LayerHelpersReturnComponentsWithoutHigherScaleCarry(int layer, int expected) {
        TimeSpan value = new TimeSpan(1, 2, 3, 4, 5) + TimeSpan.FromTicks(6);
        Assert.AreEqual(expected, value.GetValueAtLayer(layer));
        double total = layer switch { 1 => value.TotalDays, 2 => value.TotalHours, 3 => value.TotalMinutes,
            4 => value.TotalSeconds, 5 => value.TotalMilliseconds, _ => value.Ticks };
        Assert.AreEqual(total, value.GetTotalValueAtLayer(layer));
    }
    [TestMethod] public void ActualTicksPreservesSignedSubMillisecondRemainder() {
        Assert.AreEqual(5678, new TimeSpan(12345678).ActualTicks()); Assert.AreEqual(-5678, new TimeSpan(-12345678).ActualTicks());
    }
    [TestMethod] public void EnumMaximumHandlesEmptyNullUnsortedAndRepeatedValues() {
        Assert.AreEqual(SettingContext.None, EnumEx.Max<SettingContext>());
        Assert.AreEqual(SettingContext.None, EnumEx.Max<SettingContext>(null!));
        Assert.AreEqual(SettingContext.System, EnumEx.Max(SettingContext.System, SettingContext.None, SettingContext.Administrator, SettingContext.System));
    }
    private enum ByteFlags : byte { One = 1, High = 128 }
    private enum SByteFlags : sbyte { One = 1, High = -128 }
    private enum ShortFlags : short { One = 1, High = short.MinValue }
    private enum UShortFlags : ushort { One = 1, High = 32768 }
    private enum IntFlags : int { One = 1, High = int.MinValue }
    private enum UIntFlags : uint { One = 1, High = 0x80000000 }
    private enum LongFlags : long { One = 1, High = long.MinValue }
    private enum ULongFlags : ulong { One = 1, High = 0x8000000000000000 }
    private static void CheckOr<T>(T low, T high, T combined) where T : struct, Enum {
        T? none = null, left = low, right = high;
        Assert.IsNull(none.CoalesceOr(none)); Assert.AreEqual(left, left.CoalesceOr(none)); Assert.AreEqual(right, none.CoalesceOr(right));
        Assert.AreEqual((T?)combined, left.CoalesceOr(right)); Assert.AreEqual(left, left.CoalesceOr(left));
    }
    [TestMethod] public void NullableEnumOrSupportsEveryUnderlyingTypeIncludingSignBits() {
        CheckOr(ByteFlags.One, ByteFlags.High, (ByteFlags)129); CheckOr(SByteFlags.One, SByteFlags.High, (SByteFlags)(-127));
        CheckOr(ShortFlags.One, ShortFlags.High, (ShortFlags)(short.MinValue + 1)); CheckOr(UShortFlags.One, UShortFlags.High, (UShortFlags)32769);
        CheckOr(IntFlags.One, IntFlags.High, (IntFlags)(int.MinValue + 1)); CheckOr(UIntFlags.One, UIntFlags.High, (UIntFlags)0x80000001);
        CheckOr(LongFlags.One, LongFlags.High, (LongFlags)(long.MinValue + 1)); CheckOr(ULongFlags.One, ULongFlags.High, (ULongFlags)0x8000000000000001);
    }
    [DataTestMethod] [DataRow(SettingContext.None, "None/Any")] [DataRow(SettingContext.Administrator, "Administrator")]
    [DataRow(SettingContext.System, "SYSTEM")] [DataRow(SettingContext.TrustedInstaller, "TrustedInstaller (noop)")]
    public void ContextUiLabelsAreStable(SettingContext context, string label) => Assert.AreEqual(label, context.GetUiDisplay());
    [TestMethod] public void MissingUiMetadataThrowsAndEnumerationSkipsUnannotatedValues() {
        Assert.ThrowsException<InvalidOperationException>(() => ActionResult.WhatIf.GetUiDisplay());
        Assert.AreEqual(0, UiDisplayExtensions.GetAllUiDisplay<ActionResult>().Count());
        Assert.AreEqual(4, UiDisplayExtensions.GetAllUiDisplay<SettingContext>().Count());
    }
}
