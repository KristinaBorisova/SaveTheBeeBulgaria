# Integration Guide for Save the Bee Bulgaria Blog

## CMS Integration Notes

### 1. Dynamic Content Replacement

#### Blog List Page Template Variables
```html
<!-- Replace static content with dynamic variables -->
@foreach (var post in Model.Posts)
{
    <article class="blog-card" itemscope itemtype="https://schema.org/BlogPosting">
        <div class="blog-card-image">
            <img src="{{post.imageUrl}}" 
                 alt="{{post.title}}" 
                 loading="lazy"
                 width="400" 
                 height="200"
                 itemprop="image">
            <div class="blog-card-date">
                <time datetime="{{post.createdOn}}" itemprop="datePublished">
                    {{post.createdOn | date: "dd.MM.yyyy"}}
                </time>
            </div>
        </div>
        
        <div class="blog-card-content">
            <h2 class="blog-card-title" itemprop="headline">
                <a href="{{post.url}}">{{post.title}}</a>
            </h2>
            
            <div class="blog-card-meta">
                <span itemprop="author" itemscope itemtype="https://schema.org/Person">
                    <i class="fas fa-user" aria-hidden="true"></i>
                    <span itemprop="name">{{post.author.name}}</span>
                </span>
                <span>
                    <i class="fas fa-comments" aria-hidden="true"></i>
                    <span>{{post.commentsCount}} коментара</span>
                </span>
            </div>
            
            <div class="blog-card-excerpt" itemprop="description">
                {{post.excerpt | truncate: 150}}
            </div>
            
            <a href="{{post.url}}" class="read-more">
                Прочети повече
                <i class="fas fa-arrow-right" aria-hidden="true"></i>
            </a>
        </div>
    </article>
}
```

#### Article Page Template Variables
```html
<!-- Article content replacement -->
<article class="article-main" itemscope itemtype="https://schema.org/Article">
    <div class="article-hero">
        <img src="{{article.imageUrl}}" 
             alt="{{article.title}}" 
             itemprop="image"
             width="800" 
             height="400">
    </div>

    <div class="article-content">
        <header class="article-header">
            <h1 class="article-title" itemprop="headline">{{article.title}}</h1>
            <p class="article-subtitle">{{article.subtitle}}</p>
            
            <div class="article-meta">
                <div class="meta-item">
                    <i class="fas fa-user" aria-hidden="true"></i>
                    <span itemprop="author" itemscope itemtype="https://schema.org/Person">
                        <span itemprop="name">{{article.author.name}}</span>
                    </span>
                </div>
                <div class="meta-item">
                    <i class="fas fa-calendar" aria-hidden="true"></i>
                    <time datetime="{{article.publishedDate}}" itemprop="datePublished">
                        {{article.publishedDate | date: "dd.MM.yyyy"}}
                    </time>
                </div>
                <div class="meta-item">
                    <i class="fas fa-clock" aria-hidden="true"></i>
                    <span>{{article.readingTime}} мин. четене</span>
                </div>
            </div>
        </header>

        <div class="article-body" itemprop="articleBody">
            {{article.content | markdown}}
        </div>

        <!-- Tags -->
        <div class="tags">
            <h3>Тагове</h3>
            <div class="tag-list">
                {% for tag in article.tags %}
                <a href="/blog/tag/{{tag.slug}}" class="tag">{{tag.name}}</a>
                {% endfor %}
            </div>
        </div>
    </div>
</article>
```

### 2. Related Posts Algorithm

#### Backend Implementation (C#)
```csharp
public class RelatedPostsService
{
    public async Task<List<PostAllViewModel>> GetRelatedPostsAsync(string currentPostId, int count = 3)
    {
        var currentPost = await postService.GetPostByIdAsync(currentPostId);
        
        // Get posts with similar tags
        var postsByTags = await postService.GetPostsByTagsAsync(currentPost.Tags, count);
        
        // Get posts by same author
        var postsByAuthor = await postService.GetPostsByAuthorAsync(currentPost.AuthorId, count);
        
        // Get recent posts
        var recentPosts = await postService.GetRecentPostsAsync(count);
        
        // Combine and deduplicate
        var relatedPosts = postsByTags
            .Concat(postsByAuthor)
            .Concat(recentPosts)
            .Where(p => p.Id != currentPostId)
            .DistinctBy(p => p.Id)
            .Take(count)
            .ToList();
            
        return relatedPosts;
    }
}
```

#### Frontend Template
```html
<!-- Related Posts -->
<div class="sidebar-widget">
    <h3 class="widget-title">Свързани статии</h3>
    <div class="related-posts">
        {% for relatedPost in article.relatedPosts %}
        <a href="{{relatedPost.url}}" class="related-post">
            <img src="{{relatedPost.imageUrl}}" 
                 alt="{{relatedPost.title}}" 
                 class="related-post-image">
            <div class="related-post-content">
                <h4>{{relatedPost.title}}</h4>
                <div class="related-post-date">{{relatedPost.publishedDate | date: "dd.MM.yyyy"}}</div>
            </div>
        </a>
        {% endfor %}
    </div>
</div>
```

### 3. Newsletter Integration

#### Backend Service
```csharp
public class NewsletterService
{
    public async Task<bool> SubscribeAsync(string email)
    {
        // Validate email
        if (!IsValidEmail(email))
            return false;
            
        // Check if already subscribed
        var existing = await dbContext.SubscribedEmails
            .FirstOrDefaultAsync(e => e.Email == email);
            
        if (existing != null)
            return false;
            
        // Add to database
        var subscription = new SubscribedEmail
        {
            Email = email,
            SubscribedOn = DateTime.UtcNow,
            IsActive = true
        };
        
        await dbContext.SubscribedEmails.AddAsync(subscription);
        await dbContext.SaveChangesAsync();
        
        // Send welcome email
        await emailService.SendWelcomeEmailAsync(email);
        
        return true;
    }
}
```

#### Frontend Form
```html
<form class="newsletter-form" method="post" action="/newsletter/subscribe">
    <input type="email" 
           name="email" 
           class="newsletter-input" 
           placeholder="Въведете вашия имейл адрес" 
           required
           aria-label="Имейл адрес за абонамент">
    <button type="submit" class="newsletter-button">Абонирай се</button>
</form>
```

### 4. Comment System Integration

#### Backend Controller
```csharp
[HttpPost]
public async Task<IActionResult> AddComment(string postId, string content)
{
    try
    {
        string authorId = User.Identity.IsAuthenticated ? User.GetId() : "guest";
        
        await postService.AddCommentAsync(postId, content, authorId);
        
        TempData[SuccessMessage] = "Коментарът беше добавен успешно!";
        return RedirectToAction("Details", new { id = postId });
    }
    catch (Exception e)
    {
        TempData[ErrorMessage] = "Възникна грешка при добавянето на коментара.";
        return RedirectToAction("Details", new { id = postId });
    }
}
```

#### Frontend Template
```html
<!-- Comments Section -->
<section class="comments-section">
    <div class="comments-header">
        <h2>{{article.commentsCount}} коментара</h2>
    </div>

    {% for comment in article.comments %}
    <div class="comment">
        <div class="comment-avatar">
            {{comment.author.initials}}
        </div>
        <div class="comment-content">
            <div class="comment-author">{{comment.author.name}}</div>
            <div class="comment-date">{{comment.createdDate | date: "dd.MM.yyyy HH:mm"}}</div>
            <div class="comment-text">{{comment.content}}</div>
        </div>
    </div>
    {% endfor %}

    <!-- Comment Form -->
    <div class="comment-form">
        <h3>Сподели мнение</h3>
        <form method="post" action="/blog/{{article.id}}/comment">
            <div class="form-group">
                <label for="content">Вашият коментар:</label>
                <textarea name="content" id="content" required></textarea>
            </div>
            <button type="submit" class="btn-submit">Изпрати коментар</button>
        </form>
    </div>
</section>
```

## Database Schema Extensions

### Posts Table Extensions
```sql
ALTER TABLE Posts ADD COLUMN Excerpt NVARCHAR(500);
ALTER TABLE Posts ADD COLUMN Subtitle NVARCHAR(200);
ALTER TABLE Posts ADD COLUMN ReadingTime INT;
ALTER TABLE Posts ADD COLUMN UpdatedOn DATETIME2;
ALTER TABLE Posts ADD COLUMN Slug NVARCHAR(200);
ALTER TABLE Posts ADD COLUMN MetaDescription NVARCHAR(300);
ALTER TABLE Posts ADD COLUMN FeaturedImageUrl NVARCHAR(500);
```

### Tags Table
```sql
CREATE TABLE Tags (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(100) NOT NULL,
    Slug NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500),
    CreatedOn DATETIME2 DEFAULT GETDATE()
);

CREATE TABLE PostTags (
    PostId UNIQUEIDENTIFIER NOT NULL,
    TagId UNIQUEIDENTIFIER NOT NULL,
    PRIMARY KEY (PostId, TagId),
    FOREIGN KEY (PostId) REFERENCES Posts(Id),
    FOREIGN KEY (TagId) REFERENCES Tags(Id)
);
```

### Newsletter Subscriptions
```sql
CREATE TABLE SubscribedEmails (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Email NVARCHAR(255) NOT NULL UNIQUE,
    SubscribedOn DATETIME2 DEFAULT GETDATE(),
    IsActive BIT DEFAULT 1,
    UnsubscribedOn DATETIME2 NULL
);
```

## Configuration Files

### appsettings.json
```json
{
  "BlogSettings": {
    "PostsPerPage": 10,
    "RelatedPostsCount": 3,
    "ReadingTimeWordsPerMinute": 200,
    "ExcerptLength": 150,
    "EnableComments": true,
    "ModerateComments": false
  },
  "NewsletterSettings": {
    "WelcomeEmailTemplate": "welcome-newsletter",
    "FromEmail": "noreply@savethebeebulgaria.com",
    "FromName": "Спаси Пчелата България"
  },
  "SeoSettings": {
    "DefaultMetaDescription": "Научете повече за пчеларството в България",
    "DefaultKeywords": "пчеларство, мед, кошери, България",
    "SiteUrl": "https://savethebeebulgaria.com",
    "TwitterHandle": "@savethebeebg"
  }
}
```

## Deployment Checklist

### 1. File Structure
```
HoneyWebPlatform.Web/
├── Views/
│   └── Blog/
│       ├── All.cshtml (✅ Updated)
│       └── Details.cshtml (✅ Updated)
├── wwwroot/
│   └── css/
│       └── blog-enhanced.css (✅ New)
└── Controllers/
    └── BlogController.cs (✅ Existing)
```

### 2. Dependencies
- ✅ ASP.NET Core MVC
- ✅ Entity Framework Core
- ✅ AutoMapper
- ✅ Font Awesome (for icons)
- ✅ Bootstrap (existing)

### 3. Environment Variables
```bash
# Production
BLOG_POSTS_PER_PAGE=10
NEWSLETTER_FROM_EMAIL=noreply@savethebeebulgaria.com
SEO_SITE_URL=https://savethebeebulgaria.com
```

### 4. CDN Configuration
```html
<!-- Use CDN for external resources -->
<link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css">
<script src="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/js/all.min.js"></script>
```

## Testing Checklist

### 1. Functionality Testing
- ✅ Blog list page loads correctly
- ✅ Article page displays properly
- ✅ Comments system works
- ✅ Newsletter subscription functions
- ✅ Related posts algorithm works
- ✅ Search functionality (if implemented)

### 2. Responsive Testing
- ✅ Mobile devices (320px - 768px)
- ✅ Tablets (768px - 1024px)
- ✅ Desktop (1024px+)
- ✅ Touch interactions work
- ✅ Navigation is accessible

### 3. Performance Testing
- ✅ Page load times < 3 seconds
- ✅ Core Web Vitals meet targets
- ✅ Images load properly
- ✅ CSS loads without blocking
- ✅ JavaScript executes correctly

### 4. SEO Testing
- ✅ Meta tags are present
- ✅ Schema markup validates
- ✅ URLs are SEO-friendly
- ✅ Images have alt text
- ✅ Internal linking works

## Maintenance Guide

### 1. Regular Updates
- Update CSS framework versions
- Monitor Core Web Vitals
- Check for broken links
- Update content regularly
- Monitor comment spam

### 2. Performance Monitoring
- Use Google PageSpeed Insights
- Monitor Core Web Vitals
- Check server response times
- Monitor database performance
- Track user engagement

### 3. Content Management
- Regular content audits
- Update outdated information
- Add new blog posts
- Manage comments
- Update related posts

## Support and Documentation

### 1. User Documentation
- Content editor guide
- Image optimization tips
- SEO best practices
- Comment moderation guide

### 2. Technical Documentation
- Code architecture overview
- Database schema documentation
- API documentation
- Deployment procedures

### 3. Troubleshooting
- Common issues and solutions
- Performance optimization tips
- SEO troubleshooting
- Browser compatibility issues
