using System;
using System.Collections.Generic;

public abstract class DisposableNode : Node, IDisposable
{
    private bool _disposed = false;

    protected readonly List<Action> _cleanupActions = new List<Action>();

    protected void SubscribeToEvent(Action unsubscribeAction)
    {
        if (unsubscribeAction != null)
            _cleanupActions.Add(unsubscribeAction);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            foreach (var cleanup in _cleanupActions)
                cleanup?.Invoke();

            _cleanupActions.Clear();
        }

        _disposed = true;
    }

    ~DisposableNode()
    {
        Dispose(false);
    }
}
