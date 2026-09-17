namespace SapphTools.DHA.Stig.Common.Classes; 
public static class ByteMagic {
    private static readonly uint[] _lookup32 = CreateLookup32();

    private static uint[] CreateLookup32() {
        var result = new uint[256];
        for (int i = 0; i < 256; i++) {
            string s=i.ToString("X2");
            result[i] = ((uint)s[0]) + ((uint)s[1] << 16);
        }
        return result;
    }

    public static string ToHexString(this byte[] bytes) {
        var lookup32 = _lookup32;
        var result = new char[bytes.Length * 5];
        for (int i = 0; i < bytes.Length; i++) {
            var val = lookup32[bytes[i]];
            result[5 * i]     = '0';
            result[5 * i + 1] = 'x';
            result[2 * i + 2] = (char)(val & 0xFFFF);
            result[2 * i + 3] = (char)(val >> 16);
            result[5 * i + 4] = ',';
        }
        return new string(result[..^1]);
    }

}
