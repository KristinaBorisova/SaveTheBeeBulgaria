# 🛡️ Branch Protection Setup Guide

## Overview
This guide explains how to configure branch protection rules on GitHub/GitLab to implement the Modified Git Flow strategy for Save The Bee Bulgaria.

## 🎯 Branch Protection Strategy

### Production Branch (`master`)
**Purpose**: Ensure only production-ready code reaches the live site

**Protection Rules**:
- ✅ **Require pull request reviews** (at least 1 approval)
- ✅ **Require status checks to pass** (CI/CD tests)
- ✅ **Require branches to be up to date** before merging
- ✅ **Include administrators** in restrictions
- ✅ **Restrict pushes** to the branch (only via PRs)
- ✅ **Allow force pushes** (disabled)
- ✅ **Allow deletions** (disabled)

### Development Branch (`dev`)
**Purpose**: Allow rapid development while maintaining some safety

**Protection Rules**:
- ✅ **Allow direct pushes** (for rapid development)
- ✅ **Require status checks** (optional - for CI/CD)
- ❌ **No review requirements** (development freedom)
- ❌ **No push restrictions** (allow direct commits)

---

## 🔧 GitHub Configuration Steps

### 1. Navigate to Repository Settings
1. Go to your GitHub repository
2. Click **Settings** tab
3. Click **Branches** in the left sidebar

### 2. Configure `master` Branch Protection
1. Click **Add rule** or **Add branch protection rule**
2. In **Branch name pattern**, enter: `master`
3. Configure the following settings:

#### Required Settings:
- ✅ **Require a pull request before merging**
  - ✅ Require approvals: **1**
  - ✅ Dismiss stale PR approvals when new commits are pushed
  - ✅ Require review from code owners (if you have CODEOWNERS file)

- ✅ **Require status checks to pass before merging**
  - ✅ Require branches to be up to date before merging
  - Add any CI/CD status checks (if you have them)

- ✅ **Require conversation resolution before merging**

- ✅ **Restrict pushes that create files**
  - ✅ Restrict pushes that create files over 100MB

#### Optional Settings:
- ✅ **Require linear history** (prevents merge commits)
- ✅ **Require signed commits** (if you want GPG signing)
- ✅ **Require deployments to succeed before merging** (if using GitHub Deployments)

#### Administrative Settings:
- ✅ **Include administrators** (applies rules to repo admins too)
- ✅ **Allow force pushes** (disabled)
- ✅ **Allow deletions** (disabled)

### 3. Configure `dev` Branch Protection (Optional)
1. Click **Add rule** again
2. In **Branch name pattern**, enter: `dev`
3. Configure minimal settings:

#### Minimal Settings:
- ❌ **Require a pull request before merging** (disabled)
- ✅ **Require status checks to pass before merging** (optional)
- ❌ **Require conversation resolution** (disabled)

#### Administrative Settings:
- ❌ **Include administrators** (disabled - allows direct pushes)
- ✅ **Allow force pushes** (enabled for development flexibility)
- ✅ **Allow deletions** (enabled)

### 4. Save Configuration
1. Click **Create** or **Save changes**
2. Verify the rules are active

---

## 🔧 GitLab Configuration Steps

### 1. Navigate to Repository Settings
1. Go to your GitLab project
2. Click **Settings** → **Repository**
3. Expand **Push Rules** section

### 2. Configure Push Rules for `master`
1. In **Branch name pattern**, enter: `master`
2. Configure the following:

#### Required Settings:
- ✅ **Require approval from code owners**
- ✅ **Reject unsigned commits**
- ✅ **Reject files larger than** (e.g., 10MB)

### 3. Configure Merge Request Rules
1. Go to **Settings** → **Merge requests**
2. Configure:

#### Required Settings:
- ✅ **Merge method**: Merge commit
- ✅ **Merge checks**: All must pass
- ✅ **Merge approvals**: Require 1 approval
- ✅ **Merge when pipeline succeeds**

---

## 🚀 Railway Integration

### Current Configuration
Railway is configured to deploy from the `master` branch automatically. This works perfectly with our branch protection strategy:

1. **Development**: Work in `dev` branch (no deployment)
2. **Release**: Create `release/*` branch from `dev`
3. **Production**: Merge `release/*` to `master` → Railway auto-deploys
4. **Hotfix**: Create `hotfix/*` from `master` → Railway auto-deploys

### Railway Dashboard Settings
Ensure in your Railway project:
- **Deploy Branch**: `master`
- **Auto Deploy**: Enabled
- **Build Command**: Uses `Dockerfile.railway`

---

## 📋 Branch Protection Checklist

### ✅ `master` Branch Protection
- [ ] Require pull request reviews (1+ approval)
- [ ] Require status checks to pass
- [ ] Require branches to be up to date
- [ ] Include administrators in restrictions
- [ ] Restrict direct pushes
- [ ] Disable force pushes
- [ ] Disable branch deletion

### ✅ `dev` Branch Protection (Optional)
- [ ] Allow direct pushes
- [ ] Optional: Require status checks
- [ ] No review requirements
- [ ] Allow force pushes (for development flexibility)

### ✅ Repository Settings
- [ ] Configure CI/CD status checks
- [ ] Set up CODEOWNERS file (optional)
- [ ] Configure branch naming patterns
- [ ] Test protection rules

---

## 🔍 Testing Branch Protection

### Test `master` Protection:
```bash
# This should fail (if protection is working)
git checkout master
echo "test" >> test.txt
git add test.txt
git commit -m "Test direct push"
git push origin master
# Should be rejected by GitHub/GitLab
```

### Test `dev` Branch Freedom:
```bash
# This should work
git checkout dev
echo "test" >> test.txt
git add test.txt
git commit -m "Test direct push to dev"
git push origin dev
# Should succeed
```

---

## 🚨 Emergency Override

### If You Need to Bypass Protection (Emergency Only):

#### GitHub:
1. Go to repository **Settings** → **Branches**
2. Temporarily disable protection rules
3. Make emergency changes
4. Re-enable protection rules

#### GitLab:
1. Go to **Settings** → **Repository**
2. Temporarily disable push rules
3. Make emergency changes
4. Re-enable push rules

### Alternative: Use Hotfix Workflow
Instead of bypassing protection, use the proper hotfix workflow:
```bash
git checkout master
git checkout -b hotfix/emergency-fix
# Make changes
git commit -m "Emergency fix"
git checkout master
git merge hotfix/emergency-fix --no-ff
git push origin master
```

---

## 📞 Troubleshooting

### Common Issues:

1. **"Cannot push to protected branch"**
   - Solution: Use pull request workflow or hotfix branch

2. **"Required status check is pending"**
   - Solution: Wait for CI/CD to complete or check CI/CD configuration

3. **"Required review from code owners"**
   - Solution: Request review from code owners or update CODEOWNERS file

4. **"Branch is not up to date"**
   - Solution: Pull latest changes before creating PR

### Getting Help:
- Check GitHub/GitLab documentation for branch protection
- Review CI/CD pipeline status
- Verify CODEOWNERS file configuration
- Test with non-critical branches first

---

## ✅ Success Indicators

You'll know branch protection is working when:
- ✅ Direct pushes to `master` are rejected
- ✅ Pull requests require approval before merging
- ✅ CI/CD status checks must pass
- ✅ `dev` branch allows direct pushes
- ✅ Railway deploys automatically from `master`
- ✅ Hotfix workflow works smoothly

This setup ensures your production environment remains stable while allowing flexible development workflows.
