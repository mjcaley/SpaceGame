struct FragmentInput
{
    float4 color: COLOR;
};

struct FragmentOutput
{
    float4 color: COLOR;
};

[shader("fragment")]
func main(input: FragmentInput) -> FragmentOutput {
    FragmentOutput output;
    output.color = input.color;

    return output;
}
