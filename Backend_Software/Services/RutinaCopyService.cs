using Softawer.Models;

namespace Softawer.Services;

public class RutinaCopyService
{
    public RutinaDetalleResponse CrearCopiaEditable(RutinaDetalleResponse origen, int idUsuario)
    {
        return new RutinaDetalleResponse
        {
            IdUsuario = idUsuario,
            Nombre = origen.Nombre.StartsWith("Mi ", StringComparison.OrdinalIgnoreCase) ? origen.Nombre : $"Mi {origen.Nombre}",
            Descripcion = origen.Descripcion,
            TipoRutina = "copiada",
            Objetivo = origen.Objetivo,
            CategoriaImc = origen.CategoriaImc,
            Dificultad = origen.Dificultad,
            EsPublica = false,
            IdRutinaOrigen = origen.IdRutina,
            Ejercicios = origen.Ejercicios.Select(ejercicio => new RutinaEjercicio
            {
                IdEjercicio = ejercicio.IdEjercicio,
                Dia = ejercicio.Dia,
                Orden = ejercicio.Orden,
                Series = ejercicio.Series,
                Repeticiones = ejercicio.Repeticiones,
                DuracionMinutos = ejercicio.DuracionMinutos,
                DescansoSegundos = ejercicio.DescansoSegundos,
                Notas = ejercicio.Notas,
                NombreEjercicio = ejercicio.NombreEjercicio,
                DescripcionEjercicio = ejercicio.DescripcionEjercicio,
                GrupoMuscular = ejercicio.GrupoMuscular
            }).ToList()
        };
    }
}
