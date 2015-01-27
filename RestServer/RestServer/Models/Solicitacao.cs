using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestServer.Models
{
    public enum StatusSolicitacao { Processando=1, Download};

    public class Solicitacao
    {
        public int Id;
        public string Descricao;
        public StatusSolicitacao status;
        public DateTime DataSolicitacao;
        public DateTime? DataConclusaoSolicitacao;
        public bool ErrosSolicitacao;
    }
}
