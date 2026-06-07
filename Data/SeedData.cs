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
            var context     = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // ----- Roles -----
            foreach (var rol in new[] { "Coordinador", "Docente", "Alumno" })
                if (!await roleManager.RoleExistsAsync(rol))
                    await roleManager.CreateAsync(new IdentityRole(rol));

            // ----- Carreras -----
            if (!context.CarrerasRegistradas.Any())
            {
                var lista = new List<CarreraRegistrada>();
                foreach (var n in Carreras.Ingenierias)
                    lista.Add(new CarreraRegistrada { Nombre = n, Grupo = "Ingenierías",      EsMedica = false });
                foreach (var n in Carreras.CienciasSociales)
                    lista.Add(new CarreraRegistrada { Nombre = n, Grupo = "Ciencias Sociales", EsMedica = false });
                foreach (var n in Carreras.CienciasMedicas)
                    lista.Add(new CarreraRegistrada { Nombre = n, Grupo = "Ciencias Médicas",  EsMedica = true  });
                context.CarrerasRegistradas.AddRange(lista);
                await context.SaveChangesAsync();
            }

            // ----- Helper para crear usuarios -----
            async Task CrearUsuarioAsync(string email, string password, string rol,
                string nombre, string apellido, string? carrera = null)
            {
                if (await userManager.FindByEmailAsync(email) == null)
                {
                    var u = new ApplicationUser
                    {
                        UserName = email, Email = email, EmailConfirmed = true,
                        Nombre = nombre, Apellido = apellido, Carrera = carrera
                    };
                    var r = await userManager.CreateAsync(u, password);
                    if (r.Succeeded) await userManager.AddToRoleAsync(u, rol);
                }
            }

            // ----- Coordinador -----
            await CrearUsuarioAsync("coordinador@unilife.com", "Admin123*", "Coordinador", "Admin", "UniLife");

            // ----- Docentes (12) -----
            await CrearUsuarioAsync("juan.perez@unilife.com",      "Docente123*", "Docente", "Juan",     "Pérez");
            await CrearUsuarioAsync("ana.torres@unilife.com",       "Docente123*", "Docente", "Ana",      "Torres");
            await CrearUsuarioAsync("carlos.ramos@unilife.com",     "Docente123*", "Docente", "Carlos",   "Ramos");
            await CrearUsuarioAsync("lucia.vargas@unilife.com",     "Docente123*", "Docente", "Lucía",    "Vargas");
            await CrearUsuarioAsync("roberto.quispe@unilife.com",   "Docente123*", "Docente", "Roberto",  "Quispe");
            await CrearUsuarioAsync("miguel.herrera@unilife.com",   "Docente123*", "Docente", "Miguel",   "Herrera");
            await CrearUsuarioAsync("sandra.castillo@unilife.com",  "Docente123*", "Docente", "Sandra",   "Castillo");
            await CrearUsuarioAsync("fernando.lopez@unilife.com",   "Docente123*", "Docente", "Fernando", "López");
            await CrearUsuarioAsync("patricia.vega@unilife.com",    "Docente123*", "Docente", "Patricia", "Vega");
            await CrearUsuarioAsync("jorge.mendoza@unilife.com",    "Docente123*", "Docente", "Jorge",    "Mendoza");
            await CrearUsuarioAsync("carmen.silva@unilife.com",     "Docente123*", "Docente", "Carmen",   "Silva");
            await CrearUsuarioAsync("alberto.huanca@unilife.com",   "Docente123*", "Docente", "Alberto",  "Huanca");

            // ----- Alumnos (30) -----
            // Ingenierías
            await CrearUsuarioAsync("maria.garcia@unilife.com",     "Alumno123*", "Alumno", "María",     "García",    "Ingeniería en Sistemas");
            await CrearUsuarioAsync("pedro.mendoza@unilife.com",    "Alumno123*", "Alumno", "Pedro",     "Mendoza",   "Ingeniería en Sistemas");
            await CrearUsuarioAsync("kevin.aliaga@unilife.com",     "Alumno123*", "Alumno", "Kevin",     "Aliaga",    "Ingeniería en Sistemas");
            await CrearUsuarioAsync("sofia.luna@unilife.com",       "Alumno123*", "Alumno", "Sofía",     "Luna",      "Ingeniería Civil");
            await CrearUsuarioAsync("carlos.rojas@unilife.com",     "Alumno123*", "Alumno", "Carlos",    "Rojas",     "Ingeniería Civil");
            await CrearUsuarioAsync("andres.chavez@unilife.com",    "Alumno123*", "Alumno", "Andrés",    "Chávez",    "Ingeniería Mecatrónica");
            await CrearUsuarioAsync("lucia.campos@unilife.com",     "Alumno123*", "Alumno", "Lucía",     "Campos",    "Ingeniería Mecatrónica");
            await CrearUsuarioAsync("rafael.torres@unilife.com",    "Alumno123*", "Alumno", "Rafael",    "Torres",    "Ingeniería Mecatrónica");
            await CrearUsuarioAsync("valeria.cano@unilife.com",     "Alumno123*", "Alumno", "Valeria",   "Cano",      "Ingeniería Industrial");
            await CrearUsuarioAsync("ana.reyes@unilife.com",        "Alumno123*", "Alumno", "Ana",       "Reyes",     "Ingeniería Industrial");
            await CrearUsuarioAsync("miguel.silva@unilife.com",     "Alumno123*", "Alumno", "Miguel",    "Silva",     "Ingeniería Industrial");
            // Ciencias Sociales
            await CrearUsuarioAsync("diego.flores@unilife.com",     "Alumno123*", "Alumno", "Diego",     "Flores",    "Derecho");
            await CrearUsuarioAsync("paola.gutierrez@unilife.com",  "Alumno123*", "Alumno", "Paola",     "Gutiérrez", "Derecho");
            await CrearUsuarioAsync("camila.rios@unilife.com",      "Alumno123*", "Alumno", "Camila",    "Ríos",      "Psicología");
            await CrearUsuarioAsync("fernanda.vega@unilife.com",    "Alumno123*", "Alumno", "Fernanda",  "Vega",      "Psicología");
            await CrearUsuarioAsync("sebastian.mora@unilife.com",   "Alumno123*", "Alumno", "Sebastián", "Mora",      "Psicología");
            await CrearUsuarioAsync("jose.medina@unilife.com",      "Alumno123*", "Alumno", "José",      "Medina",    "Administración de Empresas");
            await CrearUsuarioAsync("alejandra.cruz@unilife.com",   "Alumno123*", "Alumno", "Alejandra", "Cruz",      "Administración de Empresas");
            await CrearUsuarioAsync("nicolas.gomez@unilife.com",    "Alumno123*", "Alumno", "Nicolás",   "Gómez",     "Administración de Empresas");
            await CrearUsuarioAsync("daniela.paredes@unilife.com",  "Alumno123*", "Alumno", "Daniela",   "Paredes",   "Comunicación Social");
            await CrearUsuarioAsync("valentina.rojas@unilife.com",  "Alumno123*", "Alumno", "Valentina", "Rojas",     "Comunicación Social");
            await CrearUsuarioAsync("pablo.diaz@unilife.com",       "Alumno123*", "Alumno", "Pablo",     "Díaz",      "Comunicación Social");
            // Ciencias Médicas
            await CrearUsuarioAsync("luis.huaman@unilife.com",      "Alumno123*", "Alumno", "Luis",      "Huamán",    "Medicina");
            await CrearUsuarioAsync("gabriela.santos@unilife.com",  "Alumno123*", "Alumno", "Gabriela",  "Santos",    "Enfermería");
            await CrearUsuarioAsync("ana.mamani@unilife.com",       "Alumno123*", "Alumno", "Ana",       "Mamani",    "Enfermería");
            await CrearUsuarioAsync("marco.castillo@unilife.com",   "Alumno123*", "Alumno", "Marco",     "Castillo",  "Odontología");
            await CrearUsuarioAsync("jose.quispe@unilife.com",      "Alumno123*", "Alumno", "José",      "Quispe",    "Odontología");
            await CrearUsuarioAsync("isabela.mora@unilife.com",     "Alumno123*", "Alumno", "Isabela",   "Mora",      "Nutrición");
            await CrearUsuarioAsync("carla.flores@unilife.com",     "Alumno123*", "Alumno", "Carla",     "Flores",    "Nutrición");

            // ----- Referencias a usuarios para relaciones -----
            var dJuan      = await userManager.FindByEmailAsync("juan.perez@unilife.com");
            var dAna       = await userManager.FindByEmailAsync("ana.torres@unilife.com");
            var dCarlos    = await userManager.FindByEmailAsync("carlos.ramos@unilife.com");
            var dLucia     = await userManager.FindByEmailAsync("lucia.vargas@unilife.com");
            var dRoberto   = await userManager.FindByEmailAsync("roberto.quispe@unilife.com");
            var dMiguel    = await userManager.FindByEmailAsync("miguel.herrera@unilife.com");
            var dSandra    = await userManager.FindByEmailAsync("sandra.castillo@unilife.com");
            var dFernando  = await userManager.FindByEmailAsync("fernando.lopez@unilife.com");
            var dPatricia  = await userManager.FindByEmailAsync("patricia.vega@unilife.com");
            var dJorge     = await userManager.FindByEmailAsync("jorge.mendoza@unilife.com");
            var dCarmen    = await userManager.FindByEmailAsync("carmen.silva@unilife.com");
            var dAlberto   = await userManager.FindByEmailAsync("alberto.huanca@unilife.com");

            var aMaria     = await userManager.FindByEmailAsync("maria.garcia@unilife.com");
            var aPedro     = await userManager.FindByEmailAsync("pedro.mendoza@unilife.com");
            var aKevin     = await userManager.FindByEmailAsync("kevin.aliaga@unilife.com");
            var aSofia     = await userManager.FindByEmailAsync("sofia.luna@unilife.com");
            var aCarlosR   = await userManager.FindByEmailAsync("carlos.rojas@unilife.com");
            var aAndres    = await userManager.FindByEmailAsync("andres.chavez@unilife.com");
            var aLuciaC    = await userManager.FindByEmailAsync("lucia.campos@unilife.com");
            var aRafael    = await userManager.FindByEmailAsync("rafael.torres@unilife.com");
            var aValeria   = await userManager.FindByEmailAsync("valeria.cano@unilife.com");
            var aAnaR      = await userManager.FindByEmailAsync("ana.reyes@unilife.com");
            var aMiguelS   = await userManager.FindByEmailAsync("miguel.silva@unilife.com");
            var aDiego     = await userManager.FindByEmailAsync("diego.flores@unilife.com");
            var aPaola     = await userManager.FindByEmailAsync("paola.gutierrez@unilife.com");
            var aCamila    = await userManager.FindByEmailAsync("camila.rios@unilife.com");
            var aFernanda  = await userManager.FindByEmailAsync("fernanda.vega@unilife.com");
            var aSebastian = await userManager.FindByEmailAsync("sebastian.mora@unilife.com");
            var aJoseM     = await userManager.FindByEmailAsync("jose.medina@unilife.com");
            var aAlejandra = await userManager.FindByEmailAsync("alejandra.cruz@unilife.com");
            var aNicolas   = await userManager.FindByEmailAsync("nicolas.gomez@unilife.com");
            var aDaniela   = await userManager.FindByEmailAsync("daniela.paredes@unilife.com");
            var aValentina = await userManager.FindByEmailAsync("valentina.rojas@unilife.com");
            var aPablo     = await userManager.FindByEmailAsync("pablo.diaz@unilife.com");
            var aLuis      = await userManager.FindByEmailAsync("luis.huaman@unilife.com");
            var aGabriela  = await userManager.FindByEmailAsync("gabriela.santos@unilife.com");
            var aAnaMamani = await userManager.FindByEmailAsync("ana.mamani@unilife.com");
            var aMarco     = await userManager.FindByEmailAsync("marco.castillo@unilife.com");
            var aJoseQ     = await userManager.FindByEmailAsync("jose.quispe@unilife.com");
            var aIsabela   = await userManager.FindByEmailAsync("isabela.mora@unilife.com");
            var aCarla     = await userManager.FindByEmailAsync("carla.flores@unilife.com");

            // ----- Cursos bloque original (solo si BD vacía) -----
            if (!await context.Cursos.AnyAsync())
            {
                var c0 = new List<Curso>
                {
                    new Curso { Nombre = "Matemáticas Discretas",   Descripcion = "Lógica proposicional y teoría de conjuntos",          Carrera = "Ingeniería en Sistemas", Semestre = "III", DocenteId = dJuan?.Id    },
                    new Curso { Nombre = "Programación Web",         Descripcion = "Desarrollo con ASP.NET Core MVC",                    Carrera = "Ingeniería en Sistemas", Semestre = "V",   DocenteId = dJuan?.Id    },
                    new Curso { Nombre = "Base de Datos",            Descripcion = "Modelado relacional y SQL",                          Carrera = "Ingeniería en Sistemas", Semestre = "IV",  DocenteId = dAna?.Id     },
                    new Curso { Nombre = "Algoritmos y Estructuras", Descripcion = "Diseño y análisis de algoritmos",                    Carrera = "Ingeniería en Sistemas", Semestre = "II",  DocenteId = dJuan?.Id    },
                    new Curso { Nombre = "Resistencia de Materiales",Descripcion = "Propiedades mecánicas y análisis de cargas",         Carrera = "Ingeniería Civil",       Semestre = "IV",  DocenteId = dCarlos?.Id  },
                    new Curso { Nombre = "Topografía",               Descripcion = "Medición y representación del terreno",              Carrera = "Ingeniería Civil",       Semestre = "III", DocenteId = dAna?.Id     },
                    new Curso { Nombre = "Derecho Constitucional",   Descripcion = "Fundamentos del ordenamiento constitucional",        Carrera = "Derecho",                Semestre = "II",  DocenteId = dCarlos?.Id  },
                    new Curso { Nombre = "Derecho Civil",            Descripcion = "Personas, actos jurídicos y contratos",              Carrera = "Derecho",                Semestre = "IV",  DocenteId = dAna?.Id     },
                    new Curso { Nombre = "Anatomía Humana",          Descripcion = "Estructura del cuerpo humano",                       Carrera = "Medicina",               Semestre = "I",   DocenteId = dCarlos?.Id  },
                    new Curso { Nombre = "Bioquímica Médica",        Descripcion = "Bases moleculares de la fisiología",                 Carrera = "Medicina",               Semestre = "II",  DocenteId = dAna?.Id     },
                };
                context.Cursos.AddRange(c0);
                await context.SaveChangesAsync();

                context.HorariosCurso.AddRange(
                    new HorarioCurso { CursoId = c0[0].Id, Tipo = "Teórico",  Dia = "Lunes",     HoraInicio = new TimeSpan(8,  0, 0), HoraFin = new TimeSpan(10, 0, 0), Pabellon = "A", Aula = "201" },
                    new HorarioCurso { CursoId = c0[0].Id, Tipo = "Práctico", Dia = "Miércoles", HoraInicio = new TimeSpan(8,  0, 0), HoraFin = new TimeSpan(10, 0, 0), Pabellon = "B", Aula = "102" },
                    new HorarioCurso { CursoId = c0[1].Id, Tipo = "Teórico",  Dia = "Martes",    HoraInicio = new TimeSpan(10, 0, 0), HoraFin = new TimeSpan(12, 0, 0), Pabellon = "A", Aula = "3"   },
                    new HorarioCurso { CursoId = c0[1].Id, Tipo = "Práctico", Dia = "Jueves",    HoraInicio = new TimeSpan(14, 0, 0), HoraFin = new TimeSpan(16, 0, 0), Pabellon = "B", Aula = "101" },
                    new HorarioCurso { CursoId = c0[2].Id, Tipo = "Teórico",  Dia = "Miércoles", HoraInicio = new TimeSpan(14, 0, 0), HoraFin = new TimeSpan(16, 0, 0), Pabellon = "A", Aula = "105" },
                    new HorarioCurso { CursoId = c0[3].Id, Tipo = "Teórico",  Dia = "Viernes",   HoraInicio = new TimeSpan(8,  0, 0), HoraFin = new TimeSpan(10, 0, 0), Pabellon = "A", Aula = "202" },
                    new HorarioCurso { CursoId = c0[4].Id, Tipo = "Teórico",  Dia = "Lunes",     HoraInicio = new TimeSpan(12, 0, 0), HoraFin = new TimeSpan(14, 0, 0), Pabellon = "C", Aula = "301" },
                    new HorarioCurso { CursoId = c0[5].Id, Tipo = "Teórico",  Dia = "Martes",    HoraInicio = new TimeSpan(8,  0, 0), HoraFin = new TimeSpan(10, 0, 0), Pabellon = "C", Aula = "302" },
                    new HorarioCurso { CursoId = c0[5].Id, Tipo = "Práctico", Dia = "Jueves",    HoraInicio = new TimeSpan(8,  0, 0), HoraFin = new TimeSpan(10, 0, 0), Pabellon = "C", Aula = "Lab1"},
                    new HorarioCurso { CursoId = c0[6].Id, Tipo = "Teórico",  Dia = "Miércoles", HoraInicio = new TimeSpan(10, 0, 0), HoraFin = new TimeSpan(12, 0, 0), Pabellon = "D", Aula = "401" },
                    new HorarioCurso { CursoId = c0[7].Id, Tipo = "Teórico",  Dia = "Lunes",     HoraInicio = new TimeSpan(16, 0, 0), HoraFin = new TimeSpan(18, 0, 0), Pabellon = "D", Aula = "402" },
                    new HorarioCurso { CursoId = c0[8].Id, Tipo = "Teórico",  Dia = "Lunes",     HoraInicio = new TimeSpan(8,  0, 0), HoraFin = new TimeSpan(10, 0, 0), Pabellon = "E", Aula = "501" },
                    new HorarioCurso { CursoId = c0[8].Id, Tipo = "Práctico", Dia = "Miércoles", HoraInicio = new TimeSpan(12, 0, 0), HoraFin = new TimeSpan(14, 0, 0), Pabellon = "E", Aula = "Lab2"},
                    new HorarioCurso { CursoId = c0[9].Id, Tipo = "Teórico",  Dia = "Viernes",   HoraInicio = new TimeSpan(10, 0, 0), HoraFin = new TimeSpan(12, 0, 0), Pabellon = "E", Aula = "502" }
                );

                void Inscribir(Curso curso, params ApplicationUser?[] alumnos)
                {
                    foreach (var a in alumnos)
                        if (a != null)
                            context.CursoAlumnos.Add(new CursoAlumno { CursoId = curso.Id, AlumnoId = a.Id });
                }

                Inscribir(c0[0], aMaria, aPedro, aKevin);
                Inscribir(c0[1], aMaria, aPedro);
                Inscribir(c0[2], aMaria, aPedro, aKevin);
                Inscribir(c0[3], aMaria, aKevin);
                Inscribir(c0[4], aSofia, aCarlosR);
                Inscribir(c0[5], aSofia, aCarlosR);
                Inscribir(c0[6], aDiego, aPaola);
                Inscribir(c0[7], aDiego, aPaola);
                Inscribir(c0[8], aLuis);
                Inscribir(c0[9], aLuis);
                await context.SaveChangesAsync();
            }

            // ----- Cursos expandidos (si aún no existen) -----
            if (!await context.Cursos.AnyAsync(c => c.Nombre == "Robótica" && c.Carrera == "Ingeniería Mecatrónica"))
            {
                var cx = new List<Curso>
                {
                    // Ingeniería en Sistemas
                    new Curso { Nombre = "Redes de Computadoras",           Descripcion = "Arquitectura TCP/IP, routing y switching",                 Carrera = "Ingeniería en Sistemas", Semestre = "III", DocenteId = dAlberto?.Id  },
                    new Curso { Nombre = "Inteligencia Artificial",          Descripcion = "Machine learning, redes neuronales y NLP",                 Carrera = "Ingeniería en Sistemas", Semestre = "VI",  DocenteId = dJuan?.Id     },
                    new Curso { Nombre = "Ingeniería de Software",           Descripcion = "Metodologías ágiles, UML y gestión de proyectos",          Carrera = "Ingeniería en Sistemas", Semestre = "V",   DocenteId = dAlberto?.Id  },
                    // Ingeniería Civil
                    new Curso { Nombre = "Cálculo Estructural",              Descripcion = "Análisis de estructuras estáticas e isostáticas",          Carrera = "Ingeniería Civil",       Semestre = "V",   DocenteId = dJorge?.Id    },
                    new Curso { Nombre = "Hidráulica",                       Descripcion = "Mecánica de fluidos aplicada a obras civiles",             Carrera = "Ingeniería Civil",       Semestre = "IV",  DocenteId = dJorge?.Id    },
                    // Ingeniería Mecatrónica
                    new Curso { Nombre = "Robótica",                         Descripcion = "Programación y control de robots industriales",            Carrera = "Ingeniería Mecatrónica", Semestre = "V",   DocenteId = dMiguel?.Id   },
                    new Curso { Nombre = "Control Automático",               Descripcion = "Sistemas de control en lazo abierto y cerrado",            Carrera = "Ingeniería Mecatrónica", Semestre = "IV",  DocenteId = dMiguel?.Id   },
                    new Curso { Nombre = "Electrónica Digital",              Descripcion = "Circuitos digitales, compuertas lógicas y microcontroladores", Carrera = "Ingeniería Mecatrónica", Semestre = "III", DocenteId = dRoberto?.Id  },
                    new Curso { Nombre = "Diseño Mecánico",                  Descripcion = "CAD y análisis de elementos mecánicos",                    Carrera = "Ingeniería Mecatrónica", Semestre = "II",  DocenteId = dMiguel?.Id   },
                    // Ingeniería Industrial
                    new Curso { Nombre = "Gestión de Operaciones",           Descripcion = "Planificación y control de la producción",                 Carrera = "Ingeniería Industrial",  Semestre = "IV",  DocenteId = dMiguel?.Id   },
                    new Curso { Nombre = "Estadística Industrial",           Descripcion = "Control estadístico de procesos y muestreo",               Carrera = "Ingeniería Industrial",  Semestre = "III", DocenteId = dAna?.Id      },
                    new Curso { Nombre = "Logística y Cadena de Suministro", Descripcion = "Gestión de inventarios y distribución",                    Carrera = "Ingeniería Industrial",  Semestre = "V",   DocenteId = dFernando?.Id },
                    new Curso { Nombre = "Calidad Total",                    Descripcion = "ISO 9001, Six Sigma y mejora continua",                    Carrera = "Ingeniería Industrial",  Semestre = "VI",  DocenteId = dMiguel?.Id   },
                    // Derecho
                    new Curso { Nombre = "Derecho Penal",                    Descripcion = "Teoría del delito y sistema punitivo",                     Carrera = "Derecho",                Semestre = "III", DocenteId = dCarlos?.Id   },
                    new Curso { Nombre = "Derecho Procesal Civil",           Descripcion = "Procesos judiciales civiles y recursos",                   Carrera = "Derecho",                Semestre = "V",   DocenteId = dFernando?.Id },
                    // Psicología
                    new Curso { Nombre = "Psicología General",               Descripcion = "Fundamentos del comportamiento humano",                    Carrera = "Psicología",             Semestre = "I",   DocenteId = dSandra?.Id   },
                    new Curso { Nombre = "Psicología Clínica",               Descripcion = "Diagnóstico y tratamiento de trastornos psicológicos",     Carrera = "Psicología",             Semestre = "IV",  DocenteId = dSandra?.Id   },
                    new Curso { Nombre = "Neuropsicología",                  Descripcion = "Relación cerebro-comportamiento y evaluación cognitiva",   Carrera = "Psicología",             Semestre = "V",   DocenteId = dSandra?.Id   },
                    new Curso { Nombre = "Psicología del Desarrollo",        Descripcion = "Etapas evolutivas desde la infancia hasta la vejez",       Carrera = "Psicología",             Semestre = "III", DocenteId = dLucia?.Id    },
                    // Administración de Empresas
                    new Curso { Nombre = "Contabilidad General",             Descripcion = "Principios contables y estados financieros",               Carrera = "Administración de Empresas", Semestre = "I",  DocenteId = dFernando?.Id },
                    new Curso { Nombre = "Marketing Estratégico",            Descripcion = "Segmentación, posicionamiento y mezcla de marketing",      Carrera = "Administración de Empresas", Semestre = "IV", DocenteId = dAna?.Id      },
                    new Curso { Nombre = "Finanzas Corporativas",            Descripcion = "Valoración de empresas y gestión de capital",              Carrera = "Administración de Empresas", Semestre = "V",  DocenteId = dFernando?.Id },
                    new Curso { Nombre = "Gestión de Recursos Humanos",      Descripcion = "Reclutamiento, capacitación y clima organizacional",       Carrera = "Administración de Empresas", Semestre = "III",DocenteId = dFernando?.Id },
                    // Comunicación Social
                    new Curso { Nombre = "Periodismo Digital",               Descripcion = "Redacción periodística y medios digitales",                Carrera = "Comunicación Social",    Semestre = "III", DocenteId = dSandra?.Id   },
                    new Curso { Nombre = "Producción Audiovisual",           Descripcion = "Fotografía, video y edición multimedia",                   Carrera = "Comunicación Social",    Semestre = "IV",  DocenteId = dSandra?.Id   },
                    new Curso { Nombre = "Relaciones Públicas",              Descripcion = "Comunicación corporativa y manejo de crisis",              Carrera = "Comunicación Social",    Semestre = "V",   DocenteId = dLucia?.Id    },
                    new Curso { Nombre = "Comunicación Digital",             Descripcion = "Redes sociales, SEO y estrategia de contenidos",          Carrera = "Comunicación Social",    Semestre = "II",  DocenteId = dSandra?.Id   },
                    // Medicina
                    new Curso { Nombre = "Fisiología Humana",                Descripcion = "Funcionamiento de los sistemas del organismo",             Carrera = "Medicina",               Semestre = "III", DocenteId = dPatricia?.Id },
                    new Curso { Nombre = "Farmacología",                     Descripcion = "Mecanismos de acción y uso clínico de fármacos",          Carrera = "Medicina",               Semestre = "IV",  DocenteId = dPatricia?.Id },
                    // Enfermería
                    new Curso { Nombre = "Enfermería Básica",                Descripcion = "Procedimientos fundamentales de atención de enfermería",   Carrera = "Enfermería",             Semestre = "I",   DocenteId = dPatricia?.Id },
                    new Curso { Nombre = "Cuidados Intensivos",              Descripcion = "Manejo del paciente crítico en UCI",                       Carrera = "Enfermería",             Semestre = "V",   DocenteId = dLucia?.Id    },
                    new Curso { Nombre = "Salud Pública",                    Descripcion = "Epidemiología y promoción de la salud comunitaria",        Carrera = "Enfermería",             Semestre = "III", DocenteId = dPatricia?.Id },
                    new Curso { Nombre = "Farmacología de Enfermería",       Descripcion = "Administración de medicamentos y cálculo de dosis",        Carrera = "Enfermería",             Semestre = "IV",  DocenteId = dCarmen?.Id   },
                    // Odontología
                    new Curso { Nombre = "Anatomía Dental",                  Descripcion = "Morfología y estructura de las piezas dentales",           Carrera = "Odontología",            Semestre = "I",   DocenteId = dCarmen?.Id   },
                    new Curso { Nombre = "Periodoncia",                      Descripcion = "Tratamiento de enfermedades de las encías",                Carrera = "Odontología",            Semestre = "IV",  DocenteId = dCarmen?.Id   },
                    new Curso { Nombre = "Ortodoncia",                       Descripcion = "Corrección de maloclusiones dentales",                     Carrera = "Odontología",            Semestre = "V",   DocenteId = dCarmen?.Id   },
                    new Curso { Nombre = "Cirugía Oral",                     Descripcion = "Exodoncias, implantes y cirugía maxilofacial",             Carrera = "Odontología",            Semestre = "VI",  DocenteId = dRoberto?.Id  },
                    // Nutrición
                    new Curso { Nombre = "Nutrición Básica",                 Descripcion = "Macronutrientes, micronutrientes y requerimientos",        Carrera = "Nutrición",              Semestre = "I",   DocenteId = dCarmen?.Id   },
                    new Curso { Nombre = "Bioquímica Nutricional",           Descripcion = "Metabolismo de carbohidratos, lípidos y proteínas",        Carrera = "Nutrición",              Semestre = "II",  DocenteId = dCarlos?.Id   },
                    new Curso { Nombre = "Dietoterapia",                     Descripcion = "Dietas terapéuticas para enfermedades crónicas",           Carrera = "Nutrición",              Semestre = "IV",  DocenteId = dCarmen?.Id   },
                    new Curso { Nombre = "Alimentación Comunitaria",         Descripcion = "Programas de nutrición y seguridad alimentaria",           Carrera = "Nutrición",              Semestre = "V",   DocenteId = dPatricia?.Id },
                };
                context.Cursos.AddRange(cx);
                await context.SaveChangesAsync();

                // Horarios para cursos expandidos
                var horarios = new List<HorarioCurso>();
                string[] dias = { "Lunes", "Martes", "Miércoles", "Jueves", "Viernes" };
                int[] horas  = { 8, 10, 12, 14, 16 };
                string[] pabs = { "A", "B", "C", "D", "E" };

                for (int i = 0; i < cx.Count; i++)
                {
                    horarios.Add(new HorarioCurso
                    {
                        CursoId    = cx[i].Id,
                        Tipo       = "Teórico",
                        Dia        = dias[i % 5],
                        HoraInicio = new TimeSpan(horas[i % 5], 0, 0),
                        HoraFin    = new TimeSpan(horas[i % 5] + 2, 0, 0),
                        Pabellon   = pabs[i % 5],
                        Aula       = $"{(i % 5 + 1) * 100 + (i % 10 + 1)}"
                    });
                }
                context.HorariosCurso.AddRange(horarios);

                // Inscripciones para cursos expandidos
                void InscribirEx(Curso curso, params ApplicationUser?[] alumnos)
                {
                    foreach (var a in alumnos)
                        if (a != null)
                            context.CursoAlumnos.Add(new CursoAlumno { CursoId = curso.Id, AlumnoId = a.Id });
                }

                // Sistemas
                InscribirEx(cx[0], aMaria, aPedro, aKevin);
                InscribirEx(cx[1], aMaria, aKevin);
                InscribirEx(cx[2], aPedro, aKevin);
                // Civil
                InscribirEx(cx[3], aSofia, aCarlosR);
                InscribirEx(cx[4], aSofia, aCarlosR);
                // Mecatrónica
                InscribirEx(cx[5], aAndres, aLuciaC, aRafael);
                InscribirEx(cx[6], aAndres, aLuciaC, aRafael);
                InscribirEx(cx[7], aAndres, aRafael);
                InscribirEx(cx[8], aLuciaC, aRafael);
                // Industrial
                InscribirEx(cx[9],  aValeria, aAnaR, aMiguelS);
                InscribirEx(cx[10], aValeria, aAnaR, aMiguelS);
                InscribirEx(cx[11], aValeria, aMiguelS);
                InscribirEx(cx[12], aAnaR,    aMiguelS);
                // Derecho
                InscribirEx(cx[13], aDiego, aPaola);
                InscribirEx(cx[14], aDiego, aPaola);
                // Psicología
                InscribirEx(cx[15], aCamila, aFernanda, aSebastian);
                InscribirEx(cx[16], aCamila, aFernanda);
                InscribirEx(cx[17], aCamila, aSebastian);
                InscribirEx(cx[18], aFernanda, aSebastian);
                // Administración
                InscribirEx(cx[19], aJoseM, aAlejandra, aNicolas);
                InscribirEx(cx[20], aJoseM, aAlejandra, aNicolas);
                InscribirEx(cx[21], aJoseM, aNicolas);
                InscribirEx(cx[22], aAlejandra, aNicolas);
                // Comunicación
                InscribirEx(cx[23], aDaniela, aValentina, aPablo);
                InscribirEx(cx[24], aDaniela, aValentina);
                InscribirEx(cx[25], aDaniela, aPablo);
                InscribirEx(cx[26], aValentina, aPablo);
                // Medicina
                InscribirEx(cx[27], aLuis);
                InscribirEx(cx[28], aLuis);
                // Enfermería
                InscribirEx(cx[29], aGabriela, aAnaMamani);
                InscribirEx(cx[30], aGabriela, aAnaMamani);
                InscribirEx(cx[31], aGabriela, aAnaMamani);
                InscribirEx(cx[32], aAnaMamani);
                // Odontología
                InscribirEx(cx[33], aMarco, aJoseQ);
                InscribirEx(cx[34], aMarco, aJoseQ);
                InscribirEx(cx[35], aMarco);
                InscribirEx(cx[36], aJoseQ);
                // Nutrición
                InscribirEx(cx[37], aIsabela, aCarla);
                InscribirEx(cx[38], aIsabela, aCarla);
                InscribirEx(cx[39], aIsabela, aCarla);
                InscribirEx(cx[40], aCarla);

                await context.SaveChangesAsync();
            }

            // ----- Eventos -----
            if (!await context.Eventos.AnyAsync())
            {
                context.Eventos.AddRange(
                    new Evento { Titulo = "Semana de Bienvenida",             Descripcion = "Actividades de integración para nuevos estudiantes",      Fecha = DateTime.Today.AddDays(5),  Hora = new TimeSpan(9,  0, 0), Lugar = "Plaza Central",        TipoEvento = "Actividad",  EsGeneral = true  },
                    new Evento { Titulo = "Feria de Clubs",                   Descripcion = "Presentación de clubs y actividades extracurriculares",   Fecha = DateTime.Today.AddDays(10), Hora = new TimeSpan(10, 0, 0), Lugar = "Patio Principal",      TipoEvento = "Feria",      EsGeneral = true  },
                    new Evento { Titulo = "Charla de IA",                     Descripcion = "Introducción a la inteligencia artificial",               Fecha = DateTime.Today.AddDays(3),  Hora = new TimeSpan(16, 0, 0), Lugar = "Aula Magna",           TipoEvento = "Charla",     EsGeneral = false, Carrera = "Ingeniería en Sistemas" },
                    new Evento { Titulo = "Hackathon UNI",                    Descripcion = "48 horas de programación y desarrollo de proyectos",      Fecha = DateTime.Today.AddDays(7),  Hora = new TimeSpan(9,  0, 0), Lugar = "Auditorio Central",    TipoEvento = "Hackathon",  EsGeneral = false, Carrera = "Ingeniería en Sistemas" },
                    new Evento { Titulo = "Taller de Git",                    Descripcion = "Control de versiones con Git y GitHub",                  Fecha = DateTime.Today.AddDays(12), Hora = new TimeSpan(11, 0, 0), Lugar = "Laboratorio 2",        TipoEvento = "Taller",     EsGeneral = false, Carrera = "Ingeniería en Sistemas" },
                    new Evento { Titulo = "Simulacro de Juicio Oral",         Descripcion = "Práctica de litigación oral en el aula de juicios",      Fecha = DateTime.Today.AddDays(4),  Hora = new TimeSpan(15, 0, 0), Lugar = "Aula de Juicios",      TipoEvento = "Práctica",   EsGeneral = false, Carrera = "Derecho" },
                    new Evento { Titulo = "Charla: Derechos Humanos",         Descripcion = "Conferencia sobre derechos humanos y jurisprudencia",     Fecha = DateTime.Today.AddDays(9),  Hora = new TimeSpan(17, 0, 0), Lugar = "Auditorio D",          TipoEvento = "Charla",     EsGeneral = false, Carrera = "Derecho" },
                    new Evento { Titulo = "Jornada de Salud Comunitaria",     Descripcion = "Atención preventiva en comunidades cercanas",            Fecha = DateTime.Today.AddDays(6),  Hora = new TimeSpan(8,  0, 0), Lugar = "Centro de Salud",      TipoEvento = "Jornada",    EsGeneral = false, Carrera = "Medicina" }
                );
                await context.SaveChangesAsync();
            }

            // ----- Eventos adicionales -----
            if (!await context.Eventos.AnyAsync(e => e.Titulo == "Feria de Robótica e Innovación"))
            {
                context.Eventos.AddRange(
                    new Evento { Titulo = "Día del Deporte Universitario",    Descripcion = "Competencias deportivas entre facultades",                Fecha = DateTime.Today.AddDays(15), Hora = new TimeSpan(8,  0, 0), Lugar = "Estadio Universitario",TipoEvento = "Deporte",    EsGeneral = true  },
                    new Evento { Titulo = "Ceremonia de Graduación 2026",     Descripcion = "Ceremonia de egreso de la promoción 2026",               Fecha = DateTime.Today.AddDays(60), Hora = new TimeSpan(18, 0, 0), Lugar = "Auditorio Principal",  TipoEvento = "Ceremonia",  EsGeneral = true  },
                    new Evento { Titulo = "Workshop de Ciberseguridad",       Descripcion = "Técnicas de hacking ético y seguridad informática",      Fecha = DateTime.Today.AddDays(14), Hora = new TimeSpan(14, 0, 0), Lugar = "Laboratorio 1",        TipoEvento = "Taller",     EsGeneral = false, Carrera = "Ingeniería en Sistemas"  },
                    new Evento { Titulo = "Congreso de Ingeniería Civil",     Descripcion = "Presentación de proyectos de infraestructura y obras",   Fecha = DateTime.Today.AddDays(20), Hora = new TimeSpan(9,  0, 0), Lugar = "Auditorio C",          TipoEvento = "Congreso",   EsGeneral = false, Carrera = "Ingeniería Civil"        },
                    new Evento { Titulo = "Feria de Robótica e Innovación",   Descripcion = "Exhibición de robots y proyectos de automatización",     Fecha = DateTime.Today.AddDays(18), Hora = new TimeSpan(10, 0, 0), Lugar = "Pabellón B",           TipoEvento = "Feria",      EsGeneral = false, Carrera = "Ingeniería Mecatrónica"  },
                    new Evento { Titulo = "Congreso de Gestión Industrial",   Descripcion = "Tendencias en manufactura y gestión de operaciones",     Fecha = DateTime.Today.AddDays(22), Hora = new TimeSpan(9,  0, 0), Lugar = "Sala de Conferencias", TipoEvento = "Congreso",   EsGeneral = false, Carrera = "Ingeniería Industrial"   },
                    new Evento { Titulo = "Congreso de Salud Mental",         Descripcion = "Psicología positiva, bienestar y prevención",            Fecha = DateTime.Today.AddDays(11), Hora = new TimeSpan(16, 0, 0), Lugar = "Auditorio B",          TipoEvento = "Congreso",   EsGeneral = false, Carrera = "Psicología"              },
                    new Evento { Titulo = "Feria Empresarial Universitaria",  Descripcion = "Emprendimientos y networking con empresas del sector",   Fecha = DateTime.Today.AddDays(25), Hora = new TimeSpan(10, 0, 0), Lugar = "Plaza Central",        TipoEvento = "Feria",      EsGeneral = false, Carrera = "Administración de Empresas"},
                    new Evento { Titulo = "Festival de Cortometrajes",        Descripcion = "Proyección de producciones audiovisuales estudiantiles", Fecha = DateTime.Today.AddDays(13), Hora = new TimeSpan(18, 0, 0), Lugar = "Auditorio Principal",  TipoEvento = "Cultural",   EsGeneral = false, Carrera = "Comunicación Social"     },
                    new Evento { Titulo = "Congreso Médico Estudiantil",      Descripcion = "Presentación de casos clínicos e investigaciones",       Fecha = DateTime.Today.AddDays(30), Hora = new TimeSpan(8,  0, 0), Lugar = "Auditorio E",          TipoEvento = "Congreso",   EsGeneral = false, Carrera = "Medicina"                },
                    new Evento { Titulo = "Jornada Nacional de Enfermería",   Descripcion = "Cuidados paliativos y atención humanizada al paciente",  Fecha = DateTime.Today.AddDays(16), Hora = new TimeSpan(9,  0, 0), Lugar = "Sala Polivalente",     TipoEvento = "Jornada",    EsGeneral = false, Carrera = "Enfermería"              },
                    new Evento { Titulo = "Jornada de Salud Oral",            Descripcion = "Atención dental gratuita y charlas de prevención",       Fecha = DateTime.Today.AddDays(8),  Hora = new TimeSpan(8,  0, 0), Lugar = "Clínica Dental Univ.", TipoEvento = "Jornada",    EsGeneral = false, Carrera = "Odontología"             },
                    new Evento { Titulo = "Feria de Alimentación Saludable",  Descripcion = "Talleres de nutrición, recetas y hábitos saludables",    Fecha = DateTime.Today.AddDays(17), Hora = new TimeSpan(10, 0, 0), Lugar = "Patio Central",        TipoEvento = "Feria",      EsGeneral = false, Carrera = "Nutrición"               }
                );
                await context.SaveChangesAsync();
            }

            // ----- Lugares -----
            if (!await context.Lugares.AnyAsync())
            {
                context.Lugares.AddRange(
                    new Lugar { Nombre = "Biblioteca Central",         Tipo = "Biblioteca",      Direccion = "Pabellón Central, 1er piso",   Distancia = 0.1, PrecioPromedio = 0,    Calificacion = 4.8, Descripcion = "Biblioteca universitaria con más de 50,000 volúmenes, salas de lectura silenciosa y acceso a bases de datos académicas.",    ImagenUrl = "" },
                    new Lugar { Nombre = "Cafetería Central",          Tipo = "Cafetería",       Direccion = "Pabellón A, planta baja",      Distancia = 0.2, PrecioPromedio = 12,   Calificacion = 4.2, Descripcion = "Cafetería principal con menú variado, desayunos, almuerzos y snacks. Precios accesibles para estudiantes.",                  ImagenUrl = "" },
                    new Lugar { Nombre = "Laboratorio de Cómputo 1",   Tipo = "Laboratorio",     Direccion = "Pabellón B, 2do piso",         Distancia = 0.3, PrecioPromedio = 0,    Calificacion = 4.5, Descripcion = "40 computadoras con software especializado para ingeniería, programación y diseño. Acceso con carné universitario.",           ImagenUrl = "" },
                    new Lugar { Nombre = "Laboratorio de Cómputo 2",   Tipo = "Laboratorio",     Direccion = "Pabellón B, 3er piso",         Distancia = 0.3, PrecioPromedio = 0,    Calificacion = 4.4, Descripcion = "30 computadoras de alto rendimiento para simulaciones, CAD y procesamiento de datos.",                                        ImagenUrl = "" },
                    new Lugar { Nombre = "Sala de Estudios A",         Tipo = "Zona de estudio", Direccion = "Pabellón Central, 2do piso",   Distancia = 0.1, PrecioPromedio = 0,    Calificacion = 4.6, Descripcion = "Sala silenciosa con 60 puestos de estudio individual, enchufes y Wi-Fi de alta velocidad. Disponible las 24 horas.",          ImagenUrl = "" },
                    new Lugar { Nombre = "Sala de Estudios B",         Tipo = "Zona de estudio", Direccion = "Pabellón D, 1er piso",         Distancia = 0.4, PrecioPromedio = 0,    Calificacion = 4.3, Descripcion = "Sala de estudios grupal con pizarras y proyector. Ideal para trabajos en equipo y exposiciones.",                            ImagenUrl = "" },
                    new Lugar { Nombre = "Coworking Estudiantil",      Tipo = "Coworking",       Direccion = "Pabellón E, planta baja",      Distancia = 0.5, PrecioPromedio = 0,    Calificacion = 4.7, Descripcion = "Espacio de coworking moderno con mesas altas, sofás, café y ambiente informal. Perfecto para proyectos creativos.",           ImagenUrl = "" },
                    new Lugar { Nombre = "Auditorio Principal",        Tipo = "Auditorio",       Direccion = "Edificio Cultural, 1er piso",  Distancia = 0.6, PrecioPromedio = 0,    Calificacion = 4.9, Descripcion = "Auditorio con capacidad para 500 personas, sistema de sonido profesional y pantalla de proyección de gran formato.",          ImagenUrl = "" },
                    new Lugar { Nombre = "Cancha Deportiva",           Tipo = "Deportes",        Direccion = "Zona deportiva, acceso norte", Distancia = 0.8, PrecioPromedio = 0,    Calificacion = 4.1, Descripcion = "Complejo deportivo con canchas de fútbol, vóley y básquet. Reservas disponibles a través de la app UniLife.",                  ImagenUrl = "" },
                    new Lugar { Nombre = "Centro Médico Universitario",Tipo = "Centro médico",   Direccion = "Pabellón F, planta baja",      Distancia = 0.4, PrecioPromedio = 5,    Calificacion = 4.5, Descripcion = "Atención médica primaria, psicológica y odontológica para estudiantes. Consultas a precio preferencial con carné vigente.", ImagenUrl = "" }
                );
                await context.SaveChangesAsync();
            }

            // Corregir eventos tecnológicos mal categorizados en versiones anteriores
            var eventosTechMal = await context.Eventos
                .Where(e => (e.Titulo == "Charla de IA" || e.Titulo == "Hackathon UNI" || e.Titulo == "Taller de Git") && e.EsGeneral)
                .ToListAsync();
            foreach (var ev in eventosTechMal) { ev.EsGeneral = false; ev.Carrera = "Ingeniería en Sistemas"; }
            if (eventosTechMal.Any()) await context.SaveChangesAsync();

            // Eliminar tareas de ejemplo vencidas del seed anterior
            var tareasEjemplo = new[] { "Entrega proyecto final", "Leer capítulo 5", "Resolver ejercicios", "Quiz de lógica" };
            var tareasViejas  = context.Tareas.Where(t => tareasEjemplo.Contains(t.Titulo));
            if (await tareasViejas.AnyAsync()) { context.Tareas.RemoveRange(tareasViejas); await context.SaveChangesAsync(); }
        }
    }
}
