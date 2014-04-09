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

                Console.WriteLine("What search do you want to look at?");
                String querystring = Console.ReadLine();

                
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
    }
}
