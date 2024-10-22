using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics;
using System.Text;
using System.Threading.Tasks;

namespace Cometris.Utils
{
    public static class Avx512Utils
    {
        public static Vector512<ushort> OnesComplement(Vector512<ushort> value) => (~value.AsUInt32()).AsUInt16();
    }
}
