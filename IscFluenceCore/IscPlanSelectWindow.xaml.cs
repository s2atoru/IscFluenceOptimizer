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

namespace Juntendo.MedPhys.Esapi.IscFluence
{
    /// <summary>
    /// PlanSelectWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class IscPlanSelectWindow : Window
    {
        public IscPlanSelectWindow()
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
            IscPlanSelect iscPlanSelect = DataContext as IscPlanSelect;
            var selectedTemplatePlanId = (string)this.TemplatePlanComboBox.SelectedValue;

            if (String.IsNullOrEmpty(selectedTemplatePlanId) || String.IsNullOrWhiteSpace(selectedTemplatePlanId))
            {
                throw new InvalidOperationException($"SelectedTemplatePlanId is null, empty, or white spaces)");
            }

            iscPlanSelect.SelectedTemplatePlanId = selectedTemplatePlanId;
            DialogResult = true;
            Close();
        }
    }
}
