using Microsoft.AspNetCore.Mvc;
using script_versioning_api.Models;
using script_versioning_api.Services;

namespace script_versioning_api.Controllers;



[ApiController]
[Route("api/[controller]")]
public class FileController : ControllerBase
{
    private readonly GitLabService _gitlab;
    private readonly ILogger<FileController> _logger;

    public FileController(GitLabService gitlab, ILogger<FileController> logger)
    {
        _gitlab = gitlab;
        _logger = logger;
    }

    [HttpPost("change")]
    public async Task<IActionResult> Change(FileChangeDto dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.FileName))
        {
            return BadRequest("Invalid request");
        }

        if (string.IsNullOrWhiteSpace(dto.Server) ||
            string.IsNullOrWhiteSpace(dto.Hash) ||
            string.IsNullOrWhiteSpace(dto.ContentBase64))
        {
            return BadRequest("Missing required fields");
        }

        var server = dto.Server.Trim().ToLower().Replace(" ", "-");
        var file = Path.GetFileNameWithoutExtension(dto.FileName)
                  .Trim()
                  .ToLower()
                  .Replace(" ", "-");

        var repoName = $"script-{server}-{file}";

        _logger.LogInformation(
            "Processing change for file {File} from server {Server}, target repo: {Repo}",
            dto.FileName, dto.Server, repoName
        );

        try
        {
            await _gitlab.EnsureRepoExists(repoName); // 🔥 zmienione
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GitLab error while processing repo {Repo}", repoName);
            return StatusCode(500, $"GitLab error: {ex.Message}");
        }

        return StatusCode(201, new { message = "Processed", repo = repoName });
    }
}



// Endpoint v.01 Przyjmujący zmianę pliku i inicjujący proces versioningu.
//
// Co robi:
// - odbiera dane pliku (DTO)
// - waliduje dane wejściowe
// - normalizuje (sanityzuje) nazwę serwera i pliku
// - buduje nazwę repozytorium (script-{server}-{file})
// - loguje operację (observability)
// - wywołuje GitLabService w celu utworzenia repozytorium
//
// Czego NIE robi jeszcze:
// - nie zapisuje pliku
// - nie commit'uje zmian do repo
// - nie sprawdza czy plik się zmienił (hash)
// - nie prowadzi historii wersji
//
// To jest entry point pod system versioningu — dalsza logika powinna być rozwijana w warstwie serwisów.

//COMMIT 1 — decode Base64
//COMMIT 2 — commit pliku do GitLab
//COMMIT 3 — commit message (versioning)
//COMMIT 4 — hash check (kluczowe)
//COMMIT 5 — struktura repo
//COMMIT 6 — obsługa "repo already exists"
//COMMIT 7 — config zamiast ENV w controllerze
//COMMIT 8 — logging rozszerzony
//COMMIT 9 — response upgrade
//COMMIT 10 — error handling GitLab API
//
//
//