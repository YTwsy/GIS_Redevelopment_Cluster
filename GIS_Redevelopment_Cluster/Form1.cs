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
                        case GeometryType.Points:
                            {
                                _Geopoints = new GeoPoints();
                                _Geopoints = _Geometry as GeoPoints;

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

        ///// <summary>
        ///// 打开CSV 文件
        ///// </summary>
        ///// <param name="fileName">文件全名</param>
        ///// <param name="firstRow">开始行</param>
        ///// <param name="firstColumn">开始列</param>
        ///// <param name="getRows">获取多少行</param>
        ///// <param name="getColumns">获取多少列</param>
        ///// <param name="haveTitleRow">是有标题行</param>
        ///// <returns>DataTable</returns>
        
        //public static DataTable OpenCSV(string fullFileName, Int16 firstRow = 0, Int16 firstColumn = 0, Int16 getRows = 0, Int16 getColumns = 0, bool haveTitleRow = true)
        //{
        //    DataTable dt = new DataTable();
        //    FileStream fs = new FileStream(fullFileName, System.IO.FileMode.Open, System.IO.FileAccess.Read);
        //    StreamReader sr = new StreamReader(fs, System.Text.Encoding.Default);
        //    //记录每次读取的一行记录
        //    string strLine = "";
        //    //记录每行记录中的各字段内容
        //    string[] aryLine;
        //    //标示列数
        //    int columnCount = 0;
        //    //是否已建立了表的字段
        //    bool bCreateTableColumns = false;
        //    //第几行
        //    int iRow = 1;

        //    //去除无用行
        //    if (firstRow > 0)
        //    {
        //        for (int i = 1; i < firstRow; i++)
        //        {
        //            sr.ReadLine();
        //        }
        //    }

        //    // { ",", ".", "!", "?", ";", ":", " " };
        //    string[] separators = { "," };
        //    //逐行读取CSV中的数据
        //    while ((strLine = sr.ReadLine()) != null)
        //    {
        //        strLine = strLine.Trim();
        //        aryLine = strLine.Split(separators, System.StringSplitOptions.RemoveEmptyEntries);

        //        if (bCreateTableColumns == false)
        //        {
        //            bCreateTableColumns = true;
        //            columnCount = aryLine.Length;
        //            //创建列
        //            for (int i = firstColumn; i < (getColumns == 0 ? columnCount : firstColumn + getColumns); i++)
        //            {
        //                DataColumn dc
        //                    = new DataColumn(haveTitleRow == true ? aryLine[i] : "COL" + i.ToString());
        //                dt.Columns.Add(dc);
        //            }

        //            bCreateTableColumns = true;

        //            if (haveTitleRow == true)
        //            {
        //                continue;
        //            }
        //        }


        //        DataRow dr = dt.NewRow();
        //        for (int j = firstColumn; j < (getColumns == 0 ? columnCount : firstColumn + getColumns); j++)
        //        {
        //            dr[j - firstColumn] = aryLine[j];
        //        }
        //        dt.Rows.Add(dr);

        //        iRow = iRow + 1;
        //        if (getRows > 0)
        //        {
        //            if (iRow > getRows)
        //            {
        //                break;
        //            }
        //        }

        //    }

        //    sr.Close();
        //    fs.Close();
        //    return dt;
        //}
        public void show_cluster_result(DataBase GDB)
        {
            //        public void show_cluster_result(SFeatureCls resource_sfcls, DataBase GDB, string name)

            QueryDef _QueryDef = null;
            Rect rect = new Rect();
            RecordSet _RecordSet = null;
            IGeometry _Geometry = null;
            SpaQueryMode mode = new SpaQueryMode();
            GeoVarLine _GeoVarLine = null;
            GeoLines _GeoLines = null;
            GeoPoints _Geopoints = null;
            IGeometry point_Geometry = null;
            RegInfo _RegInfo = null; //线的图形信息
            PntInfo _pntInfo = null; //点的图形信息
            SFeatureCls pntCls = null;
            long oid = 0;

            //int id = 0;
            ////创建点简单要素类
            //SFeatureCls result_sfcls = null;
            //result_sfcls = new SFeatureCls(GDB);
            //id = result_sfcls.Create(name, GeomType.Reg, 0, 0, null);

            //_Geopoints = new GeoPoints();
            //_Geopoints = _Geometry as GeoPoints;

            //point_Geometry = null;
            //Dots3D dots;
            //Dot3D dot;
            ////point_Geometry = _Geopoints.SetDots(dots);
            ////point_Geometry =_Geopoints.Append(dot);
            //_Geopoints.SetDots(dots);

            //result_sfcls.Append(point_Geometry, _RecordSet.Att, _pntInfo);

            //string sqlTxt = string.Format("SELECT * FROM [GrapInfoDB].[dbo].[Detail_Table_2] WHERE Name = '{0}' AND Type = '{1}'", name, type);
            //SqlCommand maxCmd = new SqlCommand(sqlTxt, sconn);
            //SqlDataReader dr = maxCmd.ExecuteReader();

            //while (dr.Read())
            //{
            //    #region 点
            //    if (type == "点")
            //    {

            using (StreamReader sr = new StreamReader("D:/GIS/GIS二次开发/Debug_dev_0/k - means_point.txt"))
            {
                string line;
                PntInfo pntInfo = null;     //点图形参数信息
                pntInfo = new PntInfo();
                int id = 0;
                //创建面简单要素类
                pntCls = new SFeatureCls(GDB);
                id = pntCls.Create("k-means_result", GeomType.Pnt, 0, 0, null);

                // 从文件读取并显示行，直到文件的末尾 
                while ((line = sr.ReadLine()) != null)
                {
                    Console.WriteLine(line);
                    string[] destStr = line.Split(',');
                    //foreach (string outstr in destStr)
                    //{
                    //    Console.Write(outstr);
                    //    Console.Write(" ");
                    //}
                    Console.WriteLine(destStr[0]);
                    Console.WriteLine(destStr[1]);

                    PntInfo info = new PntInfo();
                    _RegInfo = new RegInfo();
                    //设置面的图形信息
                    info.BackClr = 376;//绿色


                    Dot dot = new Dot(double.Parse(destStr[1]), double.Parse(destStr[2]));
                    Dot3D dot3D = new Dot3D(double.Parse(destStr[1]), double.Parse(destStr[1]), 0);

                    QueryDef qf = new QueryDef();
                    qf.SetNear(dot,5, 5);

                    GeoPoints point = new GeoPoints();
                    point.Append(dot3D);
                    Record rcd = new Record();
                    rcd.Fields = pntCls.Fields;
                    pntCls.Append(point, rcd, pntInfo);



                      
















                    ////string spatialInfos = dr[3].ToString();
                    ////string grapInfos = dr[4].ToString();

                    ////string[] loc = spatialInfos.Split(',');
                    ////Dot dot = new Dot(double.Parse(loc[0]), double.Parse(loc[1]));

                    //PntInfo info = new PntInfo();
                    //_RegInfo = new RegInfo();
                    ////设置面的图形信息
                    //info.BackClr = 376;//绿色
                    ////info.SymID = int.Parse(grapInfos.Split(',')[0]);
                    ////int i = int.Parse(grapInfos.Split(',')[1]);
                    ////info.OutClr = new int[] { i, i, i };
                    ////info.Height = float.Parse(grapInfos.Split(',')[2]);
                    ////info.Width = float.Parse(grapInfos.Split(',')[3]);

                    ////Dot3D dot3D = new Dot3D(dot.X, dot.Y, 0);
                    //Dot3D dot3D = new Dot3D(double.Parse(destStr[1]), double.Parse(destStr[1]), 0);

                    //GeoPoints point = new GeoPoints();
                    //point.Append(dot3D);
                    //Record rcd = new Record();
                    //rcd.Fields = pntCls.Fields;
                    //oid=pntCls.Append(point, rcd, info);

                    //rcd.SetValue("ID", oid);
                    //pntCls.UpdateAtt(oid, rcd);
                    ////}/*end of if (type == "点")*/
                    ////#endregion

                    ////break;
                }


            }



            //判断地图视图中是否有处于显示状态中的地图
            if (this.mapCtrl.ActiveMap == null)
            {
                MessageBox.Show("请先在地图视图中显示一幅地图！！！");
                return;
            }

            this._Tree.WorkSpace.BeginUpdateTree();
            this._Tree.WorkSpace.BeginUpdateTree();
            //附加矢量图层
            VectorLayer vecLayer = new VectorLayer(VectorLayerType.SFclsLayer);
            bool v = vecLayer.AttachData(pntCls);
            //将图层添加到地图中
            vecLayer.Name = pntCls.ClsName;
            //获取激活地图
            Map activeMap = this.mapCtrl.ActiveMap;
            activeMap.Append(vecLayer);
            vecLayer.DetachData();
            //复位
            this.mapCtrl.ActiveMap = activeMap;
            this.mapCtrl.Restore();
            this._Tree.WorkSpace.EndUpdateTree();

        }
        private void button2_Click(object sender, EventArgs e)
        {
            show_cluster_result(GDB);
        }
    }
}
