using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.Diagnostics;

namespace Index
{
    class Program
    {
        static void Main(string[] args)
        {
            //ComputeDistanceMatrix("imdb");
            ComputeDistanceMatrix("dblp");
            //Stopwatch sw = Stopwatch.StartNew();
            
            /*
            System.IO.StreamWriter dblp = new System.IO.StreamWriter("C:\\Users\\Kyle\\Documents\\GitHub\\EECS405\\Index\\Index\\Index\\dblp\\dblpTitles3.txt");
            int c = 0;
            for (int i = 40; i < 51; i++)
            {
                Console.Clear();
                Console.WriteLine(i.ToString());
                MySqlConnection conn = new MySqlConnection(ConfigurationManager.ConnectionStrings["dblp"].ConnectionString);
                MySqlCommand cmd = new MySqlCommand("SELECT title FROM dblp.dblp_pub_new LIMIT @start,50000;", conn);
                cmd.Parameters.AddWithValue("start", i * 50000);
                conn.Open();
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    dblp.WriteLine((reader.GetString(0)).Trim());
                    c++;
                }
                conn.Close();
            }
            dblp.Close();
            
            Console.WriteLine(c);
            
            for (int i = 20; i < 60; i+=2)
            {
                Console.WriteLine(i.ToString());
                System.IO.StreamReader LCSsize = new System.IO.StreamReader("C:\\Users\\Kyle\\Documents\\GitHub\\EECS405\\Index\\Index\\Index\\dblpTitleLCSsize.txt");
                System.IO.StreamWriter LCSGT = new System.IO.StreamWriter("C:\\Users\\Kyle\\Documents\\GitHub\\EECS405\\Index\\Index\\Index\\dblpLCSGT"+i.ToString()+".txt");

                int row = 0;
                int count = 0;

                while (LCSsize.Peek() > 0)
                {
                    //Console.WriteLine(LCSsize.ReadLine());
                    string[] sizes = LCSsize.ReadLine().Split('\t');
                    int col = 0;
                    foreach (string s in sizes)
                    {
                        if (!string.IsNullOrWhiteSpace(s) && (Convert.ToInt32(s) > i) && (row != col))
                        {
                            LCSGT.WriteLine("[" + row + "," + col + "]");
                            count++;
                        }
                        col++;
                    }
                    row++;
                    if (row % 10000 == 0)
                    {
                        Console.WriteLine(row.ToString());
                    }
                }
                sw.Stop();
                Console.WriteLine("Elapsed time: " + sw.Elapsed.TotalSeconds);
                Console.WriteLine(count.ToString());
            }*/
            Console.ReadKey();
        }

        static public void ComputeDistanceMatrix(string database)
        {
            LinkedList<string> titles = new LinkedList<string>();
            /*
            MySqlConnection conn;
            MySqlCommand cmd;
            
            switch (database)
            {
                case "imdb":
                    conn = new MySqlConnection(ConfigurationManager.ConnectionStrings["imdb"].ConnectionString);
                    cmd = new MySqlCommand("SELECT title FROM movies", conn);
                    break;
                default: //Case DBLP
                    conn = new MySqlConnection(ConfigurationManager.ConnectionStrings["dblp"].ConnectionString);
                    cmd = new MySqlCommand("SELECT title FROM dblp_pub_new", conn);
                    break;
            }

            conn.Open();
            MySqlDataReader reader = cmd.ExecuteReader();
             */

            //TODO: check directory
            System.IO.StreamReader reader = new System.IO.StreamReader("C:\\Users\\James\\Documents\\GitHub\\EECS405\\Index\\Index\\Index\\dblp\\dblpTitles.txt");
            while (reader.Peek() > 0)
            {
                //Console.WriteLine(reader.Getstring(0).Trim());
                titles.AddLast(((reader.ReadLine()).Replace(" ",string.Empty).Replace("\"",string.Empty)).Trim());
                //file.WriteLine(((reader.GetString(0).Replace("\"", string.Empty)).Trim()));
            }
            //conn.Close();
            reader.Close();

            //TODO: check directory
            System.IO.StreamWriter file1 = new System.IO.StreamWriter("C:\\Users\\James\\Documents\\GitHub\\EECS405\\Index\\Index\\Index\\" + database + "\\"+database+"TitleLCS.txt");
            System.IO.StreamWriter file2 = new System.IO.StreamWriter("C:\\Users\\James\\Documents\\GitHub\\EECS405\\Index\\Index\\Index\\" + database + "\\"+database+"TitleLCSsize.txt");

            int c = 1;
            foreach (string s1 in titles)
            {
                if(c%100 == 0)
                    Console.WriteLine("Processing title " + c.ToString());

                if (((c*100 / titles.Count)%10 == 0) && ((c*100 / titles.Count) != 0))
                {
                    Console.Clear();
                    Console.WriteLine((c*100 / titles.Count).ToString() + "% Processed");
                }

                foreach (string s2 in titles)
                {
                    string lcs ="";
                    LongestCommonSubstring(s1, s2, out lcs);
                    if (lcs.Equals(string.Empty))
                        file1.Write(" \t");
                    else
                        file1.Write(lcs + "\t");
                    file2.Write(lcs.Length + "\t");
                    if (s1.Equals(s2))
                        break;
                }
                file1.WriteLine();
                file2.WriteLine();
                c++;
            }

            file1.Close();
            file2.Close();

            Console.WriteLine(titles.Count);

        }

        static public int LongestCommonSubstring(string str1, string str2)
        {
            if (string.IsNullOrEmpty(str1) || string.IsNullOrEmpty(str2))
                return 0;

            int[,] num = new int[str1.Length, str2.Length];
            int maxlen = 0;

            for (int i = 0; i < str1.Length; i++)
            {
                for (int j = 0; j < str2.Length; j++)
                {
                    if (str1[i] != str2[j])
                        num[i, j] = 0;
                    else
                    {
                        if ((i == 0) || (j == 0))
                            num[i, j] = 1;
                        else
                            num[i, j] = 1 + num[i - 1, j - 1];

                        if (num[i, j] > maxlen)
                        {
                            maxlen = num[i, j];
                        }
                    }
                }
            }
            return maxlen;
        }

        static public int LongestCommonSubstring(string str1, string str2, out string sequence)
        {
            sequence = string.Empty;
            if (string.IsNullOrEmpty(str1) || string.IsNullOrEmpty(str2))
                return 0;

            int[,] num = new int[str1.Length, str2.Length];
            int maxlen = 0;
            int lastSubsBegin = 0;
            StringBuilder sequenceBuilder = new StringBuilder();

            for (int i = 0; i < str1.Length; i++)
            {
                for (int j = 0; j < str2.Length; j++)
                {
                    if (str1[i] != str2[j])
                        num[i, j] = 0;
                    else
                    {
                        if ((i == 0) || (j == 0))
                            num[i, j] = 1;
                        else
                            num[i, j] = 1 + num[i - 1, j - 1];

                        if (num[i, j] > maxlen)
                        {
                            maxlen = num[i, j];
                            int thisSubsBegin = i - num[i, j] + 1;
                            if (lastSubsBegin == thisSubsBegin)
                            {//if the current LCS is the same as the last time this block ran
                                sequenceBuilder.Append(str1[i]);
                            }
                            else //this block resets the string builder if a different LCS is found
                            {
                                lastSubsBegin = thisSubsBegin;
                                sequenceBuilder.Length = 0; //clear it
                                sequenceBuilder.Append(str1.Substring(lastSubsBegin, (i + 1) - lastSubsBegin));
                            }
                        }
                    }
                }
            }
            sequence = sequenceBuilder.ToString();
            return maxlen;
        }
    }
}
