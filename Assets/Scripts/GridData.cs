using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;

namespace Assets.Scripts
{
    public sealed class GridData
    {
        public int Size { get; }
        public byte[,,] volumes;
        public GridData(int size)
        {
            Size = size;
            volumes = new byte[size, size, size];
        }
    }
}
