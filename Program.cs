using API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; 
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDataContext>();

var app = builder.Build();

app.MapPost("/api/funcionario/cadastrar", async ([FromBody] Funcionario funcionario, [FromServices] AppDataContext ctx) =>
{
    if (string.IsNullOrEmpty(funcionario.Nome) || string.IsNullOrEmpty(funcionario.Cpf))
    {
        return Results.BadRequest("Nome e CPF são obrigatórios.");
    }

    var funcionarioExistente = await ctx.Funcionarios.FirstOrDefaultAsync(f => f.Cpf == funcionario.Cpf);

    if (funcionarioExistente != null)
    {
        return Results.BadRequest("Já existe um funcionário com este CPF.");
    }

    ctx.Funcionarios.Add(funcionario);
    await ctx.SaveChangesAsync();

    return Results.Created($"/api/funcionario/{funcionario.Id}", funcionario);
});


app.MapGet("/api/funcionario/listar", async ([FromServices] AppDataContext ctx) =>
{
    var funcionarios = await ctx.Funcionarios.ToListAsync();
    return Results.Ok(funcionarios);
});

app.MapPut("/api/funcionario/atualizar/{id}", async (int id, [FromBody] Funcionario funcionarioAtualizado, [FromServices] AppDataContext ctx) =>
{
    // Buscar o funcionário existente
    var funcionarioExistente = await ctx.Funcionarios.FindAsync(id);
    if (funcionarioExistente == null)
    {
        return Results.NotFound("Funcionário não encontrado.");
    }

    // Atualizar os campos do funcionário
    funcionarioExistente.Nome = funcionarioAtualizado.Nome;
    funcionarioExistente.Cpf = funcionarioAtualizado.Cpf;
    funcionarioExistente.Cargo = funcionarioAtualizado.Cargo;
    funcionarioExistente.Salario = funcionarioAtualizado.Salario;

    // Salvar as mudanças no banco de dados
    await ctx.SaveChangesAsync();

    return Results.Ok(funcionarioExistente);
});

app.MapDelete("/api/funcionario/deletar/{id}", async (int id, [FromServices] AppDataContext ctx) =>
{
    // Buscar o funcionário existente
    var funcionarioExistente = await ctx.Funcionarios.FindAsync(id);
    if (funcionarioExistente == null)
    {
        return Results.NotFound("Funcionário não encontrado.");
    }

    // Remover o funcionário do banco de dados
    ctx.Funcionarios.Remove(funcionarioExistente);
    await ctx.SaveChangesAsync();

    return Results.Ok($"Funcionário com ID {id} foi deletado.");
});



app.MapPost("/api/folha/cadastrar", async ([FromBody] FolhaDePagamento folha, [FromServices] AppDataContext ctx) =>
{
    var funcionario = await ctx.Funcionarios.FirstOrDefaultAsync(f => f.Cpf == folha.CpfFuncionario);
    if (funcionario == null)
    {
        return Results.BadRequest("Funcionário não encontrado.");
    }

    // Calcular salário bruto
    decimal salarioBruto = CalculosFolhaDePagamento.CalcularSalarioBruto(folha.HorasTrabalhadas, funcionario.Salario / 160); // Exemplo: Assumindo 160 horas/mês

    // Calcular INSS
    decimal inss = CalculosFolhaDePagamento.CalcularINSS(salarioBruto);

    // Calcular Imposto de Renda
    decimal ir = CalculosFolhaDePagamento.CalcularIR(salarioBruto);

    // Calcular FGTS
    decimal fgts = CalculosFolhaDePagamento.CalcularFGTS(salarioBruto);

    // Calcular Salário Líquido
    decimal salarioLiquido = CalculosFolhaDePagamento.CalcularSalarioLiquido(salarioBruto, ir, inss);

    folha.ValorPago = salarioLiquido;

    ctx.FolhasDePagamento.Add(folha);
    await ctx.SaveChangesAsync();

    return Results.Created($"/api/folha/{folha.Id}", folha);
});


app.MapGet("/api/folha/listar", async ([FromServices] AppDataContext ctx) =>
{
    var folhas = await ctx.FolhasDePagamento.ToListAsync();
    return Results.Ok(folhas);
});

app.MapGet("/api/folha/buscar/{cpf}/{mes}/{ano}", async (string cpf, int mes, int ano, [FromServices] AppDataContext ctx) =>
{
    var folha = await ctx.FolhasDePagamento.FirstOrDefaultAsync(f => f.CpfFuncionario == cpf && f.Mes == mes && f.Ano == ano);

    if (folha == null)
    {
        return Results.NotFound("Folha de pagamento não encontrada.");
    }

    return Results.Ok(folha);
});

app.MapPut("/api/folha/atualizar/{id}", async (int id, [FromBody] FolhaDePagamento folhaAtualizada, [FromServices] AppDataContext ctx) =>
{
    // Buscar a folha de pagamento existente
    var folhaExistente = await ctx.FolhasDePagamento.FindAsync(id);
    if (folhaExistente == null)
    {
        return Results.NotFound("Folha de pagamento não encontrada.");
    }

    // Atualizar os campos da folha de pagamento
    folhaExistente.CpfFuncionario = folhaAtualizada.CpfFuncionario;
    folhaExistente.Mes = folhaAtualizada.Mes;
    folhaExistente.Ano = folhaAtualizada.Ano;
    folhaExistente.HorasTrabalhadas = folhaAtualizada.HorasTrabalhadas;

    // Recalcular o valor pago com base nas horas trabalhadas e regras de negócio
    var funcionario = await ctx.Funcionarios.FirstOrDefaultAsync(f => f.Cpf == folhaAtualizada.CpfFuncionario);
    if (funcionario == null)
    {
        return Results.BadRequest("Funcionário não encontrado.");
    }

    // Calcular o salário bruto
    decimal salarioBruto = folhaAtualizada.HorasTrabalhadas * (funcionario.Salario / 160);

    // Calcular INSS, IR e Salário Líquido
    decimal inss = CalculosFolhaDePagamento.CalcularINSS(salarioBruto);
    decimal ir = CalculosFolhaDePagamento.CalcularIR(salarioBruto);
    decimal salarioLiquido = CalculosFolhaDePagamento.CalcularSalarioLiquido(salarioBruto, ir, inss);

    folhaExistente.ValorPago = salarioLiquido;

    // Salvar as mudanças no banco de dados
    await ctx.SaveChangesAsync();

    return Results.Ok(folhaExistente);
});

app.MapDelete("/api/folha/deletar/{id}", async (int id, [FromServices] AppDataContext ctx) =>
{
    // Buscar a folha de pagamento existente
    var folhaExistente = await ctx.FolhasDePagamento.FindAsync(id);
    if (folhaExistente == null)
    {
        return Results.NotFound("Folha de pagamento não encontrada.");
    }

    // Remover a folha de pagamento do banco de dados
    ctx.FolhasDePagamento.Remove(folhaExistente);
    await ctx.SaveChangesAsync();

    return Results.Ok($"Folha de pagamento com ID {id} foi deletada.");
});



app.Run();
