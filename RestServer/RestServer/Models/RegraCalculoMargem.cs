using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestServer.Models
{
    public class RegraCalculoMargem
    {
        public int Id;
        public string Descricao;
        public string Sinal;
        public int CodigoDataPrev;
        public bool PreCalculo;
        public bool PosCalculo;
        public bool CartaoRmc;
    }
}