using CleanArchitecture.Core.DTOs.Transcript;
using QuestPDF.Infrastructure;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using System;

namespace CleanArchitecture.Core.Helpers
{
    public static class TranscriptPdfGenerator
    {
        public static byte[] Generate(TranscriptResponse data)
        {
            // QuestPDF Lisans Ayarı (Community/Open Source için zorunlu)
            QuestPDF.Settings.License = LicenseType.Community;

            var primaryColor = "#FF6B00"; // UniNexus Orange
            var secondaryColor = "#1E3A8A"; // Dark Blue
            var lightGray = "#F9FAFB";
            var darkText = "#1F2937";
            var mutedText = "#6B7280";

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(0);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontColor(darkText).FontFamily(Fonts.Arial));

                    // Header
                    page.Header().Background(Colors.White).Padding(30).BorderBottom(3).BorderColor(primaryColor).Row(row =>
                    {
                        try 
                        {
                            row.ConstantItem(160).AlignMiddle().Image(@"G:\UniNexus\Architecture\uninexus-frontend\public\admin_logo.png");
                        }
                        catch
                        {
                            row.ConstantItem(160).AlignMiddle().Text("UniNexus").FontSize(32).FontColor(secondaryColor).SemiBold();
                        }

                        row.RelativeItem().PaddingLeft(20).AlignMiddle().Column(col =>
                        {
                            col.Item().Text("CO-CURRICULAR TRANSCRIPT").FontSize(16).FontColor(secondaryColor).Bold();
                            col.Item().Text("Official Student Record").FontSize(10).FontColor(mutedText);
                        });

                        row.ConstantItem(100).AlignRight().AlignMiddle().Text("OFFICIAL\nDOCUMENT")
                            .FontSize(10).FontColor(primaryColor).LineHeight(1.2f).SemiBold();
                    });

                    // Content
                    page.Content().Padding(40).Column(col =>
                    {
                        // Student Info Card
                        col.Item().Background(lightGray).Border(1).BorderColor("#E5E7EB").Padding(20).Row(row =>
                        {
                            row.RelativeItem().Column(infoCol =>
                            {
                                infoCol.Item().Text("STUDENT DETAILS").FontSize(10).FontColor(mutedText).Bold();
                                infoCol.Item().PaddingTop(5).Text(data.StudentName).FontSize(18).SemiBold().FontColor(secondaryColor);
                                infoCol.Item().PaddingTop(2).Text($"Student ID: {data.StudentNumber}").FontSize(12).FontColor(darkText);
                            });

                            row.ConstantItem(150).AlignRight().Column(scoreCol =>
                            {
                                scoreCol.Item().AlignRight().Text("TOTAL SCORE").FontSize(10).FontColor(mutedText).Bold();
                                scoreCol.Item().PaddingTop(2).AlignRight().Text($"{data.TotalPoints} NC").FontSize(24).Bold().FontColor(primaryColor);
                            });
                        });

                        col.Item().PaddingTop(30).Text("ACTIVITY HISTORY").FontSize(14).FontColor(secondaryColor).Bold();
                        
                        col.Item().PaddingTop(10).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(4);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(1);
                            });

                            // Table Header
                            table.Header(header =>
                            {
                                header.Cell().Background(secondaryColor).Padding(8).Text("Category").FontColor(Colors.White).SemiBold();
                                header.Cell().Background(secondaryColor).Padding(8).Text("Activity").FontColor(Colors.White).SemiBold();
                                header.Cell().Background(secondaryColor).Padding(8).AlignCenter().Text("Date").FontColor(Colors.White).SemiBold();
                                header.Cell().Background(secondaryColor).Padding(8).AlignRight().Text("Points").FontColor(Colors.White).SemiBold();
                            });

                            // Table Body
                            var rowIndex = 0;
                            foreach (var item in data.Activities)
                            {
                                var bgColor = rowIndex % 2 == 0 ? "#FFFFFF" : lightGray;
                                var borderColor = "#E5E7EB";

                                var categoryColor = item.Category switch
                                {
                                    "Club" => "#7C3AED",
                                    "Membership" => "#0891B2",
                                    "Promotion" => "#059669",
                                    _ => secondaryColor
                                };

                                table.Cell().BorderBottom(1).BorderColor(borderColor).Background(bgColor).Padding(8).AlignMiddle()
                                    .Text(item.Category ?? "Event").FontSize(9).Bold().FontColor(categoryColor);
                                table.Cell().BorderBottom(1).BorderColor(borderColor).Background(bgColor).Padding(8).AlignMiddle()
                                    .Text(item.EventName).FontSize(10);
                                table.Cell().BorderBottom(1).BorderColor(borderColor).Background(bgColor).Padding(8).AlignMiddle().AlignCenter()
                                    .Text(item.Date).FontSize(10);
                                table.Cell().BorderBottom(1).BorderColor(borderColor).Background(bgColor).Padding(8).AlignMiddle().AlignRight()
                                    .Text(item.Points > 0 ? $"+{item.Points}" : "—").FontSize(10).Bold().FontColor(item.Points > 0 ? primaryColor : mutedText);

                                rowIndex++;
                            }
                        });

                        // Signatures & Formalities
                        col.Item().PaddingTop(60).Row(row =>
                        {
                            row.RelativeItem().Column(signCol =>
                            {
                                signCol.Item().LineHorizontal(1).LineColor("#D1D5DB");
                                signCol.Item().PaddingTop(5).AlignCenter().Text("Student Sign").FontSize(10).FontColor(mutedText);
                            });

                            row.ConstantItem(50); // spacer

                            row.RelativeItem().Column(signCol =>
                            {
                                signCol.Item().LineHorizontal(1).LineColor("#D1D5DB");
                                signCol.Item().PaddingTop(5).AlignCenter().Text("SKS Department Approval").FontSize(10).FontColor(mutedText);
                                signCol.Item().AlignCenter().Text("Health, Culture and Sports Dept.").FontSize(8).FontColor(mutedText);
                            });
                        });
                    });

                    // Footer
                    page.Footer().Background("#111827").PaddingVertical(10).PaddingHorizontal(40).Row(row =>
                    {
                        row.RelativeItem().Text($"Generated on: {DateTime.UtcNow:dd MMM yyyy HH:mm} UTC").FontSize(8).FontColor("#9CA3AF");
                        row.RelativeItem().AlignRight().Text(x =>
                        {
                            x.Span("Page ").FontSize(8).FontColor("#9CA3AF");
                            x.CurrentPageNumber().FontSize(8).FontColor("#9CA3AF");
                            x.Span(" of ").FontSize(8).FontColor("#9CA3AF");
                            x.TotalPages().FontSize(8).FontColor("#9CA3AF");
                        });
                    });
                });
            }).GeneratePdf();
        }
    }
}
