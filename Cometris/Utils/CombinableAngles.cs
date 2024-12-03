using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MikoMino;

namespace Cometris.Utils
{
    /// <summary>
    /// Represents a combination of <see cref="Angle"/> values.
    /// </summary>
    [Flags]
    public enum CombinableAngles : byte
    {
        None = 0,
        Up = 1,
        Right = 2,
        Down = 4,
        Left = 8,
    }
}
