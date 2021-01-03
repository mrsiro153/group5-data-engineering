using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace DE_CW_1
{
    public class MerkleTree
    {
        private const int HASH_LENGTH = 32;
        public List<MerkleNode> merkleTree { get; set; }
        public String merkleRoot { get; set; }
        public FormattedTree formattedMerkleTree { get; set; }

        public MerkleTree(List<String> data_chunks, int chunk_size, string idenfier) {
            Build(data_chunks, chunk_size, idenfier);
        }

        public void Build(List<String> data_chunks, int chunk_size, string idenfier)
        {
            merkleTree = new List<MerkleNode>();
            List<MerkleNode> list_leaves = InitTree(data_chunks);
            List<MerkleNode> current_nodes = list_leaves;
            merkleTree.AddRange(current_nodes);
            int lastIndex = 0;

            while (current_nodes.Count() != 1)
            {
                System.Diagnostics.Debug.WriteLine(lastIndex);
                System.Diagnostics.Debug.WriteLine(current_nodes.Count());
                current_nodes = buildTree(current_nodes, lastIndex);
                System.Diagnostics.Debug.WriteLine(current_nodes.Count());
                lastIndex = merkleTree.Count() - 1;
                merkleTree.AddRange(current_nodes);
            }

            merkleTree.AddRange(current_nodes);
            merkleRoot = current_nodes[0].Hash;
            formattedMerkleTree = new MerkleTreeFomatter(idenfier, chunk_size, list_leaves, merkleRoot).merkleTree;
    }

        protected List<MerkleNode> InitTree(List<String> data_chunks)
        {
            List<MerkleNode> list_leaves = new List<MerkleNode>();

            for(int i=0; i<data_chunks.Count(); i++)
            {
                MerkleNode node = new MerkleNode(ComputeHash(data_chunks[i]), i, data_chunks[i]);
                list_leaves.Add(node);
                System.Diagnostics.Debug.WriteLine("Leaf " + i + " - " + node.Chunk + " : " + node.Hash + "\n\n");

            }
            return list_leaves;

        }

        public List<MerkleNode> buildTree(List<MerkleNode> list_nodes, int lastIndex)
        {
            int numberOfNodes = list_nodes.Count();
            List<MerkleNode> list_parents = new List<MerkleNode>();
            MerkleNode left_child = null;
            MerkleNode right_child = null;
            int parentId = merkleTree.Count() + merkleTree.Count() % 2;

            for (int i = 0; i < numberOfNodes; i += 2)
            {
                left_child = list_nodes[i];
                right_child = list_nodes[i + 1];

                list_parents.Add(BuildParent(left_child, right_child, lastIndex + i, parentId));
                parentId++;
            }

            return list_parents;
        }

        protected MerkleNode BuildParent(MerkleNode left_child, MerkleNode right_child, int leftIndex, int parentId)
        {
            MerkleNode parent = new MerkleNode(ComputeHash(left_child.Hash + right_child.Hash), parentId, left_child.Chunk + right_child.Chunk);

            merkleTree[leftIndex].Parent = parent;
            merkleTree[leftIndex + 1].Parent = parent;

            parent.LeftNode = left_child;
            parent.RightNode = right_child;

            System.Diagnostics.Debug.WriteLine("Left child - " + left_child.Chunk + " : " + left_child.Hash + "\nRight child - " + right_child.Chunk + " : " + right_child.Hash + "\nParent - " + parent.Chunk + " : " + parent.Hash + "\n\n");
            return parent;
        }


        protected String ComputeHash(String data)
        {
            var crypt = new SHA256Managed();
            string hash = String.Empty;
            byte[] crypto = crypt.ComputeHash(Encoding.ASCII.GetBytes(data));
            foreach (byte theByte in crypto)
            {
                hash += theByte.ToString("x2");
            }
            return hash;
        }
    }
}
