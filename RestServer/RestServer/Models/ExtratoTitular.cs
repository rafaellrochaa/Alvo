using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace RestServer.Models
{
    public class ExtratoTitular
    {
        //Dados da consulta titular
        public string Matricula { get; set; }
        public string Situacao { get; set; }
        public string NomeTitular { get; set; }
        public string NomeMae { get; set; }
        public string Cpf { get; set; }
        public string Identidade { get; set; }
        public string MunicipioIdentidade { get; set; }
        public string UfIdentidade { get; set; }
        public string Sexo { get; set; }
        public DateTime? Nascimento { get; set; }
        public string Endereco { get; set; }
        public string Cep { get; set; }
        public string Municipio { get; set; }
        public string Uf { get; set; }
        public string Bairro { get; set; }
        public string Tel { get; set; }
        public string Ddd{ get; set; }
        public string Ramal { get; set; }
        public string Email { get; set; }
    }
}