using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Index
{
    class Node
    {
        public Dictionary<string, Node> branches;
        public bool isRoot;
        public bool isLeaf;
        public Node prevNode;
        public Node nextNode;
    }
}
