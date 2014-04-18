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
        public int threshold = 50;

        /// <summary>
        /// A trie-structure that stores grams character by character
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public Trie(int min, int max)
        {
            minLength = min;
            maxLength = max;

            root = new TrieNode('\0');
            root.max = maxLength;
            root.min = minLength;
            root.level = -1;
            
        }

        /// <summary>
        /// Uses an array of strings to add to or construct a trie
        /// </summary>
        /// <param name="strings"></param>
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
                    for (int i = 0; i + minLength < s.Length; i++)
                    {
                        int gramLength = maxLength;
                        if (i + maxLength >= s.Length)
                        {
                            gramLength = s.Length - i - 1;
                        }

                        if (gramLength >= minLength)
                        {
                            String substring = s.Substring(i, gramLength);
                            root.AddString(substring);
                        }
                    }
                }
            }
        }

        public void Prune()
        {
            root.Prune(threshold);
        }

        override public String ToString()
        {
            return root.AllStrings();
        }
    }

    public class TrieNode : IComparable<TrieNode>
    {
        public Dictionary<char, TrieNode> children;
        public char character { get; set; }
        public int min { get; set; }
        public int max { get; set; }
        public int frequency {get; set;}
        public int level;
        public TrieNode parent;


        /// <summary>
        /// Node in a trie.
        /// Stores character and 
        /// Children of this node are extensions of this gram.
        /// </summary>
        /// <param name="c"></param>
        public TrieNode(char c)
        {
            character = c;
            children = new Dictionary<char, TrieNode>();
        }


        /// <summary>
        /// Recursively adds the content of the string to the node as children. 
        /// </summary>
        /// <param name="s"></param>
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

        /// <summary>
        /// Add a child TrieNode to this one.
        /// If the node already exists, increment the frequency of that node
        /// </summary>
        /// <param name="c"></param>
        public void AddChild(char c)
        {
            if (!this.children.ContainsKey(c))
            {
                this.children.Add(c, new TrieNode(c));
                this.children[c].level = this.level + 1;
                this.children[c].min = this.min;
                this.children[c].max = this.max;
                this.children[c].parent = this;
                this.children[c].frequency = 1;
            }
            else
            {
                this.children[c].frequency++;
            }
        }

        /// <summary>
        /// Sorts Trie node by frequency first, then by ASCII value if frequencies are identical
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>

        public int CompareTo(TrieNode other)
        {
            if (other == null)
            {
                return 1;
            }

            int diff = this.frequency - other.frequency;

            if (diff != 0)
            {
                return diff;
            }
            else
            {
                return this.character.CompareTo(other.character);
            }
        }

        /// <summary>
        /// Returns true if the TrieNode is a leaf
        /// (Has no children/contains the null character)
        /// </summary>
        /// <returns></returns>
        public bool IsLeaf()
        {
            return this.children.Count == 0;
        }


        /// <summary>
        /// Prunes the Trie according to the VGRAM specs
        /// </summary>
        /// <param name="threshold"></param>
        public void Prune(int threshold){
            if (children.Count == 0)
            {
                return;
            }


            if (this.level <= this.min)
            {
                foreach (TrieNode child in children.Values)
                {
                    child.Prune(threshold);
                }
            }
            else
            {
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
                    children = new Dictionary<char, TrieNode>();
                    leaf.frequency = this.frequency;
                    children['\0'] = leaf;
                }
                else
                {
                    // Select a maximal subset of children of n
                    // so that the summation of their freq values
                    // L.freq is still not greater than Threshold

                    List<TrieNode> prunelist = new List<TrieNode>();

                    foreach (char key in this.children.Keys)
                    {
                        if (key != '\0')
                        {
                            prunelist.Add(this.children[key]);
                        }
                    }

                    if (prunelist.Count == 0)
                    {
                        return;
                    }

                    prunelist.Sort();

                    // Following LargeFirst paradigm. 
                    // Add small elements until we can't fit any larger elements

                    int sumFrequency = 0;
                    TrieNode next = prunelist.First();

                    // Spare the smallest children from the pruning
                    while (sumFrequency + next.frequency < threshold && prunelist.Count > 0)
                    {
                        sumFrequency += next.frequency;
                        prunelist.Remove(next);
                        if (prunelist.Count > 0)
                        {
                            next = prunelist.First();
                        }
                    }

                    // Prune the larger ones
                    foreach (TrieNode node in prunelist)
                    {
                        this.children['\0'].frequency += node.frequency;
                        this.children.Remove(node.character);
                    }

                    //For the remainder of the elements, call prune.
                    foreach (TrieNode node in this.children.Values)
                    {
                        node.Prune(threshold);
                    }
                }
            }
        }


        public String AllStrings()
        {
            String startingString = "";
            String returnString = "";

            foreach (TrieNode child in children.Values)
            {
                List<String> substrings = child.GenerateStrings(startingString);
                foreach (String substring in substrings)
                {
                    returnString += substring + "\n";
                }
            }

            return returnString;
        }

        public List<String> GenerateStrings(String s)
        {
            List<String> endlist = new List<String>();

            if (this.IsLeaf())
            {
                endlist.Add(s + " - " + this.frequency);
            }
            else
            {
                foreach (TrieNode child in children.Values)
                {
                    foreach (String substring in child.GenerateStrings(s + this.character))
                    {
                        endlist.Add(substring);
                    }
                }
            }
            return endlist;
        }

        override public String ToString()
        {
            if (parent.character == '\0')
            {
                return this.character.ToString();
            }
            else
            {
                if (this.character != '\0')
                {
                    return parent.ToString() + this.character;
                }
                else
                {
                    return parent.ToString() + " - End";
                }
                
            }
        }

    }
}
