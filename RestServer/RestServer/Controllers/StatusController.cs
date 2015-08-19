using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RestServer.Models;

namespace RestServer.Controllers
{
    public class StatusController : Controller
    {
        //
        // GET: /Status/

        [AllowAnonymous]

        public string Atualizar()
        {
            string retorno = "OK";

            //Se a solicitação já está com o status Download, significa que os lotes foram coletados, portanto o controller faz apenas 1 refresh.
            ConexaoDb2 db = new ConexaoDb2();
            var solicitacoes = db.ConsultarSolicitacoesPendentes();

            foreach (var solicitacao in solicitacoes)
            {
                bool haviamlotesPendentes = false;

                if (!(db.SolicitacaoColetada(solicitacao.Id)))
                {
                    haviamlotesPendentes = true;
                    Parser parse = new Parser();

                    //lista que contém as consultas não coletadas.
                    List<Consulta> consultasSolicitadas = db.ListarConsultasSolicitacao(solicitacao.Id);
                    try
                    {
                        foreach (var consulta in consultasSolicitadas)
                        {
                            if (!db.VerificarConsultaProcessada(consulta.Id))
                            {
                                //Se o lote não foi processado é realizada a consulta no servidor e o status do processo é atualizado no banco de dados
                                db.AtualizarConsulta(parse.InterpretarJsonPedidoConsulta(Servidor.ConsultarStatusPedido(consulta.Chave)));
                            }

                            else
                            {
                                //Se a consulta foi processada é necessário fazer a coleta.
                                db.inserirResultado(parse.InterpretarJsonConsultarBuscaLote(Servidor.buscarPedido(consulta.Chave)), (int)consulta.Id, solicitacao.Id, consulta.Status);
                            }
                        }
                    }
                    finally
                    {
                        db.LiberarAtualizandoStatus(solicitacao.Id);
                    }
                }

                if (haviamlotesPendentes)
                {
                    /*Quando haviam lotes pendentes na solicitação é feita uma verificação para saber se os lotes foram coletados, se foram todos coletados
                    a soliticação passa para o status download, (após ser coletada, ele não passa mais neste if), e 
                    é disparado um email para o usuário informando que o arquivo já pode ser baixado.*/

                    //Conectando no banco de dados
                    ConexaoDb2 conexao = new ConexaoDb2();
                    bool SolicitacaoCompleta = conexao.AtualizarStatusSolicitacao();

                    //Rotina de disparo de email
                    if (SolicitacaoCompleta)
                    {
                        Usuario u = new Usuario();

                        //Obtendo usuário da solicitação atual
                        int idUsuario = conexao.IdUsuarioSolicitacao(solicitacao.Id);
                        u = conexao.RecuperarDadosUsuario(idUsuario);

                        //Criando link de download para encaminhar no email
                        string ArquivoDownload = @"http://www.agilus.com.br/Alvo/Consultas/ResultadoCsv?idSolicitacao=" + solicitacao.Id.ToString();

                        //Gerando arquivo com dados da solicitação
                        Parser.CriarArquivoCSVDisco(solicitacao.Id, "Consulta_U" + idUsuario.ToString() + "_S" + solicitacao.Id.ToString() + ".csv");

                        //Disparar email
                        string assunto = "Sua consulta foi processada com sucesso";

                        string mensagem = @"Olá " + char.ToUpper(u.Nome[0]) + u.Nome.Substring(1) + ".<br>" +
                        @"Sua consulta foi finalizada e você pode baixar o arquivo com o resultado no link abaixo.<br> " + ArquivoDownload + "<br> " +
                        @"Para fazer o download do arquivo, será necessário digitar seu login e senha. Após isto o download iniciará automaticamente.";

                        Email email = new Email(u.Email, mensagem, assunto);
                        email.DispararMensagem();
                    }
                }
            }
            return retorno;
        }


    }
}
