# Quick Start Guide: GitHub Migration

## Prerequisites Checklist

- [ ] Repository moved to GitHub
- [ ] Azure DevOps project configured
- [ ] Azure Pipelines GitHub App installed on repository
- [ ] GitHub Service Connection created in Azure DevOps

## 1. Configure GitHub Service Connection

The GitHub service connection should already be configured in Azure DevOps. The connection ID is stored in `pipelines/config/var-commonvariables.yml`:

```yaml
githubServiceConnection: '2ab8bde8-f905-48c7-8c36-e42bf1641ce4'
```

If you need to create a new one:
1. Go to Project Settings → Service connections in Azure DevOps
2. Click "New service connection" → GitHub
3. Authenticate with GitHub
4. Name the connection and copy its ID
5. Update the ID in `var-commonvariables.yml`

## 2. Install GetGitHubToken Extension

This custom extension retrieves GitHub tokens from the service connection, eliminating the need for manual PAT management.

### Build the Extension

```bash
cd azure-devops-extension/GetGitHubTokenTask
npm install
npm run build
cd ..
tfx extension create --manifest-globs vss-extension.json
```

This creates a `.vsix` file.

### Install in Azure DevOps

1. Go to Organization Settings → Extensions → Upload extension
2. Upload the `.vsix` file
3. Install the extension in your organization

**Note**: The extension is required for GitHub status reporting and branch protection configuration.

## 3. Run Pipeline Setup

1. Navigate to Pipelines in Azure DevOps
2. Run the `setup-pipelines` pipeline (or create it first from `pipelines/setup-pipelines.yml`)
3. The pipeline will use the GitHub service connection from common variables
4. All pipelines will be created automatically

The setup now uses:
- `repositoryType`: `github` (default)
- `githubServiceConnection`: From common variables
- `--service-connection` flag for pipeline creation

## 4. Configure GitHub Branch Protection

### Using Azure Pipeline (Recommended)

1. Run the `setup-github-branchprotection` pipeline
2. The pipeline uses the GetGitHubToken extension to authenticate
3. Verify the settings were applied:
   - Visit: `https://github.com/marc-mueller/FastFoodBetterTogether/settings/branches`

### Manual Configuration (Alternative)

1. Go to: `https://github.com/marc-mueller/FastFoodBetterTogether/settings/branches`
2. Click "Add rule" for `main` branch
3. Configure:
   - ✅ Require a pull request before merging
   - ✅ Require approvals: 1
   - ✅ Dismiss stale pull request approvals when new commits are pushed
   - ✅ Require status checks to pass before merging
   - ✅ Require branches to be up to date before merging
   - Add required status checks:
     - `ci-orderservice`
     - `ci-kitchenservice`
     - `ci-financeservice`
     - `ci-frontendcustomerorderstatus`
     - `ci-frontendkitchenmonitor`
     - `ci-frontendselfservicepos`
     - `fastfood-security-scan`
     - `pr-deployment-orderservice`
     - `pr-deployment-kitchenservice`
     - `pr-deployment-financeservice`
     - `pr-deployment-frontendcustomerorderstatus`
     - `pr-deployment-frontendkitchenmonitor`
     - `pr-deployment-frontendselfservicepos`
     - `fastfood-wi-content-check`

## 5. Test the Migration

1. Create a test branch:
   ```bash
   git checkout -b test/pipeline-validation
   ```

2. Make a small change (e.g., update a README)

3. Push the branch and create a Pull Request in GitHub

4. Verify:
   - ✅ CI pipelines trigger automatically
   - ✅ `pr-initialize` pipeline runs
   - ✅ `pr-workitemcheck` pipeline runs
   - ✅ Status checks appear in GitHub PR
   - ✅ Checks are required before merge
   - ✅ PR deployment environments are created

5. Close the PR and verify:
   - ✅ `pr-cleanup` pipeline runs automatically

6. After verification, you can delete the test branch

## 6. Configure Individual Pipelines (if needed)

If pipelines don't exist yet in Azure DevOps:

1. Go to Pipelines → New Pipeline
2. Select "GitHub"
3. Authorize and select `marc-mueller/FastFoodBetterTogether`
4. Select "Existing Azure Pipelines YAML file"
5. Choose the pipeline file (e.g., `pipelines/ci-orderservice.yml`)
6. Run or save the pipeline

Repeat for each pipeline, or use the `setup-pipelines.yml` automation.

## Key Differences from PAT Approach

### ❌ OLD Approach (Manual PAT)
```
1. Create GitHub PAT manually with specific scopes
2. Store PAT in Azure DevOps as secret variable
3. Reference $(GitHubToken) in scripts
4. Remember to rotate PAT periodically
5. Manage PAT security and permissions
```

### ✅ NEW Approach (Service Connection + Extension)
```
1. GitHub service connection configured once
2. GetGitHubToken extension installed
3. Extension retrieves token automatically
4. Token is short-lived from service connection
5. No manual token management
```

### Usage Comparison

**OLD:**
```yaml
- bash: |
    curl -H "Authorization: token $(ManuallyCreatedPAT)" ...
  env:
    GITHUB_TOKEN: $(ManuallyCreatedPAT)  # Manual secret
```

**NEW:**
```yaml
- task: GetGitHubToken@1
  inputs:
    gitHubConnection: '$(githubServiceConnection)'

- bash: |
    curl -H "Authorization: token $(GitHubToken)" ...
  env:
    GITHUB_TOKEN: $(GitHubToken)  # Auto-retrieved
```

## Validation

Run the validation script to ensure all changes are in place:

```bash
./pipelines/validate-github-migration.sh
```

Expected output: "All critical checks passed!"

## Troubleshooting

### Issue: Pipelines don't trigger on PR

**Check:**
1. Pipeline has `pr:` trigger section in YAML
2. Azure Pipelines GitHub App has access to repository
3. Path filters include the changed files

### Issue: Status checks don't appear in GitHub

**Check:**
1. GetGitHubToken extension is installed
2. GitHub service connection is configured
3. Pipelines use `step-setuniversalstatus.yml`
4. Repository owner/name are correct (`marc-mueller/FastFoodBetterTogether`)

### Issue: GetGitHubToken task not found

**Solution:**
1. Build the extension (see Step 2 above)
2. Upload and install the `.vsix` file in Azure DevOps organization
3. Ensure the extension is enabled for your project

### Issue: Branch protection not enforced

**Check:**
1. Branch protection rules are configured in GitHub
2. Rule applies to `main` branch
3. Required status checks match pipeline context names
4. Rule is not set to "include administrators" (which can bypass rules)

## Next Steps

1. ✅ Monitor first few PRs to ensure everything works
2. ✅ Update team documentation about the new workflow
3. ✅ Verify PR cleanup works when closing PRs
4. ✅ Review and adjust branch protection rules as needed

## Support

For detailed information, see:
- `pipelines/GITHUB-MIGRATION.md` - Complete migration guide
- `azure-devops-extension/README.md` - Extension documentation
- [Azure Pipelines GitHub Integration](https://learn.microsoft.com/en-us/azure/devops/pipelines/repos/github)
- [GitHub Branch Protection](https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/managing-protected-branches/about-protected-branches)
