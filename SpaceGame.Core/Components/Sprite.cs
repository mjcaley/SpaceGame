using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Drawing;

namespace SpaceGame.Core.Components
{
    public class Sprite
    {
        string Texture { get; set; }
        string Layer { get; set; }
        Vector2 Size { get; set; }
    }
}
