using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Npgsql;
using System.Data;

namespace RestServer.Models
{
    public enum TipoRetorno { csv, xml }

    public class ConexaoDb2
    {
        //Produção
        static string serverName = "postgresql02.agilus.com.br";
        static string port = "5432";
        static string userName = "agilus12";
        static string password = "post168421";
        static string databaseName = "agilus12";
        public string connectionString = String.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};", serverName, port, userName, password, databaseName);
        public NpgsqlConnection conexao;


        public int VerificarAcessoUsuario(string usuario, string Senha, string ip)
        {
            int idUsuario;

            using (NpgsqlConnection conexao = new NpgsqlConnection(this.connectionString))
            {
                string consultaUsuarioDb = "select id,habilitado,ip from usuario where nome=@nome and senha=@senha";
                NpgsqlCommand cmd = new NpgsqlCommand(consultaUsuarioDb, conexao);
                cmd.Parameters.AddWithValue("@nome", usuario);
                cmd.Parameters.AddWithValue("@senha", Senha);

                conexao.Open();

                NpgsqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    if (!Convert.ToBoolean(dr["habilitado"]))
                    {
                        throw new Exception("O usuário " + char.ToUpper(usuario[0]) + usuario.Substring(1) + " já possui cadastro, mas ainda não foi habilitado. Entre em contato com o suporte pelo telefone (11)3266-3124 e solicite o desbloqueio.");
                    }

                    if (!string.IsNullOrEmpty(dr["ip"].ToString()))
                    {
                        Servidor.VerificarIP(ip, dr["ip"].ToString()); //Verifica se o ip de post do usuário é o mesmo que está cadastrado no banco de dados Alvo
                    }
                }

                else
                {
                    throw new Exception("Usuário ou senha inválidos.");
                }

                idUsuario = (int)dr["id"];
            }

            return idUsuario;
        }

        public void CriarUsuario(Usuario novoUsuario)
        {
            using (NpgsqlConnection conexao = new NpgsqlConnection(this.connectionString))
            {
                string inserirUsuarioBd = "insert into usuario(nome,senha,email) values(@nome,@senha,@email)";
                NpgsqlCommand cmd = new NpgsqlCommand(inserirUsuarioBd, conexao);
                int inseriu;

                cmd.Parameters.AddWithValue("@nome", novoUsuario.Nome);
                cmd.Parameters.AddWithValue("@senha", novoUsuario.Senha);
                cmd.Parameters.AddWithValue("@email", novoUsuario.Email);
                conexao.Open();

                try
                {
                    inseriu = cmd.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    if (e.Message.Contains(@"violates unique constraint ""i_nome"""))
                    {
                        throw new Exception("Já existe um usuário cadastrado com este nome. Favor usar outro nome para fazer o cadastro.");
                    }

                    if (e.Message.Contains(@"violates unique constraint ""i_email"""))
                    {
                        throw new Exception("Já existe um usuário cadastrado com este email. Favor usar outro email para fazer o cadastro.");
                    }

                    else
                    {
                        throw new Exception(e.Message);
                    }
                }

                if (inseriu == 0)
                {
                    throw new Exception("Não foi possível criar o usuário. Entre em contato com o administrador.");
                }
            }
        }

        public void ExcluirUsuario(int idUsuario)
        {
            using (NpgsqlConnection conexao = new NpgsqlConnection(this.connectionString))
            {
                string deletarUsuarioDb = "delete from usuario where id= @idUsuario";
                NpgsqlCommand cmd = new NpgsqlCommand(deletarUsuarioDb, conexao);
                cmd.Parameters.AddWithValue("@idUsuario", idUsuario);

                conexao.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void AlterarUsuario(Usuario usuario, int idUsuario)
        {
            using (NpgsqlConnection conexao = new NpgsqlConnection(this.connectionString))
            {
                string alterarUsuarioDb = "update usuario set nome=@nome, email=@email where id=@id";
                NpgsqlCommand cmd = new NpgsqlCommand(alterarUsuarioDb, conexao);
                cmd.Parameters.AddWithValue("@nome", usuario.Nome);
                cmd.Parameters.AddWithValue("@email", usuario.Email);
                cmd.Parameters.AddWithValue("@id", idUsuario);
                conexao.Open();

                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    if (e.Message.Contains(@"for key 'i_nome'"))
                    {
                        throw new Exception("Já existe um usuário cadastrado com este nome. Favor usar outro nome para fazer o cadastro.");
                    }

                    if (e.Message.Contains(@"for key 'i_email'"))
                    {
                        throw new Exception("Já existe um usuário cadastrado com este email. Favor usar outro email para fazer o cadastro.");
                    }

                    else
                    {
                        throw new Exception(e.Message);
                    }
                }
            }
        }

        public Usuario RecuperarDadosUsuario(int idUsuario)
        {
            Usuario usuario = new Usuario();

            using (NpgsqlConnection conexao = new NpgsqlConnection(this.connectionString))
            {
                string consultarDadosUsuarioDb = "select nome,email,senha from usuario where id=@idUsuario";
                NpgsqlCommand cmd = new NpgsqlCommand(consultarDadosUsuarioDb, conexao);
                cmd.Parameters.AddWithValue("@idUsuario", idUsuario);
                conexao.Open();
                NpgsqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    usuario.Nome = dr["nome"].ToString();
                    usuario.Email = dr["email"].ToString();
                    usuario.Senha = dr["senha"].ToString();
                }
            }

            return usuario;
        }

        public void InserirConsulta(Consulta consulta, int idSolicitacao)
        {
            using (NpgsqlConnection conexao = new NpgsqlConnection(this.connectionString))
            {
                string inserirConsultaDb = "insert into consulta(id_solicitacao,data_consulta,chave,id_status_processo,id_tipo_consulta) values(@idSolicitacao,@DataConsulta,@Chave,@StatusProcesso,@tipoConsulta)";

                NpgsqlCommand cmd = new NpgsqlCommand(inserirConsultaDb, conexao);

                NpgsqlParameter id = new NpgsqlParameter("@idSolicitacao", NpgsqlTypes.NpgsqlDbType.Integer);
                id.Value = idSolicitacao;
                cmd.Parameters.Add(id);

                NpgsqlParameter dataConsulta = new NpgsqlParameter("@DataConsulta", NpgsqlTypes.NpgsqlDbType.Timestamp);
                dataConsulta.Value = consulta.DataConsulta;
                cmd.Parameters.Add(dataConsulta);

                NpgsqlParameter Chave = new NpgsqlParameter("@Chave", NpgsqlTypes.NpgsqlDbType.Varchar);
                Chave.Value = consulta.Chave;
                cmd.Parameters.Add(Chave);

                NpgsqlParameter StatusProcesso = new NpgsqlParameter("@StatusProcesso", NpgsqlTypes.NpgsqlDbType.Integer);
                StatusProcesso.Value = Convert.ToInt16(consulta.StatusProcesso);
                cmd.Parameters.Add(StatusProcesso);

                NpgsqlParameter tipoConsulta = new NpgsqlParameter("@tipoConsulta", NpgsqlTypes.NpgsqlDbType.Integer);
                tipoConsulta.Value = Convert.ToInt16(consulta.tipoConsultaRealizada);
                cmd.Parameters.Add(tipoConsulta);

                conexao.Open();

                cmd.ExecuteNonQuery();
            }
        }

        public List<Consulta> ListarConsultasSolicitacao(int idSolicitacao)
        {
            // Esse método é usado para obter o id e chave das consultas. A chave é necessária para consultar no servidor o status da consulta
            // e o id será usado posteriormente para atualização dos status na tabela consulta.

            List<Consulta> ConsultasSolicitadas = new List<Consulta>();

            using (NpgsqlConnection conexao = new NpgsqlConnection(this.connectionString))
            {
                string ObterConsultas = @"create temporary table lista_consulta as
                                        select id, chave, data_consulta, id_status_processo, data_processado, id_tipo_consulta
                                        from consulta where id_solicitacao= @id and id_status_processo < 3 and atualizando_status='0'
                                        order by id;

                                        update consulta as c
                                        set atualizando_status = '1'
                                        from lista_consulta lc
                                        where c.id = lc.id;

                                        select *
                                        from lista_consulta;";

                NpgsqlCommand cmd = new NpgsqlCommand(ObterConsultas, conexao);
                cmd.Parameters.AddWithValue("@id", idSolicitacao);

                conexao.Open();

                NpgsqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    Consulta c = new Consulta();

                    c.Id = Convert.ToInt16(dr[0]);
                    c.Chave = dr[1].ToString();
                    c.DataConsulta = Convert.ToDateTime(dr[2]);
                    c.StatusProcesso = (TipoProcesso)Convert.ToInt16(dr[3]);

                    if (dr[4].Equals(DBNull.Value))
                    {
                        c.DataProcessado = null;
                    }
                    else
                    {
                        c.DataProcessado = (DateTime?)dr[4];
                    }

                    c.tipoConsultaRealizada = (Contexto)Convert.ToInt16(dr[5]);

                    ConsultasSolicitadas.Add(c);
                }
            }

            return ConsultasSolicitadas;
        }

        public List<int> ListarIdConsultasSolicitadas(int idSolicitacao)
        {
            // Esse método é usado para obter o id e chave das consultas. A chave é necessária para consultar no servidor o status da consulta
            // e o id será usado posteriormente para atualização dos status na tabela consulta.

            List<int> idConsultasSolicitadas = new List<int>();

            using (NpgsqlConnection conexao = new NpgsqlConnection(this.connectionString))
            {
                string ObterConsultas = "select id from consulta where id_solicitacao=@Id order by id";
                NpgsqlCommand cmd = new NpgsqlCommand(ObterConsultas, conexao);
                cmd.Parameters.AddWithValue("@Id", idSolicitacao);

                conexao.Open();

                NpgsqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    idConsultasSolicitadas.Add(Convert.ToInt16(dr["id"]));
                }
            }

            return idConsultasSolicitadas;
        }

        public void AtualizarConsulta(Consulta consultaAtualizada)
        {
            using (NpgsqlConnection conexao = new NpgsqlConnection(this.connectionString))
            {
                string alterarConsultaDb = "update consulta set id_status_processo= @StatusProcesso, data_processado=@DataProcessado, atualizando_status='0' where chave=@Chave;";
                NpgsqlCommand cmd = new NpgsqlCommand(alterarConsultaDb, conexao);
                cmd.Parameters.AddWithValue("@StatusProcesso", Convert.ToInt16(consultaAtualizada.StatusProcesso));
                cmd.Parameters.AddWithValue("@DataProcessado", consultaAtualizada.DataProcessado);
                cmd.Parameters.AddWithValue("@Chave", consultaAtualizada.Chave);

                conexao.Open();

                cmd.ExecuteNonQuery();
            }
        }
        public bool VerificarConsultaProcessada(int? idConsulta)
        {
            bool processado;
            //Trazer descrição da tabela status processo
            using (NpgsqlConnection conexao = new NpgsqlConnection(this.connectionString))
            {
                string consultaBd = "select sp.descricao as status_processo from consulta c join status_processo sp on c.id_status_processo = sp.id  where c.id = @idConsulta;";
                NpgsqlCommand cmd = new NpgsqlCommand(consultaBd, conexao);
                cmd.Parameters.AddWithValue("@idConsulta", idConsulta);
                conexao.Open();

                string statusProcesso = cmd.ExecuteScalar().ToString();

                //Se a coleta foi ou está sendo realizada, a coleta do mesmo lote deve ser evitada, por isso o retorno do método tem que ser "true"
                processado = (statusProcesso == "processado");
            }

            return processado;
        }

        public void inserirResultado(Dictionary<string, string> ResultadoMatricula, int idConsulta, int idSolicitacao, int statusItem)
        {
            Parser parser = new Parser();
            string erro = String.Empty;

            using (NpgsqlConnection conexao = new NpgsqlConnection(this.connectionString))
            {
                string inserirResultadoDb = "insert into resultado(id_consulta, nb, resultado, contem_erro) values(@idConsulta, @beneficio, @Resultado, @erro)";

                NpgsqlCommand cmd = new NpgsqlCommand(inserirResultadoDb, conexao);

                cmd.Parameters.AddWithValue("@idConsulta", idConsulta);
                cmd.Parameters.Add("@beneficio", NpgsqlTypes.NpgsqlDbType.Varchar);
                cmd.Parameters.Add("@Resultado", NpgsqlTypes.NpgsqlDbType.Varchar);
                cmd.Parameters.Add("@erro", NpgsqlTypes.NpgsqlDbType.Bit);

                conexao.Open();

                foreach (var item in ResultadoMatricula)
                {
                    if (statusItem == 1 || statusItem == 0)
                    {
                        int tamanhoResultado;

                        cmd.Parameters[1].Value = item.Key;

                        //Avaliar valor, se nulo dentro destes 2 status (0,1) = erro de tentativas, senão valor do resultado.
                        if (item.Value != null)
                        {
                            cmd.Parameters[2].Value = Parser.CompactarResultado(item.Value);
                            tamanhoResultado = item.Value.Length;
                        }
                        else
                        {
                            erro = "Erro de tentativas.";
                            cmd.Parameters[3].Value = 1;
                            cmd.Parameters[2].Value = Parser.CompactarResultado(erro);
                            tamanhoResultado = erro.Length;
                            inserirErroDb(conexao, idConsulta, idSolicitacao, item.Key, erro);
                        }

                        if (!erro.Equals("Erro de tentativas.") && item.Value.Contains("<html>") && !item.Value.Contains("</html>"))
                        {
                            erro = "Erro dataprev: Consulta incompleta.";
                            cmd.Parameters[3].Value = 1;
                            tamanhoResultado = erro.Length;
                            inserirErroDb(conexao, idConsulta, idSolicitacao, item.Key, erro);
                        }

                        //Avaliando tamanho, se o tamanho do elemento menor de 500, considera como erro de consulta (resultado com texto que veio na consulta).
                        if (tamanhoResultado < 500 && erro.Equals(String.Empty))
                        {
                            cmd.Parameters[3].Value = 1;
                            erro = "Numero de caractares insuficiente para formar o resultado.";
                            inserirErroDb(conexao, idConsulta, idSolicitacao, item.Key, erro);
                        }

                        if (erro.Equals(String.Empty))
                        {
                            cmd.Parameters[3].Value = 0;
                        }

                        cmd.ExecuteNonQuery();
                    }
                    else if (statusItem == 2 || statusItem == 3)
                    {
                        erro = "Erro de tentativas.";
                        inserirErroDb(conexao, idConsulta, idSolicitacao, item.Key, erro);
                        cmd.Parameters[1].Value = item.Key;
                        cmd.Parameters[2].Value = Parser.CompactarResultado(erro);
                        cmd.Parameters[3].Value = 1;

                        cmd.ExecuteNonQuery();
                    }

                    else if (statusItem == 4)
                    {
                        erro = "NB invalido.";
                        inserirErroDb(conexao, idConsulta, idSolicitacao, item.Key, erro);
                        cmd.Parameters[1].Value = item.Key;
                        cmd.Parameters[2].Value = Parser.CompactarResultado(erro);
                        cmd.Parameters[3].Value = 1;

                        cmd.ExecuteNonQuery();
                    }

                    erro = String.Empty;
                }

                cmd = null;

                //Atualizando todas consultas para com erro. Caso alguma delas dê erro, considera-se todas erradas.
                string atualizarMatriculasErro = @"UPDATE resultado SET contem_erro= '1' WHERE id in (select r.id from resultado r INNER JOIN consulta c ON r.id_consulta = c.id INNER JOIN resultado r2 ON r.nb = r2.nb INNER JOIN consulta c2 on r2.id_consulta = c2.id WHERE r.contem_erro='0' and r2.contem_erro='1' and c.id_solicitacao = @id_solicitacao and c2.id_solicitacao = @id_solicitacao)";
                cmd = new NpgsqlCommand(atualizarMatriculasErro, conexao);
                cmd.Parameters.AddWithValue("@id_solicitacao", idSolicitacao);

                cmd.CommandTimeout = 120000;//2 minutos, aumentei o timeout de resposta, pois estava estourando na hora de atualizar os erros da tabela resultado.
                cmd.ExecuteNonQuery();

                cmd = null;

                string alterarConsultaDb = "update consulta set id_status_processo= 3, atualizando_status='0' where id=@id_consulta";
                cmd = new NpgsqlCommand(alterarConsultaDb, conexao);
                cmd.Parameters.AddWithValue("@id_consulta", idConsulta);

                cmd.ExecuteNonQuery();
            }
        }

        private void inserirErroDb(NpgsqlConnection conexao, int idConsulta, int id_solicitacao, string beneficio, string erro)
        {
            string inserirErrosDB = "insert into erros_consulta(id_consulta, nb, descricao) select @idConsulta, @beneficio, @erro where not exists(select 1 from erros_consulta ec join consulta c on ec.id_consulta = c.id join solicitacao s on c.id_solicitacao = s.id where s.id = @idSolicitacao and nb = @beneficio)";

            NpgsqlCommand cmdErro = new NpgsqlCommand(inserirErrosDB, conexao);
            cmdErro.Parameters.AddWithValue("@idConsulta", idConsulta);
            cmdErro.Parameters.Add("@beneficio", NpgsqlTypes.NpgsqlDbType.Varchar);
            cmdErro.Parameters.Add("@erro", NpgsqlTypes.NpgsqlDbType.Varchar);
            cmdErro.Parameters.AddWithValue("@idSolicitacao", id_solicitacao);

            cmdErro.Parameters[1].Value = beneficio;
            cmdErro.Parameters[2].Value = erro;

            cmdErro.ExecuteNonQuery();

        }

        public string RecuperarResultado(int idSolicitacao, TipoRetorno tipoRetorno)
        {
            string resultado = String.Empty;

            using (NpgsqlConnection conexao = new NpgsqlConnection(this.connectionString))
            {
                string consultarDadosResultadoDb = @"select distinct c.id_tipo_consulta as tipo_consulta, r.nb,r.resultado from resultado r join consulta c on id_consulta = c.id where c.id_solicitacao = @id_solicitacao and contem_erro <> '1' order by nb,tipo_consulta;";
                NpgsqlCommand cmd = new NpgsqlCommand(consultarDadosResultadoDb, conexao);
                cmd.Parameters.AddWithValue("@id_solicitacao", idSolicitacao);
                cmd.CommandTimeout = 0;
                conexao.Open();

                if (tipoRetorno == TipoRetorno.csv)
                {
                    resultado = new Parser().GerarCsvResultado(cmd.ExecuteReader());
                }
                else
                {
                    resultado = new Parser().GeraXmlResultado(cmd.ExecuteReader());
                }
            }
            return resultado;
        }

        public int InserirSolicitacao(int idUsuario, string DescricaoSolicitacao)
        {
            int solicitacao;

            using (NpgsqlConnection conexao = new NpgsqlConnection(this.connectionString))
            {
                string inserirConsultaDb = "insert into solicitacao(id_usuario,descricao,id_status_solicitacao,data_solicitacao) values(@idUsuario,@descricao,1,now())";

                NpgsqlCommand cmd = new NpgsqlCommand(inserirConsultaDb, conexao);

                NpgsqlParameter id = new NpgsqlParameter("@idUsuario", NpgsqlTypes.NpgsqlDbType.Integer);
                id.Value = idUsuario;
                cmd.Parameters.Add(id);

                NpgsqlParameter descricao = new NpgsqlParameter("@descricao", NpgsqlTypes.NpgsqlDbType.Varchar);
                descricao.Value = DescricaoSolicitacao;
                cmd.Parameters.Add(descricao);

                conexao.Open();

                cmd.ExecuteNonQuery();

                //Obtendo o id da solicitação inserida   
                solicitacao = Convert.ToInt16(new NpgsqlCommand("SELECT lastval()", conexao).ExecuteScalar());
            }

            return solicitacao;
        }

        public int VerificarSaldo(int idUsuario)
        {
            int saldo;

            using (NpgsqlConnection conexao = new NpgsqlConnection(this.connectionString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("select fn_verifica_saldo(@idUsuario)", conexao);
                cmd.CommandType = CommandType.Text;

                NpgsqlParameter id = new NpgsqlParameter("@idUsuario", NpgsqlTypes.NpgsqlDbType.Integer);
                id.Value = idUsuario;
                cmd.Parameters.Add(id);

                conexao.Open();

                string validaRetorno = cmd.ExecuteScalar().ToString();

                if (string.IsNullOrEmpty(validaRetorno))
                {
                    throw new Exception("O período de validade para efetuar consultas expirou.");
                }
                saldo = Convert.ToInt32(validaRetorno);
            }

            return saldo;
        }

        public bool AtualizarStatusSolicitacao()
        {
            bool atualizou = false;

            using (NpgsqlConnection conexao = new NpgsqlConnection(this.connectionString))
            {
                int linhasAfetadas = 0;
                string alterarStatusDb = "update solicitacao s set id_status_solicitacao= 2, data_conclusao_solicitacao = now() where not exists(select 1 from consulta c where c.id_solicitacao= s.id and (c.id_status_processo < 3 or c.id_status_processo=4)) and data_conclusao_solicitacao is null";
                NpgsqlCommand cmd = new NpgsqlCommand(alterarStatusDb, conexao);

                conexao.Open();
                linhasAfetadas = cmd.ExecuteNonQuery();
                atualizou = (linhasAfetadas > 0);
            }
            return atualizou;
        }

        public int IdUsuarioSolicitacao(int idSolicitacao)
        {
            int idUsuario;

            using (NpgsqlConnection conexao = new NpgsqlConnection(this.connectionString))
            {
                string comando = "select u.id from usuario u join solicitacao s on u.id = s.id_usuario where s.id=@id_solicitacao";

                NpgsqlCommand cmd = new NpgsqlCommand(comando, conexao);
                cmd.Parameters.AddWithValue("@id_solicitacao", idSolicitacao);
                conexao.Open();

                idUsuario = Convert.ToInt16(cmd.ExecuteScalar());
            }

            return idUsuario;
        }

        public List<Solicitacao> ConsultarSolicitacoes(int idUsuario)
        {
            //Verifica no banco de dados as solicitações de lote realizadas e retorna a lista de solicitações do usuário

            List<Solicitacao> Solicitacoes = new List<Solicitacao>();

            using (NpgsqlConnection conexao = new NpgsqlConnection(this.connectionString))
            {
                string solicitacaoBd = "select id,descricao,id_status_solicitacao, data_solicitacao, data_conclusao_solicitacao from solicitacao where id_usuario = @id_usuario order by id";
                NpgsqlCommand cmd = new NpgsqlCommand(solicitacaoBd, conexao);
                cmd.Parameters.AddWithValue("@id_usuario", idUsuario);
                conexao.Open();

                NpgsqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    Solicitacao itensSolicitacao = new Solicitacao();
                    itensSolicitacao.Id = Convert.ToInt16(dr["id"]);
                    if (dr["descricao"].Equals(DBNull.Value))
                    {
                        itensSolicitacao.Descricao = "Consulta sem nome";
                    }
                    else
                    {
                        itensSolicitacao.Descricao = dr["descricao"].ToString();
                    }
                    itensSolicitacao.status = (StatusSolicitacao)Convert.ToInt16(dr["id_status_solicitacao"]);
                    itensSolicitacao.DataSolicitacao = Convert.ToDateTime(dr["data_solicitacao"]);
                    itensSolicitacao.ErrosSolicitacao = ContemErros(Convert.ToInt16(dr["id"]));

                    if (dr["data_conclusao_solicitacao"].Equals(DBNull.Value))
                    {
                        itensSolicitacao.DataConclusaoSolicitacao = null;
                    }
                    else
                    {
                        itensSolicitacao.DataConclusaoSolicitacao = Convert.ToDateTime(dr["data_conclusao_solicitacao"]);
                    }

                    Solicitacoes.Add(itensSolicitacao);
                }
            }

            return Solicitacoes;
        }

        public bool SolicitacaoColetada(int idSolicitacao)
        {
            bool coletaRealizada = false;

            using (NpgsqlConnection conexao = new NpgsqlConnection(this.connectionString))
            {
                string coleta = "select id_status_solicitacao from solicitacao where id=@id";
                NpgsqlCommand cmd = new NpgsqlCommand(coleta, conexao);
                cmd.Parameters.AddWithValue("@id", idSolicitacao);
                conexao.Open();

                int status_solicitacao = Convert.ToInt16(cmd.ExecuteScalar());

                coletaRealizada = ((StatusSolicitacao)status_solicitacao == StatusSolicitacao.Download);
            }

            return coletaRealizada;
        }

        private bool ContemErros(int idSolicitacao)
        {
            bool ContemErros = false;

            using (NpgsqlConnection conexao = new NpgsqlConnection(this.connectionString))
            {
                string coleta = @"select count(*) from solicitacao s join consulta c on s.id = c.id_solicitacao where s.id= @idSolicitacao and exists(select 1 from erros_consulta r where r.id_consulta = c.id)";
                NpgsqlCommand cmd = new NpgsqlCommand(coleta, conexao);
                cmd.Parameters.AddWithValue("@idSolicitacao", idSolicitacao);
                conexao.Open();

                int numRegistrosErro = Convert.ToInt16(cmd.ExecuteScalar());

                if (numRegistrosErro > 0)
                {
                    ContemErros = true;
                }
            }

            return ContemErros;
        }

        public bool ExisteSolicitacaoPendente()
        {
            bool pendente = false;

            using (NpgsqlConnection conexao = new NpgsqlConnection(this.connectionString))
            {
                string solicitacaoPendente = "select count(*) from solicitacao where id_status_solicitacao < 2";
                NpgsqlCommand cmd = new NpgsqlCommand(solicitacaoPendente, conexao);
                conexao.Open();

                int numeroSolicitacoes = Convert.ToInt16(cmd.ExecuteScalar());

                if (numeroSolicitacoes > 0)
                {
                    pendente = true;
                }
            }
            return pendente;
        }

        public DataSet RecuperarErro(int idSolicitacao)
        {
            DataSet ds = new DataSet();

            using (NpgsqlConnection conexao = new NpgsqlConnection(this.connectionString))
            {
                string consultarDadosResultadoDb = @"select e.nb,e.descricao  from erros_consulta e  join consulta c on id_consulta = c.id  where c.id_solicitacao = @idSolicitacao order by nb,id_tipo_consulta";
                NpgsqlDataAdapter da = new NpgsqlDataAdapter(consultarDadosResultadoDb, conexao);
                da.SelectCommand.Parameters.Add(new NpgsqlParameter("@idSolicitacao", idSolicitacao));

                conexao.Open();

                da.Fill(ds);
            }
            return ds;
        }

        public List<Solicitacao> ConsultarSolicitacoesPendentes()
        {
            List<Solicitacao> solicitacoes = new List<Solicitacao>();

            using (NpgsqlConnection conexao = new NpgsqlConnection(this.connectionString))
            {
                string consultarSolicitacoesDb = @"select id, descricao,id_usuario,id_status_solicitacao, data_solicitacao 
                                                from solicitacao as s
                                                where id_status_solicitacao = 1
                                                and exists(select 1
	                                                    from consulta
	                                                    where id_solicitacao = s.id
	                                                    and atualizando_status='0'
	                                                    and id_status_processo < 3);";

                NpgsqlCommand cmd = new NpgsqlCommand(consultarSolicitacoesDb, conexao);
                conexao.Open();
                NpgsqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    Solicitacao s = new Solicitacao();
                    s.Id = Convert.ToInt16(dr["id"]);
                    s.Descricao = dr["descricao"].ToString();
                    s.status = (StatusSolicitacao)Convert.ToInt16((dr["id_status_solicitacao"]));
                    s.DataSolicitacao = Convert.ToDateTime(dr["data_solicitacao"]);

                    solicitacoes.Add(s);
                }
            }
            return solicitacoes;
        }

        public List<RegraCalculoMargem> ObterRegraCalculoMargem()
        {
            List<RegraCalculoMargem> regras = new List<RegraCalculoMargem>();

            using (NpgsqlConnection conexao = new NpgsqlConnection(this.connectionString))
            {
                string ObterDados = "select * from rubrica_dataprev";
                NpgsqlCommand cmd = new NpgsqlCommand(ObterDados, conexao);
                conexao.Open();
                NpgsqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    RegraCalculoMargem consultaRegra = new RegraCalculoMargem();

                    consultaRegra.Id = Convert.ToInt16(dr[0]);
                    consultaRegra.Descricao = dr[1].ToString();
                    consultaRegra.Sinal = dr[2].ToString(); ;
                    consultaRegra.PreCalculo = Convert.ToBoolean(dr[4]);
                    consultaRegra.PosCalculo = Convert.ToBoolean(dr[5]);
                    consultaRegra.CartaoRmc = Convert.ToBoolean(dr[6]);

                    regras.Add(consultaRegra);
                }
            }

            return regras;
        }


        public void AlterarSenha(Usuario usuario, int idUsuario)
        {
            using (NpgsqlConnection conexao = new NpgsqlConnection(this.connectionString))
            {
                string alterarSenhaUsuarioDb = "update usuario set senha=@senha where id=@id";
                NpgsqlCommand cmd = new NpgsqlCommand(alterarSenhaUsuarioDb, conexao);
                cmd.Parameters.AddWithValue("@senha", usuario.Senha);
                cmd.Parameters.AddWithValue("@id", idUsuario);
                conexao.Open();

                try
                {
                    cmd.ExecuteNonQuery();
                }

                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }
        }

        public void InserirExtrato(int idUsuario, int idSolicitacao, int qtdeConsultas)
        {
            using (NpgsqlConnection conexao = new NpgsqlConnection(this.connectionString))
            {
                string inserirExtratoDB = "insert into extrato(id_usuario, id_solicitacao, qtde_consultas) select @idUsuario, @idSolicitacao, -@qtdeConsultas";

                NpgsqlCommand cmdExtrato = new NpgsqlCommand(inserirExtratoDB, conexao);
                cmdExtrato.Parameters.AddWithValue("@idUsuario", idUsuario);
                cmdExtrato.Parameters.AddWithValue("@idSolicitacao", idSolicitacao);
                cmdExtrato.Parameters.AddWithValue("@qtdeConsultas", qtdeConsultas);
                conexao.Open();

                cmdExtrato.ExecuteNonQuery();
            }
        }

        public void LiberarAtualizandoStatus(int idSolicitacao)
        {
            using (NpgsqlConnection conexao = new NpgsqlConnection(this.connectionString))
            {
                string sql = "update consulta set atualizando_status = '0' where id_solicitacao= @id_solicitacao";
                NpgsqlCommand cmd = new NpgsqlCommand(sql, conexao);

                cmd.Parameters.AddWithValue("@id_solicitacao", idSolicitacao);
                conexao.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}