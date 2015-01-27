using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestServer.Models
{
    public enum TipoProcesso { novo, processando, processado, coletado };

    public class Consulta
    {
        public int? Id;
        public string Chave;
        public DateTime DataConsulta;
        public int? matricula;
        public TipoProcesso StatusProcesso;
        public int Status;
        //public int numElementos;
        //public DateTime? DataRegistro;
        public DateTime? DataProcessado; //Enquanto a consulta não é finalizada pode ser nulo
        public Contexto tipoConsultaRealizada;

    }
}