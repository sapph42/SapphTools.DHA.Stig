using System.Reflection;
using SapphTools.SecurityDescriptor.Attributes;
using SapphTools.SecurityDescriptor.Classes;
using SapphTools.SecurityDescriptor.Enums;
using SapphTools.SecurityDescriptor.Extensions;

namespace UnitTests;

[TestClass, TestCategory("Pure")]
public sealed class SecurityRightTests {
    public static IEnumerable<object[]> Rights() {
        foreach (SddlRights value in Enum.GetValues<SddlRights>().Distinct()) {
            RightMetaAttribute? meta = typeof(SddlRights).GetField(value.ToString())?.GetCustomAttribute<RightMetaAttribute>();
            if (meta is not null) yield return [value, meta.Abbr];
        }
    }
    [DataTestMethod, DynamicData(nameof(Rights), DynamicDataSourceType.Method)]
    public void SymbolicRightsPreserveMaskEqualityAndHash(SddlRights value, string abbreviation) {
        Right first = Right.Construct(value), second = Right.Construct(abbreviation);
        Assert.AreEqual((uint)value, first.GetHexValue(), abbreviation);
        Assert.AreEqual(abbreviation, first.ToString());
        Assert.IsTrue(first.Equals(second)); Assert.IsTrue(second.Equals(first));
        Assert.IsTrue(first.Equals((object)second)); Assert.AreEqual(first.GetHashCode(), second.GetHashCode());
        Assert.IsFalse(first.Equals((Right?)null)); Assert.IsFalse(first.Equals(new object()));
        Assert.AreEqual(1, new HashSet<Right> { first, second }.Count);
    }

    [DataTestMethod, DataRow(ObjectType.Generic), DataRow(ObjectType.Standard), DataRow(ObjectType.File)]
    [DataRow(ObjectType.RegistryKey), DataRow(ObjectType.DirectoryService)]
    public void RightMetadataLookupHonorsObjectType(ObjectType type) {
        string[] abbreviations = RightExtensions.GetAllAbbr(type).ToArray();
        Assert.IsTrue(abbreviations.Length > 0);
        foreach (string abbreviation in abbreviations) {
            Assert.IsTrue(RightExtensions.TryGetRight(abbreviation, type, out SddlRights? value));
            Assert.IsNotNull(value);
            Assert.IsFalse(string.IsNullOrWhiteSpace(value.Value.GetDescription()));
        }
        Assert.IsFalse(RightExtensions.TryGetRight("Invented", type, out SddlRights? missing));
        Assert.IsNull(missing);
    }

    [TestMethod]
    public void RightsWithDifferentMasksCompareUnequal() {
        Right read = Right.Construct("GR"), write = Right.Construct("GW");
        Assert.IsFalse(read.Equals(write)); Assert.IsFalse(write.Equals(read));
        Assert.IsFalse(RightExtensions.TryGetRight("GA", ObjectType.File, out _));
    }

    [DataTestMethod, DataRow(""), DataRow("INVALID"), DataRow("0xnothex")]
    public void InvalidRightsAreRejected(string text) {
        Assert.ThrowsException<ArgumentException>(() => Right.Construct(text));
    }

    [TestMethod, TestCategory("KnownRegression")]
    public void MandatoryRightsMetadataIsNotLostToDuplicateEnumMasks() {
        foreach (string abbreviation in new[] { "NR", "NW", "NX" }) {
            Assert.IsTrue(RightExtensions.TryGetRight(abbreviation, ObjectType.Mandatory, out _), abbreviation);
        }
        CollectionAssert.AreEquivalent(new[] { "NR", "NW", "NX" }, RightExtensions.GetAllAbbr(ObjectType.Mandatory).ToArray());
    }

    [DataTestMethod, TestCategory("KnownRegression"), DataRow("GRjunk"), DataRow("junkGR")]
    public void RightsParserMustRejectUnconsumedCharacters(string text) {
        Assert.ThrowsException<ArgumentException>(() => Right.Construct(text));
    }

    [DataTestMethod, TestCategory("KnownRegression")]
    [DataRow(0u), DataRow(1u), DataRow(0x10000000u), DataRow(0x80000000u), DataRow(uint.MaxValue)]
    public void NumericRightsRetainTheirMaskAndUseParseableSddl(uint mask) {
        Right value = Right.Construct(mask);
        Assert.AreEqual(mask, value.GetHexValue());
        string text = value.ToString();
        Assert.IsTrue(text.StartsWith("0x", StringComparison.OrdinalIgnoreCase));
        Right reparsed = Right.Construct(text);
        Assert.AreEqual(mask, reparsed.GetHexValue());
        Assert.IsTrue(value.Equals(reparsed)); Assert.AreEqual(value.GetHashCode(), reparsed.GetHashCode());
    }

    [DataTestMethod, TestCategory("KnownRegression")]
    [DataRow("GRGW", 0xC0000000u), DataRow("RCWD", 0x00060000u)]
    public void CombinedSymbolicRightsRetainEveryBit(string text, uint mask) {
        Right value = Right.Construct(text);
        Assert.AreEqual(mask, value.GetHexValue());
        Assert.AreEqual(mask, Right.Construct(value.ToString()).GetHexValue());
    }

    [TestMethod]
    public void MetadataEnumerationsRoundTripWellKnownPrincipalNames() {
        string[] full = MetaExtensions.GetAllFull<SidString>().ToArray();
        string[] expanded = MetaExtensions.GetAllExpanded<SidString>().ToArray();
        Assert.IsTrue(full.Length > 0); Assert.IsTrue(expanded.Length > 0);
        CollectionAssert.AreEquivalent(full, Trustee.EnumerateWellKnownPrincipals().ToArray());
        foreach (string name in full) {
            Assert.IsTrue(MetaExtensions.TryGetMetaFull(name, out SidString value));
            Assert.AreEqual(name, value.GetFull());
        }
        foreach (string name in expanded) {
            Assert.IsTrue(MetaExtensions.TryGetMetaExpanded(name, out SidString value));
            Assert.AreEqual(name, value.GetExpanded());
        }
        Assert.IsFalse(MetaExtensions.TryGetMetaFull("Invented", out SidString _));
        Assert.IsFalse(MetaExtensions.TryGetMetaExpanded("Invented", out SidString _));
    }
}
