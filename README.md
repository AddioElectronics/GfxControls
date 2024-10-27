# GfxControls

**GfxControls** is a lightweight C# library containing WPF and WinForms controls that enables embedding a DirectX, OpenGL, and Vulkan windows within an application.

**Note:** _Only DirectX via WPF is currently available._

## Getting Started

### DirectX 

1. **Add a Reference**  
   Reference `GfxControls.WPF.dll` or `GfxControls.Forms.dll` in your project.

2. **Add the Control in XAML**  
   Include `GfxControls.WPF.DirectX.D3D11Host` in your XAML file.

3. **Initialize DirectX**  
   Wait for `D3D11Host.IsDXInitialized` to be **true** or subscribe to the `D3D11Host.DXInitialized` event before accessing DirectX resources.

4. **Access DirectX Interfaces**  
   Use the provided DX interfaces directly in your native code or cast them to managed objects using libraries like SharpDX.
   You can access these through properties that return an `IntPtr`.
   

**Exposed Interfaces**:
The following DirectX interfaces are available:
- `IDXGIDevice`
- `IDXGISwapChain`
- `IDXGISwapChain1`
- `ID3D11Device`
- `ID3D11DeviceContext`

**Example: Convert Interface Pointers to SharpDX Objects**:

You can cast the exposed interfaces to SharpDX objects using `ComObject.FromPointer`:
```csharp
//D3D11Host dxHost;
SharpDX.DXGI.Device dxdevice = ComObject.FromPointer<SharpDX.DXGI.Device>(dxHost.DXGIDevice);
SharpDX.DXGI.SwapChain swapChain = ComObject.FromPointer<SharpDX.DXGI.SwapChain>(dxHost.SwapChain);
SharpDX.DXGI.SwapChain1 swapChain1 = ComObject.FromPointer<SharpDX.DXGI.SwapChain1>(dxHost.SwapChain1);
SharpDX.Direct3D11.Device device = ComObject.FromPointer<SharpDX.Direct3D11.Device>(dxHost.D3DDevice);
SharpDX.Direct3D11.DeviceContext context = ComObject.FromPointer<SharpDX.Direct3D11.DeviceContext>(dxHost.DeviceContext);
```

**Explore These Example Projects**:
- [WPF SharpDX Example](https://github.com/AddioElectronics/GfxControls/Examples/SharpDXExample)
- **WPF CLI Example** _(Coming Soon)_

### OpenGL

**Coming Soon**

### Vulkan

**Coming Soon**

## Building the Project

1. **Select Configuration**
   - Choose the build configuration you need, such as `Debug`, `Release`, or any custom configurations. The project includes multiple configurations for different .NET frameworks, including .NET 8, .NET 6, .NET Core 3.1, and .NET Framework 4.7.

2. **Restore NuGet Packages (Optional)**
   - Open the `Package Manager Console` in Visual Studio and run:
     ```bash
     dotnet restore
     ```
   - Restore the CLI project using Visual Studio Command Prompt:
     ```bash
     msbuild GfxControls.CLI\GfxControls.CLI.vcxproj /t:Restore
     ```

3. **Build**
   - Compile the _WpfGfxControls_ or an example project in Visual Studio.

### Notes for Switching Between Framework Configurations

The CLI project might not build after changing configurations or frameworks. Here’s how to resolve the build issues:
- **Clean the Project** in Visual Studio (`GfxControls.CLI` > `Clean`).
- **Restore NuGet Packages** using MSBuild.
- **Reload the Project** in Visual Studio to ensure settings are updated.
- **Delete Intermediate and Output Directories** like `bin`, `obj` and `Framework` if any build issues persist.
