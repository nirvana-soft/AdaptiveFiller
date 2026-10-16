using HyakuninKumiteApplication.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace HyakuninKumiteApplication.Controls
{
    public class KataBracketControl : Border
    {
        public KataEntry KataEntry { get; set; }

        private TextBlock TotalScoreText = new TextBlock()
        {
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center, // This actually centers the text characters
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            FontWeight = FontWeights.Bold,
            FontSize = 14
        };

        public KataBracketControl(KataEntry ke)
        {
            KataEntry = ke;

            InitializeComponent();

            Tag = KataEntry;
        }

        public void InitializeComponent()
        {
            BorderBrush = Brushes.Gray;
            BorderThickness = new Thickness(1);
            Background = Brushes.White;
            Margin = new Thickness(15, 10, 0, 0);
            Height = 60;
            Width = 800;
            CornerRadius = new CornerRadius(5);
            Padding = new Thickness(8);
            HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;

            Grid grid = new Grid()
            {
                HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                VerticalAlignment = System.Windows.VerticalAlignment.Stretch,
            };
            for (int i = 0; i < 7; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            if (KataEntry == null)
                return;

            StackPanel participantPanel = ParticipantSP(KataEntry.Part);
            Grid.SetColumn(participantPanel, 0);
            grid.Children.Add(participantPanel);
            grid.ColumnDefinitions[0].Width = new GridLength(250);

            // 3. Create 5 TextBoxes for columns 1 through 5
            for (int i = 0; i < 5; i++)
            {
                TextBox textBox = new TextBox()
                {
                    Margin = new Thickness(5, 0, 5, 0)
                };

                // Store the order number (0 to 4, or i + 1 if you prefer 1 to 5) in the Tag property
                textBox.Tag = i;

                textBox.Text = KataEntry.Scores[i].ToString();

                // Restrict input to integers only
                textBox.PreviewTextInput += TextBox_PreviewTextInput;

                // Trigger common action on update, passing the order number
                textBox.TextChanged += TextBox_TextChanged;

                // Place in columns 1, 2, 3, 4, and 5
                Grid.SetColumn(textBox, i + 1);
                grid.Children.Add(textBox);
            }

            TotalScoreText.Text = KataEntry.TotalScore.ToString();
            Grid.SetColumn(TotalScoreText, 6);
            grid.Children.Add(TotalScoreText);

            Child = grid;
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9.]"); // Matches anything that is NOT a digit
            e.Handled = regex.IsMatch(e.Text);
        }

        // Common action triggered when any of the 5 textboxes change
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.Tag is int orderNumber)
            {
                // Get the current text value (default to 0 or handle empty strings safely)
                string currentText = textBox.Text;
                string parsedText = currentText.IsWhiteSpace() ? "0" : currentText;

                // Call your common action here, passing the order number
                KataEntry.SetScore(orderNumber, float.Parse(parsedText));
                TotalScoreText.Text = KataEntry.TotalScore.ToString();
            }
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
                    FontSize = 13,
                });

                panel.Children.Add(new TextBlock
                {
                    Text = $"{p1?.Country} / {p1?.DOP} / {p1?.Age}y.o." ?? "BYE",
                    FontWeight = FontWeights.Normal,
                    FontSize = 11,
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
    }
}
