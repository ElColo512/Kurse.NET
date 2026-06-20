using Kurse.Models.Enums;

namespace Kurse.Models.Extensions
{
    public static class DiaSemanaExtensions
    {
        public static string ToShortName(this DiaSemana dia)
        {
            return dia switch
            {
                DiaSemana.Lunes => "Lun",
                DiaSemana.Martes => "Mar",
                DiaSemana.Miercoles => "Mié",
                DiaSemana.Jueves => "Jue",
                DiaSemana.Viernes => "Vie",
                DiaSemana.Sabado => "Sáb",
                DiaSemana.Domingo => "Dom",
                _ => string.Empty
            };
        }
    }
}
