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

namespace Juntendo.MedPhys.Esapi.IscFluenceOptimizer
{
    /// <summary>
    /// Interaction logic for ParameterSettingView.xaml
    /// </summary>
    public partial class ParameterSettingView : Window
    {
        public ParameterSettingView()
        {
            InitializeComponent();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {    
            DialogResult = true;
            Close();
        }
    }
}
