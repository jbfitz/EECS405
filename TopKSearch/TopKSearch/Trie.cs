using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrieSpace;



namespace TrieSpace
{
    public class Trie
    {
        public TrieNode root;

        public Trie()
        {
            root = new TrieNode('\0');
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
                    root.AddString(s);
                }
            }
        }

        public void SetIds()
        {
            root.SetIds();
        }

        override public String ToString()
        {
            return root.ToString();
        }

    }

    public class TrieNode
    {
        public Dictionary<char, TrieNode> children;
        public char character { get; set; }
        public int max { get; set; }
        public int min { get; set; }
        public int id;
        public bool root { get; set; }
        public bool endOfWord;


        public TrieNode(char c)
        {
            character = c;
            children = new Dictionary<char, TrieNode>();
            endOfWord = false;
        }


        //Recursively adds the content of the string to the node as children. 
        public void AddString(String s)
        {
            char firstChar = s[0];
            AddChild(firstChar);
            if (s.Substring(1) != "")
            {
                children[firstChar].AddString(s.Substring(1));
            }
            else
            {
                children[firstChar].endOfWord = true;
            }
        }

        public void AddChild(char c)
        {
            if (!this.children.ContainsKey(c))
            {
                this.children.Add(c, new TrieNode(c));
            }
        }

        public int SetIds()
        {
            return SetIds(1);
        }

        public int SetIds(int currentId)
        {
            this.min = currentId;
            if (endOfWord)
            {
                this.id = currentId++;
            }

            if (children.Count > 0)
            {
                foreach (TrieNode child in children.Values)
                {
                    currentId = child.SetIds(currentId) + 1;
                }
                this.max = currentId - 1;
            }
            else
            {
                this.max = this.min;
            }
            return this.max;
        }

        public bool IsLeaf()
        {
            return endOfWord;
        }

        override public String ToString()
        {
            if (children.Count > 0)
            {
                String returnString = "";
                foreach (TrieNode child in children.Values)
                {
                    returnString += this.character + child.ToString();
                }

                if (this.endOfWord)
                {
                    returnString += this.character;
                }
                return returnString;

            }
            else
            {
                return this.character.ToString();
            }
        }
    }
}
