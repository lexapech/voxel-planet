using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEditor.PlayerSettings;

namespace Assets.Scripts
{
    public interface IMeshDataProvider
    {
        GridData GetGridData(Vector3 position, float size);
        
    }
}
