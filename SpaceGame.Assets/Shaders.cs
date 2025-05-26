using System.Reflection;

namespace SpaceGame.Assets;

public static class Shaders
{
    public static class SpriteVertex
    {
        public static byte[] Spirv
        {
            get
            {
                var info = Assembly.GetExecutingAssembly().GetName();
                var name = info.Name;
                using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"{name}.spv");
                using var memoryStream = new MemoryStream();
                stream!.CopyTo(memoryStream);
                
                return memoryStream.ToArray();
            }
        }
    }
}
