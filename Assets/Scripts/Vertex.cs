using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class Vertex
    {
        public Vector3 position;
        public int edges;
        public int index;
        public void Normalize(int index)
        {
            this.index = index;
            position = position / edges;
        }
    }
}
