# 🚨 URGENT: Image Upload Persistence Fix

## 🎯 **Problem Identified**
After every deployment/update, all uploaded product images disappear because:
- Images stored in `wwwroot/Uploads/` directory
- Docker containers are ephemeral (stateless)
- Files get wiped on every deployment
- Database still references non-existent files

## 🔧 **Immediate Fix: Railway Volume**

### **Step 1: Update Railway Configuration**

Add persistent volume to `railway.json`:

```json
{
  "$schema": "https://railway.app/railway.schema.json",
  "build": {
    "builder": "DOCKERFILE",
    "dockerfilePath": "Dockerfile.railway"
  },
  "deploy": {
    "startCommand": "dotnet HoneyWebPlatform.Web.dll",
    "restartPolicyType": "ON_FAILURE",
    "restartPolicyMaxRetries": 10
  },
  "volumes": [
    {
      "name": "uploads",
      "mountPath": "/app/wwwroot/uploads"
    }
  ]
}
```

### **Step 2: Update Dockerfile**

Modify `Dockerfile.railway` to create upload directories:

```dockerfile
# Use a specific .NET 6.0 image
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy csproj files
COPY HoneyWebPlatform.Web/*.csproj ./HoneyWebPlatform.Web/
COPY HoneyWebPlatform.Data/*.csproj ./HoneyWebPlatform.Data/
COPY HoneyWebPlatform.Data.Models/*.csproj ./HoneyWebPlatform.Data.Models/
COPY HoneyWebPlatform.Services.Data/*.csproj ./HoneyWebPlatform.Services.Data/
COPY HoneyWebPlatform.Services.Data.Models/*.csproj ./HoneyWebPlatform.Services.Data.Models/
COPY HoneyWebPlatform.Services.Mapping/*.csproj ./HoneyWebPlatform.Services.Mapping/
COPY HoneyWebPlatform.Web.Infrastructure/*.csproj ./HoneyWebPlatform.Web.Infrastructure/
COPY HoneyWebPlatform.Web.ViewModels/*.csproj ./HoneyWebPlatform.Web.ViewModels/

# Restore dependencies
RUN dotnet restore "HoneyWebPlatform.Web/HoneyWebPlatform.Web.csproj"

# Copy everything else
COPY . .

# Build and publish
WORKDIR "/src/HoneyWebPlatform.Web"
RUN dotnet build "HoneyWebPlatform.Web.csproj" -c Release -o /app/build
RUN dotnet publish "HoneyWebPlatform.Web.csproj" -c Release -o /app/publish

# Ensure static files are included
RUN ls -la /app/publish/wwwroot/

# Final stage - Fixed for Railway deployment
FROM base AS final
WORKDIR /app

COPY --from=build /app/publish .

# Create upload directories for persistent storage
RUN mkdir -p /app/wwwroot/uploads/HoneyPictures
RUN mkdir -p /app/wwwroot/uploads/PropolisPictures
RUN mkdir -p /app/wwwroot/uploads/PostPictures
RUN mkdir -p /app/wwwroot/uploads/HivePictures
RUN mkdir -p /app/wwwroot/uploads/UsersProfilePictures

ENTRYPOINT ["dotnet", "HoneyWebPlatform.Web.dll"]
```

### **Step 3: Update Application Startup**

Add startup code to ensure directories exist in `Program.cs`:

```csharp
// Add this in Program.cs after app is built but before running
var app = builder.Build();

// Ensure upload directories exist
var uploadPaths = new[]
{
    Path.Combine(app.Environment.WebRootPath, "uploads", "HoneyPictures"),
    Path.Combine(app.Environment.WebRootPath, "uploads", "PropolisPictures"),
    Path.Combine(app.Environment.WebRootPath, "uploads", "PostPictures"),
    Path.Combine(app.Environment.WebRootPath, "uploads", "HivePictures"),
    Path.Combine(app.Environment.WebRootPath, "uploads", "UsersProfilePictures")
};

foreach (var path in uploadPaths)
{
    if (!Directory.Exists(path))
    {
        Directory.CreateDirectory(path);
    }
}
```

## 🚀 **Long-term Solution: Cloud Storage**

### **Option A: Cloudinary (Easiest)**
1. Sign up at cloudinary.com
2. Install NuGet package: `CloudinaryDotNet`
3. Update image upload logic to use Cloudinary API
4. Store only Cloudinary URLs in database

### **Option B: AWS S3**
1. Create AWS S3 bucket
2. Install NuGet package: `AWSSDK.S3`
3. Configure AWS credentials
4. Upload files to S3, store S3 URLs in database

### **Option C: Azure Blob Storage**
1. Create Azure Storage Account
2. Install NuGet package: `Azure.Storage.Blobs`
3. Configure connection string
4. Upload files to blob storage

## 📋 **Immediate Action Plan**

1. **Update railway.json** with volume configuration
2. **Update Dockerfile.railway** with directory creation
3. **Add startup directory creation** in Program.cs
4. **Deploy and test** image persistence
5. **Plan migration** to cloud storage for production

## 🔍 **Testing Steps**

After implementing the fix:
1. Upload a new product image
2. Deploy an update
3. Check if image still exists
4. Verify image displays correctly

## ⚠️ **Important Notes**

- **Current Images**: Will still be lost (need to re-upload)
- **Database**: No changes needed (paths remain the same)
- **Performance**: Local storage is faster than cloud storage
- **Scalability**: Cloud storage is better for multiple instances

## 🎯 **Recommendation**

1. **Immediate**: Implement Railway volume fix
2. **Short-term**: Test and verify persistence
3. **Long-term**: Migrate to Cloudinary for better scalability
4. **Backup**: Always backup important images before major updates
