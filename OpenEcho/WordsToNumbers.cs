/*
    OpenEcho is a program to automate basic tasks at home all while being handsfree.
    Copyright (C) 2015  Gregory Morgan

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
    }
}
