using System;
using System.Globalization;
using System.Speech.Recognition;
using System.Threading;

namespace House
{
  class msdndSpeechTest
  {
    // Indicate whether asynchronous recognition is complete.
    static bool completed;

    public void start(string[] args)
    {
       while (true) 
       {
          // Create an in-process speech recognizer.
          using (SpeechRecognitionEngine recognizer =
            new SpeechRecognitionEngine(new CultureInfo("en-US")))
          {
            // Create a grammar for choosing cities for a flight.
            Choices cities = new Choices(new string[] 
            { "weather", "time", "temperature", "forecast" });

            GrammarBuilder gb = new GrammarBuilder();
            gb.Append("House, what is the");
            gb.Append(cities);

            // Construct a Grammar object and load it to the recognizer.
            Grammar cityChooser = new Grammar(gb);
            cityChooser.Name = ("City Chooser");
            recognizer.LoadGrammarAsync(cityChooser);

            // Attach event handlers.
            recognizer.SpeechDetected +=
              new EventHandler<SpeechDetectedEventArgs>(
                SpeechDetectedHandler);
            recognizer.SpeechHypothesized +=
              new EventHandler<SpeechHypothesizedEventArgs>(
                SpeechHypothesizedHandler);
            recognizer.SpeechRecognitionRejected +=
              new EventHandler<SpeechRecognitionRejectedEventArgs>(
                SpeechRecognitionRejectedHandler);
            recognizer.SpeechRecognized +=
              new EventHandler<SpeechRecognizedEventArgs>(
                SpeechRecognizedHandler);
            recognizer.RecognizeCompleted +=
              new EventHandler<RecognizeCompletedEventArgs>(
                RecognizeCompletedHandler);

            // Assign input to the recognizer and start an asynchronous
            // recognition operation.
            recognizer.SetInputToDefaultAudioDevice();

            completed = false;
            //Console.WriteLine("Starting asynchronous recognition...");
            recognizer.RecognizeAsync();

            // Wait for the operation to complete.
            while (!completed)
            {
              Thread.Sleep(333);
            }
            Console.WriteLine("Done.");
          }
       }
    }

    // Handle the SpeechDetected event.
    static void SpeechDetectedHandler(object sender, SpeechDetectedEventArgs e)
    {
      //Console.WriteLine(" In SpeechDetectedHandler:");
      //Console.WriteLine(" - AudioPosition = {0}", e.AudioPosition);
    }

    // Handle the SpeechHypothesized event.
    static void SpeechHypothesizedHandler(
      object sender, SpeechHypothesizedEventArgs e)
    {
      //Console.WriteLine(" In SpeechHypothesizedHandler:");
      
      string grammarName = "<not available>";
      string resultText = "<not available>";
      if (e.Result != null)
      {
        if (e.Result.Grammar != null)
        {
          grammarName = e.Result.Grammar.Name;
        }
        resultText = e.Result.Text;
      }

      //Console.WriteLine(" - Grammar Name = {0}; Result Text = {1}",
      //  grammarName, resultText);
    }

    // Handle the SpeechRecognitionRejected event.
    static void SpeechRecognitionRejectedHandler(
      object sender, SpeechRecognitionRejectedEventArgs e)
    {
      //Console.WriteLine(" In SpeechRecognitionRejectedHandler:");

      string grammarName = "<not available>";
      string resultText = "<not available>";
      if (e.Result != null)
      {
        if (e.Result.Grammar != null)
        {
          grammarName = e.Result.Grammar.Name;
        }
        resultText = e.Result.Text;
      }

      //Console.WriteLine(" - Grammar Name = {0}; Result Text = {1}",
      //  grammarName, resultText);
    }

    // Handle the SpeechRecognized event.
    static void SpeechRecognizedHandler(
      object sender, SpeechRecognizedEventArgs e)
    {
      //Console.WriteLine(" In SpeechRecognizedHandler.");

      string grammarName = "<not available>";
      string resultText = "<not available>";
      if (e.Result != null)
      {
        if (e.Result.Grammar != null)
        {
          grammarName = e.Result.Grammar.Name;
        }
        resultText = e.Result.Text;
      }

      //Console.WriteLine(" - Grammar Name = {0}; Result Text = {1}",
      //  grammarName, resultText);
    }

    // Handle the RecognizeCompleted event.
    static void RecognizeCompletedHandler(
      object sender, RecognizeCompletedEventArgs e)
    {
      Console.WriteLine(" In RecognizeCompletedHandler.");

      if (e.Error != null)
      {
        Console.WriteLine(
          " - Error occurred during recognition: {0}", e.Error);
        return;
      }
      if (e.InitialSilenceTimeout || e.BabbleTimeout)
      {
        Console.WriteLine(
          " - BabbleTimeout = {0}; InitialSilenceTimeout = {1}",
          e.BabbleTimeout, e.InitialSilenceTimeout);
        return;
      }
      if (e.InputStreamEnded)
      {
        Console.WriteLine(
          " - AudioPosition = {0}; InputStreamEnded = {1}",
          e.AudioPosition, e.InputStreamEnded);
      }
      if (e.Result != null)
      {
        Console.WriteLine(
          " - Grammar = {0}; Text = {1}; Confidence = {2}",
          e.Result.Grammar.Name, e.Result.Text, e.Result.Confidence);


        //Console.WriteLine(" - AudioPosition = {0}", e.AudioPosition);
      }
      else
      {
        Console.WriteLine(" - No result.");
      }
      completed = true;
    }
  }
}
