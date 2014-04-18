using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrequencyTrieSpace.Trie;

namespace VGram
{
    class VGramSearch
    {
        public int qMax;
        public int qMin;
        public int threshold;
        public Dictionary<String, String> gramDictionary;

        static void Main(string[] args)
        {
            // Construct GramDictionary
            try
            {
                GenerateGramDictionary("The Prince.txt");

                Console.WriteLine("What search do you want to look at?");
                String querystring = Console.ReadLine();



                Console.WriteLine("Valid Strings:");


            }

            catch (Exception e)
            {
                Console.Error.WriteLine("Error Encountered: ");
                Console.Error.WriteLine(e.Message);
            }

            Console.WriteLine("Press any button to exit...");
            Console.ReadKey();

        }

        public static void GenerateGramDictionary(String filename)
        {

        }


        public void Prune()
        {

        }
    }
}
