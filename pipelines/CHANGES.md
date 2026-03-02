# Migration Changes Summary

This document provides a quick reference of what changed during the Azure Repos to GitHub migration.

## Pipeline Files Changed

### CI Pipelines - Added PR Triggers

All CI pipelines now include explicit `pr:` triggers for GitHub pull requests.

| File | Change | Before | After |
|------|--------|--------|-------|
| `ci-orderservice.yml` | Added PR trigger | Only had `trigger:` | Has both `trigger:` and `pr:` |
| `ci-kitchenservice.yml` | Added PR trigger | Only had `trigger:` | Has both `trigger:` and `pr:` |
| `ci-financeservice.yml` | Added PR trigger | Only had `trigger:` | Has both `trigger:` and `pr:` |
| `ci-frontendcustomerorderstatus.yml` | Added PR trigger | Only had `trigger:` | Has both `trigger:` and `pr:` |
| `ci-frontendkitchenmonitor.yml` | Added PR trigger | Only had `trigger:` | Has both `trigger:` and `pr:` |
| `ci-frontendselfservicepos.yml` | Added PR trigger | Only had `trigger:` | Has both `trigger:` and `pr:` |
| `ci-targetinfrastructure.yml` | Added PR trigger | Only had `trigger:` | Has both `trigger:` and `pr:` |
| `buildanddeploybuildenvimage.yml` | Added PR trigger | Only had `trigger:` | Has both `trigger:` and `pr:` |

### PR Pipelines - Added PR Triggers

| File | Change | Before | After |
|------|--------|--------|-------|
| `pr-initialize.yml` | Added PR trigger | `trigger: none` | Has `pr:` section |
| `pr-securityscan.yml` | Added PR trigger | `trigger: none` | Has `pr:` section |

### Webhook-Based Pipelines - Disabled Azure Repos Webhooks

| File | Change | Impact |
|------|--------|--------|
| `pr-cleanup.yml` | Commented out webhooks | Needs manual or alternative triggering |
| `pr-workitemcheck.yml` | Commented out webhooks | Needs manual or alternative triggering |

### Setup Pipelines - Repository Type Support

| File | Change | Details |
|------|--------|---------|
| `setup-pipelines.yml` | Added GitHub support | New `repositoryType` parameter (default: `github`) |
| `setup-branchpolicies.yml` | Added documentation | Notes about GitHub vs Azure DevOps policies |

### New Files Created

| File | Purpose |
|------|---------|
| `setup-github-branchprotection.yml` | Configure GitHub branch protection via API |
| `step-setgithubstatus.yml` | Post commit status to GitHub API |
| `step-setuniversalstatus.yml` | Auto-detect repo type and set status |
| `GITHUB-MIGRATION.md` | Comprehensive migration guide |
| `QUICK-START.md` | Quick setup instructions |
| `validate-github-migration.sh` | Validation script |
| `CHANGES.md` | This file |

### Updated Status Reporting

| File | Change | Before | After |
|------|--------|--------|-------|
| `step-setprstatus.yml` | Added docs | Azure Repos only | Notes about GitHub |
| `pr-securityscan.yml` | Uses universal template | `step-setprstatus.yml` | `step-setuniversalstatus.yml` |
| `pr-workitemcheck.yml` | Uses universal template | `step-setprstatus.yml` | `step-setuniversalstatus.yml` |
| `job-verifyprdeployment.yml` | Uses universal template | `step-setprstatus.yml` | `step-setuniversalstatus.yml` |

## Behavior Changes

### Triggering

**Azure Repos (Before):**
- Push to branch: Automatic via `trigger:`
- Pull requests: Automatic (no explicit PR trigger needed)
- Webhooks: Service hooks for PR events

**GitHub (After):**
- Push to branch: Automatic via `trigger:`
- Pull requests: Requires explicit `pr:` trigger
- Webhooks: Not supported; use alternatives

### Branch Protection

**Azure Repos (Before):**
- Managed via Azure DevOps branch policies
- Configured via Azure CLI or UI
- Enforced by Azure DevOps

**GitHub (After):**
- Managed via GitHub branch protection rules
- Configured via GitHub API, CLI, or UI
- Enforced by GitHub
- Azure DevOps build validations appear as status checks

### Status Reporting

**Azure Repos (Before):**
- Posted to Azure DevOps PR Status API
- Used PR ID and iteration

**GitHub (After):**
- Posted to GitHub Commit Status API
- Uses commit SHA
- Auto-detection via `Build.Repository.Provider`

## Environment Variables

### New Variables Required

| Variable | Type | Scope | Purpose |
|----------|------|-------|---------|
| `GitHubToken` | Secret | Pipeline | GitHub Personal Access Token for API calls |

### Existing Variables (Unchanged)

All existing Azure DevOps variables continue to work:
- `System.TeamFoundationCollectionUri`
- `System.TeamProject`
- `Build.Repository.Name`
- `System.PullRequest.PullRequestNumber`
- Etc.

### New Variables Available

| Variable | Value | Use Case |
|----------|-------|----------|
| `Build.Repository.Provider` | `GitHub` | Detect repository type |
| `Build.SourceVersion` | Commit SHA | GitHub status API |

## API Endpoints

### Before (Azure Repos)

```
POST {orgUrl}/{project}/_apis/git/repositories/{repoId}/pullRequests/{prId}/statuses
```

### After (GitHub)

```
POST https://api.github.com/repos/{owner}/{repo}/statuses/{sha}
POST https://api.github.com/repos/{owner}/{repo}/branches/{branch}/protection
```

## Configuration Files

### Pipeline Definition (Azure DevOps)

**Before:**
```powershell
az pipelines create --repository-type tfsgit --repository FastFood
```

**After:**
```powershell
az pipelines create --repository-type github --repository marc-mueller/FastFoodBetterTogether
```

### Branch Protection

**Before (Azure DevOps):**
```bash
az repos policy approver-count create --branch main --repository-id $repoId
```

**After (GitHub API):**
```bash
curl -X PUT https://api.github.com/repos/{owner}/{repo}/branches/main/protection
```

## Testing Checklist

Use this to verify migration success:

- [ ] All CI pipelines trigger on PR
- [ ] All CI pipelines trigger on push to main
- [ ] PR initialize pipeline creates environments
- [ ] PR security scan runs and reports status
- [ ] Status checks appear in GitHub PR UI
- [ ] Branch protection prevents direct push to main
- [ ] Required status checks prevent premature merge
- [ ] CD pipelines trigger after CI completes
- [ ] PR deployments create namespaces
- [ ] PR cleanup can be triggered (manual/scheduled)

## Rollback Procedure

If you need to roll back to Azure Repos:

1. Update `setup-pipelines.yml` repositoryType to `tfsgit`
2. Run `setup-pipelines.yml` to recreate pipelines
3. Remove `pr:` sections from CI pipelines (optional)
4. Change status templates back to `step-setprstatus.yml`
5. Uncomment webhook resources in `pr-cleanup.yml` and `pr-workitemcheck.yml`
6. Recreate branch policies via `setup-branchpolicies.yml`

## References

- [Main Migration Guide](GITHUB-MIGRATION.md)
- [Quick Start Guide](QUICK-START.md)
- [Validation Script](validate-github-migration.sh)
