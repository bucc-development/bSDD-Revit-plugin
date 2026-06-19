using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using BsddRevitPlugin.Logic.IfcJson;
using BsddRevitPlugin.Logic.UI.View;
using BsddRevitPlugin.Logic.UI.Wrappers;
using Newtonsoft.Json;
using NLog;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using static BsddRevitPlugin.Logic.UI.View.BsddSearch;

namespace BsddRevitPlugin.Logic.UI.BsddBridge
{

    // Exposed to JavaScript: a CefSharp bound object (Revit 2024/2025) and a WebView2 host object
    // (Revit 2026); the latter requires the type to be COM-visible.
    [ComVisible(true)]
    public class BsddSearchBridge
    {

        // Declaration of events and external events
        UpdateElementsWithIfcData updateElementsWithIfcData;
        ExternalEvent _bsddLastSelectionEvent;

        private static Window _bsddSearchParent;
        private static BsddBridgeData _bsddBridgeData;


        public BsddSearchBridge(BsddBridgeData bsddBridgeData, ExternalEvent bsddLastSelectionEvent) // Modify this line
        {
            _bsddBridgeData = bsddBridgeData;
            _bsddLastSelectionEvent = bsddLastSelectionEvent;

            updateElementsWithIfcData = new UpdateElementsWithIfcData();
        }
        public void SetParentWindow(Window bsddSearchParent)
        {
            _bsddSearchParent = bsddSearchParent;
        }
        /// <summary>
        /// Saves the data returned from the bSDD API.
        /// </summary>
        /// <param name="ifcJsonData">The form data to save.</param>
        /// <returns>The response from the bSDD API.</returns>
        public string save(string ifcJsonData)
        {
            //Get BsddBridgeData

            Logger logger = LogManager.GetCurrentClassLogger();

            logger.Info($"SAVE: Trying to save ifcJsonData to Element: {ifcJsonData}");

            var converter = new IfcJsonConverter();

            // Deserialize the JSON data into an IfcData object using the IfcDataConverter
           // var ifcEntityLst = JsonConvert.DeserializeObject<List<IfcEntity>>(ifcJsonData, converter)
            var bridgeData = JsonConvert.DeserializeObject<BsddBridgeData>(ifcJsonData, converter);
            
            updateElementsWithIfcData.Raise(bridgeData);

            _bsddSearchParent.Dispatcher.Invoke(() => _bsddSearchParent.Close());

            _bsddLastSelectionEvent.Raise();

            // Return the serialized JSON data for the IfcData object
            return JsonConvert.SerializeObject(bridgeData);

        }
        /// <summary>
        /// Closes UI 
        /// </summary>
        /// <returns>The response from the bSDD API.</returns>
        public void cancel()
        {
            //Close UI
            _bsddSearchParent.Dispatcher.Invoke(() => _bsddSearchParent.Close());

        }
        ////public string loadSettings ()
        ////{
        ////    return JsonConvert.SerializeObject(_bsddBridgeData);
        ////}
        /// <summary>
        /// Loads BsddBridgeData.
        /// </summary>
        public string loadBridgeData()
        {
            return JsonConvert.SerializeObject(_bsddBridgeData);
        }
    }

}
