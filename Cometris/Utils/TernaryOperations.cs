using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cometris.Utils
{
    [Flags]
    public enum TernaryOperations : byte
    {
        None = 0,
        NorABC = 1,
        AndCNorBA = 2,
        AndBNorAC = 4,
        NorANandBC = 8,
        AndANorBC = 16,
        NorBNandAC = 32,
        NorCNandBA = 64,
        AndABC = 128,
        NorAB = 3,
        NorAEqBC = 6,
        AndBNotA = 0x0c,
        A = 0xf0,
        B = 0xcc,
        C = 0xaa,
        NotA = unchecked((byte)~A),
        NotB = unchecked((byte)~B),
        NotC = unchecked((byte)~C),
        OrABC = A | B | C,
        AndAB = A & B,
        AndAC = A & C,
        AndBC = B & C,
        OrAB = A | B,
        OrAC = A | C,
        OrBC = B | C,
        XorAB = A ^ B,
        XorAC = A ^ C,
        XorBC = B ^ C,
        True = byte.MaxValue
    }
}
