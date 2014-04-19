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
        public int threshold = 25;

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
        public void AddStrings(string[] strings)
        {
            if (strings.Length == 0)
            {
                Console.Error.WriteLine("Warning: No strings were passed in. Exiting Trie Construction...");
                return;
            }

            foreach (string s in strings)
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
                            PositionPair pair = new PositionPair(s, i);
                            string substring = s.Substring(i, gramLength);
                            root.AddString(substring, pair);
                        }
                    }
                }
            }
        }


        public TrieNode LongestMatchingVGRAM(string s)
        {
            TrieNode currentNode = this.root;
            int index = 0;
            bool endVgramSearch = false;

            while(!endVgramSearch && index<s.Length){
                char currentChar = s[index];
                if (currentNode.children.ContainsKey(currentChar))
                {
                    currentNode = currentNode.children[currentChar];
                    index++;
                }
                else
                {
                    endVgramSearch = true;
                }
            }

            if (index < minLength)
            {
                return null;
            }

            return currentNode.children['\0'];
        }

        public void Prune()
        {
            root.Prune(threshold);
        }

        override public string ToString()
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
        public HashSet<PositionPair> pairs;
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
            pairs = new HashSet<PositionPair>();
        }


        /// <summary>
        /// Recursively adds the content of the string to the node as children. 
        /// </summary>
        /// <param name="s"></param>
        public void AddString(string s, PositionPair p)
        {
            char firstChar = s[0];
            AddChild('\0', p);
            AddChild(firstChar, p);
            if (s.Substring(1) != "")
            {
                children[firstChar].AddString(s.Substring(1), p);
            }
            else
            {
                children[firstChar].AddChild('\0', p);
            }
        }

        /// <summary>
        /// Add a child TrieNode to this one.
        /// If the node already exists, increment the frequency of that node
        /// </summary>
        /// <param name="c"></param>
        public void AddChild(char c, PositionPair p)
        {
            if (!this.children.ContainsKey(c))
            {
                this.children.Add(c, new TrieNode(c));
                this.children[c].level = this.level + 1;
                this.children[c].min = this.min;
                this.children[c].max = this.max;
                this.children[c].parent = this;
                this.children[c].frequency = 1;
                this.children[c].pairs.Add(p);
            }
            else
            {
                this.children[c].frequency++;
                if (!this.children[c].pairs.Contains(p))
                {
                    this.children[c].pairs.Add(p);
                }
                else
                {
                    int x = 0;
                }
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


            if (this.level+1 < this.min)
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

        public string AllStrings()
        {
            string startingstring = "";
            string returnstring = "";

            foreach (TrieNode child in children.Values)
            {
                List<string> substrings = child.GenerateStrings(startingstring);
                foreach (string substring in substrings)
                {
                    returnstring += substring + "\n";
                }
            }

            return returnstring;
        }

        public List<string> GenerateStrings(string s)
        {
            List<string> endlist = new List<string>();

            if (this.IsLeaf())
            {
                endlist.Add(s + " - " + this.frequency);
            }
            else
            {
                foreach (TrieNode child in children.Values)
                {
                    foreach (string substring in child.GenerateStrings(s + this.character))
                    {
                        endlist.Add(substring);
                    }
                }
            }
            return endlist;
        }

        override public string ToString()
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
                    return parent.ToString();
                }
                
            }
        }

    }


    public class PositionPair : IComparable<PositionPair>, IEquatable<PositionPair>
    {
        public string value;
        public int position;

        /// <summary>
        /// Positional pairs stored in the trienodes refering to the strings. 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="i"></param>

        public PositionPair(string s, int i)
        {
            value = s;
            position = i;
        }

        public int CompareTo(PositionPair pair){
            if (this.value == pair.value && this.position == pair.position)
            {
                return 0;
            }

            if (this.position.Equals(pair.position))
            {
                return this.value.CompareTo(pair.value);
            }
            else
            {
                return this.position.CompareTo(pair.position);
            }
        }

        public bool Equals(PositionPair pair)
        {
            return this.position == pair.position && this.value == pair.value;
        }

        public override int GetHashCode()
        {
            return (this.value.GetHashCode() + this.position.GetHashCode()).GetHashCode();        
        }
        
    }
}
