#!/bin/bash
# Script to validate Azure Pipelines GitHub migration
# This script checks that all required changes have been made

set -e

echo "======================================"
echo "Azure Pipelines GitHub Migration Validator"
echo "======================================"
echo ""

REPO_ROOT=$(git rev-parse --show-toplevel)
cd "$REPO_ROOT"

ERRORS=0
WARNINGS=0

# Function to check for pattern in file
check_pattern() {
    local file="$1"
    local pattern="$2"
    local description="$3"
    
    if [ ! -f "$file" ]; then
        echo "✗ File not found: $file"
        ((ERRORS++))
        return 1
    fi
    
    if grep -q "$pattern" "$file"; then
        echo "✓ $description: $file"
        return 0
    else
        echo "✗ $description: $file"
        ((ERRORS++))
        return 1
    fi
}

# Function to check for pattern NOT in file
check_not_pattern() {
    local file="$1"
    local pattern="$2"
    local description="$3"
    
    if [ ! -f "$file" ]; then
        echo "⚠ File not found: $file"
        ((WARNINGS++))
        return 1
    fi
    
    if ! grep -q "$pattern" "$file"; then
        echo "✓ $description: $file"
        return 0
    else
        echo "⚠ $description: $file"
        ((WARNINGS++))
        return 1
    fi
}

echo "Checking CI Pipelines for PR Triggers..."
echo "----------------------------------------"

CI_PIPELINES=(
    "pipelines/ci-orderservice.yml"
    "pipelines/ci-kitchenservice.yml"
    "pipelines/ci-financeservice.yml"
    "pipelines/ci-frontendcustomerorderstatus.yml"
    "pipelines/ci-frontendkitchenmonitor.yml"
    "pipelines/ci-frontendselfservicepos.yml"
    "pipelines/ci-targetinfrastructure.yml"
    "infrastructure/BuildEnvironment/buildanddeploybuildenvimage.yml"
)

for pipeline in "${CI_PIPELINES[@]}"; do
    check_pattern "$pipeline" "^pr:" "Has PR trigger"
done

echo ""
echo "Checking PR Pipelines..."
echo "------------------------"

check_pattern "pipelines/pr-initialize.yml" "^pr:" "Has PR trigger"
check_pattern "pipelines/pr-securityscan.yml" "^pr:" "Has PR trigger"

echo ""
echo "Checking Repository Type Configuration..."
echo "------------------------------------------"

check_pattern "pipelines/setup-pipelines.yml" "repositoryType" "Has repositoryType parameter"
check_pattern "pipelines/setup-pipelines.yml" "default: 'github'" "Default repository type is github"
check_not_pattern "pipelines/setup-pipelines.yml" "--repository-type tfsgit" "No hardcoded tfsgit"

echo ""
echo "Checking Status Reporting Templates..."
echo "---------------------------------------"

check_pattern "pipelines/pullrequest/step-setgithubstatus.yml" "https://api.github.com" "GitHub status template exists"
check_pattern "pipelines/pullrequest/step-setuniversalstatus.yml" "Build.Repository.Provider" "Universal status template exists"

# Check that PR pipelines use universal status
check_pattern "pipelines/pr-securityscan.yml" "step-setuniversalstatus.yml" "Uses universal status template"
check_pattern "pipelines/deploy/job-verifyprdeployment.yml" "step-setuniversalstatus.yml" "Uses universal status template"

echo ""
echo "Checking GitHub Branch Protection Setup..."
echo "-------------------------------------------"

check_pattern "pipelines/setup-github-branchprotection.yml" "api.github.com/repos" "GitHub branch protection pipeline exists"
check_pattern "pipelines/setup-branchpolicies.yml" "GitHub repositories" "Has GitHub documentation"

echo ""
echo "Checking Webhook-Based Pipelines..."
echo "------------------------------------"

# These should have webhooks commented out or removed
if grep -q "^#.*webhooks:" "pipelines/pr-cleanup.yml" || ! grep -q "webhooks:" "pipelines/pr-cleanup.yml"; then
    echo "✓ pr-cleanup.yml: Webhooks disabled/commented"
else
    echo "⚠ pr-cleanup.yml: Active webhooks found (may not work with GitHub)"
    ((WARNINGS++))
fi

if grep -q "^#.*webhooks:" "pipelines/pr-workitemcheck.yml" || ! grep -q "webhooks:" "pipelines/pr-workitemcheck.yml"; then
    echo "✓ pr-workitemcheck.yml: Webhooks disabled/commented"
else
    echo "⚠ pr-workitemcheck.yml: Active webhooks found (may not work with GitHub)"
    ((WARNINGS++))
fi

echo ""
echo "Checking Documentation..."
echo "-------------------------"

check_pattern "pipelines/GITHUB-MIGRATION.md" "GitHub" "Migration guide exists"

echo ""
echo "======================================"
echo "Validation Summary"
echo "======================================"
echo "Errors:   $ERRORS"
echo "Warnings: $WARNINGS"
echo ""

if [ $ERRORS -eq 0 ]; then
    echo "✓ All critical checks passed!"
    if [ $WARNINGS -gt 0 ]; then
        echo "⚠ There are $WARNINGS warnings to review"
        exit 0
    else
        echo "✓ No warnings found"
        exit 0
    fi
else
    echo "✗ $ERRORS errors found. Please fix before using with GitHub."
    exit 1
fi
