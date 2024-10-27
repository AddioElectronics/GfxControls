#pragma once

namespace GfxControls::Interop
{
	[System::Runtime::InteropServices::StructLayout(System::Runtime::InteropServices::LayoutKind::Sequential)]
	public value struct RECT
	{
	public:
		int left;
		int top;
		int right;
		int bottom;

		property int Width
		{
			int get() {
				return abs(left - right);
			}
		}

		property int Height
		{
			int get() {
				return abs(bottom - top);
			}
		}
	};
}
