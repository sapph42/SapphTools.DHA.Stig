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
    public virtual bool Equals(IValue? other) {
        if (other is null) {
            return false;
        }
        if (other as FileSystemAclValue is FileSystemAclValue fs) {
            return Sddl.Equals(fs.Sddl) && Target.Equals(fs.Target, StringComparison.OrdinalIgnoreCase);
        }
        if (other as RegistryAclValue is RegistryAclValue reg) {
            return Sddl.Equals(reg.Sddl) && Target.Equals(reg.Target, StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }
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
    public virtual bool Equals(IValue? other) {
        if (other is null) {
            return false;
        }
        if (other is SeRightsValue val) {
            return !AccountNames.Except(val.AccountNames).Any() &&
                !val.AccountNames.Except(AccountNames).Any() &&
                Target.Value.Equals(val.Target.Value, StringComparison.OrdinalIgnoreCase);
        }
        return false;
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
    public abstract bool Equals(IValue? other);
    string IValue.ToString() => throw new NotImplementedException();
}
public abstract class SamValue<T> : SamValue, IValue<T> {
    public abstract T Clone();
    public override string ToString() => TargetString;
    IValue IValue.Clone() => (IValue)Clone()!;
}
public abstract class SamValue<T, TSub> : SamValue<TSub> where T : struct  {
    public abstract T SamStruct { get; set; }
    public override bool Equals(IValue? other) {
        if (other is SamValue<T, TSub> sam) {
            return Target.Equals(sam.Target) && 
                SamStruct.Equals(sam.SamStruct);
        }
        return false;
    }
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
    public virtual bool Equals(IValue? other) {
        if (other is null) {
            return false;
        }
        if (other is RegistryKeyValue val) {
            return Target.Equals(val.Target, StringComparison.OrdinalIgnoreCase) &&
                Name.Equals(val.Name, StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }
    public override string ToString() => TargetString;
    IValue IValue.Clone() => Clone();
}
[JsonConverter(typeof(RegistryValueValueConverter))]
public class RegistryValueValue : IValue<RegistryValueValue> {
    [JsonInclude]
    public virtual TargetType Action => TargetType.RegistryValue;
    public required string Target { get; set; }
    public required string Name { get; set; }
    public required object? Data { get; set; }
    public RegistryValueKind Kind { get; set; }
    public bool Overwrite { get; set; }
    public virtual string TargetString => Target + "\\" + Name;
    public bool DataEquals(IValue other) {
        try {
            if (other is RegistryValueValue otherVal) {
                JsonElement firstEl = JsonSerializer
                    .Deserialize<JsonElement>(
                        JsonSerializer.Serialize(Data, Constants.JsonSerializerOptions),
                        Constants.JsonSerializerOptions
                    );
                JsonElement secondEl = JsonSerializer
                    .Deserialize<JsonElement>(
                        JsonSerializer.Serialize(otherVal.Data, Constants.JsonSerializerOptions),
                        Constants.JsonSerializerOptions
                    );
                return JsonElement.DeepEquals(firstEl, secondEl) && Kind == otherVal.Kind;
            }
        } catch { }
        return false;
    }
    public virtual RegistryValueValue Clone() {
        object? dataClone;
        if (Data is null) {
            dataClone = null;
        } else {
            dataClone = Kind switch {
                RegistryValueKind.String => new string((string)Data),
                RegistryValueKind.ExpandString => new string((string)Data),
                RegistryValueKind.Binary => ((byte[])Data).Clone(),
                RegistryValueKind.DWord => Data,
                RegistryValueKind.MultiString => ((string[])Data).Clone(),
                RegistryValueKind.QWord => Data,
                _ => Data.GetType().GetInterface("ICloneable") is not null ?
                    Data.GetType().GetMethod("Clone")?.Invoke(Data, null) :
                    Data
            };
        }
        return new() {
            Target = Target,
            Name = Name,
            Data = dataClone,
            Kind = Kind,
            Overwrite = Overwrite
        };
    }
    public virtual bool Equals(IValue? other) {
        if (other is null) {
            return false;
        }
        if (other is RegistryValueValue val) {
            return Target.Equals(val.Target, StringComparison.OrdinalIgnoreCase) &&
                Name.Equals(val.Name, StringComparison.OrdinalIgnoreCase) &&
                DataEquals(val);

        }
        return false;
    }
    public override string ToString() => TargetString;
    IValue IValue.Clone() => Clone();
}
[JsonConverter(typeof(RegistryValuePatternValueConverter))]
public class RegistryValuePatternValue : RegistryValueValue, IValue<RegistryValuePatternValue> {
    [JsonInclude]
    public override TargetType Action => TargetType.RegistryValuePattern;
    public Regex? TargetPattern { get; set; }
    public string? SubPath { get; set; }
    public Regex? PathPattern { get; set; }
    public RegistryValueValue? ResolvedTarget { get; set; }
    public override string TargetString => Target + 
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
    private string EquitableString => Target.ToLowerInvariant() +
        (TargetPattern is not null ?
            "\\" + TargetPattern.ToString() :
            string.Empty
        ) +
        (SubPath is not null ?
            "\\" + SubPath.ToString().ToLowerInvariant() :
            string.Empty
        ) +
         (PathPattern is not null ?
            "\\" + PathPattern.ToString() :
            string.Empty
        ) +
        "\\" + Name.ToLowerInvariant();
    public override RegistryValuePatternValue Clone() {
        object? dataClone;
        if (Data is null) {
            dataClone = null;
        } else {
            dataClone = Kind switch {
                RegistryValueKind.String => new string((string)Data),
                RegistryValueKind.ExpandString => new string((string)Data),
                RegistryValueKind.Binary => ((byte[])Data).Clone(),
                RegistryValueKind.DWord => Data,
                RegistryValueKind.MultiString => ((string[])Data).Clone(),
                RegistryValueKind.QWord => Data,
                _ => Data.GetType().GetInterface("ICloneable") is not null ?
                    Data.GetType().GetMethod("Clone")?.Invoke(Data, null) :
                    Data
            };
        }
        return new() {
            Target = Target,
            TargetPattern = TargetPattern,
            SubPath = SubPath,
            PathPattern = PathPattern,
            Name = Name,
            Data = dataClone,
            Kind = Kind,
            Overwrite = Overwrite,
            ResolvedTarget = ResolvedTarget?.Clone()
        };
    }
    public override bool Equals(IValue? other) {
        if (!base.Equals(other)) {
            return false;
        }
        if (other is RegistryValuePatternValue val) {
            if (ResolvedTarget is not null) {
                return EquitableString.Equals(val.EquitableString) && ResolvedTarget.Equals(val.ResolvedTarget);
            } else if (val.ResolvedTarget is not null) {
                return false;
            } else {
                return EquitableString.Equals(val.EquitableString);
            }
        }
        return false;
    }
    public override string ToString() => TargetString;
    IValue IValue.Clone() => Clone();
}
public class CertificatesValue : IValue<CertificatesValue> {
    [JsonInclude]
    public TargetType Action => TargetType.CertStore;
    public required string Target { get; set; }
    public required IArtifact Artifact { get; set; }
    public ArtifactCatalog CatalogArtifact => (ArtifactCatalog)Artifact;
    public string TargetString => $"Local Machine\\{Target}";

    public CertificatesValue Clone() {
        return new() {
            Target = Target,
            Artifact = Artifact.Clone()
        };
    }
    public bool Equals(IValue? other) {
        if (other is CertificatesValue val) {
            return Target.Equals(val.Target, StringComparison.OrdinalIgnoreCase) &&
                Artifact.Equals(val.Artifact);
        }
        return false;
    }
    public override string ToString() => TargetString;
    IValue IValue.Clone() => Clone();
}