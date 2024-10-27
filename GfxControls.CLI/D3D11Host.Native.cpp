#include "pch.h"
#include "D3D11Host.Native.h"
#include "HResultException.h"

#pragma unmanaged

namespace GfxControls::Native
{
    static std::unordered_map<HWND, D3D11HostNative*> _instances;

    LRESULT CALLBACK SharedWndProc(
        HWND   hWnd,
        UINT   msg,
        WPARAM wParam,
        LPARAM lParam)
    {
        auto it = _instances.find(hWnd);
        if (it != _instances.end()) {
            D3D11HostNative* pThis = it->second;
            pThis->WndProc(hWnd, msg, wParam, lParam);
        }

        // Default handling if instance not available
        return DefWindowProc(hWnd, msg, wParam, lParam);
    }

    LRESULT D3D11HostNative::WndProc(
        HWND   hWnd,
        UINT   msg,
        WPARAM wParam,
        LPARAM lParam)
    {
        switch (msg)
        {
        //case WM_DESTROY:
        //    PostQuitMessage(0);
        //    break;
        default:
            if (_forwardMessages) {
                SendMessage(_parentHWnd, msg, wParam, lParam);
            }
            //PostMessageW(_parentHWnd, msg, wParam, lParam);
            return DefWindowProc(hWnd, msg, wParam, lParam);
        }

        return 0;
    }

    D3D11HostNative::D3D11HostNative(HWND parentHWnd, UINT width, UINT height, bool forwardMessages)
    {
        HINSTANCE hInstance = GetModuleHandle(NULL);
        if (!hInstance) {
            throw std::exception("Failed to get module handle.");
        }
        _parentHWnd = parentHWnd;
        _forwardMessages = forwardMessages;
        _width = width;
        _height = height;
        //_wndProc = wndProc;

        // Create the window
        WNDCLASSEX wcex = {};
        wcex.cbSize = sizeof(WNDCLASSEX);
        wcex.style = CS_HREDRAW | CS_VREDRAW;
        wcex.lpfnWndProc = SharedWndProc;
        wcex.cbClsExtra = 0;
        wcex.cbWndExtra = 0;
        wcex.hInstance = hInstance;
        wcex.hCursor = LoadCursor(NULL, IDC_ARROW);
        wcex.hbrBackground = (HBRUSH)(COLOR_WINDOW + 1);
        wcex.lpszClassName = L"GfxControls";
        RegisterClassEx(&wcex);

        _hWnd = CreateWindowEx(
            0,
            L"GfxControls",
            L"SwapChainPanel",
            WS_CHILD | WS_VISIBLE | WS_OVERLAPPED,
            0, 0,
            width,
            height,
            parentHWnd,
            NULL,
            NULL,
            NULL);

        if (!_hWnd) {
            throw std::exception("Failed to create window.");
        }

        _instances[_hWnd] = this;

        if (!ShowWindow(_hWnd, SW_SHOW)) {
            throw std::exception("Failed to show window.");
        }

        UpdateWindow(_hWnd);
        InitializeDX(_hWnd, width, height);

        
        _initialized = true;
    }

    D3D11HostNative::~D3D11HostNative()
    {
        Release();

        if (_initialized && _hWnd)
        {
            auto it = _instances.find(_hWnd);
            if (it != _instances.end()) {
                _instances.erase(_hWnd);
            }
        }
    }

    void D3D11HostNative::InitializeDX(HWND hWnd, UINT width, UINT height)
    {
        HRESULT hr = 0;

        hr = CreateDXGIFactory(__uuidof(IDXGIFactory), (void**) &_factory);
        if (FAILED(hr)) {
            throw HResultException(hr, "Failed to create IDXGIFactory.");
        }

        DXGI_SWAP_CHAIN_DESC scd = {};
        scd.BufferCount = 2;
        scd.BufferDesc.Width = width;
        scd.BufferDesc.Height = height;
        scd.BufferDesc.Format = DXGI_FORMAT_R8G8B8A8_UNORM;
        scd.BufferDesc.RefreshRate.Numerator = 60;
        scd.BufferDesc.RefreshRate.Denominator = 1;
        scd.SampleDesc.Count = 1;
        scd.SampleDesc.Quality = 0;
        scd.SwapEffect = DXGI_SWAP_EFFECT_DISCARD;
        scd.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT;
        scd.OutputWindow = hWnd;
        scd.Windowed = TRUE;

        D3D_FEATURE_LEVEL featureLevel = D3D_FEATURE_LEVEL_11_0;

        hr = D3D11CreateDeviceAndSwapChain(
            nullptr,
            D3D_DRIVER_TYPE_HARDWARE,
            nullptr,
            0,
            nullptr,//&featureLevel,
            0,//1,
            D3D11_SDK_VERSION,
            &scd,
            &_swapChain,
            &_d3dDevice,
            nullptr,
            &_context
        );
        if (FAILED(hr)) {
            throw HResultException(hr, "Failed to create ID3D11Device and IDXGISwapChain.");
        }

        hr = _swapChain->QueryInterface<IDXGISwapChain1>(&_swapChain1);
        if (FAILED(hr)) {
            throw HResultException(hr, "Failed to create IDXGISwapChain1.");
        }

        hr = _factory->MakeWindowAssociation(hWnd, DXGI_MWA_NO_WINDOW_CHANGES);
        if (FAILED(hr)) {
            throw HResultException(hr, "Failed to associate DirectX with Window.");
        }
    }

    void D3D11HostNative::ReleaseDX()
    {
		if (_swapChain1) _swapChain1->Release();
		if (_swapChain) _swapChain->Release();
		if (_context) _context->Release();
		if (_d3dDevice) _d3dDevice->Release();
		if (_dxgiDevice) _dxgiDevice->Release();
		if (_factory) _factory->Release();

        _factory = nullptr;
        _d3dDevice = nullptr;
        _context = nullptr;
        _dxgiDevice = nullptr;
        _swapChain = nullptr;
        _swapChain1 = nullptr;
	}

    void D3D11HostNative::DestroyWindow()
    {
        if (_hWnd)
        {
            ::DestroyWindow(_hWnd);
            _hWnd = nullptr;
        }
    }

    void D3D11HostNative::UpdateWindowSize(UINT width, UINT height)
    {
        if (width != _width ||
            height != _height)
        {
            _width = width;
            _height = height;

            if (!SetWindowPos(_hWnd, 0, 0, 0, width, height, SWP_NOZORDER | SWP_NOACTIVATE)) {
                throw std::exception("Failed to resize window.");
            }
        }
    }

    void D3D11HostNative::Release()
    {
        if (!_isDisposed && _initialized)
        {
            ReleaseDX();
            DestroyWindow();

            _isDisposed = true;
            _initialized = false;
        }
    }
}