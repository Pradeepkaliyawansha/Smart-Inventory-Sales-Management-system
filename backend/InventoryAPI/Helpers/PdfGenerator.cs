using iTextSharp.text;
using iTextSharp.text.pdf;
using InventoryAPI.Models.DTOs;
using System.Text;

namespace InventoryAPI.Helpers
{
    public class PdfGenerator
    {
        public byte[] GenerateInvoicePdf(InvoiceDto invoice)
        {
            using var memoryStream = new MemoryStream();
            var document = new Document(PageSize.A4, 25, 25, 30, 30);
            var writer = PdfWriter.GetInstance(document, memoryStream);
            
            document.Open();

            // Company Header
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, BaseColor.BLACK);
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.BLACK);
            var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK);

            var title = new Paragraph("INVENTORY MANAGEMENT SYSTEM", titleFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 10f
            };
            document.Add(title);

            var invoiceTitle = new Paragraph($"INVOICE: {invoice.InvoiceNumber}", headerFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 20f
            };
            document.Add(invoiceTitle);

            // Invoice Details Table
            var detailsTable = new PdfPTable(2) { WidthPercentage = 100 };
            detailsTable.SetWidths(new float[] { 1f, 1f });

            // Left side - Customer Details
            var customerCell = new PdfPCell();
            customerCell.Border = Rectangle.NO_BORDER;
            customerCell.AddElement(new Paragraph("BILL TO:", headerFont));
            customerCell.AddElement(new Paragraph($"Customer: {invoice.Customer?.Name ?? "N/A"}", normalFont));
            customerCell.AddElement(new Paragraph($"Email: {invoice.Customer?.Email ?? "N/A"}", normalFont));
            customerCell.AddElement(new Paragraph($"Phone: {invoice.Customer?.Phone ?? "N/A"}", normalFont));
            customerCell.AddElement(new Paragraph($"Address: {invoice.Customer?.Address ?? "N/A"}", normalFont));

            // Right side - Invoice Details
            var invoiceCell = new PdfPCell();
            invoiceCell.Border = Rectangle.NO_BORDER;
            invoiceCell.AddElement(new Paragraph("INVOICE DETAILS:", headerFont));
            invoiceCell.AddElement(new Paragraph($"Date: {invoice.SaleDate:dd/MM/yyyy}", normalFont));
            invoiceCell.AddElement(new Paragraph($"Sales Person: {invoice.SalesPerson?.FullName ?? "N/A"}", normalFont));
            invoiceCell.AddElement(new Paragraph($"Payment Method: {invoice.PaymentMethod}", normalFont));

            detailsTable.AddCell(customerCell);
            detailsTable.AddCell(invoiceCell);
            detailsTable.SpacingAfter = 20f;
            document.Add(detailsTable);

            // Items Table
            var itemsTable = new PdfPTable(5) { WidthPercentage = 100 };
            itemsTable.SetWidths(new float[] { 3f, 1f, 1.5f, 1f, 1.5f });

            // Table Headers
            var headerCells = new string[] { "Product", "Qty", "Unit Price", "Discount", "Total" };
            foreach (var header in headerCells)
            {
                var cell = new PdfPCell(new Phrase(header, headerFont))
                {
                    BackgroundColor = BaseColor.LIGHT_GRAY,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Padding = 5
                };
                itemsTable.AddCell(cell);
            }

            // Table Rows
            foreach (var item in invoice.SaleItems)
            {
                itemsTable.AddCell(new PdfPCell(new Phrase(item.ProductName, normalFont)) { Padding = 5 });
                itemsTable.AddCell(new PdfPCell(new Phrase(item.Quantity.ToString(), normalFont)) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 5 });
                itemsTable.AddCell(new PdfPCell(new Phrase($"${item.UnitPrice:F2}", normalFont)) { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 5 });
                itemsTable.AddCell(new PdfPCell(new Phrase($"{item.DiscountPercentage}%", normalFont)) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 5 });
                itemsTable.AddCell(new PdfPCell(new Phrase($"${item.TotalPrice:F2}", normalFont)) { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 5 });
            }

            itemsTable.SpacingAfter = 20f;
            document.Add(itemsTable);

            // Totals Table
            var totalsTable = new PdfPTable(2) { WidthPercentage = 50, HorizontalAlignment = Element.ALIGN_RIGHT };
            totalsTable.SetWidths(new float[] { 2f, 1f });

            var totalRows = new (string label, decimal amount)[]
            {
                ("Subtotal:", invoice.SubTotal),
                ("Discount:", -invoice.DiscountAmount),
                ("Tax:", invoice.TaxAmount),
                ("Total:", invoice.TotalAmount),
                ("Paid:", invoice.PaidAmount),
                ("Balance:", invoice.TotalAmount - invoice.PaidAmount)
            };

            foreach (var (label, amount) in totalRows)
            {
                var labelCell = new PdfPCell(new Phrase(label, label == "Total:" ? headerFont : normalFont))
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    Padding = 3
                };

                var amountCell = new PdfPCell(new Phrase($"${amount:F2}", label == "Total:" ? headerFont : normalFont))
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    Padding = 3
                };

                if (label == "Total:")
                {
                    labelCell.BackgroundColor = BaseColor.LIGHT_GRAY;
                    amountCell.BackgroundColor = BaseColor.LIGHT_GRAY;
                }

                totalsTable.AddCell(labelCell);
                totalsTable.AddCell(amountCell);
            }

            document.Add(totalsTable);

            // Notes
            if (!string.IsNullOrEmpty(invoice.Notes))
            {
                var notesTitle = new Paragraph("Notes:", headerFont) { SpacingBefore = 20f, SpacingAfter = 5f };
                document.Add(notesTitle);

                var notes = new Paragraph(invoice.Notes, normalFont);
                document.Add(notes);
            }

            // Footer
            var footer = new Paragraph("Thank you for your business!", normalFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingBefore = 30f
            };
            document.Add(footer);

            document.Close();
            return memoryStream.ToArray();
        }

        public byte[] GenerateSalesReportPdf(SalesReportDto report)
        {
            using var memoryStream = new MemoryStream();
            var document = new Document(PageSize.A4, 25, 25, 30, 30);
            var writer = PdfWriter.GetInstance(document, memoryStream);
            
            document.Open();

            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, BaseColor.BLACK);
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.BLACK);
            var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK);

            // Title
            var title = new Paragraph("SALES REPORT", titleFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 10f
            };
            document.Add(title);

            // Report Period
            var period = new Paragraph($"Period: {report.FromDate:dd/MM/yyyy} - {report.ToDate:dd/MM/yyyy}", headerFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 20f
            };
            document.Add(period);

            // Summary Section
            var summaryTitle = new Paragraph("SUMMARY", headerFont) { SpacingAfter = 10f };
            document.Add(summaryTitle);

            var summaryTable = new PdfPTable(2) { WidthPercentage = 60 };
            summaryTable.SetWidths(new float[] { 2f, 1f });

            var summaryData = new (string label, string value)[]
            {
                ("Total Sales:", $"${report.TotalSales:F2}"),
                ("Total Transactions:", report.TotalTransactions.ToString()),
                ("Average Transaction:", $"${report.AverageTransactionValue:F2}"),
                ("Total Discounts:", $"${report.TotalDiscounts:F2}"),
                ("Total Tax:", $"${report.TotalTax:F2}")
            };

            foreach (var (label, value) in summaryData)
            {
                summaryTable.AddCell(new PdfPCell(new Phrase(label, normalFont)) { Border = Rectangle.NO_BORDER, Padding = 3 });
                summaryTable.AddCell(new PdfPCell(new Phrase(value, normalFont)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 3 });
            }

            summaryTable.SpacingAfter = 20f;
            document.Add(summaryTable);

            // Top Selling Products
            if (report.TopSellingProducts.Any())
            {
                var productsTitle = new Paragraph("TOP SELLING PRODUCTS", headerFont) { SpacingAfter = 10f };
                document.Add(productsTitle);

                var productsTable = new PdfPTable(4) { WidthPercentage = 100 };
                productsTable.SetWidths(new float[] { 3f, 1f, 1.5f, 1.5f });

                // Headers
                var productHeaders = new string[] { "Product", "Qty Sold", "Unit Price", "Revenue" };
                foreach (var header in productHeaders)
                {
                    var cell = new PdfPCell(new Phrase(header, headerFont))
                    {
                        BackgroundColor = BaseColor.LIGHT_GRAY,
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        Padding = 5
                    };
                    productsTable.AddCell(cell);
                }

                // Data
                foreach (var product in report.TopSellingProducts.Take(10))
                {
                    productsTable.AddCell(new PdfPCell(new Phrase(product.ProductName, normalFont)) { Padding = 5 });
                    productsTable.AddCell(new PdfPCell(new Phrase(product.QuantitySold.ToString(), normalFont)) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 5 });
                    productsTable.AddCell(new PdfPCell(new Phrase($"${product.UnitPrice:F2}", normalFont)) { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 5 });
                    productsTable.AddCell(new PdfPCell(new Phrase($"${product.TotalRevenue:F2}", normalFont)) { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 5 });
                }

                productsTable.SpacingAfter = 20f;
                document.Add(productsTable);
            }

            document.Close();
            return memoryStream.ToArray();
        }

        public byte[] GenerateInventoryReportPdf(InventoryReportDto report)
        {
            using var memoryStream = new MemoryStream();
            var document = new Document(PageSize.A4, 25, 25, 30, 30);
            var writer = PdfWriter.GetInstance(document, memoryStream);
            
            document.Open();

            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, BaseColor.BLACK);
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.BLACK);
            var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK);

            // Title
            var title = new Paragraph("INVENTORY REPORT", titleFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 20f
            };
            document.Add(title);

            // Summary
            var summaryTitle = new Paragraph("INVENTORY SUMMARY", headerFont) { SpacingAfter = 10f };
            document.Add(summaryTitle);

            var summaryTable = new PdfPTable(2) { WidthPercentage = 60 };
            summaryTable.SetWidths(new float[] { 2f, 1f });

            var summaryData = new (string label, string value)[]
            {
                ("Total Products:", report.TotalProducts.ToString()),
                ("Total Inventory Value:", $"${report.TotalInventoryValue:F2}"),
                ("Low Stock Products:", report.LowStockProductsCount.ToString()),
                ("Out of Stock Products:", report.OutOfStockProductsCount.ToString())
            };

            foreach (var (label, value) in summaryData)
            {
                summaryTable.AddCell(new PdfPCell(new Phrase(label, normalFont)) { Border = Rectangle.NO_BORDER, Padding = 3 });
                summaryTable.AddCell(new PdfPCell(new Phrase(value, normalFont)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 3 });
            }

            summaryTable.SpacingAfter = 20f;
            document.Add(summaryTable);

            document.Close();
            return memoryStream.ToArray();
        }
    }
}