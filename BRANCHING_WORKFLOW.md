# 🌿 Production Branching Strategy - Save The Bee Bulgaria

## 📋 Modified Git Flow Strategy

This project uses a **Modified Git Flow** approach designed for maintaining a stable MVP in production while allowing ongoing feature development.

### Branch Structure

#### 1. **`master`** - Production Branch
- **Purpose**: Always reflects production-ready MVP code
- **Deployment**: Automatically deploys to Railway
- **Protection**: Protected branch, no direct commits
- **Updates**: Only receives merges from `release/*` or `hotfix/*` branches
- **Rule**: Only merge finalized, tested features

#### 2. **`dev`** - Active Development Branch  
- **Purpose**: Integration branch for all new features and experiments
- **Deployment**: Not deployed to production
- **Rule**: All new features, experiments, and testing
- **Protection**: Safe to push directly for rapid development

#### 3. **`release/*`** - Release Preparation Branches
- **Purpose**: Prepare specific features for production release
- **Naming**: `release/v1.1.0`, `release/2024-11-mvp`, etc.
- **Lifecycle**: Created from `dev` → merged to `master` → merged back to `dev`
- **Activities**: Final testing, bug fixes, version bumps, documentation

#### 4. **`hotfix/*`** - Emergency Production Fixes
- **Purpose**: Critical bug fixes for production
- **Naming**: `hotfix/fix-order-email`, `hotfix/cart-crash`, etc.
- **Source**: Created from `master` (current production)
- **Lifecycle**: Fix applied → merged to `master` → merged back to `dev`
- **Fast-track**: Bypasses normal release cycle

#### 5. **`feature/*`** - Feature Development Branches (Optional)
- **Purpose**: Isolate large features or experiments
- **Naming**: `feature/payment-integration`, `feature/advanced-search`
- **Source**: Created from `dev`
- **Lifecycle**: Development → merged back to `dev` when complete

---

## 🔄 Workflow Diagrams

### Normal Monthly Release Flow
```
dev → release/v1.1.0 → master (production)
                    ↓
                   dev (merge back)
```

### Hotfix Flow
```
master → hotfix/critical-bug → master (production)
                              ↓
                             dev (merge back)
```

---

## 🚀 Daily Workflows

### **Starting New Development Work:**
```bash
# Switch to dev branch
git checkout dev

# Pull latest changes
git pull origin dev

# Create feature branch (optional, for large features)
git checkout -b feature/your-feature-name

# Make your changes...
# Test your changes...

# Commit and push to dev
git add .
git commit -m "Add new feature: description"
git push origin dev
```

### **Monthly Release Process (When Features Are Ready):**
```bash
# 1. Create release branch from dev
git checkout dev
git pull origin dev
git checkout -b release/v1.1.0

# 2. Prepare for release (version bumps, final testing, bug fixes)
# Make any necessary adjustments
git commit -m "Prepare release v1.1.0"

# 3. Merge to master (production)
git checkout master
git pull origin master
git merge release/v1.1.0 --no-ff
git tag -a v1.1.0 -m "Release v1.1.0 - New features: X, Y, Z"
git push origin master --tags

# 4. Merge back to dev (to include any release fixes)
git checkout dev
git merge release/v1.1.0 --no-ff
git push origin dev

# 5. Delete release branch (optional, keep for history)
git branch -d release/v1.1.0
```

### **Hotfix Process (Critical Production Bug):**
```bash
# 1. Create hotfix branch from master
git checkout master
git pull origin master
git checkout -b hotfix/fix-order-email

# 2. Fix the bug
# Make changes...
git add .
git commit -m "Fix critical order email bug"

# 3. Merge to master (production) IMMEDIATELY
git checkout master
git merge hotfix/fix-order-email --no-ff
git tag -a v1.0.1 -m "Hotfix: Order email bug"
git push origin master --tags

# 4. Merge back to dev (so dev has the fix too)
git checkout dev
git merge hotfix/fix-order-email --no-ff
git push origin dev

# 5. Delete hotfix branch
git branch -d hotfix/fix-order-email
```

### **Feature Branch Workflow (Optional, for Large Features):**
```bash
# 1. Create feature branch from dev
git checkout dev
git pull origin dev
git checkout -b feature/payment-integration

# 2. Develop feature over days/weeks
git add .
git commit -m "Add payment gateway integration"
git push origin feature/payment-integration

# 3. When complete, merge back to dev
git checkout dev
git pull origin dev
git merge feature/payment-integration --no-ff
git push origin dev

# 4. Delete feature branch
git branch -d feature/payment-integration
git push origin --delete feature/payment-integration
```

---

## 🚀 Railway Deployment Configuration

Railway is configured to deploy from the `master` branch only. This ensures:
- ✅ Only production-ready code gets deployed
- ✅ Development work doesn't affect live site
- ✅ Safe testing environment in `dev` branch
- ✅ Hotfixes can be deployed immediately

---

## 📝 Best Practices

### **Do's:**
- ✅ Always work in `dev` branch for new features
- ✅ Test thoroughly before creating release branches
- ✅ Use descriptive commit messages
- ✅ Keep `dev` branch updated with `master` after releases
- ✅ Tag all releases with semantic versioning
- ✅ Always merge hotfixes back to `dev`

### **Don'ts:**
- ❌ Never push directly to `master`
- ❌ Don't merge untested code to `master`
- ❌ Don't leave `dev` branch behind `master`
- ❌ Don't forget to merge hotfixes back to `dev`

---

## 🏷️ Versioning Strategy

Use **Semantic Versioning** (SemVer):
- **v1.0.0** - MVP release
- **v1.1.0** - Minor feature additions (monthly releases)
- **v1.1.1** - Hotfix/patch releases
- **v2.0.0** - Major breaking changes

Tag format: `v{major}.{minor}.{patch}`

---

## 🔧 Quick Commands Reference

```bash
# Switch branches
git checkout master    # Switch to production branch
git checkout dev       # Switch to development branch

# Check current branch
git branch

# See all branches
git branch -a

# Create new feature branch
git checkout -b feature/feature-name

# Create release branch
git checkout -b release/v1.1.0

# Create hotfix branch
git checkout -b hotfix/fix-description

# Merge release to master (when ready for production)
git checkout master
git merge release/v1.1.0 --no-ff
git tag -a v1.1.0 -m "Release v1.1.0"
git push origin master --tags

# Update dev with latest master
git checkout dev
git merge master
git push origin dev
```

---

## 🛡️ Branch Protection Rules (Recommended)

Configure on GitHub/GitLab:

### `master` (production) branch:
- ✅ Require pull request reviews (at least 1 approval)
- ✅ Require status checks to pass
- ✅ Require branches to be up to date
- ✅ Include administrators in restrictions
- ✅ Restrict who can push (only release/hotfix merges)

### `dev` branch:
- ✅ Allow direct pushes (for rapid development)
- ✅ Optional: Require status checks for CI/CD
- ❌ No strict review requirements (development freedom)

---

## 🎯 Current Status

- **Master Branch**: Production code (deployed to Railway) - MVP v1.0.0
- **Dev Branch**: Development code (safe for testing)
- **Railway**: Configured to deploy from `master` only
- **Next Release**: Use `release/v1.1.0` when features are ready

---

## 📞 Need Help?

If you need to:
- **Deploy to production**: Create `release/*` branch from `dev` → merge to `master`
- **Test new features**: Work in `dev` branch
- **Fix production bug**: Create `hotfix/*` branch from `master`
- **Rollback changes**: Use Git history or create hotfix branch
- **Emergency fixes**: Create hotfix branch from `master`

---

## ✅ Advantages of This Approach

- **Stable Production**: `master` always reflects what's live
- **Development Freedom**: `dev` can have experimental features
- **Controlled Releases**: `release/*` branches allow testing before production
- **Fast Hotfixes**: `hotfix/*` branches bypass normal release cycle
- **Clear History**: Tags and merge commits show release timeline
- **Flexibility**: Monthly releases OR on-demand when features are ready