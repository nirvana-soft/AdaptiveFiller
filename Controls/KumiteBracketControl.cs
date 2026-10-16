using HyakuninKumiteApplication.Entities;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using HyakuninKumiteApplication.Models.Enums;

namespace HyakuninKumiteApplication.Controls
{
    public class KumiteBracketControl : Border
    {
        public int Index { get; set; }
        public Match Match = new Match();
        public Participant Participant;

        bool selected = false;

        public KumiteBracketControl(Match m, int index)
        {
            InitializeComponent();

            Participant = m.Pair[index];
            Tag = Participant;

            Match = m;
            Index = index;

            this.Child = ParticipantSP(Participant);
        }

        private StackPanel ParticipantSP(Participant p1)
        {
            StackPanel panel = new StackPanel();

            if (p1.Name != "Bye" && !p1.isBye && p1.Name != "Place Holder Part")
            {
                panel.Children.Add(new TextBlock
                {
                    Text = $"[{p1?.CompId}] {p1?.Name} {p1?.Surname}" ?? "BYE",
                    FontWeight = FontWeights.Bold,
                    FontSize = 14,
                });

                panel.Children.Add(new TextBlock
                {
                    Text = $"{p1?.Country} / {p1?.DOP} / {p1?.Age}y.o. / {p1?.Weight}kg" ?? "BYE",
                    FontWeight = FontWeights.Normal
                });
            }
            else
            {
                panel.Children.Add(new TextBlock
                {
                    Text = "",
                    FontWeight = FontWeights.Bold
                });

                panel.Children.Add(new TextBlock
                {
                    Text = "",
                    FontWeight = FontWeights.Normal
                });
                /*
                panel.Children.Add(new TextBlock
                {
                    Text = "BYE",
                    FontWeight = FontWeights.SemiBold
                });
                panel.Children.Add(new TextBlock
                {
                    Text = "BYE",
                    FontWeight = FontWeights.SemiBold
                });*/
            }

            return panel;
        }

        public void InitializeComponent()
        {
            BorderBrush = Brushes.Gray;
            BorderThickness = new Thickness(1);
            Background = Brushes.White;
            Margin = new Thickness(5, 0, 0, 0);
            Height = 55;
            Width = 400;
            CornerRadius = new CornerRadius(5);
            Padding = new Thickness(8);
            HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
        }

        public void Select()
        {
            selected = true;
            Background = new SolidColorBrush((System.Windows.Media.Color)ColorConverter.ConvertFromString("#e1ecf7"));
        }

        public void Deselect()
        {
            selected = false;
            Background = Brushes.White;
        }
    }
}
