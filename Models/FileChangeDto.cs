namespace script_versioning_api.Models;

public class FileChangeDto
{
    public string FileName { get; set; } = string.Empty;
    public string Server { get; set; } = string.Empty;
    public string Hash { get; set; } = string.Empty;
    public string ContentBase64 { get; set; } = string.Empty;
}

// DTO (Data Transfer Object) reprezentujący zmianę pliku przesyłaną do API.
//
// Użycie:
// - Obiekt przyjmowany w endpointach (np. POST /scripts)
// - Przenosi dane między klientem (np. ServiceNow, Ansible, skrypt) a backendem
//
// Pola:
// - FileName: nazwa pliku (np. script.ps1)
// - Server: identyfikator serwera/hosta, z którego pochodzi plik
// - Hash: suma kontrolna (np. SHA256) używana do wykrywania zmian
// - ContentBase64: zawartość pliku zakodowana w Base64 (transport przez API)
//
// Flow:
// 1. Klient wysyła JSON z danymi pliku
// 2. API deserializuje JSON do FileChangeDto
// 3. Backend może:
//    - porównać hash (czy plik się zmienił)
//    - zapisać plik na dysku
//    - wykonać commit do repozytorium Git
//
// Przykładowy request:
// {
//   "fileName": "test.ps1",
//   "server": "srv01",
//   "hash": "abc123",
//   "contentBase64": "SGVsbG8gd29ybGQ="
// }

