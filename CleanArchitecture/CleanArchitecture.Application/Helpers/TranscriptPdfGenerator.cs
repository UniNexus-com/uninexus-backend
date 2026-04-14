using CleanArchitecture.Core.DTOs.Transcript;
using QuestPDF.Infrastructure;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Helpers
{
    public static class TranscriptPdfGenerator
    {
        public static byte[] Generate(TranscriptResponse data)
        {
            // QuestPDF Lisans Ayarı (Community/Open Source için zorunlu)
            QuestPDF.Settings.License = LicenseType.Community;

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(50);
                    page.Header().Text("UniNexus - Co-Curricular Transcript").FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);

                    page.Content().PaddingVertical(20).Column(col =>
                    {
                        col.Item().Text($"Öğrenci: {data.StudentName}").FontSize(14);
                        col.Item().Text($"Öğrenci Numarası: {data.StudentNumber}").FontSize(14);
                        col.Item().Text($"Toplam Puan: {data.TotalPoints}").FontSize(14).Bold();

                        col.Item().PaddingTop(20).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Etkinlik Adı").Bold();
                                header.Cell().Text("Tarih").Bold();
                                header.Cell().Text("Puan").Bold();
                            });

                            foreach (var item in data.Activities)
                            {
                                table.Cell().Text(item.EventName);
                                table.Cell().Text(item.Date);
                                table.Cell().Text(item.Points.ToString());
                            }
                        });
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Sayfa ");
                        x.CurrentPageNumber();
                    });
                });
            }).GeneratePdf();
        }
    }
}
