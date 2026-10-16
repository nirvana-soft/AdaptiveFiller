using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static HyakuninKumiteApplication.Entities.AgeGroup;
using static HyakuninKumiteApplication.Entities.AgeGroup.Division;

namespace HyakuninKumiteApplication.Views.SubView
{
    /// <summary>
    /// Interaction logic for AddDivCGWindow.xaml
    /// </summary>
    public partial class AddDivCGWindow : Window
    {
        public List<Division> selectedDivs = new List<Division>();

        public AddDivCGWindow(List<Division> divisions, List<Division> iSelectedDivs)
        {
            InitializeComponent();

            LoadDivisions(divisions, iSelectedDivs);
        }

        private void LoadDivisions(List<Division> divs, List<Division> iDivs)
        {
            LBDivisions.Items.Clear();

            foreach(var div in divs)
            {
                ListBoxItem item = new ListBoxItem
                {
                    Margin = new Thickness(0, 0, 0, 0),
                    Content = div.Name,
                    FontSize = 11,
                    Tag = div,
                    Height = 30,
                };

                LBDivisions.Items.Add(item);

                if (iDivs.Contains(div))
                    LBDivisions.SelectedItems.Add(item);
            }
        }

        private void AddSelected_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = (ListBoxItem)LBDivisions.SelectedItem;

            foreach(var item in LBDivisions.SelectedItems)
            {
                var _item = (ListBoxItem)item;

                selectedDivs.Add((Division)_item.Tag);
            }

            DialogResult = selectedDivs.Count > 0;
        }
    }
}
