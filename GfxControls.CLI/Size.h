#pragma once

namespace GfxControls
{
	[System::Runtime::InteropServices::StructLayout(System::Runtime::InteropServices::LayoutKind::Sequential)]
	public value struct Size
	{
	private:
		UINT _width;
		UINT _height;

	public:
		Size(UINT width, UINT height)
		{
			this->_width = width;
			this->_height = height;
		}

		property UINT Width
		{
			UINT get() {
				return _width;
			}
		}

		property UINT Height
		{
			UINT get() {
				return _height;
			}
		}
	};
}
