// GIS_K-means.cpp : 此文件包含 "main" 函数。程序执行将在此处开始并结束。
//

#include <iostream>
#include <fstream>
#include <string>
#include <cstdlib>
#include<vector>
#include <math.h>
#include <algorithm>
#include <functional>
#include<iomanip>
#include<sstream>

//D:/GIS/GIS二次开发/

constexpr auto poi = 800;

using namespace std;

struct point
{
    int id;
    double x;
    double y;
    int class_point;
};

struct distance_to
{
    int id;
    double distance_to_K;
};

class KMeans
{
public:
    int K;
    vector<point>points;
    //800 个 点
    int point_count;
    //KMeans() :points(10000) {};
    vector<vector<point>>every_k_class_with_point;
    vector<point>k_center;

    void set_k(int the_k)
    {
        K = the_k;
        every_k_class_with_point.resize(the_k);
        k_center.resize(the_k);
    }
    void set_point_count(int point_count_will)
    {
        point_count = point_count_will;
        if (point_count > points.max_size())
        {
            cout << point_count << endl;
            cout << "resize too big" << endl;
        }
        points.resize(point_count);
    }

    void first_k();

    void first_k_pro();

    bool change_center();

    void txt_to_vector_point();

    bool kmeans();

    double get_dintance(point a, point b);

    void output();

    void write_to_txt();
};

double KMeans::get_dintance(point a, point b)
{
    return sqrt((a.x - b.x) * (a.x - b.x) + (b.y - a.y) * (b.y - a.y));
}

//void KMeans::txt_to_vector_point(vector<point>& points)   //读文件

void KMeans::txt_to_vector_point()   //读文件
{
    ifstream ifs;
    ifs.open("Pre_cluster_points.txt", ios::in);
    if (!ifs.is_open())
    {
        cout << "Pre_cluster_points.txt文件打开失败" << endl;
        return;        //若是main函数中，需return 0；
    }                              //判断是否打开成功

     //point points[w];

    string buf;
    int w = 0;
    while (getline(ifs, buf) && w < point_count)  //把数据读到buf中
    {
        int lines_length = buf.length();
        for (int i = 0; i < lines_length; i++)
        {
            if (buf[i] == ',')
            {
                string buf_push;
                for (int j = 0; j < i; j++)//j为第几位开始读
                {
                    cout << buf[j];
                    buf_push.push_back(buf[j]);
                }
                points[w].x = atof(buf_push.c_str());

                cout << endl;

                string buf_push_1;
                for (int o = i + 2; o < lines_length; o++)//lines_length为读到第几位
                {
                    cout << buf[o];
                    buf_push_1.push_back(buf[o]);
                }
                points[w].y = atof(buf_push_1.c_str());

                cout << endl;
                break;
            }
        }
        points[w].id = w;
        w++;
    }
    ifs.close(); //最后关闭文件

}

void KMeans::first_k()
{
    for (int i = 0; i < K; i++)
    {
        int first_center_id = rand() % point_count;
        k_center[i].x = points[first_center_id].x;
        k_center[i].y = points[first_center_id].y;
        every_k_class_with_point[i].push_back(points[first_center_id]);
    }
}

bool  kmeans_cmp(distance_to a, distance_to b)
{
    return a.distance_to_K < b.distance_to_K;
}

bool  kmeans_cmp_opposite(distance_to a, distance_to b)
{
    return a.distance_to_K > b.distance_to_K;
}

void KMeans::first_k_pro()
{
    int first_center_id = rand() % point_count;
    k_center[0].x = points[first_center_id].x;
    k_center[0].y = points[first_center_id].y;
    //every_k_class_with_point[0].push_back(points[first_center_id]);
    for (int j = 0; j < K; j++)
    {
        for (int i = 0; i < point_count; i++)
        {
            vector<distance_to>dis(point_count);
            int distance_sum = 0;
            for (int o = 0; o < j; o++)
            {
                distance_sum += get_dintance(KMeans::points[i], k_center[o]);
            }
            dis[i].distance_to_K = distance_sum;
            dis[i].id = i;
            sort(dis.begin(), dis.end(), kmeans_cmp_opposite);

            //以下为调试
            //cout <<KMeans::points.size()<< endl;
            //cout << dis.size() << endl;
            //调试到这里

            k_center[j].x = points[dis[0].id].x;  //out of range?
            k_center[j].y = points[dis[0].id].y;
        }
        every_k_class_with_point[j].push_back(points[first_center_id]);
    }
}


bool KMeans::change_center()
{
    vector<double>center_from_pre(K);
    for (int k_class_i = 0; k_class_i < K; k_class_i++)
    {
        double new_center_x = 0;
        double new_center_y = 0;

        for (int i = 0; i < every_k_class_with_point[k_class_i].size(); i++)
        {
            new_center_x += every_k_class_with_point[k_class_i][i].x;
            new_center_y += every_k_class_with_point[k_class_i][i].y;
        }
        new_center_x = new_center_x / every_k_class_with_point[k_class_i].size();
        new_center_y = new_center_y / every_k_class_with_point[k_class_i].size();

        double center = sqrt((new_center_x - k_center[k_class_i].x) *
            (new_center_x - k_center[k_class_i].x) + (new_center_y - k_center[k_class_i].y) *
            (new_center_y - k_center[k_class_i].y));        //接口准备

        center_from_pre[k_class_i] = center;

        k_center[k_class_i].x = new_center_x;
        k_center[k_class_i].y = new_center_y;
    }

    for (int i = 0; i < K; i++)
    {
        if (center_from_pre[i] != 0)
        {
            return 1;
        }
    }
    return 0;
    //0 false   没有聚类中心再发生变化
    //1 true    有聚类中心发生变化

}

bool KMeans::kmeans()
{
    //KMeans::set_k(5);
    //KMeans::txt_to_vector_point();          //可能bug
    //KMeans::first_k();
    vector<point>pre_points(points);
    for (int i = 0; i < point_count; i++)
    {
        vector<distance_to>dis(K);
        for (int j = 0; j < K; j++)
        {
            dis[j].distance_to_K = get_dintance(KMeans::points[i], k_center[j]);
            dis[j].id = j;
        }
        sort(dis.begin(), dis.end(), kmeans_cmp);

        //记录之前的class，为之后判断；

        //pre_points[i].class_point= points[i].class_point
        points[i].class_point = dis[0].id;
    }
    for (int i = 0; i < point_count; i++)
    {
        for (int k = 0; k < K; k++)
        {
            if (points[i].class_point == k)
            {
                every_k_class_with_point[k].push_back(points[i]);
                break;
            }
        }
    }

    output();

    for (int i = 0; i < point_count; i++)
    {
        if (points[i].class_point != pre_points[i].class_point)
        {
            return 1;
        }
    }
    return 0;
}
//0 false   没有类别再发生变化
//1 true    有类别发生变化

void KMeans::output()
{
    for (int i = 0; i < points.size(); i++)
    {
        cout << points[i].id << "         " << setprecision(16) << points[i].x << "        "
            << setprecision(17) << points[i].y << "       " << points[i].class_point << endl;
    }
}

void KMeans::write_to_txt()
{
    ofstream ofs;

    string K_to_name = "k - means_point.txt";
    //stringstream ss;
    //string str;
    //ss << K;
    //ss >> str;
    //K_to_name = K_to_name + str + "class_.txt";

    ofs.open(K_to_name, ios::out);

    for (int i = 0; i < point_count; i++)
    {
        ofs << points[i].id << "," << setprecision(16) << points[i].x << ","
            << setprecision(17) << points[i].y << "," << points[i].class_point << endl;
    }
    ofs.close();
}


int main()
{
    //KMeans kmeans5;
    //kmeans5.set_k(5);
    //kmeans5.txt_to_vector_point();//可能bug
    //kmeans5.first_k();
    int max_k_means;
    //cout << "输入最大迭代次数" << endl;
    //cin >> max_k_means;

    //for (int i = 0; i < max_k_means; i++)
    //{
    //    cout << "迭代次数" << i + 1 << ":" << endl;

    //    if (kmeans5.kmeans() == 0)
    //    {
    //        cout << "没有类别再发生变化" << endl;
    //        break;
    //    }
    //    if (kmeans5.change_center() == 0)
    //    {
    //        cout << "没有聚类中心再发生变化" << endl;
    //        break;
    //    };
    //}
    //kmeans5.output();
    //kmeans5.write_to_txt();

    //KMeans kmeans7;
    //kmeans7.set_k(7);
    //kmeans7.txt_to_vector_point();//可能bug
    //kmeans7.first_k();
    //cout << "输入最大迭代次数" << endl;
    //cin >> max_k_means;

    //for (int i = 0; i < max_k_means; i++)
    //{
    //    cout << "迭代次数" << i + 1 << ":" << endl;

    //    if (kmeans7.kmeans() == 0)
    //    {
    //        cout << "没有类别再发生变化" << endl;
    //        break;
    //    }
    //    if (kmeans7.change_center() == 0)
    //    {
    //        cout << "没有聚类中心再发生变化" << endl;
    //        break;
    //    };
    //}
    //kmeans7.output();
    //kmeans7.write_to_txt();

    //KMeans kmeans9;
    //kmeans9.set_k(9);
    //kmeans9.txt_to_vector_point();//可能bug
    //kmeans9.first_k();
    //cout << "输入最大迭代次数" << endl;
    //cin >> max_k_means;

    //for (int i = 0; i < max_k_means; i++)
    //{
    //    cout << "迭代次数" << i + 1 << ":" << endl;

    //    if (kmeans9.kmeans() == 0)
    //    {
    //        cout << "没有类别再发生变化" << endl;
    //        break;
    //    }
    //    if (kmeans9.change_center() == 0)
    //    {
    //        cout << "没有聚类中心再发生变化" << endl;
    //        break;
    //    };
    //}
    //kmeans9.output();
    //kmeans9.write_to_txt();


    ifstream ifs;
    ifs.open("k-means_K&max_cluster.txt", ios::in);
    if (!ifs.is_open())
    {
        cout << "k - means_K & max_cluster.txt文件打开失败" << endl;
        return 0;        //若是main函数中，需return 0；
    }                              //判断是否打开成功

     //point points[w];

    string buf;
    getline(ifs, buf);
    int K = 0;
    stringstream stream(buf);
    stream >> K;
    cout << K << endl;
    getline(ifs, buf);
    stringstream streams(buf);
    streams >> max_k_means;
    cout << max_k_means << endl;


    ifs.close(); //最后关闭文件

    ifstream ifss;
    ifss.open("points_counts.txt", ios::in);
    if (!ifss.is_open())
    {
        cout << "points_counts.txt文件打开失败" << endl;
        return 0;        //若是main函数中，需return 0；
    }                              //判断是否打开成功
    getline(ifss, buf);
    int point_counts;
    stringstream streamss(buf);
    streamss >> point_counts;
    cout << point_counts << endl;



    KMeans kmeans9_pro;
    kmeans9_pro.set_point_count(point_counts);
    kmeans9_pro.set_k(K);
    kmeans9_pro.txt_to_vector_point();//可能bug
    kmeans9_pro.first_k_pro();
    //cout << "输入最大迭代次数" << endl;
    //cin >> max_k_means;

    for (int i = 0; i < max_k_means; i++)
    {
        cout << "迭代次数" << i + 1 << ":" << endl;

        if (kmeans9_pro.kmeans() == 0)
        {
            cout << "没有类别再发生变化" << endl;
            break;
        }
        if (kmeans9_pro.change_center() == 0)
        {
            cout << "没有聚类中心再发生变化" << endl;
            break;
        };
    }
    kmeans9_pro.output();
    kmeans9_pro.write_to_txt();

}