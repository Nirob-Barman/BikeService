using BikeService.Application.DTOs.Invoice;
using BikeService.Application.Interfaces;
using BikeService.Domain.Enums;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BikeService.Infrastructure.Services
{
    public class InvoicePdfService : IPdfService
    {
        public byte[] GenerateInvoicePdf(InvoiceDto invoice)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.5f, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    page.Header().Element(header => ComposeHeader(header, invoice));
                    page.Content().PaddingTop(10).Element(content => ComposeContent(content, invoice));
                    page.Footer().AlignCenter().PaddingTop(10).Text(x =>
                    {
                        x.Span("Thank you for choosing BikeService  •  ").FontSize(8).FontColor(Colors.Grey.Medium);
                        x.Span("Page ").FontSize(8).FontColor(Colors.Grey.Medium);
                        x.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Medium);
                        x.Span(" of ").FontSize(8).FontColor(Colors.Grey.Medium);
                        x.TotalPages().FontSize(8).FontColor(Colors.Grey.Medium);
                    });
                });
            }).GeneratePdf();
        }

        private static void ComposeHeader(IContainer container, InvoiceDto invoice)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("BikeService").FontSize(22).Bold().FontColor(Colors.Blue.Darken2);
                    col.Item().Text("Professional Bike Repair & Maintenance").FontSize(9).FontColor(Colors.Grey.Medium);
                });

                row.ConstantItem(160).Column(col =>
                {
                    col.Item().AlignRight().Text($"INVOICE #{invoice.Id}").FontSize(16).Bold();
                    col.Item().AlignRight().Text(invoice.CreatedAt.ToString("MMMM d, yyyy")).FontSize(9).FontColor(Colors.Grey.Medium);
                    col.Item().PaddingTop(4).AlignRight().Element(e => StatusBadge(e, invoice.Status));
                });
            });
        }

        private static void StatusBadge(IContainer container, InvoiceStatus status)
        {
            var (label, color) = status switch
            {
                InvoiceStatus.Draft   => ("DRAFT",   Colors.Grey.Medium),
                InvoiceStatus.Issued  => ("ISSUED",  Colors.Orange.Medium),
                InvoiceStatus.Paid    => ("PAID",    Colors.Green.Darken1),
                InvoiceStatus.Void    => ("VOID",    Colors.Red.Medium),
                _                    => (status.ToString().ToUpper(), Colors.Grey.Medium)
            };

            container.Background(color).Padding(4).Text(label)
                .FontSize(9).Bold().FontColor(Colors.White);
        }

        private static void ComposeContent(IContainer container, InvoiceDto invoice)
        {
            container.Column(col =>
            {
                // Divider
                col.Item().PaddingVertical(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                // Bike info
                col.Item().Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("BILLED FOR").FontSize(8).Bold().FontColor(Colors.Grey.Medium);
                        c.Item().PaddingTop(2).Text(invoice.BikeSummary).Bold();
                        if (!string.IsNullOrWhiteSpace(invoice.CustomerName))
                            c.Item().Text(invoice.CustomerName).FontColor(Colors.Grey.Darken1);
                    });

                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("INVOICE DETAILS").FontSize(8).Bold().FontColor(Colors.Grey.Medium);
                        c.Item().PaddingTop(2).Row(r =>
                        {
                            r.ConstantItem(80).Text("Invoice No:").FontColor(Colors.Grey.Darken1);
                            r.RelativeItem().Text($"#{invoice.Id}").Bold();
                        });
                        c.Item().Row(r =>
                        {
                            r.ConstantItem(80).Text("Date:").FontColor(Colors.Grey.Darken1);
                            r.RelativeItem().Text(invoice.CreatedAt.ToString("MMM d, yyyy"));
                        });
                        if (!string.IsNullOrEmpty(invoice.PromoCode))
                        {
                            c.Item().Row(r =>
                            {
                                r.ConstantItem(80).Text("Promo Code:").FontColor(Colors.Grey.Darken1);
                                r.RelativeItem().Text(invoice.PromoCode).FontColor(Colors.Green.Darken1);
                            });
                        }
                    });
                });

                col.Item().PaddingVertical(12).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                // Items table
                col.Item().Text("SERVICE ITEMS").FontSize(8).Bold().FontColor(Colors.Grey.Medium);
                col.Item().PaddingTop(6).Element(e => ComposeItemsTable(e, invoice));

                col.Item().PaddingVertical(12).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                // Totals
                col.Item().AlignRight().Element(e => ComposeTotals(e, invoice));
            });
        }

        private static void ComposeItemsTable(IContainer container, InvoiceDto invoice)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(cols =>
                {
                    cols.RelativeColumn(4);
                    cols.RelativeColumn(1);
                    cols.RelativeColumn(1.5f);
                    cols.RelativeColumn(1.5f);
                });

                // Header
                table.Header(header =>
                {
                    header.Cell().Background(Colors.Blue.Darken2).Padding(6)
                        .Text("Description").FontColor(Colors.White).Bold().FontSize(9);
                    header.Cell().Background(Colors.Blue.Darken2).Padding(6).AlignCenter()
                        .Text("Qty").FontColor(Colors.White).Bold().FontSize(9);
                    header.Cell().Background(Colors.Blue.Darken2).Padding(6).AlignRight()
                        .Text("Unit Price").FontColor(Colors.White).Bold().FontSize(9);
                    header.Cell().Background(Colors.Blue.Darken2).Padding(6).AlignRight()
                        .Text("Total").FontColor(Colors.White).Bold().FontSize(9);
                });

                // Rows
                if (!invoice.Items.Any())
                {
                    table.Cell().ColumnSpan(4).Padding(8).AlignCenter()
                        .Text("No items on this invoice.").FontColor(Colors.Grey.Medium).Italic();
                }
                else
                {
                    var rowIndex = 0;
                    foreach (var item in invoice.Items)
                    {
                        var bg = rowIndex % 2 == 0 ? Colors.White : Colors.Grey.Lighten5;
                        var description = item.ServiceTypeName ?? item.PartName ?? "—";

                        table.Cell().Background(bg).Padding(6).Text(description);
                        table.Cell().Background(bg).Padding(6).AlignCenter().Text(item.Quantity.ToString());
                        table.Cell().Background(bg).Padding(6).AlignRight().Text($"BDT {item.UnitPrice:N2}");
                        table.Cell().Background(bg).Padding(6).AlignRight().Text($"BDT {item.LineTotal:N2}");
                        rowIndex++;
                    }
                }
            });
        }

        private static void ComposeTotals(IContainer container, InvoiceDto invoice)
        {
            container.Width(220).Column(col =>
            {
                TotalRow(col, "Subtotal", invoice.TotalAmount);
                TotalRow(col, "Tax (15%)", invoice.TaxAmount);

                if (invoice.DiscountAmount > 0)
                    TotalRow(col, $"Discount ({invoice.PromoCode})", -invoice.DiscountAmount, Colors.Green.Darken1);

                col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                col.Item().PaddingTop(4).Row(row =>
                {
                    row.RelativeItem().Text("TOTAL DUE").Bold().FontSize(11);
                    row.ConstantItem(100).AlignRight().Text($"BDT {invoice.FinalAmount:N2}").Bold().FontSize(11).FontColor(Colors.Blue.Darken2);
                });
            });
        }

        private static void TotalRow(ColumnDescriptor col, string label, decimal amount, string? color = null)
        {
            col.Item().PaddingVertical(2).Row(row =>
            {
                row.RelativeItem().Text(label).FontColor(Colors.Grey.Darken1);
                var text = row.ConstantItem(100).AlignRight().Text($"BDT {amount:N2}");
                if (color != null) text.FontColor(color);
            });
        }
    }
}
