using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Puch.JPK
{
    /// <summary>
    /// Interaction logic for ItemEditView.xaml
    /// </summary>
    public partial class ItemEditView : Window
    {
        public ItemEditView()
        {
            InitializeComponent();
        }

        private void Button_OK_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            this.DialogResult = true;
        }
    }
}
