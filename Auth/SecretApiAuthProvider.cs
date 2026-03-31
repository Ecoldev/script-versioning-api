using System.Text.Json;

namespace script_versioning_api.Auth;

public class SecretApiAuthProvider : IGitLabAuthProvider
{
    private readonly HttpClient _http;
    private readonly string _urlTemplate;
    private readonly string _credential;

    private string? _token;
    private DateTime _expires;

    public SecretApiAuthProvider(HttpClient http, IConfiguration config)
    {
        _http = http;
        _urlTemplate = config["GitLab:SecretUrlTemplate"]
            ?? throw new Exception("Missing SecretUrlTemplate");

        _credential = config["GitLab:Credential"]
            ?? throw new Exception("Missing GitLab:Credential");
    }

    public async Task AddAuth(HttpRequestMessage request)
    {
        if (string.IsNullOrEmpty(_token) || _expires < DateTime.UtcNow)
        {
            var url = _urlTemplate.Replace("{credential}", _credential);

            var res = await _http.GetAsync(url);

            if (!res.IsSuccessStatusCode)
            {
                var err = await res.Content.ReadAsStringAsync();
                throw new Exception($"Secret API error: {err}");
            }

            var json = await res.Content.ReadAsStringAsync();
            var secret = JsonSerializer.Deserialize<SecretDto>(json);

            if (string.IsNullOrWhiteSpace(secret?.Password))
                throw new Exception("Token is empty");

            _token = secret.Password;
            _expires = DateTime.UtcNow.AddMinutes(15);
        }

        request.Headers.Remove("PRIVATE-TOKEN");
        request.Headers.Add("PRIVATE-TOKEN", _token);
    }
}

public class SecretDto
{
    public string Username { get; set; }
    public string Password { get; set; }
}