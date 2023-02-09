#region Namespaces

using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

#endregion


namespace RevitTemplate
{
    /// <summary>
    /// This is the ExternalCommand which gets executed from the ExternalApplication. In a WPF context,
    /// this can be lean, as it just needs to show the WPF. Without a UI, this could contain the main
    /// order of operations for executing the business logic.
    /// </summary>
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class EntryCommandSeparateThread : IExternalCommand
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandData"></param>
        /// <param name="message"></param>
        /// <param name="elements"></param>
        /// <returns></returns>
        public virtual Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}