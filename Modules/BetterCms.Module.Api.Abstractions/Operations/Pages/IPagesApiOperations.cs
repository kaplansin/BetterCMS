﻿using BetterCms.Module.Api.Operations.Pages.Contents.Content;
using BetterCms.Module.Api.Operations.Pages.Pages;
using BetterCms.Module.Api.Operations.Pages.Pages.Page;
using BetterCms.Module.Api.Operations.Pages.Redirects;
using BetterCms.Module.Api.Operations.Pages.Redirects.Redirect;
using BetterCms.Module.Api.Operations.Pages.Sitemap;
using BetterCms.Module.Api.Operations.Pages.Widgets;
using BetterCms.Module.Api.Operations.Pages.Widgets.Widget;

namespace BetterCms.Module.Api.Operations.Pages
{
    public interface IPagesApiOperations
    {
        IPagesService Pages { get; }
        
        IPageService Page { get; }
        
        IContentService Content { get; }
        
        IWidgetsService Widgets { get; }
        
        IWidgetService Widget { get; }
        
        IRedirectsService Redirects { get; }
        
        IRedirectService Redirect { get; }
        
        ISitemapService Sitemap { get; }
    }
}
