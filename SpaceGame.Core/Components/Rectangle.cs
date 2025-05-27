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
        Tuple<int, int, int> Colour { get; set; }
        Vector2 Size { get; set; }
    }
}
