using SpaceGame.Build.Shaders;
using System.CommandLine;
using System.CommandLine.Parsing;

Option<DirectoryInfo> sourceOption = new("--src")
{
    Description = "Source directory to look for *.slang files"
};

Option<DirectoryInfo> destinationOption = new("--dest")
{
    Required = true,
    Description = "Destination directory where source files are generated"
};

Option<string> namespaceOption = new("--namespace")
{
    Description = "Name of the namespace classes will be in",
};

Option<bool> cleanOption = new("--clean")
{
    Description = "Cleans generated sources",
    DefaultValueFactory = (_) => false
};

RootCommand rootCommand = [];
rootCommand.Options.Add(sourceOption);
rootCommand.Options.Add(destinationOption);
rootCommand.Options.Add(namespaceOption);
rootCommand.Options.Add(cleanOption);

rootCommand.Validators.Add(result =>
{
    if (!result.GetValue(cleanOption))
    {
        if (result.GetValue(sourceOption) is null)
            result.AddError("Source path is required");
        if (result.GetValue(namespaceOption) is null)
            result.AddError("Namespace name is required");
    }
});

rootCommand.SetAction(async (parseResult, cancellationToken) =>
{
    var destination = parseResult.GetValue(destinationOption)!;

    if (parseResult.GetValue(cleanOption))
    {
        foreach (var file in destination.EnumerateFiles("Shaders.*.cs"))
        {
            file.Delete();
        }

        return 0;
    }

    var source = parseResult.GetValue(sourceOption);
    var @namespace = parseResult.GetValue(namespaceOption);
    if (source is null || destination is null || @namespace is null)
    {
        throw new ArgumentException($"Inputs were null");
    }

    var generator = new SourceGenerator(@namespace, destination);
    try
    {
        await generator.Generate(source.EnumerateFiles("*.slang"));
    }
    catch (CompileException e)
    {
        Console.Error.WriteLine(e.Message);
        return 1;
    }
    catch (Exception e)
    {
        Console.Error.WriteLine($"Unexpected error: {e.Message}");
        return 1;
    }

    return 0;
});

ParseResult result = rootCommand.Parse(args);
return result.Invoke();
