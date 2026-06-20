using Kurse.Models.Requests;
using Kurse.ViewModels.Alumno;

namespace Kurse.Services.Interfaces
{
    public interface IPdfService
    {
        byte[] Generate(PdfReportRequest request);
        byte[] GenerateCertificate(CertificadoViewModel model);
    }
}
