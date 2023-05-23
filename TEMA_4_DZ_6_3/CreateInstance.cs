using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace TEMA_4_DZ_6_3
{
    public class CreateInstance
    {
        public static List<Wall> CreateWalls(ExternalCommandData commandData)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;

            Level level1 = GetLevels(commandData)
                .Where(x => x.Name.Equals("Уровень 1"))
                .FirstOrDefault();

            Level level2 = GetLevels(commandData)
                .Where(x => x.Name.Equals("Уровень 2"))
                .FirstOrDefault();

            double width = UnitUtils.ConvertToInternalUnits(10000, UnitTypeId.Millimeters);
            double depth = UnitUtils.ConvertToInternalUnits(5000, UnitTypeId.Millimeters);
            double dx = width / 2;
            double dy = depth / 2;

            List<XYZ> points = new List<XYZ>();
            points.Add(new XYZ(-dx, -dy, 0));
            points.Add(new XYZ(dx, -dy, 0));
            points.Add(new XYZ(dx, dy, 0));
            points.Add(new XYZ(-dx, dy, 0));
            points.Add(new XYZ(-dx, -dy, 0));

            List<Wall> walls = new List<Wall>();

            Transaction transaction = new Transaction(doc, "Построение стен");
            transaction.Start();
            for (int i = 0; i < 4; i++)
            {
                Line line = Line.CreateBound(points[i], points[i + 1]);
                Wall wall = Wall.Create(doc, line, level1.Id, false);
                walls.Add(wall);
                wall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE).Set(level2.Id);
            }

            transaction.Commit();
            return walls;
        }

        public static void AddDoor(ExternalCommandData commandData, Level level1, Wall wall)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            FamilySymbol doorType = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .OfCategory(BuiltInCategory.OST_Doors)
                .OfType<FamilySymbol>()
                .Where(x => x.Name.Equals("0915 x 2134 мм"))
                .Where(x => x.FamilyName.Equals("Одиночные-Щитовые"))
                .FirstOrDefault();

            LocationCurve hostCurve = wall.Location as LocationCurve;
            XYZ point1 = hostCurve.Curve.GetEndPoint(0);
            XYZ point2 = hostCurve.Curve.GetEndPoint(1);
            XYZ point = (point1 + point2) / 2;

            Transaction transaction = new Transaction(doc, "Создание двери");
            transaction.Start();
            if (!doorType.IsActive)
                doorType.Activate();
            doc.Create.NewFamilyInstance(point, doorType, wall, level1, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
            transaction.Commit();
        }

        public static void AddWindows(ExternalCommandData commandData, Level level1, List<Wall> walls)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            FamilySymbol windowType = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .OfCategory(BuiltInCategory.OST_Windows)
                .OfType<FamilySymbol>()
                .Where(x => x.Name.Equals("0915 x 1830 мм"))
                .Where(x => x.FamilyName.Equals("Фиксированные"))
                .FirstOrDefault();

            List<XYZ> points = new List<XYZ>();

            foreach (var wall in walls)
            {
                LocationCurve hostCurve = wall.Location as LocationCurve;
                XYZ point1 = hostCurve.Curve.GetEndPoint(0);
                XYZ point2 = hostCurve.Curve.GetEndPoint(1);
                XYZ point = (point1 + point2) / 2;
                points.Add(point);
            }

            Transaction transaction = new Transaction(doc, "Создание двери");
            transaction.Start();

            if (!windowType.IsActive)
                windowType.Activate();

            for (int i = 0; i < walls.Count; i++)
            {
               doc.Create.NewFamilyInstance(points[i], windowType, walls[i], level1, Autodesk.Revit.DB.Structure.StructuralType.NonStructural).get_Parameter(BuiltInParameter.INSTANCE_SILL_HEIGHT_PARAM).Set(1);
            }

            transaction.Commit();
        }

        //public static void AddRoof(ExternalCommandData commandData, Level level2, List<Wall> walls)
        //{
        //    Document doc = commandData.Application.ActiveUIDocument.Document;

        //    RoofType roofType = new FilteredElementCollector(doc)
        //        .OfClass(typeof(RoofType))
        //        .OfType<RoofType>()
        //        .Where(x => x.Name.Equals("Типовой - 400мм"))
        //        .Where(x => x.FamilyName.Equals("Базовая крыша"))
        //        .FirstOrDefault();

        //    double wallWidth = walls[0].Width;
        //    double dt = wallWidth / 2;
        //    List<XYZ> points = new List<XYZ>();
        //    points.Add(new XYZ(-dt, -dt, 0));
        //    points.Add(new XYZ(dt, -dt, 0)); 
        //    points.Add(new XYZ(dt, dt, 0));
        //    points.Add(new XYZ(-dt, dt, 0));
        //    points.Add(new XYZ(-dt, -dt, 0));


        //    Application application = doc.Application;
        //    CurveArray footprint = application.Create.NewCurveArray();

        //    for (int i = 0; i < 4; i++)
        //    {
        //        LocationCurve curve = walls[i].Location as LocationCurve;
        //        XYZ p1 = curve.Curve.GetEndPoint(0);
        //        XYZ p2 = curve.Curve.GetEndPoint(1);
        //        Line line = Line.CreateBound(p1 + points[i], p2 + points[i + 1]);
        //        footprint.Append(line);
        //    }
        //    ModelCurveArray footPrintToModelCurveMapping = new ModelCurveArray();

        //    Transaction transaction = new Transaction(doc, "Создание крыши");
        //    transaction.Start();
        //    FootPrintRoof footprintRoof = doc.Create.NewFootPrintRoof(footprint, level2, roofType, out footPrintToModelCurveMapping);
        //    foreach (ModelCurve m in footPrintToModelCurveMapping)
        //    {
        //        footprintRoof.set_DefinesSlope(m, true);
        //        footprintRoof.set_SlopeAngle(m, 0.5);
        //    }
        //    transaction.Commit();
        //}


        public static List<Level> GetLevels(ExternalCommandData commandData)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            List<Level> listLevel = new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .OfType<Level>()
                .ToList();
            return listLevel;
        }

        public static void AddRoof2(ExternalCommandData commandData, List<Wall> walls)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            RoofType roofType = new FilteredElementCollector(doc)
                .OfClass(typeof(RoofType))
                .OfType<RoofType>()
                .Where(x => x.Name.Equals("Типовой - 400мм"))
                .Where(x => x.FamilyName.Equals("Базовая крыша"))
                .FirstOrDefault();

            //LocationCurve curve1 = walls[0].Location as LocationCurve;
            //LocationCurve curve2 = walls[2].Location as LocationCurve;

            //    XYZ p1 = curve1.Curve.GetEndPoint(0);
            //    XYZ p2 = curve1.Curve.GetEndPoint(1);
            //    XYZ p3 = curve2.Curve.GetEndPoint(0);
            //    XYZ p4 = curve2.Curve.GetEndPoint(1);

            //CurveArray curveArray = new CurveArray();
            //curveArray.Append(Line.CreateBound(p1, p2));
            //curveArray.Append(Line.CreateBound(p3, p4));

            CurveArray curveArray = new CurveArray();
            //curveArray.Append(Line.CreateBound(p1, p3));
            curveArray.Append(Line.CreateBound(new XYZ(0, -10, 14), new XYZ(0, 0, 20)));
            curveArray.Append(Line.CreateBound(new XYZ(0, 0, 20), new XYZ(0, 10, 14)));


            Level level2 = GetLevels(commandData)
                .Where(x => x.Name.Equals("Уровень 2"))
                .FirstOrDefault();

            var view = new FilteredElementCollector(doc)
                .OfClass(typeof(View))
                .OfType<View>()
                .Where(x => x.Name.Equals("Уровень 2"))
                .FirstOrDefault();

            BoundingBoxXYZ bb1 = walls[0].get_BoundingBox(null);
            XYZ pp1 = bb1.Max;

            BoundingBoxXYZ bb2 = walls[1].get_BoundingBox(null);
            XYZ pp2 = bb2.Max;

            using (Transaction tr = new Transaction(doc))
            {
                tr.Start("Create ExtrusionRoof");

                ReferencePlane plane = doc.Create.NewReferencePlane(pp2, pp1, XYZ.BasisZ, view);


                doc.Create.NewExtrusionRoof(curveArray, plane, level2, roofType, 0, -33);
                //.get_Parameter(BuiltInParameter.ROOF_CONSTRAINT_OFFSET_PARAM)
                // .Set(UnitUtils.ConvertToInternalUnits(4000, UnitTypeId.Millimeters));
                tr.Commit();
            }
        }


    }
}

