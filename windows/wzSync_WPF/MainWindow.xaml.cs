using System;
using System.Collections.Generic;
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
using wzSync_WPF.Classes;

namespace wzSync_WPF
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ListFileItem item1 = new ListFileItem();
            ListFileItem item2 = new ListFileItem();
            item1.Name = "Test01";  item1.Path = "/home/dev/";
            item1.Name = "Test02";  item1.Path = "/usr/local/";
            
            list_main.Items.Add(item1);
            list_main.Items.Add(item2);
        }
    }
}
