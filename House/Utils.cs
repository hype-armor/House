using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections;

namespace ExtensionMethods
{
	public static class MyExtensions
	{

		public static string ToDate(this object value)
		{
			DateTime Datet;
			if (value != "")
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
	}
}