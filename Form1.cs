using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.Text.RegularExpressions;
using System.Linq;
using System.IO;
using System.Threading;
using System.Net;

namespace Pixivworm
{
    
    public partial class Form1 : Form
    {
       // public HttpToolClass httptoll = new HttpToolClass();

        public static Thread Thread1;//线程的声明辅助线程

        public delegate void UpdateView(prop x);//委托的申明

        public UpdateView UpdateV, errordanger;

        public struct prop
        {
            public int pro1max;

            public int pro1value;

            public int pro2max;

            public int pro2value;

            public string text;
        }

        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// morelist 多图图片下载地址url合集
        /// </summary>
        ArrayList morelist = new ArrayList();

        /// <summary>
        /// orglist 单张图片下载地址url合集
        /// </summary>
        ArrayList orglist = new ArrayList();

        /// <summary>
        /// giflist 动态图片zip下载地址url合集
        /// </summary>
        ArrayList giflist = new ArrayList();

        /// <summary>
        /// morelisterr 多图图片下载地址url失败合集
        /// </summary>
        ArrayList morelisterr = new ArrayList();

        /// <summary>
        /// orglisterr 单张图片下载地址url失败合集
        /// </summary>
        ArrayList orglisterr = new ArrayList();

        /// <summary>
        /// giflisterr 动态图片zip下载地址url失败合集
        /// </summary>
        ArrayList giflisterr = new ArrayList();

        /// <summary>
        /// morelistname 多图图片 图片名称合集
        /// </summary>
        ArrayList morelistname = new ArrayList();

        /// <summary>
        /// orglistname 单张图片 图片名称合集
        /// </summary>
        ArrayList orglistname = new ArrayList();

        /// <summary>
        /// gifname 动态图片zip 图片名称合集
        /// </summary>
        ArrayList gifname = new ArrayList();

        /// <summary>
        /// 头文件信息
        /// </summary>
        HttpHeader header = new HttpHeader();

        /// <summary>
        /// 用来更新UI的结构体
        /// 包括progress1的最大值,当前值
        /// 包括textbox的当前信息的显示
        /// </summary>
        prop pr = new prop();

        /*-正则相关初始化-*/
        Regex reg;
        Match match;

        /*-lock锁定义-*/
        Object locker = new Object();

        /*-线程池中单独线程状态计数-*/
        /// <summary>
        /// 线程池中单独分析图片url线程状态计数
        /// </summary>
        CountdownEvent handler;

        /// <summary>
        /// 线程池中图片下载url线程状态计数
        /// </summary>
        CountdownEvent handler2img;

        /*-初始化-*/
        /// <summary>
        /// P站作者ID
        /// </summary>
        public String textb1 = "";

        /// <summary>
        /// 文件保存地址
        /// </summary>
        public String textblogin = "";

        /// <summary>
        ///  作者ID图文件保存地址
        /// </summary>
        public String savename = "NOName";

        /// <summary>
        /// 登录ID
        /// </summary>
        public String loginID = "";

        /// <summary>
        /// 登录密码
        /// </summary>
        public String loginpassword = "";

        /// <summary>
        /// 工作线程
        /// </summary>
        public int workerthread = 5;

        /// <summary>
        /// IO线程
        /// </summary>
        public int iothread = 5;

        /// <summary>
        /// 超时
        /// </summary>
     //   public int timeout = 30000;

        /// <summary>
        /// 是否自动下载
        /// </summary>
           public int autodownload = 0;

        /// <summary>
        /// 重试次数
        /// </summary>
        public int retry = 5;

        /// <summary>
        /// 去掉文件夹中的非法字符
        /// </summary>
        /// <param name="filename">去除前filename</param>
        /// <returns>去除后filename</returns>
        private string filename(string filename)
        {
            string str = filename;
            str = str.Replace("\\", string.Empty);
            str = str.Replace("/", string.Empty);
            str = str.Replace(":", string.Empty);
            str = str.Replace("*", string.Empty);
            str = str.Replace("?", string.Empty);
            str = str.Replace("\"", string.Empty);
            str = str.Replace("<", string.Empty);
            str = str.Replace(">", string.Empty);
            str = str.Replace("|", string.Empty);
            str = str.Replace(" ", string.Empty);

            return str;
        }

        /// <summary>
        /// 正则表达式 返回解析后的match
        /// 减少代码量
        /// </summary>
        /// <param name="regex">正则表达式</param>
        /// <param name="str">需要解析的string</param>
        /// <returns>解析后的match</returns>
        private Match regexmatch(String regex,String str)
        {
            Regex reg = new Regex(regex);

            Match match = reg.Match(Convert.ToString(str).ToString());

            return match;
        }

        /// <summary>
        /// 判断每个作品的作品类型
        /// 单张大图
        /// 多张
        /// 动态
        /// </summary>
        /// <param name="piclist">每个作品的作品类型url合集中的位置[] 由object转int arraylist[x]</param>
        private void httpgetpageimg(object piclist)
        { 
            bool getloop=false;

            int strp = 0;
            do{
                try
                {               
                    /*----------判断是否为静态图-----------*/
                    HttpToolClass httptoll = new HttpToolClass();

                    ArrayList must2 = httptoll.sqlliistGet("http://www.pixiv.net" + piclist.ToString(), "", header);

                    Match orgpicmatch = regexmatch(@"img\salt=""(.*?)""(.*?)data-src=""(.*?)""(.*?)>", Convert.ToString(must2[0]));

                    if (orgpicmatch.Groups[0].Value == "")
                    {
                        /*----------判断是否为多图-----------*/

                        string morlistfilename = regexmatch(@"</ul><h1\sclass=""title"">(.*?)</h1><", Convert.ToString(must2[0]).ToString()).Groups[1].ToString();

                        Match matchmorpic = regexmatch(@"mode=manga(.*?)""", Convert.ToString(must2[0]).ToString());

                        string urllist = matchmorpic.Groups[0].Value.Replace(@"""", "");

                        if (urllist != "")
                        {
                            urllist = urllist.Replace(@"amp;", "");

                            ArrayList morepiclist = new ArrayList();

                            morepiclist.Add(httptoll.sqlliistGet("http://www.pixiv.net/member_illust.php?" + urllist, "", header)[0].ToString());

                            int lop = 0;

                            Match match = regexmatch(@"data-filter=""(.*?)""\sdata-src=""(.*?)""", Convert.ToString(morepiclist[0]).ToString());

                            if (match.Success)
                            {
                                match = regexmatch(@"</script><a\shref=""(.*?)"" target=""(.*?)"" class=""(.*?)>", Convert.ToString(morepiclist[0]).ToString());


                                while (match.Success)
                                {


                                    urllist = match.Groups[1].Value.Replace(@"""", "");

                                    urllist = urllist.Replace(@"amp;", "");

                                    ArrayList morepiclistorg = new ArrayList();

                                    morepiclistorg.Add(httptoll.sqlliistGet("http://www.pixiv.net" + urllist, "", header)[0].ToString());

                                    Match match2 = regexmatch(@"<body><img src=""(.*?)""", Convert.ToString(morepiclistorg[0]).ToString());

                                    if (match2.Success)
                                    {
                                        lock (locker)
                                        {
                                            morelist.Add(match2.Groups[1].ToString());

                                            morelistname.Add(morlistfilename + "\\" + match2.Groups[1].ToString() + lop);

                                            lop++;
                                        }
                                        pr.text = pr.text + "\r\n" + match2.Groups[1].ToString();

                                        this.BeginInvoke(UpdateV, pr);

                                    }
                                    else
                                    {
                                        this.BeginInvoke(new MethodInvoker(delegate
                                        {
                                            MessageBox.Show("链接：http://www.pixiv.net" + urllist + "无法解析");
                                        }));
                                    }


                                    match = match.NextMatch();




                                }
                            }
                            else
                            {
                                /*----------尝试配置漫画模式多图-----------*/
                                match = regexmatch(@"pixiv.context.originalImages(.*?) = ""(.*?)""", Convert.ToString(morepiclist[0]).ToString());

                                if (match.Success)
                                {
                                    while (match.Success)
                                    {
                                        lock (locker)
                                        {
                                            string comicopicurl = match.Groups[2].ToString().Replace(@"/", "");

                                            comicopicurl = comicopicurl.Replace("\\", "/");

                                            morelist.Add(comicopicurl);

                                            morelistname.Add(morlistfilename + "\\" + comicopicurl + lop);
                                        }
                                        match = match.NextMatch();

                                        lop++;


                                        pr.text = pr.text + "\r\n" + match.Groups[2].ToString();

                                        this.BeginInvoke(UpdateV, pr);
                                    }
                                }
                                else
                                {
                                    this.BeginInvoke(new MethodInvoker(delegate
                                    {
                                        MessageBox.Show("链接：http://www.pixiv.net" + piclist.ToString() + "无法解析");
                                    }));
                                }

                            }
                        }
                        else
                        {
                            /*----------判断是否为动态图-----------*/
                            if (regexmatch(@"ugoku-illust-player-container", Convert.ToString(must2[0]).ToString()).Groups[0].Value != "")
                            {
                                lock (locker)
                                {
                                    giflist.Add(regexmatch(@"ugokuIllustFullscreenData\s\s=\s{""src"":""(.*?)"",", Convert.ToString(must2[0]).ToString().Replace(@"\", "")).Groups[1].Value);

                                    gifname.Add(morlistfilename + "\\");
                                }
                            }
                            else
                            {
                                /*----------判断是否为big图-----------*/
                                string bigpic = regexmatch(@"mode=big(.*?)""", Convert.ToString(must2[0]).ToString()).Groups[0].Value;
                                if (bigpic != "")
                                {
                                    lock (locker)
                                    {
                                        CookieClass cookieClass = CookieClass.Instance;

                                        bigpic = bigpic.Replace(@"amp;", "");

                                        bigpic = bigpic.Replace(@"""", "");

                                        ArrayList morepiclist = new ArrayList();

                                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://www.pixiv.net/member_illust.php?" + bigpic);

                                        ArrayList arrayListpostmid = new ArrayList();

                                        request.CookieContainer = cookieClass.cookie;

                                        request.Method = "GET";
                                        request.ContentType = header.contentType;
                                        request.ServicePoint.ConnectionLimit = header.maxTry;
                                        request.Accept = header.accept;
                                        request.UserAgent = header.userAgent;

                                        request.Headers.Add("Accept-Language", "zh-CN,zh;q=0.8");

                                        bigpic = bigpic.Replace("big", "medium");

                                        request.Referer = "http://www.pixiv.net/member_illust.php?" + bigpic;

                                        HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                                        Stream myResponseStream = response.GetResponseStream();

                                        StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));

                                        string retString = myStreamReader.ReadToEnd();

                                        myStreamReader.Close();

                                        myResponseStream.Close();

                                        //  morepiclist.Add(httptoll.sqlliistGet("http://www.pixiv.net/member_illust.php?" + bigpic.Replace(@"""", ""), "", header)[0].ToString());

                                        morepiclist.Add(retString);

                                        String bigurl = regexmatch(@"img\ssrc=""(.*?)""", Convert.ToString(morepiclist[0]).ToString()).Groups[1].Value;

                                        if (bigurl != "")
                                        {
                                            string value11 = bigurl;

                                            pr.text = value11;

                                            this.BeginInvoke(UpdateV, pr);

                                            // lock (locker)
                                            // {
                                            orglist.Add(value11);

                                            orglistname.Add(morlistfilename);
                                            //  }  
                                        }
                                        else
                                        {
                                            this.BeginInvoke(new MethodInvoker(delegate
                                            {
                                                MessageBox.Show("链接：http://www.pixiv.net" + piclist.ToString() + "无法解析");
                                            }));
                                        }
                                    }
                                }
                                else
                                {
                                    this.BeginInvoke(new MethodInvoker(delegate
                                    {
                                        MessageBox.Show("链接：http://www.pixiv.net" + piclist.ToString() + "无法解析");
                                    }));
                                }
                            }
                        }
                    }
                    else
                    {
                        string value11 = orgpicmatch.Groups[3].Value;

                        pr.text = value11;

                        this.BeginInvoke(UpdateV, pr);

                        lock (locker)
                        {
                            orglist.Add(value11);

                            orglistname.Add(orgpicmatch.Groups[1].Value);
                        }
                    }
                    pr.pro1value++;

                    pr.text = "";

                    this.BeginInvoke(UpdateV, pr);

                    handler2img.Signal();

                    getloop = false;
                }
                catch(Exception er)
                {
                    
                    getloop = true;

                    if (strp > retry)
                    {
                        pr.pro1value++;

                        getloop = false;

                        pr.text = "该链接解析失败："+"http://www.pixiv.net" + piclist.ToString();

                        this.BeginInvoke(UpdateV, pr);

                    }
                        strp++;

                }
            } while (getloop);
        }



        /// <summary>
        /// 获取PIVIX用户作品网页内容
        /// 获取每个作品的URL
        /// </summary>
        /// <param name="i">每个作品的地址url合集中的位置[] 由object转int arraylist[x]</param>
        private void httpgetpage(object i)
        {
            HttpToolClass httptoll = new HttpToolClass();
           
            // lock(locker)
           // {              
                pr.pro1value++;
               
                pr.text = "网页分析" + i;

                this.BeginInvoke(UpdateV, pr);
           // }
                       
            /*-获取用户ID作品页面合集中每个作品的单独的url地址
            * @"""/member_illust.php\?mode=medium(.*?)"""
            * -*/                
            ArrayList must2 = httptoll.sqlliistGet("http://www.pixiv.net/member_illust.php?id=" + textb1 + "&type=all&p=" + i.ToString(), "", header);
            
            Match match = regexmatch(@"""/member_illust.php\?mode=medium(.*?)""", Convert.ToString(must2[0]).ToString());
                     
            ArrayList pic = new ArrayList();
           
            while (match.Success)
            {
                if (!pic.Contains(match.Groups[0].ToString().Replace(@"amp;", "")) && (match.Groups[0].ToString().Replace(@"amp;", "")!=""))
                {
                   /* 将获取到的每个作品的独立展示页面url 
                    * 放入arraylist类型变量 pic 中保存
                    * _______*/
                    pic.Add(match.Groups[0].ToString().Replace(@"amp;", ""));
                  
                    //  lock (locker)
                  //  {
                        pr.text = match.Groups[0].ToString().Replace(@"amp;", "");
                        this.BeginInvoke(UpdateV, pr);
                  //  } 
                }
               
                match = match.NextMatch();
            }

          //  lock(locker)
          //  {                
                pr.pro1max = pr.pro1max +pic.Count;
                pr.text = "";
                this.BeginInvoke(UpdateV, pr);
           // }

            /*_判断当前激活线程数目
                * ThreadPool.GetMaxThreads
                * 
                * 增加线程池线程数量
                * SetMaxThreads
                * 
                * 给新增加线程加上计数标记，判断放入线程池中线程是否完成
                * handler2img.AddCount
                * 
                * 将获取到的每个作品的独立展示页面url （arraylist pic 集合）
                * 放入方法httpgetpageimg中得到每个作品的下载地址url
                *
                *将httpgetpageimg 方法放入线程池
                * _______*/
          //  int out1 = 0;

         //   int out2 = 0;

         //   ThreadPool.GetMaxThreads(out out1,out out2);

           // if (out1 + pic.Count > workerthread)
          //  {
          //      ThreadPool.SetMaxThreads(workerthread, iothread);
          //  }
          //  else
          //  {
           //     ThreadPool.SetMaxThreads(out1 + (pic.Count/2), out2 + (pic.Count/2));
         //   }
          

           // lock (locker)
          //  {
               // ThreadPool.GetMaxThreads(out out1, out out2);

              //  if (handler2img == null)
               // {
                    handler2img = new CountdownEvent(pic.Count);
               // }
              //  else
               // {
                //    handler2img.AddCount(pic.Count);
               // }
         //   }
            for (int x = 0; x < pic.Count; x++)
            { 
                string piclist = pic[x].ToString().Replace(@"\;", "");
                
                piclist = piclist.Replace(@"""", "");

                ThreadPool.QueueUserWorkItem(new WaitCallback(httpgetpageimg), piclist);                  
            }

          

            /*本线程完成计数*/
            handler.Signal();
        }


        /// <summary>
        /// 从多图url下载地址中下载多图图片并保存
        /// </summary>
        /// <param name="x">多图图片地址url合集中的位置[] 由object转int arraylist[x]</param>
        private void httpmorelistdown(object x)
        {
           // lock (locker)
          //  {
               

                int l = Convert.ToInt32(x);

                pr.text = "正在下载" + morelist[l].ToString();

                this.BeginInvoke(UpdateV, pr);

                string rPath = "";

             //   Stream pictrue = null;

                Stream outStream = null;

                // Stream inStream = null;
                HttpWebResponse pictrue = null;
                try
                {
                    /*-获取多图下载地址进行下载
                     * 
                     * 从流中得到Image类型
                     * 
                     * -*/
                    int dowhile = 0;

                    /*-创建多图文件夹的默认文件夹名称
                            * piccollectionfilename
                            * -*/
                    string piccollectionfilename = "Collection";

                    do
                    {
                        try
                        {
                           
                          //  pictrue = null;

                        //    httptoll.sqlliistGetimag(morelist[l].ToString(), "", header, ref pictrue);

                            ///  Image imagbox1 = Image.FromStream(pictrue);

                            string p = morelist[l].ToString();

                            /*-获取图片扩展名
                             * pickuoz
                             * -*/
                            int pickuoz = p.LastIndexOf(".");

                            string pickuo = p.Substring(pickuoz);

                            /*-获取图片名（包含了多图文件夹的名称以//分割）
                             * rPath
                             * -*/
                            rPath = morelistname[l].ToString();

                           

                            /*-从rPath中获取以//分割的多图文件夹名称（//符号之前的string）
                             * piccollectionfilename
                             * -*/
                            piccollectionfilename = regexmatch(@"(.*?)\\", rPath).Groups[1].Value.Replace(".", "");

                            /*-从rPath中获取图片名称 去掉了名称中包含的url信息
                             * rPath
                             * 
                             * 加上文件后缀名pickuo
                             * 
                             * rPath+pickuo
                             * -*/
                            rPath = filename(regexmatch(@"img/(.*?)\.", rPath).Groups[0].Value.Replace(".", "") + pickuo);

                            /*-去除不合法的文件名字符
                             * -*/
                            piccollectionfilename = filename(piccollectionfilename);

                            /*-创建储存地址
                             * -*/
                            if (!Directory.Exists(textblogin + "\\" + savename + "\\" + piccollectionfilename + "\\"))
                            {
                                Directory.CreateDirectory(textblogin + "\\" + savename + "\\" + piccollectionfilename + "\\");//不存在就创建目录 
                            }

                            /*-创建文件名称 -*/

                            lock (locker)
                            {
                                /*--从流中保存图片到本地-*/
                                if (File.Exists(textblogin + "\\" + savename + "\\" + piccollectionfilename + "\\" + rPath))
                                {
                                    /*-创建文件名称重复的话在图片名称前加上string lop
                                        * -*/
                                    int lop = 0;
                                    while (File.Exists(textblogin + "\\" + savename + "\\" + piccollectionfilename + "\\" + (lop.ToString()) + rPath))
                                    {
                                        lop++;
                                    }

                                    rPath = (lop.ToString()) + rPath;
                                }
                                //  lock (locker)
                                //  {
                                /*-缓存-*/

                                outStream = File.Create(textblogin + "\\" + savename + "\\" + piccollectionfilename + "\\" + rPath);
                            }
                            //  inStream = pictrue2;  

                            /*-每次获取到的流长度-*/     
                            int ll = 0;
                            /*-获取到的流总共长度-*/     
                            long filestream = 0;
                            /*-该文件应该的长度-*/   
                            long picrespose = 0;

                            int picresposeend = 0;

                            do
                            {
                                HttpToolClass httptoll = new HttpToolClass();

                                pictrue = null;

                                HttpHeader header = new HttpHeader();

                                /*-从原始数据的点开始获取数据，用于数据不完整时多次获取-*/   
                                header.Length = (int)filestream;

                                header.Lengthend = picresposeend;

                                /*-获取该图片链接返回数据-*/   
                                httptoll.sqlliistGetimag(morelist[l].ToString(), "", header, ref pictrue);

                                Stream inStream = null;

                                inStream = pictrue.GetResponseStream();

                                byte[] buffer = new byte[40960];
                                if (filestream == 0)
                                {
                                    picrespose = pictrue.ContentLength;


                                    if (picrespose > 0)
                                    {
                                        picresposeend = (int)(picrespose - 1);
                                    }
                                    else
                                    {
                                        picresposeend = 0;
                                    }
                                }




                              //  filestream = 0;

                                do
                                {                                 

                                    ll = inStream.Read(buffer, 0, buffer.Length);
                                  //  ll = pictrue.GetResponseStream().Read(buffer, 0, buffer.Length);

                                    if (ll > 0)
                                    {
                                        outStream.Write(buffer, 0, ll);

                                        filestream = filestream + ll;
                                    }                                   

                                } while (ll > 0);

                                if (inStream != null)
                                {
                                    inStream.Close();
                                }

                            } while (filestream != picrespose);   /*-判断该次获取的数据长度是否完整，不是的话继续获取剩余数据-*/    
                            
                            
                            //inStream = pictrue;


                         /*   int ll;
                            do
                            {
                                ll = pictrue.GetResponseStream().Read(buffer, 0, buffer.Length);

                                if (ll > 0)
                                {
                                    outStream.Write(buffer, 0, ll);
                                }
                            } while (ll > 0);*/
                            //     }
                            // lock (locker)
                            //  {
                            pr.text = "下载多图ing请稍等" + rPath;

                            pr.pro1value++;

                            this.BeginInvoke(UpdateV, pr);

                            dowhile = retry;
                            //  }
                        }
                        catch (Exception er)
                        {
                            if (outStream != null)
                            {
                                outStream.Close();
                            }
                            if (pictrue != null)
                            {
                                pictrue.Close();
                            }
                            File.Delete(textblogin + "\\" + savename + "\\" + piccollectionfilename + "\\" + rPath);
                            if (dowhile >= retry)
                            {
                                throw new Exception(" " + er.Message);
                            }
                        }

                        dowhile++;

                    } while (dowhile <= retry);

                }
                catch (Exception er)
                {
                    //  lock (locker)
                    //  {    
                  morelisterr.Add(l);
                    pr.text = "该链接出错" + er;

                    pr.pro1value++;

                    this.BeginInvoke(UpdateV, pr);
                    //   }
                }
                finally
                {
                    /*--关闭流-*/
                    if (outStream != null)
                    {
                        outStream.Close();
                    }

                    /*  if (inStream != null)
                      {
                          inStream.Close();
                      }*/

                    if (pictrue != null)
                    {
                       

                        pictrue.Close();
                    }

                    ///    imagbox1.Dispose();               
                }
                handler.Signal();
                this.BeginInvoke(new MethodInvoker(delegate
                {
                    label5.Text = "当前状态:正在下载 剩余 " + handler.CurrentCount + "/" + handler.InitialCount;

                }));

              
          //  }
        }

        /// <summary>
        /// 从单张大图url下载地址中下载图片并保存
        /// </summary>
        /// <param name="i">单张大图地址url合集中的位置[] 由object转int arraylist[x]</param>
        private void httporglistdown(object i)
        {
          //  lock (locker)
           // {
               

                int l = Convert.ToInt32(i);

                pr.text = "正在下载" + orglist[l].ToString();

                this.BeginInvoke(UpdateV, pr);

                string rPath = "";

               // Stream pictrue2 = null;

                FileStream outStream = null;

                HttpWebResponse pictrue2 = null;

                // Stream inStream = null;

                try
                {
                    int dowhile = 0;

                    do
                    {
                        try
                        {
                           
                            // Image imagbox2 = Image.FromStream(pictrue2);

                            string p = orglist[l].ToString();

                            int pickuoz = p.LastIndexOf(".");

                            string pickuo = p.Substring(pickuoz);

                            rPath = filename(orglistname[l].ToString() + pickuo);

                            if (!Directory.Exists(textblogin + "\\" + savename + "\\"))
                            {
                                Directory.CreateDirectory(textblogin + "\\" + savename + "\\");//不存在就创建目录 
                            }

                            /*--从流中保存图片到本地-*/
                            lock (locker)
                            {
                                if (File.Exists(textblogin + "\\" + savename + "\\" + rPath))
                                {
                                    /*-创建文件名称重复的话在图片名称前加上string lop
                                        * -*/
                                    int lop = 0;

                                    while (File.Exists(textblogin + "\\" + savename + "\\" + lop.ToString() + rPath))
                                    {
                                        // rPath = (lop++.ToString()) + rPath;
                                        lop++;
                                    }
                                    rPath = (lop.ToString()) + rPath;

                                }

                                //  lock (locker)
                                //  {
                                /*-缓存-*/

                                outStream = File.Create(textblogin + "\\" + savename + "\\" + rPath);
                            }
                            //  inStream = pictrue2;  
                            int ll = 0;

                            int picresposeend = 0;

                            long filestream = 0;

                            long picrespose = 0;



                            do{
                                HttpToolClass httptoll = new HttpToolClass();

                                pictrue2 = null;
                                
                                HttpHeader header=new HttpHeader();
                                
                                
                                header.Length =(int) filestream;

                                header.Lengthend = picresposeend;
                                
                                httptoll.sqlliistGetimag(orglist[l].ToString(), "", header, ref pictrue2);

                                Stream inStream = null;

                                inStream = pictrue2.GetResponseStream();

                                byte[] buffer = new byte[40960];
                                if (filestream == 0)
                                {
                                    picrespose = pictrue2.ContentLength;


                                    if (picrespose > 0)
                                    {
                                        picresposeend = (int)(picrespose - 1);
                                    }
                                    else
                                    {
                                        picresposeend = 0;
                                    }
                                }
                               // filestream = 0;
                                
                               
                                do
                                { 
                                    ll = inStream.Read(buffer, 0, buffer.Length);
                                    
                                    if (ll > 0)
                                    {                                   
                                        outStream.Write(buffer, 0, ll);

                                        filestream = filestream + ll;
                                    }
                                    //Thread.Sleep(1);

                                } while (ll > 0);

                                if (inStream != null)
                                {
                                    inStream.Close();
                                }
                                if (filestream > picrespose)
                                {
                                    throw new Exception("下载大小不正确");
                                }



                            } while ((filestream < picrespose) );
                            
                            pr.text = "下载ing请稍等" + rPath;

                            pr.pro1value++;

                            this.BeginInvoke(UpdateV, pr);

                            dowhile = retry;
                            

                        }
                        catch (Exception er)
                        {                          
                            if (outStream != null)
                            {
                                outStream.Close();
                            }
                            if (pictrue2 != null)
                            { 
                                pictrue2.Close();
                            }
                            File.Delete(textblogin + "\\" + savename + "\\" + rPath);

                            if (dowhile >= retry)
                            {
                                throw new Exception(" " + er.Message);
                            }
                        }

                        dowhile++;

                    } while (dowhile <= retry);
                }
                catch (Exception er)
                {
                    //  lock (locker)
                    //  {
                    orglisterr.Add(l);

                    pr.text = "该链接出错" + er;

                    pr.pro1value++;

                    this.BeginInvoke(UpdateV, pr);
                    //  }
                }
                finally
                {
                    /*--关闭流-*/
                    if (outStream != null)
                    {
                        outStream.Close();
                    }

                    /*  if (inStream != null)
                      {
                          inStream.Close();
                      }*/

                    if (pictrue2 != null)
                    {
                       // pictrue2.Flush();

                        pictrue2.Close();
                    }
                }
                handler.Signal();
                this.BeginInvoke(new MethodInvoker(delegate
                {
                    label5.Text = "当前状态:正在下载 剩余 " + handler.CurrentCount + "/" + handler.InitialCount;

                }));
         //   }

        }
        /// <summary>
        /// 从动态图url下载地址中下载zip并保存
        /// </summary>
        /// <param name="x">动态图地址url合集中的位置[] 由object转int arraylist[x]</param>
        private void httpgiflistdown(object x)
        {          

            int l = Convert.ToInt32(x);

            pr.text = "正在下载" + giflist[l].ToString();

            this.BeginInvoke(UpdateV, pr);

            string rPath = "";

            try
            {
                int dowhile = 0;

                Stream stream = null;

                HttpWebResponse zip = null;

                string path = "";

                do
                {                   
                    try
                    {
                        string p = giflist[l].ToString();

                        int pickuoz = p.LastIndexOf(".");

                        string pickuo = p.Substring(pickuoz);

                        rPath = gifname[l].ToString();

                        string piccollectionfilename = "GIFCollection";                

                        piccollectionfilename = regexmatch(@"(.*?)\\", rPath).Groups[1].Value.Replace(".", "");                

                        rPath = filename(regexmatch(@"img/(.*?)\.", rPath).Groups[0].Value.Replace(".", "") + pickuo);

                        piccollectionfilename = "Gif"+filename(piccollectionfilename);

                        if (!Directory.Exists(textblogin + "\\" + savename + "\\" + piccollectionfilename + "\\"))
                        {
                            Directory.CreateDirectory(textblogin + "\\" + savename + "\\" + piccollectionfilename + "\\");//不存在就创建目录 
                        }

                        path = textblogin + "\\" + savename + "\\" + piccollectionfilename + "\\" + piccollectionfilename + rPath;
                       
                        stream = new FileStream(path, FileMode.Create);
                        
                        long filestream = 0;

                         
                        long picrespose = 0;

                        int picresposeend = 0;

                        do{                            

                            HttpToolClass httptoll = new HttpToolClass();
                        
                            zip = null;

                            HttpHeader header = new HttpHeader();

                            header.Length = (int)filestream;

                            header.Lengthend = picresposeend;

                            httptoll.sqlliistGetimag(giflist[l].ToString(), "", header, ref zip);

                            byte[] bArr = new byte[40960];

                            Stream inStream = null;

                            inStream = zip.GetResponseStream();

                            int size = inStream.Read(bArr, 0, bArr.Length);
                                
                          //  int size = zip.GetResponseStream().Read(bArr, 0, (int)bArr.Length);
                            if (filestream == 0)
                            {
                                picrespose = zip.ContentLength;

                                if (picrespose > 0)
                                {
                                    picresposeend = (int)(picrespose - 1);
                                }
                                else
                                {
                                    picresposeend = 0;
                                }
                            }
                          

                            filestream = size;
                                
                            while (size > 0)
                            {
                                stream.Write(bArr, 0, size);                                
                                
                                size = inStream.Read(bArr, 0, bArr.Length);

                               // size = zip.GetResponseStream().Read(bArr, 0, (int)bArr.Length);

                              //  Thread.Sleep(50);

                                filestream = filestream + size;
                            }

                            if (inStream != null)
                            {
                                inStream.Close();
                            }
    

                        } while (filestream != picrespose);

                        stream.Close();
                            
                        zip.Close();
                      
                        pr.text = "下载ing请稍等" + piccollectionfilename + rPath;

                        pr.pro1value++;

                        this.BeginInvoke(UpdateV, pr);

                        dowhile = retry;

                    }
                    catch (Exception er)
                    {
                        if (stream!=null)
                        stream.Close();

                        if (zip != null)
                        zip.Close();

                        File.Delete(path);

                        if (dowhile >= retry)
                        {
                            throw new Exception(" " + er.Message);
                        }
                    }

                    dowhile++;

                } while (dowhile <= retry);
                //  }            
            }
            catch (Exception er)
            {               
                //  lock (locker)                
                //  {
                giflisterr.Add(l);
                pr.text = "该链接出错" + er;

                pr.pro1value++;

                this.BeginInvoke(UpdateV, pr);
                //   }
            }          

            handler.Signal();
            this.BeginInvoke(new MethodInvoker(delegate
            {
                label5.Text = "当前状态:正在下载 剩余 " + handler.CurrentCount + "/" + handler.InitialCount;

            }));
        }

        /// <summary>
        /// 主辅助线程，开始下载流程
        /// </summary>
        private void httpmainloginpost()
        {
            try
            {
                HttpToolClass httptoll = new HttpToolClass();

                String machineCode = "mode=login&pixiv_id=" + loginID + "&pass="+loginpassword+"&source=pc&return_to=/&skip=1";

                pr.text = "登录中...";

                this.BeginInvoke(new MethodInvoker(delegate
                {
                    label5 .Text = "当前状态:登录中...";
                     
                }));

                this.BeginInvoke(UpdateV, pr);

                ArrayList must = httptoll.sqlliist("https://www.pixiv.net/login.php", machineCode, header);

                if (regexmatch("立即注册", must[0].ToString()).Value != "立即注册")
                {
                    pr.text = "登录完成，开始解析";

                    this.BeginInvoke(new MethodInvoker(delegate
                    {
                        label5.Text = "当前状态:开始解析...";

                    }));

                    this.BeginInvoke(UpdateV, pr);

                    ArrayList must2 = httptoll.sqlliistGet("http://www.pixiv.net/member_illust.php?id=" + textb1, "", header);
                     
                    int pagecountall = Convert.ToInt32(regexmatch(@"<span\sclass=""count-badge"">(\d+)(.*?)</span>", Convert.ToString(must2[0]).ToString()).Groups[1].Value);

                    int pagecount =0;

                    if (pagecountall % 20 > 0)
                    {
                        pagecount = (pagecountall / 20) + 1;
                    }
                    else
                    {
                        pagecount = (pagecountall / 20) ;
                    }

                    pr.pro1max = pagecount;

                    pr.pro1value = 0;

                    pr.text = "解析中...";

                    this.BeginInvoke(UpdateV, pr);

                    savename = regexmatch(@"<h1\sclass=""user"">(.*?)</h1>", Convert.ToString(must2[0]).ToString()).Groups[1].ToString();

                    pr.text = "ID作者为：" + savename;

                    savename = textb1+" "+ filename(savename)+" ";

                    this.BeginInvoke(UpdateV, pr);

                   

                    
                    this.BeginInvoke(new MethodInvoker(delegate
                     {
                         HttpWebResponse pictruelogo = null;
                         httptoll.sqlliistGetimag(regexmatch(@"class=""user-link""><img\ssrc=""(.*?)""\salt(.*?)-image""", Convert.ToString(must2[0]).ToString()).Groups[1].ToString(), "", header, ref pictruelogo);

                         pictureBox1.Image = Image.FromStream(pictruelogo.GetResponseStream());
                         label4.Text = savename;

                         if (pictruelogo != null)
                         {
                             // pictruelogo.Flush();

                             pictruelogo.Close();
                         }
                     
                     }));

                    
                    handler = new CountdownEvent(pagecount);

                   // if (pagecount > workerthread)
                  //  {
                  //      ThreadPool.SetMaxThreads(workerthread, iothread);
                  //  }
                  //  else
                 //   {
                    ThreadPool.SetMaxThreads(workerthread, iothread);
                 //   }
                //
                        this.BeginInvoke(new MethodInvoker(delegate
                        {
                            label5.Text = "当前状态:获取下载地址...";

                        }));

                    for (int i = 1; i <= pagecount; i++)
                    {
                        //ThreadPool.QueueUserWorkItem(new WaitCallback(httpgetpage), i);

                        httpgetpage(i);
                        /*-等待所有的获取图片ID的线程结束
                           * 
                           * httpgetpageimg获取图片ID的方法           
             
                           */

                        this.BeginInvoke(new MethodInvoker(delegate
                        {
                            label5.Text = "当前状态:获取下载地址 正在处理 " + i + "/" + pagecount;

                        }));
                        handler2img.Wait();

                    }

                    handler.Wait();


                    this.BeginInvoke(new MethodInvoker(delegate
                    {
                        label5.Text = "当前状态:正在导出下载列表,地址:" + textblogin;

                    }));

                    DateTime currentTime = new DateTime();
                     currentTime = DateTime.Now;

                     String datatimepaith = textblogin + "\\" + savename + currentTime.ToString("yyyy-MM-dd-hh-mm-ss") + ".txt";

                    try
                    {

                        if (!Directory.Exists(textblogin + "\\"))
                        {
                            Directory.CreateDirectory(textblogin + "\\" );//不存在就创建目录 
                        }
                        FileStream fs = new FileStream(datatimepaith, FileMode.Create);

                        StreamWriter sw = new StreamWriter(fs);
                        //开始写入
                        for (int i = 0; i < morelist.Count; i++)
                        {
                            sw.Write(morelist[i].ToString()+ "\r\n");
                        }
                        for (int i = 0; i < orglist.Count; i++)
                        {
                            sw.Write(orglist[i].ToString() + "\r\n");
                        }
                        for (int i = 0; i < giflist.Count; i++)
                        {
                            sw.Write(giflist[i].ToString() + "\r\n");
                        }
                        
                        //清空缓冲区
                        sw.Flush();
                        //关闭流
                        sw.Close();
                        fs.Close();
                    }
                    catch (Exception eer)
                    {
                        MessageBox.Show("保存下载列表失败" + eer);
                        this.BeginInvoke(new MethodInvoker(delegate
                        {
                            textBox2.Text = "完成" + "\r\n" + textBox2.Text;
                            button1.Enabled = true;
                            textBox1.Enabled = true;
                            checkBox1.Enabled = true;
                        }));
                        this.BeginInvoke(new MethodInvoker(delegate
                        {
                            label5.Text = "当前状态:导出下载列表失败";

                        }));

                        return;
                    }


                    if (checkBox1.Checked != true)
                    {

                        this.BeginInvoke(new MethodInvoker(delegate
                        {
                            textBox2.Text = "下载完成" + "\r\n" + textBox2.Text;
                            button1.Enabled = true;
                            textBox1.Enabled = true;
                            checkBox1.Enabled = true;
                        }));
                        this.BeginInvoke(new MethodInvoker(delegate
                        {
                            label5.Text = "当前状态:导出下载列表完成";

                        }));

                        System.Diagnostics.Process.Start(datatimepaith); //打开此文件。
                    }
                    else
                    {

                        int allcollection = morelist.Count + orglist.Count + giflist.Count;

                        pr.pro1max = pr.pro1max + allcollection;

                        pr.text = "分析完成,开始下载";

                        this.BeginInvoke(UpdateV, pr);

                        handler = new CountdownEvent(allcollection);



                        //  ThreadPool.SetMaxThreads(allcollection/2, allcollection/2);
                        if (allcollection > workerthread)
                        {
                            ThreadPool.SetMaxThreads(workerthread, iothread);
                        }
                        else
                        {
                            ThreadPool.SetMaxThreads(allcollection, allcollection);
                        }
                        int maxcount = 0;

                        if (morelist.Count >= orglist.Count)
                        {
                            maxcount = morelist.Count;
                        }
                        else
                        {
                            maxcount = orglist.Count;
                        }

                        if (maxcount < giflist.Count)
                        {
                            maxcount = giflist.Count;
                        }

                        this.BeginInvoke(new MethodInvoker(delegate
                        {
                            label5.Text = "当前状态:正在下载 剩余 " + allcollection + "/" + allcollection;

                        }));

                        for (int l = 0; l < maxcount; l++)
                        {
                            if (l < morelist.Count)
                            {
                                ThreadPool.QueueUserWorkItem(new WaitCallback(httpmorelistdown), l);
                            }
                            if (l < orglist.Count)
                            {
                                ThreadPool.QueueUserWorkItem(new WaitCallback(httporglistdown), l);
                            }
                            if (l < giflist.Count)
                            {
                                ThreadPool.QueueUserWorkItem(new WaitCallback(httpgiflistdown), l);
                            }
                        }

                        handler.Wait();
                        this.BeginInvoke(new MethodInvoker(delegate
                        {
                            label5.Text = "当前状态:下载完成 已处理 " + allcollection  + "/" + allcollection ;

                        }));

                        

                        bool checkgoon = false;

                        do{

                            this.BeginInvoke(new MethodInvoker(delegate
                            {
                                textBox2.Text = "下载完成" + "\r\n" + textBox2.Text;
                                button1.Enabled = true;
                                textBox1.Enabled = true;
                                checkBox1.Enabled = true;
                            }));
                           

                            int aller = giflisterr.Count + orglisterr.Count + morelisterr.Count;

                            
                            if (giflisterr.Count != 0 || orglisterr.Count != 0 || morelisterr.Count != 0)
                            {
                                MessageBoxButtons messButton = MessageBoxButtons.OKCancel;

                                DialogResult dr = MessageBox.Show("有未成功下载链接是否重新下载", "异常", messButton);

                                if (dr == DialogResult.OK)//如果点击“确定”按钮
                                {
                                    this.BeginInvoke(new MethodInvoker(delegate
                                    {
                                        textBox2.Text = "重新下载未成功链接" + "\r\n" + textBox2.Text;
                                        button1.Enabled = false;
                                        textBox1.Enabled = false;
                                        checkBox1.Enabled = false;
                                    }));

                                    pr.pro1max = pr.pro1max + aller;

                                    pr.text = "重新下载未成功链接";

                                    this.BeginInvoke(UpdateV, pr);
                                    this.BeginInvoke(new MethodInvoker(delegate
                                    {
                                        label5.Text = "当前状态:重新下载未成功链接 剩余 "  + aller + "/"  + aller;

                                    }));

                                   
                                    handler = new CountdownEvent(aller);
                                    int moer = morelisterr.Count;
                                    int orer = orglisterr.Count;
                                    int gifer = giflisterr.Count;

                                    ArrayList orgertemporary = new ArrayList();
                                    ArrayList moertemporary = new ArrayList();
                                    ArrayList giftemporary = new ArrayList();

                                    for (int l = 0; l < moer; l++)
                                    {
                                        int i = Convert.ToInt32(morelisterr[l].ToString());
                                        moertemporary.Add(i);

                                    }
                                    for (int l = 0; l < orer; l++)
                                    {

                                        int i = Convert.ToInt32(orglisterr[l].ToString());
                                        orgertemporary.Add(i);

                                    }
                                    for (int l = 0; l < gifer; l++)
                                    {
                                        int i = Convert.ToInt32(giflisterr[l].ToString());
                                        giftemporary.Add(i);

                                    }

                                    orglisterr.Clear();
                                    giflisterr.Clear();
                                    morelisterr.Clear();



                                    for (int l = 0; l < moer; l++)
                                    {

                                        ThreadPool.QueueUserWorkItem(new WaitCallback(httpmorelistdown), moertemporary[l].ToString());

                                    }
                                    for (int l = 0; l < orer; l++)
                                    {

                                        ThreadPool.QueueUserWorkItem(new WaitCallback(httporglistdown), orgertemporary[l].ToString());

                                    }
                                    for (int l = 0; l < gifer; l++)
                                    {

                                        ThreadPool.QueueUserWorkItem(new WaitCallback(httpgiflistdown), giftemporary[l].ToString());

                                    }

                                    handler.Wait();

                                    checkgoon = true;

                                }
                                else//如果点击“取消”按钮
                                {
                                    checkgoon = false;
                                    try
                                    {
                                        FileStream fs = new FileStream("loger.txt", FileMode.Create);

                                        StreamWriter sw = new StreamWriter(fs);

                                        //开始写入
                                        for (int i = 0; i < morelisterr.Count; i++)
                                        {
                                            sw.Write(morelisterr[i].ToString() + "\r\n");
                                        }
                                        for (int i = 0; i < orglisterr.Count; i++)
                                        {
                                            sw.Write(orglisterr[i].ToString() + "\r\n");
                                        }
                                        for (int i = 0; i < giflisterr.Count; i++)
                                        {
                                            sw.Write(giflisterr[i].ToString() + "\r\n");
                                        }
                                        //清空缓冲区
                                        sw.Flush();
                                        //关闭流
                                        sw.Close();
                                        fs.Close();
                                    }
                                    catch (Exception eer)
                                    {
                                        MessageBox.Show("打开故障记录失败" + eer);

                                        return;
                                    }
                                    this.BeginInvoke(new MethodInvoker(delegate
                                    {
                                        textBox2.Text = "下载完成" + "\r\n" + textBox2.Text;
                                        button1.Enabled = true;
                                        textBox1.Enabled = true;
                                        checkBox1.Enabled = true;
                                    }));
                                    this.BeginInvoke(new MethodInvoker(delegate
                                    {
                                        label5.Text = "当前状态:下载完成 已处理 " + (allcollection + aller).ToString() + "/" + (allcollection + aller).ToString();

                                    }));
                                    string path = @"loger.txt";  //
                                    System.Diagnostics.Process.Start(path); //打开此文件。


                                }
                            }
                            else
                            {
                                this.BeginInvoke(new MethodInvoker(delegate
                                {
                                    label5.Text = "当前状态:下载完成 已处理 " + (allcollection + aller).ToString() + "/" + (allcollection + aller).ToString();

                                }));

                                checkgoon = false;
                            }
                        } while (checkgoon);


                    }
                    //handler2img.Wait();





                }
                else
                {
                    this.BeginInvoke(new MethodInvoker(delegate
                    {
                        textBox2.Text = "登录出错，登录ID不可用" + "\r\n" + textBox2.Text;
                        button1.Enabled = true;
                        textBox1.Enabled = true;
                        checkBox1.Enabled = true;
                    }));
                    this.BeginInvoke(new MethodInvoker(delegate
                    {
                        label5.Text = "当前状态:登录出错";

                    }));

                }

            }
            catch (Exception er)
            {
                MessageBox.Show(""+er);

                this.BeginInvoke(new MethodInvoker(delegate
                {
                    textBox2.Text = "总之有个错误冒出来了,下载可能失败" + "\r\n" + textBox2.Text;
                    button1.Enabled = true;
                    textBox1.Enabled = true;
                    checkBox1.Enabled = true;
                }));

            }

        }

        /// <summary>
        /// 委托调用的方法
        /// 更新UI状态
        /// 更新textbox
        /// textbox上限20条信息
        /// 更新progress
        /// </summary>
        /// <param name="x">传入pr结构体</param>
        private void updateview(prop x)
        {
           // lock (locker)
           // {
                progressBar1.Maximum = x.pro1max;

                progressBar1.Value = x.pro1value;

                //progressBar2.Maximum = x.pro2max;
                // progressBar2.Value = x.pro2value;

                textBox2.Text = x.text + "\r\n" + textBox2.Text;

                if (textBox2.Lines.Length > 20)
                {
                    string[] newlines = new string[20];

                    Array.Copy(textBox2.Lines,0, newlines, 0, 20);

                    this.textBox2.Lines = newlines;
                }
            //}

        }
         
  
        /// <summary>
        /// 点击开始 分析ID号
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                /*-初始化变量和控件状态
                 * 
                 * 控件状态置为false
                 * -*/
                textBox2.Text = "";

                morelist = new ArrayList();

                orglist = new ArrayList();

                giflist = new ArrayList();

                gifname = new ArrayList();

                morelistname = new ArrayList();

                orglistname = new ArrayList();

                morelisterr = new ArrayList();

                orglisterr = new ArrayList();

                giflisterr = new ArrayList();

                handler2img = null;

                textBox1.Enabled = false;

                textb1 = textBox1.Text.Replace(" ","");

                if (textb1 == "")
                {
                    MessageBox.Show("输入不可为空");
                    textBox1.Enabled = true;
                    return;
                }

                string pattern = @"^\d*$";                

                /*-判断输入ID是否合法-*/
                if (!Regex.IsMatch(textb1, pattern)) //判断是否可以转换为整型
                {
                    MessageBox.Show("ID不合法");
                    textBox1.Enabled = true;
                    return;
                }
                else
                {
                    textb1= Convert.ToInt32(textb1).ToString();

                    pr.text = "读取P站ID:" + textb1;

                    updateview(pr);
                } 

                textblogin = textBox3.Text;//保存地址

                /*-读取保存用户ID及密码       * 
                * 
                * -*/
                try
                {
                    StreamReader sr = new StreamReader("loginid.lib", Encoding.Default);

                    String line;

                    if ((line = sr.ReadLine()) != null)
                    {
                        string texID = line.ToString();

                        /*-从texID中获取以//分割的ID和PASSWORD名称（;符号之前的ID  之后的password）
                            * 
                            * -*/
                        Match match = regexmatch(@"(.*?);(.*?);(.*?);(.*?);(.*?);(.*?);", texID);

                        loginID = match.Groups[1].Value;

                        loginpassword = match.Groups[2].Value.Replace(" ", "");

                        try
                        {

                            workerthread =Convert.ToInt32( match.Groups[3].Value.Replace(" ", ""));

                            iothread = Convert.ToInt32(match.Groups[4].Value.Replace(" ", ""));

                          //  autodownload = Convert.ToInt32(match.Groups[5].Value.Replace(" ", ""));

                          //  header.timeout = timeout;                        

                            retry =Convert.ToInt32( match.Groups[6].Value.Replace(" ", ""));
                        }
                        catch (Exception err)
                        {
                            sr.Close();
                            MessageBox.Show("配置文件异常,正在恢复初始配置");
                            try
                            {
                                FileStream fs = new FileStream("loginid.lib", FileMode.Create);

                                StreamWriter sw = new StreamWriter(fs);
                                //开始写入
                                sw.Write("roousama;852963;50;50;1;2;");
                                //清空缓冲区
                                sw.Flush();
                                //关闭流
                                sw.Close();
                                fs.Close();
                            }
                            catch (Exception eer)
                            {
                                MessageBox.Show("恢复失败" + eer);

                                return;
                            } 

                        }
                        
                    }
                    else
                    {
                        pr.text = "读取信息出错" ;

                        updateview(pr);

                        return;                        
                    }

                  sr.Close();
                }
                catch (Exception er)
                {
                    MessageBox.Show("无法打开配置文件,恢复初始配置");

                    try
                    {
                        FileStream fs = new FileStream("loginid.lib", FileMode.Create);

                        StreamWriter sw = new StreamWriter(fs);
                        //开始写入
                        sw.Write("roousama;852963;50;50;1;2;");
                        //清空缓冲区
                        sw.Flush();
                        //关闭流
                        sw.Close();
                        fs.Close();
                    }
                    catch (Exception eer)
                    {
                        MessageBox.Show("恢复失败" + eer);

                        return;
                    } 

                    return;
                }

                checkBox1.Enabled = false;
                button1.Enabled = false;

                /*-建立线程开始下载分析*/
                Thread1 = new Thread(new ThreadStart(httpmainloginpost));

                Thread1.Start(); 
                
                pr.text = "开始分析网址";

                updateview(pr);

            }
            catch (Exception er)
            {
                MessageBox.Show(""+er);

                button1.Enabled = true;
                textBox1.Enabled = true;

            }
        }


        /// <summary>
        /// menu菜单中确认保存地址
        /// 将保存地址在本地txt中保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 选择路径ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {               
                textBox3.Text = this.folderBrowserDialog1.SelectedPath; 

                FileStream fs = new FileStream("savelocal.txt", FileMode.Create);

                StreamWriter sw = new StreamWriter(fs);
                //开始写入
                sw.Write(textBox3.Text);
                //清空缓冲区
                sw.Flush();
                //关闭流
                sw.Close();
                fs.Close();
            }
        }


        /// <summary>
        /// winform初始化 
        /// 初始化头文件信息
        /// 读取文件保存地址
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {

            System.Net.ServicePointManager.DefaultConnectionLimit = 512;
            /*-初始化http连接头文件信息
             * 
             * 初始化委托实例
             * -*/
            header.accept = "image/gif, image/x-xbitmap, image/jpeg, image/pjpeg, application/x-shockwave-flash, application/x-silverlight, application/vnd.ms-excel, application/vnd.ms-powerpoint, application/msword, application/x-ms-application, application/x-ms-xbap, application/vnd.ms-xpsdocument, application/xaml+xml, application/x-silverlight-2-b1, */*";

            header.contentType = "application/x-www-form-urlencoded";

            header.userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:45.0) Gecko/20100101 Firefox/45.0";

            header.maxTry = 300;

          //  header.timeout = 1000;

            header.Length = 0;

            UpdateV = new UpdateView(updateview);/////委托的实例

            /*-读取保存地址       * 
             * 
             * -*/
            StreamReader srload = new StreamReader("savelocal.txt", Encoding.Default);

            String lineload;

            try
            {
                /*-读取默认ID图片       * 
                 * 
                 * -*/
                Image img = Image.FromFile("noface.png");//双引号里是图片的路径

                pictureBox1.Image = img;

                if ((lineload = srload.ReadLine()) != null)
                {
                    textBox3.Text = lineload.ToString();
                }
                else
                {
                    textBox3.Text = @"c:\";
                }
            }
            catch (Exception er)
            {
                MessageBox.Show("保存路径出错"+er);
                textBox3.Text = @"c:\";
            }
            srload.Close();



            /*-读取保存用户ID及密码       * 
                * 
                * -*/
            try
            {
                StreamReader sr = new StreamReader("check.lib", Encoding.Default);

                String line;

                if ((line = sr.ReadLine()) != null)
                {
                    autodownload =Convert.ToInt32( line.ToString()); 
                }
                else
                {
                    pr.text = "读取信息出错";

                    updateview(pr);

                    return;
                }

                sr.Close();
            }
            catch (Exception er)
            {
                MessageBox.Show("无法打开配置文件,恢复初始配置");

                try
                {
                    FileStream fs = new FileStream("check.lib", FileMode.Create);

                    StreamWriter sw = new StreamWriter(fs);
                    //开始写入
                    sw.Write("1");
                    //清空缓冲区
                    sw.Flush();
                    //关闭流
                    sw.Close();
                    fs.Close();
                }
                catch (Exception eer)
                {
                    MessageBox.Show("恢复失败" + eer);

                    return;
                }

                return;
            }

            if (autodownload == 1)
            {
                checkBox1.Checked = true;
            }
            else
            {
                checkBox1.Checked = false;
            }



        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            System.Environment.Exit(System.Environment.ExitCode);
            this.Dispose();
            this.Close();
        }

        private void 功能设置ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Seting setingform = new Seting();

            setingform.ShowDialog();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (checkBox1.Checked == true)
            {
                autodownload = 1;
            }
            else
            {
                autodownload = 0;
            }
            try
            {
                FileStream fs = new FileStream("check.lib", FileMode.Create);

                StreamWriter sw = new StreamWriter(fs);
                //开始写入
                sw.Write(autodownload);
                //清空缓冲区
                sw.Flush();
                //关闭流
                sw.Close();
                fs.Close();
            }
            catch (Exception eer)
            {
                MessageBox.Show("保存配置失败" + eer);

                
            }



        }
    }

    /// <summary>
    /// HTTP头文件辅助类
    /// </summary>
    public class HttpHeader
    {

        public HttpHeader()
        {

            accept = "image/gif, image/x-xbitmap, image/jpeg, image/pjpeg, application/x-shockwave-flash, application/x-silverlight, application/vnd.ms-excel, application/vnd.ms-powerpoint, application/msword, application/x-ms-application, application/x-ms-xbap, application/vnd.ms-xpsdocument, application/xaml+xml, application/x-silverlight-2-b1, */*";

            contentType = "application/x-www-form-urlencoded";

            userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:45.0) Gecko/20100101 Firefox/45.0";

            maxTry = 300;

           // timeout = 1000;

            Length = 0;

            Lengthend = 0;
        }

        public string contentType { get; set; }

        public string accept { get; set; }

        public string userAgent { get; set; }

        public string method { get; set; }

        public int maxTry { get; set; }

        public int timeout { get; set; }

        public int Length { get; set; }

        public int Lengthend { get; set; }
    }
    
}
