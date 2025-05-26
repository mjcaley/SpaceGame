struct AssembledVertex
{
    float2 position : POSITION;
    float3 color : COLOR;
};

struct VertexOutput
{
    float4 position : SV_Position;
    float4 color : COLOR;
};

[shader("vertex")]
func vertexMain(vertex: AssembledVertex) -> VertexOutput {
    VertexOutput output;
    output.position = float4(vertex.position, 0.0f, 1.0f);
    output.color = float4(vertex.color, 1.0f);
    
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

[shader("fragment")]
func fragmentMain(input: FragmentInput) -> FragmentOutput {
    FragmentOutput output;
    output.color = input.color;

    return output;
}
