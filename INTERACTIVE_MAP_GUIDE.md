# 🗺️ Interactive Beekeepers Map - Setup & Testing Guide

## ✅ What's Been Implemented

1. **New Menu Item**: "Интерактивна карта на Пчелари" under the "Пчелари" dropdown
2. **Map Page**: `/Beekeeper/Map` - Interactive Google Maps page showing all beekeepers
3. **Features**:
   - Interactive map centered on Bulgaria
   - Pins for each registered beekeeper
   - Filter by region
   - Filter by honey type
   - Click pins to see beekeeper info with link to their profile
   - Clean, minimalist design

## 🚀 Running Locally

### Step 1: Start the Application

```bash
cd HoneyWebPlatform.Web
dotnet run
```

The app will start on `http://localhost:5000` (or check the terminal output for the exact URL)

### Step 2: Navigate to the Map Page

1. Open your browser to the running app URL
2. Go to **Пчелари** dropdown menu in the navigation
3. Click **"Интерактивна карта на Пчелари"**

OR directly visit: `http://localhost:5000/Beekeeper/Map`

### Step 3: Expected Behavior

**Without Google Maps API Key (Current State)**:
- Page loads but shows a placeholder message
- Navigation works fine
- Filters UI is visible
- Message explains API key is needed

**With Google Maps API Key**:
- Full interactive map displays
- Pins show for all beekeepers with coordinates
- Filters work to show/hide pins
- Clicking a pin shows beekeeper info

## 🔑 Setting Up Google Maps API Key

### Option 1: Environment Variable (Recommended for Production)

```bash
export GOOGLE_MAPS_API_KEY="YOUR_API_KEY_HERE"
```

### Option 2: appsettings.json

```json
{
  "GoogleMaps": {
    "ApiKey": "YOUR_API_KEY_HERE"
  }
}
```

### Getting a Google Maps API Key

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create or select a project
3. Enable the **Maps JavaScript API**
4. Create credentials → API Key
5. Copy the key and configure it (see options above)

**Important**: Restrict your API key to:
- **Application restrictions**: HTTP referrer (your domain)
- **API restrictions**: Maps JavaScript API

## 📍 Adding Beekeeper Coordinates

Currently, beekeepers don't have coordinates set. To add sample data:

### SQL Script Example:

```sql
UPDATE "Beekeepers" 
SET 
    "Latitude" = 42.6979,  -- Sample coordinates for Sofia
    "Longitude" = 23.3219
WHERE "Id" = '6d013af5-d0e9-4704-a6a9-877ae0000000'; -- Ivan Borissov's ID
```

### Via Code:

Add a method to update beekeeper coordinates:
```csharp
public async Task UpdateBeekeeperCoordinatesAsync(string beekeeperId, double latitude, double longitude)
{
    var beekeeper = await dbContext.Beekeepers
        .FirstOrDefaultAsync(b => b.Id.ToString() == beekeeperId);
    
    if (beekeeper != null)
    {
        beekeeper.Latitude = latitude;
        beekeeper.Longitude = longitude;
        await dbContext.SaveChangesAsync();
    }
}
```

## 🧪 Testing the Features

### 1. Basic Map Display
- Visit `/Beekeeper/Map`
- Should see the map centered on Bulgaria

### 2. Filtering
- Select a region from the dropdown
- Select a honey type from the dropdown
- Markers should hide/show based on filters
- Click "Изчисти филтри" to reset

### 3. Marker Info Windows
- Click any visible marker
- Should display:
  - Beekeeper name
  - Region
  - Honey types
  - "Виж профил →" link

### 4. Navigation
- Click "Виж профил →" in info window
- Should navigate to beekeeper profile page

## 🐛 Troubleshooting

### Map not showing
- Check if Google Maps API key is configured
- Check browser console for errors
- Verify API key restrictions allow your domain

### No markers visible
- Check if any beekeepers have coordinates set
- Default beekeepers will be centered at Bulgaria center (42.7339, 25.4858)

### Filters not working
- Check browser console for JavaScript errors
- Verify beekeeper data has region and honey types

## 📝 Database Migration

Since we added `Latitude` and `Longitude` fields to the `Beekeeper` model, you'll need to create and apply a migration:

```bash
# Create migration
dotnet ef migrations add AddBeekeeperCoordinates --project HoneyWebPlatform.Data

# Apply migration (Development - uses SQLite)
dotnet ef database update --project HoneyWebPlatform.Data
```

**Note**: The current code uses default coordinates if not set, so the page will work without migration, but all markers will be at the same location.

## 🎨 Customization

### Changing Map Colors
Edit `HoneyWebPlatform.Web/Views/Beekeeper/Map.cshtml`:
- Find the `icon` property in the marker creation
- Modify `fillColor` to change pin color (currently `#F59F0A` - golden)

### Adding More Filter Options
Edit the view to add more regions/honey types in the dropdowns

### Changing Map Style
Add Google Maps styling to the map initialization:
```javascript
map.setOptions({
    styles: [/* your custom styles */]
});
```

## 📊 Current Limitations

1. **No coordinates in database**: Beekeepers need manual coordinate updates
2. **Default honey types**: Currently shows "Различни видове мед" for all
3. **Single location per beekeeper**: Assumes one location per beekeeper
4. **No clustering**: Large numbers of markers might be cluttered

## 🚀 Next Steps

1. Get Google Maps API key
2. Add coordinate data to existing beekeepers
3. Enhance honey type detection from actual products
4. Consider adding marker clustering for many markers
5. Add search by beekeeper name
6. Add geocoding to convert addresses to coordinates automatically

## ✅ Checklist

- [x] Add latitude/longitude fields to Beekeeper model
- [x] Create BeekeepersMapViewModel
- [x] Add Map action to BeekeeperController
- [x] Create Map.cshtml view with Google Maps
- [x] Add filters for region and honey type
- [x] Add menu item to navigation
- [x] Add Google Maps API key configuration
- [ ] Get and configure Google Maps API key
- [ ] Add coordinates to existing beekeepers
- [ ] Create database migration for new fields
- [ ] Test with real beekeeper data

