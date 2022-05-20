using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace XPND_HH_Sync_UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string f_name = "./export.csv";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void expend_file_select_Click(object sender, RoutedEventArgs e)
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
                f_name = FileDlg.FileName;
                Console.WriteLine($"Selected File Location {f_name}");
            }
        }
    }
}
