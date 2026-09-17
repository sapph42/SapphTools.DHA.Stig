namespace SapphTools.DHA.Stig.Common.WinApi;
public class LsaUnicodeStringWrapper : IDisposable, IEquatable<LsaUnicodeStringWrapper> {
    private LsaUnicodeString _unicodeString;
    private readonly SafeHBuffer _buffer;
    private bool darkWingDuck = false;
    private bool disposedValue;
    private readonly Guid theseus = Guid.NewGuid();

    public LsaUnicodeStringWrapper(string val) {
        ushort len = (ushort)val.Length;
        ushort maxLen = (ushort)(len + 2);
        _unicodeString = new() {
            Length = len,
            MaximumLength = maxLen
        };
        _buffer = SafeHBuffer.AllocFactory(maxLen);
        _buffer.WriteArray(
            0,
            System.Text.Encoding.Unicode.GetBytes(val),
            0,
            len
        );
        _buffer.Write(len, (short)0);
    }
    public LsaUnicodeString DangerousGetStruct(ref bool success) {
        if (darkWingDuck) {
            return _unicodeString;
        }
        _buffer.DangerousAddRef(ref success);
        _unicodeString.Buffer = _buffer.DangerousGetHandle();
        darkWingDuck = true;
        return _unicodeString;
    }
    public void DangerousReleaseStructBuffer() {
        if (!darkWingDuck) {
            return;
        }
        _unicodeString.Buffer = IntPtr.Zero;
        _buffer.DangerousRelease();
        darkWingDuck = false;
    }
    public override bool Equals(object? obj) {
        if (obj is not LsaUnicodeStringWrapper w) {
            return false;
        }
        return Equals(w);
    }
    public bool Equals(LsaUnicodeStringWrapper? other) {
        return theseus == other?.theseus;
    }
    public override int GetHashCode() {
        return theseus.GetHashCode();
    }
    protected virtual void Dispose(bool disposing) {
        if (!disposedValue) {
            if (disposing) {
                // dispose managed state (managed objects)
            }
            if (darkWingDuck) {
                _buffer.DangerousRelease();
            }
            _buffer.Dispose();
            disposedValue = true;
        }
    }
    ~LsaUnicodeStringWrapper() {
        Dispose(disposing: false);
    }
    public void Dispose() {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}