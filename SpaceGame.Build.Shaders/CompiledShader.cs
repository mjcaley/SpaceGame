using Slangc.NET;

namespace SpaceGame.Build.Shaders;

internal class CompiledShader
{
    public CompiledShader(byte[] code, SlangEntryPoint entryPoint, SlangParameter[] parameters)
    {
        Code = code;
        Stage = entryPoint.Stage switch
        {
            SlangStage.Vertex => Stage.Vertex,
            SlangStage.Fragment => Stage.Fragment,
            SlangStage.Compute => Stage.Compute,
            _ => throw new NotSupportedException($"Unsupported shader stage: {entryPoint.Stage}")
        };
        EntryPoint = entryPoint.Name;
        BindingsCount = new BindingsCount
        {
            UniformBuffers = parameters.Count(b => b.Type.Kind == SlangTypeKind.ConstantBuffer)
        };
    }

    public byte[] Code { get; init; }
    public Stage Stage { get; init; }
    public string EntryPoint { get; init; }
    public BindingsCount BindingsCount { get; init; }
}
