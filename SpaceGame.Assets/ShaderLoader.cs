using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using SpaceGame.Infrastructure;

namespace SpaceGame.Assets;

public class ShaderLoader(string basePath, ILoader<Shader>? parent = null) : ILoader<Shader>
{
    private readonly ILoader<Shader>? _parent = parent;
    private readonly Dictionary<string, Shader> _shaders = new();
    
    public bool TryGet(string name, [MaybeNullWhen(false)] out Shader? shader)
    {
        if (_shaders.TryGetValue(name, out var s))
        {
            shader = s;
            return true;
        }

        if (_parent is not null)
        {
            _parent.TryGet(name, out shader);
        }
        
        shader = null;
        return false;
    }
    
    public Shader Load(string name)
    {
        if (_shaders.TryGetValue(name, out var shader))
        {
            return shader;
        }
        
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"{name}.spv");
        using var memoryStream = new MemoryStream();
        stream!.CopyTo(memoryStream);
        _shaders[name] = new Shader(memoryStream.ToArray());
        
        return _shaders[name];
    }

    public ILoader<Shader> AddScope()
    {
        return new ShaderLoader(basePath, this);
    }
}