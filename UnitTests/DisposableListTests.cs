namespace UnitTests;

[TestClass, TestCategory("Pure")]
public sealed class DisposableListTests {
    private sealed class DisposableSpy(bool throws = false) : IDisposable {
        public int Count;
        public void Dispose() { Count++; if (throws) throw new InvalidOperationException("dispose failure"); }
    }

    [TestMethod]
    public void DisposableListAttemptsEveryItemAndIsIdempotent() {
        DisposableSpy first = new(true), second = new();
        DisposableList<DisposableSpy> items = [first, second];
        items.Dispose(); items.Dispose();
        Assert.AreEqual(1, first.Count);
        Assert.AreEqual(1, second.Count);
    }
}
