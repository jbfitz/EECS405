using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrequencyTrieSpace;

namespace FrequencyTrieSpace
{
    public class Trie
    {
        public TrieNode root;
        public int minLength;
        public int maxLength;

        public Trie(int min, int max)
        {
            root = new TrieNode('\0');
            root.level = -1;
            minLength = min;
            maxLength = max;
        }

        public void AddStrings(String[] strings)
        {
            if (strings.Length == 0)
            {
                Console.Error.WriteLine("Warning: No Strings were passed in. Exiting Trie Construction...");
                return;
            }

            foreach (String s in strings)
            {
                if (s != "")
                {
                    for (int i = 0; i < s.Length; i++)
                    {
                        String substring = s.Substring(i, i + maxLength);
                        root.AddString(substring);
                    }
                }
            }
        }

        public void Prune()
        {
            int threshold = 5;
            root.Prune(threshold);
        }
    }

    public class TrieNode
    {
        public Dictionary<char, TrieNode> children;
        public char character { get; set; }
        public bool root { get; set; }
        public int frequency {get; set;}
        public int level;
        public TrieNode parent;


        public TrieNode(char c)
        {
            character = c;
            children = new Dictionary<char, TrieNode>();
        }


        //Recursively adds the content of the string to the node as children. 
        public void AddString(String s)
        {
            char firstChar = s[0];
            AddChild('\0');
            AddChild(firstChar);
            if (s.Substring(1) != "")
            {
                children[firstChar].AddString(s.Substring(1));
            }
            else
            {
                children[firstChar].AddChild('\0');
            }
        }

        public void AddChild(char c)
        {
            if (!this.children.ContainsKey(c))
            {
                this.children.Add(c, new TrieNode(c));
                this.children[c].level = this.level + 1;
                this.children[c].parent = this;
            }
            else
            {
                this.children[c].frequency++;
            }
        }

        public bool IsLeaf()
        {
            return this.children.Count == 0;
        }

        public void Prune(int threshold){
            foreach (char c in children.Keys)
            {
                if (c != '\0')
                {
                    children[c].Prune(threshold);
                }
            }

            TrieNode leaf = children['\0'];

            if (this.frequency <= threshold)
            {
                //Remove all non-leaf children of this node
                children = new Dictionary<char,TrieNode>();
                leaf.frequency = this.frequency;
                children['\0'] = leaf;
            } 
            else 
            {
                // Select a maximal subset o fchildren of n
                // so that the summation of their freq values
                // L.freq is still not greater than Threshold

                foreach (char key in this.children.Keys)
                {

                }

            }

           
        }
    }
}
