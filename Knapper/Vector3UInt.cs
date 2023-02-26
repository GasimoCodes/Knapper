using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knapper
{
    /// <summary>
    /// Stores 3D Vector of UInts
    /// </summary>
    public struct Vector3UInt
    {
        public uint weight { get; set; }
        public uint value { get; set; }
        public ulong combinationIndex { get; set; }

        public Vector3UInt(uint weight, uint value, ulong combinationIndex)
        {
            this.weight = weight;
            this.value = value;
            this.combinationIndex = combinationIndex;
        }
    }
}
