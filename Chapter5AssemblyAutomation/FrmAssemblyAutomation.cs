using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace Chapter5AssemblyAutomation
{
    public partial class FrmAssemblyAutomation : Form
    {
        private readonly SldWorks _swApp;
        private ModelDoc2 _swModel;
        public FrmAssemblyAutomation()
        {
            InitializeComponent();
            //连接SolidWorks
            _swApp = Utility.SolidWorksSingleton.GetApplication();
        }

        private void btnAddComp_Click(object sender, EventArgs e)
        {
            _swApp.CommandInProgress = true;
            string currentDir = System.Environment.CurrentDirectory;//运行程序的路径
            //零部件的地址和装配体的地址
            //将模型文件夹（Guitar Effect Pedal）包括在项目中，然后设置其中的文件属性，复制到输出目录
            string strCompModelDir = Path.Combine(currentDir, "Guitar Effect Pedal", "control knob.SLDPRT");
            string strAssyModelDir = Path.Combine(currentDir, "Guitar Effect Pedal", "Guitar Effect Pedal.SLDASM");
            //打开装配体
            int errors = 0;
            int warnings = 0;
            _swModel=_swApp.OpenDoc6(strAssyModelDir, (int)swDocumentTypes_e.swDocASSEMBLY, (int)swOpenDocOptions_e.swOpenDocOptions_Silent, "", ref errors, ref warnings);
            //选择机壳面：代码是我用vba录制宏得到的，其中原理暂时还不明白
            _swModel.Extension.SelectByRay(0.0492028325765, 0.034925, -9.45739026733257E-02, -0.504351911997689, -0.544791294714104, -0.669948948852078, 6.42794555525274E-04, 2, false, 0, 0);

            //提前打开装配体的情况，手动选择机壳面
            //ModelDoc2 swModel = _swApp.ActiveDoc;
            //if (!(swModel is AssemblyDoc))
            //{
            //    _swApp.SendMsgToUser("请打开装配体");
            //    return;
            //}

            //获取装配体的名称，去掉扩展名
            string assemblyName = ParseAssemblyName(_swModel.GetTitle());

            SelectionMgr swSelMgr = _swModel.SelectionManager;
            //测试所选择面的有效性
            var selObjType = swSelMgr.GetSelectedObjectType3(1, -1);
            Face2 swSafeSelFace = default;
            if (selObjType == (int)swSelectType_e.swSelFACES)
            {
                var swSelFace = swSelMgr.GetSelectedObject6(1, -1);
                Entity swEntity = swSelFace;//存储指向所选面的指针
                //安全实体是能够在主要几何信息变化后任然保持其有效性的实体对象
                swSafeSelFace = (Face2)swEntity.GetSafeEntity();
            }
            else
            {
                _swApp.SendMsgToUser("没选中面");
            }

            //变换，装配体空间和零件空间的相对位置（原点的距离和旋转角度）
            MathTransform swCompTransform = EstablishTargetComponentsTransform(swSafeSelFace);
            //打开部件的零件文件
            AssemblyDoc swAssy= OpenComponentModelToAddToAssembly(strCompModelDir, assemblyName);
            //建立线、边集合
            EstablishCircularCurveAndEdgeList(swSafeSelFace, out List<Edge> circularEdgeList, out List<Curve> circularCurveList);
            //建立面集合
            EstablishCylindricalFaceList(circularEdgeList,out List<Entity> safeCylindricalFaceList);
            //建立点集合
            
            EstablishPointsList(swCompTransform, circularCurveList, out List<MathPoint> pointList);
            //添加旋钮并将其装配到机箱上
            AddcomponentsToAssembly( _swModel, swAssy, pointList, "control knob.SLDPRT", swSafeSelFace, assemblyName, safeCylindricalFaceList);
           

            _swApp.CloseDoc(strCompModelDir);
            //恢复设置
            _swApp.DocumentVisible(true, (int)swDocumentTypes_e.swDocPART);
            _swApp.CommandInProgress = false;
            _swApp.SendMsgToUser("装配完成！");
        }
        
        private string ParseAssemblyName(string assyTitle)
        {
            var strings = assyTitle.Split('.');
            return strings[0];
        }
        private MathTransform EstablishTargetComponentsTransform(Face2 swSafeSelFace)
        {
            //使用变换可以确定或移动零部件在装配体中的物理位置
            Entity swSelFace = swSafeSelFace as Entity;
            Component2 swComp = swSelFace.GetComponent();
            //创建数学变换矩阵
            return swComp.Transform2;
        }

        private AssemblyDoc OpenComponentModelToAddToAssembly(string strCompModelDir, string assemblyName)
        {
            int errors = 0;
            int warnings = 0;
            //关闭任何新打开文件的可见性，避免界面杂乱无章。
            _swApp.DocumentVisible(false, (int)swDocumentTypes_e.swDocPART);
            //打开部件的零件文件。
            _swApp.OpenDoc6(strCompModelDir, (int)swDocumentTypes_e.swDocPART, 0, "", ref errors, ref warnings);
            //重新激活装配体以完成其余工作。
            _swModel = _swApp.ActivateDoc3(assemblyName, false, (int)swRebuildOnActivation_e.swDontRebuildActiveDoc, errors);
            return _swModel as AssemblyDoc;
        }
        private void EstablishCircularCurveAndEdgeList(Face2 swSafeSelFace, out List<Edge> circularEdgeList, out List<Curve> circularCurveList)
        {
            circularEdgeList = new List<Edge>();
            circularCurveList = new List<Curve>();
            Loop2 swLoop = swSafeSelFace.GetFirstLoop();//面上的第一个环
            while (swLoop != null)//遍历面上所有的环
            {
                if (!swLoop.IsOuter())//如果是内环，则获取属于该环的边组成数组
                {
                    var swEdges = swLoop.GetEdges();
                    foreach (Edge item in swEdges)
                    {
                        Curve swCurve = item.GetCurve();//获取数组中每条边对应的曲线对象
                        if (swCurve.IsCircle())//判断曲线是不是圆形
                        {
                            swCurve.GetEndParams(out _, out _, out var bIsClosed, out _);
                            if (bIsClosed)//如果曲线是完整回路
                            {
                                circularEdgeList.Add(item);//添加当前边到边集合中，用于确定圆柱面
                                circularCurveList.Add(swCurve);//添加当前曲线当曲线集合中，用于确定圆心点
                            }
                        }
                    }
                }
                swLoop = swLoop.GetNext();//下一个环
            }
        }

        private void EstablishCylindricalFaceList(List<Edge> circularEdgeList, out List<Entity> safeCylindricalFaceList)
        {
            safeCylindricalFaceList = new List<Entity>();
            foreach (Edge edge in circularEdgeList)
            {
                var swFaces = edge.GetTwoAdjacentFaces2();//获取共享边线的两个面数组
                Surface swSurface1 = swFaces[0].GetSurface();
                Surface swSurface2 = swFaces[1].GetSurface();
                Entity swEntity = null;
                //确定哪一个是圆柱面
                if (swSurface1.IsCylinder()) swEntity = swFaces[0] as Entity;
                if (swSurface2.IsCylinder()) swEntity = swFaces[1] as Entity;
                if (swEntity != null)
                {
                    safeCylindricalFaceList.Add(swEntity.GetSafeEntity());//添加到圆柱面集合中，用于建立同心配合
                }
            }
        }

        private void EstablishPointsList(MathTransform swCompTransform,List<Curve> circularCurveList,out List<MathPoint> pointList)
        {
            MathUtility swMathUtility = _swApp.GetMathUtility();
            pointList = new List<MathPoint>();
            foreach (Curve curve in circularCurveList)//遍历每条边线
            {
                var circleParams = curve.CircleParams;//获取圆心
                double[] arrayData = { circleParams[0], circleParams[1], circleParams[2] };//创建数组来保存圆心点坐标
                //使用MathPoint对象，该对象具有将点位置乘以装配体中目标部件变换矩阵的方法
                MathPoint swMathPoint = swMathUtility.CreatePoint(arrayData);
                //使用变换来设置零部件相对于装配体原点的距离和旋转角度
                swMathPoint = swMathPoint.MultiplyTransform(swCompTransform);
                pointList.Add(swMathPoint);//将MathPoint对象添加到点集合中，用于确定添加零件在装配体中的相对位置
            }
        }

        private void AddcomponentsToAssembly(ModelDoc2 swModel, AssemblyDoc swAssy,List<MathPoint> pointList,string strCompFullPath, Face2 swSafeSelFace,string assemblyName, List<Entity> safeCylindricalFaceList)
        {
            for (int i = 0; i < pointList.Count; i++)
            {
                double[] pointData = (double[])pointList[i].ArrayData;//点坐标必须强转成double[]数组，否则下一步报错
                //添加旋钮到装配体中，给定添加位置的坐标值
                Component2 swComp =swAssy.AddComponent5(strCompFullPath, (int)swAddComponentConfigOptions_e.swAddComponentConfigOptions_CurrentSelectedConfig, "", false, "", pointData[0], pointData[1], pointData[2]);

                string strCompName = swComp.Name2;
                SelectData selData = ((SelectionMgr)swModel.SelectionManager).CreateSelectData();
                selData.Mark = 1;
                swModel.ClearSelection2(true);
                //选择装配体中的机壳面
                ((Entity)swSafeSelFace).Select4(true, selData);
                //选择旋钮中的Top平面
                swModel.Extension.SelectByID2($"Top@{strCompName}@{assemblyName}", "PLANE", 0, 0, 0, true, 1, null, (int)swSelectOption_e.swSelectOptionDefault);
                //添加重合约束
                swAssy.AddMate5((int)swMateType_e.swMateCOINCIDENT, (int)swMateAlign_e.swMateAlignCLOSEST, false, 0, 0, 0, 0, 0, 0, 0, 0, false, false, (int)swMateWidthOptions_e.swMateWidth_Centered, out _);
                //清空选择
                swModel.ClearSelection2(true);
                //选择装配体中的圆柱面
                safeCylindricalFaceList[i].Select4(true, selData);
                //选择旋钮的坐标原点
                swModel.Extension.SelectByID2($"Point1@Origin@{strCompName}@{assemblyName}", "EXTSKETCHPOINT", 0, 0, 0, true, 1, null, (int)swSelectOption_e.swSelectOptionDefault);
                //添加同心约束
                swAssy.AddMate5((int)swMateType_e.swMateCOINCIDENT, (int)swMateAlign_e.swMateAlignCLOSEST, false, 0, 0, 0, 0, 0, 0, 0, 0, false, false, (int)swMateWidthOptions_e.swMateWidth_Centered, out _);
                swModel.ClearSelection2(true);
            }
        }

        private void AssyTest_Click(object sender, EventArgs e)
        {
            AddAndMateComp addAndMate = new AddAndMateComp();
            addAndMate.AddMateTest();
        }
    }
}
