using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Index
{
    class IndexTree
    {
        public Node root;
        public int branchingFactor;

        public IndexTree()
        {
            root = new Node();
            branchingFactor = 100;
        }

        public IndexTree(int bf)
        {
            root = new Node();
            branchingFactor = 100;
        }

        public void Insert(string s)
        {

        }

    }
}
