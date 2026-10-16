using HyakuninKumite.Services;
using HyakuninKumiteApplication.Controls;
using HyakuninKumiteApplication.Entities;
using HyakuninKumiteApplication.Entities.Category;
using HyakuninKumiteApplication.Views.SubView;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using static HyakuninKumiteApplication.Entities.AgeGroup;

using Match = HyakuninKumiteApplication.Entities.Match;

namespace HyakuninKumiteApplication.Views
{
    /// <summary>
    /// Interaction logic for GroupManagerView.xaml
    /// </summary>
    public partial class GroupManagerView : UserControl
    {
        public Competition comp;

        public CategoryGroup selectedCG;

        private ObservableCollection<Division> divitems;

        public Action<MatchRowViewModel> OnDivisionNameClicked { get; set; }

        private ICollectionView matchView;

        public GroupManagerView(Competition comp)
        {
            InitializeComponent();

            SetCompetition(comp);
            if(comp.CategoryGroups.Count > 0)
                selectedCG = comp.CategoryGroups[0];

            LoadDataOnComponent();
        }

        public void SetCompetition(Competition comp)
        {
            this.comp = comp;
        }



        #region Event Handlers

        #region Drag Drop

        private Point dragStartPoint;
        private Division draggedItem;

        private void Card_MouseDown(object sender, MouseButtonEventArgs e)
        {
            dragStartPoint = e.GetPosition(null);

            if (((FrameworkElement)sender).DataContext is Division group)
                draggedItem = group;
        }

        private void Card_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || draggedItem == null)
                return;

            Point currentPos = e.GetPosition(null);

            if (Math.Abs(currentPos.X - dragStartPoint.X) > 5 ||
                Math.Abs(currentPos.Y - dragStartPoint.Y) > 5)
            {
                DragDrop.DoDragDrop((DependencyObject)sender, draggedItem, DragDropEffects.Move);
            }
        }

        private void DivPanel_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(Division)))
                return;

            Division droppedItem = (Division)e.Data.GetData(typeof(Division));

            FrameworkElement target = e.OriginalSource as FrameworkElement;

            while (target != null && target.DataContext is not Division)
                target = VisualTreeHelper.GetParent(target) as FrameworkElement;

            if (target?.DataContext is not Division targetItem)
                return;

            int oldIndex = divitems.IndexOf(droppedItem);
            int newIndex = divitems.IndexOf(targetItem);

            if (oldIndex != newIndex)
            {
                divitems.Move(oldIndex, newIndex);

                LoadDivisions();
            }
        }
        #endregion

        #region Filter
        private void TBMatchSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            matchView?.Refresh();
        }
        private void TBMatchSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TBMatchSearch.Text == "Filter matches...")
                TBMatchSearch.Text = "";
        }
        private void TBMatchSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TBMatchSearch.Text))
                TBMatchSearch.Text = "Filter matches...";
        }

        private bool FilterMatches(object obj)
        {
            if (obj is not MatchRowViewModel vm)
                return false;

            string search = TBMatchSearch.Text?.Trim().ToLowerInvariant();

            if (search == "filter matches...")
                return true;

            if (string.IsNullOrWhiteSpace(search))
                return true;

            return vm.MatchCompNumber.ToString().Contains(search)
                    || vm.PreMatchNum?.ToLower().Contains(search) == true
                    || vm.NextMatchNum?.ToLower().Contains(search) == true
                    || vm.DivisionName?.ToLower().Contains(search) == true
                    || vm.Round?.ToLower().Contains(search) == true
                    || vm.ShiroName?.ToLower().Contains(search) == true
                    || vm.AkaName?.ToLower().Contains(search) == true
                    || vm.WinnerName?.ToLower().Contains(search) == true;
        }
        #endregion

        private void Divitems_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            selectedCG.SetDivisions(divitems.ToList());

            comp.SaveCompetitionData();
        }

        private void ParticipantName_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock textBlock &&
                textBlock.DataContext is MatchRowViewModel vm &&
                int.TryParse(textBlock.Tag?.ToString(), out int participantIndex))
            {
                if (vm.OriginalMatch == null || vm.OriginalMatch.Pair[participantIndex] == null || vm.OriginalMatch.Pair[participantIndex].isBye)
                    return;

                Match match = vm.OriginalMatch;
                Participant part = match.Pair[participantIndex];
                vm.OriginalWC.QualifyPart(vm.OriginalMatch, part);

                comp.SaveCompetitionData();
                LoadMatchesOnGrid();
            }
        }

        private void DivisionName_Click(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is FrameworkElement element && element.Tag is MatchRowViewModel vm)
            {
                OnDivisionNameClicked?.Invoke(vm);
            }
        }
        #endregion

        #region UI Helpers
        public void LoadDataOnComponent()
        {
            var cg = selectedCG;

            if (cg == null)
                return;

            lblGroupName.Text = cg.Name;

            LoadGroups();
            LoadCategoryGroup();
        }

        private void LoadGroups()
        {
            LBCategoryGroups.Items.Clear();

            foreach (var cg in comp.CategoryGroups.ToList())
            {
                ListBoxItem item = new ListBoxItem
                {
                    Margin = new Thickness(0, 0, 0, 0),
                    Content = cg.Name,
                    FontSize = 11,
                    Tag = cg,
                    Height = 30,
                };

                if (cg == this.selectedCG)
                    LBCategoryGroups.SelectedItem = item;

                LBCategoryGroups.Items.Add(item);
            }
        }

        private void LoadCategoryGroup() 
        {
            LoadDivisions();
            LoadMatchesOnGrid();
        }

        private void LoadDivisions()
        {
            List<Division> divs = selectedCG.Divisions.ToList();

            if (divitems == null)
            {
                divitems = new ObservableCollection<Division>(divs);
                divitems.CollectionChanged += Divitems_CollectionChanged;
            }
            else
            {
                divitems.CollectionChanged -= Divitems_CollectionChanged;
                divitems.Clear();
                foreach (var d in divs) divitems.Add(d);
                divitems.CollectionChanged += Divitems_CollectionChanged;
            }

            DivPanel.ItemsSource = divitems;
            lblDivisions.Text = $"Divisions({divs.Count})";
        }

        private void LoadMatchesOnGrid()
        {
            var rowsData = new List<MatchRowViewModel>();

            foreach (var div in selectedCG.Divisions)
            {
                foreach (var wc in div.WeightCategories)
                {
                    foreach (var m in wc.AllMatches)
                    {
                        if (m.MatchCompNumber == 0)
                            continue;

                        var preMatches = wc.PreMatchesOf(m);
                        string preMatchNum = "-";
                        if (preMatches != null && preMatches.Count > 0)
                        {
                            if (preMatches[0].MatchCompNumber != 0)
                                preMatchNum = preMatches[0].MatchCompNumber.ToString();
                            else if (preMatches.Count > 1 && preMatches[1].MatchCompNumber != 0)
                                preMatchNum = preMatches[1].MatchCompNumber.ToString();
                        }

                        var nextMatch = wc.NextMatchOf(m);
                        string nextMatchNum = nextMatch != null && nextMatch.MatchCompNumber != 0 ? nextMatch.MatchCompNumber.ToString() : "-";

                        rowsData.Add(new MatchRowViewModel
                        {
                            MatchCompNumber = m.MatchCompNumber,
                            PreMatchNum = preMatchNum,
                            NextMatchNum = nextMatchNum,
                            DivisionName = $"{div.Name} {wc.Name}",
                            Round = (m.UniversalRound + 1).ToString(),
                            ShiroName = GetPartInfoString(m.Pair[0]),
                            AkaName = GetPartInfoString(m.Pair[1]),
                            WinnerName = GetPartInfoString(m.Winner) ?? "-",
                            OriginalMatch = m,
                            OriginalWC = wc,
                            OriginalDIV = div
                        });
                    }
                }
            }

            var sortedList = rowsData.OrderBy(r => r.MatchCompNumber).ToList();
            matchView = CollectionViewSource.GetDefaultView(sortedList);

            matchView.Filter = FilterMatches;

            MatchesGrid.ItemsSource = matchView;
        }

        private void MatchesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is DataGrid grid)
            {
                grid.Dispatcher.BeginInvoke(new Action(() => grid.UnselectAll()));
            }
        }
        #endregion


        private string GetPartInfoString(Participant part)
        {
            if (part == null)
                return null;

            string partInfo = part.isBye 
                ? "TBD"
                : $"[{part.CompId}] {part.Name} {part.Surname}";

            return partInfo;
        }

        //Qualify Part
        private void TxtParticipant_Click(object sender, RoutedEventArgs e)
        {
            /*
            TextBlock cell = sender as TextBlock;
            var (col, row, match) = ((int, int, Match))cell.Tag;

            int parentIndex = match.GetMatchRoundIndex();

            match.Winner = match.Pair[col - 5];

            Console.WriteLine(parentIndex);

            match.WeightCategory.QualifyPart(match, match.Winner, match.RelativeRound, parentIndex);

            comp.SaveCompetitionData();

            LoadMatchesOnGrid(selectedCG.Matches);*/
        }

        private void BtnAddGroup_Click(object sender, RoutedEventArgs e)
        {
            comp.AddCategoryGroup();

            selectedCG = comp.CategoryGroups[^1];
            LoadDataOnComponent();
        }

        private void BtnRemoveGroup_Click(object sender, RoutedEventArgs e)
        {
            if (comp.CategoryGroups.Count <= 1)
                return;

            if (LBCategoryGroups.SelectedItem is ListBoxItem item)
            {
                var cg = (CategoryGroup)item.Tag;

                comp.DeleteCategoryGroup(cg);
                selectedCG = comp.CategoryGroups[0];
                LBCategoryGroups.SelectedItem = LBCategoryGroups.Items[0];

                LoadDataOnComponent();
            }
        }

        private void BtnAddDiv_Click(object sender, RoutedEventArgs e)
        {
            AddDivCGWindow window = new AddDivCGWindow(comp.GetDivisions(), selectedCG.Divisions.ToList());

            if (window.ShowDialog() == true)
            {
                selectedCG.SetDivisions(window.selectedDivs);

                LoadCategoryGroup();
            }
        }

        private void LBGroups_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (LBCategoryGroups.SelectedItem is ListBoxItem item)
            {
                var cg = (CategoryGroup)item.Tag;

                if (cg != selectedCG)
                {
                    selectedCG = cg;

                    LoadCategoryGroup();
                }
            }
        }
    }
}
