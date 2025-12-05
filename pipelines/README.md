# Azure Pipelines - GitHub Migration

This directory contains Azure Pipelines YAML files that have been migrated to work with GitHub as the source repository.

## 📚 Documentation

Start here based on your needs:

### For Quick Setup
- **[QUICK-START.md](QUICK-START.md)** - Step-by-step setup instructions (5 minutes)

### For Understanding the Migration
- **[GITHUB-MIGRATION.md](GITHUB-MIGRATION.md)** - Complete technical migration guide
- **[CHANGES.md](CHANGES.md)** - What changed and why

### For Implementation Details
- **[IMPLEMENTATION-SUMMARY.md](../IMPLEMENTATION-SUMMARY.md)** - Comprehensive implementation summary

## 🚀 Quick Start (TL;DR)

1. **Install GetGitHubToken Extension**
   - Build extension from `azure-devops-extension/`
   - Upload `.vsix` to Azure DevOps organization
   
2. **GitHub Service Connection**
   - Already configured (ID in `config/var-commonvariables.yml`)

3. **Run Setup Pipelines**
   ```
   1. Run: setup-pipelines.yml (uses service connection automatically)
   2. Run: setup-github-branchprotection.yml
   ```

4. **Test**
   - Create a PR in GitHub
   - Verify pipelines trigger and status checks appear
   - Close PR to verify cleanup runs

## 📁 File Structure

### CI Pipelines
Trigger on push to branches and on pull requests:
- `ci-orderservice.yml`
- `ci-kitchenservice.yml`
- `ci-financeservice.yml`
- `ci-frontendcustomerorderstatus.yml`
- `ci-frontendkitchenmonitor.yml`
- `ci-frontendselfservicepos.yml`
- `ci-targetinfrastructure.yml`

### CD Pipelines
Trigger when CI builds complete:
- `cd-orderservice.yml`
- `cd-kitchenservice.yml`
- `cd-financeservice.yml`
- `cd-frontendcustomerorderstatus.yml`
- `cd-frontendkitchenmonitor.yml`
- `cd-frontendselfservicepos.yml`
- `cd-targetinfrastructure.yml`

### PR Pipelines
Run on pull requests:
- `pr-initialize.yml` - Set up PR environment
- `pr-securityscan.yml` - Security scanning
- `pr-cleanup.yml` - Clean up PR resources (triggers on PR close)
- `pr-workitemcheck.yml` - Validate work items (triggers on PR updates)

### Setup Pipelines
One-time or occasional setup:
- `setup-pipelines.yml` - Create/update all pipelines
- `setup-branchpolicies.yml` - Configure Azure DevOps build policies
- `setup-github-branchprotection.yml` - Configure GitHub branch protection

### Supporting Files

**Templates:**
- `pullrequest/step-setgithubstatus.yml` - Post status to GitHub (uses GetGitHubToken extension)
- `pullrequest/step-setprstatus.yml` - Post status to Azure Repos
- `pullrequest/step-setuniversalstatus.yml` - Auto-detect and post status

**Extension:**
- `../azure-devops-extension/` - Custom GetGitHubToken task for retrieving tokens from service connections

**Validation:**
- `validate-github-migration.sh` - Validate migration is correct

**Configuration:**
- `config/` - Variable templates
- `build/` - Build job templates
- `deploy/` - Deployment job templates
- `test/` - Test step templates

## ✅ Validation

Verify the migration is complete and correct:

```bash
./validate-github-migration.sh
```

Expected output: "✓ All critical checks passed!"

## 🔧 Key Changes for GitHub

### 1. PR Triggers
All CI and PR pipelines now include explicit `pr:` sections:

```yaml
pr:
  branches:
    include:
      - main
      - release/*
  paths:
    include:
      - src/services/myservice/**
```

### 2. Repository Type
`setup-pipelines.yml` supports GitHub:

```yaml
parameters:
  - name: repositoryType
    type: string
    default: 'github'  # Changed from 'tfsgit'
```

### 3. Status Reporting
Universal template auto-detects repository type:

```yaml
- template: pullrequest/step-setuniversalstatus.yml
  parameters:
    contextName: 'my-check'
    state: 'succeeded'
    description: 'Check passed'
```

### 4. Branch Protection
GitHub branch protection configured via:
- Pipeline: `setup-github-branchprotection.yml` (automated)
- Manual: GitHub UI at `Settings → Branches`

## 🔄 Comparison: Azure Repos vs GitHub

| Feature | Azure Repos | GitHub |
|---------|-------------|--------|
| PR Triggers | Automatic | Needs `pr:` section |
| Branch Protection | Azure DevOps | GitHub Settings |
| Status Updates | PR Status API | Commit Status API |
| Auto-Detection | `Build.Repository.Provider == 'TfsGit'` | `Build.Repository.Provider == 'GitHub'` |

## 🐛 Troubleshooting

### Pipelines don't trigger on PR
- Check pipeline has `pr:` section
- Verify Azure Pipelines GitHub App is installed
- Check path filters match changed files

### Status checks don't appear
- Verify `GitHubToken` is configured
- Check token has `repo:status` scope
- Ensure using `step-setuniversalstatus.yml`

### Branch protection not working
- Run `setup-github-branchprotection.yml`
- Or configure manually in GitHub Settings → Branches

See [GITHUB-MIGRATION.md](GITHUB-MIGRATION.md) for detailed troubleshooting.

## 📞 Support

- **Quick Setup**: [QUICK-START.md](QUICK-START.md)
- **Technical Details**: [GITHUB-MIGRATION.md](GITHUB-MIGRATION.md)
- **Changes Reference**: [CHANGES.md](CHANGES.md)
- **Microsoft Docs**: [Azure Pipelines with GitHub](https://learn.microsoft.com/en-us/azure/devops/pipelines/repos/github)

## ✨ Features

- ✅ Works with both Azure Repos and GitHub (auto-detection)
- ✅ Backward compatible
- ✅ Comprehensive documentation
- ✅ Automated validation
- ✅ Zero breaking changes

## 🎯 Status

**Migration Status**: ✅ Complete

**Validation**: ✅ All checks passing

**Documentation**: ✅ Complete

**Code Review**: ✅ Addressed

---

**Last Updated**: 2025-12-04
**Migration Version**: 1.0
