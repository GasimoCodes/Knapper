using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knapper
{
    public struct Vector2UInt
    {
        public uint weight { get; set; }
        public uint value { get; set; }

        public Vector2UInt(uint weight, uint value)
        {
            this.weight = weight;
            this.value = value;
        }
    }
}
