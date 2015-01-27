using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MySql.Data.MySqlClient;
using System.IO;
using System.Data;

namespace RestServer.Models
{
    public class ConexaoDbeee
    {
        string connectionString = @"server=mysql10.agilus.com.br;Database=agilus11;User Id=agilus11;Password=ac@78902";

        public int VerificarAcessoUsuario(string usuario, string Senha)
        {
            int idUsuario;

            using (MySqlConnection conexao = new MySqlConnection(this.connectionString))
            {
                string consultaUsuarioDb = "select id,habilitado from usuario where nome=@nome and senha=@senha";
                MySqlCommand cmd = new MySqlCommand(consultaUsuarioDb, conexao);
                cmd.Parameters.AddWithValue("@nome", usuario);
                cmd.Parameters.AddWithValue("@senha", Senha);

                conexao.Open();

                MySqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    if (!Convert.ToBoolean(dr["habilitado"]))
                    {
                        throw new Exception("Usuário bloqueado. O usuário " + usuario + " já possui cadastro, mas ainda não foi habilitado. /n Entre em contato com o suporte e solicite o desbloqueio.");
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
            using (MySqlConnection conexao = new MySqlConnection(this.connectionString))
            {
                string inserirUsuarioBd = "insert into usuario(nome,senha,email) values(@nome,@senha,@email)";
                MySqlCommand cmd = new MySqlCommand(inserirUsuarioBd, conexao);
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

                if (inseriu == 0)
                {
                    throw new Exception("Não foi possível criar o usuário. Entre em contato com o administrador.");
                }
            }
        }

        public void ExcluirUsuario(int idUsuario)
        {
            using (MySqlConnection conexao = new MySqlConnection(this.connectionString))
            {
                string deletarUsuarioDb = "delete from usuario where id= @idUsuario";
                MySqlCommand cmd = new MySqlCommand(deletarUsuarioDb, conexao);
                cmd.Parameters.AddWithValue("@idUsuario", idUsuario);

                conexao.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void AlterarUsuario(Usuario usuario, int idUsuario)
        {
            using (MySqlConnection conexao = new MySqlConnection(this.connectionString))
            {
                string alterarUsuarioDb = "update usuario set nome=@nome, email=@email, senha=@senha where id=@id";
                MySqlCommand cmd = new MySqlCommand(alterarUsuarioDb, conexao);
                cmd.Parameters.AddWithValue("@nome", usuario.Nome);
                cmd.Parameters.AddWithValue("@email", usuario.Email);
                cmd.Parameters.AddWithValue("@senha", usuario.Senha);
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

            using (MySqlConnection conexao = new MySqlConnection(this.connectionString))
            {
                string consultarDadosUsuarioDb = "select nome,email,senha from usuario where id=@idUsuario";
                MySqlCommand cmd = new MySqlCommand(consultarDadosUsuarioDb, conexao);
                cmd.Parameters.AddWithValue("@idUsuario", idUsuario);
                conexao.Open();
                MySqlDataReader dr = cmd.ExecuteReader();

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
            using (MySqlConnection conexao = new MySqlConnection(this.connectionString))
            {
                string inserirConsultaDb = "insert into consulta(id_solicitacao,data_consulta,chave,id_status_processo,id_tipo_consulta) values(@idSolicitacao,@DataConsulta,@Chave,@StatusProcesso,@tipoConsulta)";

                MySqlCommand cmd = new MySqlCommand(inserirConsultaDb, conexao);

                MySqlParameter id = new MySqlParameter("@idSolicitacao", MySqlDbType.Int16);
                id.Value = idSolicitacao;
                cmd.Parameters.Add(id);

                MySqlParameter dataConsulta = new MySqlParameter("@DataConsulta", MySqlDbType.Timestamp);
                dataConsulta.Value = consulta.DataConsulta;
                cmd.Parameters.Add(dataConsulta);

                MySqlParameter Chave = new MySqlParameter("@Chave", MySqlDbType.VarChar);
                Chave.Value = consulta.Chave;
                cmd.Parameters.Add(Chave);

                MySqlParameter StatusProcesso = new MySqlParameter("@StatusProcesso", MySqlDbType.Int16);
                StatusProcesso.Value = Convert.ToInt16(consulta.StatusProcesso);
                cmd.Parameters.Add(StatusProcesso);

                MySqlParameter tipoConsulta = new MySqlParameter("@tipoConsulta", MySqlDbType.Int16);
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

            using (MySqlConnection conexao = new MySqlConnection(this.connectionString))
            {
                string ObterConsultas = "select id, chave, data_consulta, id_status_processo, data_processado, id_tipo_consulta from consulta where id_solicitacao=@Id and id_status_processo < 3";
                MySqlCommand cmd = new MySqlCommand(ObterConsultas, conexao);
                cmd.Parameters.AddWithValue("@Id", idSolicitacao);

                conexao.Open();

                MySqlDataReader dr = cmd.ExecuteReader();

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

            using (MySqlConnection conexao = new MySqlConnection(this.connectionString))
            {
                string ObterConsultas = "select id from consulta where id_solicitacao=@Id order by id";
                MySqlCommand cmd = new MySqlCommand(ObterConsultas, conexao);
                cmd.Parameters.AddWithValue("@Id", idSolicitacao);

                conexao.Open();

                MySqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    idConsultasSolicitadas.Add(Convert.ToInt16(dr["id"]));
                }
            }

            return idConsultasSolicitadas;
        }

        public void AtualizarConsulta(Consulta consultaAtualizada)
        {
            using (MySqlConnection conexao = new MySqlConnection(this.connectionString))
            {
                string alterarConsultaDb = "update consulta set id_status_processo= @StatusProcesso, data_processado=@DataProcessado where chave=@Chave";
                MySqlCommand cmd = new MySqlCommand(alterarConsultaDb, conexao);
                cmd.Parameters.AddWithValue("@StatusProcesso", Convert.ToInt16(consultaAtualizada.StatusProcesso));
                cmd.Parameters.AddWithValue("@DataProcessado", consultaAtualizada.DataProcessado);
                cmd.Parameters.AddWithValue("@Chave", consultaAtualizada.Chave);

                conexao.Open();

                cmd.ExecuteNonQuery();
            }
        }

        //public List<Consulta> VerificarConsultasRealizadas(int idUsuario)
        //{
        //    // Arrumar este método

        //    //Verifica no banco de dados as pesquisas de lote realizadas e retorna a lista de consultas com status e outras informações

        //    List<Consulta> consultas = new List<Consulta>();

        //    using (MySqlConnection conexao = new MySqlConnection(this.connectionString))
        //    {
        //        string consultaBd = "select id,chave, data_consulta, id_status_processo, data_processado, id_tipo_consulta from consulta order by data_consulta asc";
        //        MySqlCommand cmd = new MySqlCommand(consultaBd, conexao);
        //        cmd.Parameters.AddWithValue("@idUsuario", idUsuario);
        //        conexao.Open();

        //        MySqlDataReader dr = cmd.ExecuteReader();
        //        while (dr.Read())
        //        {
        //            Consulta itensConsulta = new Consulta();
        //            itensConsulta.Id = Convert.ToInt16(dr["id"]);
        //            itensConsulta.Chave = dr["chave"].ToString();
        //            itensConsulta.DataConsulta = Convert.ToDateTime(dr["data_consulta"]);
        //            itensConsulta.StatusProcesso = (TipoProcesso)Convert.ToInt16(dr["id_status_processo"]);

        //            if (dr["data_processado"].Equals(DBNull.Value))
        //            {
        //                itensConsulta.DataProcessado = null;
        //            }
        //            else
        //            {
        //                itensConsulta.DataProcessado = (DateTime?)dr["data_processado"];
        //            }

        //            itensConsulta.tipoConsultaRealizada = (Contexto)Convert.ToInt16(dr["id_tipo_consulta"]);

        //            consultas.Add(itensConsulta);
        //        }
        //    }

        //    return consultas;
        //}

        public bool VerificarConsultaProcessada(int? idConsulta)
        {
            bool processado;
            //Trazer descrição da tabela status processo
            using (MySqlConnection conexao = new MySqlConnection(this.connectionString))
            {
                string consultaBd = "select sp.descricao status_processo from consulta c join status_processo sp on c.id_status_processo = sp.id  where c.id = @idConsulta";
                MySqlCommand cmd = new MySqlCommand(consultaBd, conexao);
                cmd.Parameters.AddWithValue("@idConsulta", idConsulta);
                conexao.Open();

                string statusProcesso = cmd.ExecuteScalar().ToString();
                processado = (statusProcesso == "processado");
            }

            return processado;
        }

        public void inserirResultado(Dictionary<string, string> ResultadoMatricula, int idConsulta, int idSolicitacao, int statusItem)
        {
            Parser parser = new Parser();

            using (MySqlConnection conexao = new MySqlConnection(this.connectionString))
            {
                string inserirResultadoDb = "insert into resultado(id_consulta, nb, resultado, contem_erro) values(@idConsulta, @beneficio, @Resultado, @erro)";

                MySqlCommand cmd = new MySqlCommand(inserirResultadoDb, conexao);
                cmd.Parameters.AddWithValue("@idConsulta", idConsulta);
                cmd.Parameters.Add("@beneficio", MySqlDbType.Int64);
                cmd.Parameters.Add("@Resultado", MySqlDbType.VarChar);
                cmd.Parameters.Add("@erro", MySqlDbType.Bit);

                foreach (var item in ResultadoMatricula)
                {
                    if (statusItem == 1 || statusItem == 0 )
                    {
                        int? tamanhoResultado;

                        cmd.Parameters[1].Value = item.Key;

                        //Avaliar valor, se nulo dentro destes 2 status (0,1) = erro de tentativas, senão valor do resultado.
                        if (item.Value != null)
                        {
                            cmd.Parameters[2].Value = Parser.CompactarResultado(item.Value);
                            tamanhoResultado = item.Value.Length;
                        }
                        else
                        {
                            cmd.Parameters[2].Value = Parser.CompactarResultado("Erro de tentativas.");
                            tamanhoResultado = "Erro de tentativas".Length;
                        }

                        //Avaliando tamanho, se o tamanho do elemento menor de 500, considera como erro de consulta (resultado com texto que veio na consulta).
                        if (tamanhoResultado != null && tamanhoResultado < 500)
                        {
                            cmd.Parameters[3].Value = 1;
                        }
                        else
                        {
                            cmd.Parameters[3].Value = 0;
                        }
                        conexao.Open();
                        cmd.ExecuteNonQuery();
                        conexao.Close();
                    }
                    else if(statusItem == 2 || statusItem == 3)
                    {
                        cmd.Parameters[1].Value = item.Key;
                        cmd.Parameters[2].Value = Parser.CompactarResultado("Erro de tentativas.");
                        cmd.Parameters[3].Value = 1;
                        
                        conexao.Open();
                        cmd.ExecuteNonQuery();
                        conexao.Close();
                    }

                    else if (statusItem == 4)
                    {
                        cmd.Parameters[1].Value = item.Key;
                        cmd.Parameters[2].Value = Parser.CompactarResultado("NB inválido.");
                        cmd.Parameters[3].Value = 1;

                        conexao.Open();
                        cmd.ExecuteNonQuery();
                        conexao.Close();
                    }
                }

                cmd = null;

                //Atualizando as consultas rv e conbas com erro, utilizo a consulta titular como base.
                string atualizarMatriculasErro = "update resultado set contem_erro='1' where id in (select t1.id from resultado t1 join consulta c on t1.id_consulta = c.id where t1.contem_erro = '0' and c.id_solicitacao= @id_solicitacao and exists(select 1 from resultado r join consulta c2 on r.id_consulta = c2.id where r.contem_erro = '1' and c2.id_solicitacao= @id_solicitacao and t1.nb = r.nb) )";
                cmd = new MySqlCommand(atualizarMatriculasErro, conexao);
                cmd.Parameters.AddWithValue("@id_solicitacao", idSolicitacao);

                conexao.Open();
                cmd.CommandTimeout = 120000;//2 minutos, aumentei o timeout de resposta, pois estava estourando na hora de atualizar os erros da tabela resultado.
                cmd.ExecuteNonQuery();

                conexao.Close();
                cmd = null;

                string alterarConsultaDb = "update consulta set id_status_processo= 3 where id=@idConsulta";
                cmd = new MySqlCommand(alterarConsultaDb, conexao);
                cmd.Parameters.AddWithValue("@idConsulta", idConsulta);

                conexao.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public DataSet RecuperarResultado(int idSolicitacao)
        {
            DataSet ds = new DataSet();

            using (MySqlConnection conexao = new MySqlConnection(this.connectionString))
            {
                string consultarDadosResultadoDb = "select c.id_tipo_consulta as tipo_consulta, r.nb,r.resultado from resultado r join consulta c on id_consulta = c.id where c.id_solicitacao = @idSolicitacao and contem_erro <> 1 order by nb,id_tipo_consulta";
                MySqlDataAdapter da = new MySqlDataAdapter(consultarDadosResultadoDb, conexao);
                da.SelectCommand.Parameters.Add(new MySqlParameter("@idSolicitacao", idSolicitacao));

                conexao.Open();

                da.Fill(ds);
            }
            return ds;
        }

        public int InserirSolicitacao(int idUsuario, string DescricaoSolicitacao)
        {
            int solicitacao;

            using (MySqlConnection conexao = new MySqlConnection(this.connectionString))
            {
                string inserirConsultaDb = "insert into solicitacao(id_usuario,descricao,id_status_solicitacao,data_solicitacao) values(@idUsuario,@descricao,1,now())";

                MySqlCommand cmd = new MySqlCommand(inserirConsultaDb, conexao);

                MySqlParameter id = new MySqlParameter("@idUsuario", MySqlDbType.Int16);
                id.Value = idUsuario;
                cmd.Parameters.Add(id);

                MySqlParameter descricao = new MySqlParameter("@descricao", MySqlDbType.VarChar);
                descricao.Value = DescricaoSolicitacao;
                cmd.Parameters.Add(descricao);

                conexao.Open();

                cmd.ExecuteNonQuery();

                //Obtendo o id da solicitação inserida   
                solicitacao = Convert.ToInt16(new MySqlCommand("select @@identity", conexao).ExecuteScalar());
            }

            return solicitacao;
        }

        public void AtualizarStatusSolicitacao(int idUsuario)
        {
            using (MySqlConnection conexao = new MySqlConnection(this.connectionString))
            {
                string alterarStatusDb = "update solicitacao s set id_status_solicitacao= 2, data_conclusao_solicitacao = now() where id_usuario=1 and not exists(select 1 from consulta c where c.id_solicitacao= s.id and c.id_status_processo < 3) and data_conclusao_solicitacao is null";
                MySqlCommand cmd = new MySqlCommand(alterarStatusDb, conexao);
                cmd.Parameters.AddWithValue("@Id", idUsuario);

                conexao.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public List<Solicitacao> ConsultarSolicitacoes(int idUsuario)
        {
            //Verifica no banco de dados as solicitações de lote realizadas e retorna a lista de solicitações do usuário

            List<Solicitacao> Solicitacoes = new List<Solicitacao>();

            using (MySqlConnection conexao = new MySqlConnection(this.connectionString))
            {
                string solicitacaoBd = "select id,descricao,id_status_solicitacao, data_solicitacao, data_conclusao_solicitacao from solicitacao where id_usuario = @idUsuario";
                MySqlCommand cmd = new MySqlCommand(solicitacaoBd, conexao);
                cmd.Parameters.AddWithValue("@idUsuario", idUsuario);
                conexao.Open();

                MySqlDataReader dr = cmd.ExecuteReader();
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

            using (MySqlConnection conexao = new MySqlConnection(this.connectionString))
            {
                string coleta = "select id_status_solicitacao from solicitacao where id=@id";
                MySqlCommand cmd = new MySqlCommand(coleta, conexao);
                cmd.Parameters.AddWithValue("@id", idSolicitacao);
                conexao.Open();

                int status_solicitacao = Convert.ToInt16(cmd.ExecuteScalar());

                if ((StatusSolicitacao)status_solicitacao == StatusSolicitacao.Download)
                {
                    coletaRealizada = true;
                }
            }

            return coletaRealizada;
        }

        private bool ContemErros(int idSolicitacao)
        {
            bool ContemErros = false;

            using (MySqlConnection conexao = new MySqlConnection(this.connectionString))
            {
                string coleta = "select count(*) from solicitacao s join consulta c on s.id = c.id_solicitacao where s.id= @idSolicitacao and exists(select 1 from resultado r where r.id_consulta = c.id and r.contem_erro = 1)";
                MySqlCommand cmd = new MySqlCommand(coleta, conexao);
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


        public bool ExisteSolicitacaoPendente(int idUsuario)
        {
            bool pendente = false;

            using (MySqlConnection conexao = new MySqlConnection(this.connectionString))
            {
                string solicitacaoPendente = "select count(*) from solicitacao where id_status_solicitacao < 2 and id_usuario = @idUsuario";
                MySqlCommand cmd = new MySqlCommand(solicitacaoPendente, conexao);
                cmd.Parameters.AddWithValue("@idUsuario", idUsuario);
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

            using (MySqlConnection conexao = new MySqlConnection(this.connectionString))
            {
                string consultarDadosResultadoDb = "select r.nb,r.resultado from resultado r join consulta c on id_consulta = c.id where c.id_solicitacao = @idSolicitacao and id_tipo_consulta = 1 and contem_erro = 1 order by nb,id_tipo_consulta";
                MySqlDataAdapter da = new MySqlDataAdapter(consultarDadosResultadoDb, conexao);
                da.SelectCommand.Parameters.Add(new MySqlParameter("@idSolicitacao", idSolicitacao));

                conexao.Open();

                da.Fill(ds);
            }
            return ds;
        }

    }
}