# Feature: github-actions

## Goal
Add GitHub Actions CI/CD workflow matching the Platform pattern, with a unified publish job that picks release vs prerelease environment based on branch.

## Scope
- `.github/workflows/build.yml` — build, security, publish jobs

## Acceptance Criteria
- Build job: restore, build, warning check (threshold 10), test with coverage, codecov, compute version, pack 5 NuGet packages
- Security job: CodeQL analysis
- Publish job: single job with dynamic environment (release on master push, prerelease on PR), push to NuGet, create GitHub release
- Version: `MAJOR_MINOR: '0.4'`, auto-incrementing patch from git tags
- .NET SDKs: 8.0, 9.0, 10.0

## Done Condition
User confirms the workflow is satisfactory.

## Originating Branch
develop
