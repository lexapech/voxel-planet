using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEditor.PlayerSettings;

namespace Assets.Scripts
{
    class PlainsMeshDataProvider : IMeshDataProvider
    {
        private int _chunkSize;
        public PlainsMeshDataProvider(int chunkSize)
        {
            _chunkSize = chunkSize;
        }

        public GridData GetGridData(Vector3 position, float size)
        {
            var grid = new GridData(_chunkSize);
            for (int z = 0; z < _chunkSize; z++)
            {
                for (int y = 0; y < _chunkSize; y++)
                {
                    for (int x = 0; x < _chunkSize; x++)
                    {
                        Vector3 currentPos = position + Vector3.one * _chunkSize / 2 + new Vector3(x, y, z) * size;
                        grid.volumes[x, y, z] = (byte)(Mathf.Clamp01(Mathf.PerlinNoise(currentPos.x / 100f, currentPos.z / 100f) * 40 + (Mathf.PerlinNoise(currentPos.x * 3 / 40f, currentPos.z * 3 / 40f) * 6 - 5) + 5 - y) * 255);
                    }
                }
            }
            return grid;
        }
    }
}
