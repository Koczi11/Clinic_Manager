using System.Globalization;
using Clinic_Manager.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Clinic_Manager.Reports;

public class UpcomingVisitsReportDocument : IDocument
{
    private readonly List<Visit> _visits;
    private readonly DateTime _reportDate;
    private static readonly CultureInfo Culture = new("pl-PL");

    public UpcomingVisitsReportDocument(List<Visit> visits, DateTime reportDate)
    {
        _visits = visits;
        _reportDate = reportDate;
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
                .FontSize(16).Bold().FontColor(Colors.Pink.Darken2);

            column.Item().Text("Raport: Harmonogram wizyt na kolejny dzień")
                .FontSize(13).SemiBold();

            column.Item().PaddingTop(4).Text($"Dla dnia: {_reportDate:yyyy-MM-dd (dddd)}")
                .FontSize(10).FontColor(Colors.Grey.Darken2);

            column.Item().Text($"Wygenerowano: {DateTime.Now:yyyy-MM-dd HH:mm}")
                .FontSize(9).FontColor(Colors.Grey.Darken1);

            column.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Pink.Lighten3);
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.PaddingVertical(10).Column(column =>
        {
            if (_visits.Count == 0)
            {
                column.Item().PaddingTop(20).Text("Brak zaplanowanych wizyt na ten dzień.")
                    .Italic().FontColor(Colors.Grey.Darken1);
                return;
            }

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(60);  // Godzina
                    columns.RelativeColumn(3);   // Pacjent
                    columns.RelativeColumn(3);   // Lekarz
                    columns.RelativeColumn(4);   // Opis / Cel
                });

                table.Header(header =>
                {
                    header.Cell().Element(HeaderCell).Text("Godzina");
                    header.Cell().Element(HeaderCell).Text("Pacjent");
                    header.Cell().Element(HeaderCell).Text("Lekarz");
                    header.Cell().Element(HeaderCell).Text("Opis / Cel wizyty");
                });

                foreach (var visit in _visits.OrderBy(v => v.VisitDate))
                {
                    table.Cell().Element(BodyCell).Text(visit.VisitDate.ToString("HH:mm"));
                    table.Cell().Element(BodyCell).Text($"{visit.Patient.LastName} {visit.Patient.FirstName} \n(PESEL: {visit.Patient.Pesel})");
                    table.Cell().Element(BodyCell).Text(visit.Doctor.UserName ?? visit.Doctor.Email ?? "Lekarz");
                    table.Cell().Element(BodyCell).Text(visit.Description ?? "—");
                }
            });

            column.Item().PaddingTop(12).AlignRight().Column(summary =>
            {
                summary.Item().Text($"Łączna liczba zaplanowanych wizyt: {_visits.Count}")
                    .FontSize(10).Bold().FontColor(Colors.Pink.Darken2);
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
        container.Background(Colors.Pink.Lighten5).PaddingVertical(5).PaddingHorizontal(4)
            .BorderBottom(1).BorderColor(Colors.Pink.Lighten3);

    private static IContainer BodyCell(IContainer container) =>
        container.PaddingVertical(6).PaddingHorizontal(4)
            .BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
}
