using SpaceGame.Assets.ShaderGenerator;
using System.Reflection;

namespace SpaceGame.Assets;

public static class Shaders
{
    [Shader("index-coloured-quad.vert.slang", Stage.Vertex)]
    public static partial class ColouredVertexShader
    {
        [ShaderFormat(Format.SPIRV)]
        private static Shader _spriv;


        [ShaderFormat(Format.DXIL)]
        private static Shader _dxil;
    }

    public static class QuadVertex
    {
        public static byte[] Spirv
        {
            get
            {
                var path = Path.Join(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "res", "shaders", "coloured-quad.vert.spv");
                return File.ReadAllBytes(path);
            }
        }
    }
    public static class QuadFragment
    {
        public static byte[] Spirv
        {
            get
            {
                var path = Path.Join(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "res", "shaders", "coloured-quad.frag.spv");
                return File.ReadAllBytes(path);
            }
        }
    }

    //[Shader("indexed-coloured-quad.slang")]
    public static class IndexedQuadVertex
    {
        public static byte[] Spirv
        {
            get
            {
                var path = Path.Join(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "res", "shaders", "indexed-coloured-quad.vert.spv");
                return File.ReadAllBytes(path);
            }
        }
    }
    public static class IndexedQuadFragment
    {
        public static byte[] Spirv
        {
            get
            {
                var path = Path.Join(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "res", "shaders", "indexed-coloured-quad.frag.spv");
                return File.ReadAllBytes(path);
            }
        }
    }
}
