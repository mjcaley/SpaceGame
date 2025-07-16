namespace SpaceGame.Renderer;

internal record InstanceOffsets
{
    public required uint VertexOffset { get; init; }
    public required uint InstanceDetailsOffset { get; init; }
    public required int NumberOfInstances { get; init; }
}
