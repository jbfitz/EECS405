using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrequencyTrieSpace;

namespace VGram
{
    class VGramSearch
    {

        public Trie vgramDictionary;

        static void Main(string[] args)
        {
            VGramSearch vgs = new VGramSearch();
            // Construct GramDictionary
            vgs.vgramDictionary = new Trie(2, 4);
            vgs.GenerateGramDictionary("The Prince.txt");
            Console.WriteLine(vgs.vgramDictionary.ToString());
            

            Console.WriteLine("What search do you want to look at?");
            string querystring = Console.ReadLine();
            Console.WriteLine("\n\n");

            List<string> results = vgs.VSearch(querystring);

            foreach (string result in results)
            {
                Console.WriteLine(result);
            }

            Console.WriteLine("Valid Strings:");

            Console.WriteLine("Press any button to exit...");
            Console.ReadKey();

        }

        public void GenerateGramDictionary(string filename)
        {
            using (StreamReader sr = new StreamReader(filename))
            {
                while (!sr.EndOfStream)
                {
                    string ln = sr.ReadLine();
                    if (ln.Length > 0)
                    {
                        string[] words = ln.Split(' ');
                        if (words.Length > 0)
                        {
                            this.vgramDictionary.AddStrings(words);
                        }
                    }
                }
                this.vgramDictionary.Prune();
                Console.WriteLine("Trie constructed!");
            }
        }


        public List<string> VSearch(string query)
        {
            int i = 0;
            Dictionary<int, TrieNode> vgramSet = new Dictionary<int,TrieNode>();

            while (i < query.Length - vgramDictionary.minLength + 1)
            {
                TrieNode nextNode = vgramDictionary.LongestMatchingVGRAM(query.Substring(i));
                if (nextNode != null)
                {
                    vgramSet.Add(i, nextNode);
                }
                i++;
            }

            //Generate a set of candidate strings;
            Dictionary<string, int> candidateStrings = new Dictionary<string,int>();

            foreach (int key in vgramSet.Keys)
            {
                TrieNode node = vgramSet[key];
                foreach (PositionPair pair in node.pairs)
                {
                    if (pair.position == key)
                    {
                        if (candidateStrings.ContainsKey(pair.value))
                        {
                            candidateStrings[pair.value]++;
                        }
                        else
                        {
                            candidateStrings[pair.value] = 1;
                        }
                    }
                }
            }



            int k = 0;

            List<string> foundStrings = new List<string>();

            while (foundStrings.Count < 10 && k < 5)
            {
                foreach(string key in candidateStrings.Keys){
                    if (LengthBounded(key, query, k))
                    {
                        if (Math.Abs(candidateStrings[key] - vgramSet.Count) <= k)
                        {
                            if (!foundStrings.Contains(key))
                            {
                                foundStrings.Add(key);
                            }
                        }
                    }
                }

                foreach (string str in foundStrings)
                {
                    candidateStrings.Remove(str);
                }

                k++;
            }

            return foundStrings;
        }


        public bool LengthBounded(string s1, string s2, int k)
        {
            return Math.Abs(s1.Length - s2.Length) <= k;
        }
    }
}
