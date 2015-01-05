using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenEcho
{
    class WordsToNumbers
    {
        public string retWord(int Num)
        {

            Dictionary<int, string> WordList = new Dictionary<int,string>();
            WordList.Add(0, "Zero");
            WordList.Add(1, "One");
            WordList.Add(2, "Two");
            WordList.Add(3, "Three");
            WordList.Add(4, "Four");
            WordList.Add(5, "Five");
            WordList.Add(6, "Six");
            WordList.Add(7, "Seven");
            WordList.Add(8, "Eight");
            WordList.Add(9, "Nine");
                                        
            WordList.Add(10, "Ten");
            WordList.Add(11, "Eleven");
            WordList.Add(12, "Twelve");
            WordList.Add(13, "Thirteen");
            WordList.Add(14, "Fourteen");
            WordList.Add(15, "Fifteen");
            WordList.Add(16, "Sixteen");
            WordList.Add(17, "Seventeen");
            WordList.Add(18, "Eighteen");
            WordList.Add(19, "Nineteen");
                                        
            WordList.Add(20, "Twenty");
            WordList.Add(30, "Thirty");
            WordList.Add(40, "Forty");
            WordList.Add(50, "Fifty");
            WordList.Add(60, "Sixty");
            WordList.Add(70, "Seventy");
            WordList.Add(80, "Eighty");
            WordList.Add(90, "Ninety");

            WordList.Add(100, "Hundred");
            WordList.Add(1000, "Thousand");

            return WordList[Num];
        }
    }
}
