using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MapGIS.GeoDataBase;
using MapGIS.GeoObjects.Att;
using System.Threading;
using MapGIS.GeoMap;
using MapGIS.GeoObjects.Geometry;
using MapGIS.GeoObjects.Info;
using MapGIS.GISControl;
using MapGIS.GISControl.IATool;
using MapGIS.UI.Controls;
using System.Data.SqlClient;
using MapGIS.GeoObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

//D:\GIS\GIS二次开发\GIS_Redevelopment_Cluster\GIS_Redevelopment_Cluster\bin\x64\Debug\net5 .0 - windows


namespace GIS_Redevelopment_Cluster
{
    public partial class Form1 : Form
    {
        //在SplitContainer添加MapControl控件
        MapControl mapCtrl = new MapControl();
        //工作空间树
        MapWorkSpaceTree _Tree = new MapWorkSpaceTree();
        //定义数据源、数据库变量
        Server Svr = null;
        DataBase GDB = null;
        SFeatureCls sfcls = null;

        public Form1()
        {
            InitializeComponent();
            initControls();
        }


        public void initControls()
        {
            //MapControl控件在Panel2里
            this.splitContainer1.Panel2.Controls.Add(mapCtrl);
            mapCtrl.Width = this.splitContainer1.Panel2.Width;
            mapCtrl.Height = this.splitContainer1.Panel2.Height;
            //工作空间树控件加载到Panel1上
            _Tree.Dock = DockStyle.Fill;
            this.splitContainer1.Panel1.Controls.Add(_Tree);

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
                //string strTest = "该例子测试一个字符串写入文本文件。";
                //System.IO.File.WriteAllText(@"D:\test1.txt", strTest, Encoding.UTF8);


                //定义数据源、数据库变量
                Server Svr = null;
                DataBase GDB = null;
                SFeatureCls SFCls = null;

                Svr = new Server();
                //连接数据源
                Svr.Connect("MapGisLocal", "", "");
                GDB = new DataBase();
                GDB = Svr.OpenGDB("空间信息高性能计算实验");
                SFCls = new SFeatureCls(GDB);

                //打开图层
                SFCls.Open("wuhan_busstop", 1);

                //新建一个文档树
                _Tree.WorkSpace.BeginUpdateTree();
                _Tree.Document.Title = "地图文档";
                _Tree.Document.New();

                //在地图文档下添加一个地图
                Map map = new Map();
                map.Name = "新地图";
                //附加矢量图层
                VectorLayer vecLayer = new VectorLayer(VectorLayerType.SFclsLayer);
                vecLayer.AttachData(SFCls);
                //将图层添加到地图中
                vecLayer.Name = SFCls.ClsName;
                map.Append(vecLayer);

                _Tree.Document.GetMaps().Append(map);
                //将该地图设置为MapConrol的激活地图
                this.mapCtrl.ActiveMap = map;
                this.mapCtrl.Restore();

                //展开所有的节点
                _Tree.ExpandAll();
                _Tree.WorkSpace.EndUpdateTree();

                vecLayer.DetachData();//附加解除 
            
            

            QueryDef _QueryDef = null;
            Rect rect = new Rect();
            RecordSet _RecordSet = null;
            IGeometry _Geometry = null;
            SpaQueryMode mode = new SpaQueryMode();
            GeoVarLine _GeoVarLine = null;
            GeoLines _GeoLines = null;
            GeoPoints _Geopoints = null;

            //int id = 0;
            //创建简单要素类
            //SFeatureCls result_sfcls = null;
            //result_sfcls = new SFeatureCls(GDB);



            _QueryDef = new QueryDef();
            rect.XMax = SFCls.Range.XMax;
            rect.YMax = SFCls.Range.YMax;
            rect.YMin = SFCls.Range.YMin;
            rect.XMin = SFCls.Range.XMin;
            mode = SpaQueryMode.Intersect;

            _QueryDef = new QueryDef();
            _QueryDef.SetRect(rect,mode);
            _RecordSet = SFCls.Select(_QueryDef);


            bool rtn;
            rtn = _RecordSet.MoveFirst();

            //文件流
            FileStream fs = new("Pre_cluster_points.txt", System.IO.FileMode.Create, FileAccess.Write);
            StreamWriter sw = new(fs);
            //string str =  "行号" + "," + "列号" + "," + "像元值";
            //sw.WriteLine(str);

            int point_counts=0;

            while (!_RecordSet.IsEOF)
            {
                _Geometry = _RecordSet.Geometry;//获取当前要素的空间信息
                GeometryType type = _Geometry.Type;//获取当前要素的几何约束类型

                int oid = (int)_RecordSet.CurrentID;

                if (_Geometry != null)
                {
                    switch (type)
                    {
                        //case GeometryType.VarLine:
                        //    {
                        //        _GeoVarLine = new GeoVarLine();
                        //        _GeoVarLine = _Geometry as GeoVarLine;
                        //    }
                        case GeometryType.Points:
                            {
                                _Geopoints = new GeoPoints();
                                _Geopoints = _Geometry as GeoPoints;

                                //Dot3D dot = _Geopoints.GetItem(oid);
                                Dots3D dots = _Geopoints.GetDots();
                                Dot3D dot = dots.GetItem(0);
                                double dot_x = dot.X;
                                double dot_y = dot.Y;
                                string str = dot_x.ToString() + "," + dot_y.ToString();
                                point_counts++;
                                //sw.WriteLine(dot_x.ToString());
                                //sw.WriteLine(dot_y.ToString());
                                sw.WriteLine(str);
                                break;
                            }
                        default:
                            break;
                    }
                }
                rtn = _RecordSet.MoveNext();

            }
            sw.Close();
            using (var file = new System.IO.StreamWriter("points_counts.txt"))
            {
                // ... 
                file.WriteLine(point_counts.ToString());
                // rest of code here ... 
            }
            //FileStream fss = new("D:/GIS/GIS二次开发/points_counts.txt", System.IO.FileMode.Create, FileAccess.Write);
            //StreamWriter sw = new(fs);
            //string pc = point_counts.ToString();
            //sw.WriteLine(pc);

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            //string strTest = "该例子测试一个字符串写入文本文件。";
            //System.IO.File.WriteAllText(@"D:\test1.txt", strTest, Encoding.UTF8);
            Process p = new Process();
            //设置要启动的应用程序
            p.StartInfo.FileName = "K-means_PRO.exe";

            ////是否使用操作系统shell启动
            //p.StartInfo.UseShellExecute = false;
            //// bu接受来自调用程序的输入信息
            //p.StartInfo.RedirectStandardInput = false;
            ////输出信息
            //p.StartInfo.RedirectStandardOutput = true;
            //// 输出错误
            //p.StartInfo.RedirectStandardError = true;

            //显示程序窗口
            p.StartInfo.CreateNoWindow = false;
            //启动程序
            p.Start();
            //等待程序执行完退出进程
            p.WaitForExit();
            p.Close();

        }

        public void Buffer(SFeatureCls resource_sfcls, DataBase GDB, string name)
        {
            QueryDef _QueryDef = null;
            Rect rect = new Rect();
            RecordSet _RecordSet = null;
            IGeometry _Geometry = null;
            SpaQueryMode mode = new SpaQueryMode();
            GeoVarLine _GeoVarLine = null;
            GeoLines _GeoLines = null;
            GeoPoints _Geopoints = null;

 


        }
        private void button2_Click(object sender, EventArgs e)
        {

        }
    }
}
