using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Drawing;
using SpaceGame.Infrastructure;

namespace SpaceGame.Core.Components
{
    public class Sprite
    {
        public string Texture { get; set; }
        public Layer Layer { get; set; }
        public Vector2 Size { get; set; }
    }
}
