using DocumentFormat.OpenXml.Drawing.Diagrams;
using HyakuninKumite.Services;
using HyakuninKumiteApplication.Entities;
using HyakuninKumiteApplication.Views;
using System.Data;
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

namespace HyakuninKumiteApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Services
        private CompetitionListService cList = new CompetitionListService();
        private Competition selectedComp;

        //User Controls
        static OverviewView overviewView;
        static BracketsView bracketsView;
        static GroupManagerView groupManagerView;

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                cList.LoadComps();
                InitializeViews();
                LoadCompHistory(cList.GetComps());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        #region Event Handlers
        public void New_Click(object sender, RoutedEventArgs e)
        {
            Competition comp = new Competition()
            {
                Name = "New Competition",
                StartDate = "Start Date",
                FilePath = "null",
            };

            string s = fs.ExportHYAFile(comp);

            if (s != null)
            {
                comp.GenerateAgeGroups();
                comp.GenerateCategoryGroup();
                LoadComp(comp);
            }
        }

        public void Open_Click(object sender, RoutedEventArgs e)
        {
            Competition comp = fs.UploadHYAFile();
            if (comp == null)
                return;

            LoadComp(comp);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (selectedComp == null)
                return;

            selectedComp.SaveCompetitionData();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void LBCompetitionHistory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LBCompetitionHistory.SelectedItem is ListBoxItem item)
            {
                Competition comp = (Competition)item.Tag;

                if (comp != this.selectedComp)
                    LoadComp(comp);
            }
        }

        //Key Bindings
        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Save_Click(sender, e);
        }

        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Open_Click(sender, e);
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.OriginalSource != TabControl)
                return;

            RefreshViews();
        }
        #endregion

        #region UI Helpers
        private void InitializeViews()
        {
            if (selectedComp != null)
            {
                overviewView = new OverviewView(selectedComp);
                bracketsView = new BracketsView(selectedComp);
                groupManagerView = new GroupManagerView(selectedComp);
            }

            OverviewTab.Content = selectedComp == null
                ? new EmptyView(this)
                : overviewView;

            BracketsTab.Content = selectedComp == null
                ? new EmptyView(this)
                : bracketsView;

            CategoryGroupTab.Content = selectedComp == null
                ? new EmptyView(this)
                : groupManagerView;

            RefreshViews();
        }

        private void RefreshViews()
        {
            if (selectedComp == null)
                return;

            overviewView.SetCompetition(selectedComp);
            overviewView.LoadDataOnComponent();

            bracketsView.SetCompetition(selectedComp);
            bracketsView.LoadDataOnComponent();

            groupManagerView.SetCompetition(selectedComp);
            groupManagerView.LoadDataOnComponent();
            if (groupManagerView != null)
            groupManagerView.OnDivisionNameClicked = (vm) =>
            {
                Console.WriteLine("CLICKED GM");
                
                TabControl.SelectedIndex = TabControl.Items.IndexOf(BracketsTab);

                bracketsView.selectedDiv = vm.OriginalDIV;
                bracketsView.LoadBracket(vm.OriginalWC);
            };
        }

        private void LoadCompHistory(List<Competition> comps)
        {
            LBCompetitionHistory.Items.Clear();

            foreach (var comp in comps)
            {
                ListBoxItem item = new ListBoxItem
                {
                    Margin = new Thickness(0, 0, 0, 0),
                    Content = comp.Name,
                    FontSize = 11,
                    Tag = comp,
                    Height = 30,
                };

                if (comp == this.selectedComp)
                    LBCompetitionHistory.SelectedItem = item;

                LBCompetitionHistory.Items.Add(item);

            }
        }

        private void LoadComp(Competition comp)
        {
            this.selectedComp = comp;
            cList.AddComp(comp);

            InitializeViews();
        }
        #endregion
    }
}

