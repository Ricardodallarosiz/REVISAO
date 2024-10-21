using System;
using Microsoft.EntityFrameworkCore;

namespace API.Models;

public class AppDataContext:DbContext
{
    public DbSet<Funcionario> Funcionarios { get; set; }
    public DbSet<FolhaDePagamento> FolhasDePagamento { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source = banco.db");
    }
}
