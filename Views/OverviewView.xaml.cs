using DocumentFormat.OpenXml.Spreadsheet;
using HyakuninKumite.Services;
using HyakuninKumiteApplication.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    /// Interaction logic for OverviewView.xaml
    /// </summary>
    public partial class OverviewView : UserControl
    {
        //Services
        private Competition comp;

        ObservableCollection<Participant> parts;
        private ICollectionView participantView;

        public OverviewView(Competition comp)
        {
            InitializeComponent();

            SetCompetition(comp);
            LoadDataOnComponent();
        }

        public void SetCompetition(Competition comp)
        {
            this.comp = comp;
        }

        #region Event Handlers
        private void TBParticipantSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            participantView?.Refresh();
        }
        private void TBParticipantSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TBParticipantSearch.Text == "Search participants...")
                TBParticipantSearch.Text = "";
        }
        private void TBParticipantSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TBParticipantSearch.Text))
                TBParticipantSearch.Text = "Search participants...";
        }

        private async void BtnUploadViaExcel_Click(object sender, RoutedEventArgs e)
        {
            var _parts = await fs.UploadPartsViaExcelFile();

            if (_parts == null || _parts?.Count == 0)
                return;

            foreach(var p in _parts)
            {
                parts.Add(p);
            }

            ParticipantGrid.ItemsSource = parts;

            SetParticipantList();
        }
        private void BtnExportPart_Click(object sender, RoutedEventArgs e)
        {
            var parts = this.parts.ToList();
            fs.ExtractPartsToExcelFile(parts);
        }
        private void BtnClearPart_Click(object sender, RoutedEventArgs e)
        {
            parts.Clear();
            SetParticipantList();
            LoadDataOnComponent();
        }

        private void SetParticipantList()
        {
            comp.SetParticipantList(parts.ToList());
        }
        #endregion

        #region UI Helpers
        public void LoadDataOnComponent()
        {
            if (comp == null)
                return;

            //Load Comp Data
            TBName.Text = comp.Name;
            TBDate.Text = comp.StartDate;

            txtPartCount.Text = comp.Participants.Count.ToString();
            txtKumiteDivCount.Text = comp.AgeGroups.Count.ToString();
            txtKataDivCount.Text = comp.AgeGroups.Count.ToString();

            //Load each Age Group
            LBAgeGroups.Items.Clear();
            foreach (var ag in comp.AgeGroups)
            {
                LBAgeGroups.Items.Add(ag.Name);
            }

            //Load Participants
            parts = new ObservableCollection<Participant>(comp.Participants);

            //Initiate Search Box
            participantView = CollectionViewSource.GetDefaultView(parts);
            participantView.Filter = FilterParticipants;

            ParticipantGrid.ItemsSource = participantView;
        }

        private bool FilterParticipants(object obj)
        {
            if (obj is not Participant p)
                return false;

            string search = TBParticipantSearch.Text?.Trim().ToLowerInvariant();

            if (search == "search participants...")
                return true;

            if (string.IsNullOrWhiteSpace(search))
                return true;

            return p.Name?.ToLower().Contains(search) == true
                || p.Surname?.ToLower().Contains(search) == true
                || p.Country?.ToLower().Contains(search) == true
                || p.Gender.ToString()?.ToLower().Contains(search) == true
                || p.BC?.ToLower().Contains(search) == true
                || p.DOP?.ToLower().Contains(search) == true
                || p.Age.ToString().Contains(search)
                || p.Weight.ToString().Contains(search);
        }
        #endregion
    }
}
