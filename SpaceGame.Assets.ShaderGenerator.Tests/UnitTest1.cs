using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using VerifyXunit;
using Xunit.Sdk;

namespace SpaceGame.Assets.ShaderGenerator.Tests;

internal class InMemoryAdditionalText(string path, string content) : AdditionalText
{
    private readonly SourceText _content = SourceText.From(content, Encoding.UTF8);

    public override string Path { get; } = path;

    public override SourceText GetText(CancellationToken cancellationToken = default)
        => _content;
}

internal class FakeAdditionalText(string path) : AdditionalText
{
    public override string Path => path;

    public override SourceText? GetText(CancellationToken cancellationToken = default)
    {
        using var stream = new StreamReader(Path);
        return SourceText.From(stream.ReadToEnd());
    }
}

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        var temp = "shader.slang";
        using (var writer = new StreamWriter(temp))
        {
            writer.Write(@"struct AssembledVertex
{
    float2 position : POSITION;
    float4 color : COLOR;
};

struct VertexOutput
{
    float4 position : SV_Position;
    float4 color : COLOR;
};

[[vk::binding(0, 1)]]
ConstantBuffer<float4x4> Translate;

[shader(""vertex"")]
func vertexMain(vertex: AssembledVertex) -> VertexOutput {
    VertexOutput output;
    output.position = mul(Translate, float4(vertex.position, 0.0f, 1.0f));
    output.color = vertex.color;
    
    return output;
}

struct FragmentInput
{
    float4 color: COLOR;
};

struct FragmentOutput
{
    float4 color: COLOR;
};

[shader(""fragment"")]
func fragmentMain(input: FragmentInput) -> FragmentOutput {
    FragmentOutput output;
    output.color = input.color;

    return output;
}
");
        }

            var compilation = CSharpCompilation.Create("TestProject");
        var generator = new ShaderGenerator();
        var sourceGenerator = generator.AsSourceGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [sourceGenerator],
            additionalTexts: [new FakeAdditionalText("shader.slang")],
            driverOptions: new GeneratorDriverOptions(default, trackIncrementalGeneratorSteps: true))
            .RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        var results = driver.GetRunResult().Results;
        var result = results.Single();
        var allOutputs = result.TrackedOutputSteps.SelectMany(outputStep => outputStep.Value);
    }
}
