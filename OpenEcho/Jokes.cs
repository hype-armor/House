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
using System.Linq;
using System.Net;
using ExtensionMethods;

namespace OpenEcho
{
    class Jokes
    {
        int random 
        {
            get 
            {
                Random r = new Random(); 
                return r.Next(100);
            }
        }

        // http://www.joke-db.com/widgets/src/wp/dad/*/3
        public string TellAJoke(string type = "dad", string keywords = "*", int index = -1)
        {
            if (index < 0)
            {
                index = random;
            }

            WebClient wc = new WebClient();
            string content = wc.DownloadString("http://www.joke-db.com/widgets/src/wp/" + type + "/" + keywords + "/" + index.ToString());
            content = content.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries).Last();
            content = content.CleanText();
            return content;
        }
    }
}
