using HyakuninKumite.Services;
using HyakuninKumiteApplication.Controls;
using HyakuninKumiteApplication.Entities;
using HyakuninKumiteApplication.Entities.Category;
using HyakuninKumiteApplication.Models.Enums;
using HyakuninKumiteApplication.Services;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
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
using static HyakuninKumiteApplication.Entities.AgeGroup;
using static HyakuninKumiteApplication.Entities.AgeGroup.Division;
using Range = HyakuninKumiteApplication.Models.Range;

using Match = HyakuninKumiteApplication.Entities.Match;
using System.Text.RegularExpressions;

namespace HyakuninKumiteApplication.Views
{
    /// <summary>
    /// Interaction logic for KumiteView.xaml
    /// </summary>
    /// 
    public partial class BracketsView : UserControl
    {
        Competition comp;
        public AgeGroup selectedAG;
        public Division selectedDiv;
        public Category selectedCategory;

        public BracketsView(Competition comp)
        {
            InitializeComponent();
            SetCompetition(comp);

            if (comp.AgeGroups.Count == 0)
                return;
            selectedAG = comp.AgeGroups[0];
            selectedDiv = selectedAG.MaleDivision;
            selectedCategory = selectedDiv.WeightCategories[0];

            LoadDataOnComponent();
        }

        public void SetCompetition(Competition comp)
        {
            this.comp = comp;
        }

        public void LoadDataOnComponent()
        {
            txtInfo1.Text = $"Total Participants: {comp.Participants.Count}";

            LoadAgeGroups();
            LoadWeightCategories();
            LoadBracket(selectedCategory);
        }

        #region Event Handlers
        #region ScrollPanel Navigation
        private bool isSpacePressed = false;
        private bool isPanning = false;
        private Point startPoint;
        private double startHorizontalOffset;
        private double startVerticalOffset;

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space && !isSpacePressed)
            {
                isSpacePressed = true;
                MyScrollViewer.Cursor = Cursors.SizeAll;
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                isSpacePressed = false;
                isPanning = false;
                MyScrollViewer.Cursor = Cursors.Arrow;
            }
        }

        private void ScrollViewer_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isSpacePressed)
            {
                isPanning = true;
                startPoint = e.GetPosition(MyScrollViewer);
                startHorizontalOffset = MyScrollViewer.HorizontalOffset;
                startVerticalOffset = MyScrollViewer.VerticalOffset;
                MyScrollViewer.CaptureMouse();
                e.Handled = true;
            }
        }

        private void ScrollViewer_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (isPanning)
            {
                Point currentPoint = e.GetPosition(MyScrollViewer);
                Vector delta = currentPoint - startPoint;

                MyScrollViewer.ScrollToHorizontalOffset(startHorizontalOffset - delta.X);
                MyScrollViewer.ScrollToVerticalOffset(startVerticalOffset - delta.Y);
            }
        }

        private void ScrollViewer_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isPanning)
            {
                isPanning = false;
                MyScrollViewer.ReleaseMouseCapture();
                e.Handled = true;
            }
        }
        #endregion

        //Delete Function
        private void LBAgeGroups_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Delete)
                return;

            if (LBAgeGroups.SelectedItem == null)
                return;

            var item = LBAgeGroups.SelectedItem as ListBoxItem;
            var ag = (AgeGroup)item.Tag;

            comp.DeleteAgeGroup(ag);

            selectedAG = comp.AgeGroups[0];
            selectedDiv = selectedAG.MaleDivision;
            selectedCategory = selectedDiv.WeightCategories[0];

            LoadDataOnComponent();
        }
        private void GenderMaleRB_Checked(object sender, RoutedEventArgs e)
        {
            if (selectedAG == null)
                return;
            selectedDiv = selectedAG.MaleDivision;
            selectedCategory = selectedDiv.WeightCategories[0];
            LoadDataOnComponent();
        }
        private void GenderFemaleRB_Checked(object sender, RoutedEventArgs e)
        {
            if (selectedAG == null)
                return;
            selectedDiv = selectedAG.FemaleDivision;
            selectedCategory = selectedDiv.WeightCategories[0];
            LoadDataOnComponent();
        }

        private void BtnMatchmake_Click(object sender, RoutedEventArgs e)
        {
            comp.Matchmake();
            LoadDataOnComponent();

            MessageBox.Show(
                $"{comp.Participants.Count} participants have been successfully matchmaked in a total of {comp.AgeGroups.Count} age groups.",
                "Success",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }

        private void BtnNumberizeElem_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < Btn_NumberizeElem.Items.Count; i++)
            {
                var item = Btn_NumberizeElem.Items[i];

                if (item is MenuItem menuItem && menuItem.Header is CheckBox checkBox)
                {
                    bool isChecked = checkBox.IsChecked ?? false;

                    if (isChecked && i == 0)
                        comp.EnumerateParticipants();
                    if (isChecked && i == 1)
                        comp.EnumerateMatches();
                }
            }
            LoadDataOnComponent();

            MessageBox.Show(
                $"{comp.Participants.Count} participants have been successfully numberized respective to their matches.",
                "Success",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }

        private void BtnExportPDF_Click(object sender, RoutedEventArgs e)
        {
            fs.ExportToBracketPDF(comp);
        }

        //prevents non-integer input
        private void TB_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]");
            e.Handled = regex.IsMatch(e.Text);
        }

        KumiteBracketControl firstSelected = null;
        private void BracketControl_Click(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            KumiteBracketControl clicked = (KumiteBracketControl)sender;

            //select
            if (e.ClickCount == 1)
            {
                if (clicked.Match.RelativeRound != 0) //only for the first round
                    return;

                if (firstSelected == null)
                {
                    firstSelected = clicked;

                    //Focus
                    firstSelected.Select();
                    return;
                }
                else if(clicked == firstSelected)
                {
                    firstSelected.Deselect();
                    firstSelected = null;
                }
                else
                {
                    //Confirmation
                    MessageBoxResult result = MessageBox.Show(
                        "Swapping participants will cause the entire results of only this category to be resetted. " +
                        "Proceed with caution before swapping participants. " +
                        "Do you wish to proceed?",
                        "Swap Warning",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning
                    );

                    if (result == MessageBoxResult.No)
                        return;

                    KumiteBracketControl latternbtn = (KumiteBracketControl)sender;

                    Participant p1 = firstSelected.Tag as Participant;
                    Participant p2 = latternbtn.Tag as Participant;

                    latternbtn?.Match.Pair[latternbtn.Index] = p1;
                    firstSelected?.Match.Pair[firstSelected.Index] = p2;

                    (selectedCategory as WeightCategory).ResetAdvancements();

                    comp.SaveCompetitionData();

                    firstSelected = null;
                    LoadBracket(selectedCategory);
                }
            }
            //qualify
            else if (e.ClickCount == 2)
            {
                Match match = clicked.Match;
                Participant part = clicked.Participant;

                if (match.AvailableSlot(PartType.NullPart) != -1)
                {
                    MessageBox.Show(
                        "The match does not have 2 assigned participants, so one cannot qualify without another participant available.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    ); return;
                }

                (selectedCategory as WeightCategory).QualifyPart(match, part);

                firstSelected = null;
                comp.SaveCompetitionData();
                LoadBracket(selectedCategory);
            }

        }
        private void Background_Click(object sender, RoutedEventArgs e)
        {
            if (firstSelected != null)
            {
                firstSelected.Background = Brushes.Transparent;
                firstSelected = null;
            }
        }
        
        private void LBAgeGroups_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (firstSelected != null)
            {
                firstSelected.Deselect();
                firstSelected = null;
            }

            if (LBAgeGroups.SelectedItem is ListBoxItem item)
            {
                AgeGroup ag = (AgeGroup)item.Tag;

                if (ag != selectedAG)
                {
                    selectedAG = ag;
                    selectedDiv = ag.MaleDivision;
                    selectedCategory = selectedDiv.WeightCategories[0];

                    LoadDataOnComponent();
                }
            }
        }
        private void LBWeightCategories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (firstSelected != null)
            {
                firstSelected.Deselect();
                firstSelected = null;
            }

            if (LBWeightCategories.SelectedItem is ListBoxItem item)
            {
                Category wc = (Category)item.Tag;

                if (selectedCategory != wc)
                {
                    selectedCategory = wc;
                    LoadBracket(selectedCategory);
                }
            }
        }

        private void BtnAddAgeGroup_Click(object sender, RoutedEventArgs e)
        {
            comp.AddAgeGroup(new Range(0,1));
            selectedAG = comp.AgeGroups[^1];
            selectedDiv = selectedAG.MaleDivision;
            selectedCategory = selectedDiv.WeightCategories[0];

            LoadDataOnComponent();
        }
        private void TBAge_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            Keyboard.ClearFocus();

            if (TBMinAge?.Text == null || TBMaxAge?.Text == null)
                return;

            try
            {
                int min = Convert.ToInt32(TBMinAge.Text);
                int max = Convert.ToInt32(TBMaxAge.Text);

                Range r = new Range(min, max);

                AgeGroup ag = MMS.FindRangeOverlap(r, comp.AgeGroups.ToList());

                if (ag != null && ag != selectedAG)
                {
                    MessageBox.Show($"ERR: New value causes overlapping with {ag.Name}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                    return;
                }

                selectedAG?.AgeRange = r;

                comp.RenameAgeGroups();

                LoadDataOnComponent();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void TBWeight_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            Keyboard.ClearFocus();

            if (TBMinWeight?.Text == null || TBMaxWeight?.Text == null)
                return;

            try
            {
                int min = Convert.ToInt32(TBMinWeight.Text);
                int max = Convert.ToInt32(TBMaxWeight.Text);

                Range r = new Range(min, max);

                WeightCategory wc = MMS.FindRangeOverlap(r, selectedAG.GetWeightCategories());

                if (wc != null && wc != selectedCategory)
                {
                    MessageBox.Show($"ERR: New value causes overlapping with {wc.Name}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                (selectedCategory as  WeightCategory)?.WeightRange = r;

                selectedDiv.AutonameWeightCategories();

                LoadDataOnComponent();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnGenerateWC_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(TBWeightInterval.Text, out int result))
            {
                selectedDiv.WeightRange = new Range(int.Parse(TBMinWeight.Text), int.Parse(TBMaxWeight.Text));

                selectedDiv.WeightIntervals = result;
                selectedDiv.GenerateWeightCategories();

                LoadDataOnComponent();
            }
            else
            {
                MessageBox.Show($"ERR: Input must be integer", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void LBWeightCategories_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Delete)
                return;

            if (LBWeightCategories.SelectedItem == null)
                return;

            if (selectedDiv.WeightCategories.Count == 1)
                MessageBox.Show($"ERR: Cannot Delete the only Weight Category in a Division", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            var item = LBWeightCategories.SelectedItem as ListBoxItem;
            var wc = (WeightCategory)item.Tag;

            selectedDiv.RemoveWeightCategory(wc);
            selectedCategory = selectedDiv.WeightCategories[0];

            comp.SaveCompetitionData();

            LoadDataOnComponent();
        }
        #endregion

        #region UI Helpers
        private void LoadAgeGroups()
        {
            LBAgeGroups.Items.Clear();

            foreach (var ag in comp.AgeGroups)
            {
                ListBoxItem item = new ListBoxItem
                {
                    Margin = new Thickness(0, 0, 0, 0),
                    Content = $"{ag.Name} ({ag.GetParticipants().Count()})",
                    FontSize = 11,
                    Tag = ag,
                    Height = 30,
                };

                if (ag == this.selectedAG)
                    LBAgeGroups.SelectedItem = item;

                LBAgeGroups.Items.Add(item);
            }
        }
        private void LoadWeightCategories()
        {
            LBWeightCategories.Items.Clear();

            foreach (var w in selectedDiv.WeightCategories)
            {
                ListBoxItem item = new ListBoxItem
                {
                    Margin = new Thickness(0, 0, 0, 0),
                    Content = $"{w.Name} ({w.Parts.Count})",
                    FontSize = 11,
                    Tag = w,
                    Height = 30,
                };

                if (w == this.selectedCategory)
                    LBWeightCategories.SelectedItem = item;

                LBWeightCategories.Items.Add(item);
            }

            //Add kata category
            var kc = selectedDiv._KataCategory;

            ListBoxItem item2 = new ListBoxItem
            {
                Margin = new Thickness(0, 0, 0, 0),
                Content = $"{kc.Name} ({kc.Parts.Count})",
                FontSize = 11,
                Tag = kc,
                Height = 30,
            };

            if (kc == this.selectedCategory)
                LBWeightCategories.SelectedItem = item2;

            LBWeightCategories.Items.Add(item2);
        }
        public void LoadBracket(Category selectedCategory)
        {
            //Clear Panel
            BracketBorder.Child = null;

            CommandRegistry.selectedCategory = selectedCategory;

            if (selectedCategory is WeightCategory selectedWC)
            {

                //DEBUG
                /*try
                {
                    List<Participant> _Pair = selectedWC?.Rounds[selectedWC.Rounds.Count - 1].Matches[0].Pair;
                    CommandRegistry.v1stMessage = $"{_Pair[0]?.Name} vs {_Pair[1]?.Name}";
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error when assigning v1st message: {ex.Message}");
                }

                try
                {
                    List<Participant> Pair = selectedWC?.Rounds[selectedWC.Rounds.Count - 2].Matches[0].Pair;
                    CommandRegistry.v3rdMessage = $"{Pair[0]?.Name} vs {Pair[1]?.Name}";
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error when assigning v3rd message: {ex.Message}");
                }*/


                TBMinAge.Text = selectedAG.AgeRange.min.ToString();
                TBMaxAge.Text = selectedAG.AgeRange.max.ToString();

                TBMinWeight.Text = selectedWC.WeightRange.min.ToString();
                TBMaxWeight.Text = selectedWC.WeightRange.max.ToString();

                txtWCHeader.Text = $"{selectedDiv.CategoryGroupName} -- {selectedDiv.Name} {selectedWC.Name}";

                if (selectedWC.Rounds.Count <= 0)
                {
                    TextBlock tb = new TextBlock
                    {
                        Foreground = Brushes.Black,
                        FontSize = 24,
                        TextAlignment = TextAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Text = "There are no participants in this category."
                    };

                    BracketBorder.Child = tb;

                    return;
                }
                else
                {
                    Grid BracketGrid = new Grid();

                    for (int i = 0; i < selectedWC.Rounds.Count; i++)
                    {
                        var r = selectedWC.Rounds[i];

                        BracketGrid.ColumnDefinitions.Add(new ColumnDefinition() { MaxWidth = 450});

                        // Create a StackPanel for this round's column
                        StackPanel panel = new StackPanel();
                        Grid.SetColumn(panel, i);
                        Grid.SetRow(panel, 0);
                        BracketGrid.Children.Add(panel);

                        if (panel == null)
                            return;

                        panel.Children.Clear();

                        int j = 0;

                        //Calculate Margins
                        int alpha = (int)Math.Pow(2, i);

                        int controlheight = 110;
                        int controlmargin = 5;

                        int x = (alpha - 1) * (controlheight + controlmargin) / 2;
                        int gap = 2 * x + controlmargin;
                        int tm = x + controlmargin;

                        int k = -1;
                        foreach (Match m in r.Matches)
                        {
                            k++;

                            bool swapable = i == 0;

                            var mc = new MatchCardControl(m);
                            foreach (var bc in mc.ChildrenBracketControls)
                            {
                                bc.MouseLeftButtonDown += (sender, e) => BracketControl_Click(sender, e);
                            }
                            mc.Height = controlheight;

                            if (i != 0)
                            {
                                if (j == 0)
                                    mc.Margin = new Thickness(5, tm, 0, 0);
                                else
                                    mc.Margin = new Thickness(5, gap, 0, 0);
                            }
                            else
                            {
                                mc.Margin = new Thickness(5, controlmargin, 0, 0);
                            }


                            panel.Children.Add(mc);

                            j++;
                        }

                        //if last round and has 3rd place match
                        if (selectedWC.HasConsolationMatch && i == selectedWC.Rounds.Count - 1)
                        {
                            Match m = selectedWC.ConsolationMatch;

                            bool swapable = false;

                            var mc = new MatchCardControl(m);
                            foreach (var bc in mc.ChildrenBracketControls)
                            {
                                bc.MouseLeftButtonDown += (sender, e) => BracketControl_Click(sender, e);
                            }
                            mc.Height = controlheight;

                            mc.Margin = new Thickness(5, controlheight / 2, 20, 0);

                            panel.Children.Add(mc);

                            var sp = ResultSP(selectedWC.Result);

                            sp.Margin = new Thickness(28, controlheight / 3, 20, 0);

                            panel.Children.Add(sp);
                        }
                    }
                
                    BracketBorder.Child = BracketGrid;
                }
            }
            else if(selectedCategory is KataCategory selectedKC)
            {
                txtWCHeader.Text = $"{selectedDiv.CategoryGroupName} -- {selectedAG.Name} {selectedDiv.Gender} Kata Category";

                StackPanel panel = new StackPanel();

                panel.Children.Add(HeaderSP("First Round Kata Entries"));

                foreach (var ke in selectedKC.FirstRound)
                {
                    if(panel != null)
                    {
                        panel.Children.Add(new KataBracketControl(ke)
                        {
                            HorizontalAlignment = HorizontalAlignment.Left
                        });
                    }
                }

                if (selectedKC.Parts.Count < 6)
                    return;

                selectedKC.AutoQualify();
                selectedKC.InitializeResults();

                panel.Children.Add(HeaderSP("Final Round Kata Entries"));

                foreach (var ke in selectedKC.SecondRound)
                {
                    if (panel != null)
                    {
                        panel.Children.Add(new KataBracketControl(ke)
                        {
                            HorizontalAlignment = HorizontalAlignment.Left
                        });
                    }
                }

                panel.Children.Add(ResultSP(selectedKC.Result));

                BracketBorder.Child = panel;
            }
        }
        #endregion

        //Custom Control
        private StackPanel ResultSP(List <Participant> result)
        {
            StackPanel panel = new StackPanel();

            int count = 0;
            foreach(Participant p in result)
            {
                count++;

                if(p?.Name == "Bye" || p?.CompId == 0 || p == null)
                    panel.Children.Add(new TextBlock
                    {
                        Text = $"{count}. Place: TBD",
                        FontWeight = FontWeights.Regular,
                        FontSize = 12,
                        Margin = new Thickness(0, 4, 0, 0),
                        Height = 15
                    });
                else
                    panel.Children.Add(new TextBlock
                    {
                        Text = $"{count}. Place: [{p.CompId}] {p.Name} {p.Surname} / {p.BC} {p.DOP}",
                        FontWeight = FontWeights.Regular,
                        FontSize = 12,
                        Margin = new Thickness(0, 4, 0, 0),
                        Height = 30
                    });

            }

            return panel;
        }

        private Border HeaderSP(string txt)
        {
            Border panel = new Border()
            {
                Width = 800,
                Height = 30,
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(3),
                Background = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(15, 5, 0, 0)
            };

            TextBlock Text = new TextBlock()
            {
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center, // This actually centers the text characters
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                Text = txt
            };

            panel.Child = Text;

            return panel;   
    }
    }

}

