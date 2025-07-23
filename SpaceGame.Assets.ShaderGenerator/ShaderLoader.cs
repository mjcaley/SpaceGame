using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using Slangc.NET;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;

namespace SpaceGame.Assets.ShaderGenerator;

[Generator]
public class ShaderGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {

        //        Debug.WriteLine("ShaderLoader.Initialize called");
        //        Debugger.Break();

        //#if DEBUG
        //        if (!Debugger.IsAttached)
        //        {
        //            Debugger.Launch();
        //        }
        //#endif 

        var pipeline = context.AdditionalTextsProvider
            .Where(static (text) => text.Path.EndsWith(".slang"))
            .Select((text, cancellationToken) =>
            {
                //Debugger.Launch();
                var name = Path.GetFileName(text.Path);
                var bytes = SlangCompiler.Compile([text.Path, "-g3", "-target", "spirv"]);

                return (name, bytes);
            });

        
        context.RegisterSourceOutput(pipeline,
           (context, result) =>
           {
               // Note: this AddSource is simplified. You will likely want to include the path in the name of the file to avoid
               // issues with duplicate file names in different paths in the same project.

               var cleanName = string.Join(string.Empty, Path.GetFileNameWithoutExtension(result.name).Split('-', '_', ' ', '.').Select(w => w.Substring(0, 1).ToUpper() + w.Substring(1)));
               
               context.AddSource($"{cleanName}generated.cs", SourceText.From(
                   $$"""
                   using System.Collections.Immutable;
                    public static class {{result.name}}Shader
                    {
                        private static ImmutableArray<byte> _spriv = ImmutableArray.Create<byte>([{{string.Join(", ", result.bytes.Select(b => b.ToString()))}}]);
                        public static ImmutableArray<byte> Spriv => _spriv; 
                    }
                   """, Encoding.UTF8));
           });
    }
}
