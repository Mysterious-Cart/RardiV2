using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace Rardi.Shared.Services
{
    public class AppService : IDisposable
    {
        private RenderFragment? _appBarContent;
        private string _currentPageTitle = "Rardi";
        private string _currentPageUrl = "/";
        private List<BreadcrumbItem> _breadcrumbs = new();
        private bool _showBackButton = false;
        private Action? _backButtonAction;

        public event Action? StateChanged;

        // App Bar Content
        public RenderFragment? AppBarContent => _appBarContent;
        
        // Page Information
        public string CurrentPageTitle => _currentPageTitle;
        public string CurrentPageUrl => _currentPageUrl;
        public List<BreadcrumbItem> Breadcrumbs => _breadcrumbs;
        public bool ShowBackButton => _showBackButton;
        public Action? BackButtonAction => _backButtonAction;

        public void SetAppBarContent(RenderFragment? content)
        {
            _appBarContent = content;
            NotifyStateChanged();
        }

        public void SetPageInfo(string title, string url, List<BreadcrumbItem>? breadcrumbs = null)
        {
            _currentPageTitle = title;
            _currentPageUrl = url;
            _breadcrumbs = breadcrumbs ?? new List<BreadcrumbItem>();
            NotifyStateChanged();
        }

        public void SetBackButton(bool show, Action? action = null)
        {
            _showBackButton = show;
            _backButtonAction = action;
            NotifyStateChanged();
        }

        public void UpdateFromUrl(string url)
        {
            _currentPageUrl = url;
            
            // Update page info based on URL
            var (title, breadcrumbs) = GetPageInfoFromUrl(url);
            SetPageInfo(title, url, breadcrumbs);
        }

        private (string title, List<BreadcrumbItem> breadcrumbs) GetPageInfoFromUrl(string url)
        {
            var breadcrumbs = new List<BreadcrumbItem>();
            
            return url.ToLowerInvariant() switch
            {
                "/" or "/home" => ("Dashboard", new List<BreadcrumbItem>
                {
                    new("Home", "/", false)
                }),
                
                "/products" => ("Products", new List<BreadcrumbItem>
                {
                    new("Home", "/", true),
                    new("Products", "/products", false)
                }),
                
                "/customers" => ("Customers", new List<BreadcrumbItem>
                {
                    new("Home", "/", true),
                    new("Customers", "/customers", false)
                }),
                
                "/orders" => ("Orders", new List<BreadcrumbItem>
                {
                    new("Home", "/", true),
                    new("Orders", "/orders", false)
                }),
                
                "/settings" => ("Settings", new List<BreadcrumbItem>
                {
                    new("Home", "/", true),
                    new("Settings", "/settings", false)
                }),
                
                var productDetailUrl when productDetailUrl.StartsWith("/products/") => 
                    ("Product Details", new List<BreadcrumbItem>
                    {
                        new("Home", "/", true),
                        new("Products", "/products", true),
                        new("Product Details", url, false)
                    }),
                
                var customerDetailUrl when customerDetailUrl.StartsWith("/customers/") => 
                    ("Customer Details", new List<BreadcrumbItem>
                    {
                        new("Home", "/", true),
                        new("Customers", "/customers", true),
                        new("Customer Details", url, false)
                    }),
                
                _ => ("Page", new List<BreadcrumbItem>
                {
                    new("Home", "/", true),
                    new("Page", url, false)
                })
            };
        }

        private void NotifyStateChanged() => StateChanged?.Invoke();

        public void Dispose()
        {
            StateChanged = null;
        }
    }

    public record BreadcrumbItem(string Text, string Url, bool IsClickable);
}