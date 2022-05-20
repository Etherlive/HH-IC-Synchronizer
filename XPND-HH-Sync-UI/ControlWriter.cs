using System.IO;
using System.Text;
using System.Windows.Controls;

namespace XPND_HH_Sync_UI
{
    public class ControlWriter : TextWriter
    {
        #region Fields

        private RichTextBox textbox;

        #endregion Fields

        #region Constructors

        public ControlWriter(RichTextBox textbox)
        {
            this.textbox = textbox;
        }

        #endregion Constructors

        #region Properties

        public override Encoding Encoding
        {
            get { return Encoding.ASCII; }
        }

        #endregion Properties

        #region Methods

        public override void Write(char value)
        {
            textbox.Dispatcher.Invoke(() =>
            {
                textbox.AppendText(value.ToString().Replace("\n", ""));
                textbox.ScrollToEnd();
            });
        }

        public override void Write(string value)
        {
            textbox.Dispatcher.Invoke(() =>
            {
                textbox.AppendText(value.Replace("\n", ""));
                textbox.ScrollToEnd();
            });
        }

        #endregion Methods
    }
}