using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RestServer.Models;
using System.IO;
using System.Web.Script.Serialization;

namespace RestServer.Controllers
{
    [Authorize]
    public class ConsultasController : Controller
    {
        //
        // GET: /Consultas/

        public ActionResult Index()
        {
            List<Solicitacao> solicitacoes = new List<Solicitacao>();

            ConexaoDb2 db = new ConexaoDb2();
            solicitacoes = db.ConsultarSolicitacoes(Convert.ToInt16(Session["idUsuario"]));

            return View(solicitacoes);
        }

        public FileResult ResultadoCSV(int idSolicitacao)
        {
            //Grava os extratos em um arquivo de saída e retorna para o usuário

            string nomeArquivo = "Consulta_U" + Session["idUsuario"] + "_S" + idSolicitacao.ToString() + ".csv";

            return File(Parser.CriarArquivoCSVDisco(idSolicitacao, nomeArquivo), "text/csv", nomeArquivo);
        }

        public FileResult ErrosCsv(int idSolicitacao)
        {
            //Grava os erros em um arquivo de saída e retorna para o usuário

            ConexaoDb2 db = new ConexaoDb2();
            Parser parse = new Parser();
            string arquivo = String.Empty;

            arquivo = parse.GerarArquivoErro(db.RecuperarErro(idSolicitacao));

            return File(Parser.ConverterTextoStreamMemoria(arquivo), "csv", "Erros_Consulta_U" + Session["idUsuario"] + "_S" + idSolicitacao.ToString() + ".csv");
        }

        public ActionResult ListaSolicitacoes()
        {
            List<Solicitacao> solicitacoes = new List<Solicitacao>();

            ConexaoDb2 db = new ConexaoDb2();
            solicitacoes = db.ConsultarSolicitacoes(Convert.ToInt16(Session["idUsuario"]));

            return this.Content(new Parser().ToXML(solicitacoes), "text/xml");
        }

        public ActionResult ResultadoXML(int idSolicitacao)
        {
            //Grava os extratos em um arquivo de saída e retorna para o usuário
            ConexaoDb2 db = new ConexaoDb2();
            Parser parse = new Parser();
            string ArquivoTemp = String.Empty;

            string nomeArquivo = System.IO.Path.GetTempPath() + "ConsultaXml_U" + Session["idUsuario"] + "_S" + idSolicitacao.ToString() + ".xml";

            if (!System.IO.File.Exists(nomeArquivo))
            {
                //Arquivo com Resultados
                ArquivoTemp = db.RecuperarResultado(idSolicitacao, TipoRetorno.xml);

                //Renomeando arquivo
                System.IO.File.Move(ArquivoTemp, nomeArquivo);
            }

            return File(nomeArquivo, "application/xml", "ConsultaXml_U" + Session["idUsuario"] + "_S" + idSolicitacao.ToString() + ".xml");
        }

        public ActionResult ErrosXML(int idSolicitacao)
        {
            string arquivo = String.Empty;

            //Arquivo com Resultados
            return this.Content(new Parser().ToXML(new Parser().GerarErrosXML(new ConexaoDb2().RecuperarErro(idSolicitacao))), "text/xml");
        }

        public ActionResult Incluir()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Incluir(ArquivoUpload arquivoUpload, string Descricao)
        {
            string nomeArquivo;
            int idSolicitacaoGerada, idUsuario;

            idUsuario = (int)Session["idUsuario"];

            nomeArquivo = Path.GetTempFileName();
            arquivoUpload.arquivo.SaveAs(nomeArquivo);

            ConexaoDb2 db = new ConexaoDb2();
            ManipulaCsv arquivoMatriculas = new ManipulaCsv();

            //Verificar SALDO, se tiver 2 consultas libera leitura de matrículas e guarda em uma varialvel valor do saldo
            int saldo = db.VerificarSaldo(idUsuario);

            if (saldo >= 2)
            {
                List<string> matriculas = arquivoMatriculas.LerCsv(nomeArquivo);

                //Verificar se o número de beneficios é maior que o saldo;
                if (matriculas.Count <= saldo)
                {
                    idSolicitacaoGerada = db.InserirSolicitacao(idUsuario, Descricao); //Pego o código da última solicitação inserida por este usuário.

                    if (!(saldo == 500000))
                    {
                        db.InserirExtrato(idUsuario, idSolicitacaoGerada, matriculas.Count);
                    }

                    List<string> LoteConsulta = new List<string>();
                    int cont = 0;
                    int i = 0;
                    //Abriria conexão

                    while (cont < matriculas.Count)
                    {
                        while (i < 1000 && cont < matriculas.Count)
                        {
                            LoteConsulta.Add(matriculas[cont].Trim());
                            i++;
                            cont++;
                        }

                        // Solicitacao de consulta no servidor e gravação de resultado no banco de dados
                        Parser metodos = new Parser();
                        Consulta consulta = new Consulta();

                        //Lote RV
                        consulta = metodos.InterpretarJsonPedidoConsulta(Servidor.RealizarPedido(LoteConsulta, Contexto.rv));
                        db.InserirConsulta(consulta, idSolicitacaoGerada);

                        //Lote Titular
                        consulta = metodos.InterpretarJsonPedidoConsulta(Servidor.RealizarPedido(LoteConsulta, Contexto.titular));
                        db.InserirConsulta(consulta, idSolicitacaoGerada);

                        //Lote Conbas
                        consulta = metodos.InterpretarJsonPedidoConsulta(Servidor.RealizarPedido(LoteConsulta, Contexto.conbas));
                        db.InserirConsulta(consulta, idSolicitacaoGerada);

                        i = 0;
                        LoteConsulta.Clear();
                    }
                }

                else
                {
                    throw new Exception("Você tem " + saldo + " consultas disponíveis. Reduza o número de benefícios para que a consulta seja realizada");
                }
            }

            else
            {
                throw new Exception("Você tem menos de 2 consultas disponíveis. Para realizar as consultas em lote você precisa ter pelo menos 2 consultas.");
            }

            return RedirectToAction("Index", "Consultas", null);
        }

    }
}
