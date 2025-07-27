using SpaceGame.Build.Shaders;
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
    if (source is null || destination is null || @namespace is null)
    {
        throw new ArgumentException($"Inputs were null");
    }

    var generator = new SourceGenerator(@namespace, destination);
    generator.Generate(source.EnumerateFiles("*.slang")).Wait();

    return 0;
});

ParseResult result = rootCommand.Parse(args);
return result.Invoke();
