namespace Unilife.Models.Api
{
    public record EventoRecomendadoDto(
        int    Id,
        string Titulo,
        string Descripcion,
        string Fecha,
        string Hora,
        string Lugar,
        string TipoEvento,
        bool   EsGeneral,
        string? Carrera
    );

    public record LugarRecomendadoDto(
        int     Id,
        string  Nombre,
        string  Tipo,
        string  Direccion,
        double  Distancia,
        decimal PrecioPromedio,
        double  Calificacion,
        string  Descripcion
    );

    public record ValoracionLugarDto(
        int LugarId,
        int Puntaje        // 1-5
    );

    public record LugarConPromedioDto(
        int     Id,
        string  Nombre,
        string  Tipo,
        string  Direccion,
        double  Distancia,
        decimal PrecioPromedio,
        double  Calificacion,
        string  Descripcion,
        double? PromedioValoraciones,
        int     TotalValoraciones,
        int?    MiPuntaje
    );

    public record PagedResult<T>(
        IEnumerable<T> Items,
        int            Total,
        int            Page,
        int            PageSize
    );
}
