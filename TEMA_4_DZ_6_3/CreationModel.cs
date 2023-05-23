using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace TEMA_4_DZ_6_3
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class CreationModel : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
           
            List<Wall> walls = CreateInstance.CreateWalls(commandData);
            Level level1 = CreateInstance.GetLevels(commandData).Where(x => x.Name.Equals("Уровень 1")) as Level;
            Level level2 = CreateInstance.GetLevels(commandData).Where(x => x.Name.Equals("Уровень 2")) as Level;
            CreateInstance.AddDoor(commandData, level1, walls[0]);

            //создаем окна для трех стен кроме первой
            CreateInstance.AddWindows(commandData, level1, walls.GetRange(1, 3));

            //создаем крышу
            CreateInstance.AddRoof2(commandData, walls);

            return Result.Succeeded;
        }
    }
}
