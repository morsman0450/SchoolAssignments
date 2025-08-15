namespace SchoolAssignments.Services
{
    using SchoolAssignments.DTOs;
    using System.Net.Http.Json;
    using System.Text.Json;

    public class Judge0Service : IJudge0Service
    {
        private readonly HttpClient _http;

        public Judge0Service(HttpClient http)
        {
            _http = http;
            // VARIANTA 1: RapidAPI (stabilní)
            _http.BaseAddress = new Uri("https://judge0-ce.p.rapidapi.com/");
            _http.DefaultRequestHeaders.Add("X-RapidAPI-Key", "<TVŮJ_API_KEY>");
            _http.DefaultRequestHeaders.Add("X-RapidAPI-Host", "judge0-ce.p.rapidapi.com");

            // VARIANTA 2 (místo RapidAPI): veřejná CE instance
            // _http.BaseAddress = new Uri("https://ce.judge0.com/");
        }

        public async Task<Judge0RunResult> RunAsync(int languageId, string sourceCode, string? stdin)
        {
            var payload = new
            {
                language_id = languageId,
                source_code = sourceCode,
                stdin = stdin ?? ""
            };

            using var res = await _http.PostAsJsonAsync("submissions?base64_encoded=false&wait=true", payload);
            res.EnsureSuccessStatusCode();

            using var stream = await res.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(stream);

            string? stdout = doc.RootElement.GetPropertyOrDefault("stdout")?.GetString();
            string? stderr = doc.RootElement.GetPropertyOrDefault("stderr")?.GetString();
            int? exitCode = doc.RootElement.GetPropertyOrDefault("exit_code")?.GetInt32();
            string status = doc.RootElement.GetProperty("status").GetPropertyOrDefault("description")?.GetString() ?? "";

            return new Judge0RunResult
            {
                Stdout = stdout,
                Stderr = stderr,
                ExitCode = exitCode,
                StatusDescription = status
            };
        }
    }

    internal static class JsonExt
    {
        public static JsonElement? GetPropertyOrDefault(this JsonElement el, string name)
            => el.TryGetProperty(name, out var v) ? v : (JsonElement?)null;
    }

}
