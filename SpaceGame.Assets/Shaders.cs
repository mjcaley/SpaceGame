using System.Reflection;

namespace SpaceGame.Assets;

public static class Shaders
{
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
}
