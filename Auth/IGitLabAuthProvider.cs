namespace script_versioning_api.Auth;

public interface IGitLabAuthProvider
{
   Task AddAuth(HttpRequestMessage request);
}