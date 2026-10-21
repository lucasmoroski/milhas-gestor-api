namespace MilhasApi.Api.Models;

public enum PerfilUsuario
{
    Admin = 0,
    Gestor = 1,
    Operador = 2
}

public enum StatusCompra
{
    Pendente = 0,
    Confirmada = 1,
    Cancelada = 2
}

public enum StatusVenda
{
    Pendente = 0,
    Confirmada = 1,
    Emitida = 2,
    Finalizada = 3,
    Cancelada = 4
}

public enum TipoLancamento
{
    Receita = 0,
    Despesa = 1
}

public enum StatusLancamento
{
    Pendente = 0,
    Pago = 1,
    Atrasado = 2
}

public enum StatusViagem
{
    Planejada = 0,
    Emitida = 1,
    Confirmada = 2,
    Finalizada = 3
}

public enum TipoPagamentoViagem
{
    Milhas = 0,
    Pagante = 1
}
