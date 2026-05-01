using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presupuestos.Api.Controllers;

/// <summary>Ejemplo documentado de cálculo (sin multi-tenant). Útil para pruebas rápidas en Swagger.</summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class CalculationExampleController : ControllerBase
{
    [HttpGet]
    public IActionResult GetWalkthrough()
    {
        const decimal tintaLitros = 0.02m;
        const decimal costoTintaPorLitro = 50m;
        const decimal papelUnidades = 5m;
        const decimal costoPapelPorUnidad = 0.03m;

        var costoInsumosPorUnidadServicio = tintaLitros * costoTintaPorLitro + papelUnidades * costoPapelPorUnidad;
        const decimal cantidadServicio = 100m;
        var costoLinea = costoInsumosPorUnidadServicio * cantidadServicio;

        const decimal margenServicioPct = 30m;
        var precioUnitarioMargen = costoInsumosPorUnidadServicio * (1m + margenServicioPct / 100m);
        const decimal basePricePorUnidad = 2m;
        var precioUnitarioFinal = Math.Max(precioUnitarioMargen, basePricePorUnidad);
        var precioLinea = precioUnitarioFinal * cantidadServicio;

        const decimal manualOverrideTotal = 250m;
        var precioConOverride = manualOverrideTotal;

        return Ok(new
        {
            descripcion = "Impresión: insumos por unidad de servicio (modo clásico: margen + BasePrice + override)",
            insumos = new
            {
                tinta = new { litros = tintaLitros, costoPorLitro = costoTintaPorLitro, subtotal = tintaLitros * costoTintaPorLitro },
                papel = new { unidades = papelUnidades, costoPorUnidad = costoPapelPorUnidad, subtotal = papelUnidades * costoPapelPorUnidad }
            },
            costoUnitarioServicio = costoInsumosPorUnidadServicio,
            cantidad = cantidadServicio,
            costoLinea,
            margenServicioPorciento = margenServicioPct,
            precioUnitarioTrasMargen = precioUnitarioMargen,
            basePricePorUnidad,
            precioUnitarioFinalTrasPiso = precioUnitarioFinal,
            precioLineaSinOverride = precioLinea,
            ejemploOverrideManualLinea = new { manualPriceOverride = manualOverrideTotal, precioLineaUsado = precioConOverride }
        });
    }

    /// <summary>
    /// Misma base numérica que GET, pero con pipeline de reglas (como <c>FlexiblePricingEngine</c>).
    /// Activar: PUT /api/PricingSettings + reglas en /api/PricingRules.
    /// </summary>
    [HttpGet("flexible")]
    public IActionResult GetFlexiblePipelineExample()
    {
        const decimal material = 1.15m;
        const decimal qty = 100m;
        const decimal basePriceFloor = 2m;

        decimal running = material;
        var afterMargin = running * 1.3m;
        running = afterMargin;
        running += 10m;
        var afterFixed = running;
        const string formula = "price * 1.1 + material * 0.5";
        running = running * 1.1m + material * 0.5m;
        var unitAfterRules = System.Math.Max(running, basePriceFloor);
        var line = unitAfterRules * qty;

        return Ok(new
        {
            nota = "Orden: SortOrder, luego Name. Fórmulas: solo variables material, price, qty.",
            materialUnitario = material,
            cantidad = qty,
            pasos = new object[]
            {
                new { regla = "Percentage 30%", precioUnitario = afterMargin },
                new { regla = "Fixed +10 (por unidad)", precioUnitario = afterFixed },
                new { regla = $"Formula \"{formula}\"", precioUnitarioTrasFormula = running }
            },
            pisoBasePriceServicio = basePriceFloor,
            precioUnitarioFinal = unitAfterRules,
            precioLinea = line
        });
    }
}
