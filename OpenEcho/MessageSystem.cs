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

using System.Collections.Generic;

namespace OpenEcho
{
    class MessageSystem
    {
        enum MessageType { input, output };

        // messages <ID , <MESSAGETYPE, MESSAGE>>
        private Dictionary<string, Dictionary<MessageType, string>> messages
            = new Dictionary<string, Dictionary<MessageType, string>>();

        public void Post(string id, string message)
        {
            Dictionary<MessageType, string> messageList = new Dictionary<MessageType,string>();
            messageList = messages[id];
            messageList.Add(MessageType.output, message);
            messages.Add(id, messageList);
        }
    }
}
