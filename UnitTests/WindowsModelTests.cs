using SapphTools.SecurityDescriptor;
using SapphTools.SecurityDescriptor.Classes;
using SapphTools.SecurityDescriptor.Enums;

namespace UnitTests;
[TestClass, TestCategory("WindowsModel")]
public sealed class WindowsModelTests {
    [TestInitialize] public void RequireWindows() {
        if (!OperatingSystem.IsWindows()) Assert.Inconclusive("SecurityIdentifier and SDDL conversions require Windows.");
    }
    [DataTestMethod] [DataRow(false)] [DataRow(true)]
    public void AclValueRoundTripPreservesOwnerGroupDaclAndObjectType(bool registry) {
        string target = registry ? @"HKEY_CURRENT_USER\Software\Test" : @"C:\Test.txt";
        Sddl sddl = new("O:SYG:BAD:P(A;;GA;;;SY)(A;;GR;;;BA)", registry ? ObjectType.RegistryKey : ObjectType.File);
        IValue source = registry ? new RegistryAclValue { Target = target, Sddl = sddl } : new FileSystemAclValue { Target = target, Sddl = sddl };
        string json = JsonSerializer.Serialize(source, TestData.Options());
        IValue copy = JsonSerializer.Deserialize<IValue>(json, TestData.Options())!;
        Assert.AreEqual(source.Action, copy.Action); Assert.AreEqual(target, copy.TargetString);
        Sddl copied = registry ? ((RegistryAclValue)copy).Sddl : ((FileSystemAclValue)copy).Sddl;
        Assert.AreEqual(sddl.ToString(), copied.ToString()); Assert.AreEqual(sddl.Type, copied.Type);
        Assert.IsNotNull(copied.Owner); Assert.IsNotNull(copied.Group); Assert.AreEqual(2, copied.DaclAces!.Count);
    }
    [TestMethod] public void SddlSnapshotMustNotSilentlyAddProtection() {
        Sddl value = new("O:SYG:BAD:(A;;GA;;;SY)", ObjectType.RegistryKey);
        Assert.AreEqual("O:SYG:BAD:(A;;GA;;;SY)", value.ToString());
        RegistryAclValue source = new() { Target = "HKCU", Sddl = value };
        RegistryAclValue copy = source.Clone(); Assert.AreNotSame(source.Sddl, copy.Sddl); Assert.AreEqual(value.ToString(), copy.Sddl.ToString());
    }
    [TestMethod] public void SeRightsRoundTripPreservesBuiltInPrincipalsAndPrivilege() {
        SeRightsValue value = new() { Target = LsaPrivilege.SE_NETWORK_LOGON, AccountNames = [Trustee.Construct("BA"), Trustee.Construct("SY")] };
        SeRightsValue copy = JsonSerializer.Deserialize<SeRightsValue>(JsonSerializer.Serialize(value, TestData.Options()), TestData.Options())!;
        Assert.AreSame(value.Target, copy.Target); Assert.AreEqual(2, copy.AccountNames.Length);
        CollectionAssert.AreEqual(value.AccountNames.Select(t => t.SddlSafe).ToArray(), copy.AccountNames.Select(t => t.SddlSafe).ToArray());
        SeRightsValue clone = value.Clone(); Assert.AreNotSame(value.AccountNames, clone.AccountNames);
    }
}
