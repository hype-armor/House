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

namespace EchoServer
{
    class MessageSystem
    {
        public enum MessageType { input, output, tempResponse };

        // messages <ID , <MESSAGETYPE, MESSAGE>>
        private Dictionary<string, Dictionary<MessageType, string>> messages
            = new Dictionary<string, Dictionary<MessageType, string>>();

        public void Post(string id, MessageType messageType, string message)
        {
            Dictionary<MessageType, string> messageList = new Dictionary<MessageType,string>();

            // messages might not have the id so we must check and then add a new id if it does not exist.
            if (!message.Contains(id))
            {
                Dictionary<MessageType, string> queue = new Dictionary<MessageType, string>();
                queue.Add(messageType, message);
                messages.Add(id, queue);
            }
            else
            {
                messageList = messages[id];
                messageList.Add(MessageType.output, message);
                messages.Add(id, messageList);
            }
        }

        public string Get(string id)
        {
            // using the guid provided by the web server, you can get the queued messages.

            if (messages.ContainsKey(id))
            {
                Dictionary<MessageType, string> ret = messages[id];

                // check if output message has been added, if not, then check for temp response.
                if (ret.ContainsKey(MessageType.output))
                {
                    return ret[MessageType.output];
                }
                else if (ret.ContainsKey(MessageType.tempResponse))
                {
                    return ret[MessageType.tempResponse];
                }
            }

            // if nothing else has triggered, return an empty string.
            return string.Empty;

        }
    }
}
