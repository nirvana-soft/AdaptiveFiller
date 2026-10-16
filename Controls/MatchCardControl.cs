using HyakuninKumiteApplication.Entities;
using HyakuninKumiteApplication.Models.Enums;
using HyakuninKumiteApplication.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace HyakuninKumiteApplication.Controls
{
    public class MatchCardControl : Border
    {
        public List<KumiteBracketControl> ChildrenBracketControls = new List<KumiteBracketControl>();

        public void InitializeComponent()
        {
            BorderBrush = Brushes.Transparent;
            BorderThickness = new Thickness(0);
            Background = Brushes.Transparent;
            Margin = new Thickness(5, 5, 0, 12);
            CornerRadius = new CornerRadius(0);
        }

        public MatchCardControl(Match match)
        {
            Grid parentGrid = new Grid
            {
                ColumnDefinitions =
                    {
                        new ColumnDefinition { Width = new GridLength(20) },
                        new ColumnDefinition { Width = GridLength.Auto }
                    },
            };

            TextBlock matchNumber = new TextBlock
            {
                Text = $"{match.MatchCompNumber}",
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                FontWeight = FontWeights.Bold,
                FontSize = 12,
            };

            Grid.SetColumn(matchNumber, 0);
            if (match.MatchCompNumber != 0)
                parentGrid.Children.Add(matchNumber);

            Grid partGrid = new Grid
            {
                RowDefinitions =
                    {
                        new RowDefinition {Height = GridLength.Auto},
                        new RowDefinition {Height = GridLength.Auto}

                },
                HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch

            };
            Grid.SetColumn(partGrid, 1);

            for (int j = 0; j < 2; j++)
            {
                var bc = new KumiteBracketControl(match, j);
                bc.MouseRightButtonDown += BracketControl_Inspect;
                Grid.SetRow(bc, j);

                if (match.Winner == match.Pair[j] && match.GetMatchType() == MatchType.FullMatch)
                    bc.Background = Brushes.LightGreen;
                else
                    bc.Background = Brushes.White;

                partGrid.Children.Add(bc);
                ChildrenBracketControls.Add(bc);
            }

            parentGrid.Children.Add(partGrid);
            Child = parentGrid;
        }

        private void BracketControl_Inspect(object sender, RoutedEventArgs e)
        {
            KumiteBracketControl bc = (KumiteBracketControl)sender;
            Participant p = bc.Tag as Participant;

            MessageBox.Show(p.ToString());
        }
    }
}
