using System.Globalization;
using System.Text.Json;

namespace SportStats.Integracao;

// Leitura defensiva: se um campo nao existir, devolve null em vez de lancar excecao.
internal static class JsonUtil
{
    public static JsonElement? Obter(JsonElement e, params string[] caminho)
    {
        var atual = e;
        foreach (var nome in caminho)
        {
            if (atual.ValueKind != JsonValueKind.Object || !atual.TryGetProperty(nome, out var proximo)) return null;
            atual = proximo;
        }
        return atual.ValueKind == JsonValueKind.Null ? null : atual;
    }

    public static string? Texto(JsonElement e, params string[] caminho)
    {
        var v = Obter(e, caminho);
        if (v is null) return null;
        return v.Value.ValueKind switch
        {
            JsonValueKind.String => v.Value.GetString(),
            JsonValueKind.Number => v.Value.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            _ => null
        };
    }

    public static int? Inteiro(JsonElement e, params string[] caminho)
    {
        var v = Obter(e, caminho);
        if (v is null) return null;
        var x = v.Value;
        if (x.ValueKind == JsonValueKind.Number && x.TryGetInt32(out var i)) return i;
        if (x.ValueKind == JsonValueKind.String &&
            int.TryParse(x.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var j)) return j;
        return null;
    }

    public static bool Booleano(JsonElement e, params string[] caminho)
    {
        var v = Obter(e, caminho);
        return v is not null && v.Value.ValueKind == JsonValueKind.True;
    }

    public static DateTime? Data(JsonElement e, params string[] caminho)
    {
        var texto = Texto(e, caminho);
        if (texto is null) return null;
        return DateTimeOffset.TryParse(texto, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var d)
            ? d.UtcDateTime
            : null;
    }
}