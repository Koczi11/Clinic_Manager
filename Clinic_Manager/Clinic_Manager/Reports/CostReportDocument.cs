using System.Globalization;
using Clinic_Manager.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Clinic_Manager.Reports;

public class CostReportDocument : IDocument
{
    private readonly CostReportData _data;
    private static readonly CultureInfo Culture = new("pl-PL");

    public CostReportDocument(CostReportData data)
    {
        _data = data;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(40);
            page.DefaultTextStyle(x => x.FontSize(10));

            page.Header().Element(ComposeHeader);
            page.Content().Element(ComposeContent);
            page.Footer().Element(ComposeFooter);
        });
    }

    private void ComposeHeader(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().Text("Przychodnia Medyczna Clinic Manager")
                .FontSize(16).Bold().FontColor(Colors.Blue.Darken2);

            column.Item().Text("Raport kosztów świadczeń")
                .FontSize(13).SemiBold();

            column.Item().PaddingTop(4).Text(_data.FilterDescription)
                .FontSize(9).FontColor(Colors.Grey.Darken1);

            column.Item().Text($"Wygenerowano: {_data.GeneratedAt:yyyy-MM-dd HH:mm}")
                .FontSize(9).FontColor(Colors.Grey.Darken1);

            column.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.PaddingVertical(10).Column(column =>
        {
            if (_data.Rows.Count == 0)
            {
                column.Item().PaddingTop(20).Text("Brak zakończonych wizyt spełniających kryteria raportu.")
                    .Italic().FontColor(Colors.Grey.Darken1);
                return;
            }

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(80);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                    columns.ConstantColumn(70);
                    columns.ConstantColumn(70);
                    columns.ConstantColumn(75);
                });

                table.Header(header =>
                {
                    header.Cell().Element(HeaderCell).Text("Data");
                    header.Cell().Element(HeaderCell).Text("Pacjent");
                    header.Cell().Element(HeaderCell).Text("Lekarz");
                    header.Cell().Element(HeaderCell).AlignRight().Text("Procedury");
                    header.Cell().Element(HeaderCell).AlignRight().Text("Leki");
                    header.Cell().Element(HeaderCell).AlignRight().Text("Razem");
                });

                foreach (var row in _data.Rows)
                {
                    table.Cell().Element(BodyCell).Text(row.VisitDate.ToString("yyyy-MM-dd"));
                    table.Cell().Element(BodyCell).Text(row.PatientName);
                    table.Cell().Element(BodyCell).Text(row.DoctorName);
                    table.Cell().Element(BodyCell).AlignRight().Text(Money(row.ProceduresCost));
                    table.Cell().Element(BodyCell).AlignRight().Text(Money(row.MedicationsCost));
                    table.Cell().Element(BodyCell).AlignRight().Text(Money(row.TotalCost));
                }
            });

            column.Item().PaddingTop(12).AlignRight().Column(summary =>
            {
                summary.Item().Text($"Liczba wizyt: {_data.Rows.Count}")
                    .FontSize(10);
                summary.Item().Text($"Suma kosztów: {Money(_data.GrandTotal)}")
                    .FontSize(13).Bold().FontColor(Colors.Blue.Darken2);
            });
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.AlignCenter().Text(text =>
        {
            text.Span("Strona ").FontSize(8).FontColor(Colors.Grey.Darken1);
            text.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Darken1);
            text.Span(" z ").FontSize(8).FontColor(Colors.Grey.Darken1);
            text.TotalPages().FontSize(8).FontColor(Colors.Grey.Darken1);
        });
    }

    private static IContainer HeaderCell(IContainer container) =>
        container.Background(Colors.Grey.Lighten3).PaddingVertical(5).PaddingHorizontal(4)
            .BorderBottom(1).BorderColor(Colors.Grey.Medium);

    private static IContainer BodyCell(IContainer container) =>
        container.PaddingVertical(4).PaddingHorizontal(4)
            .BorderBottom(1).BorderColor(Colors.Grey.Lighten2);

    private static string Money(decimal value) => value.ToString("0.00", Culture) + " zł";
}
