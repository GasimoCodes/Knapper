using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knapper
{
    /// <summary>
    /// Stores 2D Vector of UInts
    /// </summary>
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
