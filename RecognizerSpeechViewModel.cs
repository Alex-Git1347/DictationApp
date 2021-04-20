using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace Dictation
{
    class RecognizerSpeechViewModel 
    {
        public RecognizerSpeech RecognizerSpeech { get; set; }

        public RecognizerSpeechViewModel(CoreDispatcher dispatcher)
        {
            RecognizerSpeech = new RecognizerSpeech(dispatcher);
        }
    }
}
