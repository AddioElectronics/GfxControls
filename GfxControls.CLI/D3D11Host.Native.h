#pragma once

namespace GfxControls::Native
{
	class D3D11HostNative
	{
	private:
		HWND _hWnd = nullptr;
		HWND _parentHWnd = nullptr;

		IDXGIFactory* _factory = nullptr;		
		ID3D11Device* _d3dDevice = nullptr;
		ID3D11DeviceContext* _context = nullptr;
		IDXGIDevice* _dxgiDevice = nullptr;
		IDXGISwapChain* _swapChain = nullptr;
		IDXGISwapChain1* _swapChain1 = nullptr;

		UINT _width = 0;
		UINT _height = 0;

		bool _initialized = false;
		bool _forwardMessages = false;
		bool _isDisposed = false;

	public:
		D3D11HostNative(HWND parentHWnd, UINT width, UINT height, bool forwardMessages);
		~D3D11HostNative();

		void UpdateWindowSize(UINT width, UINT height);

		LRESULT CALLBACK WndProc(
			HWND   hwnd,
			UINT   msg,
			WPARAM wParam,
			LPARAM lParam);
		
		inline HWND GetHWND() {
			return _hWnd;
		}

		inline bool IsInitialized() {
			return _initialized;
		}

		inline UINT Width() {
			return _width;
		}

		inline UINT Height() {
			return _height;
		}

		inline IDXGISwapChain* GetSwapChain() {
			return _swapChain;
		}

		inline IDXGISwapChain1* GetSwapChain1() {
			return _swapChain1;
		}

		inline IDXGIDevice* GetDXGIDevice() {
			return _dxgiDevice;
		}

		inline ID3D11Device* GetD3D11Device() {
			return _d3dDevice;
		}

		inline ID3D11DeviceContext* GetDeviceContext() {
			return _context;
		}

	private:
		void InitializeDX(HWND hWnd, UINT width, UINT height);
		void Release();
		void DestroyWindow();
		void ReleaseDX();
	};


	
}