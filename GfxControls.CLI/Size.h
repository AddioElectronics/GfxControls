#pragma once

namespace GfxControls
{
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
