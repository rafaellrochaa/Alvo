﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace RestServer.Models
{
    public class Email
    {
        private string senha = "ac@78902";
        private string smtp = "smtp.agilus.com.br";
        private string emailRemetente = "rafael@agilus.com.br";
        private string emailDestinatario;
        private string mensagem;
        private string assunto;

        public string Assunto { get; set; }

        public Email(string EmailDestinatario, string Mensagem, string Assunto)
        {
            this.emailDestinatario = EmailDestinatario;
            this.mensagem = Mensagem;
            this.assunto = Assunto;
        }

        public void DispararMensagem()
        {
            string emailDestinatario = this.emailDestinatario;
            string conteudoMensagem = this.mensagem;
            string assuntoMensagem = this.assunto;

            //Cria objeto com dados do e-mail.
            MailMessage objEmail = new MailMessage();

            //Define o Campo From e ReplyTo do e-mail.
            objEmail.From = new System.Net.Mail.MailAddress("SUPORTE ALVO " + "<"+emailRemetente+">");

            //Define os destinatários do e-mail.
            objEmail.To.Add(emailDestinatario);

            //Define a prioridade do e-mail.
            objEmail.Priority = System.Net.Mail.MailPriority.Normal;

            //Define o formato do e-mail HTML (caso não queira HTML alocar valor false)
            objEmail.IsBodyHtml = true;

            //Define título do e-mail.
            objEmail.Subject = assuntoMensagem;

            //Define o corpo do e-mail.
            objEmail.Body = conteudoMensagem;

            //Para evitar problemas de caracteres "estranhos", configuramos o charset para "ISO-8859-1"
            objEmail.SubjectEncoding = System.Text.Encoding.GetEncoding("ISO-8859-1");
            objEmail.BodyEncoding = System.Text.Encoding.GetEncoding("ISO-8859-1");

            //Cria objeto com os dados do SMTP
            System.Net.Mail.SmtpClient objSmtp = new System.Net.Mail.SmtpClient();

            //Alocamos o endereço do host para enviar os e-mails  
            objSmtp.Credentials = new System.Net.NetworkCredential(emailRemetente, senha);
            objSmtp.Host = smtp;
            objSmtp.Port = 587;
            //Caso utilize conta de email do exchange da locaweb deve habilitar o SSL
            //objSmtp.EnableSsl = true;

            //Enviamos o e-mail através do método .send()
            try
            {
                objSmtp.Send(objEmail);
            }
            catch (Exception ex)
            {
                throw new Exception("Ocorreram problemas no envio do e-mail. Erro = " + ex.Message);
            }
            finally
            {
                //excluímos o objeto de e-mail da memória
                objEmail.Dispose();
            }
        }

    }
}
