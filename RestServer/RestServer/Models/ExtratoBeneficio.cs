using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestServer.Models
{
    public class ExtratoBeneficio
    {
        public string nb { get; set; }
        public ExtratoTitular Titular { get; set; }
        public ExtratoRV Rv { get; set; }
        public ExtratoConbas Conbas { get; set; }
    }
}