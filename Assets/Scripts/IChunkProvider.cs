using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    public interface IChunkProvider
    {
        public MeshChunk Instantiate();
        public void Destroy(MeshChunk chunk);
    }
}
