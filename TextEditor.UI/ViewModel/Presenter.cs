using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using TextEditor.UI.Properties;
using TextEditor.UI.ViewModel.Infrastructure;


namespace TextEditor.UI.ViewModel
{
    public class Presenter
        : ObservableObject
    {
        private string  _currentFile;
        private string  _lastCommittedText;
        private string  _committedSize;
        private string  _status;


        public string CurrentFile
        {
            get {
                return _currentFile; 
            }
            set {
                _currentFile = value;
                RaisePropertyChangedEvent("CurrentFile");
            }
        }
        public string LastCommittedText
        {
            get { 
                return _lastCommittedText; 
            }
            set { 
                _lastCommittedText = value;
                RaisePropertyChangedEvent("LastCommittedText");
            }
        }
        public string CommittedSize
        {
            get { 
                return _committedSize; 
            }
            set { 
                _committedSize = value;
                RaisePropertyChangedEvent("CommittedSize");
            }
        }
        public string Status
        {
            get { 
                return _status; 
            }
            set { 
                _status = value;
                RaisePropertyChangedEvent("Status");
            }
        }


        public Presenter()
        {
            commitTextAndSize(string.Empty, 0);
            CurrentFile = string.Empty;
            Status = Resources.ReadyStatus;
        }


        private string selectFile<T>(string filter) where T : FileDialog, new()
        {
            var fileDialog = new T();
            fileDialog.Filter = filter;

            string initialDirectory = Path.GetFullPath(
                Path.Combine(
                    Directory.GetCurrentDirectory(),
                    Resources.DataDirectory)
                );

            if (!Directory.Exists(initialDirectory))
            {
                Directory.CreateDirectory(initialDirectory);
            }

            fileDialog.InitialDirectory = initialDirectory;

            bool? dialogResult = fileDialog.ShowDialog();

            if (dialogResult == true)
            {
                return fileDialog.FileName;
            }

            return string.Empty;
        }
        private void saveToFile(RichTextBox rtb, string filePath = null)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                filePath = selectFile<SaveFileDialog>(
                                Resources.PlainTextFileDialogFilter);
            }

            if (!string.IsNullOrEmpty(filePath))
            {
                string text = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd).Text;

                File.WriteAllText(
                    filePath, text, Encoding.Default);

                commitTextAndSize(text, new FileInfo(filePath).Length);

                string formattedFilePath = Path.ChangeExtension(
                    filePath, Resources.FormattedTextExtension);

                if (File.Exists(formattedFilePath))
                {
                    File.Delete(formattedFilePath);
                }

                using (var docStream = new FileStream(formattedFilePath, FileMode.CreateNew))
                {
                    XamlWriter.Save(rtb.Document, docStream);
                }

                File.SetAttributes(
                    formattedFilePath,
                    File.GetAttributes(formattedFilePath) | FileAttributes.Hidden);

                CurrentFile = filePath;
                Status = Resources.FileSavedStatus;
            }
        }
        private FlowDocument loadFlowDocumentFromFile(string formattedFilePath)
        {
            FlowDocument docFromFile = null;

            using (var openDocStream = new FileStream(formattedFilePath, FileMode.Open))
            {
                try
                {
                    docFromFile = XamlReader.Load(openDocStream) as FlowDocument;
                }
                catch (XamlParseException)
                {
                    docFromFile = null;

                    MessageBox.Show(
                        Resources.XamlParseErrorMessage, 
                        Resources.XamlParseErrorCaption,
                        MessageBoxButton.OK, 
                        MessageBoxImage.Warning);
                }
            }

            return docFromFile;
        }
        private bool isUserUpWithChanges(RichTextBox rtb)
        {
            string currentText = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd)
                                    .Text;

            string committedWithoutNewLine =
                LastCommittedText.TrimEnd('\n', '\r');

            string currentWithoutNewLine =
                currentText.TrimEnd('\n', '\r');

            bool isTextModified = string.CompareOrdinal(
                    committedWithoutNewLine,
                    currentWithoutNewLine) != 0;

            if (isTextModified)
            {
                var messageBuilder = new StringBuilder();
                messageBuilder.AppendLine(Resources.SaveChangesQuestion);

                if (!string.IsNullOrEmpty(CurrentFile))
                {
                    messageBuilder.AppendLine(CurrentFile);
                }

                var userAnswer = MessageBox.Show(
                    messageBuilder.ToString(), Resources.SaveChangesCaption,
                    MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                if (userAnswer == MessageBoxResult.Cancel)
                {
                    return false;
                }
                else if (userAnswer == MessageBoxResult.Yes)
                {
                    saveToFile(rtb, CurrentFile);
                    
                    MessageBox.Show(
                        Resources.SuccessfullySavedMessage, 
                        Resources.SaveChangesCaption,
                        MessageBoxButton.OK, 
                        MessageBoxImage.Information);
                }

                return true;
            }

            return true;
        }
        private void commitTextAndSize(string text, long size)
        {
            LastCommittedText = text;
            CommittedSize = 
                string.Format(Resources.CommittedSizeStatusFormat, size);
        }


        private Image SelectImage(string filePath, double width, double height)
        {
            var originalBmp = new BitmapImage(new Uri(filePath, UriKind.Absolute));
            double scale = 
                Math.Min(width / originalBmp.Width, height / originalBmp.Height);

            Image image = new Image();
            image.Width = originalBmp.Width * scale;
            image.Height = originalBmp.Height * scale;
            image.Source = originalBmp;
            
            return image;
        }


        public ICommand NewFileCommand
        {
            get {
                return new ParameterizedDelegateCommand(
                    (param) => {
                        var rtb = param as RichTextBox;

                        if (!isUserUpWithChanges(rtb))
                            return;

                        commitTextAndSize(string.Empty, 0);
                        CurrentFile = string.Empty;

                        rtb.Document.Blocks.Clear();

                        Status = Resources.NewFileStatus;
                    });
            }
        }
        public ICommand SaveCommand
        {
            get {
                return new ParameterizedDelegateCommand(
                    (param) => {
                        saveToFile(param as RichTextBox, CurrentFile);
                    });
            }
        }
        public ICommand SaveToFileCommand
        {
            get {
                return new ParameterizedDelegateCommand(
                    (param) => {
                        saveToFile(param as RichTextBox);
                    });
            }
        }
        public ICommand OpenFileCommand
        {
            get {
                return new ParameterizedDelegateCommand(
                    (param) => {
                        var rtb = param as RichTextBox;

                        if (!isUserUpWithChanges(rtb))
                            return;

                        string filePath = selectFile<OpenFileDialog>(
                            Resources.PlainTextFileDialogFilter);

                        if (!string.IsNullOrEmpty(filePath))
                        {
                            commitTextAndSize(
                                File.ReadAllText(filePath), new FileInfo(filePath).Length);
                            
                            string formattedFilePath = Path.ChangeExtension(
                                filePath, Resources.FormattedTextExtension);

                            FlowDocument docFromFile = null;

                            if (File.Exists(formattedFilePath) &&
                                (docFromFile = loadFlowDocumentFromFile(formattedFilePath)) != null)
                            {
                                rtb.Document = docFromFile;
                            }
                            else
                            {
                                rtb.Document.Blocks.Clear();
                                rtb.Document.Blocks.Add(
                                    new Paragraph(new Run(LastCommittedText)));
                            }

                            CurrentFile = filePath;
                            Status = Resources.FileOpenedStatus;
                        }
                    });
            }
        }

        public ICommand InsertPictureCommand
        {
            get {
                return new ParameterizedDelegateCommand(
                    (param) => {
                        var rtb = param as RichTextBox;

                        string filePath = selectFile<OpenFileDialog>(
                            Resources.ImageFileDialogFilter);

                        if (!string.IsNullOrEmpty(filePath))
                        {
                            Image img = SelectImage(filePath, rtb.Width, rtb.Height);

                            TextPointer tp = 
                                rtb.CaretPosition.GetInsertionPosition(LogicalDirection.Forward);

                            var floater = new Floater(new BlockUIContainer(img), tp);
                            floater.HorizontalAlignment = HorizontalAlignment.Center;
                            floater.Width = rtb.Width;
                        }
                    });
            }
        }

        public ICommand ShowAboutCommand
        {
            get {
                return new DelegateCommand(
                    () => {
                        var aboutWindow = new AboutWindow();
                        Nullable<bool> dialogResult = aboutWindow.ShowDialog();
                    });
            }
        }
    }
}
