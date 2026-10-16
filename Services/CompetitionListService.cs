using HyakuninKumiteApplication.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;

namespace HyakuninKumite.Services
{
    public class CompetitionListService
    {
        //--> Variables
        private static List<Competition> comps = new List<Competition>();
        private static List<CompetitionListElement> compElements = new List<CompetitionListElement>();

        string compElementsListFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Data\comps.json");

        public CompetitionListService()
        {
            //CREATE JSON IF NON-EXISTENT

            string filePath = compElementsListFilePath;

            string directory = Path.GetDirectoryName(filePath);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (!File.Exists(filePath))
            {
                File.Create(filePath).Dispose();
            }
        }

        #region Competition Functions
        public void AddComp(Competition comp)
        {
            int fp_index = comps.FindIndex(c => c.FilePath == comp.FilePath);
            int name_index = comps.Count(c => c.Name == comp.Name);

            if (fp_index != -1)//--> If there is a Competition Registered from the same FILE PATH
            {
                EditComp(fp_index, comp);
                return;
            }
            else if (name_index > 0)//--> If there is a Competition Registered with the same NAME
            {
                comp.Name += $"({name_index + 1})";
            }

            comps.Add(comp);
            SaveComps();
        }
        public bool DeleteComp(int _DataID)
        {
            var comp = comps.Find(c => c.DataID == _DataID);
            if (comp != null)
            {
                comps.Remove(comp);
                SaveComps();
                return true;
            }
            return false;
        }
        public bool EditComp(Competition comp)
        {
            int index = comps.FindIndex(c => c.DataID == comp.DataID);

            return EditComp(index, comp);
        }
        public bool EditComp(int index, Competition comp)
        {
            if (index != -1)
            {
                comps[index] = comp;
                SaveComps();
                return true;
            }
            else
            {
                return false;
            }
        }
        public void ClearComps()
        {
            comps.Clear();
            SaveComps();
        }
        public List<Competition> GetComps()
        {
            return comps;
        }
        #endregion

        #region Database Functions
        public void LoadComps()
        {
            //-> Load The Competition and their FilePaths
            if(compElements == null || compElements.Count <= 0)
            {
                compElements = fs.LoadCompElementsFromJSON(compElementsListFilePath);
            }

            //-> Load The Competition Datas
            if (comps == null || comps.Count <= 0)
            {
                if (compElements == null)
                    return;

                foreach(var comp in compElements)
                {
                    if (comp == null)
                        continue;

                    var _comp = fs.LoadCompetitionFromHYAFile(comp.FilePath);

                    if(_comp != null)
                        comps.Add(_comp);
                }
            }
        }

        public void SaveComps()
        {
            compElements = new List<CompetitionListElement>();
            foreach (var comp in comps)
            {
                CompetitionListElement compElement = new CompetitionListElement
                {
                    Name = comp.Name,
                    FilePath = comp.FilePath,
                };

                compElements.Add(compElement);
            }

            fs.SaveCompElementsToJSON(compElements, compElementsListFilePath);
        }
        #endregion
    }

    //-> Additional Competition Class to avoid saving
    //-> Unneccesary information to Competition List JSON
    //-> Those information can be load afterwards in the code
    //-> LoadComps Method the second IF condition
    public class CompetitionListElement
    {
        public string Name;
        public string FilePath;
    }

}
