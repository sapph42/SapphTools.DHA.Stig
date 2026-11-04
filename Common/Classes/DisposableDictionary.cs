namespace SapphTools.DHA.Stig.Common.Classes; 
public class DisposableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IDisposable
        where TKey : notnull
        where TValue : IDisposable {
    private bool disposedValue;
    protected virtual void Dispose(bool disposing) {
        if (!disposedValue) {
            if (disposing) {
                foreach (TValue item in Values) {
                    try { item.Dispose(); } catch { }
                }
            }
            disposedValue = true;
        }
    }
    public void Dispose() {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
