using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Interop;
using System.Windows;
using System.Windows.Input;
using GfxControls.Extensions;
using GfxControls.Interop;
using GfxControls.Shared.Interop;
using Size = GfxControls.Size;

namespace GfxControls.WPF.DirectX
{
    public class D3D11Host : HwndHost
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

        /// <summary>
        /// Event for when the control has been resized,
        /// returns the exact size of the native window.
        /// </summary>
        public event EventHandler<Size>? Resized;

        private IntPtr _hWndHost;
        private CLI.D3D11Host? _native;
        private bool _isMouseInside;
        private bool _isDragOver;

        public D3D11Host()
        {
 
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

        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            // Create the child window (HWND) for DirectX rendering
            try
            {
                _native = new CLI.D3D11Host(hwndParent.Handle, (uint)ActualWidth, (uint)ActualHeight, true);
                _hWndHost = _native.HWnd;
                if (AllowDrop)
                {
                    AllowDrop = true;
                }

                //RegisterDragDrop(_native.HWnd, this);
                Dispatcher.BeginInvoke(() => { DXInitialized?.Invoke(this, new EventArgs()); });

                return new HandleRef(this, _hWndHost);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to initialize GfxControls. Error: {ex}");
                GfxControlsError?.Invoke(this, ex);
                return new HandleRef();
            }
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            // Destroy the child window
            if (_native != null && hwnd.Handle == _native.HWnd)
            {
                DisposeNative();
            }
        }

        private void DisposeNative()
        {
            if (_native != null)
            {
                //RevokeDragDrop(_native.HWnd);
                NativeMethods.DragAcceptFiles(_native.HWnd, false);
                _native.Dispose();
                _native = null;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                DisposeNative();
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            var size = new Size((uint)ActualWidth, (uint)ActualHeight);

            if (_native?.IsRendering ?? false)
            {
                try
                {
                    _native.UpdateWindowSize(size.Width, size.Height);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to resize GfxControls window. Error: {ex}");
                    GfxControlsError?.Invoke(this, ex);
                }
            }

            Resized?.Invoke(this, size);
            base.OnRenderSizeChanged(sizeInfo);
        }

        protected override void OnDragEnter(DragEventArgs e)
        {
            // Handle when AllowDrop is set to false from base class.
            if (!AllowDrop)
            {
                AllowDrop = false;
                return;
            }

            base.OnDragEnter(e);
        }

        protected override nint WndProc(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
        {
            switch (msg)
            {
                case 0x0007: // WM_SETFOCUS
                    {
                        var args = new RoutedEventArgs(GotFocusEvent, this);
                        RaiseEvent(args);

                        var lostFocusedElement = FocusManager.GetFocusedElement(this) as UIElement;
                        var keyboardFocusArgs = new KeyboardFocusChangedEventArgs(
                            Keyboard.PrimaryDevice,
                            Environment.TickCount,
                            lostFocusedElement, // The element that lost focus
                            this                // The element that gained focus
                        )
                        {
                            RoutedEvent = PreviewGotKeyboardFocusEvent
                        };

                        RaiseEvent(args);
                        args.RoutedEvent = GotKeyboardFocusEvent;
                        RaiseEvent(args);
                    }
                    break;

                case 0x0008: // WM_KILLFOCUS
                    {
                        var args = new RoutedEventArgs(LostFocusEvent, this);
                        RaiseEvent(args);

                        var newFocusedElement = FocusManager.GetFocusedElement(this) as UIElement;
                        var keyboardFocusArgs = new KeyboardFocusChangedEventArgs(
                            Keyboard.PrimaryDevice,
                            Environment.TickCount,
                            this,               // The element that lost focus
                            newFocusedElement   // The element that gained focus
                        )
                        {
                            RoutedEvent = PreviewLostKeyboardFocusEvent
                        };

                        RaiseEvent(args);
                        args.RoutedEvent = LostKeyboardFocusEvent;
                        RaiseEvent(args);
                    }
                    break;

                case 0x0100: // WM_KEYDOWN
                    {
                        Key key = KeyInterop.KeyFromVirtualKey((int)wParam);

                        var args = new KeyEventArgs(
                            Keyboard.PrimaryDevice,
                            PresentationSource.FromVisual(this),
                            Environment.TickCount,
                            key)
                        {
                            RoutedEvent = KeyDownEvent
                        };

                        RaiseEvent(args);
                    }
                    break;

                case 0x0101: // WM_KEYUP
                    {
                        Key key = KeyInterop.KeyFromVirtualKey((int)wParam);

                        var args = new KeyEventArgs(
                            Keyboard.PrimaryDevice,
                            PresentationSource.FromVisual(this),
                            Environment.TickCount,
                            key)
                        {
                            RoutedEvent = KeyUpEvent
                        };

                        RaiseEvent(args);
                    }
                    break;

                // Add additional cases for other mouse buttons as needed
                case 0x0200: // WM_MOUSEMOVE
                    {
                        var args = new MouseEventArgs(Mouse.PrimaryDevice, Environment.TickCount)
                        {
                            RoutedEvent = PreviewMouseMoveEvent
                        };

                        RaiseEvent(args);
                        args.RoutedEvent = MouseMoveEvent;
                        RaiseEvent(args);

                        if (!_isMouseInside)
                        {
                            // Mouse Enter
                            _isMouseInside = true;
                            args.RoutedEvent = MouseEnterEvent;
                            RaiseEvent(args);

                            if (_native != null && _native.HWnd != IntPtr.Zero)
                            {
                                // Request to recieve WM_MOUSELEAVE message.
                                TRACKMOUSEEVENT trackMouseEvent = new TRACKMOUSEEVENT()
                                {
                                    cbSize = (uint)Marshal.SizeOf<TRACKMOUSEEVENT>(),
                                    dwFlags = TRACKMOUSEEVENT.Flags.TME_LEAVE,
                                    hwndTrack = _native.HWnd
                                };

                                NativeMethods.TrackMouseEvent(ref trackMouseEvent);
                            }
                        }
                    }
                    break;

                case 0x0201: // WM_LBUTTONDOWN
                    {
                        var args = new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left)
                        {
                            RoutedEvent = PreviewMouseDownEvent
                        };

                        RaiseEvent(args);
                        args.RoutedEvent = PreviewMouseLeftButtonDownEvent;
                        RaiseEvent(args);
                        args.RoutedEvent = MouseDownEvent;
                        RaiseEvent(args);
                        args.RoutedEvent = MouseLeftButtonDownEvent;
                        RaiseEvent(args);
                    }
                    break;

                case 0x0202: // WM_LBUTTONUP
                    {
                        var args = new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left)
                        {
                            RoutedEvent = PreviewMouseUpEvent
                        };

                        RaiseEvent(args);
                        args.RoutedEvent = PreviewMouseLeftButtonUpEvent;
                        RaiseEvent(args);
                        args.RoutedEvent = MouseUpEvent;
                        RaiseEvent(args);
                        args.RoutedEvent = MouseLeftButtonUpEvent;
                        RaiseEvent(args);
                    }
                    break;

                case 0x0203: // WM_LBUTTONDBLCLK                
                    {
#warning Double click events not tested
                        var args = new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left)
                        {
                            RoutedEvent = PreviewMouseDownEvent
                        };

                        RaiseEvent(args);
                        args.RoutedEvent = PreviewMouseLeftButtonDownEvent;
                        RaiseEvent(args);
                        args.RoutedEvent = MouseDownEvent;
                        RaiseEvent(args);
                        args.RoutedEvent = MouseLeftButtonDownEvent;
                        RaiseEvent(args);
                    }
                    break;

                case 0x0204: // WM_RBUTTONDOWN
                    {
                        var args = new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Right)
                        {
                            RoutedEvent = PreviewMouseRightButtonDownEvent
                        };

                        RaiseEvent(args);
                        args.RoutedEvent = PreviewMouseRightButtonDownEvent;
                        RaiseEvent(args);
                        args.RoutedEvent = MouseDownEvent;
                        RaiseEvent(args);
                        args.RoutedEvent = MouseRightButtonDownEvent;
                        RaiseEvent(args);
                    }
                    break;

                case 0x0205: // WM_RBUTTONUP
                    {
                        var args = new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Right)
                        {
                            RoutedEvent = PreviewMouseUpEvent
                        };

                        RaiseEvent(args);
                        args.RoutedEvent = PreviewMouseRightButtonUpEvent;
                        RaiseEvent(args);
                        args.RoutedEvent = MouseUpEvent;
                        RaiseEvent(args);
                        args.RoutedEvent = MouseRightButtonUpEvent;
                        RaiseEvent(args);
                    }
                    break;


                case 0x0206: // WM_RBUTTONDBLCLK                                  
                    {
                        var args = new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Right)
                        {
                            RoutedEvent = PreviewMouseDownEvent
                        };

                        RaiseEvent(args);
                        args.RoutedEvent = PreviewMouseRightButtonUpEvent;
                        RaiseEvent(args);
                        args.RoutedEvent = MouseUpEvent;
                        RaiseEvent(args);
                        args.RoutedEvent = MouseRightButtonUpEvent;
                        RaiseEvent(args);
                    }
                    break;

                case 0x0207: // WM_MBUTTONDOWN                  
                    {
                        var args = new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Middle)
                        {
                            RoutedEvent = PreviewMouseDownEvent
                        };

                        RaiseEvent(args);
                        args.RoutedEvent = MouseDownEvent;
                        RaiseEvent(args);
                    }
                    break;

                case 0x0208: // WM_MBUTTONUP                    
                    {
                        var args = new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Middle)
                        {
                            RoutedEvent = PreviewMouseUpEvent
                        };

                        RaiseEvent(args);
                        args.RoutedEvent = MouseUpEvent;
                        RaiseEvent(args);
                    }
                    break;

                case 0x0209: // WM_MBUTTONDBLCLK
                    {
                        var args = new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Middle)
                        {
                            RoutedEvent = PreviewMouseDownEvent
                        };

                        RaiseEvent(args);
                        args.RoutedEvent = MouseDownEvent;
                        RaiseEvent(args);
                    }
                    break;

                case 0x020A: // WM_MOUSEWHEEL
                    {
                        int delta = (short)(wParam >> 16 & 0xffff);
                        var args = new MouseWheelEventArgs(Mouse.PrimaryDevice, Environment.TickCount, delta)
                        {
                            RoutedEvent = PreviewMouseWheelEvent
                        };

                        RaiseEvent(args);
                        args.RoutedEvent = MouseWheelEvent;
                        RaiseEvent(args);
                    }
                    break;

                case 0x020B: // WM_XBUTTONDOWN                                  
                    {
                        MouseButton button = (MouseButton)((int)wParam >> 16);
                        var args = new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, button)
                        {
                            RoutedEvent = PreviewMouseDownEvent
                        };

                        RaiseEvent(args);
                        args.RoutedEvent = MouseDownEvent;
                        RaiseEvent(args);
                    }
                    break;

                case 0x020C: // WM_XBUTTONUP                                    
                    {
                        MouseButton button = (MouseButton)((int)wParam >> 16);
                        var args = new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, button)
                        {
                            RoutedEvent = PreviewMouseUpEvent
                        };

                        RaiseEvent(args);
                        args.RoutedEvent = MouseUpEvent;
                        RaiseEvent(args);
                    }
                    break;

                case 0x020D: // WM_XBUTTONDBLCLK                
                    {
                        MouseButton button = (MouseButton)((int)wParam >> 16);
                        var args = new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, button)
                        {
                            RoutedEvent = PreviewMouseDownEvent
                        };

                        RaiseEvent(args);
                        args.RoutedEvent = MouseDownEvent;
                        RaiseEvent(args);
                    }
                    break;

                case 0x0215: // WM_CAPTURECHANGED
                    {
#warning Not Implemented
                    }
                    break;

                case 0x0233: // WM_DROPFILES
                    {
                        Point mousePos = GetRelativeMousePos();

                        // Extract the dropped files
                        List<string> droppedFiles = new List<string>();
                        IntPtr hDrop = wParam;

                        uint fileCount = NativeMethods.DragQueryFile(hDrop, 0xFFFFFFFF, null, 0);
                        for (uint i = 0; i < fileCount; i++)
                        {
                            StringBuilder fileName = new StringBuilder(260);
                            NativeMethods.DragQueryFile(hDrop, i, fileName, fileName.Capacity);
                            droppedFiles.Add(fileName.ToString());
                        }

                        NativeMethods.DragFinish(hDrop); // Clean up

                        DragDropKeyStates keyStates = DragDropKeyStates.None;

                        if (Mouse.PrimaryDevice.LeftButton == MouseButtonState.Pressed)
                        {
                            keyStates |= DragDropKeyStates.LeftMouseButton;
                        }

                        if (Mouse.PrimaryDevice.RightButton == MouseButtonState.Pressed)
                        {
                            keyStates |= DragDropKeyStates.RightMouseButton;
                        }

                        if (Mouse.PrimaryDevice.MiddleButton == MouseButtonState.Pressed)
                        {
                            keyStates |= DragDropKeyStates.MiddleMouseButton;
                        }

                        if ((Keyboard.PrimaryDevice.Modifiers & ModifierKeys.Control) != 0)
                        {
                            keyStates |= DragDropKeyStates.ControlKey;
                        }

                        if ((Keyboard.PrimaryDevice.Modifiers & ModifierKeys.Shift) != 0)
                        {
                            keyStates |= DragDropKeyStates.ShiftKey;
                        }

                        if ((Keyboard.PrimaryDevice.Modifiers & ModifierKeys.Alt) != 0)
                        {
                            keyStates |= DragDropKeyStates.AltKey;
                        }

                        // Create and raise the DragEnter event
                        var eventArgs = DragEventExtensions.Create(
                            new DataObject(DataFormats.FileDrop, droppedFiles),
                            keyStates,
                            DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link,
                            this,
                            mousePos);

                        eventArgs.RoutedEvent = PreviewDragEnterEvent;
                        RaiseEvent(eventArgs);
                        eventArgs.RoutedEvent = DragEnterEvent;
                        RaiseEvent(eventArgs);

                        eventArgs.RoutedEvent = PreviewDropEvent;
                        RaiseEvent(eventArgs);
                        eventArgs.RoutedEvent = DropEvent;
                        RaiseEvent(eventArgs);
                    }
                    break;
#warning WM_DRAGOVER and WM_DRAGLEAVE messages aren't being received.
                case 0x0232: // WM_DRAGOVER
                    {
                        // TODO: WM_DRAGOVER not firing 
                        Point mousePos = GetRelativeMousePos();

                        var eventArgs = DragEventExtensions.Create(
                            null, //new DataObject(DataFormats.FileDrop, null),
                            DragDropKeyStates.None,
                            DragDropEffects.Copy,
                            this,
                            mousePos);

                        // Raise DragEnter if it's the first time entering
                        if (!_isDragOver)
                        {
                            _isDragOver = true;

                            eventArgs.RoutedEvent = PreviewDragEnterEvent;
                            RaiseEvent(eventArgs);
                            eventArgs.RoutedEvent = DragEnterEvent;
                            RaiseEvent(eventArgs);
                        }

                        eventArgs.RoutedEvent = PreviewDragOverEvent;
                        RaiseEvent(eventArgs);
                        eventArgs.RoutedEvent = DragOverEvent;
                        RaiseEvent(eventArgs);
                    }
                    break;
                case 0x0231: // WM_DRAGLEAVE
                    {
                        // TODO: WM_DRAGLEAVE not firing

                        // Reset the drag state and raise DragLeave
                        if (_isDragOver)
                        {
                            Point mousePos = GetRelativeMousePos();

                            _isDragOver = false;
                            var eventArgs = DragEventExtensions.Create(
                                null, //new DataObject(DataFormats.FileDrop, null),
                                DragDropKeyStates.None,
                                DragDropEffects.Copy,
                                this,
                                mousePos);

                            eventArgs.RoutedEvent = PreviewDragLeaveEvent;
                            RaiseEvent(eventArgs);
                            eventArgs.RoutedEvent = DragLeaveEvent;
                            RaiseEvent(eventArgs);
                        }
                    }
                    break;

                case 0x02A3: // WM_MOUSELEAVE
                    {
                        _isMouseInside = false;
                        var args = new MouseEventArgs(Mouse.PrimaryDevice, Environment.TickCount)
                        {
                            RoutedEvent = MouseLeaveEvent
                        };

                        RaiseEvent(args);
                    }
                    break;

                default:
                    break;
            }

            return base.WndProc(hwnd, msg, wParam, lParam, ref handled);
        }
    }
}
