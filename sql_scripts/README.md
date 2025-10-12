# SQL Scripts for Honey Web Platform

This folder contains all SQL scripts for manually managing your PostgreSQL database on Railway.

## 📁 Script Organization

### 🔍 **Finding IDs**
- **`find_beekeeper_id.sql`** - Comprehensive script to find and work with BeekeeperIds
- **`simple_beekeeper_finder.sql`** - Quick step-by-step guide to find BeekeeperId
- **`find_author_id.sql`** - Script to find AuthorIds for blog posts
- **`find_both_ids.sql`** - Combined script to find both AuthorId and BeekeeperId

### 🍯 **Honey Management**
- **`manual_honey_insert_postgresql.sql`** - Complete script to insert 5 honey products
- **`quick_honey_test.sql`** - Simple script to insert one test honey product

### 📝 **Post Management**
- **`create_posts_manual.sql`** - Script to create sample blog posts

### 🗃️ **Legacy Scripts**
- **`seed_honey_data.sql`** - Original MySQL script (use PostgreSQL versions instead)

## 🚀 Quick Start Guide

### Step 1: Find Required IDs
```bash
# Run in Railway PostgreSQL console
\i sql_scripts/find_both_ids.sql
```

### Step 2: Insert Sample Data
```bash
# Insert honey products
\i sql_scripts/manual_honey_insert_postgresql.sql

# Insert blog posts
\i sql_scripts/create_posts_manual.sql
```

## 📋 Script Descriptions

### **Finding IDs Scripts**

#### `find_beekeeper_id.sql`
- **Purpose**: Comprehensive beekeeper ID discovery
- **Features**: 
  - Checks existing beekeepers
  - Shows user-beekeeper relationships
  - Provides creation instructions
- **Use When**: You need to find or create BeekeeperIds

#### `simple_beekeeper_finder.sql`
- **Purpose**: Quick beekeeper ID lookup
- **Features**: Step-by-step simple queries
- **Use When**: You want a quick, simple approach

#### `find_author_id.sql`
- **Purpose**: Find AuthorIds for blog posts
- **Features**: Shows users who can write posts
- **Use When**: Creating blog posts manually

#### `find_both_ids.sql`
- **Purpose**: Find both AuthorId and BeekeeperId
- **Features**: Combined lookup for all ID types
- **Use When**: You need both types of IDs

### **Data Insertion Scripts**

#### `manual_honey_insert_postgresql.sql`
- **Purpose**: Insert 5 sample honey products
- **Features**:
  - Creates categories (Linden, Bio, Sunflower, Bouquet, Honeydew)
  - Inserts 5 diverse Bulgarian honey products
  - Uses PostgreSQL-specific syntax
- **Requirements**: Valid BeekeeperId
- **Use When**: You want to populate honey data

#### `quick_honey_test.sql`
- **Purpose**: Insert one test honey product
- **Features**: Simple single honey insertion
- **Use When**: Testing honey insertion

#### `create_posts_manual.sql`
- **Purpose**: Create sample blog posts
- **Features**: Inserts 3 sample blog posts
- **Requirements**: Valid AuthorId (UserId)
- **Use When**: You want to populate blog data

## 🔧 Usage Instructions

### **Railway PostgreSQL Console**
1. Go to Railway dashboard
2. Open PostgreSQL service
3. Click "Data" tab
4. Click "Query" to open console
5. Copy and paste script content
6. Execute the script

### **Command Line (if you have psql)**
```bash
# Connect to Railway database
psql "your-railway-connection-string"

# Run script
\i sql_scripts/find_both_ids.sql
```

### **External Database Tools**
- Use pgAdmin, DBeaver, or TablePlus
- Connect to your Railway PostgreSQL database
- Open SQL editor
- Paste and execute scripts

## ⚠️ Important Notes

### **Before Running Scripts:**
1. **Replace Placeholders**: Update `YOUR_BEEKEEPER_ID_HERE` and `YOUR_AUTHOR_ID_HERE` with actual IDs
2. **Check Dependencies**: Ensure required tables exist
3. **Backup Data**: Consider backing up before major changes

### **PostgreSQL Specific Features:**
- Uses `gen_random_uuid()` for UUID generation
- Uses `NOW()` for timestamps
- Uses `ON CONFLICT DO NOTHING` for safe inserts
- Uses double quotes for identifiers

### **Validation Rules:**
- **Honey**: Title (10-50 chars), Description (30-500 chars), Price (5-25 лв)
- **Posts**: Title (10-50 chars), Content (20-1500 chars)
- **Categories**: Name (2-50 chars)

## 🆔 ID Format Reference

### **UserId/AuthorId Format:**
```
Type: Guid (UUID)
Format: XXXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX
Example: BD56FE08-BD10-4384-89BE-63A211FBBC61
```

### **BeekeeperId Format:**
```
Type: Guid (UUID)
Format: XXXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX
Example: 7ADAF90E-FEC8-492E-8760-FE3190F1D689
```

## 🔗 Related Files

- **Entity Configurations**: `HoneyWebPlatform.Data/Configurations/`
- **Models**: `HoneyWebPlatform.Data.Models/`
- **Migrations**: `HoneyWebPlatform.Data/Migrations/`

## 📞 Support

If you encounter issues:
1. Check Railway logs for errors
2. Verify connection string format
3. Ensure all required IDs exist
4. Check PostgreSQL syntax compatibility

---

**Last Updated**: December 2024
**Database**: PostgreSQL on Railway
**Framework**: ASP.NET Core with Entity Framework
