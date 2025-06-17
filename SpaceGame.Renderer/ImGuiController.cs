using SpaceGame.SDLWrapper;
using Hexa.NET.ImGui;
using Hexa.NET.ImGui.Backends.SDL3;
using static SDL3.SDL;
using SpaceGame.Infrastructure;

namespace SpaceGame.Renderer;

public class ImGuiController : IDisposable
{
    public ImGuiController(IWindow window, IGpuDevice gpuDevice)
    {
        var scale = GetDisplayContentScale(GetPrimaryDisplay());
        
        Context = ImGui.CreateContext();
        ImGui.SetCurrentContext(Context);
        
        _io = ImGui.GetIO();
        _io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;
        ImGui.StyleColorsDark();
        var style = ImGui.GetStyle();
        style.ScaleAllSizes(scale);
        
        unsafe
        {
            var windowPtr = new SDLWindowPtr((SDLWindow*)window.Handle);
            ImGuiImplSDL3.InitForSDLGPU(windowPtr); // TODO: Handle error
        }
    }

    public ImGuiContextPtr Context { get; }
    private ImGuiIOPtr _io;
    public bool CaptureKeyboard => _io.WantCaptureKeyboard;
    public bool CaptureMouse => _io.WantCaptureMouse;

    public unsafe void ProcessEvent(ref SDLEvent @event)
    {
        fixed (SDLEvent* e = &@event)
        {
            ImGuiImplSDL3.ProcessEvent(e);
        }
    }

    public void Dispose()
    {
        ImGui.DestroyContext(Context);
    }
}