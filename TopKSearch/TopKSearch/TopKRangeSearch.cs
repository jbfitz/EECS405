using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using TrieSpace;

namespace TopKSearch
{
    class TopKRangeSearch
    {
        static Trie data = new Trie();

        static void Main(string[] args)
        {
            // Construct Trie
            try
            {
                //Console.WriteLine("Please input the filename you wish to read from:");
                //String filename = Console.ReadLine();
                ConstructTrie("The Prince.txt");
                Console.WriteLine(data);
                Console.WriteLine(data.root.min);

                Console.WriteLine("What search do you want to look at?");
                String querystring = Console.ReadLine();

                HashSet<String> topK = SearchTopK(querystring, 5);

                Console.WriteLine("Valid Strigngs:");
                foreach (String found in topK)
                {
                    Console.WriteLine(found);
                }

                
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Error Encountered: ");
                Console.Error.WriteLine(e.Message);
            }
            Console.WriteLine("Press any button to exit...");
            Console.ReadKey();

        }


        public static void ConstructTrie(String filename)
        {
            using (StreamReader sr = new StreamReader(filename))
            {
                while (!sr.EndOfStream)
                {
                    String ln = sr.ReadLine();
                    if (ln.Length > 0)
                    {
                        String[] words = ln.Split(' ');
                        if (words.Length > 0)
                        {
                            data.AddStrings(words);
                        }
                    }
                }
                data.SetIds();
                Console.WriteLine(data.root.max);
                Console.WriteLine("Trie constructed!");
            }

        }

        public static HashSet<String> SearchTopK(String query, int k)
        {
            HashSet<String> topmatch = new HashSet<String>();
            HashSet<TrieNode> currentSet;
            HashSet<TrieNode> nextSet = new HashSet<TrieNode>();

            char[] queryChars = query.ToCharArray();

            // Find strings for threshold = 0;
            TrieNode parent = data.root;
            TrieNode candidateNode;
            HashSet<TrieQuad> nextQuadSet = new HashSet<TrieQuad>();
            int index = 0;

            while (index < query.Length && parent.children.ContainsKey(queryChars[parent.level + 1]))
            {
                
                candidateNode = parent.children[queryChars[index]];

                // If exact match
                if (candidateNode.IsLeaf())
                {
                    Console.WriteLine("Candidate node String : " + candidateNode);
                    if (Math.Abs(candidateNode.ToString().Length - query.Length) == 0)
                    {
                        topmatch.Add(candidateNode.ToString());
                    }
                    else
                    {
                        nextQuadSet.Add(new TrieQuad(candidateNode, candidateNode.id, candidateNode.id, query, candidateNode.level));
                    }
                }

                if (candidateNode.children.Count > 0)
                {

                    if( !(candidateNode.min == parent.min && candidateNode.max == parent.max))
                    {
                        // Add range smaller than the exact matching
                        if (candidateNode.min != parent.min)
                        {
                            nextQuadSet.Add(new TrieQuad(parent, parent.min, candidateNode.min - 1, query, parent.level));
                        }

                        //Add range larger than exact matching
                        if (candidateNode.max != parent.max)
                        {
                            nextQuadSet.Add(new TrieQuad(parent, candidateNode.max + 1, parent.max, query, parent.level));
                        }   
                    }
                    
                }
                
                parent = candidateNode;
                index++;

            }
            // There are no more exact matches)
            nextQuadSet.Add(new TrieQuad(parent, parent.min, parent.max, query, parent.level));

            //Exact match for now. 
            return topmatch;

        }
    }


    class TrieQuad{
        TrieNode node;
        int lower;
        int upper;
        String query;
        int qIndex;
        public TrieQuad(TrieNode tn, int l, int u, String q, int i){
            node = tn;
            lower = l;
            upper = u;
            query = q;
            qIndex = i;

        }
    }
}
