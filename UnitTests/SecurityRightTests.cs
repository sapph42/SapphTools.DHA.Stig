using SapphTools.SecurityDescriptor.Classes;
using SapphTools.SecurityDescriptor.Classes.Rights;
using SapphTools.SecurityDescriptor.Enums;
using SapphTools.SecurityDescriptor.Extensions;

namespace UnitTests;

[TestClass, TestCategory("Pure")]
public sealed class SecurityRightTests {
    public static IEnumerable<object[]> Rights() {
        foreach (var entry in SddlRightValue.ByAbbreviation) {
            yield return [entry.Key, entry.Value.Value];
        }
    }
    [DataTestMethod, DynamicData(nameof(Rights), DynamicDataSourceType.Method)]
    public void SymbolicRightsPreserveMaskEqualityAndHash(string abbreviation, uint mask) {
        SddlRight first = SddlRight.Construct(mask), second = SddlRight.Construct(abbreviation);
        Assert.AreEqual(mask, first.ToValue(), abbreviation);
        Assert.AreEqual(mask, second.ToValue(), abbreviation);
        Assert.AreEqual(abbreviation, second.ToString());
        Assert.AreEqual($"0x{mask:X8}", first.ToString());
        Assert.IsTrue(first.Equals(second)); Assert.IsTrue(second.Equals(first));
        Assert.IsTrue(first.Equals((object)second)); Assert.AreEqual(first.GetHashCode(), second.GetHashCode());
        Assert.IsFalse(first.Equals((SddlRight?)null)); Assert.IsFalse(first.Equals(new object()));
        Assert.AreEqual(1, new HashSet<SddlRight> { first, second }.Count);
    }

    [DataTestMethod, DataRow(ObjectType.Generic), DataRow(ObjectType.Standard), DataRow(ObjectType.File)]
    [DataRow(ObjectType.RegistryKey), DataRow(ObjectType.DirectoryService), DataRow(ObjectType.Mandatory)]
    public void RightMetadataLookupHonorsObjectType(ObjectType type) {
        var rights = SddlRightValue.ByTypeAndAbbr[type];
        Assert.IsTrue(rights.Count > 1);
        foreach (var entry in rights) {
            SddlRightValue? value = SddlRightValue.Construct(entry.Key, type);
            Assert.IsNotNull(value);
            Assert.AreSame(entry.Value, value);
            Assert.IsFalse(string.IsNullOrWhiteSpace(value.Description));
            Assert.AreEqual(entry.Value.Description, value.Description);
        }
        Assert.IsNull(SddlRightValue.Construct("Invented", type));
    }

    [TestMethod]
    public void RightsWithDifferentMasksCompareUnequal() {
        SddlRight read = SddlRight.Construct("GR"), write = SddlRight.Construct("GW");
        Assert.IsFalse(read.Equals(write)); Assert.IsFalse(write.Equals(read));
        Assert.IsNull(SddlRightValue.Construct("GA", ObjectType.File));
    }

    [DataTestMethod, DataRow("INVALID"), DataRow("0xnothex"), DataRow("ZZ")]
    public void InvalidRightsAreRejected(string text) {
        Assert.ThrowsException<ArgumentException>(() => SddlRight.Construct(text));
    }

    [DataTestMethod, DataRow(null), DataRow(""), DataRow(" ")]
    public void EmptyRightsHaveAZeroMaskAndKeepTheirTextualRepresentation(string? text) {
        SddlRight value = SddlRight.Construct(text);
        Assert.AreEqual(0u, value.ToValue());
        Assert.AreEqual(string.Empty, value.ToString());
        Assert.IsTrue(value.Equals(SddlRight.Construct(0u)));
        Assert.AreEqual(value.GetHashCode(), SddlRight.Construct(0u).GetHashCode());
    }

    [TestMethod]
    public void MandatoryRightsMetadataIsNotLostToOverlappingMasks() {
        foreach (string abbreviation in new[] { "NR", "NW", "NX" }) {
            Assert.IsNotNull(SddlRightValue.Construct(abbreviation, ObjectType.Mandatory), abbreviation);
        }
        CollectionAssert.AreEquivalent(new[] { "NR", "NW", "NX" },
            MandatoryRight.ByAbbr.Keys.Where(abbreviation => abbreviation.Length != 0).ToArray());
        Assert.AreEqual(MandatoryRight.SDDL_NO_WRITE_UP.Value, DirectoryRight.SDDL_CREATE_CHILD.Value);
        Assert.AreEqual("No write up", MandatoryRight.SDDL_NO_WRITE_UP.Description);
        Assert.AreEqual("Create child objects", DirectoryRight.SDDL_CREATE_CHILD.Description);
        Assert.IsFalse(MandatoryRight.SDDL_NO_WRITE_UP.Equals(DirectoryRight.SDDL_CREATE_CHILD));
    }

    [TestMethod]
    public void RegistryAliasesKeepTheirTokensWhileAggregatesCompareMasks() {
        Assert.AreEqual("KR", RegistryRight.ByAbbr["KR"].Abbr);
        Assert.AreEqual("KX", RegistryRight.ByAbbr["KX"].Abbr);
        Assert.IsFalse(RegistryRight.ByAbbr["KR"].Equals(RegistryRight.ByAbbr["KX"]));
        SddlRight read = SddlRight.Construct("KR"), execute = SddlRight.Construct("KX");
        Assert.IsTrue(read.Equals(execute));
        Assert.AreEqual(read.GetHashCode(), execute.GetHashCode());
        Assert.AreEqual("KRKX", SddlRight.Construct("KRKX").ToString());
    }

    [DataTestMethod, DataRow("GRjunk"), DataRow("junkGR")]
    public void RightsParserMustRejectUnconsumedCharacters(string text) {
        Assert.ThrowsException<ArgumentException>(() => SddlRight.Construct(text));
    }

    [DataTestMethod]
    [DataRow(0u), DataRow(1u), DataRow(0x10000000u), DataRow(0x80000000u), DataRow(uint.MaxValue)]
    public void NumericRightsRetainTheirMaskAndUseParseableSddl(uint mask) {
        SddlRight value = SddlRight.Construct(mask);
        Assert.AreEqual(mask, value.ToValue());
        string text = value.ToString();
        Assert.IsTrue(text.StartsWith("0x", StringComparison.OrdinalIgnoreCase));
        SddlRight reparsed = SddlRight.Construct(text);
        Assert.AreEqual(mask, reparsed.ToValue());
        Assert.IsTrue(value.Equals(reparsed)); Assert.AreEqual(value.GetHashCode(), reparsed.GetHashCode());
    }

    [DataTestMethod]
    [DataRow("GRGW", 0xC0000000u), DataRow("RCWD", 0x00060000u), DataRow("RCRP", 0x00020010u)]
    public void CombinedSymbolicRightsRetainEveryBit(string text, uint mask) {
        SddlRight value = SddlRight.Construct(text);
        Assert.AreEqual(mask, value.ToValue());
        Assert.AreEqual(mask, SddlRight.Construct(value.ToString()).ToValue());
    }

    [DataTestMethod, DataRow(""), DataRow("GA"), DataRow("NW"), DataRow("KRKX")]
    [DataRow("0x00000000"), DataRow("0xFFFFFFFF")]
    public void RightsClonesPreserveTheirRepresentationAndEquality(string text) {
        SddlRight original = SddlRight.Construct(text), clone = original.Clone();
        Assert.AreNotSame(original, clone);
        Assert.AreEqual(original.ToString(), clone.ToString());
        Assert.AreEqual(original.ToValue(), clone.ToValue());
        Assert.IsTrue(original.Equals(clone));
        Assert.AreEqual(original.GetHashCode(), clone.GetHashCode());
    }

    [TestMethod]
    public void MetadataEnumerationsRoundTripWellKnownPrincipalNames() {
        string[] full = [.. MetaExtensions.GetAllFull<SidString>()];
        string[] expanded = [.. MetaExtensions.GetAllExpanded<SidString>()];
        Assert.IsTrue(full.Length > 0); 
        Assert.IsTrue(expanded.Length > 0);
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
