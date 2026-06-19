// Vendored from ASRR.Revit.Core (lib-asrr-revit-core) so the plugin can build for both
// net48 and net8.0-windows without depending on the net48-only ASRR submodules.
// Only the members actually used by the plugin are kept. Uses the modern ForgeTypeId/
// UnitTypeId unit API, which is valid for Revit 2024 (net48) and 2025/2026 (net8).
// Namespace is kept identical to the original so existing `using` statements are unchanged.
// Source: https://github.com/ASRRtechnologies/lib-asrr-revit-core (Utilities/CoordinateUtilities)
using Autodesk.Revit.DB;
using System;

namespace ASRR.Revit.Core.Utilities
{
    public class CoordinateUtilities
    {
        public static double ConvertMmToFeet(double millimeterValue)
        {
            return UnitUtils.Convert(millimeterValue, UnitTypeId.Millimeters, UnitTypeId.Feet);
        }

        public static double ConvertFeetToMm(double feetValue, bool round = false)
        {
            var output = UnitUtils.Convert(feetValue, UnitTypeId.Feet, UnitTypeId.Millimeters);

            if (round)
            {
                output = Math.Round(output * 100) / 100;
            }

            return output;
        }
    }
}
