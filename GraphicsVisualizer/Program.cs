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

        // UI state
        // private static int _counter = 0;
        // private static int _dragInt = 0;
        private static Vector3 _clearColor = new Vector3(0.45f, 0.55f, 0.6f);
        private static bool _showDemoWindow = false;
        private static bool _showAnotherWindow = false;
        private static bool _showMemoryEditor = false;
        private static byte[] _memoryEditorData;
        // private static uint s_tab_bar_flags = (uint)ImGuiTabBarFlags.Reorderable;
        static bool[] s_opened = { true, true, true, true }; // Persistent user state

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
            _memoryEditorData = Enumerable.Range(0, 1024).Select(i => (byte)random.Next(255)).ToArray();
            _transformExample = new TransformExample(_gd);
            _transformUIData = new TransformUIData();

            // Main application loop
            while (_window.Exists)
            {
                InputSnapshot snapshot = _window.PumpEvents();
                if (!_window.Exists) { break; }
                _controller.Update(1f / 60f, snapshot); // Feed the input events to our ImGui controller, which passes them through to ImGui.
                _transformExample.Update(_transformUIData);

                SubmitBaseUI();
                SubmitOtherUI();

                _cl.Begin();
                _cl.SetFramebuffer(_gd.MainSwapchain.Framebuffer);
                _cl.ClearColorTarget(0, new RgbaFloat(_clearColor.X, _clearColor.Y, _clearColor.Z, 1f));
                _transformExample.Draw(_cl);
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
                ImGui.Checkbox("Calculate By Hand?", ref _transformUIData._byHand);
                ImGui.SliderFloat("Uniform Scale", ref _transformUIData._uniformScale, 0.001f, 1.0f, _transformUIData.UniformScale.ToString("0.00000"), 1);  // Edit 1 float using a slider from 0.0f to 1.0f
                ImGui.SliderFloat2("Translation (X, Y)", ref _transformUIData._translate, -1.0f, 1.0f);
                ImGui.SliderFloat("Rotation", ref _transformUIData._rotateZ, -360.0f, 360.0f, _transformUIData.RotateZ.ToString("0.0000"), 1);

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

            // 2. Show the ImGui demo window. Most of the sample code is in ImGui.ShowDemoWindow(). Read its code to learn more about Dear ImGui!
            if (_showDemoWindow)
            {
                // Normally user code doesn't need/want to call this because positions are saved in .ini file anyway.
                // Here we just want to make the demo initial state a bit more friendly!
                ImGui.SetNextWindowPos(new Vector2(650, 20), ImGuiCond.FirstUseEver);
                ImGui.ShowDemoWindow(ref _showDemoWindow);
            }


            if (_showMemoryEditor)
            {
                _memoryEditor.Draw("Memory Editor", _memoryEditorData, _memoryEditorData.Length);
            }
        }
    }
}
