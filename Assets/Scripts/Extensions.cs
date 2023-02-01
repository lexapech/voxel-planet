using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public static class Extensions
    {
        public static byte Get3D(this byte[,,] array,Vector3Int index)
        {
            return array[index.x,index.y,index.z];
        }
        public static bool IsComponentWiseGreaterOrEqual(this Vector3Int vector,int val)
        {
            return vector.x >= val || vector.y >= val || vector.z >= val;
        }
        public static bool IsComponentWiseLess(this Vector3Int vector, int val)
        {
            return vector.x < val && vector.y < val && vector.z < val;
        }
        

    }
    
}
