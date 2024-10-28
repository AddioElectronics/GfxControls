#include "pch.h"

using namespace System;
using namespace System::Reflection;
using namespace System::Runtime::CompilerServices;
using namespace System::Runtime::InteropServices;
using namespace System::Security::Permissions;

#ifndef VERSION_PREFIX
#error VERSION_PREFIX is not defined.
#else

#if UNICODE
#define VERSION L_VERSION_PREFIX
#else
#define VERSION VERSION_PREFIX
#endif
#endif

[assembly:AssemblyTitleAttribute(L"GfxControlsCLI")];
[assembly:AssemblyDescriptionAttribute(L"")];
[assembly:AssemblyConfigurationAttribute(L"")];
[assembly:AssemblyCompanyAttribute(L"Xorrupt")];
[assembly:AssemblyProductAttribute(L"GfxControlsCLI")];
[assembly:AssemblyCopyrightAttribute(L"Copyright (c)  2024")];
[assembly:AssemblyTrademarkAttribute(L"")];
[assembly:AssemblyCultureAttribute(L"")];

[assembly:AssemblyVersionAttribute(VERSION)];

[assembly:ComVisible(false)];
