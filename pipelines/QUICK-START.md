# Quick Start Guide: GitHub Migration

## Prerequisites Checklist

- [ ] Repository moved to GitHub
- [ ] Azure DevOps project configured
- [ ] Azure Pipelines GitHub App installed on repository
- [ ] GitHub Personal Access Token created

## 1. Create GitHub Personal Access Token

1. Go to GitHub Settings → Developer settings → Personal access tokens → Tokens (classic)
2. Click "Generate new token (classic)"
3. Select scopes:
   - ✅ `repo` (Full control of private repositories)
   - ✅ `repo:status` (Access commit status)
4. Copy the token (you won't see it again!)

## 2. Configure Token in Azure DevOps

1. Go to your Azure DevOps project
2. Navigate to Pipelines → Library
3. Create a new variable group or update existing one
4. Add a secret variable named `GitHubToken`
5. Paste your GitHub PAT as the value
6. Make this variable available to all pipelines or link it to specific ones

## 3. Run Pipeline Setup

1. Navigate to Pipelines in Azure DevOps
2. Run the `setup-pipelines` pipeline (or create it first from `pipelines/setup-pipelines.yml`)
3. Set parameters:
   - `repositoryType`: `github`
   - `repoName`: `marc-mueller/FastFoodBetterTogether` (or your GitHub repo path)
   - `branchName`: `refs/heads/main`

This will create/update all CI/CD pipelines to use the GitHub repository.

## 4. Configure GitHub Branch Protection

### Option A: Using Azure Pipeline (Recommended)

1. Run the `setup-github-branchprotection` pipeline
2. Verify the settings were applied:
   - Visit: `https://github.com/marc-mueller/FastFoodBetterTogether/settings/branches`

### Option B: Manual Configuration

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
   - ✅ Status checks appear in GitHub PR
   - ✅ Checks are required before merge
   - ✅ PR deployment environments are created

5. After verification, you can close the test PR

## 6. Configure Individual Pipelines (if needed)

If pipelines don't exist yet in Azure DevOps:

1. Go to Pipelines → New Pipeline
2. Select "GitHub"
3. Authorize and select `marc-mueller/FastFoodBetterTogether`
4. Select "Existing Azure Pipelines YAML file"
5. Choose the pipeline file (e.g., `pipelines/ci-orderservice.yml`)
6. Run or save the pipeline

Repeat for each pipeline, or use the `setup-pipelines.yml` automation.

## 7. Handle Webhook-Based Pipelines

The following pipelines used Azure Repos webhooks and need alternative triggering:

### `pr-cleanup.yml`
**Purpose:** Clean up PR environments when PRs are closed

**Options:**
- **Manual:** Run manually when closing PRs
- **Scheduled:** Run on schedule to check for closed PRs
- **GitHub Actions:** Create a GitHub Action to trigger this via API when PRs close

### `pr-workitemcheck.yml`
**Purpose:** Validate work items have descriptions

**Options:**
- **Manual:** Run manually or as part of PR process
- **Scheduled:** Run daily to check all active PRs
- **GitHub Actions:** Create a GitHub Action to trigger this via API on PR updates

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
1. `GitHubToken` variable is configured with correct token
2. Token has `repo:status` scope
3. Pipelines use `step-setuniversalstatus.yml`
4. Repository owner/name are correct (`marc-mueller/FastFoodBetterTogether`)

### Issue: Branch protection not enforced

**Check:**
1. Branch protection rules are configured in GitHub
2. Rule applies to `main` branch
3. Required status checks match pipeline context names
4. Rule is not set to "include administrators" (which can bypass rules)

## Next Steps

1. ✅ Monitor first few PRs to ensure everything works
2. ✅ Update team documentation about the new workflow
3. ✅ Consider implementing GitHub Actions for webhook-based pipelines
4. ✅ Review and adjust branch protection rules as needed

## Support

For detailed information, see:
- `pipelines/GITHUB-MIGRATION.md` - Complete migration guide
- [Azure Pipelines GitHub Integration](https://learn.microsoft.com/en-us/azure/devops/pipelines/repos/github)
- [GitHub Branch Protection](https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/managing-protected-branches/about-protected-branches)
