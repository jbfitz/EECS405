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
            //Some men just like living dangerously
            //try
            //{
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

                Console.WriteLine("Valid Strings:");
                foreach (String found in topK)
                {
                    Console.WriteLine(found);
                }

                
            //}
            /*
            catch (Exception e)
            {
                Console.Error.WriteLine("Error Encountered: ");
                Console.Error.WriteLine(e.Message);
            }
             * */
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
                        nextQuadSet.Add(new TrieQuad(candidateNode, candidateNode.id, candidateNode.id, candidateNode.level, candidateNode.level));
                    }
                }

                if (candidateNode.children.Count > 0)
                {

                    if( !(candidateNode.min == parent.min && candidateNode.max == parent.max))
                    {
                        // Add range smaller than the exact matching
                        if (candidateNode.min != parent.min)
                        {
                            nextQuadSet.Add(new TrieQuad(parent, parent.min, candidateNode.min - 1, parent.level, parent.level));
                        }

                        //Add range larger than exact matching
                        if (candidateNode.max != parent.max)
                        {
                            nextQuadSet.Add(new TrieQuad(parent, candidateNode.max + 1, parent.max, parent.level, parent.level));
                        }   
                    }
                    
                }
                
                parent = candidateNode;
                index++;

            }

            // There are no more exact matches)
            if (!parent.IsLeaf() || parent.children.Count > 0)
            {
                nextQuadSet.Add(new TrieQuad(parent, parent.min, parent.max, parent.level, parent.level));
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
                    if(quad.node.IsLeaf())
                    {
                        potentialQuadQueue.Enqueue(new TrieQuad(quad.node, quad.node.id, quad.node.id, quad.queryIndex, quad.stringIndex));
                    }
                    
                    // Substitution and Deletion
                    foreach(TrieNode child in quad.node.children.Values){
                        if(quad.queryIndex + 2 < query.Length  && child.children.ContainsKey(queryChars[quad.queryIndex + 2])){
                            // Substituion
                            TrieNode substitutionNode = child.children[queryChars[quad.queryIndex + 2]];
                            potentialQuadQueue.Enqueue(new TrieQuad(substitutionNode, substitutionNode.min, substitutionNode.max,quad.queryIndex+1, substitutionNode.level ));
                        }

                        if(quad.queryIndex + 1 < query.Length && child.children.ContainsKey(queryChars[quad.queryIndex + 1])){
                            // Deletion
                            TrieNode deletionNode = child.children[queryChars[quad.queryIndex + 1]];
                            potentialQuadQueue.Enqueue(new TrieQuad(deletionNode, deletionNode.min, deletionNode.max, quad.queryIndex+1, deletionNode.level));
                        }
                    }

                    // Insertion
                    if(quad.queryIndex + 2 < query.Length && quad.node.children.ContainsKey(queryChars[quad.queryIndex + 2])){
                        TrieNode insertionNode = quad.node.children[queryChars[quad.queryIndex+2]];
                        potentialQuadQueue.Enqueue(new TrieQuad(insertionNode, insertionNode.min, insertionNode.max, quad.queryIndex+2, insertionNode.level));
                    }
                }


                //Shortcut if we runout of search space
                if (potentialQuadQueue.Count == 0)
                {
                    return topmatch;
                }


                while(potentialQuadQueue.Count > 0)
                {
                    TrieQuad quad = potentialQuadQueue.Dequeue();

                    if (quad.queryIndex + 1 >=  query.Length)
                    {
                        if(quad.node.IsLeaf()){
                            topmatch.Add(quad.node.ToString());

                            if (topmatch.Count > k)
                            {
                                return topmatch;
                            }
                        }
                    }
                    else if (quad.node.children.Count > 0 && quad.node.children.ContainsKey(queryChars[quad.queryIndex + 1]))
                    {
                        TrieNode next = quad.node.children[queryChars[quad.queryIndex+1]];
                        // Is this string the best match we can get?
                        if (quad.queryIndex + 1 == query.Length - 1 && quad.node.children[queryChars[quad.queryIndex + 1]].IsLeaf())
                        {
                            topmatch.Add(next.ToString());
                            
                            if (topmatch.Count > k)
                            {
                                return topmatch;
                            }
                        }
                        potentialQuadQueue.Enqueue(new TrieQuad(next, next.min, next.max, quad.queryIndex + 1, next.level));
                    }
                    else if (quad.node.children.Count != 0 || quad.queryIndex > query.Length/2)
                    {
                        //Prune some of the less useful ones away.. 
                        quad.queryIndex++;
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
        public int queryIndex;
        public int stringIndex;
        public TrieQuad(TrieNode tn, int l, int u, int qi, int si){
            node = tn;
            lower = l;
            upper = u;
            queryIndex = qi;
            stringIndex = si;

        }
    }
}
