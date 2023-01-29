using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class Edge
    {
        public enum Directions{
            X, Y, Z
        }

        public Vector3Int startPosition;
        public Directions direction;
        public float value;
        public List<Vertex> vertices;

        public Edge(Vector3Int startPosition, Directions direction)
        {
            this.startPosition = startPosition;
            this.direction = direction;
            vertices = new List<Vertex>(4);
            value = 0;
        }
    }
}
