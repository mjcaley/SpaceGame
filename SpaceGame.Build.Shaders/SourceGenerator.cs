using System.Text;
using Slangc.NET;

namespace SpaceGame.Build.Shaders;

internal class SourceGenerator(string namespaceName, DirectoryInfo outputPath)
{
    private static string ToClassName(string filename)
    {
        var textInfo = Thread.CurrentThread.CurrentCulture.TextInfo;
        var titleWords = Path.GetFileNameWithoutExtension(filename).Split(' ', '.', '-', '_')
            .Select(textInfo.ToTitleCase);

        return string.Join(string.Empty, titleWords);
    }

    private async Task GenerateFromEntryPoint(string className, Format format, CompiledShader shader)
    {
        var generatedCode = @$"using System.Collections.Immutable;
        
namespace {namespaceName};

public static partial class Shaders
{{
    public static partial class {className}
    {{
        public static partial class {shader.Stage}
        {{
            public static Shader {format} {{ get; }} = new Shader
            {{
                    Code = ImmutableArray.Create<byte>([{string.Join(',', shader.Code.Select(b => b))}]),
                    Stage = ShaderStage.{shader.Stage},
                    Format = ShaderFormat.{format},
                    EntryPoint = ""{shader.EntryPoint}"",
                    NumUniform = {shader.BindingsCount.UniformBuffers},
            }};
        }}
    }}
}}
";

        await File.WriteAllTextAsync(
            Path.Join(outputPath.FullName, $"Shaders.{className}.{shader.Stage}.{format}.g.cs"),
            generatedCode,
            Encoding.UTF8
        );
    }

    private async Task Generate(FileInfo path, Format format)
    {
        var code = SlangCompiler.CompileWithReflection(format.AsCompilerArgs(path.FullName), out var reflection);
        if (code.Length == 0)
        {
            throw new CompileException($"Failed to compile {path}");
        }
        var className = ToClassName(path.Name);

        await Task.WhenAll(
            reflection.EntryPoints
            .Select(e => GenerateFromEntryPoint(className, format, new CompiledShader(code, e, reflection.Parameters))));
    }

    public async Task Generate(IEnumerable<FileInfo> paths)
    {
        outputPath.Create();
        await Task.WhenAll(paths.Select<FileInfo, Task>(p =>
        {
            return Generate(p, Format.Spirv); // SPIR-V
            // Generate(p, ...)  // DXIL
        }));
    }
}
