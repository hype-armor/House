/*
    House is a program to automate basic tasks at home all while being handsfree.
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
using System.Web;
using System.Collections;
using System.Text.RegularExpressions;
using System.Text;

namespace ExtensionMethods
{
	public static class MyExtensions
	{

		public static string ToDate(this object value)
		{
			DateTime Datet;
			if (value.ToString() != "")
			{
				if (!DateTime.TryParse(value.ToString(), out Datet))
				{
					value = "Invalid Date! " + value;
					return null;
				}
				else
				{
					return Datet.ToString();
				}
			}
			else
			{
				return null;
			}
		}

		public static bool IsDecimal(this object value)
		{
			decimal result;
			if (Decimal.TryParse(value.ToString(), out result))
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public static decimal ToDecimal(this object value)
		{
			decimal result;
			if (Decimal.TryParse(value.ToString(), out result))
			{
				return result;
			}
			else
			{
				return decimal.MinValue;
			}
		}

		public static bool IsInt(this object value)
		{
			int Number;
			if (int.TryParse(value.ToString(), out Number))
			{
				return true;
			}
			else
			{
				return false;
			}

		}

		public static int ToInt(this object value)
		{
			int Number;
			if (int.TryParse(value.ToString(), out Number))
			{
				return Number;
			}
			else
			{
				return Number;
			}
		}

        public static string ToWords(this int number)
        {
            if (number == 0)
                return "zero";

            if (number < 0)
                return "minus " + ToWords(Math.Abs(number));

            string words = "";

            if ((number / 1000000) > 0)
            {
                words += ToWords(number / 1000000) + " million ";
                number %= 1000000;
            }

            if ((number / 1000) > 0)
            {
                words += ToWords(number / 1000) + " thousand ";
                number %= 1000;
            }

            if ((number / 100) > 0)
            {
                words += ToWords(number / 100) + " hundred ";
                number %= 100;
            }

            if (number > 0)
            {
                if (words != "")
                    words += "and ";

                var unitsMap = new[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
                var tensMap = new[] { "zero", "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };

                if (number < 20)
                    words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0)
                        words += "-" + unitsMap[number % 10];
                }
            }

            return words;
        }

        private static Dictionary<string, long> numberTable =
            new Dictionary<string, long>
                {{"zero",0},{"one",1},{"two",2},{"three",3},{"four",4},
                {"five",5},{"six",6},{"seven",7},{"eight",8},{"nine",9},
                {"ten",10},{"eleven",11},{"twelve",12},{"thirteen",13},
                {"fourteen",14},{"fifteen",15},{"sixteen",16},
                {"seventeen",17},{"eighteen",18},{"nineteen",19},{"twenty",20},
                {"thirty",30},{"forty",40},{"fifty",50},{"sixty",60},
                {"seventy",70},{"eighty",80},{"ninety",90},{"hundred",100}};

        public static long ToLong(this string numberString)
        {
            var numbers = Regex.Matches(numberString, @"\w+").Cast<Match>()
                 .Select(m => m.Value.ToLowerInvariant())
                 .Where(v => numberTable.ContainsKey(v))
                 .Select(v => numberTable[v]);
            long acc = 0, total = 0L;
            foreach (var n in numbers)
            {
                if (n >= 1000)
                {
                    total += (acc * n);
                    acc = 0;
                }
                else if (n >= 100)
                {
                    acc *= n;
                }
                else acc += n;
            }
            return (total + acc) * (numberString.StartsWith("minus",
                  StringComparison.InvariantCultureIgnoreCase) ? -1 : 1);
        }

        public static string ReplaceWithNumbers(this string numberString)
        {
            StringBuilder sb = new StringBuilder();
            string[] words = numberString.Split(new char[] {' '});
            long prevNumber = 0;
            bool add = false;
            foreach (string word in words)
            {
                string wordToAdd = "";
                if (word.Last() == 'y')
                {
                    prevNumber = word.ToLong();
                    add = true;
                    continue;
                }
                else if (!word.Trim().Contains("zero") && word.ToLong() != 0)
                {
                    if (add)
                    {
                        wordToAdd = (word.ToLong() + prevNumber).ToString();
                    }
                    else
                    {
                        wordToAdd = word.ToLong().ToString();
                    }
                    
                }
                else if (word.Trim().Contains("zero"))
                {
                    wordToAdd = "0";
                    add = false;
                }
                else
                {
                    wordToAdd = word;
                    add = false;
                }
                sb.Append(wordToAdd + " ");
            }

            return sb.ToString();
        }
	}
}