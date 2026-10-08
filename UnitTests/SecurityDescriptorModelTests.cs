using System.Security.AccessControl;
using System.Text.Json.Nodes;
using SapphTools.SecurityDescriptor;
using SapphTools.SecurityDescriptor.Classes;
using SapphTools.SecurityDescriptor.Enums;
using SapphTools.SecurityDescriptor.Extensions;

namespace UnitTests;

// These tests create security descriptors in memory. They never apply one to a file,
// registry key, policy object, or certificate store.
[TestClass, TestCategory("WindowsModel")]
public sealed class SecurityDescriptorModelTests {
    [TestInitialize]
    public void RequireWindows() {
        if (!OperatingSystem.IsWindows()) Assert.Inconclusive("SID and ACL models require Windows.");
    }
    private static Ace Allow(string trustee = "SY", string right = "GA") => new("A", "", right, null, null, trustee);

    [DataTestMethod, DataRow("FA", "0x001F01FF"), DataRow("KR", "KX")]
    [DataRow("GRGW", "0xC0000000")]
    public void AceEqualityAndHashingUseEquivalentRightsMasks(string left, string right) {
        Ace first = Allow(right: left), second = Allow(right: right);
        Assert.IsTrue(first.Equals(second)); Assert.IsTrue(second.Equals(first));
        Assert.AreEqual(first.GetHashCode(), second.GetHashCode());
        Assert.AreEqual(1, new HashSet<Ace> { first, second }.Count);
    }

    [TestMethod]
    public void AceClonePreservesSymbolicRightsAndRemainsEqual() {
        Ace original = Allow(right: "FA"), clone = original.Clone();
        Assert.AreNotSame(original, clone);
        Assert.AreNotSame(original.Right, clone.Right);
        Assert.AreEqual(original.ToString(), clone.ToString());
        Assert.AreEqual("FA", clone.Right.ToString());
        Assert.IsTrue(original.Equals(clone));
        Assert.AreEqual(original.GetHashCode(), clone.GetHashCode());
    }

    [DataTestMethod, DataRow("SY"), DataRow("BA"), DataRow("WD"), DataRow("AU")]
    public void TrusteesCompareCanonicalSidAcrossAliasAndNumericRepresentations(string alias) {
        Trustee first = Trustee.Construct(alias), second = Trustee.Construct(first.NativeSid.Value);
        Assert.IsTrue(first.Equals(second)); Assert.IsTrue(second.Equals(first));
        Assert.IsTrue(first.Equals((object)second)); Assert.AreEqual(first.GetHashCode(), second.GetHashCode());
        Assert.AreEqual(1, new HashSet<Trustee> { first, second }.Count);
        Assert.IsFalse(first.Equals((Trustee?)null)); Assert.IsFalse(first.Equals(new object()));
        Trustee copy = Trustee.Construct(first.NativeSid);
        Assert.AreEqual(first.NativeSid, copy.NativeSid);
    }

    [TestMethod]
    public void TrusteesRejectInvalidSidsAndDistinguishDifferentPrincipals() {
        Assert.ThrowsException<ArgumentException>(() => Trustee.Construct("not-a-sid"));
        Assert.IsFalse(Trustee.Construct("SY").Equals(Trustee.Construct("BA")));
    }

    [TestMethod]
    public void RightsEqualityUsesTrusteeSetRatherThanInputOrder() {
        SeRightsValue first = new() { Target = LsaPrivilege.SE_NETWORK_LOGON, AccountNames = [Trustee.Construct("SY"), Trustee.Construct("BA")] };
        SeRightsValue second = new() { Target = first.Target, AccountNames = [Trustee.Construct("BA"), Trustee.Construct("SY")] };
        Assert.IsTrue(first.Equals(second)); Assert.IsTrue(second.Equals(first));
        second.AccountNames = [Trustee.Construct("SY")];
        Assert.IsFalse(first.Equals(second)); Assert.IsFalse(second.Equals(first));
        second.AccountNames = [Trustee.Construct("SY"), Trustee.Construct("WD")];
        Assert.IsFalse(first.Equals(second)); Assert.IsFalse(second.Equals(first));
    }

    [TestMethod]
    public void TypedAndTextAceConstructionProduceEqualIndependentEntries() {
        Guid objectId = Guid.Parse("11111111-2222-3333-4444-555555555555");
        Guid inheritId = Guid.Parse("66666666-7777-8888-9999-aaaaaaaaaaaa");
        Ace text = new("OA", "CIOI", "GR", objectId.ToString(), inheritId.ToString(), "SY");
        Ace typed = new(SddlAceType.SDDL_OBJECT_ACCESS_ALLOWED,
            SddlAceFlags.SDDL_CONTAINER_INHERIT | SddlAceFlags.SDDL_OBJECT_INHERIT,
            SddlRight.Construct("GR"), objectId, inheritId, Trustee.Construct("SY"));
        Assert.IsTrue(text.Equals(typed)); Assert.IsTrue(typed.Equals(text));
        Assert.IsTrue(text.Equals((object)typed)); Assert.AreEqual(text.GetHashCode(), typed.GetHashCode());
        Assert.AreEqual(text.ToString(), typed.ToString());
        Assert.IsTrue(text.ToString().Contains(objectId.ToString(), StringComparison.Ordinal));
        Assert.IsTrue(text.ToString().Contains(inheritId.ToString(), StringComparison.Ordinal));
        Assert.IsFalse(text.Equals((Ace?)null)); Assert.IsFalse(text.Equals(new object()));
        Assert.AreEqual(1, new HashSet<Ace> { text, typed }.Count);
    }

    [DataTestMethod, DataRow("type"), DataRow("flags"), DataRow("right")]
    [DataRow("object"), DataRow("inherit"), DataRow("trustee")]
    public void AceEqualityIncludesEveryPermissionField(string field) {
        Ace first = Allow(), second = Allow();
        switch (field) {
            case "type": second.Type = SddlAceType.SDDL_ACCESS_DENIED; break;
            case "flags": second.Flags = SddlAceFlags.SDDL_INHERITED; break;
            case "right": second.Right = SddlRight.Construct("GR"); break;
            case "object": second.ObjectType = Guid.Parse("11111111-2222-3333-4444-555555555555"); break;
            case "inherit": second.ObjectInheritType = Guid.Parse("11111111-2222-3333-4444-555555555555"); break;
            case "trustee": second.Trustee = Trustee.Construct("BA"); break;
        }
        Assert.IsFalse(first.Equals(second)); Assert.IsFalse(second.Equals(first));
    }

    [TestMethod]
    public void AceConstructorRejectsUnknownTypesAndFlags() {
        Assert.ThrowsException<ArgumentException>(() => new Ace("Invented", "", "GA", null, null, "SY"));
        Assert.ThrowsException<ArgumentException>(() => new Ace("A", "ZZ", "GA", null, null, "SY"));
    }

    [TestMethod]
    public void DaclEditsPreserveOrderAndUnrelatedDescriptorSections() {
        Sddl value = new("O:SYG:BAD:P(A;;GA;;;SY)(A;;GR;;;BA)S:AI(AU;SA;GR;;;WD)", ObjectType.File);
        string owner = value.Owner!.SddlSafe, group = value.Group!.SddlSafe;
        var flags = value.DaclFlags;
        string audit = value.SaclAces!.Single().ToString();
        Ace replacement = Allow("BA", "GW");
        value.AddDacl(Allow()); Assert.AreEqual(2, value.DaclAces!.Count);
        value.RemoveDacl(Allow("WD")); Assert.AreEqual(2, value.DaclAces.Count);
        value.ReplaceDacl(Allow("BA", "GR"), replacement);
        Assert.IsTrue(value.DaclAces[0].Equals(Allow())); Assert.AreSame(replacement, value.DaclAces[1]);
        value.ReplaceDacl(Allow("WD"), Allow("AU")); Assert.AreEqual(3, value.DaclAces.Count);
        value.RemoveDacl(Allow()); Assert.AreEqual(2, value.DaclAces.Count);
        Assert.AreEqual(owner, value.Owner.SddlSafe); Assert.AreEqual(group, value.Group.SddlSafe);
        Assert.AreEqual(flags, value.DaclFlags); Assert.AreEqual(audit, value.SaclAces.Single().ToString());
        Sddl reparsed = new(value.ToString(), ObjectType.File);
        Assert.AreEqual(value.ToString(), reparsed.ToString());
    }

    [TestMethod]
    public void DaclMutatorsHandleAbsentAndEmptyAcl() {
        Sddl absent = new("O:SY", ObjectType.File);
        absent.RemoveDacl(Allow()); Assert.IsNull(absent.DaclAces);
        absent.AddDacl(Allow()); Assert.AreEqual(1, absent.DaclAces!.Count);
        absent.RemoveDacl(Allow()); Assert.AreEqual(0, absent.DaclAces.Count);
        Sddl other = new("O:SY", ObjectType.File);
        other.ReplaceDacl(Allow("BA"), Allow()); Assert.AreEqual(1, other.DaclAces!.Count);
        Assert.IsTrue(other.DaclAces.Single().Equals(Allow()));
    }

    [DataTestMethod, DataRow(ObjectType.File), DataRow(ObjectType.RegistryKey)]
    public void SecurityObjectConversionsPreserveOwnerGroupDaclAndSaclInMemory(ObjectType type) {
        string text = "O:SYG:BAD:P(A;;GA;;;SY)S:(AU;SA;GR;;;WD)";
        Sddl source = new(text, type);
        ObjectSecurity security = type == ObjectType.File ? source.ToFileSecurity() : source.ToRegistrySecurity();
        RawSecurityDescriptor actual = new(security.GetSecurityDescriptorBinaryForm(), 0), expected = new(text);
        Assert.AreEqual(expected.Owner, actual.Owner); Assert.AreEqual(expected.Group, actual.Group);
        Assert.AreEqual(expected.ControlFlags, actual.ControlFlags);
        Assert.AreEqual(expected.DiscretionaryAcl!.Count, actual.DiscretionaryAcl!.Count);
        Assert.AreEqual(expected.SystemAcl!.Count, actual.SystemAcl!.Count);
        Assert.AreEqual(expected.GetSddlForm(AccessControlSections.All), actual.GetSddlForm(AccessControlSections.All));
    }

    [TestMethod, TestCategory("KnownRegression")]
    public void SddlCloneOwnsDaclEntriesAndPrincipalState() {
        Sddl source = new("O:SYG:BAD:P(A;;GA;;;SY)", ObjectType.File), clone = source.Clone();
        Assert.AreEqual(source.ToString(), clone.ToString());
        Assert.AreNotSame(source.DaclAces, clone.DaclAces);
        Assert.AreNotSame(source.DaclAces![0], clone.DaclAces![0]);
        clone.DaclAces[0].Right = SddlRight.Construct("GR");
        clone.AddDacl(Allow("BA")); clone.Owner = Trustee.Construct("BA");
        Assert.AreEqual(1, source.DaclAces.Count);
        Assert.AreEqual("GA", source.DaclAces[0].Right.ToString()); Assert.AreEqual("SY", source.Owner!.SddlSafe);
    }

    [DataTestMethod, DataRow(false), DataRow(true)]
    public void AclValueEqualityIncludesTargetAndAction(bool registry) {
        Sddl descriptor = new("O:SYG:BAD:P(A;;GA;;;SY)", registry ? ObjectType.RegistryKey : ObjectType.File);
        IValue first = registry ? new RegistryAclValue { Target = "Test", Sddl = descriptor } : new FileSystemAclValue { Target = "Test", Sddl = descriptor };
        IValue same = registry ? new RegistryAclValue { Target = "TEST", Sddl = descriptor } : new FileSystemAclValue { Target = "TEST", Sddl = descriptor };
        Assert.IsTrue(first.Equals(same)); Assert.IsFalse(first.Equals(null));
        IValue differentAction = registry ? new FileSystemAclValue { Target = "Test", Sddl = descriptor } : new RegistryAclValue { Target = "Test", Sddl = descriptor };
        Assert.IsFalse(first.Equals(differentAction)); Assert.IsFalse(differentAction.Equals(first));
        if (same is RegistryAclValue reg) reg.Target = "Other";
        else ((FileSystemAclValue)same).Target = "Other";
        Assert.IsFalse(first.Equals(same));
        IValue clone = first.Clone(); Assert.AreNotSame(first, clone); Assert.AreEqual(first.TargetString, clone.ToString());
    }

    [DataTestMethod, DataRow(false), DataRow(true)]
    public void AclConvertersSkipUnknownFieldsAndPreserveFollowingSddl(bool registry) {
        Sddl source = new("O:SYG:BAD:P(A;;GA;;;SY)", registry ? ObjectType.RegistryKey : ObjectType.File);
        IValue original = registry ? new RegistryAclValue { Target = "Test", Sddl = source } : new FileSystemAclValue { Target = "Test", Sddl = source };
        JsonObject json = JsonNode.Parse(JsonSerializer.Serialize(original, TestData.Options()))!.AsObject();
        json.Insert(0, "Future", JsonNode.Parse("""{"items":[1,{}]}"""));
        IValue value = JsonSerializer.Deserialize<IValue>(json.ToJsonString(), TestData.Options())!;
        Assert.AreEqual("Test", value.TargetString);
        Sddl descriptor = registry ? ((RegistryAclValue)value).Sddl : ((FileSystemAclValue)value).Sddl;
        Assert.AreEqual("SY", descriptor.Owner!.SddlSafe); Assert.AreEqual(1, descriptor.DaclAces!.Count);
    }
}
