using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.SDLWrapper
{
    public class SwapchainTexture(nint handle, uint width, uint height)
    {
        public nint Handle => handle;
        public uint Width => width;
        public uint Height => height;
    }
}
