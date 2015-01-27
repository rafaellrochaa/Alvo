using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
namespace RestServer.Controllers
{
    public class LimparArquivosController : Controller
    {
        //
        // GET: /LimparArquivos/

        [AllowAnonymous]
        public string Index()
        {
            string[] arquivos = Directory.GetFiles(System.IO.Path.GetTempPath(), "*.csv", SearchOption.TopDirectoryOnly);
            string mensagem = "OK";

            foreach (string arquivo in arquivos)
            {
                if (DateTime.Now > System.IO.File.GetCreationTime(arquivo).AddHours(24))
                {
                    try
                    {
                        System.IO.File.Delete(arquivo);
                    }

                    catch (Exception e)
                    {
                        mensagem = e.Message;
                    }
                }
            }
            return mensagem;
        }

    }
}
