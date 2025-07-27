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

    private static BindingsCount CountBindings(SlangNamedTypeBinding[] bindings)
    {
        BindingsCount count = new();

        foreach (var binding in bindings.Select(b => b.Binding))
        {
            switch (binding.Kind)
            {
                case SlangParameterCategory.ConstantBuffer:
                    count.UniformBuffers++;
                    break;
            }
        }

        return count;
    }

    private async Task GenerateFromEntryPoint(string className, Format format, byte[] code, SlangEntryPoint entryPoint)
    {
        var count = CountBindings(entryPoint.Bindings);
        var generatedCode = @$"using System.Collections.Immutable;
        
namespace {namespaceName};

public static partial class Shaders
{{
    public static partial class {className}
    {{
        public static partial class {entryPoint.Stage}
        {{
            public static Shader {format} {{ get; }} = new Shader
            {{
                    Code = ImmutableArray.Create<byte>([{string.Join(',', code.Select(b => b))}]),
                    Stage = ShaderStage.{entryPoint.Stage},
                    Format = ShaderFormat.{format},
                    EntryPoint = ""{entryPoint.Name}"",
                    NumUniform = {count.UniformBuffers},
            }};
        }}
    }}
}}
";

        await File.WriteAllTextAsync(
            Path.Join(outputPath.FullName, className + $".{entryPoint.Stage}.{format}.generated.cs"),
            generatedCode,
            Encoding.UTF8
        );
    }

    private async Task Generate(FileInfo path, Format format)
    {
        var code = SlangCompiler.CompileWithReflection(format.AsCompilerArgs(path.FullName), out var reflection);
        var className = ToClassName(path.Name);

        await Task.WhenAll(reflection.EntryPoints.Select(e => GenerateFromEntryPoint(className, format, code, e)));
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
