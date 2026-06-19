using BsddRevitPlugin.Logic.UI.Services;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace BsddRevitPlugin.V2026.Services
{
    /// <summary>
    /// WebView2-backed browser service for Revit 2026 (which no longer ships CefSharp).
    /// Implements the same <see cref="IBrowserService"/> contract the shared UI expects, including a
    /// small JavaScript shim so the shared code's <c>CefSharp.BindObjectAsync('name')</c> call keeps
    /// working: it maps <c>window.name</c> onto the WebView2 async host-object proxy.
    /// </summary>
    public class BrowserService2026 : IBrowserService
    {
        private readonly ICustomBrowserControl customBrowserControl;
        private readonly WebView2 webView;

        private readonly Dictionary<string, object> _pendingHostObjects = new Dictionary<string, object>();
        private string _pendingAddress;
        private bool _initStarted;
        private bool _initialized;

        // Emulate the CefSharp JS surface used by the shared bSDD web UI on top of WebView2.
        private const string CefSharpCompatShim = @"
            window.CefSharp = window.CefSharp || {};
            window.CefSharp.BindObjectAsync = function () {
                var names = Array.prototype.slice.call(arguments).filter(function (a) { return typeof a === 'string'; });
                names.forEach(function (n) {
                    try {
                        if (window.chrome && window.chrome.webview && window.chrome.webview.hostObjects) {
                            window[n] = window.chrome.webview.hostObjects[n];
                        }
                    } catch (e) { }
                });
                return Promise.resolve({ Success: true, Count: names.length });
            };
            window.CefSharp.PostMessage = window.CefSharp.PostMessage || function (msg) {
                try { window.chrome.webview.postMessage(msg); } catch (e) { }
            };
        ";

        public BrowserService2026()
        {
            this.customBrowserControl = new BrowserControl2026();
            this.webView = ((BrowserControl2026)this.customBrowserControl).WebView;

            // Initialize once the control is connected to a window (it has an HWND then).
            this.webView.Loaded += (s, e) => BeginInitialize();
        }

        private async void BeginInitialize()
        {
            if (_initStarted) return;
            _initStarted = true;

            try
            {
                // Revit lives in read-only Program Files, so place the WebView2 user-data folder
                // under the per-user local app data location.
                string userDataFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "BsddRevitPlugin", "WebView2");
                Directory.CreateDirectory(userDataFolder);

                CoreWebView2Environment env = await CoreWebView2Environment.CreateAsync(null, userDataFolder, null);
                await this.webView.EnsureCoreWebView2Async(env);

                CoreWebView2 core = this.webView.CoreWebView2;

                // Inject the CefSharp-compat shim before any page script runs.
                await core.AddScriptToExecuteOnDocumentCreatedAsync(CefSharpCompatShim);

                // Expose any host objects requested before initialization completed.
                foreach (KeyValuePair<string, object> kv in _pendingHostObjects)
                {
                    core.AddHostObjectToScript(kv.Key, kv.Value);
                }

                _initialized = true;

                if (!string.IsNullOrEmpty(_pendingAddress))
                {
                    core.Navigate(_pendingAddress);
                }

                RaiseInitialized();
            }
            catch
            {
                // If the WebView2 runtime is unavailable the panel stays blank; fail silently
                // to avoid taking down the Revit add-in load.
            }
        }

        public ICustomBrowserControl BrowserInstance
        {
            get { return this.customBrowserControl; }
        }

        public object BrowserControl
        {
            get { return this.webView; }
        }

        public string Address
        {
            get { return _initialized ? this.webView.Source?.ToString() : _pendingAddress; }
            set
            {
                _pendingAddress = value;
                if (_initialized && this.webView.CoreWebView2 != null && !string.IsNullOrEmpty(value))
                {
                    this.webView.CoreWebView2.Navigate(value);
                }
            }
        }

        public void LoadUrl(string url)
        {
            Address = url;
        }

        public void RegisterJsObject(string name, object objectToBind, bool isAsync = false)
        {
            // WebView2 exposes host objects as chrome.webview.hostObjects.<name>; the injected shim
            // maps window.<name> onto it so the shared CefSharp-style binding code works unchanged.
            _pendingHostObjects[name] = objectToBind;
            if (_initialized && this.webView.CoreWebView2 != null)
            {
                this.webView.CoreWebView2.AddHostObjectToScript(name, objectToBind);
            }
        }

        public void ExecuteScriptAsync(string script)
        {
            if (_initialized && this.webView.CoreWebView2 != null)
            {
                _ = this.webView.ExecuteScriptAsync(script);
            }
        }

        public void ShowDevTools()
        {
            if (_initialized && this.webView.CoreWebView2 != null)
            {
                this.webView.CoreWebView2.OpenDevToolsWindow();
            }
        }

        public void FocusWebContent()
        {
            this.webView.Dispatcher.BeginInvoke(new Action(() => this.webView.Focus()));

            if (_initialized && this.webView.CoreWebView2 != null)
            {
                const string focusScript = @"
                    (function () {
                        window.focus();
                        var first =
                            document.querySelector('[autofocus]') ||
                            document.querySelector('input:not([type=hidden]):not([disabled]), textarea:not([disabled]), select:not([disabled]), button:not([disabled]), [tabindex]:not([tabindex=""-1""])');

                        if (first && typeof first.focus === 'function') {
                            first.focus();
                        } else if (document.body && typeof document.body.focus === 'function') {
                            document.body.setAttribute('tabindex', '-1');
                            document.body.focus();
                        }
                    })();";
                try { _ = this.webView.ExecuteScriptAsync(focusScript); } catch { }
            }
        }

        public event DependencyPropertyChangedEventHandler IsBrowserInitializedChanged;

        private void RaiseInitialized()
        {
            // The shared handlers only check IsBrowserInitialized; the args payload is unused, so a
            // placeholder DependencyProperty is fine here.
            IsBrowserInitializedChanged?.Invoke(
                this,
                new DependencyPropertyChangedEventArgs(FrameworkElement.TagProperty, null, true));
        }

        public bool IsBrowserInitialized => _initialized;
    }
}
