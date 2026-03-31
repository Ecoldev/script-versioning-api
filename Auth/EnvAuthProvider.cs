namespace script_versioning_api.Auth;

public class EnvAuthProvider : IGitLabAuthProvider
{
    
    private readonly string _token;

    public EnvAuthProvider()
    {
        _token = Environment.GetEnvironmentVariable("GITLAB_TOKEN")
                 ?? throw new Exception("Missing GITLAB_TOKEN");
    }

    public Task AddAuth(HttpRequestMessage request)
    {
        request.Headers.Remove("PRIVATE-TOKEN"); 
        request.Headers.Add("PRIVATE-TOKEN", _token);

        return Task.CompletedTask;
    }
}