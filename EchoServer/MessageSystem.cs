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

namespace EchoServer
{
    class MessageSystem
    {
        private List<Message> messages = new List<Message>();

        public bool ContainsTempResponse(string guid)
        {
            return true;
        }

        public void Post(Guid guid, Message.Type messageType, string message)
        {
            Message newMessage = new Message();
            newMessage.guid = guid;
            newMessage.MessageType = messageType;
            newMessage.message = message;
            messages.Add(newMessage);
            return;
        }

        public string Get(Guid guid)
        {
        // using the guid provided by the web server, you can get the queued messages.
            foreach (Message _message in messages)
            {
                if (_message.guid == guid)
                {
                    // check if output message has been added, if not, then check for temp response.
                    if (_message.MessageType == Message.Type.output)
                    {
                        return _message.message;
                    }
                    else if (_message.MessageType == Message.Type.tempResponse)
                    {
                        return _message.message;
                    }
                }
            }

            // if nothing else has triggered, return an empty string.
            return string.Empty;

        }
    }

    public class Message
    {
        public Guid guid = Guid.NewGuid();
        public enum Type { input, output, tempResponse };
        public Type MessageType;
        public string message;
    }
}
