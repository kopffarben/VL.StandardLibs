using imnodesNET;

namespace VL.ImNodes.Editors
{
    /// <summary>
    /// Disposable wrapper for ImNodes editor context, managing the native editor lifecycle
    /// </summary>
    public sealed class EditorContext : IDisposable
    {
        private IntPtr _contextHandle;
        private bool _disposed = false;
        
        public EditorContext()
        {
            _contextHandle = imnodes.CreateContext();
            if (_contextHandle == IntPtr.Zero)
                throw new InvalidOperationException("Failed to create ImNodes editor context");
        }

        public IntPtr Handle => _contextHandle;

        public void SetCurrent()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(EditorContext));
            
            imnodes.SetCurrentContext(_contextHandle);
        }

        public void Dispose()
        {
            if (!_disposed && _contextHandle != IntPtr.Zero)
            {
                imnodes.DestroyContext(_contextHandle);
                _contextHandle = IntPtr.Zero;
                _disposed = true;
            }
        }
    }
}