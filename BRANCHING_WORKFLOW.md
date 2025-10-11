# 🌿 Branching Workflow Guide

## 📋 Branch Strategy

### **Master Branch** (`master`)
- **Purpose**: Production-ready code only
- **Deployment**: Automatically deploys to Railway
- **Rule**: Only merge finalized, tested features
- **Protection**: Should be protected from direct pushes

### **Development Branch** (`dev`)
- **Purpose**: Active development and testing
- **Deployment**: Not deployed to production
- **Rule**: All new features, experiments, and testing
- **Protection**: Safe to push directly

## 🔄 Daily Workflow

### **Starting New Work:**
```bash
# Switch to dev branch
git checkout dev

# Pull latest changes
git pull origin dev

# Create feature branch (optional)
git checkout -b feature/your-feature-name

# Make your changes...
# Test your changes...

# Commit and push to dev
git add .
git commit -m "Add new feature: description"
git push origin dev
```

### **Merging to Production:**
```bash
# Switch to master
git checkout master

# Pull latest changes
git pull origin master

# Merge dev branch
git merge dev

# Push to production (triggers Railway deployment)
git push origin master
```

### **After Production Deploy:**
```bash
# Switch back to dev
git checkout dev

# Merge master back to dev (to keep dev updated)
git merge master
git push origin dev
```

## 🚀 Railway Deployment Configuration

Railway is configured to deploy from the `master` branch only. This ensures:
- ✅ Only production-ready code gets deployed
- ✅ Development work doesn't affect live site
- ✅ Safe testing environment in `dev` branch

## 📝 Best Practices

### **Do's:**
- ✅ Always work in `dev` branch for new features
- ✅ Test thoroughly before merging to `master`
- ✅ Use descriptive commit messages
- ✅ Keep `dev` branch updated with `master`

### **Don'ts:**
- ❌ Never push directly to `master`
- ❌ Don't merge untested code to `master`
- ❌ Don't leave `dev` branch behind `master`

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

# Merge dev to master (when ready for production)
git checkout master
git merge dev
git push origin master

# Update dev with latest master
git checkout dev
git merge master
git push origin dev
```

## 🎯 Current Status

- **Master Branch**: Production code (deployed to Railway)
- **Dev Branch**: Development code (safe for testing)
- **Railway**: Configured to deploy from `master` only

## 📞 Need Help?

If you need to:
- Deploy to production: Merge `dev` → `master`
- Test new features: Work in `dev` branch
- Rollback changes: Use Git history or create hotfix branch
- Emergency fixes: Create hotfix branch from `master`
