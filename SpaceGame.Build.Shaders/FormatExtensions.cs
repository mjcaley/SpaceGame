internal static class FormatExtensions
{
    public static string[] AsCompilerArgs(this Format format, string path) =>
        format switch
        {
            Format.Spirv => [path, "-g3", "-matrix-layout-column-major", "-target", "spirv", "-emit-spirv-directly", "-fvk-use-entrypoint-name"],
            _ => throw new ArgumentException($"Format {format} not supported")
        };
}
