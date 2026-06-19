using BsddRevitPlugin.Logic.UI.Services;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.Windows.Controls;

namespace BsddRevitPlugin.V2026.Services
{
    /// <summary>
    /// Revit 2026 ships WebView2 (no CefSharp). This control hosts a WebView2 instance and
    /// exposes it through the shared <see cref="ICustomBrowserControl"/> abstraction.
    /// </summary>
    public class BrowserControl2026 : Control, ICustomBrowserControl
    {
        public WebView2 WebView { get; private set; }

        public BrowserControl2026()
        {
            this.WebView = new WebView2();
        }

        public void Navigate(string url)
        {
            this.WebView.Source = new Uri(url);
        }
    }
}
