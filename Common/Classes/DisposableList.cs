namespace SapphTools.DHA.Stig.Common.Classes;
public class DisposableList<T> : List<T>, IDisposable where T : IDisposable {
    private bool disposedValue;
    protected virtual void Dispose(bool disposing) {
        if (!disposedValue) {
            if (disposing) {
                foreach (T item in this) {
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
