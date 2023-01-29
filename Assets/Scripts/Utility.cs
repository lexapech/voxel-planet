﻿using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class Utility
    {
        public static Vector3Int EdgeDirToVector(Edge.Directions direction)
        {
            switch(direction)
            {
                case Edge.Directions.X: return new Vector3Int(1, 0, 0);
                case Edge.Directions.Y: return new Vector3Int(0, 1, 0);
                case Edge.Directions.Z: return new Vector3Int(0, 0, 1);
                default: return new Vector3Int(0, 0, 0);
            }
        }
        public static Vector3Int VectorMod(Vector3Int vector, int div)
        {
            return new Vector3Int(vector.x % div, vector.y % div, vector.z % div);
        }
    }
}
