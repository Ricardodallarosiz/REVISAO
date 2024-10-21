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



app.Run();
