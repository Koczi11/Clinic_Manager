using Clinic_Manager.DTOs;

namespace Clinic_Manager.Services;

public interface IReportService
{
    Task<CostReportData> GetCostReportDataAsync(CostReportFilter filter);

    Task<byte[]> GenerateCostReportPdfAsync(CostReportFilter filter);
}
