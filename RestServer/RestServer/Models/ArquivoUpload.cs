using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestServer.Models
{
    public class ArquivoUpload
    {
        public HttpPostedFileBase arquivo { get; set; }
        public Contexto tipoConsulta { get; set; }
    }
}