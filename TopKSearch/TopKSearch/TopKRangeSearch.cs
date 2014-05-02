using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using TrieSpace;
using MySql.Data.MySqlClient;

namespace TopKSearch
{
    class TopKRangeSearch
    {
        static Trie trie = new Trie();

        static void Main(string[] args)
        {
            // Connect to DB
            string cs = @"Server=54.186.104.127;Database=imdb;Uid=root;Pwd=405project";
            MySqlConnection conn = null;
            MySqlDataReader rdr = null;

            try
            {
                conn = new MySqlConnection(cs);
                conn.Open();
                Console.WriteLine("MySql version : {0}", conn.ServerVersion);


                string stm = "SELECT * FROM movies;";
                MySqlCommand cmd = new MySqlCommand(stm, conn);
                rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    trie.AddString(rdr.GetString(0).Trim());
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error: {0} ", ex.ToString());
                return;
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
            }

            //Index Trie
            trie.SetIds();
            Console.WriteLine("Ids Set");

            
            // Construct Trie
            try
            {
                Console.WriteLine("Trie Constructed!\n");
                Console.WriteLine("Beginning Search. To exit, input an empty line");

                bool searching = true;
                do
                {
                    Console.WriteLine("What search do you want to look at?");
                    String querystring = Console.ReadLine();

                    if (querystring == "")
                    {
                        searching = false;
                    }
                    else
                    {
                        Console.WriteLine("How many terms do you want back?");
                        String strk = Console.ReadLine();
                        int k = Int16.Parse(strk);

                        HashSet<String> topK = SearchTopK(querystring, k);

                        Console.WriteLine("Valid Strings:");
                        foreach (String found in topK)
                        {
                            Console.WriteLine(found);
                        }
                    }
                } while (searching);
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
                            trie.AddStrings(words);
                        }
                    }
                }
                trie.SetIds();
                Console.WriteLine(trie.root.max);
                Console.WriteLine("Trie constructed!");
            }

        }

        public static HashSet<String> SearchTopK(String query, int k)
        {
            HashSet<String> topmatch = new HashSet<String>();
            char[] queryChars = query.ToCharArray();

            // Find strings for threshold = 0;
            TrieNode parent = trie.root;
            HashSet<TrieQuad> prevQuadSet = new HashSet<TrieQuad>();
            HashSet<TrieQuad> currentQuadSet = new HashSet<TrieQuad>();
            prevQuadSet.Add(new TrieQuad(trie.root, trie.root.min, trie.root.max, trie.root.level, trie.root.level));

            

            int threshold = 0;

            while (topmatch.Count < k)
            {
                currentQuadSet = FindNextSetOfMatches(query, prevQuadSet);    

                foreach (TrieQuad quad in currentQuadSet)
                {
                    if (Math.Abs(quad.stringIndex - query.Length - 1) <= threshold)
                    {
                        if (quad.node.IsLeaf())
                        {
                            if (!topmatch.Contains(quad.node.ToString()))
                            {
                                topmatch.Add(quad.node.ToString());

                                if (topmatch.Count >= k)
                                {
                                    return topmatch;
                                }
                            }
                        }
                    }
                }

                prevQuadSet = currentQuadSet;
                threshold++;
            }

            //Exact match for now. 
            return topmatch;

        }


        private static HashSet<TrieQuad> FindNextSetOfMatches(String query, HashSet<TrieQuad> set)
        {
            Queue<TrieQuad> queue = new Queue<TrieQuad>();
            HashSet<TrieQuad> returnSet = new HashSet<TrieQuad>();

            foreach (TrieQuad quad in set)
            {
                queue.Enqueue(quad);
            }

            while (queue.Count > 0)
            {
                TrieQuad quad = queue.Dequeue();

                TrieNode oldNode = quad.node;
                TrieNode newNode = null;

                if (quad.queryIndex + 1 < query.Length)
                {
                    if (oldNode.children.ContainsKey(query[quad.queryIndex + 1]))
                    {
                        newNode = oldNode.children[query[quad.queryIndex+1]];

                        //Make sure that we aren't adding nodes that have already been added
                        if (oldNode.min <= newNode.min && newNode.max <= oldNode.max)
                        {
                            TrieQuad nextQuad = new TrieQuad(newNode, newNode.min, newNode.max, quad.queryIndex + 1, quad.stringIndex + 1);

                            if (newNode.IsLeaf())
                            {
                                returnSet.Add(new TrieQuad(newNode, newNode.id, newNode.id, quad.queryIndex + 1, quad.stringIndex + 1));
                            }

                            queue.Enqueue(nextQuad);

                            if (newNode.children.Count > 1)
                            {
                                if (quad.lower < newNode.min)
                                {
                                    returnSet.Add(new TrieQuad(oldNode, quad.lower, newNode.min - 1, quad.queryIndex + 1, quad.stringIndex));
                                }

                                if (quad.upper > newNode.max)
                                {
                                    returnSet.Add(new TrieQuad(oldNode, newNode.max + 1, quad.upper, quad.queryIndex + 1, quad.stringIndex));
                                }
                            }
                        }
                        else
                        {
                            // Check to see if the next character in the query is present
                            returnSet.Add(new TrieQuad(quad.node, quad.lower, quad.upper, quad.queryIndex + 1, quad.stringIndex));
                            foreach(TrieNode child in oldNode.children.Values)
                            {
                                if(child != newNode){
                                    if (child.min <= oldNode.max && child.max >= oldNode.min)
                                    {
                                        returnSet.Add(new TrieQuad(child, child.min, child.max, quad.queryIndex, child.level));

                                        if (quad.queryIndex + 1 < query.Length)
                                        {
                                            returnSet.Add(new TrieQuad(child, child.min, child.max, quad.queryIndex + 1, child.level));
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        // Try comparing this string to another character in the query
                        returnSet.Add(new TrieQuad(quad.node, quad.lower, quad.upper, quad.queryIndex + 1, quad.stringIndex));
                        foreach (TrieNode child in oldNode.children.Values)
                        {
                            if (child.min <= oldNode.max && child.max >= oldNode.min)
                            {
                                returnSet.Add(new TrieQuad(child, child.min, child.max, quad.queryIndex, child.level));
                                returnSet.Add(new TrieQuad(child, child.min, child.max, quad.queryIndex + 1, child.level));
                            }
                        }
                    }
                }
                else
                {
                    // The query string is too short, just wait it out... see if our threshold lightens up...
                    returnSet.Add(new TrieQuad(quad.node, quad.lower, quad.upper, quad.queryIndex, quad.stringIndex));
                    foreach (TrieNode child in oldNode.children.Values)
                    {
                        if (child.min <= oldNode.max && child.max >= oldNode.min)
                        {
                            returnSet.Add(new TrieQuad(child, child.min, child.max, quad.queryIndex, child.level));
                        }
                    }
                }
            }

            return returnSet;
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

        override public String ToString()
        {
            return this.node.ToString();
        }
    }
}
