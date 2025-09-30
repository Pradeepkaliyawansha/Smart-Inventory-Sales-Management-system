using System.IO;
using System.Linq;
using System.Text;
using InventoryAPI.Models.DTOs;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Layout.Borders;

namespace InventoryAPI.Helpers
{
    public class PdfGenerator
    {
        public byte[] GenerateInvoicePdf(InvoiceDto invoice)
        {
            using var memoryStream = new MemoryStream();
            using var writer = new PdfWriter(memoryStream);
            using var pdf = new PdfDocument(writer);
            var document = new Document(pdf, iText.Kernel.Geom.PageSize.A4);
            document.SetMargins(30, 25, 30, 25);

            // Fonts
            var titleFont = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD);
            var normalFont = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);

            // Title
            document.Add(new Paragraph("INVENTORY MANAGEMENT SYSTEM")
                .SetFont(titleFont)
                .SetFontSize(18)
                .SetFontColor(ColorConstants.BLACK)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(10));

            document.Add(new Paragraph($"INVOICE: {invoice.InvoiceNumber}")
                .SetFont(titleFont)
                .SetFontSize(12)
                .SetFontColor(ColorConstants.BLACK)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(20));

            // Customer + Invoice details table
            var detailsTable = new Table(UnitValue.CreatePercentArray(new float[] { 1, 1 })).UseAllAvailableWidth();

            var customerCell = new Cell().SetBorder(Border.NO_BORDER);
            customerCell.Add(new Paragraph("BILL TO:").SetFont(titleFont).SetFontSize(12));
            customerCell.Add(new Paragraph($"Customer: {invoice.Customer?.Name ?? "N/A"}").SetFont(normalFont).SetFontSize(10));
            customerCell.Add(new Paragraph($"Email: {invoice.Customer?.Email ?? "N/A"}").SetFont(normalFont).SetFontSize(10));
            customerCell.Add(new Paragraph($"Phone: {invoice.Customer?.Phone ?? "N/A"}").SetFont(normalFont).SetFontSize(10));
            customerCell.Add(new Paragraph($"Address: {invoice.Customer?.Address ?? "N/A"}").SetFont(normalFont).SetFontSize(10));

            var invoiceCell = new Cell().SetBorder(Border.NO_BORDER);
            invoiceCell.Add(new Paragraph("INVOICE DETAILS:").SetFont(titleFont).SetFontSize(12));
            invoiceCell.Add(new Paragraph($"Date: {invoice.SaleDate:dd/MM/yyyy}").SetFont(normalFont).SetFontSize(10));
            invoiceCell.Add(new Paragraph($"Sales Person: {invoice.SalesPerson?.FullName ?? "N/A"}").SetFont(normalFont).SetFontSize(10));
            invoiceCell.Add(new Paragraph($"Payment Method: {invoice.PaymentMethod}").SetFont(normalFont).SetFontSize(10));

            detailsTable.AddCell(customerCell);
            detailsTable.AddCell(invoiceCell);

            document.Add(detailsTable.SetMarginBottom(20));

            // Items Table
            var itemsTable = new Table(UnitValue.CreatePercentArray(new float[] { 3, 1, 1.5f, 1, 1.5f })).UseAllAvailableWidth();

            string[] headers = { "Product", "Qty", "Unit Price", "Discount", "Total" };
            foreach (var h in headers)
            {
                itemsTable.AddHeaderCell(new Cell().Add(new Paragraph(h).SetFont(titleFont).SetFontSize(10))
                    .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetPadding(5));
            }

            foreach (var item in invoice.SaleItems)
            {
                itemsTable.AddCell(new Cell().Add(new Paragraph(item.ProductName).SetFont(normalFont).SetFontSize(10)).SetPadding(5));
                itemsTable.AddCell(new Cell().Add(new Paragraph(item.Quantity.ToString()).SetFont(normalFont).SetFontSize(10))
                    .SetTextAlignment(TextAlignment.CENTER).SetPadding(5));
                itemsTable.AddCell(new Cell().Add(new Paragraph($"${item.UnitPrice:F2}").SetFont(normalFont).SetFontSize(10))
                    .SetTextAlignment(TextAlignment.RIGHT).SetPadding(5));
                itemsTable.AddCell(new Cell().Add(new Paragraph($"{item.DiscountPercentage}%").SetFont(normalFont).SetFontSize(10))
                    .SetTextAlignment(TextAlignment.CENTER).SetPadding(5));
                itemsTable.AddCell(new Cell().Add(new Paragraph($"${item.TotalPrice:F2}").SetFont(normalFont).SetFontSize(10))
                    .SetTextAlignment(TextAlignment.RIGHT).SetPadding(5));
            }

            document.Add(itemsTable.SetMarginBottom(20));

            // Totals Table
            var totalsTable = new Table(UnitValue.CreatePercentArray(new float[] { 2, 1 }))
                .SetWidth(UnitValue.CreatePercentValue(50))
                .SetHorizontalAlignment(HorizontalAlignment.RIGHT);

            var totals = new (string label, decimal amount)[]
            {
                ("Subtotal:", invoice.SubTotal),
                ("Discount:", -invoice.DiscountAmount),
                ("Tax:", invoice.TaxAmount),
                ("Total:", invoice.TotalAmount),
                ("Paid:", invoice.PaidAmount),
                ("Balance:", invoice.TotalAmount - invoice.PaidAmount)
            };

            foreach (var (label, amount) in totals)
            {
                var labelCell = new Cell().Add(new Paragraph(label)
                    .SetFont(label == "Total:" ? titleFont : normalFont)
                    .SetFontSize(10))
                    .SetBorder(Border.NO_BORDER)
                    .SetTextAlignment(TextAlignment.RIGHT);

                var amountCell = new Cell().Add(new Paragraph($"${amount:F2}")
                    .SetFont(label == "Total:" ? titleFont : normalFont)
                    .SetFontSize(10))
                    .SetBorder(Border.NO_BORDER)
                    .SetTextAlignment(TextAlignment.RIGHT);

                if (label == "Total:")
                {
                    labelCell.SetBackgroundColor(ColorConstants.LIGHT_GRAY);
                    amountCell.SetBackgroundColor(ColorConstants.LIGHT_GRAY);
                }

                totalsTable.AddCell(labelCell);
                totalsTable.AddCell(amountCell);
            }

            document.Add(totalsTable);

            // Notes
            if (!string.IsNullOrEmpty(invoice.Notes))
            {
                document.Add(new Paragraph("Notes:")
                    .SetFont(titleFont)
                    .SetFontSize(12)
                    .SetMarginTop(20)
                    .SetMarginBottom(5));

                document.Add(new Paragraph(invoice.Notes)
                    .SetFont(normalFont)
                    .SetFontSize(10));
            }

            // Footer
            document.Add(new Paragraph("Thank you for your business!")
                .SetFont(normalFont)
                .SetFontSize(10)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginTop(30));

            document.Close();
            return memoryStream.ToArray();
        }

        public byte[] GenerateSalesReportPdf(IEnumerable<TopSellingProductDto> salesData, DateTime startDate, DateTime endDate)
        {
            using var memoryStream = new MemoryStream();
            using var writer = new PdfWriter(memoryStream);
            using var pdf = new PdfDocument(writer);
            var document = new Document(pdf, iText.Kernel.Geom.PageSize.A4);
            
            var titleFont = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD);
            var normalFont = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);
            
            document.Add(new Paragraph("Sales Report")
                .SetFont(titleFont)
                .SetFontSize(18)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(10));
                
            document.Add(new Paragraph($"Period: {startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy}")
                .SetFont(normalFont)
                .SetFontSize(12)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(20));
            
            // Add sales data table
            var table = new Table(UnitValue.CreatePercentArray(new float[] { 3, 2, 1, 2 })).UseAllAvailableWidth();
            
            // Headers
            string[] headers = { "Product Name", "Category", "Qty Sold", "Revenue" };
            foreach (var h in headers)
            {
                table.AddHeaderCell(new Cell().Add(new Paragraph(h).SetFont(titleFont).SetFontSize(10))
                    .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetPadding(5));
            }
            
            // Data rows
            foreach (var item in salesData)
            {
                table.AddCell(new Cell().Add(new Paragraph(item.ProductName).SetFont(normalFont).SetFontSize(10)).SetPadding(5));
                table.AddCell(new Cell().Add(new Paragraph(item.CategoryName).SetFont(normalFont).SetFontSize(10)).SetPadding(5));
                table.AddCell(new Cell().Add(new Paragraph(item.QuantitySold.ToString()).SetFont(normalFont).SetFontSize(10))
                    .SetTextAlignment(TextAlignment.CENTER).SetPadding(5));
                table.AddCell(new Cell().Add(new Paragraph($"${item.TotalRevenue:F2}").SetFont(normalFont).SetFontSize(10))
                    .SetTextAlignment(TextAlignment.RIGHT).SetPadding(5));
            }
            
            document.Add(table);
            document.Close();
            return memoryStream.ToArray();
        }

        public byte[] GenerateInventoryReportPdf(IEnumerable<ProductStockDto> inventoryData)
        {
            using var memoryStream = new MemoryStream();
            using var writer = new PdfWriter(memoryStream);
            using var pdf = new PdfDocument(writer);
            var document = new Document(pdf, iText.Kernel.Geom.PageSize.A4);

            var titleFont = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD);
            var normalFont = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);
            
            document.Add(new Paragraph("Inventory Report")
                .SetFont(titleFont)
                .SetFontSize(18)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(20));
            
            // Add inventory data table
            var table = new Table(UnitValue.CreatePercentArray(new float[] { 2, 2, 1, 1, 1 })).UseAllAvailableWidth();
            
            // Headers
            string[] headers = { "Product", "SKU", "Stock", "Min Level", "Status" };
            foreach (var h in headers)
            {
                table.AddHeaderCell(new Cell().Add(new Paragraph(h).SetFont(titleFont).SetFontSize(10))
                    .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetPadding(5));
            }
            
            // Data rows
            foreach (var item in inventoryData)
            {
                table.AddCell(new Cell().Add(new Paragraph(item.ProductName).SetFont(normalFont).SetFontSize(10)).SetPadding(5));
                table.AddCell(new Cell().Add(new Paragraph(item.SKU).SetFont(normalFont).SetFontSize(10)).SetPadding(5));
                table.AddCell(new Cell().Add(new Paragraph(item.CurrentStock.ToString()).SetFont(normalFont).SetFontSize(10))
                    .SetTextAlignment(TextAlignment.CENTER).SetPadding(5));
                table.AddCell(new Cell().Add(new Paragraph(item.MinStockLevel.ToString()).SetFont(normalFont).SetFontSize(10))
                    .SetTextAlignment(TextAlignment.CENTER).SetPadding(5));
                
                var status = item.IsOutOfStock ? "Out of Stock" : (item.IsLowStock ? "Low Stock" : "OK");
                var statusColor = item.IsOutOfStock ? ColorConstants.RED : (item.IsLowStock ? ColorConstants.ORANGE : ColorConstants.GREEN);
                
                table.AddCell(new Cell().Add(new Paragraph(status).SetFont(normalFont).SetFontSize(10).SetFontColor(statusColor))
                    .SetTextAlignment(TextAlignment.CENTER).SetPadding(5));
            }
            
            document.Add(table);
            document.Close();
            return memoryStream.ToArray();
        }
    }
}