using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using SpaceGame.Infrastructure;
using Slangc.NET;
using System.Diagnostics;
using System.Text;

namespace SpaceGame.Assets;

public class Shader
{
    private const byte[] _spriv = new byte[2] ( 0x01, 0x02 );
}

[Generator]
public class ShaderLoader : IIncrementalGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
    }


    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
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
                    namespace SpaceGame.Assets;

                    public class {{result.name}}Shader
                    {
                        private const byte[] 
                    }

                    """, Encoding.UTF8)));
    }
}

}
