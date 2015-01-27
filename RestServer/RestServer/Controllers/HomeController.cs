using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RestServer.Models;

namespace RestServer.Controllers
{
    
    public class HomeController : Controller
    {
        //
        // GET: /Home/
        [AllowAnonymous]
        public ActionResult Index()
        {
            if (TempData["UsuarioCriado"] != null)
            {
                ViewBag.UsuarioCriado = TempData["UsuarioCriado"];
                TempData.Remove("UsuarioCriado");
            }

            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult Index(string foo)
        {
            ConexaoDb2 conexao = new ConexaoDb2();
            int idUsuario = conexao.VerificarAcessoUsuario(Request.Form["Usuario"], Request.Form["Senha"], Request.ServerVariables.Get("REMOTE_ADDR"));
            Session["idUsuario"] = idUsuario;
            System.Web.Security.FormsAuthentication.SetAuthCookie(Request.Form["Usuario"], false);

            string returnUrl = Request.Form["returnUrl"];

            if (this.Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            else
                return RedirectToAction("Index", "Consultas", null);
        }

        public ActionResult Logoff()
        {
            System.Web.Security.FormsAuthentication.SignOut();
            Session["idUsuario"] = null;
            return RedirectToAction("Index", "Home", null);
        }
    }
}
