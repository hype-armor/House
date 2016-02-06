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

        public void CreateRequest(Guid ClientGuid, Stream Request) // called from client
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
        public Stream request;
        public byte[] response = new byte[0];

        private DateTime _postTime = DateTime.Now;
        public DateTime PostTime { get { return _postTime; } }

        public enum Status { queued, processing, delayed, ready, closed, error };
        public Status status = Status.queued;

        public Message(Guid ClientGuid, Stream Request)
        {
            // convert Request to string.
            Speech sp = new Speech(Request);
            while (sp.status == Speech.Status.processing)
            {
                // wait
                Thread.Sleep(10);
            }
            if (sp.status != Speech.Status.done)
            {
                // was unable to process input into text.
                status = Status.error;

                // need to report error to client using audio.
                // string to stream and then send that to the Speech class.
            }
            _clientGuid = ClientGuid;
            textRequest = sp.result;
        }
    }

    public class Speech
    {
        public string result = string.Empty;
        public enum Status { processing, done, error };
        public Status status = Status.processing;

        public Speech(Stream stream)
        {
            using (SpeechRecognitionEngine recognizer = new SpeechRecognitionEngine())
            {

                // Create and load a grammar.
                Grammar dictation = new DictationGrammar();
                dictation.Name = "Dictation Grammar";

                recognizer.LoadGrammar(dictation);

                // Configure the input to the recognizer.
                //Stream stream = new MemoryStream(buffer);
                recognizer.SetInputToAudioStream(stream, new SpeechAudioFormatInfo(
            44100, AudioBitsPerSample.Sixteen, AudioChannel.Mono));
                //recognizer.SetInputToWaveStream(stream);

                // Attach event handlers for the results of recognition.
                recognizer.SpeechRecognized +=
                  new EventHandler<SpeechRecognizedEventArgs>(recognizer_SpeechRecognized);
                recognizer.RecognizeCompleted +=
                  new EventHandler<RecognizeCompletedEventArgs>(recognizer_RecognizeCompleted);

                // Perform recognition on the entire file.
                recognizer.RecognizeAsync();
            }
        }

        // Handle the SpeechRecognized event.
        private void recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result != null && e.Result.Text != null)
            {
                result = e.Result.Text;
            }
            else
            {
                status = Status.error;
            }
        }

        // Handle the RecognizeCompleted event.
        private void recognizer_RecognizeCompleted(object sender, RecognizeCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                status = Status.error;
                Console.WriteLine("  Error encountered, {0}: {1}",
                e.Error.GetType().Name, e.Error.Message);
            }
            if (e.Cancelled)
            {
                status = Status.error;
                Console.WriteLine("  Operation cancelled.");
            }
            if (e.InputStreamEnded)
            {
                Console.WriteLine("  End of stream encountered.");
            }

            status = Status.done;
        }
    }
}
