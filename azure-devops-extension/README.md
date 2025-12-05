# FastFood GitHub Token Extension

This Azure DevOps extension provides a custom task to retrieve GitHub tokens from Azure DevOps GitHub service connections.

## Purpose

Instead of requiring users to create and manage Personal Access Tokens (PATs) manually, this extension allows pipelines to:
1. Use the existing GitHub service connection configured in Azure DevOps
2. Retrieve a short-lived token programmatically
3. Store it as a secret pipeline variable for use in subsequent steps

## Task: Get GitHub Token

### Inputs

- **GitHub Service Connection** (required): The GitHub service connection to use for authentication
- **Output Variable Name** (default: `GitHubToken`): The name of the pipeline variable to store the token in (will be set as secret)

### Usage Example

```yaml
# Get the token from the GitHub service connection
- task: GetGitHubToken@1
  displayName: 'Get GitHub Token'
  inputs:
    gitHubConnection: '$(githubServiceConnection)'
    variableName: 'GitHubToken'

# Use the token in a subsequent bash script
- bash: |
    curl -H "Authorization: token $(GitHubToken)" \
      https://api.github.com/repos/owner/repo/statuses/$(Build.SourceVersion)
  displayName: 'Call GitHub API'
```

## Building the Extension

```bash
cd azure-devops-extension/GetGitHubTokenTask
npm install
npm run build
cd ..
npm run package
```

This will create a `.vsix` file that can be uploaded to Azure DevOps.

## Installation

1. Build the extension (see above)
2. Upload the `.vsix` file to your Azure DevOps organization
3. Install the extension in your organization
4. Use the task in your pipelines

## Benefits

- **No PAT management**: No need to create, rotate, or manage Personal Access Tokens
- **Security**: Tokens are short-lived and managed by the service connection
- **Simplicity**: Single parameter configuration using existing service connections
- **Consistency**: Use the same authentication mechanism across all pipelines

## Comparison with PAT Approach

### Before (PAT):
```yaml
# User must:
# 1. Create a GitHub PAT manually
# 2. Store it in Azure DevOps as a secret variable
# 3. Remember to rotate it periodically
# 4. Manage scope and permissions

- bash: |
    # Use manually created PAT
    curl -H "Authorization: token $(ManuallyCreatedPAT)" ...
  env:
    GitHubToken: $(ManuallyCreatedPAT)
```

### After (Extension):
```yaml
# Just reference the service connection
- task: GetGitHubToken@1
  inputs:
    gitHubConnection: '$(githubServiceConnection)'

- bash: |
    # Token automatically available
    curl -H "Authorization: token $(GitHubToken)" ...
```

## Architecture

The task retrieves the token from the Azure DevOps service connection endpoint:
- For PAT-based connections: Reads from `parameters.accessToken`
- For OAuth connections (GitHub App): Reads from `parameters.AccessToken`
- For token-based connections: Reads from `parameters.apitoken`

The token is then set as a secret pipeline variable using `tl.setVariable(name, value, true)`.

## Integration with Status Reporting

This extension is designed to work seamlessly with the GitHub status reporting templates:

```yaml
# Step 1: Get token
- task: GetGitHubToken@1
  inputs:
    gitHubConnection: '$(githubServiceConnection)'

# Step 2: Use universal status template (will use the token automatically)
- template: pullrequest/step-setuniversalstatus.yml
  parameters:
    contextName: 'my-check'
    state: 'succeeded'
    description: 'Check passed'
```

## Requirements

- Azure DevOps organization with GitHub service connection configured
- Node.js 20 or later (for building)
- tfx-cli (for packaging)

## License

Part of the FastFood Delivery project.
