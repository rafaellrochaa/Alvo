using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RestServer.Models;

namespace RestServer.Controllers
{
    public class UsuarioController : Controller
    {
        // GET: /Usuario/
        [AllowAnonymous]
        public ActionResult Index()
        {
            //Cadastro usuário
            Usuario u = new Usuario();
            return View(u);
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult Index(Usuario usuario)
        {
            if (Request.Form["Senha"] == Request.Form["ConfirmarSenha"])
            {
                ConexaoDb2 conexao = new ConexaoDb2();
                conexao.CriarUsuario(usuario);
                
                TempData["UsuarioCriado"] = @"O usuário foi criado, porém é necessário ativá-lo para consulta. Você receberá um email com o procedimento 
                                            para ativar seu usuário.";

                string mensagem = @"Olá "+char.ToUpper(usuario.Nome[0]) + usuario.Nome.Substring(1) +", bem vindo ao sistema Alvo. <br>"+
                "Para ativar seu usuário é necessário entrar em contato com nosso suporte pelo telefone (11) 3266-3124. <br>"+
                "Mediante a contratação do serviço, seu usuário será habilitado. <br>" +
                "Aguardamos seu contato.<br> Atenciosamente, Alvo";
                
                Email email = new Email(usuario.Email, mensagem, "Falta pouco para você começar a usar o Alvo");
                email.DispararMensagem();
            }

            else
            {
                usuario.Senha = String.Empty;
                ViewBag.MensagemErro = "Senhas não correspondem.";
                return View(usuario);
            }

            return RedirectToAction("Index", "Home", null);
        }

        public ActionResult Alterar(int id)
        {
            //Alterar usuário

            ConexaoDb2 conexao = new ConexaoDb2();
            Usuario u = conexao.RecuperarDadosUsuario(id);
            return View(u);
        }

        [HttpPost]
        public ActionResult Alterar(Usuario u, int id)
        {
            ConexaoDb2 conexao = new ConexaoDb2();
            conexao.AlterarUsuario(u, id);
            return RedirectToAction("Index", "Consultas", null);
        }

        public ActionResult AlterarSenha(int id)
        {
            //Alterar senha
            ConexaoDb2 conexao = new ConexaoDb2();
            Usuario u = conexao.RecuperarDadosUsuario(id);
            return View(u);
        }

        [HttpPost]
        public ActionResult AlterarSenha(Usuario u, int id)
        {
            //Alterar senha
            if (u.Senha == Request.Form["ConfirmarSenha"])
            {
                ConexaoDb2 conexao = new ConexaoDb2();
                conexao.AlterarSenha(u, id);
                return RedirectToAction("Index", "Consultas", null);
            }
            else
            {
                ViewBag.MensagemErro = "Senhas não correspondem.";
                u.Senha = String.Empty;
                return View(u);
            }
        }


    }
}
