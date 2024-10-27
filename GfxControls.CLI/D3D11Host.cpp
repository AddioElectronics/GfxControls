#include "pch.h"
#include "D3D11Host.h"
#include "HResultException.h"

using namespace System::Threading;

namespace GfxControls::CLI
{
	D3D11Host::D3D11Host() { }

	D3D11Host::D3D11Host(IntPtr parentHwnd, UINT width, UINT height, bool forwardMessages)
	{
		this->_width = width;
		this->_height = height;

		try
		{
			this->_width = width;
			this->_height = height;

			this->_native = new GfxControls::Native::D3D11HostNative((HWND)parentHwnd.ToPointer(), width, height, forwardMessages);
			this->_hWnd = _native->GetHWND();
			_rendering = true;
		}
		catch (const HResultException& ex)
		{
			ReleaseNative();
			auto managed = gcnew Exception(String::Format(ErrorFormatHex, gcnew String(ex.what()), (int)ex.HResult()));
#ifndef NET_FRAMEWORK
			managed->HResult = (int)ex.HResult();
#endif
			throw managed;
		}
		catch (const std::exception& ex)
		{
			ReleaseNative();
			throw gcnew Exception(String::Format(ErrorFormat, gcnew String(ex.what()), GetLastErrorAsObject()));
		}
	}

	D3D11Host::~D3D11Host()
	{
		ReleaseNative();
	}

	void D3D11Host::UpdateWindowSize(UINT width, UINT height)
	{
		CheckDisposed();
		this->_width = width;
		this->_height = height;

		if (_native && _rendering)
		{
			try
			{
				_native->UpdateWindowSize(width, height);				
			}
			catch (const std::exception& ex)
			{
				ReleaseNative();
				throw gcnew Exception(String::Format(ErrorFormat, gcnew String(ex.what()), GetLastErrorAsObject()));
			}	
		}
	}

	LRESULT D3D11Host::WndProc(
		HWND   hWnd,
		UINT   msg,
		WPARAM wParam,
		LPARAM lParam)
	{
		switch (msg)
		{
		case WM_LBUTTONDOWN: // WM_LBUTTONDOWN

			break;

		case WM_LBUTTONUP: // WM_LBUTTONUP

			break;

		case WM_MOUSEMOVE: // WM_MOUSEMOVE

			break;
		}

		return 0;
	}

	void D3D11Host::ReleaseNative()
	{
		if (!_isDisposed)
		{
			if (_native) delete _native;

			_hWnd = nullptr;
			_native = nullptr;
			_width = 0;
			_height = 0;
			_rendering = false;
			_isDisposed = true;
		}
	}

	void D3D11Host::CheckDisposed()
	{
		if (_isDisposed) {
			throw gcnew System::InvalidOperationException("SwapChainPanel is disposed.");
		}
	}

	Object^ D3D11Host::GetLastErrorAsObject()
	{
		int error = GetLastError();
		return error;
	}
}