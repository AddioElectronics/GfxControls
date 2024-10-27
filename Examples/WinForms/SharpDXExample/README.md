# SharpDXExample

This sample demonstrates how to use the DirectXHost control in conjunction with SharpDX.


_The code was taken from the [SharpDX samples](https://github.com/sharpdx/SharpDX-Samples/blob/master/Desktop/Direct3D11/MiniCube/Program.cs), and modified to work with the control._

 ## Overview

### Initialization of the D3D11Host

`D3D11Host` derives from `System.Windows.Forms.Control`, which will automatically initialize the native window and DirectX.

When `D3D11Host.IsDXInitialized` evaluates to true, the native DirectX pointers can be accessed to start rendering.
Another option is to subscribe to the `D3D11Host.DXInitialized` event.

Subscribe to the `Resized` event to handle the resizing of the swap chain buffer appropriately.

```c#
private SwapChain? swapChain;
private SharpDX.Direct3D11.Device? device;
private DeviceContext? context;
private Stopwatch? stopwatch;
 
public MainWindow()
{
    InitializeComponent();

    if (dxHost.IsDXInitialized)
    {
        Initialize();
    }
    else
    {
        dxHost.DXInitialized += DxWindow_WindowCreated;
    }            
}

private void DxWindow_WindowCreated(object? sender, EventArgs e)
{
    Initialize();
}

private void Initialize()
{
    stopwatch = new Stopwatch();
    stopwatch.Start();

    dxHost.SizeChanged += DxHost_SizeChanged;
	dxHost.MouseMove += DxHost_MouseMove;
	dxHost.MouseWheel += DxHost_MouseWheel;

    InitializeGraphics();
}

private void DxHost_SizeChanged(object? sender, EventArgs e)
{
    userResized = true;
}
```

### Scene Initialization

In this section, the scene is initialized, and a render thread is established.

The native DirectX pointers can be used to create **SharpDX** objects via `ComObject.FromPointer`.
If needed, interfaces for `IDXGIDevice` and `IDXGISwapChain1` are also made available.

```c#
private void InitializeGraphics()
{
	stopwatch = new Stopwatch();
	stopwatch.Start();

	dxHost.Resized += DxHost_Resized;
	dxHost.MouseMove += DxWindow_MouseMove;
	dxHost.MouseWheel += DxWindow_MouseWheel;
	
	swapChain = ComObject.FromPointer<SwapChain>(dxHost.SwapChain);
	context = ComObject.FromPointer<DeviceContext>(dxHost.DeviceContext);
	device = ComObject.FromPointer<SharpDX.Direct3D11.Device>(dxHost.D3DDevice);
 
    desc = swapChain.Description;

    // Compile Vertex and Pixel shaders
    var vertexShaderByteCode = ShaderBytecode.CompileFromFile("MiniCube.fx", "VS", "vs_4_0");
    var vertexShader = new VertexShader(device, vertexShaderByteCode);

    var pixelShaderByteCode = ShaderBytecode.CompileFromFile("MiniCube.fx", "PS", "ps_4_0");
    var pixelShader = new PixelShader(device, pixelShaderByteCode);

    var signature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
    // Layout from VertexShader input signature
    var layout = new InputLayout(device, signature, new[]
    {
                new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
                new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 16, 0)
            });

    // Instantiate Vertex buffer from vertex data
    var vertices = Buffer.Create(device, BindFlags.VertexBuffer, new[]
                          {
                              new Vector4(-1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f), // Front
                              new Vector4(-1.0f,  1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                              new Vector4( 1.0f,  1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                              new Vector4(-1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                              new Vector4( 1.0f,  1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                              new Vector4( 1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f),

                              new Vector4(-1.0f, -1.0f,  1.0f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f), // BACK
                              new Vector4( 1.0f,  1.0f,  1.0f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                              new Vector4(-1.0f,  1.0f,  1.0f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                              new Vector4(-1.0f, -1.0f,  1.0f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                              new Vector4( 1.0f, -1.0f,  1.0f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                              new Vector4( 1.0f,  1.0f,  1.0f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f),

                              new Vector4(-1.0f, 1.0f, -1.0f,  1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f), // Top
                              new Vector4(-1.0f, 1.0f,  1.0f,  1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f),
                              new Vector4( 1.0f, 1.0f,  1.0f,  1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f),
                              new Vector4(-1.0f, 1.0f, -1.0f,  1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f),
                              new Vector4( 1.0f, 1.0f,  1.0f,  1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f),
                              new Vector4( 1.0f, 1.0f, -1.0f,  1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f),

                              new Vector4(-1.0f,-1.0f, -1.0f,  1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f), // Bottom
                              new Vector4( 1.0f,-1.0f,  1.0f,  1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f),
                              new Vector4(-1.0f,-1.0f,  1.0f,  1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f),
                              new Vector4(-1.0f,-1.0f, -1.0f,  1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f),
                              new Vector4( 1.0f,-1.0f, -1.0f,  1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f),
                              new Vector4( 1.0f,-1.0f,  1.0f,  1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f),

                              new Vector4(-1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 1.0f, 1.0f), // Left
                              new Vector4(-1.0f, -1.0f,  1.0f, 1.0f), new Vector4(1.0f, 0.0f, 1.0f, 1.0f),
                              new Vector4(-1.0f,  1.0f,  1.0f, 1.0f), new Vector4(1.0f, 0.0f, 1.0f, 1.0f),
                              new Vector4(-1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 1.0f, 1.0f),
                              new Vector4(-1.0f,  1.0f,  1.0f, 1.0f), new Vector4(1.0f, 0.0f, 1.0f, 1.0f),
                              new Vector4(-1.0f,  1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 1.0f, 1.0f),

                              new Vector4( 1.0f, -1.0f, -1.0f, 1.0f), new Vector4(0.0f, 1.0f, 1.0f, 1.0f), // Right
                              new Vector4( 1.0f,  1.0f,  1.0f, 1.0f), new Vector4(0.0f, 1.0f, 1.0f, 1.0f),
                              new Vector4( 1.0f, -1.0f,  1.0f, 1.0f), new Vector4(0.0f, 1.0f, 1.0f, 1.0f),
                              new Vector4( 1.0f, -1.0f, -1.0f, 1.0f), new Vector4(0.0f, 1.0f, 1.0f, 1.0f),
                              new Vector4( 1.0f,  1.0f, -1.0f, 1.0f), new Vector4(0.0f, 1.0f, 1.0f, 1.0f),
                              new Vector4( 1.0f,  1.0f,  1.0f, 1.0f), new Vector4(0.0f, 1.0f, 1.0f, 1.0f),
                    });

    // Create Constant Buffer
    contantBuffer = new Buffer(device, Utilities.SizeOf<Matrix>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);

    // Prepare All the stages
    context.InputAssembler.InputLayout = layout;
    context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
    context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertices, Utilities.SizeOf<Vector4>() * 2, 0));
    context.VertexShader.SetConstantBuffer(0, contantBuffer);
    context.VertexShader.Set(vertexShader);
    context.PixelShader.Set(pixelShader);

    // Prepare matrices
    //view = Matrix.LookAtLH(new Vector3(0, 0, -5), new Vector3(0, 0, 0), Vector3.UnitY);
    SetViewMatrix();
    proj = Matrix.Identity;

    if (width == 0)
    {
        width = (int)dxHost.ActualWidth;
    }

    if (height == 0)
    {
        height = (int)dxHost.ActualHeight;
    }

    rendering = true;
    renderThread = new Thread(RenderLoop);
    renderThread.Start();
}
```

### The Render Loop

The main rendering function first resizes the swap chain if required, and then proceeds to draw the scene.

```c#
void RenderLoop(object? state)
{
    Stopwatch sw = Stopwatch.StartNew();
    while (rendering)
    {
        if (sw.Elapsed.Milliseconds >= 16)
        {
            Render();
            sw.Restart();
        }
    }
}

private void Render()
{
    // If control resized
    if (userResized)
    {
        ResizeSwapChain();
    }

    var time = stopwatch.ElapsedMilliseconds / timeDivisor;

    var viewProj = Matrix.Multiply(view, proj);

    // Clear views
    context.ClearDepthStencilView(depthView, DepthStencilClearFlags.Depth, 1.0f, 0);
    context.ClearRenderTargetView(renderView, Color.Black);

    // Update WorldViewProj Matrix
    var worldViewProj = Matrix.RotationX(time) * Matrix.RotationY(time * 2) * Matrix.RotationZ(time * .7f) * viewProj;
    worldViewProj.Transpose();
    context.UpdateSubresource(ref worldViewProj, contantBuffer);

    // Draw the cube
    context.Draw(36, 0);

    // Present!
    swapChain.Present(0, PresentFlags.None);
}
```

### Resizing of the Swap Chain

The swap chain's resources are released, the buffer is resized, and the resources are recreated.

```c#
private void ResizeSwapChain()
{
    // Dispose all previous allocated resources
    Utilities.Dispose(ref backBuffer);
    Utilities.Dispose(ref renderView);
    Utilities.Dispose(ref depthBuffer);
    Utilities.Dispose(ref depthView);

    // Resize the backbuffer
    swapChain.ResizeBuffers(desc.BufferCount, width, height, Format.Unknown, SwapChainFlags.None);

    // Get the backbuffer from the swapchain
    backBuffer = Texture2D.FromSwapChain<Texture2D>(swapChain, 0);

    // Renderview on the backbuffer
    renderView = new RenderTargetView(device, backBuffer);

    // Create the depth buffer
    depthBuffer = new Texture2D(device, new Texture2DDescription()
    {
        Format = Format.D32_Float_S8X24_UInt,
        ArraySize = 1,
        MipLevels = 1,
        Width = width,
        Height = height,
        SampleDescription = new SampleDescription(1, 0),
        Usage = ResourceUsage.Default,
        BindFlags = BindFlags.DepthStencil,
        CpuAccessFlags = CpuAccessFlags.None,
        OptionFlags = ResourceOptionFlags.None
    });

    // Create the depth buffer view
    depthView = new DepthStencilView(device, depthBuffer);

    // Setup targets and viewport for rendering
    context.Rasterizer.SetViewport(new Viewport(0, 0, width, height, 0.0f, 1.0f));
    context.OutputMerger.SetTargets(depthView, renderView);

    // Setup new projection matrix with correct aspect ratio
    proj = Matrix.PerspectiveFovLH((float)Math.PI / 4.0f, width / (float)height, 0.1f, 100.0f);

    // We are done resizing
    userResized = false;
}
```
