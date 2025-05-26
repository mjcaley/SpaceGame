struct AssembledVertex
{
    float2 position : POSITION;
    float2 tex_coords : TEXCOORD0;
};

struct VertexOutput
{
    float4 position : SV_Position;
    float2 tex_coords : TEXCOORD0;
};

[shader("vertex")]
func vertexMain(vertex: AssembledVertex) -> VertexOutput {
    VertexOutput output;
    output.position = float4(vertex.position, 0, 1);
    output.tex_coords = vertex.tex_coords;
    
    return output;
}

struct FragmentInput
{
    float2 tex_coords: TEXCOORD0;
};

struct FragmentOutput
{
    float4 color: COLOR;
};

[shader("fragment")]
func fragmentMain(uniform Sampler2D sampler, input: FragmentInput) -> FragmentOutput {
    FragmentOutput output;
    output.color = sampler.Sample(input.tex_coords);
    // output.color = float4(1, 0.25, 0, 1);

    return output;
}
