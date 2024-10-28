using System;
using System.Windows;
using System.Threading;
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.D3DCompiler;
using Buffer = SharpDX.Direct3D11.Buffer;
using Color = SharpDX.Color;
using Vector2 = SharpDX.Vector2;
using Vector3 = SharpDX.Vector3;
using Vector4 = SharpDX.Vector4;
using Matrix = SharpDX.Matrix;
using Point = System.Windows.Point;
using Size = GfxControls.Size;

//https://github.com/sharpdx/SharpDX-Samples/blob/master/Desktop/Direct3D11/MiniCube/Program.cs
namespace Example1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        private static extern short GetKeyState(int nVirtKey);

        private const int VK_LBUTTON = 0x01;

        private SharpDX.Direct3D11.Device? device;
        private SwapChain? swapChain;
        private DeviceContext? context;
        private Buffer? vertexBuffer;
        private Buffer? contantBuffer;
        private RenderTargetView? renderView;
        private Texture2D? backBuffer;
        private Texture2D? depthBuffer;
        private DepthStencilView? depthView;
        private SwapChainDescription desc;
        private Matrix proj;
        private Matrix view;
        private Matrix worldMatrix;

        private Thread? renderThread;
        private bool rendering;

        private bool userResized = true;
        private int width = 0;
        private int height = 0;

        private Vector3 targetPosition = new Vector3(0, 0, 0);
        private float zoomFactor = 5.0f; // Initial zoom distance from the target
        private float zoomSpeed = 0.01f; // Speed at which zoom changes per mouse wheel tick
        private float moveSensitivity = 0.01f;

        private Point lastMousePosition;
        private long lastMoveEvent = 0;
        private bool deltaIsValid = false;
        private bool mouseInside;

        private Stopwatch? stopwatch;
        private float timeDivisor = 1000.0f;

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
            width = (int)dxHost.ActualWidth;
            height = (int)dxHost.ActualHeight;

            stopwatch = new Stopwatch();
            stopwatch.Start();

            dxHost.Resized += DxHost_Resized;
            dxHost.MouseMove += DxHost_MouseMove;
            dxHost.MouseWheel += DxHost_MouseWheel;

            InitializeGraphics();
        }

        private void DxHost_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            // Adjust zoom factor with mouse wheel
            zoomFactor -= e.Delta * zoomSpeed;
            zoomFactor = MathUtil.Clamp(zoomFactor, 2.0f, 52.0f); // Clamps the zoom between min and max distances

            SetViewMatrix();
        }

        private void DxHost_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (stopwatch!.ElapsedMilliseconds - lastMoveEvent > 16)
            {
                //// TODO: GetPosition always returning same result unless break after.
                //var pos = e.GetPosition(element);

                var pos = dxHost.GetRelativeMousePos();
                Point delta = new Point(pos.X - lastMousePosition.X, pos.Y - lastMousePosition.Y);
                lastMousePosition = pos;


                if ((GetKeyState(VK_LBUTTON) & 0x8000) != 0)
                {
                    if (deltaIsValid)
                    {
                        targetPosition = new Vector3(
                            MathUtil.Clamp(targetPosition.X + -(float)delta.X * moveSensitivity, -10, 10),
                            MathUtil.Clamp(targetPosition.Y + (float)delta.Y * moveSensitivity, -5, 5),
                            0);

                        SetViewMatrix();
                    }

                    deltaIsValid = true;
                }
                else
                {
                    deltaIsValid = false;
                }

                lastMoveEvent = stopwatch.ElapsedMilliseconds;
            }
        }

        private void DxHost_Resized(object? sender, Size e)
        {
            userResized = true;
            width = (int)e.Width;
            height = (int)e.Height;
        }

        private void SetViewMatrix()
        {
            var eyePosition = new Vector3(0, 0, -zoomFactor); // Use zoomFactor for Z position
            var upDirection = Vector3.UnitY; // Defines "up" in the world space

            view = Matrix.LookAtLH(eyePosition, targetPosition, upDirection);
            UpdateLabel();
        }

        private void UpdateLabel()
        {
            LabelBottom.Text = $"Zoom: -{zoomFactor - 2:F3} | X: {targetPosition.X:F3} | Y: {targetPosition.Y:F3}";
        }

#pragma warning disable 8602
#pragma warning disable 8604

        private void InitializeGraphics()
        {
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

            // Instantiate Vertex buiffer from vertex data
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

#pragma warning restore 8602
#pragma warning restore 8604

        protected override void OnClosing(CancelEventArgs e)
        {
            if (renderThread != null && renderThread.IsAlive)
            {
                rendering = false;
                renderThread.Join();
                renderThread = null;

                dxHost.Dispose();
            }

            base.OnClosing(e);
        }
    }
}