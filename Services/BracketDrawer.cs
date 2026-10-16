global using BD = HyakuninKumiteApplication.Services.BracketDrawer;

using HyakuninKumite.Services;
using HyakuninKumiteApplication.Entities;
using HyakuninKumiteApplication.Entities.Category;
using HyakuninKumiteApplication.Models.Enums;
using Microsoft.Win32;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Text;

namespace HyakuninKumiteApplication.Services
{
    public static class BracketDrawer
    {
        private static readonly XFont TitleFont = new XFont("Arial", 14, XFontStyle.Bold);
        private static readonly XFont SubtitleFont = new XFont("Arial", 10, XFontStyle.Regular);
        private static readonly XFont PrimaryFont = new XFont("Arial", 8, XFontStyle.Bold);
        private static readonly XFont SecondaryFont = new XFont("Arial", 7);

        public static PdfDocument DrawBrackets(Competition comp)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            PdfDocument document = new PdfDocument();

            // Table of Contents Page
            PdfPage tocPage = document.AddPage();
            tocPage.Size = PdfSharp.PageSize.A4;
            tocPage.Orientation = PdfSharp.PageOrientation.Portrait;

            XGraphics tocGfx = XGraphics.FromPdfPage(tocPage);
            tocGfx.DrawString("Table of Contents", TitleFont, XBrushes.Black, new XRect(30, 18, tocPage.Width, 40), XStringFormats.CenterLeft);

            int uniWCCount = 0;

            foreach (AgeGroup ag in comp.AgeGroups)
            {
                for (int i = 0; i <= 1; i++)
                {
                    AgeGroup.Division div = (i == 0) ? ag.MaleDivision : ag.FemaleDivision;
                    if (div?.WeightCategories == null) continue;

                    foreach (var selectedWC in div.WeightCategories)
                    {
                        if (selectedWC.Rounds.Count <= 0 || selectedWC.Parts.Count <= 1)
                            continue;

                        uniWCCount++;

                        PdfPage page = document.AddPage();
                        page.Size = PdfSharp.PageSize.A4;
                        page.Orientation = PdfSharp.PageOrientation.Portrait;

                        XGraphics gfx = XGraphics.FromPdfPage(page);

                        string groupName = div.CategoryGroupName;
                        string mainTitle = groupName;
                        string subTitle = $"{ag.Name} {div.Gender} {selectedWC.Name}";

                        gfx.DrawString(mainTitle, TitleFont, XBrushes.Black, new XRect(0, 18, page.Width, 40), XStringFormats.Center);
                        gfx.DrawString(subTitle, TitleFont, XBrushes.Black, new XRect(0, 36, page.Width, 40), XStringFormats.Center);

                        // Table of Contents Entry
                        string tocEntry = $"{groupName} - {ag.Name} {div.Gender} {selectedWC.Name}";
                        string dots = new string('.', Math.Max(0, 120 - tocEntry.Length));

                        tocGfx.DrawString(
                            tocEntry + dots + uniWCCount,
                            SubtitleFont,
                            XBrushes.Black,
                            new XRect(40, 18 + (16 * uniWCCount), page.Width, 40),
                            XStringFormats.CenterLeft
                        );

                        DrawBracket(gfx, selectedWC, 10, 55, 165, 45);
                    }
                }
            }

            return document;
        }

        public static void DrawBracket(
            XGraphics gfx,
            WeightCategory wc,
            double startX,
            double startY,
            int width,
            int height
            )
        {
            var rounds = wc.Rounds;

            if (rounds.Count == 0)
                return;

            for (int i = 0; i < rounds.Count; i++)
            {
                int totalParticipantsCount = rounds[i].Matches.Count * 2;
                int alpha = (int)Math.Pow(2, i);

                float lineThickness = 0.1f;
                int verticalControlMargin = (height / 3) * 2;

                float x = (alpha - 1) * (lineThickness + verticalControlMargin) / 2f;
                float gap = (2 * x) + verticalControlMargin;
                float tm = x + verticalControlMargin;

                int lineWidth = width / 6;
                double respectiveStartX = startX;
                double hLineEndPoint = respectiveStartX + width + lineWidth;

                if (i == 0)
                {
                    respectiveStartX += width;
                }
                else if (i == 1)
                {
                    respectiveStartX = hLineEndPoint;
                    hLineEndPoint = respectiveStartX + lineWidth;
                }
                else if (i > 1)
                {
                    respectiveStartX = hLineEndPoint + (lineWidth * (i - 1));
                    hLineEndPoint = respectiveStartX + lineWidth;
                }

                for (int k = 0; k < totalParticipantsCount; k++)
                {
                    double y1;
                    int matchIndex = (int)MathF.Floor(k / 2f);
                    Match m = rounds[i].Matches[matchIndex];
                    bool isBye = false;

                    if (i != 0)
                    {
                        y1 = (k == 0) ? startY + tm : startY + tm + (gap * k);
                    }
                    else
                    {
                        y1 = startY + (verticalControlMargin * (k + 1));

                        if (m.GetMatchType() == MatchType.FullMatch)
                        {
                            Participant p = m.Pair[k % 2];
                            CreateBracketControl(p, respectiveStartX - width, y1 - (height / 4.0), m.Winner == p, gfx, width, height / 2);
                        }
                        else
                        {
                            isBye = true;
                            if (k % 2 == 0)
                            {
                                Participant p = m.Pair[k % 2];
                                CreateBracketControl(p, respectiveStartX - width, ((y1 * 2) + gap) / 2.0 - (height / 4.0), m.Winner == p, gfx, width, height / 2);
                            }
                        }
                    }

                    double y2 = y1 + gap;

                    if (!isBye)
                    {
                        gfx.DrawLine(XPens.Black, respectiveStartX, y1, hLineEndPoint, y1 + lineThickness);

                        if (k % 2 == 0)
                        {
                            gfx.DrawLine(XPens.Black, hLineEndPoint, y1, hLineEndPoint, y2);

                            if (m.MatchCompNumber != 0)
                            {
                                gfx.DrawString(
                                    $"{m.MatchCompNumber}",
                                    SecondaryFont,
                                    XBrushes.Black,
                                    new XRect(hLineEndPoint - 14, ((y1 + y2) / 2.0) - 3, 12, 12),
                                    XStringFormats.Center
                                );
                            }
                        }
                    }
                    else if (isBye && k % 2 == 0)
                    {
                        float diff = (gap / 2f) - 0.05f;
                        double _y1 = y1 + diff + (k * 0.05);

                        gfx.DrawLine(XPens.Black, respectiveStartX, _y1, hLineEndPoint, _y1 + lineThickness);
                    }

                    //If last round
                    if (i == rounds.Count - 1 && k == 0)
                    {
                        gfx.DrawLine(XPens.Black, hLineEndPoint, (y1 + y2) / 2.0, hLineEndPoint + (lineWidth / 2.0), (y1 + y2) / 2.0 + lineThickness);

                        //Third place match
                        if (wc.HasConsolationMatch)
                        {
                            y1 = y2 + (gap / 2.0);
                            y2 = y1 + (gap / 4.0);

                            gfx.DrawLine(XPens.Black, respectiveStartX, y1, hLineEndPoint, y1 + lineThickness);
                            gfx.DrawLine(XPens.Black, respectiveStartX, y2, hLineEndPoint, y2 + lineThickness);
                            gfx.DrawLine(XPens.Black, hLineEndPoint, (y1 + y2) / 2.0, hLineEndPoint + (lineWidth / 2.0), (y1 + y2) / 2.0 + lineThickness);
                            gfx.DrawLine(XPens.Black, hLineEndPoint, y1, hLineEndPoint, y2);

                            Match _m = wc.ConsolationMatch;
                            if (_m.MatchCompNumber != 0)
                            {
                                gfx.DrawString(
                                    $"{_m.MatchCompNumber}",
                                    SecondaryFont,
                                    XBrushes.Black,
                                    new XRect(hLineEndPoint - 14, ((y1 + y2) / 2.0) - 3, 12, 12),
                                    XStringFormats.Center
                                );
                            }
                        }
                    }
                }
            }
        }

        public static void CreateMatchControl(XGraphics gfx, double x, double y, Match match, int width, int height, double margin)
        {
            CreateBracketControl(match.Pair[0], x, y, match.Winner == match.Pair[0], gfx, width, height);
            CreateBracketControl(match.Pair[1], x, y + height + margin, match.Winner == match.Pair[1], gfx, width, height);

            gfx.DrawString(
                $"{match.MatchCompNumber}",
                SecondaryFont,
                XBrushes.Black,
                new XRect(x + width + (width / 2.0), y, 12, 12),
                XStringFormats.Center
            );
        }

        public static void CreateBracketControl(Participant p1, double x, double y, bool isWinner, XGraphics gfx, int width, int height)
        {
            var pen = isWinner ? XPens.LightGreen : XPens.Black;
            gfx.DrawRectangle(pen, x, y, width, height);

            string nameText = p1 != null ? $"[{p1.CompId}] {p1.Name} {p1.Surname}" : "BYE";
            string detailsText = p1 != null ? $"{p1.Country} / {p1.Age}y.o. / {p1.Weight}kg" : "BYE";

            gfx.DrawString(nameText, PrimaryFont, XBrushes.Black, new XRect(x, y, width, height / 2.0), XStringFormats.Center);
            gfx.DrawString(detailsText, SecondaryFont, XBrushes.Black, new XRect(x, y + (height / 2.0), width, height / 2.0), XStringFormats.Center);
        }
    }
}