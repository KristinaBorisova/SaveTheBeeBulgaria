# SEO and Schema Implementation Guide

## JSON-LD Schema Implementation

### Blog List Page Schema
```json
{
    "@context": "https://schema.org",
    "@type": "Blog",
    "name": "Пчелния Блог - Спаси Пчелата България",
    "description": "Блог за пчеларство в България с практически съвети и статии",
    "inLanguage": "bg-BG",
    "url": "https://savethebeebulgaria.com/blog",
    "publisher": {
        "@type": "Organization",
        "name": "Спаси Пчелата България",
        "url": "https://savethebeebulgaria.com",
        "logo": "https://savethebeebulgaria.com/img/logo.png",
        "sameAs": [
            "https://facebook.com/savethebeebulgaria",
            "https://instagram.com/savethebeebulgaria",
            "https://youtube.com/savethebeebulgaria"
        ]
    },
    "blogPost": [
        {
            "@type": "BlogPosting",
            "headline": "Борба с варроа - най-ефективните методи",
            "datePublished": "2024-03-15T10:00:00",
            "author": {
                "@type": "Person",
                "name": "Иван Петров"
            },
            "url": "https://savethebeebulgaria.com/blog/varroa-treatment",
            "image": "https://savethebeebulgaria.com/img/varroa-treatment.jpg",
            "description": "Научете най-ефективните методи за борба с варроа в българските пчелни колонии"
        }
    ]
}
```

### Article Page Schema
```json
{
    "@context": "https://schema.org",
    "@type": "Article",
    "headline": "Борба с варроа - най-ефективните методи за българските пчелари",
    "description": "Варроа деструктор е един от най-сериозните паразити, който засяга пчелните колонии в България...",
    "image": "https://savethebeebulgaria.com/img/varroa-treatment.jpg",
    "author": {
        "@type": "Person",
        "name": "Иван Петров",
        "jobTitle": "Опитен пчелар",
        "description": "Опитен пчелар с над 10 години стаж в отглеждането на пчели"
    },
    "publisher": {
        "@type": "Organization",
        "name": "Спаси Пчелата България",
        "url": "https://savethebeebulgaria.com",
        "logo": {
            "@type": "ImageObject",
            "url": "https://savethebeebulgaria.com/img/logo.png"
        }
    },
    "datePublished": "2024-03-15T10:00:00",
    "dateModified": "2024-03-15T10:00:00",
    "inLanguage": "bg-BG",
    "url": "https://savethebeebulgaria.com/blog/varroa-treatment",
    "mainEntityOfPage": {
        "@type": "WebPage",
        "@id": "https://savethebeebulgaria.com/blog/varroa-treatment"
    },
    "wordCount": 1200,
    "timeRequired": "PT5M",
    "articleSection": "Пчеларство",
    "keywords": ["варроа", "пчеларство", "паразити", "лечение", "България"],
    "about": [
        {
            "@type": "Thing",
            "name": "Пчеларство"
        },
        {
            "@type": "Thing",
            "name": "Варроа деструктор"
        },
        {
            "@type": "Thing",
            "name": "Българско пчеларство"
        }
    ]
}
```

## Meta Tags Implementation

### Blog List Page Meta Tags
```html
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Пчелния Блог - Спаси Пчелата България</title>
    <meta name="description" content="Научете повече за пчеларството в България. Статии, съвети и новини от експерти пчелари." />
    <meta name="keywords" content="пчеларство, мед, кошери, България, пчели, варроа, сезонни грижи" />
    <meta name="author" content="Спаси Пчелата България" />
    <meta name="robots" content="index, follow" />
    <meta name="language" content="bg-BG" />
    
    <!-- Open Graph -->
    <meta property="og:title" content="Пчелния Блог - Спаси Пчелата България" />
    <meta property="og:description" content="Научете повече за пчеларството в България. Статии, съвети и новини от експерти пчелари." />
    <meta property="og:type" content="website" />
    <meta property="og:url" content="https://savethebeebulgaria.com/blog" />
    <meta property="og:image" content="https://savethebeebulgaria.com/img/blog-og-image.jpg" />
    <meta property="og:site_name" content="Спаси Пчелата България" />
    <meta property="og:locale" content="bg_BG" />
    
    <!-- Twitter Card -->
    <meta name="twitter:card" content="summary_large_image" />
    <meta name="twitter:title" content="Пчелния Блог - Спаси Пчелата България" />
    <meta name="twitter:description" content="Научете повече за пчеларството в България. Статии, съвети и новини от експерти пчелари." />
    <meta name="twitter:image" content="https://savethebeebulgaria.com/img/blog-og-image.jpg" />
    
    <!-- Canonical URL -->
    <link rel="canonical" href="https://savethebeebulgaria.com/blog" />
</head>
```

### Article Page Meta Tags
```html
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Борба с варроа - най-ефективните методи - Пчелния Блог</title>
    <meta name="description" content="Варроа деструктор е един от най-сериозните паразити, който засяга пчелните колонии в България. Научете най-ефективните методи за борба с този опасен клещ." />
    <meta name="keywords" content="варроа, пчеларство, паразити, лечение, България, пчели, кошери" />
    <meta name="author" content="Иван Петров" />
    <meta name="robots" content="index, follow" />
    <meta name="language" content="bg-BG" />
    
    <!-- Open Graph -->
    <meta property="og:title" content="Борба с варроа - най-ефективните методи" />
    <meta property="og:description" content="Варроа деструктор е един от най-сериозните паразити, който засяга пчелните колонии в България..." />
    <meta property="og:type" content="article" />
    <meta property="og:url" content="https://savethebeebulgaria.com/blog/varroa-treatment" />
    <meta property="og:image" content="https://savethebeebulgaria.com/img/varroa-treatment.jpg" />
    <meta property="og:site_name" content="Спаси Пчелата България" />
    <meta property="og:locale" content="bg_BG" />
    <meta property="article:author" content="Иван Петров" />
    <meta property="article:published_time" content="2024-03-15T10:00:00" />
    <meta property="article:modified_time" content="2024-03-15T10:00:00" />
    <meta property="article:section" content="Пчеларство" />
    <meta property="article:tag" content="варроа" />
    <meta property="article:tag" content="пчеларство" />
    <meta property="article:tag" content="паразити" />
    
    <!-- Twitter Card -->
    <meta name="twitter:card" content="summary_large_image" />
    <meta name="twitter:title" content="Борба с варроа - най-ефективните методи" />
    <meta name="twitter:description" content="Варроа деструктор е един от най-сериозните паразити, който засяга пчелните колонии в България..." />
    <meta name="twitter:image" content="https://savethebeebulgaria.com/img/varroa-treatment.jpg" />
    
    <!-- Canonical URL -->
    <link rel="canonical" href="https://savethebeebulgaria.com/blog/varroa-treatment" />
</head>
```

## Structured Data Implementation

### Breadcrumb Schema
```json
{
    "@context": "https://schema.org",
    "@type": "BreadcrumbList",
    "itemListElement": [
        {
            "@type": "ListItem",
            "position": 1,
            "name": "Начало",
            "item": "https://savethebeebulgaria.com"
        },
        {
            "@type": "ListItem",
            "position": 2,
            "name": "Блог",
            "item": "https://savethebeebulgaria.com/blog"
        },
        {
            "@type": "ListItem",
            "position": 3,
            "name": "Борба с варроа",
            "item": "https://savethebeebulgaria.com/blog/varroa-treatment"
        }
    ]
}
```

### Organization Schema
```json
{
    "@context": "https://schema.org",
    "@type": "Organization",
    "name": "Спаси Пчелата България",
    "url": "https://savethebeebulgaria.com",
    "logo": "https://savethebeebulgaria.com/img/logo.png",
    "description": "Платформа за подкрепа на българските пчелари и популяризиране на качествения български мед",
    "address": {
        "@type": "PostalAddress",
        "addressCountry": "BG",
        "addressLocality": "София"
    },
    "contactPoint": {
        "@type": "ContactPoint",
        "telephone": "+359-888-123-456",
        "contactType": "customer service",
        "email": "info@savethebeebulgaria.com"
    },
    "sameAs": [
        "https://facebook.com/savethebeebulgaria",
        "https://instagram.com/savethebeebulgaria",
        "https://youtube.com/savethebeebulgaria"
    ]
}
```

## SEO Best Practices Implementation

### 1. URL Structure
- **Blog List**: `/blog`
- **Article**: `/blog/article-slug`
- **Category**: `/blog/category/category-name`
- **Author**: `/blog/author/author-name`

### 2. Internal Linking
- Related posts in sidebar
- Author bio links
- Category links
- Tag links
- Breadcrumb navigation

### 3. Image Optimization
```html
<img src="@post.ImageUrl" 
     alt="@post.Title" 
     loading="lazy"
     width="400" 
     height="200"
     itemprop="image">
```

### 4. Content Structure
- Clear heading hierarchy (H1-H6)
- Descriptive meta descriptions
- Relevant keywords naturally integrated
- Internal linking strategy
- External links to authoritative sources

### 5. Performance Optimization
- Lazy loading for images
- Critical CSS inlined
- Minified CSS and JavaScript
- Optimized images (WebP format)
- CDN for static assets

## Bulgarian SEO Considerations

### 1. Language Targeting
```html
<html lang="bg">
<meta name="language" content="bg-BG">
<meta property="og:locale" content="bg_BG">
```

### 2. Local Keywords
- "пчеларство България"
- "български мед"
- "пчелари София"
- "кошери България"
- "варроа лечение"

### 3. Cultural Context
- Bulgarian beekeeping traditions
- Local flora and fauna references
- Seasonal considerations for Bulgaria
- Regional beekeeping practices

## Monitoring and Analytics

### 1. Google Search Console
- Monitor search performance
- Track keyword rankings
- Identify crawl errors
- Monitor Core Web Vitals

### 2. Google Analytics
- Track user behavior
- Monitor content performance
- Analyze traffic sources
- Measure engagement metrics

### 3. Schema Testing
- Use Google's Rich Results Test
- Validate JSON-LD markup
- Test structured data
- Monitor rich snippets

## Implementation Checklist

- ✅ JSON-LD schema for Blog and Article
- ✅ Meta tags for social sharing
- ✅ Canonical URLs
- ✅ Breadcrumb navigation
- ✅ Structured data markup
- ✅ Image optimization
- ✅ Internal linking strategy
- ✅ Bulgarian language targeting
- ✅ Mobile optimization
- ✅ Performance optimization
