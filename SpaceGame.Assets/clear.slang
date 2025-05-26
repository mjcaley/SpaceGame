struct AssembledVertex
{
    float2 position : POSITION;
    float4 color : COLOR;
};

struct VertexOutput
{
    float4 position : SV_Position;
    float4 color : COLOR;
};

[shader("vertex")]
func vertexMain(vertex: AssembledVertex) -> VertexOutput {
    VertexOutput output;
    output.position = float4(vertex.position, 0, 1);
    output.color = vertex.color;
    
    return output;
}

struct FragmentInput
{
    float4 color : COLOR;
};

struct FragmentOutput
{
    float4 color: COLOR;
};

[shader("fragment")]
func fragmentMain(input: FragmentInput) -> FragmentOutput {
    FragmentOutput output;
    output.color = input.color;

    return output;
}
