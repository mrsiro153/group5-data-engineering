using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DE_CW_1
{
    public class FileVerification
    {
        public string email { get; set; }
        public string bcAddress { get; set; }
        public string fileIdentifier { get; set; }
        public string shardHash { get; set; }
        public int shardIndex { get; set; }
        public FileVerification(string email, string address, string indentifier, string hash, int index)
        {
            this.email = email;
            this.bcAddress = address;
            this.fileIdentifier = indentifier;
            this.shardHash = hash;
            this.shardIndex = index;
        }
    }
}
