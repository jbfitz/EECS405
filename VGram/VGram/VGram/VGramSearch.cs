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
        public int qMax;
        public int qMin;
        public int threshold;
        public Trie vgramDictionary;
        public Dictionary<String, String> gramDictionary;

        static void Main(string[] args)
        {
            VGramSearch vgs = new VGramSearch();
            // Construct GramDictionary
            vgs.vgramDictionary = new Trie(4, 6);
            vgs.GenerateGramDictionary("The Prince.txt");
            Console.WriteLine(vgs.vgramDictionary.ToString());
            

            Console.WriteLine("What search do you want to look at?");
            String querystring = Console.ReadLine();



            Console.WriteLine("Valid Strings:");

            Console.WriteLine("Press any button to exit...");
            Console.ReadKey();

        }

        public void GenerateGramDictionary(String filename)
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
                            this.vgramDictionary.AddStrings(words);
                        }
                    }
                }
                this.vgramDictionary.Prune();
                Console.WriteLine("Trie constructed!");
            }
        }
    }
}
