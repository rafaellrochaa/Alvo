using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestServer.Models
{
    public class ExtratoConbas
    {
        public int? CodigoEspecie { get; set; }
        public string Especie { get; set; }
        public int? CodigoRamoAtividade { get; set; }
        public string RamoAtividade { get; set; }
        public int? CodigoFormaFiliacao { get; set; }
        public string FormaFiliacao { get; set; }
        public DateTime? DataAdmissao { get; set; }
    }
}