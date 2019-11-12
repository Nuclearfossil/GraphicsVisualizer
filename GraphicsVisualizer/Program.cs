// Additional references:
// https://mellinoe.wordpress.com/2017/02/08/designing-a-3d-rendering-library-for-net-core/

using GraphicsVisualizer;
using System;
using System.Linq;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

using static ImGuiNET.ImGuiNative;

namespace ImGuiNET
{
    class Program
    {
        private static Sdl2Window _window;
        private static GraphicsDevice _gd;
        private static CommandList _cl;
        private static ImGuiController _controller;
        private static MemoryEditor _memoryEditor;
        private static TransformExample _transformExample;
        private static TransformUIData _transformUIData;
        private static SoftwareProjectionExample _softwareProjectionExample;
        private static ProjectionUIData _projectionUIData;

        // UI state
        private static Vector3 _clearColor = new Vector3(0.45f, 0.55f, 0.6f);
        private static bool _showAnotherWindow = false;

        static void SetThing(out float i, float val) { i = val; }

        static void Main(string[] args)
        {
            // Create window, GraphicsDevice, and all resources necessary for the demo.
            VeldridStartup.CreateWindowAndGraphicsDevice(
                new WindowCreateInfo(50, 50, 1280, 720, WindowState.Normal, "GraphicsVisualizer"),
                new GraphicsDeviceOptions(true, null, true),
                out _window,
                out _gd);
            _window.Resized += () =>
            {
                _gd.MainSwapchain.Resize((uint)_window.Width, (uint)_window.Height);
                _controller.WindowResized(_window.Width, _window.Height);
            };
            _cl = _gd.ResourceFactory.CreateCommandList();
            _controller = new ImGuiController(_gd, _gd.MainSwapchain.Framebuffer.OutputDescription, _window.Width, _window.Height);
            _memoryEditor = new MemoryEditor();
            Random random = new Random();
            _transformExample = new TransformExample(_gd);
            _transformUIData = new TransformUIData();
            _softwareProjectionExample = new SoftwareProjectionExample(_gd);
            _projectionUIData = new ProjectionUIData();

            // Main application loop
            while (_window.Exists)
            {
                InputSnapshot snapshot = _window.PumpEvents();
                if (!_window.Exists) { break; }
                _controller.Update(1f / 60f, snapshot); // Feed the input events to our ImGui controller, which passes them through to ImGui.
                //_transformExample.Update(_transformUIData);
                _softwareProjectionExample.Update(_projectionUIData, (float)_window.Width / (float)_window.Height);

                SubmitBaseUI();
                SubmitOtherUI();

                _cl.Begin();
                _cl.SetFramebuffer(_gd.MainSwapchain.Framebuffer);
                _cl.ClearColorTarget(0, new RgbaFloat(_clearColor.X, _clearColor.Y, _clearColor.Z, 1f));
                // _transformExample.Draw(_cl);
                _softwareProjectionExample.Draw(_cl);
                _controller.Render(_gd, _cl);
                _cl.End();
                _gd.SubmitCommands(_cl);


                _gd.SwapBuffers(_gd.MainSwapchain);
            }

            // Clean up Veldrid resources
            _gd.WaitForIdle();
            _controller.Dispose();
            _cl.Dispose();
            _gd.Dispose();
        }

        private static unsafe void SubmitBaseUI()
        {
            // 1. Show a simple window.
            // Tip: if we don't call ImGui.BeginWindow()/ImGui.EndWindow() the widgets automatically appears in a window called "Debug".
            ImGui.Begin("Transform Example");
            {
                ImGui.Checkbox("Asymmetrical viewport?", ref _projectionUIData._asymetricalProjection);
                ImGui.SameLine();
                ImGui.Checkbox("Use Matrices", ref _projectionUIData._useMatrices);
                if (_projectionUIData._asymetricalProjection)
                {
                    ImGui.SliderFloat("Left", ref _projectionUIData._left, -10, 10);
                    ImGui.SliderFloat("Right", ref _projectionUIData._right, -10, 10);
                    ImGui.SliderFloat("Top", ref _projectionUIData._top, -10, 10);
                    ImGui.SliderFloat("Bottom", ref _projectionUIData._bottom, -10, 10);
                }
                else
                {
                    ImGui.SliderFloat("FOV Y", ref _projectionUIData._fovy, 1, 90);
                    ImGui.SliderFloat("Aspect Ratio", ref _projectionUIData._aspect, 0.1f, 2.0f);
                }
            }

            ImGui.End();
            ImGuiIOPtr io = ImGui.GetIO();
            SetThing(out io.DeltaTime, 2f);
        }

        private static unsafe void SubmitOtherUI()
        {
            // Demo code adapted from the official Dear ImGui demo program:
            // https://github.com/ocornut/imgui/blob/master/examples/example_win32_directx11/main.cpp#L172

            // 1. Show another simple window. In most cases you will use an explicit Begin/End pair to name your windows.
            if (_showAnotherWindow)
            {
                ImGui.Begin("Another Window", ref _showAnotherWindow);
                ImGui.Text("Hello from another window!");
                if (ImGui.Button("Close Me"))
                    _showAnotherWindow = false;
                ImGui.End();
            }
        }
    }
}
