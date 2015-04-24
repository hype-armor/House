using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenEcho
{
    class WordsToNumbers
    {
        public int retInt(string word)
        {

            Dictionary<string, int> WordList = new Dictionary<string, int>();
            WordList.Add("Zero",       0);
            WordList.Add("One",        1);
            WordList.Add("Two",        2);
            WordList.Add("Three",      3);
            WordList.Add("Four",       4);
            WordList.Add("Five",       5);
            WordList.Add("Six",        6);
            WordList.Add("Seven",      7);
            WordList.Add("Eight",      8);
            WordList.Add("Nine",       9);
                                        
            WordList.Add("Ten",       10);
            WordList.Add("Eleven",    11);
            WordList.Add("Twelve",    12);
            WordList.Add("Thirteen",  13);
            WordList.Add("Fourteen",  14);
            WordList.Add("Fifteen",   15);
            WordList.Add("Sixteen",   16);
            WordList.Add("Seventeen", 17);
            WordList.Add("Eighteen",  18);
            WordList.Add("Nineteen",  19);
                                        
            WordList.Add("Twenty",    20);
            WordList.Add("Thirty",    30);
            WordList.Add("Forty",     40);
            WordList.Add("Fifty",     50);
            WordList.Add("Sixty",     60);
            WordList.Add("Seventy",   70);
            WordList.Add("Eighty",    80);
            WordList.Add("Ninety",    90);

            return WordList[word];
        }
    }
}
