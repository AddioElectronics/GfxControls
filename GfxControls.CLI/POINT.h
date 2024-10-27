#pragma once

namespace GfxControls::Interop
{
	[System::Runtime::InteropServices::StructLayout(System::Runtime::InteropServices::LayoutKind::Sequential)]
		public value struct POINT
	{
	public:
		int x;
		int y;
	};
}
