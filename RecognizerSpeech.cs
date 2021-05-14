using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.SpeechRecognition;
using Windows.UI.Core;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Popups;
using Windows.Globalization;

namespace Dictation
{
    class RecognizerSpeech
    {
        private SpeechRecognizer speechRecognizer;
        private CoreDispatcher dispatcher;
        public StringBuilder dictatedTextBuilder = new StringBuilder();
        

        public SpeechRecognizer SpeechRecognizer
        {
            get { return speechRecognizer; }
            set
            {
                if (value != null)
                {
                    speechRecognizer = value;
                }
            }
        }

        public RecognizerSpeech(CoreDispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
            this.speechRecognizer = new SpeechRecognizer();
            speechRecognizer.ContinuousRecognitionSession.ResultGenerated += ContinuousRecognitionSession_ResultGenerated;
            speechRecognizer.ContinuousRecognitionSession.Completed += ContinuousRecognitionSession_Completed;
            speechRecognizer.HypothesisGenerated += SpeechRecognizer_HypothesisGenerated;
            CompileConstrains();
        }

        public RecognizerSpeech(CoreDispatcher dispatcher, Language languageSpeech)
        {
            this.dispatcher = dispatcher;
            this.speechRecognizer = new SpeechRecognizer(languageSpeech);
            speechRecognizer.ContinuousRecognitionSession.ResultGenerated += ContinuousRecognitionSession_ResultGenerated;
            speechRecognizer.ContinuousRecognitionSession.Completed += ContinuousRecognitionSession_Completed;
            speechRecognizer.HypothesisGenerated += SpeechRecognizer_HypothesisGenerated;
            CompileConstrains();

        }


        private async void CompileConstrains()
        {
            SpeechRecognitionCompilationResult result = await speechRecognizer.CompileConstraintsAsync();
        }

        private async void ContinuousRecognitionSession_ResultGenerated(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {
            if (args.Result.Confidence == SpeechRecognitionConfidence.Medium || args.Result.Confidence == SpeechRecognitionConfidence.High)
            {
                dictatedTextBuilder.Append(args.Result.Text + " ");

                await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {

                });
            }
            else
            {
                await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {

                });
            }
        }

        private async void ContinuousRecognitionSession_Completed(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionCompletedEventArgs args)
        {
            if (args.Status != SpeechRecognitionResultStatus.Success)
            {
                if (args.Status == SpeechRecognitionResultStatus.TimeoutExceeded)
                {
                    await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        
                    });
                }
                else
                {
                    await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {

                    });
                }
            }
        }

        private async void SpeechRecognizer_HypothesisGenerated(SpeechRecognizer sender, SpeechRecognitionHypothesisGeneratedEventArgs args)
        {
            string hypothesis = args.Hypothesis.Text;
            string textboxContent = dictatedTextBuilder.ToString() + " " + hypothesis + " ...";

            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {

            });
        }

        public async void StartRecording()
        {
            if (speechRecognizer.State == SpeechRecognizerState.Idle)
            {
                await Task.Delay(1000).ConfigureAwait(true);
                await speechRecognizer.ContinuousRecognitionSession.StartAsync();
            }
        }

        public async void StopRecording()
        {
            await Task.Delay(1000).ConfigureAwait(true);
            if (speechRecognizer.State != SpeechRecognizerState.Idle)
            {
                await speechRecognizer.ContinuousRecognitionSession.CancelAsync();
            }
        }

        public void ClearRecordingText()
        {
            dictatedTextBuilder.Clear();
        }

    }
}
