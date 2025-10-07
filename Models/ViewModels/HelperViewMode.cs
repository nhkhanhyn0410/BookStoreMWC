// Helpers/BreadcrumbHelper.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Html;

namespace BookStoreMVC.Models.ViewModels
{
    public class BreadcrumbItem
    {
        public string Title { get; set; }
        public string? Url { get; set; }
        public string? Icon { get; set; }
        public bool IsActive { get; set; }

        public BreadcrumbItem(string title, string? url = null, string? icon = null)
        {
            Title = title;
            Url = url;
            Icon = icon;
            IsActive = url == null;
        }
    }

    public static class BreadcrumbHelper
    {
        private const string BreadcrumbKey = "Breadcrumbs";

        public static void SetBreadcrumb(this Controller controller, params BreadcrumbItem[] items)
        {
            controller.ViewBag.Breadcrumbs = items.ToList();
        }

        public static void AddBreadcrumb(this Controller controller, string title, string? url = null, string? icon = null)
        {
            var breadcrumbs = controller.ViewBag.Breadcrumbs as List<BreadcrumbItem> ?? new List<BreadcrumbItem>();
            breadcrumbs.Add(new BreadcrumbItem(title, url, icon));
            controller.ViewBag.Breadcrumbs = breadcrumbs;
        }

        // Auto-generate breadcrumb từ route data
        public static List<BreadcrumbItem> GenerateFromRoute(ViewContext viewContext, bool isAdmin = false)
        {
            var breadcrumbs = new List<BreadcrumbItem>();
            var routeData = viewContext.RouteData.Values;

            var area = routeData["area"]?.ToString();
            var controller = routeData["controller"]?.ToString();
            var action = routeData["action"]?.ToString();

            // Home/Dashboard
            if (isAdmin)
            {
                breadcrumbs.Add(new BreadcrumbItem("Dashboard", "/admin/dashboard", "fa-home"));
            }
            else
            {
                // breadcrumbs.Add(new BreadcrumbItem("Trang chủ", "/", "fa-home"));
            }

            // Controller level
            if (!string.IsNullOrEmpty(controller) && controller != "Dashboard" && controller != "Home")
            {
                var controllerTitle = GetFriendlyName(controller);
                var controllerUrl = isAdmin ? $"/admin/{controller.ToLower()}" : $"/{controller.ToLower()}";
                breadcrumbs.Add(new BreadcrumbItem(controllerTitle, controllerUrl));
            }

            // Action level (if not Index)
            if (!string.IsNullOrEmpty(action) && action != "Index")
            {
                var actionTitle = GetFriendlyName(action);
                breadcrumbs.Add(new BreadcrumbItem(actionTitle, null));
            }

            return breadcrumbs;
        }

        private static string GetFriendlyName(string name)
        {
            // Map tên controller/action sang tiếng Việt
            var mapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Books", "Quản lý sách" },
                { "Orders", "Quản lý đơn hàng" },
                { "Users", "Quản lý người dùng" },
                { "Categories", "Danh mục" },
                { "Reports", "Báo cáo" },
                { "Settings", "Cài đặt" },
                { "Create", "Thêm mới" },
                { "Edit", "Chỉnh sửa" },
                { "Details", "Chi tiết" },
                { "Delete", "Xóa" }
            };

            return mapping.TryGetValue(name, out var friendlyName) ? friendlyName : name;
        }
    }

    // Extension cho View
    public static class BreadcrumbViewExtensions
    {
        public static List<BreadcrumbItem> GetBreadcrumbs(this IHtmlHelper htmlHelper)
        {
            var viewBag = htmlHelper.ViewContext.ViewBag;

            // Ưu tiên ViewBag.Breadcrumbs nếu có
            if (viewBag.Breadcrumbs != null)
            {
                return viewBag.Breadcrumbs as List<BreadcrumbItem> ?? new List<BreadcrumbItem>();
            }

            // Auto-generate nếu không có
            var isAdmin = htmlHelper.ViewContext.RouteData.Values["area"]?.ToString()?.Equals("Admin", StringComparison.OrdinalIgnoreCase) ?? false;
            return BreadcrumbHelper.GenerateFromRoute(htmlHelper.ViewContext, isAdmin);
        }
    }

    public static class SvgHelpers
    {
        public static string HomeIcon()
        {
            return @"<svg width='18' height='18' viewBox='0 0 20 20' fill='none' xmlns='http://www.w3.org/2000/svg'>
                    <path d='M18.333 10.17v1.267c0 3.251 0 4.876-.977 5.886-.976 1.01-2.547 1.01-5.69 1.01H8.333c-3.143 0-4.714 0-5.69-1.01-.977-1.01-.977-2.635-.977-5.886V10.17c0-1.907 0-2.86.433-3.651.432-.79 1.223-1.281 2.804-2.262l1.666-1.035C8.241 2.185 9.076 1.667 10 1.667s1.76.518 3.43 1.555l1.667 1.035c1.58.98 2.371 1.471 2.804 2.262M12.5 15h-5' stroke='#6B7280' stroke-opacity='.8' stroke-width='1.5' stroke-linecap='round'/>
                </svg>";
        }
    }


}