using System.Net;
using System.Net.Mail;
using Clinic_Manager.Data;
using Clinic_Manager.Models;
using Clinic_Manager.Reports;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;

namespace Clinic_Manager.Services;

public class UpcomingVisitsReportBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<UpcomingVisitsReportBackgroundService> _logger;

    public UpcomingVisitsReportBackgroundService(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<UpcomingVisitsReportBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("UpcomingVisitsReportBackgroundService uruchamia się...");

        // Pobranie konfiguracji interwału (w minutach, domyślnie 24 godziny)
        var settings = GetSmtpSettings();
        var intervalMinutes = settings.IntervalMinutes > 0 ? settings.IntervalMinutes : 1440;

        // Krótkie opóźnienie na start aplikacji (5 sekund), aby baza danych / seeder zakończyły inicjalizację
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Uruchamianie generowania raportu nadchodzących wizyt...");
                await GenerateAndSendReportAsync(stoppingToken);
                _logger.LogInformation("Raport nadchodzących wizyt został pomyślnie wygenerowany i wysłany.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Wystąpił błąd podczas generowania lub wysyłania raportu nadchodzących wizyt.");
            }

            // Oczekiwanie na kolejny interwał
            _logger.LogInformation("Oczekiwanie na kolejny interwał: {Interval} minut.", intervalMinutes);
            await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), stoppingToken);
        }
    }

    private SmtpSettings GetSmtpSettings()
    {
        var settings = new SmtpSettings();
        _configuration.GetSection("SmtpSettings").Bind(settings);
        return settings;
    }

    public async Task GenerateAndSendReportAsync(CancellationToken stoppingToken)
    {
        var settings = GetSmtpSettings();
        var tomorrow = DateTime.Today.AddDays(1);
        var tomorrowEnd = tomorrow.AddDays(1).AddSeconds(-1);

        List<Visit> visits;

        using (var scope = _scopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            visits = await context.Visits
                .Include(v => v.Patient)
                .Include(v => v.Doctor)
                .Where(v => v.Status == VisitStatus.Scheduled && v.VisitDate >= tomorrow && v.VisitDate <= tomorrowEnd)
                .OrderBy(v => v.VisitDate)
                .ToListAsync(stoppingToken);
        }

        // Generowanie PDF
        var document = new UpcomingVisitsReportDocument(visits, tomorrow);
        var pdfBytes = document.GeneratePdf();

        // Zapis testowego pliku na dysku (w głównym folderze projektu)
        var testFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "raport-nadchodzace-wizyty.pdf");
        // Dla wygody użytkownika zapiszmy też w folderze roboczym (projektu)
        var projectRootPath = Path.Combine(Directory.GetCurrentDirectory(), "raport-nadchodzace-wizyty.pdf");
        
        try
        {
            await File.WriteAllBytesAsync(projectRootPath, pdfBytes, stoppingToken);
            _logger.LogInformation("Testowy plik raportu został zapisany pod ścieżką: {Path}", projectRootPath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Nie udało się zapisać pliku w głównym katalogu roboczym. Próba zapisu w BaseDirectory.");
            await File.WriteAllBytesAsync(testFilePath, pdfBytes, stoppingToken);
            _logger.LogInformation("Testowy plik raportu został zapisany pod ścieżką: {Path}", testFilePath);
        }

        // Wysyłanie wiadomości e-mail
        if (string.IsNullOrWhiteSpace(settings.Host) || string.IsNullOrWhiteSpace(settings.RecipientEmail))
        {
            _logger.LogWarning("Ustawienia SMTP lub adres odbiorcy są niepełne. Pomijanie wysyłki e-mail.");
            return;
        }

        using var mail = new MailMessage();
        mail.From = new MailAddress(settings.SenderEmail, settings.SenderName);
        mail.To.Add(settings.RecipientEmail);
        mail.Subject = $"Raport: Nadchodzące wizyty na dzień {tomorrow:yyyy-MM-dd}";
        mail.Body = $"Dzień dobry,\n\nw załączniku znajduje się raport z zestawieniem wizyt zaplanowanych na kolejny dzień ({tomorrow:yyyy-MM-dd}).\n\nRaport zawiera {_visitsCountText(visits.Count)}.\n\nZ poważaniem,\nSystem Clinic Manager";
        
        using var pdfStream = new MemoryStream(pdfBytes);
        mail.Attachments.Add(new Attachment(pdfStream, "upcoming_visits.pdf", "application/pdf"));

        using var smtp = new SmtpClient(settings.Host, settings.Port);
        if (!string.IsNullOrWhiteSpace(settings.Username) && !string.IsNullOrWhiteSpace(settings.Password))
        {
            smtp.Credentials = new NetworkCredential(settings.Username, settings.Password);
        }
        smtp.EnableSsl = true;

        await smtp.SendMailAsync(mail, stoppingToken);
    }

    private static string _visitsCountText(int count)
    {
        if (count == 0) return "brak zaplanowanych wizyt";
        if (count == 1) return "1 zaplanowaną wizytę";
        if (count > 1 && count < 5) return $"{count} zaplanowane wizyty";
        return $"{count} zaplanowanych wizyt";
    }
}
