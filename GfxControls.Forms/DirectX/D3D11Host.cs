using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using GfxControls.Interop;

namespace GfxControls.Forms
{
    [ComVisible(true)]
    [Guid("822E7788-4B40-4C3D-8DC2-D08B5376C958")]
    public partial class D3D11Host : Control
    {
        /// <summary>
        /// Event fired when the native window has been created, and DirectX initialized.
        /// </summary>
        public event EventHandler? DXInitialized;

        /// <summary>
        /// Event fired when the <see cref="CLI.GfxControls"/> throws an exception.
        /// Passes the internal exception to the event.
        /// </summary>
        public event EventHandler<Exception>? GfxControlsError;

        private IntPtr _parentHWnd;
        private CLI.D3D11Host? _native;
        private bool _isMouseInside;
        private bool _isDragOver;
        private MouseButtons _mouseButtons;

        public D3D11Host()
        {
            _parentHWnd = this.Handle;
            InitializeComponent();
            _native = new CLI.D3D11Host(_parentHWnd, (uint)ClientSize.Width, (uint)ClientSize.Height, true);

            Disposed += D3D11Host_Disposed;
        }

        /// <summary>
        /// Gets a pointer to the native IDXGISwapChain.
        /// </summary>
        public IntPtr SwapChain => _native?.SwapChain ?? IntPtr.Zero;

        /// <summary>
        /// Gets a pointer to the native IDXGISwapChain1.
        /// </summary>
        public IntPtr SwapChain1 => _native?.SwapChain1 ?? IntPtr.Zero;

        /// <summary>
        /// Gets a pointer to the native ID3D11DeviceContext.
        /// </summary>
        public IntPtr DeviceContext => _native?.DeviceContext ?? IntPtr.Zero;

        /// <summary>
        /// Gets a pointer to the native ID3D11Device.
        /// </summary>
        public IntPtr D3DDevice => _native?.D3DDevice ?? IntPtr.Zero;

        /// <summary>
        /// Gets a pointer to the native IDXGIDevice.
        /// </summary>
        public IntPtr DXGIDevice => _native?.DXGIDevice ?? IntPtr.Zero;

        /// <summary>
        /// Returns <see langword="true"/> if the DirectX window is initialized.
        /// </summary>
        public bool IsDXInitialized => _native?.IsRendering ?? false;

        /// <summary>
        /// Gets or sets a value indicating whether this element can be used as the target
        ///  of a drag-and-drop operation. This is a dependency property.
        /// </summary>
        public new bool AllowDrop
        {
            get => base.AllowDrop;
            set
            {
                base.AllowDrop = value;
                if (_native != null && _native.HWnd != IntPtr.Zero)
                {
                    NativeMethods.DragAcceptFiles(_native.HWnd, value);
                }
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            if (_native?.IsRendering ?? false)
            {
                try
                {
                    _native.UpdateWindowSize((uint)Width, (uint)Height);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to resize GfxControls window. Error: {ex}");
                    GfxControlsError?.Invoke(this, ex);
                }
            }
        }

        private void D3D11Host_Disposed(object? sender, EventArgs e)
        {
            _native?.Dispose();
            _native = null;
        }

        public Point GetRelativeMousePos()
        {
            if (_native == null)
            {
                throw new NullReferenceException("GfxControls native object is null.");
            }

            if (_native.HWnd == IntPtr.Zero)
            {
                throw new InvalidOperationException("GfxControls window handle is null.");
            }

            NativeMethods.GetCursorPos(out POINT point);
            NativeMethods.ScreenToClient(_native.HWnd, out POINT wndPoint);
            return new Point(point.x + wndPoint.x, point.y + wndPoint.y);
        }

        protected override void OnDragEnter(DragEventArgs drgevent)
        {
            // Handle when AllowDrop is set false from base class.
            if (!AllowDrop)
            {
                AllowDrop = false;
                return;
            }

            base.OnDragEnter(drgevent);
        }

        protected override void WndProc(ref Message m)
        {
            IntPtr hWnd = m.HWnd;
            nint wParam = m.WParam;
            nint lParam = m.LParam;
            int msg = m.Msg;

            switch (msg)
            {
                case 0x0233: // WM_DROPFILES
                    {
                        Point mousePos = GetRelativeMousePos();

                        // Extract the dropped files
                        List<string> droppedFiles = new List<string>();
                        IntPtr hDrop = m.WParam;

                        uint fileCount = NativeMethods.DragQueryFile(hDrop, 0xFFFFFFFF, null, 0);
                        for (uint i = 0; i < fileCount; i++)
                        {
                            StringBuilder fileName = new StringBuilder(260);
                            NativeMethods.DragQueryFile(hDrop, i, fileName, fileName.Capacity);
                            droppedFiles.Add(fileName.ToString());
                        }

                        NativeMethods.DragFinish(hDrop); // Clean up

                        DragEventArgs args = new DragEventArgs(
                            new DataObject(DataFormats.FileDrop, droppedFiles),
                            (int)Control.ModifierKeys | (int)Control.MouseButtons,
                            mousePos.X,
                            mousePos.Y,
                            DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link,
                            DragDropEffects.Copy);


                        OnDragDrop(args);
                        m.Result = IntPtr.Zero;
                    }
                    return;
            }

            base.WndProc(ref m);
        }
    }
}
