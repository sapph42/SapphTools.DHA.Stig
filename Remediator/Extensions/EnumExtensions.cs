using System.Diagnostics;

namespace SapphTools.DHA.Stig.Remediator.Extensions; 
public static class EnumExtensions {
    public static T? CoalesceOr<T>(this T? enumVal, T? otherVal) where T : struct, Enum {
        if (enumVal is not T nonNulleV || otherVal is not T nonNulloV) {
            return enumVal ?? otherVal;
        }
        return Type.GetTypeCode(Enum.GetUnderlyingType(typeof(T))) switch {
            TypeCode.Byte => OrUnsigned(nonNulleV, nonNulloV),
            TypeCode.UInt16 => OrUnsigned(nonNulleV, nonNulloV),
            TypeCode.UInt32 => OrUnsigned(nonNulleV, nonNulloV),
            TypeCode.UInt64 => OrUnsigned(nonNulleV, nonNulloV),
            TypeCode.SByte => OrSigned(nonNulleV, nonNulloV),
            TypeCode.Int16 => OrSigned(nonNulleV, nonNulloV),
            TypeCode.Int32 => OrSigned(nonNulleV, nonNulloV),
            TypeCode.Int64 => OrSigned(nonNulleV, nonNulloV),
            _ => throw new UnreachableException()
        };
    }
    private static T OrUnsigned<T>(T val1, T val2) where T: struct, Enum {
        return (T)Enum.ToObject(typeof(T), Convert.ToUInt64(val1) | Convert.ToUInt64(val2));
    }
    private static T OrSigned<T>(T val1, T val2) where T : struct, Enum {
        return (T)Enum.ToObject(typeof(T), Convert.ToInt64(val1) | Convert.ToInt64(val2));
    }
}
