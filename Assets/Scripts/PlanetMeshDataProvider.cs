using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEditor.PlayerSettings;

namespace Assets.Scripts
{
    internal class PlanetMeshDataProvider : IMeshDataProvider
    {
        private int _chunkSize;
        private int _chunksCount;
        public PlanetMeshDataProvider(int chunkSize)
        {
            _chunksCount = 10;
            _chunkSize = chunkSize;
        }
        public GridData GetGridData(Vector3 position, float size)
        {
            Vector3 center = Vector3.zero;
            float radius = (_chunksCount * _chunkSize / 2) * 0.9f;
            var grid = new GridData(_chunkSize);
            for (int z = 0; z < _chunkSize; z++)
            {
                for (int y = 0; y < _chunkSize; y++)
                {
                    for (int x = 0; x < _chunkSize; x++)
                    {
                        Vector3 currentPos = position + (new Vector3(x, y, z) - Vector3.one * _chunkSize / 2) *size;
                        var temp = (currentPos - center).normalized;
                        var a1 = Mathf.Atan2(temp.z, temp.x);
                        var a2 = Mathf.Atan2(temp.y, temp.x);
                        float sphereDist = radius - (currentPos - center).magnitude - Mathf.PerlinNoise(a1 * 1f, a2 * 1f) * 10 - Mathf.PerlinNoise(a1 * 3f, a2 * 3f) * 5;
                        grid.volumes[x, y, z] = (byte)(Mathf.Clamp01(sphereDist) * 255);
                        //grid.volumes[x, y, z] = (byte)(Mathf.Clamp01(Mathf.PerlinNoise(currentPos.x, currentPos.z) * 25 + (Mathf.PerlinNoise(currentPos.x*3, currentPos.z*3)*10-5) + 5 - y) * 255);

                    }
                }
            }
            return grid;
        }
    }
}
