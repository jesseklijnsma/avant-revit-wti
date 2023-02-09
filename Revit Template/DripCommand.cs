#region Namespaces

using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows.Forms;
using System.Linq;
using System.Collections.Generic;

#endregion


namespace RevitTemplate
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

                doc = commandData.Application.ActiveUIDocument.Document;
                uidoc = commandData.Application.ActiveUIDocument;
                allDocuments = Util.getAllDocuments(doc);

                DripData data = new DripData(doc, uidoc);

                WTIElementCollector collector = new WTIElementCollector(doc, allDocuments);

                data.columnpoints = collector.getColumnPoints();
                data.lines = collector.getGridLines();
                data.areas = collector.getAreas();
                data.groundLevel = collector.getGroundLevel();
                data.pipetypes = collector.getPipeTypes();
                data.systemtypes = collector.getPipingSystemTypes();
                data.valvefamilies = collector.getValveFamilies();

                data.pipesizeMap = data.pipetypes.ToDictionary(x => x, x => collector.getPipeSizes(x));


                Application.Run(new WTIForm(data));
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                throw;
                //return Result.Failed;
            }
        }


       

        

    }
}