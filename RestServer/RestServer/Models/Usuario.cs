using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestServer.Models
{
    public class Usuario
    {
        public string Nome { get; set; }
        public string Senha { get; set; }
        public string Email { get; set; }
    }
}