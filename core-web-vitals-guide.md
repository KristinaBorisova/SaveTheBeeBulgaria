# Core Web Vitals Optimization Guide

## Performance Optimization Strategies

### 1. Largest Contentful Paint (LCP) - Target: < 2.5s

#### Critical CSS Implementation
```html
<!-- Critical CSS inlined in <head> -->
<style>
    /* Critical styles for above-the-fold content */
    .blog-enhanced {
        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
        line-height: 1.6;
        color: #2C3E50;
        background: linear-gradient(135deg, #fafafa 0%, #f0f8f0 100%);
        min-height: 100vh;
    }
    
    .blog-enhanced .container {
        max-width: 1200px;
        margin: 0 auto;
        padding: 0 20px;
    }
    
    .blog-enhanced .blog-grid {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(350px, 1fr));
        gap: 2rem;
        margin-bottom: 3rem;
    }
</style>

<!-- Non-critical CSS loaded asynchronously -->
<link rel="preload" href="~/css/blog-enhanced.css" as="style" onload="this.onload=null;this.rel='stylesheet'">
<noscript><link rel="stylesheet" href="~/css/blog-enhanced.css"></noscript>
```

#### Image Optimization
```html
<!-- Responsive images with proper sizing -->
<img src="@post.ImageUrl" 
     alt="@post.Title" 
     loading="lazy"
     width="400" 
     height="200"
     srcset="@post.ImageUrl 400w, @post.ImageUrlLarge 800w"
     sizes="(max-width: 768px) 100vw, (max-width: 1200px) 50vw, 33vw"
     itemprop="image">
```

#### Font Optimization
```html
<!-- Preload critical fonts -->
<link rel="preload" href="https://fonts.googleapis.com/css2?family=Segoe+UI:wght@400;600;700&display=swap" as="style">
<link rel="stylesheet" href="https://fonts.googleapis.com/css2?family=Segoe+UI:wght@400;600;700&display=swap">

<!-- Fallback fonts -->
<style>
    .blog-enhanced {
        font-family: 'Segoe UI', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
    }
</style>
```

### 2. First Input Delay (FID) - Target: < 100ms

#### JavaScript Optimization
```html
<!-- Defer non-critical JavaScript -->
<script src="~/js/blog-enhanced.js" defer></script>

<!-- Inline critical JavaScript -->
<script>
    // Critical functionality only
    document.addEventListener('DOMContentLoaded', function() {
        // Reading progress bar
        window.addEventListener('scroll', function() {
            const article = document.querySelector('.article-main');
            if (article) {
                const articleHeight = article.offsetHeight;
                const articleTop = article.offsetTop;
                const windowHeight = window.innerHeight;
                const scrollTop = window.pageYOffset;
                
                const progress = Math.min(100, Math.max(0, 
                    ((scrollTop - articleTop + windowHeight) / articleHeight) * 100
                ));
                
                const progressBar = document.getElementById('readingProgress');
                if (progressBar) {
                    progressBar.style.width = progress + '%';
                }
            }
        });
    });
</script>
```

#### Event Handler Optimization
```javascript
// Use passive event listeners for scroll events
window.addEventListener('scroll', updateReadingProgress, { passive: true });

// Debounce scroll events
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

const debouncedScrollHandler = debounce(updateReadingProgress, 16);
window.addEventListener('scroll', debouncedScrollHandler, { passive: true });
```

### 3. Cumulative Layout Shift (CLS) - Target: < 0.1

#### Image Dimensions
```html
<!-- Always specify width and height -->
<img src="@post.ImageUrl" 
     alt="@post.Title" 
     width="400" 
     height="200"
     loading="lazy"
     style="aspect-ratio: 400/200;">
```

#### Font Loading Strategy
```css
/* Prevent font swap layout shift */
@font-face {
    font-family: 'Segoe UI';
    font-display: swap; /* Use system font first, then swap */
    src: local('Segoe UI');
}

/* Reserve space for dynamic content */
.blog-card-image {
    height: 200px; /* Fixed height */
    overflow: hidden;
}

.article-hero {
    height: 400px; /* Fixed height */
    overflow: hidden;
}
```

#### Dynamic Content Handling
```css
/* Reserve space for loading states */
.blog-card {
    min-height: 400px; /* Prevent layout shift during loading */
}

.article-content {
    min-height: 500px; /* Reserve space for content */
}

/* Skeleton loading states */
.skeleton {
    background: linear-gradient(90deg, #f0f0f0 25%, #e0e0e0 50%, #f0f0f0 75%);
    background-size: 200% 100%;
    animation: loading 1.5s infinite;
}

@keyframes loading {
    0% { background-position: 200% 0; }
    100% { background-position: -200% 0; }
}
```

## Additional Performance Optimizations

### 1. Resource Hints
```html
<!-- Preload critical resources -->
<link rel="preload" href="~/css/blog-enhanced.css" as="style">
<link rel="preload" href="~/img/hero-image.jpg" as="image">

<!-- Prefetch next page -->
<link rel="prefetch" href="~/blog/page-2">

<!-- DNS prefetch for external resources -->
<link rel="dns-prefetch" href="//fonts.googleapis.com">
<link rel="dns-prefetch" href="//cdnjs.cloudflare.com">
```

### 2. Compression and Minification
```csharp
// In Program.cs or Startup.cs
services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal;
});
```

### 3. Caching Strategy
```html
<!-- Cache static assets -->
<link rel="stylesheet" href="~/css/blog-enhanced.css?v=1.0.0">
<script src="~/js/blog-enhanced.js?v=1.0.0"></script>

<!-- Service Worker for caching -->
<script>
    if ('serviceWorker' in navigator) {
        navigator.serviceWorker.register('/sw.js');
    }
</script>
```

### 4. Lazy Loading Implementation
```html
<!-- Intersection Observer for lazy loading -->
<script>
    const imageObserver = new IntersectionObserver((entries, observer) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                const img = entry.target;
                img.src = img.dataset.src;
                img.classList.remove('lazy');
                observer.unobserve(img);
            }
        });
    });

    document.querySelectorAll('img[data-src]').forEach(img => {
        imageObserver.observe(img);
    });
</script>
```

### 5. Bundle Optimization
```json
// In webpack.config.js or similar
{
    "optimization": {
        "splitChunks": {
            "chunks": "all",
            "cacheGroups": {
                "vendor": {
                    "test": /[\\/]node_modules[\\/]/,
                    "name": "vendors",
                    "chunks": "all"
                }
            }
        }
    }
}
```

## Performance Monitoring

### 1. Core Web Vitals Measurement
```javascript
// Measure Core Web Vitals
function measureWebVitals() {
    // LCP
    new PerformanceObserver((entryList) => {
        for (const entry of entryList.getEntries()) {
            console.log('LCP:', entry.startTime);
        }
    }).observe({ entryTypes: ['largest-contentful-paint'] });

    // FID
    new PerformanceObserver((entryList) => {
        for (const entry of entryList.getEntries()) {
            console.log('FID:', entry.processingStart - entry.startTime);
        }
    }).observe({ entryTypes: ['first-input'] });

    // CLS
    let clsValue = 0;
    new PerformanceObserver((entryList) => {
        for (const entry of entryList.getEntries()) {
            if (!entry.hadRecentInput) {
                clsValue += entry.value;
            }
        }
        console.log('CLS:', clsValue);
    }).observe({ entryTypes: ['layout-shift'] });
}
```

### 2. Performance Budget
```json
{
    "budget": [
        {
            "path": "/*",
            "timings": [
                {
                    "metric": "first-contentful-paint",
                    "budget": 2000
                },
                {
                    "metric": "largest-contentful-paint",
                    "budget": 2500
                },
                {
                    "metric": "cumulative-layout-shift",
                    "budget": 0.1
                }
            ],
            "resourceSizes": [
                {
                    "resourceType": "script",
                    "budget": 250
                },
                {
                    "resourceType": "total",
                    "budget": 1000
                }
            ]
        }
    ]
}
```

## Implementation Checklist

### Core Web Vitals
- ✅ **LCP < 2.5s**: Critical CSS inlined, images optimized
- ✅ **FID < 100ms**: JavaScript deferred, event handlers optimized
- ✅ **CLS < 0.1**: Image dimensions specified, font loading optimized

### Performance Optimizations
- ✅ **Resource Hints**: Preload, prefetch, DNS prefetch implemented
- ✅ **Compression**: Brotli and Gzip compression enabled
- ✅ **Caching**: Static asset versioning and caching headers
- ✅ **Lazy Loading**: Images and non-critical content lazy loaded
- ✅ **Bundle Optimization**: Code splitting and tree shaking

### Monitoring
- ✅ **Performance Monitoring**: Core Web Vitals measurement
- ✅ **Performance Budget**: Defined performance targets
- ✅ **Error Tracking**: Performance error monitoring
- ✅ **Analytics**: Performance metrics tracking

## Tools for Testing

1. **Google PageSpeed Insights**: Overall performance score
2. **Google Lighthouse**: Detailed performance audit
3. **WebPageTest**: Detailed waterfall analysis
4. **Chrome DevTools**: Real-time performance monitoring
5. **GTmetrix**: Performance monitoring and alerts

## Target Metrics

- **LCP**: < 2.5 seconds
- **FID**: < 100 milliseconds
- **CLS**: < 0.1
- **TTFB**: < 600 milliseconds
- **FCP**: < 1.8 seconds
- **SI**: < 3.4 seconds
