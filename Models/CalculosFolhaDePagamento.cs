public static class CalculosFolhaDePagamento
{
    public static decimal CalcularSalarioBruto(decimal horasTrabalhadas, decimal valorHora)
    {
        return horasTrabalhadas * valorHora;
    }

    public static decimal CalcularINSS(decimal salarioBruto)
    {
        if (salarioBruto <= 1693.72m)
            return salarioBruto * 0.08m;
        else if (salarioBruto <= 2822.90m)
            return salarioBruto * 0.09m;
        else if (salarioBruto <= 5645.80m)
            return salarioBruto * 0.11m;
        else
            return 621.03m; // Valor fixo para salários acima de 5.645,80
    }

    public static decimal CalcularIR(decimal salarioBruto)
    {
        if (salarioBruto <= 1903.98m)
            return 0m;
        else if (salarioBruto <= 2826.65m)
            return salarioBruto * 0.075m - 142.80m;
        else if (salarioBruto <= 3751.05m)
            return salarioBruto * 0.15m - 354.80m;
        else if (salarioBruto <= 4664.68m)
            return salarioBruto * 0.225m - 636.13m;
        else
            return salarioBruto * 0.275m - 869.36m;
    }

    public static decimal CalcularFGTS(decimal salarioBruto)
    {
        return salarioBruto * 0.08m; // 8% do salário bruto
    }

    public static decimal CalcularSalarioLiquido(decimal salarioBruto, decimal ir, decimal inss)
    {
        return salarioBruto - ir - inss; // Salário Líquido = Salário Bruto - IR - INSS
    }
}
