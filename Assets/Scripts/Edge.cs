using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public struct Edge
    {
        public enum Directions{
            X, Y, Z
        }
        
        public Vector3Int startPosition;
        public Directions direction;
        public float value;
        public Vertex[] vertices;
        private int _vertexCount;

        public Edge(Vector3Int startPosition, Directions direction)
        {
            this.startPosition = startPosition;
            this.direction = direction;
            vertices = new Vertex[4];
            value = 0;
            _vertexCount = 0;
        }
        public void AddVertex(Vertex vertex)
        {
            vertices[_vertexCount++]= vertex;
        }
    }
}
