using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.VisualBasic.PowerPacks; //匯入VB向量繪圖功能

namespace UDP塗鴉牆
{
    public partial class Form1 : Form
    {
        UdpClient U;
        Thread th;
        ShapeContainer C;//本機畫布物件
        ShapeContainer D; //遠端畫布物件
        Point stp; //繪圖起點；Point物件本身就是包含x、y的點。
        string p;//筆畫座標字串

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            C = new ShapeContainer();
            this.Controls.Add(C);
            D = new ShapeContainer();
            this.Controls.Add(D);
        }

        private void Listen()
        {
            int Port = Convert.ToInt32(textBox3.Text);
            U = new UdpClient(Port);
            //建立本機資訊
            IPEndPoint EP = new IPEndPoint(IPAddress.Parse("127.0.0.0"), Port);
            while (true)
            {
                byte[] B = U.Receive(ref EP);
                string A = Encoding.Default.GetString(B);
                string[] Q = A.Split('/'); //切割座標點資訊(x,y)
                Point[] R = new Point[Q.Length];//宣告座標點陣列，大小為點座標個數
                for (int i = 0; i < Q.Length; i++)
                {
                    string[] K = Q[i].Split(','); //以","切割X、Y座標(x)、(y)
                    R[i].X = int.Parse(K[0]); //定義第i點X座標
                    R[i].Y = int.Parse(K[1]);
                }
                for (int i = 0; i < Q.Length -1 ; i++) //建立線段物件
                {
                    LineShape L = new LineShape();
                    L.StartPoint = R[i];
                    L.EndPoint = R[i + 1];
                    L.Parent = D;
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                th.Abort();
                U.Close();
            }
            catch
            {
                //忽略錯誤，繼續執行。
            }
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e) //當滑鼠按下
        {
            stp = e.Location; //起點  
            p = stp.X.ToString() + "," + stp.Y.ToString();  //起點座標紀錄
        }
        /// <summary>
        /// 顯示線條於畫布上，並記錄座標點
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left) //如果按下左鍵
            {
                LineShape L = new LineShape();
                L.StartPoint = stp;
                L.EndPoint = e.Location;
                L.Parent = C; //LineShape加入畫布C
                stp = e.Location;
                p += "/" + stp.X.ToString() + "," + stp.Y.ToString(); //持續記錄座標
            }
           
        }
        /// <summary>
        /// 送出繪圖action
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            int Port = int.Parse(textBox2.Text);
            UdpClient S = new UdpClient(textBox1.Text, Port);
            byte[] B = Encoding.Default.GetBytes(p);
            S.Send(B, B.Length);
            S.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            th = new Thread(Listen);
            th.Start();
            button1.Enabled = false;
        }
    }
}
