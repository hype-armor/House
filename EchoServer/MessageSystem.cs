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

namespace EchoServer
{
    public class MessageSystem
    {
        private List<Message> messages = new List<Message>();

        public void CreateRequest(Guid guid, string message)
        {
            Message newMessage = new Message();
            newMessage.guid = guid;
            newMessage.message = message;
            messages.Add(newMessage);
        }

        public string CreateResponse(Guid guid)
        {
            // using the guid provided by the web server, you can get the queued messages.
            foreach (Message _message in messages)
            {
                if (_message.guid == guid)
                {
                    messages.Remove(_message);
                    return _message.message;
                }
            }

            // Nothing to respond with.
            return string.Empty;
        }

        public string GetRequest()
        {
            var requests = (from message in messages
                           orderby message.PostTime
                           where message.type == Message.Type.request
                           select message);

            if (requests.Count() > 0)
            {
                return requests.Last().message;
            }
            else
            {
                return string.Empty;
            }
        }

        public string GetResponse(Guid guid) // this is from client
        {
            var responses = (from message in messages
                                orderby message.PostTime
                                where message.type == Message.Type.response 
                                & message.guid == guid
                                select message);

            if (responses.Count() > 0)
            {
                return responses.Last().message;
            }
            else
            {
                return string.Empty;
            }
        }
    }

    public class Message
    {
        public Guid guid = Guid.NewGuid();
        public string message;
        public enum Type {request, response};
        public Type type;
        private DateTime _postTime = DateTime.Now;
        public DateTime PostTime { get { return _postTime; } }

        // add auto destroy after timeout? Might be only for month old client guids.
    }
}
