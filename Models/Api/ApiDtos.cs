namespace Unilife.Models.Api
{
    // ── Auth ─────────────────────────────────────────────
    public record LoginRequest(string Email, string Password);

    public record TokenResponse(
        string Token,
        string Nombre,
        string Email,
        string Rol,
        DateTime Expira);

    // ── Usuarios ─────────────────────────────────────────
    public record UsuarioDto(
        string Id,
        string Nombre,
        string Apellido,
        string Email,
        string? Carrera,
        string? FotoPerfil,
        string Rol);

    public record ActualizarPerfilRequest(
        string Nombre,
        string Apellido,
        string Email);

    // ── Cursos ───────────────────────────────────────────
    public record CursoDto(
        int    Id,
        string Nombre,
        string? Descripcion,
        string Carrera,
        string Semestre,
        string? DocenteNombre,
        int    TotalAlumnos);

    public record CursoRequest(
        string Nombre,
        string? Descripcion,
        string Carrera,
        string Semestre,
        string? DocenteId);

    // ── Eventos ──────────────────────────────────────────
    public record EventoDto(
        int      Id,
        string   Titulo,
        string   Descripcion,
        DateTime Fecha,
        string   Hora,
        string   Lugar,
        string   TipoEvento,
        bool     EsGeneral,
        string?  Carrera);

    public record EventoRequest(
        string   Titulo,
        string   Descripcion,
        DateTime Fecha,
        TimeSpan Hora,
        string   Lugar,
        string   TipoEvento,
        bool     EsGeneral,
        string?  Carrera);

    // ── Módulos ──────────────────────────────────────────
    public record ModuloDto(
        int    Id,
        string Nombre,
        string Color,
        int    Orden,
        int    TotalActividades);

    // ── Actividades ──────────────────────────────────────
    public record ActividadDto(
        int      Id,
        string   Tipo,
        string   Titulo,
        string   Descripcion,
        string?  Url,
        DateTime? FechaEntrega,
        int      Orden);
}
