using Kurse.Models.Enums;
using Kurse.Models.Requests;
using Kurse.Services.Interfaces;
using Kurse.ViewModels.Alumno;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Kurse.Services.Implementations
{
    public class PdfService : IPdfService
    {
        public byte[] Generate(PdfReportRequest request)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(request.Landscape ? PageSizes.A4.Landscape() : PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    // HEADER
                    page.Header()
                        .Column(column =>
                        {
                            column.Item()
                                .Text("Sistema Kurse")
                                .FontSize(24)
                                .Bold()
                                .FontColor(Colors.Blue.Darken2);

                            column.Item()
                                .Text(request.Title)
                                .FontSize(16)
                                .SemiBold();

                            column.Item()
                                .Text($"Fecha de generación: {request.GeneratedAt:dd/MM/yyyy HH:mm}")
                                .FontSize(9)
                                .FontColor(Colors.Grey.Darken1);

                            column.Item()
                                .PaddingTop(10)
                                .LineHorizontal(1)
                                .LineColor(Colors.Grey.Lighten1);
                        });

                    // CONTENT
                    page.Content()
                    .Container()
                    .PaddingVertical(20)
                    .Background(Colors.White)
                    .Border(1)
                    .BorderColor(Colors.Grey.Lighten2)
                    .CornerRadius(10)
                    .Padding(5)
                    .PaddingVertical(20)
                    .Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            if (request.ColumnWidths?.Count > 0)
                            {
                                foreach (float width in request.ColumnWidths)
                                {
                                    columns.RelativeColumn(width);
                                }
                            }
                            else
                            {
                                for (int i = 0; i < request.Headers.Count; i++)
                                {
                                    columns.RelativeColumn();
                                }
                            }
                        });

                        // ENCABEZADOS
                        foreach (string header in request.Headers)
                        {
                            table.Cell()
                            .Background("#495057")
                            .Border(1)
                            .BorderColor("#3d4348")
                            .PaddingVertical(8)
                            .PaddingHorizontal(5)
                            .AlignCenter()
                            .Text(header)
                            .FontColor(Colors.White)
                            .Bold();
                        }

                        // FILAS
                        for (int rowIndex = 0; rowIndex < request.Rows.Count; rowIndex++)
                        {
                            List<string> row = request.Rows[rowIndex];
                            string background = rowIndex % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                            for (int col = 0; col < row.Count; col++)
                            {
                                string cell = row[col];

                                IContainer container = table.Cell()
                                .Background(background)
                                .BorderBottom(1)
                                .BorderColor(Colors.Grey.Lighten2)
                                .PaddingVertical(6)
                                .PaddingHorizontal(4)
                                .Padding(5);

                                PdfColumnAlignment alignment = request.Alignments?.ElementAtOrDefault(col) ?? PdfColumnAlignment.Left;

                                container = alignment switch
                                {
                                    PdfColumnAlignment.Center => container.AlignCenter(),
                                    PdfColumnAlignment.Right => container.AlignRight(),
                                    _ => container
                                };

                                container.Text(cell)
                                .FontSize(9);
                            }
                        }
                    });

                    // FOOTER
                    page.Footer()
                    .Row(row =>
                    {
                        row.RelativeItem()
                        .Text("Kurse - Sistema de Gestión Académica")
                        .FontSize(9)
                        .FontColor(Colors.Grey.Darken1);

                        row.ConstantItem(100)
                        .AlignRight()
                        .Text(text =>
                        {
                            text.Span("Página ");
                            text.CurrentPageNumber();
                            text.Span(" de ");
                            text.TotalPages();
                        });
                    });
                });
            })
          .GeneratePdf();
        }

        public byte[] GenerateCertificate(CertificadoViewModel model)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);

                    page.Header()
                        .AlignCenter()
                        .Text("CERTIFICADO DE APROBACIÓN")
                        .FontSize(24)
                        .Bold();

                    page.Content()
                        .PaddingVertical(30)
                        .Column(column =>
                        {
                            column.Spacing(20);

                            column.Item()
                                .AlignCenter()
                                .Text("Se certifica que");

                            column.Item()
                                .AlignCenter()
                                .Text(model.Alumno)
                                .FontSize(22)
                                .Bold();

                            column.Item()
                                .AlignCenter()
                                .Text(text =>
                                {
                                    text.Span("ha aprobado satisfactoriamente el curso ");
                                    text.Span($"\"{model.Curso}\"").Bold();
                                    text.Span(", dictado por ");
                                    text.Span(model.Profesor).Bold();
                                    text.Span($", con una duración de {model.DuracionHoras} horas reloj, desarrollado entre el ");
                                    text.Span(model.FechaInicio.ToString("dd/MM/yyyy")).Bold();
                                    text.Span(" y el ");
                                    text.Span(model.FechaFin.ToString("dd/MM/yyyy")).Bold();
                                    text.Span(".");
                                });

                            column.Item()
                                .PaddingTop(20)
                                .AlignCenter()
                                .Text($"Se expide el presente certificado el día {model.FechaEmision:dd/MM/yyyy}.");
                        });

                    page.Footer()
                        .PaddingTop(20)
                        .AlignCenter()
                        .Column(column =>
                        {
                            column.Item()
                                .Text("_______________________________");

                            column.Item()
                                .Text("Responsable Académico");
                        });
                });
            })
                .GeneratePdf();
        }
    }
}
