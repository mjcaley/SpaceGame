using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Infrastructure;

public interface IGraphicsPipeline
{
    nint Handle { get; }
}
