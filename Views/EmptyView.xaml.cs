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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HyakuninKumiteApplication.Views
{
    /// <summary>
    /// Interaction logic for EmptyView.xaml
    /// </summary>
    public partial class EmptyView : UserControl
    {
        private MainWindow mw;

        public EmptyView()
        {
            InitializeComponent();
        }

        public EmptyView(MainWindow mw)
        {
            InitializeComponent();

            this.mw = mw;
        }

        private void BtnOpenComp_Click(object sender, RoutedEventArgs e)
        {
            if (mw == null)
                return;

            mw.Open_Click(this, e);
        }
    }
}
