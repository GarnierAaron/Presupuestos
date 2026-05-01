namespace Presupuestos.Application.Support;

/// <summary>Comparación de versiones tipo Major.Minor.Patch.build (segmentos numéricos).</summary>
public static class AppVersionComparer
{
    /// <summary>&lt; 0 si a es menor que b; 0 si iguales; &gt; 0 si a mayor.</summary>
    public static int Compare(string a, string b)
    {
        var pa = Parse(a);
        var pb = Parse(b);
        var n = Math.Max(pa.Length, pb.Length);
        for (var i = 0; i < n; i++)
        {
            var va = i < pa.Length ? pa[i] : 0;
            var vb = i < pb.Length ? pb[i] : 0;
            if (va != vb)
                return va.CompareTo(vb);
        }
        return 0;
    }

    private static int[] Parse(string version)
    {
        if (string.IsNullOrWhiteSpace(version))
            return Array.Empty<int>();

        var core = version.Trim();
        var dash = core.IndexOf('-', StringComparison.Ordinal);
        if (dash >= 0)
            core = core[..dash];

        var parts = core.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var nums = new List<int>(parts.Length);
        foreach (var p in parts)
        {
            if (int.TryParse(p, out var n))
                nums.Add(n);
            else
                nums.Add(0);
        }
        return nums.ToArray();
    }
}
