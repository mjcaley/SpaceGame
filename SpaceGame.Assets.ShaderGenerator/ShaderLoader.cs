using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Slangc.NET;
using System.Diagnostics;
using System.Text;

namespace SpaceGame.Assets.ShaderGenerator;

[Generator]
public class ShaderLoader : IIncrementalGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        Debug.WriteLine("ShaderLoader.Execute called");
        Debugger.Break();
    }


    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        Debug.WriteLine("ShaderLoader.Initialize called");
        Debugger.Break();

#if DEBUG
        if (!Debugger.IsAttached)
        {
            Debugger.Launch();
        }
#endif 
        var pipeline = context.AdditionalTextsProvider
            .Where(static (text) => text.Path.EndsWith(".slang"))
            .Select(static (text, cancellationToken) =>
            {
                var name = Path.GetFileName(text.Path);
                var bytes = SlangCompiler.CompileWithReflection(["-g3", text.Path], out SlangReflection reflection);
                return (name, bytes, reflection);
            });

        context.RegisterSourceOutput(pipeline,
            static (context, result) =>
                // Note: this AddSource is simplified. You will likely want to include the path in the name of the file to avoid
                // issues with duplicate file names in different paths in the same project.
                context.AddSource($"{result.name}generated.cs", SourceText.From(
                    $$"""
                    using System.Collections.Immutable;

                    namespace SpaceGame.Assets;

                    public static class {{result.name}}Shader
                    {
                        private static ImmutableArray<byte> _spriv = ImmutableArray.Create<byte>([{{string.Join(", ", result.bytes.Select(b => b.ToString()))}}]);
                        public static ImmutableArray<byte> Spriv => _spirv; 
                    }

                    """, Encoding.UTF8)));
    }
}
