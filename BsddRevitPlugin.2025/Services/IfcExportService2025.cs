using BIM.IFC.Export.UI;
using BsddRevitPlugin.Logic.UI.Services;
using Revit.IFC.Export.Utility;

namespace BsddRevitPlugin.V2025.Services
{
    internal class IfcExportService2025 : IfcExportService
    {
        protected override void SetExportLinkedFiles(IFCExportConfiguration configuration)
        {
            configuration.ExportLinkedFiles = LinkedFileExportAs.DontExport;
        }
        protected override void SetActiveViewId(IFCExportConfiguration configuration, Autodesk.Revit.DB.Document document)
        {
            configuration.ActiveViewId = document.ActiveView.Id;
        }
        protected override void SetActivePhaseId(IFCExportConfiguration configuration)
        {
            configuration.ActivePhaseId = -1;
            //configuration.ActivePhaseId = (int)ElementId.InvalidElementId.Value;
            ////configuration.ActivePhaseId = document.ActiveView.get_Parameter(BuiltInParameter.VIEW_PHASE).AsElementId()?.Value ?? ElementId.InvalidElementId.Value;
        }

    }
}

