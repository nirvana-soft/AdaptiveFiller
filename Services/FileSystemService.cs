global using fs = HyakuninKumite.Services.FileSystemService;

using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Wordprocessing;
using HyakuninKumiteApplication;
using HyakuninKumiteApplication.Entities;
using HyakuninKumiteApplication.Models.Enums;
using Microsoft.Win32;
using Newtonsoft.Json;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.IO;
using System.Text;
using System.Windows;

using Formatting = Newtonsoft.Json.Formatting;


namespace HyakuninKumite.Services
{
    public static class FileSystemService
    {

        public static async Task<List<Participant>> UploadPartsViaExcelFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                Title = "Select your Competitor List"
            };

            if (openFileDialog.ShowDialog() != true)
            {
                return null;
            }

            string excelFilePath = openFileDialog.FileName;

            // 1. Open your loading window right before processing starts
            var loadingWin = new LoadingWindow("Decypering Participants from Excel File");
            loadingWin.Owner = Application.Current.MainWindow;
            loadingWin.Show();

            try
            {
                // 2. Offload heavy Excel processing to a background thread
                List<Participant> addedParts = await Task.Run(() =>
                {
                    List<Participant> parts = new List<Participant>();
                    using (var workbook = new XLWorkbook(excelFilePath))
                    {
                        var worksheet = workbook.Worksheet(1);
                        int totalRows = worksheet.LastRowUsed().RowNumber();

                        for (int row = 2; row <= totalRows; row++)
                        {
                            string name = worksheet.Cell(row, 1).GetString();
                            string surname = worksheet.Cell(row, 2).GetString();
                            string country = worksheet.Cell(row, 3).GetString();
                            string g = worksheet.Cell(row, 4).GetString();
                            string dateofbirth = worksheet.Cell(row, 5).GetFormattedString();
                            string bc = worksheet.Cell(row, 7).GetString();
                            string dop = worksheet.Cell(row, 8).GetString();
                            string strkumite = worksheet.Cell(row, 11).GetString();
                            string strkata = worksheet.Cell(row, 12).GetString();

                            bool kumite = strkumite.Equals("Yes", StringComparison.OrdinalIgnoreCase);
                            bool kata = strkata.Equals("Yes", StringComparison.OrdinalIgnoreCase);

                            Gender gender = (g == "M" || g == "Male") ? Gender.Male : Gender.Female;

                            float weight = 0;

                            string rweight = worksheet.Cell(row, 10).GetFormattedString();

                            // Clean and parse weight correctly
                            if (!string.IsNullOrWhiteSpace(rweight) && rweight != "-")
                            {
                                rweight = rweight.Replace("Kg", "").Replace("kg", "").Trim();
                                if (float.TryParse(rweight, out float parsedW))
                                    weight = parsedW;
                            }

                            Participant part = new Participant
                            {
                                Name = name,
                                Surname = surname,
                                Country = country,
                                Gender = gender,
                                DateOfBirth = ParseDate(dateofbirth),
                                BC = bc,
                                DOP = dop,
                                Weight = weight,
                                Kumite = kumite,
                                Kata = kata,
                            };

                            parts.Add(part);
                        }
                    }
                    return parts;
                });

                return addedParts;
            }
            catch (Exception ex)
            {
                string errmessage = "Error reading Excel file: " + ex.Message;
                MessageBox.Show(errmessage);
                return null;
            }
            finally
            {
                // 3. Ensure the loading window always closes, even if an exception occurs
                loadingWin.Close();
                string message = $"Imported Participants Successfully!";
                MessageBox.Show(message);
            }
        }
        private static DateTime ParseDate(string dateOfBirth)
        {
            if (dateOfBirth == null)
                return new DateTime(1453, 01, 01);

            DateTime currentDate = DateTime.Now;

            string[] parts = dateOfBirth.Split('.', '/');

            if (parts.Length != 3)
                throw new FormatException($"Invalid date format.");

            int d = int.Parse(parts[0]);
            int m = int.Parse(parts[1]);
            int y = int.Parse(parts[2]);

            if (m >= 12)
                m = 12;

            return new DateTime(y, m, d);
        }
        public static void ExtractPartsToExcelFile(List<Participant> parts)
        {
            string exportFilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads\\";
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                InitialDirectory = exportFilePath,
                FileName = "competitor_list.xlsx",
                Filter = "Excel Files|*.xlsx",
                Title = "Select where the Competitor List will be exported to..."
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    exportFilePath = saveFileDialog.FileName;

                    string successMessage = "Extracting Participants List to a Excel File Successfully";
                    MessageBox.Show(successMessage);
                }
                catch (Exception ex)
                {
                    string errmessage = "Error reading Excel file: " + ex.Message;
                    MessageBox.Show(errmessage);
                }
            }
            else { return; }

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Competitor List");

                // Add headers
                worksheet.Cell(1, 1).Value = "Name";
                worksheet.Cell(1, 2).Value = "Surname";
                worksheet.Cell(1, 3).Value = "Country";
                worksheet.Cell(1, 4).Value = "Gender";
                worksheet.Cell(1, 5).Value = "Date of Birth";
                worksheet.Cell(1, 6).Value = "Membership No";
                worksheet.Cell(1, 7).Value = "BC";
                worksheet.Cell(1, 8).Value = "DOP";
                worksheet.Cell(1, 9).Value = "Height";
                worksheet.Cell(1, 10).Value = "Weight";
                worksheet.Cell(1, 11).Value = "Kumite";
                worksheet.Cell(1, 12).Value = "Kata";

                // Add data
                for (int i = 0; i < parts.Count; i++)
                {
                    string strcompID = "TBD";
                    if (parts[i].CompId != 0)
                        strcompID = parts[i].CompId.ToString();
                    string strkumite = "No";
                    if (parts[i].Kumite)
                        strkumite = "Yes";

                    string strkata = "No";
                    if (parts[i].Kata)
                        strkata = "Yes";

                    worksheet.Cell(i + 2, 1).Value = strcompID;
                    worksheet.Cell(i + 2, 2).Value = parts[i].Name;
                    worksheet.Cell(i + 2, 3).Value = parts[i].Surname;
                    worksheet.Cell(i + 2, 4).Value = parts[i].Country;
                    worksheet.Cell(i + 2, 5).Value = parts[i].Gender.ToString();
                    worksheet.Cell(i + 2, 6).Value = parts[i].DateOfBirth;
                    worksheet.Cell(i + 2, 8).Value = parts[i].BC;
                    worksheet.Cell(i + 2, 9).Value = parts[i].DOP;
                    worksheet.Cell(i + 2, 11).Value = parts[i].Weight;
                    worksheet.Cell(i + 2, 12).Value = strkumite;
                    worksheet.Cell(i + 2, 13).Value = strkata;
                }

                // Save the Excel file
                workbook.SaveAs(exportFilePath);
            }
        }

        public static Competition UploadHYAFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "HYA Files|*.hya",
                Title = "Select your Competition Project File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string message = "Uploading Competition Project File...";
                MessageBox.Show(message);

                string filePath = openFileDialog.FileName;

                try
                {
                    Competition comp = LoadCompetitionFromHYAFile(filePath);
                    string successMessage = "Loaded Competition Project File!";
                    MessageBox.Show(successMessage);

                    return comp;
                }
                catch (Exception ex)
                {
                    string errmessage = "Error reading HYA file: " + ex.Message;
                    MessageBox.Show(errmessage);

                    return null;
                }
            }
            else
            {
                return null;
            }
        }
        public static string ExportHYAFile(Competition comp)
        {
            string exportFilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads"
            );
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                InitialDirectory = exportFilePath,
                FileName = $"{comp.Name}.hya",
                Filter = "HYA Files|*.hya",
                Title = "Select where the Competition project file will be exported to..."
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    exportFilePath = saveFileDialog.FileName;

                    comp.FilePath = exportFilePath;
                    SaveCompetitionToHYAFile(comp, exportFilePath);

                    string successmessage = "Successfully Extracted Competition to a HYA Project File!";
                    MessageBox.Show(successmessage);

                    return exportFilePath;
                }
                catch (Exception ex)
                {
                    string errmessage = "Error reading HYA file: " + ex.Message;
                    MessageBox.Show(errmessage);

                    return null;
                }

            }
            else { return null; }
        }
        public static Competition LoadCompetitionFromHYAFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                string compJSONData = File.ReadAllText(filePath);
                var settings = new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };
                return JsonConvert.DeserializeObject<Competition>(compJSONData, settings);
            }
            return null;
        }

        public static void SaveCompetitionToHYAFile(Competition comp, string destinationFilePath, string sourceFilePath)
        {
            if (sourceFilePath != null)
                File.Delete(sourceFilePath);

            var settings = new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            string jsonString = JsonConvert.SerializeObject(comp, Formatting.Indented, settings);
            File.WriteAllText(destinationFilePath, jsonString);
        }
        public static void SaveCompetitionToHYAFile(Competition comp, string destinationFilePath)
        {
            SaveCompetitionToHYAFile(comp, destinationFilePath, null);
        }

        public static void ExportToBracketPDF(Competition comp)
        {
            string filePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads\\";
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                InitialDirectory = filePath,
                FileName = $"competitor_list_{DateTime.Now.ToString("HHmmss_ddMM")}.pdf",
                Filter = "PDF Files|*.pdf",
                Title = "Select where the List will be exported to..."
            };

            if (saveFileDialog.ShowDialog() != true)
            {
                return;
            }

            BD.DrawBrackets(comp).Save(saveFileDialog.FileName);
        }

        public static List<CompetitionListElement> LoadCompElementsFromJSON(string filePath)
        {
            if (File.Exists(filePath))
            {
                var jsonData = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<List<CompetitionListElement>>(jsonData);
            }
            return new List<CompetitionListElement>();
        }
        public static void SaveCompElementsToJSON(List<CompetitionListElement> comps, string filePath)
        {
            var jsonData = JsonConvert.SerializeObject(comps, Formatting.Indented);
            File.WriteAllText(filePath, jsonData);
        }

    }
}
