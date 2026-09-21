using Microsoft.Win32;
using SapphTools.SecurityDescriptor;
using SapphTools.SecurityDescriptor.Classes;
using System.Text.RegularExpressions;

namespace SapphTools.DHA.Stig.Common.Classes;
public abstract class AclValue<T> : IValue<T> {
    public abstract TargetType Action { get; }
    public required Sddl Sddl { get; set; }
    public required string Target { get; set; }
    public string TargetString => Target;
    public abstract T Clone();
    public override string ToString() => TargetString;
    IValue IValue.Clone() => (IValue)Clone()!;
}
public class FileSystemAclValue : AclValue<FileSystemAclValue> {
    [JsonInclude]
    public override TargetType Action => TargetType.FileSystemAcl;
    public override FileSystemAclValue Clone() {
        return new() {
            Sddl = Sddl.Clone(),
            Target = Target
        };
    }
}
public class RegistryAclValue : AclValue<RegistryAclValue> {
    [JsonInclude]
    public override TargetType Action => TargetType.RegistryAcl;
    public override RegistryAclValue Clone() {
        return new() {
            Sddl = Sddl.Clone(),
            Target = Target
        };
    }
}
[JsonConverter(typeof(SeRightsValueConverter))]
public class SeRightsValue : IValue<SeRightsValue> {
    [JsonInclude]
    public TargetType Action => TargetType.SeRight;
    public required Trustee[] AccountNames { get; set; }
    public required LsaPrivilege Target { get; set; }
    public string TargetString => Target.Value;
    public SeRightsValue() { }

    public SeRightsValue Clone() {
        return new() {
            AccountNames = [..AccountNames],
            Target = Target
        };
    }
    public override string ToString() => TargetString;
    IValue IValue.Clone() => Clone();
}
public abstract class SamValue : IValue {
    [JsonInclude]
    public TargetType Action => TargetType.Sam;
    public abstract SamActionType Target { get; }
    public string TargetString => Target.ToString();
    IValue IValue.Clone() => throw new NotImplementedException();
    string IValue.ToString() => throw new NotImplementedException();
}
public abstract class SamValue<T> : SamValue, IValue<T> {
    public abstract T Clone();
    public override string ToString() => TargetString;
    IValue IValue.Clone() => (IValue)Clone()!;
}
public abstract class SamValue<T, TSub> : SamValue<TSub> where T : struct  {
    public abstract T SamStruct { get; set; }
}
public class LockoutValue : SamValue<SamUserModalInfo3, LockoutValue> {
    public override SamActionType Target => SamActionType.AccountLockoutPolicy;
    public override SamUserModalInfo3 SamStruct { get; set; }
    public override LockoutValue Clone() {
        return new() {
            SamStruct = SamStruct,
        };
    }
}
public class RegistryKeyValue : IValue<RegistryKeyValue> {
    [JsonInclude]
    public TargetType Action => TargetType.RegistryKey;
    public required string Target { get; set; }
    public required string Name { get; set; }
    public string TargetString => Target;
    public RegistryKeyValue Clone() {
        return new() {
            Name = Name,
            Target = Target
        };
    }
    public override string ToString() => TargetString;
    IValue IValue.Clone() => Clone();
}
[JsonConverter(typeof(RegistryValueValueConverter))]
public class RegistryValueValue : IValue<RegistryValueValue> {
    [JsonInclude]
    public TargetType Action => TargetType.RegistryValue;
    public required string Target { get; set; }
    public required string Name { get; set; }
    public required object? Data { get; set; }
    public RegistryValueKind Kind { get; set; }
    public bool Overwrite { get; set; }
    public string TargetString => Target + "\\" + Name;
    public RegistryValueValue Clone() {
        return new() {
            Target = Target,
            Name = Name,
            Data = Data,
            Kind = Kind,
            Overwrite = Overwrite
        };
    }
    public override string ToString() => TargetString;
    IValue IValue.Clone() => Clone();
}
[JsonConverter(typeof(RegistryValuePatternValueConverter))]
public class RegistryValuePatternValue : IValue<RegistryValuePatternValue> {
    [JsonInclude]
    public TargetType Action => TargetType.RegistryValuePattern;
    public required string Target { get; set; }
    public Regex? TargetPattern { get; set; }
    public string? SubPath { get; set; }
    public Regex? PathPattern { get; set; }
    public required string Name { get; set; }
    public required object? Data { get; set; }
    public RegistryValueKind Kind { get; set; }
    public RegistryValueValue? ResolvedTarget { get; set; }
    public bool Overwrite { get; set; }
    public string TargetString => Target + 
        (TargetPattern is not null ?
            "\\" + TargetPattern.ToString() :
            string.Empty
        ) +
        (SubPath is not null ?
            "\\" + SubPath.ToString() :
            string.Empty
        ) +
         (PathPattern is not null ?
            "\\" + PathPattern.ToString() :
            string.Empty
        ) +
        "\\" + Name;
    public RegistryValuePatternValue Clone() {
        return new() {
            Target = Target,
            TargetPattern = TargetPattern,
            SubPath = SubPath,
            PathPattern = PathPattern,
            Name = Name,
            Data = Data,
            Kind = Kind,
            Overwrite = Overwrite
        };
    }
    public override string ToString() => TargetString;
    IValue IValue.Clone() => Clone();
}
public class CertificatesValue : IValue<CertificatesValue> {
    [JsonInclude]
    public TargetType Action => TargetType.CertStore;
    public required string Target { get; set; }
    public required List<IArtifact> Artifacts { get; set; }
    public string TargetString => $"Local Machine\\{Target}";

    public CertificatesValue Clone() {
        List<IArtifact> clonedArtifacts = [];
        foreach (IArtifact artifact in Artifacts) {
            clonedArtifacts.Add(artifact.Clone());
        }
        return new() {
            Target = Target,
            Artifacts = clonedArtifacts
        };
    }
    public override string ToString() => TargetString;
    IValue IValue.Clone() => Clone();
}