#region Namespaces

using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows.Forms;
using System.Linq;
using System.Collections.Generic;
using Avant.WTI.Util;
using Avant.WTI.Form;
using Avant.WTI.Data;

#endregion


namespace Avant.WTI
{
    /// <summary>
    /// This is the ExternalCommand which gets executed from the ExternalApplication. In a WPF context,
    /// this can be lean, as it just needs to show the WPF. Without a UI, this could contain the main
    /// order of operations for executing the business logic.
    /// </summary>
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class WTICommand : IExternalCommand
    {

        private Document doc;
        private UIDocument uidoc;
        private HashSet<Document> allDocuments;

        public virtual Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {

                doc = commandData.Application.ActiveUIDocument.Document;
                uidoc = commandData.Application.ActiveUIDocument;
                allDocuments = Utils.GetAllDocuments(doc);

                // Initialize data model
                WTIData data = new WTIData(doc, uidoc);

                WTIElementCollector collector = new WTIElementCollector(doc, allDocuments);

                data.columnpoints = collector.GetColumnPoints();
                data.lines = collector.GetGridLines();
                data.areas = collector.GetAreas();
                data.groundLevel = collector.GetGroundLevel();
                data.pipetypes = collector.GetPipeTypes();
                data.systemtypes = collector.GetPipingSystemTypes();
                data.valvefamilies = collector.GetValveFamilies();

                data.pipesizeMap = data.pipetypes.ToDictionary(x => x, x => collector.GetPipeSizes(x));


                data.LoadPrevious();

                // Check for input value errors
                data.refreshErrorMessages(WTIData.Data.INPUT);
                ErrorDialog errorDialog = new ErrorDialog(data.errorMessages);
                errorDialog.ShowErrors();

                if (errorDialog.maxSeverity == WTIData.DripErrorMessage.Severity.FATAL) return Result.Failed;

                // Show the input form
                Application.Run(new WTIForm(data));
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                string msg = ex.Message + "\n" + ex.StackTrace;

                MessageBox.Show(msg, "An exception has occured", MessageBoxButtons.OK, MessageBoxIcon.Warning);

#if DEBUG
                throw;
#else
                return Result.Failed;
#endif
            }
        }

    }

}