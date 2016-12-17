using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using TextEditor.UI.ViewModel.Infrastructure;

namespace TextEditor.UI.ViewModel
{
    public class Presenter
        : ObservableObject
    {
        private const string DEFAULT_WINDOW_TITLE = "new";

        private string _windowTitle;
        private string _text;

        public string WindowTitle
        {
            get { 
                return _windowTitle; 
            }
            set { 
                _windowTitle = value;
                RaisePropertyChangedEvent("WindowTitle");
            }
        }

        public string Text
        {
            get { 
                return _text; 
            }
            set { 
                _text = value;
                RaisePropertyChangedEvent("Text");
            }
        }

        public Presenter()
        {
            WindowTitle = DEFAULT_WINDOW_TITLE;
            Text = "Hello, text editor!";
        }

    }
}
