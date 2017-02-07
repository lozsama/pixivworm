using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.IO;
using System.Threading;
using System.Data;
using System.Web.Script.Serialization;
using System.Collections;

namespace Pixivworm
{
   public class HttpToolClass
    {
        /// <summary> 
        /// 通过GET方式发送数据        
        /// </summary> 
        /// <param name="Url">url</param> 
        /// <param name="postDataStr">Get数据</param>        
        /// <param name="cookie">Cookie容器</param>         
        /// <returns></returns> 
       public string SendDataByGet(string Url, string postDataStr, ref CookieContainer cookie, out ArrayList arrayListpost, HttpHeader header)
        {           
            //  HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url + (postDataStr == "" ? "" : "?") + postDataStr);

            string returnsti;                        

            ArrayList arrayListpostmid = new ArrayList();

            if (cookie.Count == 0)
            {
                request.CookieContainer = new CookieContainer();

                cookie = request.CookieContainer;
            }
            else
            {
                request.CookieContainer = cookie;
            }

            request.Method = "GET";

            try
            {
                request.Timeout =30000;
                request.ContentType = header.contentType;
                request.ServicePoint.ConnectionLimit = int.MaxValue;
                request.Accept = header.accept;
                request.UserAgent = header.userAgent;

                request.Headers.Add("Accept-Language", "zh-CN,zh;q=0.8");
                request.Referer = Url + (postDataStr == "" ? "" : "?") + postDataStr;
                

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                Stream myResponseStream = response.GetResponseStream();                

                StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));

                string retString = myStreamReader.ReadToEnd();

                myStreamReader.Close();

                myResponseStream.Close();
                
                arrayListpostmid.Add(retString);

                returnsti = "";
            }
            catch (Exception httperr)
            {
                returnsti = httperr.ToString();
            }
           
            if (arrayListpostmid == null)
            {
                arrayListpostmid = new ArrayList();
            }

            arrayListpost = arrayListpostmid;

            return returnsti;
        }



       /// <summary> 
       /// 通过GET方式发送接收图片       
       /// </summary> 
       /// <param name="Url">url</param> 
       /// <param name="postDataStr">Get数据</param>        
       /// <param name="cookie">Cookie容器</param>         
       /// <returns></returns> 
       public string SendDataByGetimag(string Url, string postDataStr, ref CookieContainer cookie, ref HttpWebResponse arrayListpost, HttpHeader header)
       {           
           HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url + (postDataStr == "" ? "" : "?") + postDataStr);

           if (header.Lengthend == 0)
           {
               request.AddRange((int)header.Length);
           }
           else
           {
               request.AddRange((int)header.Length, (int)header.Lengthend);
           }
         

           string returnsti;

         //  ArrayList arrayListpostmid = new ArrayList();

           if (cookie.Count == 0)
           {
               request.CookieContainer = new CookieContainer();

               cookie = request.CookieContainer;
           }
           else
           {
               request.CookieContainer = cookie;
           }

           request.Method = "GET";        
           try
           {
               request.Timeout = 10000;

              // request.ReadWriteTimeout = 10000; 
              
               request.ContentType = header.contentType;
               request.ServicePoint.ConnectionLimit = int.MaxValue;
               request.Accept = header.accept;
               request.UserAgent = header.userAgent;
              
               request.Headers.Add("Accept-Language", "zh-CN,zh;q=0.8");
             

               request.Referer = Url + (postDataStr == "" ? "" : "?") + postDataStr;
            
             //  HttpWebResponse response = (HttpWebResponse)request.GetResponse();
               arrayListpost = (HttpWebResponse)request.GetResponse();
               //myResponseStream = response.GetResponseStream();  
              // Thread.Sleep(10);
               // arrayListpostmid.Add(response.GetResponseStream());               

               returnsti = "";

             //  arrayListpost = response.GetResponseStream();                
           }
           catch (Exception httperr)
           {
               returnsti = httperr.ToString();
               arrayListpost = null;
           }
        /*   if (arrayListpostmid == null)
           {
               arrayListpostmid = new ArrayList();
               arrayListpost = null;
           }*/         

           return returnsti;
       }


        /// <summary> 
        /// 通过POST方式发送数据        
        /// </summary> 
        /// <param name="Url">url</param> 
        /// <param name="postDataStr">Post数据</param>        
        /// <param name="cookie">Cookie容器</param>         
        /// <returns></returns> 
        public string SendDataByPost(string Url, string postDataStr, ref CookieContainer cookie, out ArrayList arrayListpost, HttpHeader header)
        {
            string timestamp = (DateTimeToStamp(System.DateTime.Now)).ToString();

            arrayListpost = new ArrayList();          

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);

            string returnsti;           

            ArrayList arrayListpostmid = new ArrayList();

            if (cookie.Count == 0)
            {
                request.CookieContainer = new CookieContainer();

                cookie = request.CookieContainer;
            }
            else
            {
                request.CookieContainer = cookie;
            }

            request.Method = "POST";

            try
            {
                byte[] buffer = Encoding.UTF8.GetBytes(postDataStr);

                StreamWriter myWriter = null;

                request.ContentType = header.contentType;
                request.ServicePoint.ConnectionLimit = int.MaxValue;
                request.Accept = header.accept;
                request.UserAgent = header.userAgent;
                request.Headers.Add("Accept-Language", "zh-CN,zh;q=0.8");
                request.Referer = Url;
                 
                request.ContentLength = buffer.Length;  
                    
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:45.0) Gecko/20100101 Firefox/45.0";
    

                myWriter = new StreamWriter(request.GetRequestStream());

                myWriter.Write(postDataStr);

                myWriter.Close();

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                Stream myResponseStream = response.GetResponseStream();               

                StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));

                string retString = myStreamReader.ReadToEnd();

                myStreamReader.Close();

                myResponseStream.Close();
               
                arrayListpostmid.Add(retString);              

                returnsti = "";

            }
            catch (Exception httperr)
            {
                returnsti = httperr.ToString();
            }

            
            if (arrayListpostmid == null)
            {
                arrayListpostmid = new ArrayList();
            }

            arrayListpost = arrayListpostmid;

            return returnsti;
        }

/// <summary>
/// 时间戳计算
/// </summary>
/// <param name="time"></param>
/// <returns></returns>
        static private int DateTimeToStamp(System.DateTime time)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            return (int)(time - startTime).TotalSeconds;
        }



/// <summary>
/// 包装 POST 发送
/// </summary>
/// <param name="url"></param>
/// <param name="code"></param>
/// <returns></returns>
        public ArrayList sqlliist(string url, string code, HttpHeader header)
        {      

            ArrayList assembly = new ArrayList();

            CookieClass cookieClass = CookieClass.Instance;

            ArrayList postget = new ArrayList();

            ArrayList arrylistpost = new ArrayList();

            String req = SendDataByPost(url, code, ref  cookieClass.cookie, out arrylistpost,  header);

            if (req == "" && arrylistpost.Count > 0)
            {
                postget = arrylistpost;
            }
            else
            {
                if (req != "")
                {
                    throw new Exception("HTTP异常:" + req);                  
                }
            }
                
            return postget;


        }

/// <summary>
/// 包装 GET 发送
/// </summary>
/// <param name="url"></param>
/// <param name="code"></param>
/// <returns></returns>
        public ArrayList sqlliistGet(string url, string code, HttpHeader header)
        {
            ArrayList postget = new ArrayList();
            try
            {
                ArrayList assembly = new ArrayList();

                CookieClass cookieClass = CookieClass.Instance;

              

                ArrayList arrylistpost = new ArrayList();

                String req = SendDataByGet(url, code, ref  cookieClass.cookie, out arrylistpost, header);

                if (req == "" && arrylistpost.Count > 0)
                {
                    postget = arrylistpost;
                }
                else
                {
                    if (req != "")
                    {
                        throw new Exception("HTTP异常:" + req);

                    }
                    else
                    {
                        postget.Add("");
                    }
                }
            }
            catch (Exception er)
            {
                postget.Add("");
                throw new Exception("HTTP异常:" + er);                
            }


               
        
           
            return postget;
        }

        /// <summary>
        /// 包装 GET 发送获取imag
        /// </summary>
        /// <param name="url"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public void sqlliistGetimag(string url, string code, HttpHeader header, ref HttpWebResponse postgetpic)
        {

           // ArrayList assembly = new ArrayList();

            CookieClass cookieClass = CookieClass.Instance;

            String req = SendDataByGetimag(url, code, ref  cookieClass.cookie, ref postgetpic, header);

            if (req == "")
            {
               
            }
            else
            {
                if (req != "")
                {
                    throw new Exception("HTTP异常:" + req);

                }
                else
                {
                   // postgetpic = null;
                }
            }          
        }


    }
}
