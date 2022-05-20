using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using XPND_HH_Sync;

namespace XPND_HH_Sync_UI
{
    /// <summary>
    /// Interaction logic for Import.xaml
    /// </summary>
    public partial class Import : Window
    {
        private string targetFile = "./export.csv";

        Hire_Hop_Interface.Interface.Connections.CookieConnection cookie = new Hire_Hop_Interface.Interface.Connections.CookieConnection();
        ControlWriter _writer;
        Thread syncThread;

        public Import(Hire_Hop_Interface.Interface.Connections.CookieConnection cookie)
        {
            InitializeComponent();

            _writer = new ControlWriter(log);
            log.Width -= 15;

            Console.SetOut(_writer);
            Console.WriteLine("Please select your Expend Export CSV");

            this.cookie = cookie;
        }

        private void file_to_import_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog FileDlg = new Microsoft.Win32.OpenFileDialog();

            FileDlg.AddExtension = true;
            FileDlg.DefaultExt = ".csv";
            FileDlg.FileName = "export";
            FileDlg.InitialDirectory = Directory.GetCurrentDirectory();

            // Launch OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = FileDlg.ShowDialog();
            // Get the selected file name and display in a TextBox. Load content of file in a TextBlock
            if (result == true)
            {
                targetFile = FileDlg.FileName;
                Console.WriteLine($"Selected File Location {targetFile}");

                syncThread = new Thread(() => { XPND_HH_Sync.Sync.LoadFileAndSync(cookie, targetFile); });
                syncThread.Start();
            }
        }
    }
}
