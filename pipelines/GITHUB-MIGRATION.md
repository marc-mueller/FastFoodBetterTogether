# Azure Pipelines Migration Guide: Azure Repos to GitHub

## Overview

This document describes the changes made to migrate Azure Pipelines from using Azure Repos to using GitHub as the source repository, while maintaining Azure DevOps for CI/CD pipelines and Azure Boards for work item tracking.

## What Changed

### 1. Repository Type Configuration

**File: `pipelines/setup-pipelines.yml`**

- Added `repositoryType` parameter (default: `github`)
- Changed from hardcoded `--repository-type tfsgit` to dynamic `--repository-type $(repositoryType)`
- This allows the pipeline creation script to work with GitHub repositories

### 2. CI Pipeline Triggers

**Files: All `ci-*.yml` files**

Added explicit `pr:` trigger sections to all CI pipelines:

```yaml
pr:
  branches:
    include:
      - main
      - release/*
  paths:
    include:
      - src/services/{service}/**
```

**Why:** For GitHub repositories, Azure Pipelines require explicit `pr:` triggers. The `trigger:` section handles pushes to branches, while `pr:` handles pull requests.

**Affected files:**
- `pipelines/ci-orderservice.yml`
- `pipelines/ci-kitchenservice.yml`
- `pipelines/ci-financeservice.yml`
- `pipelines/ci-frontendcustomerorderstatus.yml`
- `pipelines/ci-frontendkitchenmonitor.yml`
- `pipelines/ci-frontendselfservicepos.yml`
- `pipelines/ci-targetinfrastructure.yml`
- `infrastructure/BuildEnvironment/buildanddeploybuildenvimage.yml`

### 3. PR Pipeline Triggers

**Files: `pr-initialize.yml`, `pr-securityscan.yml`**

Added explicit `pr:` trigger sections:

```yaml
pr:
  branches:
    include:
      - main
      - release/*
```

### 4. Webhook-Based Pipelines

**Files: `pr-cleanup.yml`, `pr-workitemcheck.yml`**

- Commented out Azure Repos webhook resources (not supported for GitHub)
- Added notes about alternative approaches:
  - Manual triggering
  - GitHub Actions integration
  - Scheduled runs

**Why:** Azure DevOps webhooks for Azure Repos don't work with GitHub repositories. These pipelines need alternative triggering mechanisms.

### 5. GitHub Branch Protection

**New file: `pipelines/setup-github-branchprotection.yml`**

Created a new pipeline to configure GitHub branch protection rules via GitHub API:

- Required status checks
- Required reviewers
- Branch deletion/force push protection
- Linear history requirements

**Why:** Azure DevOps branch policies only apply to Azure Repos. For GitHub, we need to use GitHub's branch protection API.

**Updated file: `pipelines/setup-branchpolicies.yml`**

Added documentation clarifying that for GitHub repos:
- Azure DevOps build validation policies still work (as status checks)
- GitHub branch protection must be configured separately

### 6. PR/Commit Status Reporting

**New files:**
- `pipelines/pullrequest/step-setgithubstatus.yml` - GitHub commit status API
- `pipelines/pullrequest/step-setuniversalstatus.yml` - Universal template that auto-detects repo type

**Updated file: `pipelines/pullrequest/step-setprstatus.yml`**

Added documentation about repository type detection.

**Updated files using status templates:**
- `pipelines/pr-securityscan.yml`
- `pipelines/pr-workitemcheck.yml`
- `pipelines/deploy/job-verifyprdeployment.yml`

Now use `step-setuniversalstatus.yml` which automatically detects whether it's GitHub or Azure Repos and calls the appropriate status API.

## Setup Instructions

### Initial Setup

1. **Run Pipeline Setup**
   ```bash
   # In Azure DevOps, run the setup-pipelines.yml pipeline
   # Set repositoryType parameter to 'github'
   # Set repoName to 'marc-mueller/FastFoodBetterTogether' (or your GitHub repo)
   ```

2. **Configure GitHub Token**
   
   Create a GitHub Personal Access Token with the following scopes:
   - `repo` (full repository access)
   - `repo:status` (commit status access)
   
   Add it as a pipeline variable/secret named `GitHubToken` in Azure DevOps.

3. **Configure GitHub Branch Protection**
   ```bash
   # Run the setup-github-branchprotection.yml pipeline
   # This will configure branch protection rules via GitHub API
   ```

4. **Verify Build Validation Policies** (Optional)
   ```bash
   # Run setup-branchpolicies.yml to configure Azure DevOps build validation policies
   # These will appear as status checks in GitHub PRs
   ```

### Pipeline-by-Pipeline Setup

When setting up individual pipelines in Azure DevOps:

1. Use the Azure DevOps UI or run `setup-pipelines.yml`
2. Select **GitHub** as the repository type
3. Authorize Azure Pipelines to access your GitHub repository
4. Select the appropriate YAML file
5. Set up any required service connections and variables

### GitHub Webhook Configuration (Optional)

For webhook-based pipelines (`pr-cleanup.yml`, `pr-workitemcheck.yml`), you have options:

**Option 1: Manual Triggering**
- Run these pipelines manually when needed

**Option 2: GitHub Actions Integration**
- Create GitHub Actions workflows that trigger these pipelines via Azure DevOps REST API
- Example: On PR close, call Azure DevOps API to run `pr-cleanup.yml`

**Option 3: Scheduled Runs**
- Configure `pr-workitemcheck.yml` to run on a schedule
- Check all active PRs in the scheduled run

## Key Differences: Azure Repos vs GitHub

| Feature | Azure Repos | GitHub |
|---------|-------------|--------|
| PR Triggers | Automatic | Require explicit `pr:` section |
| Branch Policies | Azure DevOps UI/CLI | GitHub branch protection API |
| Status Updates | Azure DevOps PR Status API | GitHub Commit Status API |
| Webhooks | Azure DevOps Service Hooks | GitHub Webhooks |
| Build Validation | Azure DevOps Policies | Required status checks in GitHub |

## Testing the Migration

1. **Create a test PR in GitHub**
2. **Verify that:**
   - CI pipelines trigger automatically for changed services
   - `pr-initialize.yml` runs and deploys PR environment
   - `pr-securityscan.yml` runs
   - Status checks appear in the GitHub PR
   - Required status checks prevent merging if failing

3. **Check branch protection:**
   - Try to push directly to main (should be blocked)
   - Try to merge PR without required checks (should be blocked)

## Troubleshooting

### Pipeline doesn't trigger on PR

**Solution:** Check that:
1. The pipeline has explicit `pr:` trigger section
2. The path filters include the changed files
3. Azure Pipelines GitHub App is installed and authorized

### Status checks don't appear in GitHub

**Solution:** Check that:
1. `GitHubToken` variable is configured
2. Token has `repo:status` scope
3. Pipeline uses `step-setuniversalstatus.yml` or `step-setgithubstatus.yml`
4. Repository owner/name are correct in the template parameters

### Branch protection not working

**Solution:**
1. Run `setup-github-branchprotection.yml` pipeline
2. Or configure manually at: `https://github.com/{owner}/{repo}/settings/branches`
3. Ensure status check names match the `contextName` in pipelines

### Webhook-based pipelines not working

**Expected:** Webhook resources are not supported for GitHub repos.

**Solution:** Use one of the alternatives:
- Manual triggering
- GitHub Actions to trigger via REST API
- Scheduled pipeline runs

## References

- [Azure Pipelines with GitHub](https://learn.microsoft.com/en-us/azure/devops/pipelines/repos/github?view=azure-devops&tabs=yaml)
- [GitHub Branch Protection API](https://docs.github.com/en/rest/branches/branch-protection)
- [GitHub Commit Status API](https://docs.github.com/en/rest/commits/statuses)
- [Azure Pipelines Triggers](https://learn.microsoft.com/en-us/azure/devops/pipelines/build/triggers?view=azure-devops)
