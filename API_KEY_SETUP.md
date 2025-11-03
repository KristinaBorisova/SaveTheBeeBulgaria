# 🔑 API Key Security Setup Guide

## Protecting Your API Keys in Public Repository

To keep your API keys private when pushing to a public repository, follow these steps:

### Step 1: For Local Development

**Option A: Create `appsettings.Development.json`** (Recommended for local development)
```bash
# Copy the example file
cp HoneyWebPlatform.Web/appsettings.Development.json.example HoneyWebPlatform.Web/appsettings.Development.json

# Edit the file and add your API key:
# "GoogleMaps": {
#   "ApiKey": "YOUR_ACTUAL_API_KEY_HERE"
# }
```

**Option B: Use Environment Variable** (Recommended for all environments)
```bash
# macOS/Linux:
export GOOGLE_MAPS_API_KEY="AIzaSyCiMZ_KuLx1yqC7rjhx4Ea8R_AAW6qQOTw"

# Windows PowerShell:
$env:GOOGLE_MAPS_API_KEY="AIzaSyCiMZ_KuLx1yqC7rjhx4Ea8R_AAW6qQOTw"

# Windows CMD:
set GOOGLE_MAPS_API_KEY=AIzaSyCiMZ_KuLx1yqC7rjhx4Ea8R_AAW6qQOTw
```

### Step 2: For Railway Production

In Railway Dashboard:
1. Go to your project → **Variables**
2. Add a new variable:
   - **Key:** `GOOGLE_MAPS_API_KEY`
   - **Value:** `AIzaSyCiMZ_KuLx1yqC7rjhx4Ea8R_AAW6qQOTw`
3. Save - Railway will automatically restart your app

### Step 3: Verify `.gitignore` Includes Sensitive Files

The following files are now ignored and won't be committed:
- `appsettings.Development.json`
- `appsettings.Local.json`
- `*.secrets.json`

### Step 4: Commit and Push

Now you can safely commit and push:

```bash
# Check what will be committed (should NOT include API keys)
git status

# Add your changes
git add .

# Commit
git commit -m "Add interactive beekeepers map feature"

# Push to public repository
git push origin feature/interactiveMap
```

## How It Works

The application checks for the API key in this order:
1. **Environment Variable:** `GOOGLE_MAPS_API_KEY` (highest priority)
2. **Configuration:** `appsettings.Development.json` → `GoogleMaps:ApiKey`
3. **Configuration:** `appsettings.json` → `GoogleMaps:ApiKey` (empty by default)

This way:
- ✅ Your local `appsettings.Development.json` with the key stays on your machine
- ✅ Railway uses environment variables (secure)
- ✅ Public repository has no API keys exposed

## Files That Are Safe to Commit

- ✅ `appsettings.json` (API key is empty)
- ✅ `appsettings.Production.json` (uses `${GOOGLE_MAPS_API_KEY}` variable)
- ✅ `appsettings.Development.json.example` (template with placeholder)

## Files That Are Ignored (Not Committed)

- ❌ `appsettings.Development.json` (your local file with actual key)
- ❌ `*.secrets.json`
- ❌ Database files (`*.db`, `*.db-shm`, `*.db-wal`)

## Important Notes

⚠️ **If you've already committed the API key:**
1. The key has been exposed in git history
2. You should **regenerate** your Google Maps API key in Google Cloud Console
3. Add restrictions to the new key (HTTP referrer restrictions)
4. Update the key in Railway and locally

⚠️ **Best Practice:**
- Never commit API keys, passwords, or secrets to git
- Use environment variables for production
- Use local config files (in `.gitignore`) for development

