using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DE_CW_1
{
    public class FileSelection
    {
        public string fileName { get; set; }
        public int fileIndex { get; set; }

        public FileSelection(string name, int idx)
        {
            this.fileIndex = idx;
            this.fileName = name;
        }
    }
}
