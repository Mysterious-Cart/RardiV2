namespace Payment.Services;

using Microsoft.EntityFrameworkCore;
using Payment.Data;
using QuestPDF.Companion;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public class ReportService(IDbContextFactory<PaymentContext> contextFactory)
{
    private readonly PaymentContext paymentContext = contextFactory.CreateDbContext();
    public async Task ShowReportFormatOverviewAsync()
    {
        var transactions = paymentContext.Transactions;
        var reports =  await (from f in transactions.Include(t => t.Customer)
                        where f.IsDeleted == false 
                        where f.CreatedAt.Year == DateTime.Now.Year
                        orderby f.CreatedAt.Month ascending
                        group f by new { f.CreatedAt.Month } into monthGroup
                        select new
                        {
                            Month = monthGroup.Key.Month,
                            Transactions = monthGroup.ToList()
                        }).ToListAsync();
                        



        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header()
                    .Height(30)
                    .Text("Yearly Reports")
                    .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                page.Content()
                    .Table(table =>
                    {
                        var contentfontsize = 10;
                        var columnheaderfontsize = 12;
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                        });

                        

                        foreach (var i in reports)
                        {
                            // Add rows
                            table.Cell().Table(innerTable =>
                            {
                                innerTable.Header(header =>
                                {
                                    header.Cell().BorderBottom(2).Padding(5).Text($"Month: {i.Month}").FontSize(columnheaderfontsize);
                                    header.Cell().BorderBottom(2).Padding(5).Text("Plate Number").FontSize(columnheaderfontsize);
                                    header.Cell().BorderBottom(2).Padding(5).Text("Total").FontSize(columnheaderfontsize);
                                });
                                innerTable.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(90);
                                    columns.RelativeColumn();
                                    columns.ConstantColumn(60);
                                });
                                innerTable.Footer(footer =>
                                {
                                    footer.Cell().ColumnSpan(3).BorderTop(2).Padding(5).AlignRight().Text($"Month: {i.Month}, Total: {i.Transactions.Sum(t => t.Total)}").FontSize(columnheaderfontsize);
                                });


                                foreach (var value in i.Transactions)
                                {
                                    innerTable.Cell().Padding(5).Text(value.CreatedAt.ToString("dd/MM/yyyy")).FontSize(contentfontsize);
                                    innerTable.Cell().Padding(5).Text(value.Customer.PlateNumber).FontSize(contentfontsize);
                                    innerTable.Cell().Padding(5).Text(value.Total.ToString()).FontSize(contentfontsize);
                                }

                            });
                        }
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                    });
            });
        })
        .GeneratePdfAndShow();
    }
}