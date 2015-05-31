/*
    OpenEcho is a program to automate basic tasks at home all while being handsfree.
    Copyright (C) 2015 Gregory Morgan

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OpenEcho
{
    class WordsToNumbers
    {
        public int retInt(string word)
        {

            Dictionary<string, int> wordList = new Dictionary<string, int>();
            wordList.Add("zero",       0);
            wordList.Add("one",        1);
            wordList.Add("two",        2);
            wordList.Add("three",      3);
            wordList.Add("four",       4);
            wordList.Add("five",       5);
            wordList.Add("six",        6);
            wordList.Add("seven",      7);
            wordList.Add("eight",      8);
            wordList.Add("nine",       9);
                                        
            wordList.Add("ten",       10);
            wordList.Add("eleven",    11);
            wordList.Add("twelve",    12);
            wordList.Add("thirteen",  13);
            wordList.Add("fourteen",  14);
            wordList.Add("fifteen",   15);
            wordList.Add("sixteen",   16);
            wordList.Add("seventeen", 17);
            wordList.Add("eighteen",  18);
            wordList.Add("nineteen",  19);
                                        
            wordList.Add("twenty",    20);
            wordList.Add("thirty",    30);
            wordList.Add("forty",     40);
            wordList.Add("fifty",     50);
            wordList.Add("sixty",     60);
            wordList.Add("seventy",   70);
            wordList.Add("eighty",    80);
            wordList.Add("ninety",    90);

            int number;
            

            if (wordList.ContainsKey(word))
            {
                return wordList[word];
            }
            else if (Int32.TryParse(word, out number))
            {
                return number;
            }
            return -1;
        }

        private Dictionary<string, long> numberTable = new Dictionary<string, long>
        {{"zero",0},{"one",1},{"two",2},{"three",3},{"four",4},
        {"five",5},{"six",6},{"seven",7},{"eight",8},{"nine",9},
        {"ten",10},{"eleven",11},{"twelve",12},{"thirteen",13},
        {"fourteen",14},{"fifteen",15},{"sixteen",16},
        {"seventeen",17},{"eighteen",18},{"nineteen",19},{"twenty",20},
        {"thirty",30},{"forty",40},{"fifty",50},{"sixty",60},
        {"seventy",70},{"eighty",80},{"ninety",90},{"hundred",100},
        {"thousand",1000}};

        public long ToLong(string numberString)
        {
            string[] words = numberString.ToLower().Split(new char[] { ' ', '-', ',' }, StringSplitOptions.RemoveEmptyEntries);
            string[] ones = {"one", "two", "three", "four", "five", "six", "seven", "eight", "nine"};
            string[] teens = {"eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen"};
            string[] tens = {"ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety"};
            Dictionary<string, int> modifiers = new Dictionary<string, int>() {
                {"billion", 1000000000},
                {"million", 1000000},
                {"thousand", 1000},
                {"hundred", 100}
            };

            if (numberString == "eleventy billion")
                return int.MaxValue; // 110,000,000,000 is out of range for an int!

            int result = 0;
            int currentResult = 0;
            int lastModifier = 1;

            foreach(string word in words) {
                if(modifiers.ContainsKey(word)) {
                    lastModifier *= modifiers[word];
                } else {
                    int n;

                    if(lastModifier > 1) {
                        result += currentResult * lastModifier;
                        lastModifier = 1;
                        currentResult = 0;
                    }

                    if((n = Array.IndexOf(ones, word) + 1) > 0) {
                        currentResult += n;
                    } else if((n = Array.IndexOf(teens, word) + 1) > 0) {
                        currentResult += n + 10;
                    } else if((n = Array.IndexOf(tens, word) + 1) > 0) {
                        currentResult += n * 10;
                    } else if(word != "and") {
                        throw new ApplicationException("Unrecognized word: " + word);
                    }
                }
            }

            return result + currentResult * lastModifier;
        }
    }
}
