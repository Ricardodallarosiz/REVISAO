using System;

namespace API.Models;

public class Funcionario
{
        public int Id { get; set; }
        public string ?Nome { get; set; }
        public string ?Cpf { get; set; }
        public string ?Cargo { get; set; }
        public decimal Salario { get; set; }
}
