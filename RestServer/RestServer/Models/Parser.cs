using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Web;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Data;
using Npgsql;


namespace RestServer.Models
{
    public class Parser
    {
        [DataContract]
        private class JsonRetornoConsulta
        {
            [DataMember]
            public string id;

            [DataMember]
            public string contexto;

            [DataMember]
            public string data_registro;

            [DataMember]
            public string data_processo;

            [DataMember]
            public int status_processo;

            [DataMember]
            public int status;

            [DataMember]
            public int num_elementos;

            [DataMember]
            public string token;

            [DataMember]
            public string processo;
        }

        [DataContract]
        private class ListaRespostaConsulta
        {
            [DataMember]
            public JsonRespostaConsulta[] Respostas;
        }

        [DataContract]
        private class JsonRespostaConsulta
        {
            [DataMember]
            public string id;

            [DataMember]
            public string lote_id;

            [DataMember]
            public string parametros;

            [DataMember]
            public string result1;

            [DataMember]
            public string result2;

            [DataMember]
            public string data_processo;

            [DataMember]
            public string status_processo;

            [DataMember]
            public string status;

            [DataMember]
            public string retry;

            [DataMember]
            public string contexto;

            [DataMember]
            public string tipo;

            [DataMember]
            public string processo_id;
        }

        public Consulta InterpretarJsonPedidoConsulta(MemoryStream retornoPost)
        {
            //Cópia do retorno da consulta, para avaliação de possíveis erros;
            MemoryStream copiaRetornoPost = new MemoryStream();
            retornoPost.CopyTo(copiaRetornoPost);
            retornoPost.Position = 0;
            copiaRetornoPost.Position = 0;

            JsonRetornoConsulta respostaJsonConsulta;
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(JsonRetornoConsulta)); // Para serializar precisa de um objeto com os campos idênticos aos campos de retorno;
            string LoteConsulta = ConverterStreamMemoriaTexto(copiaRetornoPost).Substring(0, 10);
            
            //Verificando se houve erro retornado do servidor de consulta, pois se tentar serializar antes pra verificar depois a referência é perdida;
            if (LoteConsulta.Contains(@"{""id"":"""))
            {
                respostaJsonConsulta = (JsonRetornoConsulta)ser.ReadObject(retornoPost); // Após criar o objeto serializado com o tipo, basta ler com o ReadObject;
            }
            else
            {
                throw new Exception("Erro no servidor de consultas DATAPREV: " + LoteConsulta);
            }
            return 
                new Consulta(){
                    Chave= respostaJsonConsulta.id,
                    DataConsulta= Convert.ToDateTime(respostaJsonConsulta.data_registro),
                    Status= Convert.ToInt16(respostaJsonConsulta.status),
                    StatusProcesso= (TipoProcesso)Convert.ToInt16(respostaJsonConsulta.status_processo),
                    DataProcessado= Convert.ToDateTime(respostaJsonConsulta.data_processo),
                    tipoConsultaRealizada= (Contexto)Enum.Parse(typeof(Contexto),respostaJsonConsulta.contexto)};
        }

        public Consulta InterpretarJsonPedidoConsulta(MemoryStream retornoPost, int idConsulta)
        {
            //Cópia do retorno da consulta, para avaliação de possíveis erros;
            MemoryStream copiaRetornoPost = new MemoryStream();
            retornoPost.CopyTo(copiaRetornoPost);
            retornoPost.Position = 0;
            copiaRetornoPost.Position = 0;

            JsonRetornoConsulta respostaJsonConsulta;
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(JsonRetornoConsulta)); // Para serializar precisa de um objeto com os campos idênticos aos campos de retorno;
            string LoteConsulta = ConverterStreamMemoriaTexto(copiaRetornoPost).Substring(0, 10);

            //Verificando se houve erro retornado do servidor de consulta, pois se tentar serializar antes pra verificar depois a referência é perdida;
            if (LoteConsulta.Contains(@"{""id"":"""))
            {
                respostaJsonConsulta = (JsonRetornoConsulta)ser.ReadObject(retornoPost); // Após criar o objeto serializado com o tipo, basta ler com o ReadObject;
            }
            else
            {
                throw new Exception("Erro no servidor de consultas DATAPREV: " + LoteConsulta);
            }
            return
                new Consulta()
                {
                    Id= idConsulta,
                    Chave = respostaJsonConsulta.id,
                    DataConsulta = Convert.ToDateTime(respostaJsonConsulta.data_registro),
                    Status = Convert.ToInt16(respostaJsonConsulta.status),
                    StatusProcesso = (TipoProcesso)Convert.ToInt16(respostaJsonConsulta.status_processo),
                    DataProcessado = Convert.ToDateTime(respostaJsonConsulta.data_processo),
                    tipoConsultaRealizada = (Contexto)Enum.Parse(typeof(Contexto), respostaJsonConsulta.contexto)
                };
        }

        public static T deseralization<T>(MemoryStream ms)
        {
            DataContractJsonSerializer jsonSer = new DataContractJsonSerializer(typeof(T));
            ms.Position = 0;
            return (T)jsonSer.ReadObject(ms);
        }

        public Dictionary<string, string> InterpretarJsonConsultarBuscaLote(MemoryStream retornoPost)
        {
            var respostaConsulta = deseralization<JsonRespostaConsulta[]>(retornoPost);

            var retorno = new Dictionary<string, string>();
            foreach (var item in respostaConsulta)
            {
                retorno.Add(LimparTextoResultado(item.parametros), item.result1);
            }

            return retorno;
        }



        public static bool ContemLetras(string texto)
        {
            if (texto.Where(c => char.IsLetter(c)).Count() > 0)
                return true;
            else
                return false;
        }

        private string LimparTextoResultado(string textoSujo)
        {
            string textoLimpo;
            textoLimpo = textoSujo.Replace(@"{", "").Replace("}", "").Replace(@"\", "").Replace(@"/""", "").Replace(@"""nb"":", "").Replace(@"""", "").Trim();
            return textoLimpo;
        }

        public static MemoryStream ConverterTextoStreamMemoria(string Texto)
        {
            MemoryStream textosConvertidos = new MemoryStream();

            byte[] ArrayBytes = System.Text.Encoding.UTF8.GetBytes(Texto);
            MemoryStream itemResultado = new MemoryStream(ArrayBytes);
            textosConvertidos = itemResultado;

            return textosConvertidos;
        }

        public string ConverterStreamMemoriaTexto(MemoryStream StreamMemoria)
        {
            string textosConvertidos;
            StreamReader streamTexto = new StreamReader(StreamMemoria);
            textosConvertidos = streamTexto.ReadToEnd();
            return textosConvertidos;
        }
        public ExtratoRV ProcessarResultadoRv(string resultadoBanco)
        {
            ExtratoRV extrato = new ExtratoRV();

            //Campos que já vieram na consulta titular

            //extrato.Matricula = resultadoBanco.Substring(resultadoBanco.IndexOf("NB") + 61, 10).Trim();
            //extrato.Nome = resultadoBanco.Substring(resultadoBanco.IndexOf("NB") + 73, 35).Trim();
            //extrato.Situacao= resultadoBanco.Substring(resultadoBanco.IndexOf("Situacao:") + 10, 7).Trim();

            if (!resultadoBanco.Equals("") /*&& !resultadoBanco.Equals("Erro de tentativas.")*/)
            {
                if (resultadoBanco.Substring(resultadoBanco.IndexOf("Nascimento:") + 12, 10).Trim().Equals(""))
                {
                    extrato.Nascimento = Convert.ToDateTime(resultadoBanco.Substring(resultadoBanco.IndexOf("Nascimento:") + 12, 10).Trim());
                }
                else
                {
                    extrato.Nascimento = null;
                }

                try
                {
                    extrato.Salario = Convert.ToDouble(resultadoBanco.Substring(resultadoBanco.IndexOf("MR:") + 3, 19).Trim().Replace(".", ""));
                }
                catch (FormatException)
                {
                    extrato.Salario = null;
                }
                if (!resultadoBanco.Substring(resultadoBanco.IndexOf("Cpt") + 4, 7).Trim().Equals(""))
                {
                    extrato.MesCompetencia = Convert.ToDateTime("01/" + resultadoBanco.Substring(resultadoBanco.IndexOf("Cpt") + 4, 7).Trim() + " 00:00:00");
                }

                if (!resultadoBanco.Substring(resultadoBanco.IndexOf("Cpt") + 17, 10).Trim().Equals(""))
                {
                    extrato.CompetenciaInicial = Convert.ToDateTime(resultadoBanco.Substring(resultadoBanco.IndexOf("Cpt") + 16, 10).Trim());
                }

                if (!resultadoBanco.Substring(resultadoBanco.IndexOf("Cpt") + 27, 10).Trim().Equals(""))
                {
                    extrato.CompetenciaFinal = Convert.ToDateTime(resultadoBanco.Substring(resultadoBanco.IndexOf("Cpt") + 27, 10).Trim());
                }

                extrato.CodigoAgencia = resultadoBanco.Substring(resultadoBanco.IndexOf("Vld") - 10, 10).Trim();

                if (!resultadoBanco.Substring(resultadoBanco.IndexOf("Vld") + 4, 10).Trim().Equals(""))
                {
                    extrato.ValidadeInicial = Convert.ToDateTime(resultadoBanco.Substring(resultadoBanco.IndexOf("Vld") + 4, 10).Trim());
                }

                if (!resultadoBanco.Substring(resultadoBanco.IndexOf("Vld") + 15, 10).Trim().Equals(""))
                {
                    extrato.ValidadeFinal = Convert.ToDateTime(resultadoBanco.Substring(resultadoBanco.IndexOf("Vld") + 15, 10).Trim());
                }

                if (!resultadoBanco.Substring(resultadoBanco.IndexOf("Banco") + 6, 6).Trim().Equals(""))
                {
                    extrato.Banco = Convert.ToInt16(resultadoBanco.Substring(resultadoBanco.IndexOf("Banco") + 6, 6).Trim());
                }

                if ((!resultadoBanco.Substring(resultadoBanco.IndexOf("CC:") + 4, 22).Trim().Equals("")) && (!ContemLetras(resultadoBanco.Substring(resultadoBanco.IndexOf("CC:") + 4, 22).Trim())))
                {
                    extrato.ContaCorrente = resultadoBanco.Substring(resultadoBanco.IndexOf("CC:") + 4, 22).Trim();
                }
                else
                {
                    extrato.ContaCorrente = "";
                }
                extrato.ValorLiquidoCredito = Convert.ToDouble(resultadoBanco.Substring(resultadoBanco.IndexOf(" Val. Liq. Credito R$") + 27, 9).Trim().Replace(".", ""));
                extrato.Rubricas = new List<Rubrica>();
                MemoryStream bytesDeLeitura = new MemoryStream();
                bytesDeLeitura = ConverterTextoStreamMemoria(resultadoBanco);
                StreamReader LinhaLeitura = new StreamReader(bytesDeLeitura);
                int contadorLinhas = 1;

                while (true)
                {
                    string textoLinha = LinhaLeitura.ReadLine();

                    if (textoLinha.StartsWith(" Val. Liq. Credito R$"))
                    {
                        break;
                    }

                    if (contadorLinhas > 12 && !textoLinha.StartsWith("   ") && textoLinha.StartsWith(" "))
                    {
                        if ((!textoLinha.Substring(1, 3).Trim().Equals("")) && (!ContemLetras(textoLinha.Substring(1, 3).Trim())))
                        {
                            Rubrica rubrica = new Rubrica();

                            rubrica.Codigo = Convert.ToInt16(textoLinha.Substring(1, 3));

                            rubrica.Nome = textoLinha.Substring(5, 22).Trim();

                            if (!textoLinha.Substring(27, 9).Trim().Equals("") && (!ContemLetras(textoLinha.Substring(27, 9).Trim())))
                            {
                                rubrica.Valor = Convert.ToDouble(textoLinha.Substring(27, 9).Replace(".", ""));
                            }

                            rubrica.Sinal = textoLinha.Substring(37, 1).Trim();

                            extrato.Rubricas.Add(rubrica);
                        }
                    }

                    contadorLinhas++;
                }
                return extrato;
            }
            else
            {
                //extrato.Banco = null;
                //extrato.CodigoAgencia = String.Empty;
                //extrato.CompetenciaFinal = null;
                //extrato.CompetenciaInicial = null;
                //extrato.ContaCorrente = String.Empty;
                //extrato.Matricula = String.Empty;
                //extrato.MesCompetencia = null;
                //extrato.Nascimento = null;
                //extrato.Nome = String.Empty;
                //extrato.Rubricas = null;
                //extrato.Salario = 0;
                //extrato.Situacao = String.Empty;
                //extrato.ValidadeFinal = null;
                //extrato.ValidadeInicial = null;
                //extrato.ValorLiquidoCredito = 0;
                return null;
            }
        }

        public static string CriarArquivoCSVDisco(int idSolicitacao, string nomeArquivo)
        {
            string ArquivoDownload = Path.Combine(Path.GetTempPath(), nomeArquivo);

            //Se o arquivo não existir, ele é criado
            if (!System.IO.File.Exists(ArquivoDownload))
            {
                string ArquivoTempGerado = new ConexaoDb2().RecuperarResultado(idSolicitacao, TipoRetorno.csv);

                //Renomeando o arquivo gerado
                System.IO.File.Move(ArquivoTempGerado, Path.Combine(Path.GetTempPath(), nomeArquivo));
            }

            return ArquivoDownload;
        }

        public ExtratoTitular ProcessarResultadoTitular(string LoteResultado)
        {
            ExtratoTitular extrato = new ExtratoTitular();
            extrato.Matricula = LoteResultado.Substring(LoteResultado.IndexOf("NB") + 61, 10).Trim();
            extrato.Situacao = LoteResultado.Substring(LoteResultado.IndexOf("Situacao:") + 9, 22).Trim();
            extrato.NomeTitular = LoteResultado.Substring(LoteResultado.IndexOf("Nome do Titular:") + 16, 42).Trim();
            extrato.NomeMae = LoteResultado.Substring(LoteResultado.IndexOf("Nome da Mae    :") + 16, 63).Trim();
            extrato.Cpf = LoteResultado.Substring(LoteResultado.IndexOf("CPF.  :") + 7, 23).Trim();
            extrato.Identidade = LoteResultado.Substring(LoteResultado.IndexOf("Ident.:") + 7, 15).Trim() + LoteResultado.Substring(LoteResultado.IndexOf("Ident.:") + 22, 2).Trim() + LoteResultado.Substring(LoteResultado.IndexOf("Ident.:") + 24, 2);
            extrato.MunicipioIdentidade = LoteResultado.Substring(LoteResultado.IndexOf("Municipio/UF :") + 15, 14).Trim();
            extrato.UfIdentidade = LoteResultado.Substring(LoteResultado.IndexOf("Municipio/UF :") + 42, 2).Trim();
            extrato.Sexo = LoteResultado.Substring(LoteResultado.IndexOf("Sexo         :") + 14, 34).Trim();
            try
            {
                extrato.Nascimento = Convert.ToDateTime(LoteResultado.Substring(LoteResultado.IndexOf("Nascimento   :") + 14, 11).Trim());
            }
            catch (FormatException)
            {
                extrato.Nascimento = null;
            }
            extrato.Endereco = LoteResultado.Substring(LoteResultado.IndexOf("Endereco :") + 10, 46).Trim();
            extrato.Cep = LoteResultado.Substring(LoteResultado.IndexOf("CEP.:") + 5, 18).Trim();
            extrato.Municipio = LoteResultado.Substring(LoteResultado.IndexOf("Municipio:") + 10, 46).Trim();
            extrato.Uf = LoteResultado.Substring(LoteResultado.IndexOf("UF. :") + 5, 18).Trim();
            extrato.Bairro = LoteResultado.Substring(LoteResultado.IndexOf("Bairro   :") + 10, 27).Trim();
            extrato.Tel = LoteResultado.Substring(LoteResultado.IndexOf("Tel.:") + 5, 14).Trim();
            string[] dddRamal = LoteResultado.Substring(LoteResultado.IndexOf("DDD/Ramal:") + 10, 14).Split('/');
            extrato.Ddd = dddRamal[0].Trim();
            extrato.Ramal = dddRamal[1].Trim();
            extrato.Email = LoteResultado.Substring(LoteResultado.IndexOf("E-mail   :") + 10, 62).Trim();
            return extrato;
        }

        public ExtratoConbas ProcessarResultadoConbas(string LoteResultado)
        {
            ExtratoConbas extrato = new ExtratoConbas();

            if (LoteResultado.IndexOf("Esp.:") != -1)
            {
                extrato.CodigoEspecie = Convert.ToInt16(LoteResultado.Substring(LoteResultado.IndexOf("Esp.:") + 6, 3).Trim());
                extrato.Especie = LoteResultado.Substring(LoteResultado.IndexOf("Esp.:") + 9, 39).Trim();
            }
            else
            {
                extrato.CodigoEspecie = null;
                extrato.Especie = null;
            }

            if (LoteResultado.IndexOf("Ramo atividade:") != -1)
            {
                extrato.CodigoRamoAtividade = Convert.ToInt16(LoteResultado.Substring(LoteResultado.IndexOf("Ramo atividade:") + 15, 3).Trim());
                extrato.RamoAtividade = LoteResultado.Substring(LoteResultado.IndexOf("Ramo atividade:") + 18, 30).Trim();
            }
            else
            {
                extrato.CodigoRamoAtividade = null;
                extrato.RamoAtividade = null;
            }
            if (LoteResultado.IndexOf("Forma Filiacao:") != -1)
            {
                extrato.CodigoFormaFiliacao = Convert.ToInt16(LoteResultado.Substring(LoteResultado.IndexOf("Forma Filiacao:") + 15, 3).Trim());
                extrato.FormaFiliacao = LoteResultado.Substring(LoteResultado.IndexOf("Forma Filiacao:") + 18, 30).Trim();
            }

            if (LoteResultado.IndexOf("DIB:") != -1)
            {
                extrato.DataAdmissao = Convert.ToDateTime(LoteResultado.Substring(LoteResultado.IndexOf("DIB:") + 5, 10).Trim());
            }
            else
            {
                extrato.DataAdmissao = null;
            }

            return extrato;
        }


        public static string CompactarResultado(string resultado)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(resultado);
            MemoryStream ms = new MemoryStream();
            using (GZipStream zip = new GZipStream(ms, CompressionMode.Compress, true))
            {
                zip.Write(buffer, 0, buffer.Length);
            }

            ms.Position = 0;
            MemoryStream StreamSaida = new MemoryStream();

            byte[] compactado = new byte[ms.Length];
            ms.Read(compactado, 0, compactado.Length);

            byte[] gzBuffer = new byte[compactado.Length + 4];
            System.Buffer.BlockCopy(compactado, 0, gzBuffer, 4, compactado.Length);
            System.Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gzBuffer, 0, 4);

            return Convert.ToBase64String(gzBuffer); ;
        }

        public static string DescompactarResultado(string resultadoCompactado)
        {
            byte[] gzBuffer = Convert.FromBase64String(resultadoCompactado);
            using (MemoryStream ms = new MemoryStream())
            {
                int msgLength = BitConverter.ToInt32(gzBuffer, 0);
                ms.Write(gzBuffer, 4, gzBuffer.Length - 4);

                byte[] buffer = new byte[msgLength];

                ms.Position = 0;
                using (GZipStream zip = new GZipStream(ms, CompressionMode.Decompress))
                {
                    zip.Read(buffer, 0, buffer.Length);
                }

                return Encoding.UTF8.GetString(buffer);
            }
        }

        public string GerarCsvResultado(NpgsqlDataReader ResultadosSolicitacao)
        {
            string caminho = Path.GetTempFileName();
            StreamWriter ArquivoTemp = new StreamWriter(caminho, true);
            ArquivoTemp.WriteLine(new String(' ', 2000));
            int maxRubricas = 0;
            StringBuilder cabecalhoRubricasrv = new StringBuilder();
            bool aindaHaDados = true;
            var regras = new ConexaoDb2().ObterRegraCalculoMargem();

            while (aindaHaDados)
            {
                bool gravou = !ResultadosSolicitacao.Read();
                aindaHaDados = !gravou;

                while (!gravou)
                {
                    //Esse bloco só existe para controle dos meus objetos referenciados

                    ExtratoTitular titular;
                    ExtratoRV rv;
                    ExtratoConbas conbas;
                    StringBuilder linha = new StringBuilder();

                    titular = ProcessarResultadoTitular(DescompactarResultado(ResultadosSolicitacao["resultado"].ToString()));
                    ResultadosSolicitacao.Read();

                    rv = ProcessarResultadoRv(DescompactarResultado(ResultadosSolicitacao["resultado"].ToString()));
                    rv.GetMargem(regras);
                    ResultadosSolicitacao.Read();

                    conbas = ProcessarResultadoConbas(DescompactarResultado(ResultadosSolicitacao["resultado"].ToString()));

                    if (rv.Rubricas.Count > maxRubricas)
                    {
                        maxRubricas = rv.Rubricas.Count;
                    }

                    ManipulaCsv gravarResultado = new ManipulaCsv();

                    //A quebra de linha está no extrato rv última linha, por isso na concatenação ele ficou por último
                    linha.Append(gravarResultado.GerarCsvTitular(titular) + gravarResultado.GerarCsvConbas(conbas) + gravarResultado.GerarCsvRv(rv));

                    // Gravando o arquivo em disco
                    try
                    {
                        ArquivoTemp.Write(linha.ToString());
                    }

                    catch (Exception)
                    {
                        throw new Exception("Erro ao gerar o arquivo (Fluxo de resultados congestionado). Entre em contato com o administrador do site.");
                    }

                    gravou = true;
                }
            }

            ArquivoTemp.Close();

            for (int i = 1; i < maxRubricas + 1; i++)
            {
                cabecalhoRubricasrv.Append("Codigo Rubrica [" + i.ToString() + "];");
                cabecalhoRubricasrv.Append("Nome Rubrica [" + i.ToString() + "];");
                cabecalhoRubricasrv.Append("Valor Rubrica [" + i.ToString() + "];");
                cabecalhoRubricasrv.Append("Sinal Rubrica [" + i.ToString() + "];");
            }

            //Cabeçalho do arquivo
            string cabecalho = "MATRICULA;SITUACAO;NOME TITULAR;NOME MAE;CPF;IDENTIDADE;MUNICIPIO DA IDENTIDADE;UF DA IDENTIDADE;SEXO;DATA DE NASCIMENTO;ENDERECO;CEP;MUNICIPIO;UF;BAIRRO;DDD;RAMAL;FONE;EMAIL;CODIGO ESPECIE;ESPECIE;CODIGO RAMO ATIVIDADE;RAMO ATIVIDADE;CODIGO FORMA FILIACAO;FORMA FILIACAO;DATA DE ADMISSAO;SALARIO;MES COMPETENCIA;COMPETENCIA INICIAL;COMPETENCIA FINAL;CODIGO AGENCIA;VALIDADE INICIAL;VALIDADE FINAL;BANCO;CONTA CORRENTE;VALOR LIQUIDO CREDITO;MARGEM;" + cabecalhoRubricasrv.ToString();

            FileStream arquivo = new FileStream(caminho, FileMode.Open, FileAccess.Write);
            arquivo.Seek(0, SeekOrigin.Begin);
            arquivo.Write(Encoding.UTF8.GetBytes(cabecalho), 0, cabecalho.Length);
            arquivo.Close();

            return caminho;
        }

        public string GeraXmlResultado(NpgsqlDataReader ResultadosSolicitacao)
        {
            string caminho = Path.GetTempFileName();
            bool aindaHaDados = true;
            StreamWriter ArquivoTemp = new StreamWriter(caminho, true);

            //var resultado = ResultadosSolicitacao.Tables[0].Rows;
            var regras = new ConexaoDb2().ObterRegraCalculoMargem();
            ArquivoTemp.WriteLine("<?xml version=\"1.0\" encoding=\"utf-16\"?>");
            ArquivoTemp.WriteLine("<ExtratoBeneficio xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">");

            while (aindaHaDados)
            {
                bool gravou = !ResultadosSolicitacao.Read();
                aindaHaDados = !gravou;

                while (!gravou)
                {
                    ExtratoTitular titular;
                    ExtratoRV rv;
                    ExtratoConbas conbas;

                    titular = ProcessarResultadoTitular(DescompactarResultado(ResultadosSolicitacao["resultado"].ToString()));
                    ResultadosSolicitacao.Read();

                    rv = ProcessarResultadoRv(DescompactarResultado(ResultadosSolicitacao["resultado"].ToString()));
                    rv.GetMargem(regras);
                    ResultadosSolicitacao.Read();

                    conbas = ProcessarResultadoConbas(DescompactarResultado(ResultadosSolicitacao["resultado"].ToString()));
                    ExtratoBeneficio eb = new ExtratoBeneficio() { nb = titular.Matricula, Titular = titular, Rv = rv, Conbas = conbas };

                    string elemento = ToXML<ExtratoBeneficio>(eb);
                    ArquivoTemp.WriteLine("<Beneficio>");
                    ArquivoTemp.Write(elemento.Substring(162, elemento.Length - 181));
                    ArquivoTemp.WriteLine("</Beneficio>");
                    gravou = true;
                }
            }
            ArquivoTemp.WriteLine("</ExtratoBeneficio>");
            ArquivoTemp.Close();

            return caminho;
        }

        public string ToXML<T>(T obj)
        {
            using (StringWriter stringWriter = new StringWriter(new StringBuilder()))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                xmlSerializer.Serialize(stringWriter, obj);
                return stringWriter.ToString();
            }
        }

        public string GerarArquivoErro(System.Data.DataSet erros)
        {
            string file = string.Empty;
            StringBuilder linha = new StringBuilder();
            int cont = 0;
            var ResultadoErro = erros.Tables[0].Rows;

            while (cont < ResultadoErro.Count)
            {
                linha.Append(ResultadoErro[cont][0].ToString() + ";" + ResultadoErro[cont][1].ToString() + "\n");
                cont++;
            }

            linha.Insert(0, "MATRICULA; ERRO \n");
            file = linha.ToString();

            return file;
        }

        public List<XmlErros> GerarErrosXML(System.Data.DataSet erros)
        {
            int cont = 0;
            var ResultadoErro = erros.Tables[0].Rows;
            List<XmlErros> errosXml = new List<XmlErros>();

            while (cont < ResultadoErro.Count)
            {
                errosXml.Add(new XmlErros() { nb = ResultadoErro[cont][0].ToString(), erro = ResultadoErro[cont][1].ToString() });
                cont++;
            }

            return errosXml;
        }

    }
}