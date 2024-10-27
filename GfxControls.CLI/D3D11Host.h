#pragma once
#include "D3D11Host.Native.h"

using namespace System;

namespace GfxControls::CLI
{
	public ref class D3D11Host : public IDisposable
	{
	private:
		HWND _hWnd = nullptr;
		UINT _width = 0;
		UINT _height = 0;

		GfxControls::Native::D3D11HostNative* _native = nullptr;

		bool _rendering = false;
		bool _isDisposed = false;

		literal String^ ErrorFormat = "{0} (Error Code: {1})";
		literal String^ ErrorFormatHex = "{0} (Error Code: 0x{1:X})";

		D3D11Host();



	public:
		D3D11Host(IntPtr parentHwnd, UINT width, UINT height);
		~D3D11Host();

		void UpdateWindowSize(UINT width, UINT height);

		LRESULT WndProc(
			HWND   hwnd,
			UINT   msg,
			WPARAM wParam,
			LPARAM lParam);

		property bool IsRendering
		{
			bool get() {
				return _rendering;
			}
		}

		property IntPtr HWnd
		{
			IntPtr get() {
				return IntPtr(_hWnd);
			}
		}

		property IntPtr SwapChain
		{
			IntPtr get() {
				return IntPtr(_native->GetSwapChain());
			}
		}

		property IntPtr SwapChain1
		{
			IntPtr get() {
				return IntPtr(_native->GetSwapChain1());
			}
		}

		property IntPtr DeviceContext
		{
			IntPtr get() {
				return IntPtr(IntPtr(_native->GetDeviceContext()));
			}
		}

		property IntPtr D3DDevice
		{
			IntPtr get() {
				return IntPtr(IntPtr(_native->GetD3D11Device()));
			}
		}

		property IntPtr DXGIDevice
		{
			IntPtr get() {
				return IntPtr(IntPtr(_native->GetDXGIDevice()));
			}
		}

	private:
		void ReleaseNative();
		void CheckDisposed();
		static Object^ GetLastErrorAsObject();
	};
}
