using BIM.IFC.Export.UI;
using BsddRevitPlugin.Logic.UI.Services;

namespace BsddRevitPlugin.V2025.Services
{
    public class ServiceFactory2025 : IServiceFactory
    {
        public IBrowserService CreateBrowserService()
        {
            return new BrowserService2025();
        }
        public IIfcExportService CreateIfcExportService()
        {
            return new IfcExportService2025();
        }
    }
}

