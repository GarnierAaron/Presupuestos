using System.Globalization;
using DynamicExpresso;
using Presupuestos.Application.Common.Exceptions;

namespace Presupuestos.Application.Pricing;

/// <summary>
/// Evalúa expresiones solo con variables <c>material</c>, <c>price</c>, <c>qty</c> (DynamicExpresso, sin reflexión arbitraria).
/// </summary>
public static class SafeFormulaEvaluator
{
    private const int MaxLength = 500;

    private static readonly string[] BannedFragments =
    {
        "new ", "typeof", "import", "system.", "console.", "reflection", "process.", "file.", "http",
        "Assembly", "Delegate", "Mutex", "Thread", "Environment"
    };

    public static decimal Evaluate(string expression, decimal material, decimal price, decimal quantity)
    {
        if (string.IsNullOrWhiteSpace(expression))
            throw new AuthException("La fórmula no puede estar vacía.");

        expression = expression.Trim();
        if (expression.Length > MaxLength)
            throw new AuthException($"La fórmula supera {MaxLength} caracteres.");

        var lower = expression.ToLowerInvariant();
        foreach (var bad in BannedFragments)
        {
            if (lower.Contains(bad.ToLowerInvariant()))
                throw new AuthException("La fórmula contiene elementos no permitidos.");
        }

        try
        {
            var interpreter = new Interpreter();
            interpreter.SetVariable("material", material);
            interpreter.SetVariable("price", price);
            interpreter.SetVariable("qty", quantity);

            var result = interpreter.Eval(expression);
            return Convert.ToDecimal(result, CultureInfo.InvariantCulture);
        }
        catch (AuthException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new AuthException($"No se pudo evaluar la fórmula: {ex.Message}");
        }
    }
}
