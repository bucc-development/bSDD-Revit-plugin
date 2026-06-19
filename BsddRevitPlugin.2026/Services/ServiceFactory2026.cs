using BIM.IFC.Export.UI;
using BsddRevitPlugin.Logic.UI.Services;

namespace BsddRevitPlugin.V2026.Services
{
    public class ServiceFactory2026 : IServiceFactory
    {
        public IBrowserService CreateBrowserService()
        {
            return new BrowserService2026();
        }
        public IIfcExportService CreateIfcExportService()
        {
            return new IfcExportService2026();
        }
    }
}

