using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace RestServer.Models
{
    public class ManipulaCsv
    {
        public List<string> LerCsv(string caminho)
        {
            List<string> matriculas = new List<string>();
            StreamReader lerMatriculas = new StreamReader(caminho);

            while (!lerMatriculas.EndOfStream)
            {
                string textoCorrente = lerMatriculas.ReadLine();

                if (!Parser.ContemLetras(textoCorrente) &&
                    (textoCorrente.Contains("0") || textoCorrente.Contains("1") || textoCorrente.Contains("2") || textoCorrente.Contains("3") || textoCorrente.Contains("4") ||
                    textoCorrente.Contains("5") || textoCorrente.Contains("6") || textoCorrente.Contains("7") || textoCorrente.Contains("8") || textoCorrente.Contains("9")))
                {
                    matriculas.Add(textoCorrente.Trim());
                }
            }

            //Retirando matrículas duplicadcas
            matriculas.Distinct().ToList();

            if (matriculas.Count < 2 || matriculas.Count > 100000)
            {
                throw new Exception("A consulta não pôde ser realizada. O arquivo deve conter no mínimo 2 benefícios e no máximo 100 mil.");
            }
            lerMatriculas.Close();
            return matriculas;
        }

        public string GerarCsvRv(ExtratoRV extratos)
        {
            StringBuilder dadosExtratoCsv = new StringBuilder();

            //Campos que se repetem na consulta titular

            //dadosExtratoCsv.Append("\"" + extratos.Matricula +"\";");
            //dadosExtratoCsv.Append(extratos.Nome + ";");
            //dadosExtratoCsv.Append(extratos.Situacao + ";");
            //dadosExtratoCsv.Append(extratos.Nascimento.ToString() + ";");

            dadosExtratoCsv.Append(extratos.Salario.ToString() + ";");
            dadosExtratoCsv.Append(extratos.MesCompetencia.ToString() + ";");
            dadosExtratoCsv.Append(extratos.CompetenciaInicial.ToString() + ";");
            dadosExtratoCsv.Append(extratos.CompetenciaFinal.ToString() + ";");
            dadosExtratoCsv.Append(extratos.CodigoAgencia.ToString() + ";");
            dadosExtratoCsv.Append(extratos.ValidadeInicial.ToString() + ";");
            dadosExtratoCsv.Append(extratos.ValidadeFinal.ToString() + ";");
            dadosExtratoCsv.Append(extratos.Banco.ToString() + ";");
            dadosExtratoCsv.Append(extratos.ContaCorrente.ToString() + ";");
            dadosExtratoCsv.Append(extratos.ValorLiquidoCredito.ToString() + ";");
            dadosExtratoCsv.Append(extratos.Margem.ToString()+";");
            foreach (var rubrica in extratos.Rubricas)
            {
                dadosExtratoCsv.Append(rubrica.Codigo.ToString() + ";");
                dadosExtratoCsv.Append(rubrica.Nome.ToString() + ";");
                dadosExtratoCsv.Append(rubrica.Valor.ToString() + ";");
                dadosExtratoCsv.Append(rubrica.Sinal.ToString()+ ";");
            }

            dadosExtratoCsv.Append("\n");

            return dadosExtratoCsv.ToString();
        }

        public string GerarCsvTitular(ExtratoTitular extratos)
        {
            StringBuilder resultadoCsv = new StringBuilder();

            resultadoCsv.Append("\"" + extratos.Matricula + "\";");
            resultadoCsv.Append(extratos.Situacao + ";");
            resultadoCsv.Append(extratos.NomeTitular + ";");
            resultadoCsv.Append(extratos.NomeMae + ";");
            resultadoCsv.Append("\"" + extratos.Cpf + "\";");
            resultadoCsv.Append("\"" + extratos.Identidade + "\";");
            resultadoCsv.Append(extratos.MunicipioIdentidade + ";");
            resultadoCsv.Append(extratos.UfIdentidade + ";");
            resultadoCsv.Append(extratos.Sexo + ";");
            resultadoCsv.Append(extratos.Nascimento.ToString() + ";");
            resultadoCsv.Append(extratos.Endereco + ";");
            resultadoCsv.Append("\"" + extratos.Cep + "\";");
            resultadoCsv.Append(extratos.Municipio + ";");
            resultadoCsv.Append(extratos.Uf + ";");
            resultadoCsv.Append(extratos.Bairro + ";");
            resultadoCsv.Append("\"" + extratos.Ddd + "\";");
            resultadoCsv.Append("\"" + extratos.Ramal + "\";");
            resultadoCsv.Append("\"" + extratos.Tel + "\";");
            resultadoCsv.Append(extratos.Email + ";");

            return resultadoCsv.ToString();
        }

        public string GerarCsvConbas(ExtratoConbas extratos)
        {
            StringBuilder resultadoCsv = new StringBuilder();

            resultadoCsv.Append("\"" + extratos.CodigoEspecie + "\";");
            resultadoCsv.Append(extratos.Especie + ";");
            resultadoCsv.Append("\"" + extratos.CodigoRamoAtividade + "\";");
            resultadoCsv.Append(extratos.RamoAtividade + ";");
            resultadoCsv.Append("\"" + extratos.CodigoFormaFiliacao + "\";");
            resultadoCsv.Append(extratos.FormaFiliacao + ";");
            resultadoCsv.Append(extratos.DataAdmissao + ";");
            
            return resultadoCsv.ToString();
        }


    }
}