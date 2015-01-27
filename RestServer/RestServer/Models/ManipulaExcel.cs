using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Office.Interop.Excel;
using System.IO;

namespace RestServer.Models
{
    public class ManipulaExcel
    {
        public List<string> LerPlanilha(string caminho)
        {
            List<string> matriculas = new List<string>();
            int linha = 2; //A partir da segunda linha, primeira é cabeçalho
            int intervaloVazio = 0;
            bool fim = false;

            Application abreExcel = new Application();
            Workbook Planilha = abreExcel.Workbooks.Open(caminho);

            try
            {
                Worksheet aba = Planilha.Worksheets[1];

                while (!fim)
                {
                    if (intervaloVazio > 9)
                    {
                        fim = true;
                    }

                    if ((aba.Columns[1].Cells[linha].Value != null) || (!aba.Columns[1].Cells[linha].Value.Equals(String.Empty)))
                    {
                        matriculas.Add(aba.Columns[1].Cells[linha].Value);
                        intervaloVazio = 0;
                        linha++;
                    }

                    else
                    {
                        intervaloVazio++;
                        linha++;
                    }
                }
                
                Planilha.Close();

                if (linha < 3)
                {
                    throw new Exception("Matrículas insuficientes. Para consulta em lote é necessário ao menos 2 matrículas");
                }
            }

            catch(Exception)
            {
                abreExcel.Quit();
            }

            finally
            {
                abreExcel.Quit();
            }

            return matriculas;
        }


        public Workbook GerarPlanilhaRv(List<ExtratoRV> extratos)
        {
            int linha = 1;
            bool primeiraLinha = true;
            Workbook PlanilhaResultado = new Workbook();

            foreach (ExtratoRV e in extratos)
            {
                int coluna = 1;

                if (primeiraLinha)
                {
                    PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = "MATRÍCULA";
                    PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = "NOME";
                    PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = "SITUAÇÃO";
                    PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = "NASCIMENTO";
                    PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = "SALÁRIO";
                    PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = "MÊS COMPETÊNCIA";
                    PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = "COMPETÊNCIA INICIAL";
                    PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = "COMPETÊNCIA FINAL";
                    PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = "CÓDIGO AGÊNCIA";
                    PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = "VALIDADE INICIAL";
                    PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = "VALIDADE FINAL";
                    PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = "BANCO";
                    PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = "CONTA CORRENTE";
                    PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = "VALOR LÍQUIDO CRÉDITO";

                    primeiraLinha = false;
                    coluna = 1;
                }

                PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = e.Matricula;
                PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = e.Nome;
                PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = e.Situacao;
                PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = e.Nascimento;
                PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = e.Salario;
                PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = e.MesCompetencia;
                PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = e.CompetenciaInicial;
                PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = e.CompetenciaFinal;
                PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = e.CodigoAgencia;
                PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = e.ValidadeInicial;
                PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = e.ValidadeFinal;
                PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = e.Banco;
                PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = e.ContaCorrente;
                PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = e.ValorLiquidoCredito;

                int contRubricas = 1;
                foreach (var rubrica in e.Rubricas)
                {
                    if (PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[1].Value.Equals(String.Empty))
                    {
                        //Verifico se aquela coluna na primeira linha está vazia, se estiver ele inclui o texto na célula da coluna e linha da rúbrica atual
                        PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[1].Value = "Código Rubrica [" + contRubricas.ToString() + "]";
                        PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[1].Value = "Nome Rubrica [" + contRubricas.ToString() + "]";
                        PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[1].Value = "Valor Rúbrica [" + contRubricas.ToString() + "]";
                        PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[1].Value = "Sinal Rúbrica [" + contRubricas.ToString() + "]";
                    }

                    PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = rubrica.Codigo;
                    PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = rubrica.Nome;
                    PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = rubrica.Valor;
                    PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = rubrica.Sinal;
                }

                linha++;
            }
            
            return PlanilhaResultado;
        }

        public Workbook GerarPlanilhaTitular(List<ExtratoTitular> extratos)
        {
            int linha = 1;
            bool primeiraLinha = true;
            Workbook PlanilhaResultado = new Workbook();

            foreach (ExtratoTitular e in extratos)
            {
                int coluna = 1;

                if (primeiraLinha)
                {
                    PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = "MATRÍCULA";
                    PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = "SITUAÇÃO";
                    PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = "NOME TITULAR";
                    PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = "NOME MÃE";
                    PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = "CPF";
                    PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = "IDENTIDADE";
                    PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = "MUNICÍPIO DA IDENTIDADE";
                    PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = "UF DA IDENTIDADE";
                    PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = "SEXO";
                    PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = "DATA DE NASCIMENTO";
                    PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = "ENDEREÇO";
                    PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = "CEP";
                    PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = "MUNICÍPIO";
                    PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = "UF";
                    PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = "BAIRRO";
                    PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = "FONE";
                    PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = "DDD/ RAMAL";
                    PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = "EMAIL";

                    primeiraLinha = false;
                    coluna = 1;
                    linha++;
                }

                PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = e.Matricula;
                PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = e.Situacao;
                PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = e.NomeTitular;
                PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = e.NomeMae;
                PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = e.Cpf;
                PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = e.Identidade;
                PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = e.MunicipioIdentidade;
                PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = e.UfIdentidade;
                PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = e.Sexo;
                PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = e.Nascimento;
                PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = e.Endereco;
                PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = e.Cep;
                PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = e.Municipio;
                PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = e.Uf;
                PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = e.Bairro;
                PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = e.Tel;
                PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = e.Ddd + "/ " + e.Ramal;
                PlanilhaResultado.Worksheets[1].Columns[coluna++].Cells[linha].Value = e.Email;
                linha++;
            }
            return PlanilhaResultado;
        }


        public string SalvarPlanilhaResultado(Workbook planilha , string pasta, Contexto tipoConsulta, int idUsuario)
        {
            string NomeArquivoResultado = String.Empty;
          
            NomeArquivoResultado = Path.GetDirectoryName(pasta) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(pasta) + idUsuario.ToString() + "Consulta " + Enum.GetName(typeof(Contexto), tipoConsulta) + Path.GetExtension(pasta);
          
            try
            {
                planilha.SaveAs(NomeArquivoResultado);
            }
            catch (Exception)
            {
                throw new Exception("Erro ao tentar salvar planílha com dados obtidos na consulta.");
            }

            return NomeArquivoResultado;
        }
    }
}