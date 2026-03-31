# Script Versioning API

##  Overview

Script Versioning API is a lightweight backend service designed to **track and version changes of scripts/files using GitLab repositories**.

The system acts as a **middleware between systems (e.g., ServiceNow, servers, automation tools)** and GitLab, enabling centralized version control without requiring direct Git usage by end users.

---

##  Current Features

###  Repository provisioning

* Automatically creates a GitLab repository per file:

  ```
  script-{server}-{file}
  ```
* Checks if repository exists before creating

### Secure GitLab authentication

Supports multiple authentication strategies:

#### 1. ENV (development)

* Uses environment variable:

  ```
  GITLAB_TOKEN
  ```

#### 2. Secret API (production-ready)

* Fetches credentials dynamically from internal endpoint:

  ```
  http://127.0.0.1:6000/api/secret/{credential}
  ```
* Designed for integration with secrets managers (e.g. Conjur)

###  Clean architecture

* `Controller` → handles requests
* `GitLabService` → GitLab API logic
* `AuthProvider` → authentication abstraction

---

##  Architecture

```
Client / ServiceNow / Script Source
              ↓
        FileController
              ↓
        GitLabService
              ↓
     IGitLabAuthProvider
        ↓           ↓
   EnvAuth      SecretApiAuth
              ↓
        Secrets Gateway
              ↓
           Conjur
              ↓
           GitLab API
```

---

##  API Endpoint

### POST `/api/file/change`

#### Request body:

```json
{
  "fileName": "script.ps1",
  "server": "server01",
  "hash": "abc123",
  "contentBase64": "ZWNobyBoZWxsbw=="
}
```

#### Current behavior:

* Validates input
* Normalizes names
* Generates repository name
* Ensures repository exists in GitLab

---

##  Configuration

### appsettings.json

#### ENV mode:

```json
{
  "GitLab": {
    "AuthType": "Env"
  }
}
```

#### Secret API mode:

```json
{
  "GitLab": {
    "AuthType": "SecretApi",
    "SecretUrlTemplate": "http://127.0.0.1:6000/api/secret/{credential}",
    "Credential": "kontogitlab"
  }
}
```

---

##  Security

* No tokens stored in code
* Token retrieval abstracted via providers
* Ready for integration with secrets managers (e.g. Conjur)
* Supports credential rotation without app restart

---

## Tech Stack

* .NET 8 (ASP.NET Core)
* GitLab REST API
* HttpClient (DI-based)
* JSON serialization

---

##  Current Limitations

> The system currently **only creates repositories**.

Missing features (planned):

* ❌ File commit to repository
* ❌ Change detection (hash comparison)
* ❌ Version history tracking
* ❌ Branching strategy
* ❌ Rollback support

---

##  Roadmap

### Phase 1 (in progress)

* [x] Repository creation
* [x] Auth abstraction
* [x] Secret integration
* [ ] Commit file to GitLab
* [ ] Base64 decoding

### Phase 2

* [ ] Hash-based deduplication
* [ ] Commit versioning
* [ ] Metadata storage

### Phase 3

* [ ] Full GitOps flow
* [ ] Rollback support
* [ ] Integration with ServiceNow workflows

---

## Vision

The goal is to build a **centralized versioning engine for scripts and configurations**, allowing organizations to:

* Track all changes across environments
* Eliminate manual script management
* Integrate versioning into existing ITSM processes
* Improve security and auditability

---

##  Running locally

```bash
dotnet run
```

API available at:

```
http://localhost:5215
```

---

## License

Internal / Private project (customizable)
