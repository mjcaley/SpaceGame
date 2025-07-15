using SpaceGame.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace SpaceGame.Core.Components
{
    public class Rectangle
    {
        public Layer Layer { get; set; }
        public Vector2 Origin { get; set; }
        public Vector2 Size { get; set; }
        public Vector4 Colour { get; set; }
    }
}
