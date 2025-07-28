struct AssembledVertex
{
    float2 position : POSITION;
    float4 color : COLOR;
    float4x4 model;
};

struct VertexOutput
{
    float4 position : SV_Position;
    float4 color : COLOR;
};

struct Camera
{
    float4x4 view;
    float4x4 projection;
};

[[vk::binding(0, 1)]]
ConstantBuffer<Camera> camera;

[shader("vertex")]
func vertexMain(vertex: AssembledVertex) -> VertexOutput {
    VertexOutput output;
    output.position = mul(vertex.model, float4(vertex.position, 0.0f, 1.0f));
    output.position = mul(camera.view, output.position);
    output.position = mul(camera.projection, output.position);
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

[shader("fragment")]
func fragmentMain(input: FragmentInput) -> FragmentOutput {
    FragmentOutput output;
    output.color = input.color;

    return output;
}

