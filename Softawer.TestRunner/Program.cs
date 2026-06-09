using Softawer.Models;
using Softawer.Services;

var failures = new List<string>();

Run("Schema contains required tables and relationships", TestSchema);
Run("BMI calculation and category mapping", TestImc);
Run("Password hashing and verification", TestPasswordHashing);
Run("Routine copy creates isolated editable clone", TestRoutineCopy);
Run("Reservation overlap and machine-state validation", TestReservationPolicy);
Run("Reservation date validation", TestReservationDatePolicy);
Run("Reservation cancellation validation", TestReservationCancellationPolicy);
Run("Workout session flow allows start, detail and completion", TestSesionFlow);

if (failures.Count > 0)
{
    Console.Error.WriteLine("TESTS FAILED");
    foreach (var failure in failures)
    {
        Console.Error.WriteLine($" - {failure}");
    }

    Environment.Exit(1);
}

Console.WriteLine("ALL TESTS PASSED");

void Run(string name, Action test)
{
    try
    {
        test();
        Console.WriteLine($"PASS: {name}");
    }
    catch (Exception exception)
    {
        failures.Add($"{name}: {exception.Message}");
    }
}

void TestSchema()
{
    var sql = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "database.sql", "TablasGym.sql"));
    AssertContains(sql, "CREATE TABLE usuarios");
    AssertContains(sql, "CREATE TABLE perfil_usuario");
    AssertContains(sql, "alias VARCHAR(100) NULL");
    AssertContains(sql, "avatar_url VARCHAR(500) NULL");
    AssertContains(sql, "telefono_trabajo VARCHAR(20) NULL");
    AssertContains(sql, "email_trabajo VARCHAR(100) NULL");
    AssertContains(sql, "sitio_personal VARCHAR(200) NULL");
    AssertContains(sql, "twitter VARCHAR(50) NULL");
    AssertContains(sql, "CREATE TABLE historial_imc");
    AssertContains(sql, "CREATE TABLE ejercicios");
    AssertContains(sql, "CREATE TABLE rutina_ejercicio");
    AssertContains(sql, "CREATE TABLE reserva_cancelaciones");
    AssertContains(sql, "CREATE TABLE sesiones_entrenamiento");
    AssertContains(sql, "CREATE TABLE detalle_sesion_entrenamiento");
    AssertContains(sql, "FOREIGN KEY (id_usuario) REFERENCES usuarios(id_usuario)");
    AssertContains(sql, "FOREIGN KEY (id_maquina) REFERENCES maquinas(id_maquina)");
    AssertContains(sql, "FOREIGN KEY (id_reserva) REFERENCES reservas(id_reserva)");
    AssertContains(sql, "FOREIGN KEY (id_rutina_origen) REFERENCES rutinas(id_rutina)");
}

void TestImc()
{
    var imcService = new ImcService();
    var imcNormal = imcService.CalcularImc(175, 70);
    AssertEqual(22.86m, imcNormal, "El IMC normal no coincide.");
    AssertEqual("normal", imcService.ClasificarImc(imcNormal), "La categoria normal no coincide.");
    AssertEqual("sobrepeso", imcService.ClasificarImc(27), "La categoria sobrepeso no coincide.");
    AssertEqual("obesidad", imcService.ClasificarImc(31), "La categoria obesidad no coincide.");
}

void TestPasswordHashing()
{
    var hashService = new PasswordHashService();
    var hash = hashService.HashPassword("1234");
    AssertTrue(hash.StartsWith("PBKDF2$"), "El hash no tiene el prefijo esperado.");
    AssertTrue(hashService.VerifyPassword("1234", hash), "La verificacion del password correcto fallo.");
    AssertTrue(!hashService.VerifyPassword("4321", hash), "La verificacion del password incorrecto debio fallar.");
}

void TestRoutineCopy()
{
    var source = new RutinaDetalleResponse
    {
        IdRutina = 12,
        IdUsuario = null,
        Nombre = "Fuerza principiante",
        Descripcion = "Original",
        TipoRutina = "predefinida",
        Objetivo = "ganar_fuerza",
        Dificultad = "principiante",
        EsPublica = true,
        Ejercicios =
        [
            new RutinaEjercicio
            {
                IdEjercicio = 3,
                Dia = "Lunes",
                Orden = 1,
                Series = 4,
                Repeticiones = 10,
                NombreEjercicio = "Curl"
            }
        ]
    };

    var copyService = new RutinaCopyService();
    var copia = copyService.CrearCopiaEditable(source, 99);

    AssertEqual("copiada", copia.TipoRutina, "La rutina copiada debe marcarse como copiada.");
    AssertEqual(99, copia.IdUsuario!.Value, "La rutina copiada debe pertenecer al usuario.");
    AssertEqual(source.IdRutina, copia.IdRutinaOrigen!.Value, "La rutina copiada debe guardar su origen.");

    copia.Ejercicios[0].Series = 8;
    AssertEqual(4, source.Ejercicios[0].Series!.Value, "Editar la copia no debe modificar la original.");
}

void TestReservationPolicy()
{
    var service = new ReservaPolicyService();
    var maquina = new Maquina { Estado = "disponible" };
    var today = DateOnly.FromDateTime(DateTime.Today);
    var reservas = new List<Reserva>
    {
        new()
        {
            Estado = "activa",
            HoraInicio = new TimeOnly(10, 0),
            HoraFin = new TimeOnly(11, 0)
        }
    };

    var ok = service.ValidarReserva(maquina, reservas, new CrearReservaRequest
    {
        FechaReserva = today,
        HoraInicio = new TimeOnly(11, 0),
        HoraFin = new TimeOnly(12, 0)
    }, today);
    AssertTrue(ok is null, "Una reserva sin cruce deberia permitirse.");

    var overlap = service.ValidarReserva(maquina, reservas, new CrearReservaRequest
    {
        FechaReserva = today,
        HoraInicio = new TimeOnly(10, 30),
        HoraFin = new TimeOnly(11, 30)
    }, today);
    AssertTrue(overlap is not null, "Una reserva traslapada debio rechazarse.");

    var machineState = service.ValidarReserva(new Maquina { Estado = "mantencion" }, [], new CrearReservaRequest
    {
        FechaReserva = today,
        HoraInicio = new TimeOnly(12, 0),
        HoraFin = new TimeOnly(13, 0)
    }, today);
    AssertTrue(machineState is not null, "Una maquina en mantencion debio rechazarse.");
}

void TestReservationDatePolicy()
{
    var service = new ReservaPolicyService();
    var today = DateOnly.FromDateTime(DateTime.Today);
    var maquina = new Maquina { Estado = "disponible" };

    var past = service.ValidarReserva(maquina, [], new CrearReservaRequest
    {
        FechaReserva = today.AddDays(-1),
        HoraInicio = new TimeOnly(12, 0),
        HoraFin = new TimeOnly(13, 0)
    }, today);
    AssertTrue(past is not null, "Una reserva con fecha pasada debio rechazarse.");

    var currentDate = service.ValidarReserva(maquina, [], new CrearReservaRequest
    {
        FechaReserva = today,
        HoraInicio = new TimeOnly(12, 0),
        HoraFin = new TimeOnly(13, 0)
    }, today);
    AssertTrue(currentDate is null, "Una reserva para hoy deberia permitirse.");
}

void TestReservationCancellationPolicy()
{
    var service = new ReservaPolicyService();
    var today = DateOnly.FromDateTime(DateTime.Today);
    var reserva = new Reserva
    {
        IdUsuario = 7,
        Estado = "activa",
        FechaReserva = today
    };

    AssertTrue(service.ValidarCancelacion(reserva, 7, today) is null, "El usuario duenio deberia poder cancelar una reserva activa de hoy.");
    AssertTrue(service.ValidarCancelacion(reserva, 8, today) is not null, "Una reserva ajena debio rechazarse.");

    reserva.Estado = "cancelada";
    AssertTrue(service.ValidarCancelacion(reserva, 7, today) is not null, "Una reserva ya cancelada debio rechazarse.");

    reserva.Estado = "activa";
    reserva.FechaReserva = today.AddDays(-1);
    AssertTrue(service.ValidarCancelacion(reserva, 7, today) is not null, "Una reserva pasada debio rechazarse al cancelar.");
}

void TestSesionFlow()
{
    var service = new SesionEntrenamientoService();
    var sesion = service.CrearSesion(new IniciarSesionRequest
    {
        IdUsuario = 1,
        IdRutina = 2,
        Notas = "Inicio de prueba"
    });

    AssertEqual("en_progreso", sesion.Estado, "La sesion nueva debe iniciar en progreso.");

    var detalle = service.CrearDetalle(10, new AgregarDetalleSesionRequest
    {
        IdEjercicio = 5,
        SeriesRealizadas = 4,
        RepeticionesRealizadas = 12
    });
    AssertEqual(10, detalle.IdSesion, "El detalle debe quedar asociado a la sesion.");
    AssertEqual(5, detalle.IdEjercicio, "El detalle debe conservar el ejercicio.");

    service.CompletarSesion(sesion, new CompletarSesionRequest
    {
        PorcentajeCompletado = 100,
        TiempoTotalMinutos = 45
    });

    AssertEqual("completada", sesion.Estado, "La sesion debio quedar completada.");
    AssertTrue(sesion.FechaFin.HasValue, "La sesion completada debe registrar fecha de fin.");
}

void AssertContains(string content, string expected)
{
    if (!content.Contains(expected, StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException($"No se encontro '{expected}' en el archivo SQL.");
    }
}

void AssertEqual<T>(T expected, T actual, string message)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
    {
        throw new InvalidOperationException($"{message} Esperado: {expected}. Actual: {actual}.");
    }
}

void AssertTrue(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException(message);
    }
}
