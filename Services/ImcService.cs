namespace Softawer.Services;

public class ImcService
{
    public decimal CalcularImc(decimal alturaCm, decimal pesoKg)
    {
        var alturaMetros = alturaCm / 100m;
        if (alturaMetros <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(alturaCm), "La altura debe ser mayor a cero.");
        }

        var imc = pesoKg / (alturaMetros * alturaMetros);
        return Math.Round(imc, 2, MidpointRounding.AwayFromZero);
    }

    public string ClasificarImc(decimal imc)
    {
        if (imc < 18.5m)
        {
            return "bajo_peso";
        }

        if (imc < 25m)
        {
            return "normal";
        }

        if (imc < 30m)
        {
            return "sobrepeso";
        }

        return "obesidad";
    }
}
