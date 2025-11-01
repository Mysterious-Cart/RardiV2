namespace Payment.Graphql;

public class Mutation
{
    public async Task<MutationResult> ImportTransactionFromCsv(string filepath, [Service] Payment.Services.DataManagementService dataManagementService)
    {
        await dataManagementService.ImportTransactionFromCsv(filepath);
        return new MutationResult { Success = true };
    }
    public async Task<MutationResult> ShowReportFormatOverviewAsync([Service] Payment.Services.ReportService reportService)
    {
        await reportService.ShowReportFormatOverviewAsync();
        return new MutationResult { Success = true };
    }

}

public class MutationResult
{
    public bool Success { get; set; }
}