using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knapper
{
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
