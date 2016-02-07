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
using Extensions;
using System.Net;
using System.IO;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Threading;
using System.Speech.AudioFormat;

namespace EchoServer
{
    public class MessageSystem
    {
        internal int workers = 0;
        private List<Message> queue = new List<Message>();
        public int messageCount { get { return (from Message in queue
                                                orderby Message.PostTime
                                                where Message.status == Message.Status.ready
                                                select Message).Count(); } }

        public void CreateRequest(Guid ClientGuid, byte[] Request) // called from client
        {
            Message newMessage;
            if (Request.Length > 0)
            {
                newMessage = new Message(ClientGuid, Request);
                queue.Add(newMessage);
            }
        }

        public Message GetNextMessage() // called from server
        {
            IEnumerable<Message> Messages =
                (from Message in queue
                 orderby Message.PostTime
                 where Message.status == Message.Status.queued
                 select Message);

            if (Messages.Count() > 0)
            {
                Message message = Messages.Last();
                message.status = Message.Status.processing;
                return message;
            }

            return null;
        }

        public Message GetResponse(Guid ClientGuid) // called from client
        {
            IEnumerable<Message> Messages =
                   (from Message in queue
                    orderby Message.PostTime
                    where Message.clientGuid == ClientGuid
                    && (Message.status == Message.Status.ready
                    || Message.status == Message.Status.delayed)
                    select Message);
            if (Messages.Count() > 0)
            {
                Message message = Messages.Last();
                if (message.status == Message.Status.ready)
                {
                    message.status = Message.Status.closed;
                    return message;
                }
                else if (message.status == Message.Status.delayed)
                {
                    message.status = Message.Status.processing;
                    return message;
                }
            }
            return null; // need a way to tell client, we are will working on it.
        }
    }

    public class Message
    {
        private Guid _clientGuid = Guid.Empty;
        public Guid clientGuid { get { return _clientGuid; } }

        private Guid _MessageGuid = Guid.NewGuid();
        public Guid MessageGuid { get { return _MessageGuid; } }

        public string textRequest = string.Empty;
        public string textResponse = string.Empty;
        public byte[] request;
        public byte[] response = new byte[0];

        private DateTime _postTime = DateTime.Now;
        public DateTime PostTime { get { return _postTime; } }

        public enum Status { queued, processing, delayed, ready, closed, error };
        public Status status = Status.queued;

        public Message(Guid ClientGuid, byte[] Request)
        {
            
            _clientGuid = ClientGuid;
            request = Request;
        }
    }
}
