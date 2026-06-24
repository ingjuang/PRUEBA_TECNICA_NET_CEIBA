using System;
using System.Collections.Generic;
using System.Text;

namespace EventosVivosBackNet.Application.Commond.Models.DTOs.DtoBase
{
    public class DtoGenericResponse<T>
    {
        public string? Mensaje { get; set; }
        public bool EsExitoso { get; set; }
        public T? Resultado { get; set; }

        public bool IsOkEsquema { get; set; }
    }
}
