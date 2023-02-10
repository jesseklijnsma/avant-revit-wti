#region Namespaces

using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows.Forms;
using System.Linq;
using System.Collections.Generic;
using Avant.WTI.Util;
using Avant.WTI.Drip.Form;

#endregion


namespace Avant.WTI.Drip
{
    /// <summary>
    /// This is the ExternalCommand which gets executed from the ExternalApplication. In a WPF context,
    /// this can be lean, as it just needs to show the WPF. Without a UI, this could contain the main
    /// order of operations for executing the business logic.
    /// </summary>
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class DripCommand : IExternalCommand
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
                DripData data = new DripData(doc, uidoc);

                WTIElementCollector collector = new WTIElementCollector(doc, allDocuments);

                data.columnpoints = collector.GetColumnPoints();
                data.lines = collector.GetGridLines();
                data.areas = collector.GetAreas();
                data.groundLevel = collector.GetGroundLevel();
                data.pipetypes = collector.GetPipeTypes();
                data.systemtypes = collector.GetPipingSystemTypes();
                data.valvefamilies = collector.GetValveFamilies();

                data.pipesizeMap = data.pipetypes.ToDictionary(x => x, x => collector.GetPipeSizes(x));

                // Show the input form
                Application.Run(new WTIForm(data));
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                string caption = ex.Message + "\n" + ex.StackTrace;

                MessageBox.Show("An exception has occured", caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);


                return Result.Failed;
            }
        }

    }

}