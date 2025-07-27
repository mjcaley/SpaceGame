internal record BindingsCount
{
    public int Samplers { get; set; }
    public int StorageTextures { get; set; }
    public int StorageBuffers { get; set; }
    public int UniformBuffers { get; set; }
}
