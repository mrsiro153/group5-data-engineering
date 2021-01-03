using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DE_CW_1
{
    public class MerkleNode
    {
        public int Id { get; set; }
        public String Hash { get; set; }
        public MerkleNode? LeftNode { get; set; }
        public MerkleNode? RightNode { get; set; }
        public MerkleNode? Parent { get; set; }
        public String Chunk { get; set; }

        public bool IsLeaf { get { return LeftNode == null && RightNode == null; } }

        public MerkleNode(String hash, int id, String chunk = null)
        {
            Id = id;  
            Hash = hash;
            Chunk = chunk;
            LeftNode = null;
            RightNode = null;
            Parent = null;
        }
    }
}
