using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Unilife.Models;

namespace Unilife.Data
{
    public static class SeedData
    {
        public static async Task InicializarAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // ----- Roles -----
            string[] roles = { "Coordinador", "Docente", "Alumno" };
            foreach (var rol in roles)
            {
                if (!await roleManager.RoleExistsAsync(rol))
                    await roleManager.CreateAsync(new IdentityRole(rol));
            }

            // ----- Carreras -----
            if (!context.CarrerasRegistradas.Any())
            {
                var lista = new List<CarreraRegistrada>();
                foreach (var n in Carreras.Ingenierias)
                    lista.Add(new CarreraRegistrada { Nombre = n, Grupo = "Ingenierías", EsMedica = false });
                foreach (var n in Carreras.CienciasSociales)
                    lista.Add(new CarreraRegistrada { Nombre = n, Grupo = "Ciencias Sociales", EsMedica = false });
                foreach (var n in Carreras.CienciasMedicas)
                    lista.Add(new CarreraRegistrada { Nombre = n, Grupo = "Ciencias Médicas", EsMedica = true });
                context.CarrerasRegistradas.AddRange(lista);
                await context.SaveChangesAsync();
            }

            // ----- Usuarios -----
            async Task CrearUsuarioAsync(
                string email, string password, string rol,
                string nombre, string apellido, string? carrera = null)
            {
                if (await userManager.FindByEmailAsync(email) == null)
                {
                    var usuario = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true,
                        Nombre = nombre,
                        Apellido = apellido,
                        Carrera = carrera
                    };

                    var resultado = await userManager.CreateAsync(usuario, password);
                    if (resultado.Succeeded)
                        await userManager.AddToRoleAsync(usuario, rol);
                }
            }

            await CrearUsuarioAsync("coordinador@unilife.com", "Admin123*", "Coordinador", "Admin", "UniLife");

            // Docentes
            await CrearUsuarioAsync("juan.perez@unilife.com",      "Docente123*", "Docente", "Juan",     "Pérez");
            await CrearUsuarioAsync("ana.torres@unilife.com",       "Docente123*", "Docente", "Ana",      "Torres");
            await CrearUsuarioAsync("carlos.ramos@unilife.com",     "Docente123*", "Docente", "Carlos",   "Ramos");
            await CrearUsuarioAsync("lucia.vargas@unilife.com",     "Docente123*", "Docente", "Lucía",    "Vargas");
            await CrearUsuarioAsync("roberto.quispe@unilife.com",   "Docente123*", "Docente", "Roberto",  "Quispe");

            // Alumnos — Ingenierías
            await CrearUsuarioAsync("maria.garcia@unilife.com",     "Alumno123*", "Alumno", "María",    "García",    "Ingeniería en Sistemas");
            await CrearUsuarioAsync("pedro.mendoza@unilife.com",    "Alumno123*", "Alumno", "Pedro",    "Mendoza",   "Ingeniería en Sistemas");
            await CrearUsuarioAsync("sofia.luna@unilife.com",       "Alumno123*", "Alumno", "Sofía",    "Luna",      "Ingeniería Civil");
            await CrearUsuarioAsync("andres.chavez@unilife.com",    "Alumno123*", "Alumno", "Andrés",   "Chávez",    "Ingeniería Mecatrónica");
            await CrearUsuarioAsync("valeria.cano@unilife.com",     "Alumno123*", "Alumno", "Valeria",  "Cano",      "Ingeniería Industrial");

            // Alumnos — Ciencias Sociales
            await CrearUsuarioAsync("diego.flores@unilife.com",     "Alumno123*", "Alumno", "Diego",    "Flores",    "Derecho");
            await CrearUsuarioAsync("camila.rios@unilife.com",      "Alumno123*", "Alumno", "Camila",   "Ríos",      "Psicología");
            await CrearUsuarioAsync("jose.medina@unilife.com",      "Alumno123*", "Alumno", "José",     "Medina",    "Administración de Empresas");
            await CrearUsuarioAsync("daniela.paredes@unilife.com",  "Alumno123*", "Alumno", "Daniela",  "Paredes",   "Comunicación Social");

            // Alumnos — Ciencias Médicas
            await CrearUsuarioAsync("luis.huaman@unilife.com",      "Alumno123*", "Alumno", "Luis",     "Huamán",    "Medicina");
            await CrearUsuarioAsync("gabriela.santos@unilife.com",  "Alumno123*", "Alumno", "Gabriela", "Santos",    "Enfermería");
            await CrearUsuarioAsync("marco.castillo@unilife.com",   "Alumno123*", "Alumno", "Marco",    "Castillo",  "Odontología");
            await CrearUsuarioAsync("isabela.mora@unilife.com",     "Alumno123*", "Alumno", "Isabela",  "Mora",      "Nutrición");

            // ----- Cursos (datos reales en la BD) -----
            var docente     = await userManager.FindByEmailAsync("juan.perez@unilife.com");
            var docente2    = await userManager.FindByEmailAsync("ana.torres@unilife.com");
            var docente3    = await userManager.FindByEmailAsync("carlos.ramos@unilife.com");
            var alumnoSeed  = await userManager.FindByEmailAsync("maria.garcia@unilife.com");
            var alumno2     = await userManager.FindByEmailAsync("pedro.mendoza@unilife.com");
            var alumno3     = await userManager.FindByEmailAsync("diego.flores@unilife.com");
            var alumno4     = await userManager.FindByEmailAsync("luis.huaman@unilife.com");

            if (!await context.Cursos.AnyAsync())
            {
                var cursos = new List<Curso>
                {
                    // Ingeniería en Sistemas
                    new Curso { Nombre = "Matemáticas Discretas",  Descripcion = "Lógica proposicional y teoría de conjuntos", Carrera = "Ingeniería en Sistemas",   Semestre = "III", DocenteId = docente?.Id  },
                    new Curso { Nombre = "Programación Web",        Descripcion = "Desarrollo con ASP.NET Core MVC",           Carrera = "Ingeniería en Sistemas",   Semestre = "V",   DocenteId = docente?.Id  },
                    new Curso { Nombre = "Base de Datos",           Descripcion = "Modelado relacional y SQL",                 Carrera = "Ingeniería en Sistemas",   Semestre = "IV",  DocenteId = docente2?.Id },
                    new Curso { Nombre = "Algoritmos y Estructuras", Descripcion = "Diseño y análisis de algoritmos",          Carrera = "Ingeniería en Sistemas",   Semestre = "II",  DocenteId = docente?.Id  },
                    // Ingeniería Civil
                    new Curso { Nombre = "Resistencia de Materiales", Descripcion = "Propiedades mecánicas y análisis de cargas", Carrera = "Ingeniería Civil",      Semestre = "IV",  DocenteId = docente3?.Id },
                    new Curso { Nombre = "Topografía",              Descripcion = "Medición y representación del terreno",     Carrera = "Ingeniería Civil",         Semestre = "III", DocenteId = docente2?.Id },
                    // Derecho
                    new Curso { Nombre = "Derecho Constitucional",  Descripcion = "Fundamentos del ordenamiento constitucional", Carrera = "Derecho",               Semestre = "II",  DocenteId = docente3?.Id },
                    new Curso { Nombre = "Derecho Civil",           Descripcion = "Personas, actos jurídicos y contratos",     Carrera = "Derecho",                  Semestre = "IV",  DocenteId = docente2?.Id },
                    // Medicina
                    new Curso { Nombre = "Anatomía Humana",         Descripcion = "Estructura del cuerpo humano",              Carrera = "Medicina",                 Semestre = "I",   DocenteId = docente3?.Id },
                    new Curso { Nombre = "Bioquímica Médica",       Descripcion = "Bases moleculares de la fisiología",        Carrera = "Medicina",                 Semestre = "II",  DocenteId = docente2?.Id },
                };
                context.Cursos.AddRange(cursos);
                await context.SaveChangesAsync();

                // Horarios
                context.HorariosCurso.AddRange(
                    new HorarioCurso { CursoId = cursos[0].Id, Tipo = "Teórico",   Dia = "Lunes",     HoraInicio = new TimeSpan(8,  0, 0), HoraFin = new TimeSpan(10, 0, 0), Pabellon = "A", Aula = "201" },
                    new HorarioCurso { CursoId = cursos[0].Id, Tipo = "Práctico",  Dia = "Miércoles", HoraInicio = new TimeSpan(8,  0, 0), HoraFin = new TimeSpan(10, 0, 0), Pabellon = "B", Aula = "102" },
                    new HorarioCurso { CursoId = cursos[1].Id, Tipo = "Teórico",   Dia = "Martes",    HoraInicio = new TimeSpan(10, 0, 0), HoraFin = new TimeSpan(12, 0, 0), Pabellon = "A", Aula = "3"   },
                    new HorarioCurso { CursoId = cursos[1].Id, Tipo = "Práctico",  Dia = "Jueves",    HoraInicio = new TimeSpan(14, 0, 0), HoraFin = new TimeSpan(16, 0, 0), Pabellon = "B", Aula = "101" },
                    new HorarioCurso { CursoId = cursos[2].Id, Tipo = "Teórico",   Dia = "Miércoles", HoraInicio = new TimeSpan(14, 0, 0), HoraFin = new TimeSpan(16, 0, 0), Pabellon = "A", Aula = "105" },
                    new HorarioCurso { CursoId = cursos[3].Id, Tipo = "Teórico",   Dia = "Viernes",   HoraInicio = new TimeSpan(8,  0, 0), HoraFin = new TimeSpan(10, 0, 0), Pabellon = "A", Aula = "202" },
                    new HorarioCurso { CursoId = cursos[4].Id, Tipo = "Teórico",   Dia = "Lunes",     HoraInicio = new TimeSpan(12, 0, 0), HoraFin = new TimeSpan(14, 0, 0), Pabellon = "C", Aula = "301" },
                    new HorarioCurso { CursoId = cursos[5].Id, Tipo = "Teórico",   Dia = "Martes",    HoraInicio = new TimeSpan(8,  0, 0), HoraFin = new TimeSpan(10, 0, 0), Pabellon = "C", Aula = "302" },
                    new HorarioCurso { CursoId = cursos[5].Id, Tipo = "Práctico",  Dia = "Jueves",    HoraInicio = new TimeSpan(8,  0, 0), HoraFin = new TimeSpan(10, 0, 0), Pabellon = "C", Aula = "Lab1"},
                    new HorarioCurso { CursoId = cursos[6].Id, Tipo = "Teórico",   Dia = "Miércoles", HoraInicio = new TimeSpan(10, 0, 0), HoraFin = new TimeSpan(12, 0, 0), Pabellon = "D", Aula = "401" },
                    new HorarioCurso { CursoId = cursos[7].Id, Tipo = "Teórico",   Dia = "Lunes",     HoraInicio = new TimeSpan(16, 0, 0), HoraFin = new TimeSpan(18, 0, 0), Pabellon = "D", Aula = "402" },
                    new HorarioCurso { CursoId = cursos[8].Id, Tipo = "Teórico",   Dia = "Lunes",     HoraInicio = new TimeSpan(8,  0, 0), HoraFin = new TimeSpan(10, 0, 0), Pabellon = "E", Aula = "501" },
                    new HorarioCurso { CursoId = cursos[8].Id, Tipo = "Práctico",  Dia = "Miércoles", HoraInicio = new TimeSpan(12, 0, 0), HoraFin = new TimeSpan(14, 0, 0), Pabellon = "E", Aula = "Lab2"},
                    new HorarioCurso { CursoId = cursos[9].Id, Tipo = "Teórico",   Dia = "Viernes",   HoraInicio = new TimeSpan(10, 0, 0), HoraFin = new TimeSpan(12, 0, 0), Pabellon = "E", Aula = "502" }
                );

                // Inscripciones
                var inscrip = new List<(int idx, ApplicationUser? alumno)>
                {
                    (0, alumnoSeed), (1, alumnoSeed), (2, alumnoSeed), (3, alumnoSeed),
                    (0, alumno2),   (1, alumno2),    (3, alumno2),
                    (4, null),      // civil sin alumno seed — ok
                    (6, alumno3),   (7, alumno3),
                    (8, alumno4),   (9, alumno4),
                };
                foreach (var (idx, alumno) in inscrip)
                {
                    if (alumno != null)
                        context.CursoAlumnos.Add(new CursoAlumno { CursoId = cursos[idx].Id, AlumnoId = alumno.Id });
                }
                await context.SaveChangesAsync();
            }

            // ----- Eventos -----
            if (!await context.Eventos.AnyAsync())
            {
                context.Eventos.AddRange(
                    // Generales — para toda la universidad
                    new Evento { Titulo = "Semana de Bienvenida", Descripcion = "Actividades de integración para nuevos estudiantes", Fecha = DateTime.Today.AddDays(5), Hora = new TimeSpan(9, 0, 0), Lugar = "Plaza Central", TipoEvento = "Actividad", EsGeneral = true },
                    new Evento { Titulo = "Feria de Clubs", Descripcion = "Presentación de clubs y actividades extracurriculares", Fecha = DateTime.Today.AddDays(10), Hora = new TimeSpan(10, 0, 0), Lugar = "Patio Principal", TipoEvento = "Feria", EsGeneral = true },
                    // Ingeniería en Sistemas
                    new Evento { Titulo = "Charla de IA", Descripcion = "Introducción a la inteligencia artificial", Fecha = DateTime.Today.AddDays(3), Hora = new TimeSpan(16, 0, 0), Lugar = "Aula Magna", TipoEvento = "Charla", EsGeneral = false, Carrera = "Ingeniería en Sistemas" },
                    new Evento { Titulo = "Hackathon UNI", Descripcion = "48 horas de programación", Fecha = DateTime.Today.AddDays(7), Hora = new TimeSpan(9, 0, 0), Lugar = "Auditorio Central", TipoEvento = "Hackathon", EsGeneral = false, Carrera = "Ingeniería en Sistemas" },
                    new Evento { Titulo = "Taller de Git", Descripcion = "Control de versiones con Git", Fecha = DateTime.Today.AddDays(12), Hora = new TimeSpan(11, 0, 0), Lugar = "Laboratorio 2", TipoEvento = "Taller", EsGeneral = false, Carrera = "Ingeniería en Sistemas" },
                    // Derecho
                    new Evento { Titulo = "Simulacro de Juicio Oral", Descripcion = "Práctica de litigación oral en el aula de juicios", Fecha = DateTime.Today.AddDays(4), Hora = new TimeSpan(15, 0, 0), Lugar = "Aula de Juicios", TipoEvento = "Práctica", EsGeneral = false, Carrera = "Derecho" },
                    new Evento { Titulo = "Charla: Derechos Humanos", Descripcion = "Conferencia sobre derechos humanos y jurisprudencia", Fecha = DateTime.Today.AddDays(9), Hora = new TimeSpan(17, 0, 0), Lugar = "Auditorio D", TipoEvento = "Charla", EsGeneral = false, Carrera = "Derecho" },
                    // Medicina
                    new Evento { Titulo = "Jornada de Salud Comunitaria", Descripcion = "Atención preventiva en comunidades cercanas", Fecha = DateTime.Today.AddDays(6), Hora = new TimeSpan(8, 0, 0), Lugar = "Centro de Salud", TipoEvento = "Jornada", EsGeneral = false, Carrera = "Medicina" }
                );
                await context.SaveChangesAsync();
            }

            // Corregir eventos tecnológicos que quedaron como generales en versiones anteriores
            var eventosTechMal = await context.Eventos
                .Where(e => (e.Titulo == "Charla de IA" || e.Titulo == "Hackathon UNI" || e.Titulo == "Taller de Git") && e.EsGeneral)
                .ToListAsync();
            foreach (var ev in eventosTechMal)
            {
                ev.EsGeneral = false;
                ev.Carrera = "Ingeniería en Sistemas";
            }
            if (eventosTechMal.Any())
                await context.SaveChangesAsync();

            // Eliminar tareas de ejemplo vencidas que hayan quedado del seed anterior
            var tareasEjemplo = new[] { "Entrega proyecto final", "Leer capítulo 5", "Resolver ejercicios", "Quiz de lógica" };
            var tareasViejas = context.Tareas.Where(t => tareasEjemplo.Contains(t.Titulo));
            if (await tareasViejas.AnyAsync())
            {
                context.Tareas.RemoveRange(tareasViejas);
                await context.SaveChangesAsync();
            }
        }
    }
}
