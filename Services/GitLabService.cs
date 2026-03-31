using System.Text;
using System.Text.Json;
using script_versioning_api.Auth;

namespace script_versioning_api.Services;

public class GitLabService
{
    private readonly HttpClient _http;
    private readonly IGitLabAuthProvider _auth;

    public GitLabService(HttpClient http, IGitLabAuthProvider auth)
    {
        _http = http;
        _auth = auth;
    }

    public async Task EnsureRepoExists(string repoName)
    {
        var encodedPath = Uri.EscapeDataString(repoName);

        var checkRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"https://gitlab.com/api/v4/projects/{encodedPath}"
        );

        await _auth.AddAuth(checkRequest);

        var checkResponse = await _http.SendAsync(checkRequest);

        if (checkResponse.IsSuccessStatusCode)
        {
            return;
        }

        if (checkResponse.StatusCode != System.Net.HttpStatusCode.NotFound)
        {
            var err = await checkResponse.Content.ReadAsStringAsync();
            throw new Exception($"GitLab check error: {checkResponse.StatusCode} - {err}");
        }

        // create repo
        var body = new
        {
            name = repoName,
            visibility = "private"
        };

        var createRequest = new HttpRequestMessage(
            HttpMethod.Post,
            "https://gitlab.com/api/v4/projects"
        );

        await _auth.AddAuth(createRequest);

        createRequest.Content = new StringContent(
            JsonSerializer.Serialize(body),
            Encoding.UTF8,
            "application/json"
        );

        var createResponse = await _http.SendAsync(createRequest);

        if (!createResponse.IsSuccessStatusCode)
        {
            var err = await createResponse.Content.ReadAsStringAsync();
            throw new Exception($"GitLab create error: {createResponse.StatusCode} - {err}");
        }
    }
}

// Serwis odpowiedzialny za integrację z GitLab API.
//
// Rola:
// - komunikacja z zewnętrznym systemem GitLab (REST API)
// - zarządzanie repozytoriami (tworzenie, w przyszłości commitowanie plików)
// - enkapsulacja logiki HTTP (controller nie powinien znać szczegółów API)
//
// Obecna funkcjonalność:
// - CreateRepoIfNotExists:
//   tworzy nowe repozytorium w GitLab na podstawie nazwy przekazanej z controllera
//
// Flow:
// 1. Controller wywołuje serwis z nazwą repozytorium
// 2. Serwis buduje request HTTP do GitLab API
// 3. Wysyła request z tokenem autoryzacyjnym
// 4. GitLab tworzy repozytorium (jeśli nie istnieje)
//
// Endpoint GitLab:
// POST https://gitlab.com/api/v4/projects
//
// Wymagania:
// - token GitLab (PRIVATE-TOKEN) z odpowiednimi uprawnieniami (api)
//
// Uwagi architektoniczne:
// - HttpClient powinien być wstrzykiwany przez DI (Dependency Injection)
// - token NIE powinien być przekazywany jako string w kodzie (use secrets / env)
// - brak obsługi błędów (należy dodać sprawdzanie response.StatusCode)
// - brak sprawdzenia czy repo już istnieje (obecnie zawsze próbuje utworzyć)
//
// Przyszłe rozszerzenia:
// - sprawdzanie istnienia repo (GET /projects)
// - commit pliku (POST /repository/commits)
// - wersjonowanie zmian (commit message, branch strategy)