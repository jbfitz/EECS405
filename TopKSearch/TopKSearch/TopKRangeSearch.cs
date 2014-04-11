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

                Console.WriteLine("How many terms do you want back?");
                String strk = Console.ReadLine();
                int k = Int16.Parse(strk);

                HashSet<String> topK = SearchTopK(querystring, k);

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
            char[] queryChars = query.ToCharArray();

            // Find strings for threshold = 0;
            TrieNode parent = data.root;
            TrieNode candidateNode;
            HashSet<TrieQuad> nextQuadSet = new HashSet<TrieQuad>();
            HashSet<TrieQuad> currentQuadSet;
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
            if (!parent.IsLeaf() || parent.children.Count > 0)
            {
                nextQuadSet.Add(new TrieQuad(parent, parent.min, parent.max, query, parent.level));
            }

            int threshold = 0;

            while (topmatch.Count < k)
            {
                threshold++;
                currentQuadSet = nextQuadSet;
                Queue<TrieQuad> potentialQuadQueue = new Queue<TrieQuad>();
                nextQuadSet = new HashSet<TrieQuad>();


                foreach (TrieQuad quad in currentQuadSet)
                {
                    // Substitution and Deletion

                    
                    foreach(TrieNode child in quad.node.children.Values){
                        if(quad.qIndex + 2 < query.Length  && child.children.ContainsKey(queryChars[quad.qIndex + 2])){
                            // Substituion
                            TrieNode substitutionNode = child.children[queryChars[quad.qIndex + 2]];
                            potentialQuadQueue.Enqueue(new TrieQuad(substitutionNode, substitutionNode.min, substitutionNode.max, query, quad.qIndex+2));
                        }

                        if(quad.qIndex + 1 < query.Length && child.children.ContainsKey(queryChars[quad.qIndex + 1])){
                            // Deletion
                            TrieNode deletionNode = child.children[queryChars[quad.qIndex + 1]];
                            potentialQuadQueue.Enqueue(new TrieQuad(deletionNode, deletionNode.min, deletionNode.max, query, quad.qIndex+1));
                        }
                    }

                    // Insertion
                    if(quad.qIndex + 2 < query.Length && quad.node.children.ContainsKey(queryChars[quad.qIndex + 2])){
                        TrieNode insertionNode = quad.node.children[queryChars[quad.qIndex+2]];
                        potentialQuadQueue.Enqueue(new TrieQuad(insertionNode, insertionNode.min, insertionNode.max, query, quad.qIndex+2));
                    }
                }

                while(potentialQuadQueue.Count > 0)
                {
                    TrieQuad quad = potentialQuadQueue.Dequeue();

                    if (quad.qIndex + 1 >=  query.Length)
                    {
                        topmatch.Add(quad.node.ToString());
                    }
                    else if (quad.node.children.ContainsKey(queryChars[quad.qIndex + 1]))
                    {
                        TrieNode next = quad.node.children[queryChars[quad.qIndex+1]];
                        // Is this string the best match we can get?
                        if (quad.qIndex + 1 == query.Length - 1)
                        {
                            topmatch.Add(next.ToString());
                        }
                        potentialQuadQueue.Enqueue(new TrieQuad(next, next.min, next.max, query, quad.qIndex + 1));
                    }
                    else
                    {
                        nextQuadSet.Add(quad);
                    }
                    if (topmatch.Count > k)
                    {
                        return topmatch;
                    }
                }
            }

            //Exact match for now. 
            return topmatch;

        }
    }


    class TrieQuad{
        public TrieNode node;
        public int lower;
        public int upper;
        public String query;
        public int qIndex;
        public TrieQuad(TrieNode tn, int l, int u, String q, int i){
            node = tn;
            lower = l;
            upper = u;
            query = q;
            qIndex = i;

        }
    }
}
