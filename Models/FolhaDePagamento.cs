using System;

namespace API.Models;

public class FolhaDePagamento
{
     public int Id { get; set; }
        public string ?CpfFuncionario { get; set; }
        public int Mes { get; set; }
        public int Ano { get; set; }
        public decimal HorasTrabalhadas { get; set; }
        public decimal ValorPago { get; set; }
}
