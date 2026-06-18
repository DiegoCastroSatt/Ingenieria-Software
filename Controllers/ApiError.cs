using Microsoft.AspNetCore.Mvc;

namespace Softawer.Controllers;

internal static class ApiError
{
    public static ObjectResult BadRequest(ControllerBase controller, string detail) =>
        Error(controller, StatusCodes.Status400BadRequest, "Solicitud invalida.", detail);

    public static ObjectResult Unauthorized(ControllerBase controller, string detail) =>
        Error(controller, StatusCodes.Status401Unauthorized, "Credenciales invalidas.", detail);

    public static ObjectResult Forbidden(ControllerBase controller, string detail) =>
        Error(controller, StatusCodes.Status403Forbidden, "Accion no permitida.", detail);

    public static ObjectResult NotFound(ControllerBase controller, string detail) =>
        Error(controller, StatusCodes.Status404NotFound, "Recurso no encontrado.", detail);

    public static ObjectResult Conflict(ControllerBase controller, string detail) =>
        Error(controller, StatusCodes.Status409Conflict, "Conflicto de negocio.", detail);

    public static ObjectResult ServiceUnavailable(ControllerBase controller, string detail) =>
        Error(controller, StatusCodes.Status503ServiceUnavailable, "Servicio no disponible.", detail);

    private static ObjectResult Error(ControllerBase controller, int statusCode, string title, string detail) =>
        controller.Problem(
            statusCode: statusCode,
            title: title,
            detail: detail);
}
