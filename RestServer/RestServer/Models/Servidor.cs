using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace RestServer.Models
{
    public enum Contexto { titular = 1, rv, conbas };

    public static class Servidor
    {
        //Token
        private static string token = "15a3e82367657a52b7aa1108dcbcc164";// Chave estática

        private static MemoryStream RealizarPost(string restdata)
        {
            MemoryStream respostaPost;

            using (var client = new WebClient())
            {
                var values = new NameValueCollection();

                values["restdata"] = restdata;

                var response = client.UploadValues("http://grade.alvo.ws/restserver/api/content/format/", values);

                var responseString = Encoding.Default.GetString(response);

                respostaPost = new MemoryStream(Encoding.UTF8.GetBytes(responseString));
            }
            return respostaPost;
        }

        public static void VerificarIP(string ipVerificacao, string ipCadastrado)
        {
            if (Parser.ContemLetras(ipCadastrado))
            {
                WebResponse response = null;
                WebRequest request = WebRequest.Create("http://tracert.com/resolver?arg=" + ipCadastrado);
                StreamReader interpretador = null;

                response = request.GetResponse();
                Stream responseStream = response.GetResponseStream();

                System.Text.Encoding encoding = System.Text.Encoding.Default;
                interpretador = new StreamReader(responseStream, encoding);

                string endereco = interpretador.ReadToEnd();

                if (response != null)
                {
                    response.Close();
                }

                if (interpretador != null)
                {
                    interpretador.Close();
                }
                if (endereco.IndexOf("<h2>Could not resolve") > 0)
                {
                    throw new Exception("O DNS dinâmico não pôde ser resolvido.");
                }
                if (endereco.IndexOf("<h2>Resolution of") > 0)
                {
                    int inicio = endereco.IndexOf("<h2>Resolution of"); //ínicio da string
                    string[] enderecoResolvido = endereco.Substring(inicio, ((endereco.IndexOf("</h2>",inicio+1)) - inicio)).Split(' ');
                    ipCadastrado = enderecoResolvido[enderecoResolvido.Length - 1];
                }
            }

            if (ipVerificacao != ipCadastrado)
            {
                throw new Exception("O ip cadastrado em sua conta não confere com o seu ip atual.");
            }
        }

        public static MemoryStream RealizarPedido(List<string> matriculas, Contexto tipoConsulta)
        {
            if (matriculas.Equals(String.Empty))
            {
                throw new Exception("Erro ao processar matrículas. Verifique se as matrículas informadas estão no formato e local correto.");
            }

            string restdata =
            @"{'contexto': '" + tipoConsulta + @"',
            'processo':'crawler',  
            'token': '" + token + @"',
            'data': [";

            foreach (string m in matriculas)
            {
                restdata += @"{'nb': '" + m + @"'},";
            }

            restdata = restdata.Substring(0, restdata.Length - 1) + @"]}";

            restdata = restdata.Replace("'", "\"");

            return RealizarPost(restdata);
        }

        public static MemoryStream ConsultarStatusPedido(string chave)
        {
            string restdata =
            @"{'transacao': '" + chave + @"',
               'token': '" + token + @"'
              }";

            restdata = restdata.Replace("'", "\"");
            return RealizarPost(restdata);
        }

        public static MemoryStream buscarPedido(string chave)
        {
            string restdata =
            @"{'transacao': '" + chave + @"',
            'token': '" + token + @"',
            'elementos': 'true'
            }";

            restdata = restdata.Replace("'", "\"");

            return RealizarPost(restdata);
        }
    }
}