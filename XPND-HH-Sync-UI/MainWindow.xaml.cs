using Hire_Hop_Interface.Interface;
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
using XPND_HH_Sync;

namespace XPND_HH_Sync_UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string f_name = "./export.csv";
        Hire_Hop_Interface.Interface.Connections.CookieConnection cookie = new Hire_Hop_Interface.Interface.Connections.CookieConnection();
        Auth auth = XPND_HH_Sync.Auth.PMY;

        public MainWindow()
        {
            InitializeComponent();

            login_failed.Visibility = Visibility.Hidden;
            if (auth.LoadFromFile())
            {
                hh_email.Text = auth.hh_email;
                hh_password.Text = auth.hh_pword;

                remeber_password.IsChecked = auth.hh_pword.Length > 0;
            }
        }
        private async void hh_login_Click(object sender, RoutedEventArgs e)
        {
            string email = hh_email.Text, password = hh_password.Text;
            bool loggedin = await Authentication.Login(cookie, email, password);
            if (loggedin)
            {
                auth.hh_email = email;
                auth.hh_pword = password;
                auth.SaveDetails(remeberPassword: remeber_password.IsChecked.Value);
            }
            else
            {
                login_failed.Visibility = Visibility.Visible;
            }
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
