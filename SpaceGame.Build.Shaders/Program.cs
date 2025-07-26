using Slangc.NET;
using System.CommandLine;
using System.CommandLine.Parsing;

Option<DirectoryInfo> sourceOption = new("--src")
{
    Required = true,
    Description = "Source directory to look for *.slang files"
};

Option<DirectoryInfo> destinationOption = new("--dest")
{
    Required = true,
    Description = "Destination directory where source files are generated"
};

Option<string> namespaceOption = new("--namespace")
{
    Required = true,
    Description = "Name of the namespace classes will be in"
};

RootCommand rootCommand = [];
rootCommand.Options.Add(sourceOption);
rootCommand.Options.Add(destinationOption);
rootCommand.Options.Add(namespaceOption);
rootCommand.SetAction(parseResult =>
{
    var source = parseResult.GetValue(sourceOption);
    var destination = parseResult.GetValue(destinationOption);
    var @namespace = parseResult.GetValue(namespaceOption);

    foreach (var shader in source.EnumerateFiles("*.slang"))
    {
        Console.WriteLine($"Found {shader}");
        var code = SlangCompiler.CompileWithReflection([shader.FullName, "-g3", "-capability", "spirv_1_0"], out var reflection);
        //var code = SlangCompiler.Compile([shader.FullName, "-g3", "-capability", "spirv_1_0"]);
        Console.WriteLine($"Length of compiled shader is {code.Length}");
        // Console.WriteLine($"Reflection data is {reflection}");
    }

    return 0;
});

ParseResult result = rootCommand.Parse(args);
return result.Invoke();
