using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace RestServer.Models
{
    public class ExtratoRV
    {
        public string Matricula { get; set; }
        public string Nome { get; set; }
        public string Situacao { get; set; }
        public DateTime? Nascimento { get; set; }
        public double? Salario { get; set; }
        public DateTime? MesCompetencia { get; set; }
        public DateTime? CompetenciaInicial { get; set; }
        public DateTime? CompetenciaFinal { get; set; }
        public string CodigoAgencia { get; set; }
        public DateTime? ValidadeInicial { get; set; }
        public DateTime? ValidadeFinal { get; set; }
        public int? Banco { get; set; }
        public string ContaCorrente { get; set; }
        public double ValorLiquidoCredito { get; set; }
        public double Margem { get; set; }
        public List<Rubrica> Rubricas { get; set; }

        public void GetMargem(List<RegraCalculoMargem> regras)
        {
            double valoresPreCalculo =
            (from rg in regras
             join rb in Rubricas
             on rg.Descricao equals rb.Nome
             where rg.PreCalculo == true
             select Convert.ToDouble(rb.Sinal + "1" ) * rb.Valor).Sum();

            //O benefício tem margem caso a tabela de regras for o código do rmc
            bool cartao =
            (from rg in regras
             join rb in Rubricas
             on rg.Descricao equals rb.Nome
             where rg.CartaoRmc == true
             select rg.CartaoRmc).DefaultIfEmpty(false).First();

            double valoresPosCalculo =
            (from rg in regras
            join rb in Rubricas
            on rg.Descricao equals rb.Nome
            where rg.PosCalculo == true
            select Convert.ToDouble(rb.Sinal + "1") * rb.Valor).Sum();

            Margem = valoresPreCalculo * (cartao ? 0.2 : 0.3) + valoresPosCalculo;
        }
    }

}