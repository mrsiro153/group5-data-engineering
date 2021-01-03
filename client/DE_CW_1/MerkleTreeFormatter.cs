using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DE_CW_1
{
    //public class FormattedNode
    //{
    //    public int? Id { get; set; }
    //    public String Hash { get; set; }

    //    public FormattedNode(String hash, int id)
    //    {
    //        Hash = hash;
    //        Id = id;
    //    }
    //}
    public class FormattedTree
    {
        public string FileIdentifier { get; set; }
        public int ChunkSize { get; set; }
        public List<String> Leaves { get; set; }
        public String RootHash { get; set; }

        public FormattedTree(string identifier, int chunkSize, List<String> listleaves, String merkleRootHash)
        {
            this.FileIdentifier = identifier;
            this.ChunkSize = chunkSize;
            this.Leaves = listleaves;
            this.RootHash = merkleRootHash;
        }
    }

    public class MerkleTreeFomatter
    {

        public FormattedTree merkleTree { get; set; }


        public MerkleTreeFomatter(string identifier, int chunkSize, List<MerkleNode> listFullLeaves, String merkleRoot)
        {
            List<String> listleaves = new List<String>();
            foreach(MerkleNode fullLeaf in listFullLeaves)
            {;
                listleaves.Add(fullLeaf.Hash);
            }

            merkleTree = new FormattedTree(identifier, chunkSize, listleaves, merkleRoot);
        }
    }
}
