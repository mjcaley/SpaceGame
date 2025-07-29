using ImGuiNET;

namespace SpaceGame.Renderer;

// need:
// - input hook
// - 

internal class ImGuiController
{
    private nint _context = ImGui.CreateContext();
    private Renderer _renderer;

    public ImGuiController(Renderer renderer)
    {
        _renderer = renderer;
        Init();
    }

    private void Init()
    {
        var io = ImGui.GetIO();
        ImGui.SetCurrentContext(_context);
        io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;
        

        ImGui.StyleColorsDark();
    }

    public void BeforeLayout(float deltaTime)
    {
        ImGui.GetIO().DeltaTime = deltaTime;
    }

    public void AfterLayout()
    {
        ImGui.Render();
        Render();
    }

    private void UpdateInput()
    {
        var io = ImGui.GetIO();

        // Bunch of events here
    }

    private unsafe void Render()
    {
        var drawData = ImGui.GetDrawData();
        var vertexBuffer = _renderer.BorrowVertexBuffer(drawData.TotalVtxCount * sizeof(ImDrawVert));
        var indexBuffer = _renderer.BorrowIndexBuffer(drawData.TotalIdxCount * sizeof(ushort));


    }
}
