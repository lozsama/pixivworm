using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;

namespace Pixivworm
{
    public partial class Seting : Form
    {
        public Seting()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            loginID = textBox5.Text.Replace(" ", ""); ;

            loginpassword = textBox6.Text.Replace(" ", "");

            workerthread = textBox1.Text.Replace(" ", "");

            iothread = textBox2.Text.Replace(" ", "");

          //  timeout = textBox3.Text.Replace(" ", "");

            retry = textBox4.Text.Replace(" ", "");



            if (loginID == "" || loginpassword == "" || workerthread == "" || iothread == "" ||/* timeout == "" ||*/ retry == "")
            {
                MessageBox.Show("输入不可为空");
                return;
            }
            else
            {
                try
                {
                    int Intworkerthread = Convert.ToInt32(workerthread);

                    int Intiothread = Convert.ToInt32(iothread);

                    int Inttimeout = Convert.ToInt32(timeout);

                    int Intretry = Convert.ToInt32(retry);

                    string strloginID = loginID;

                    string strloginpassword = loginpassword;

                    FileStream fs = new FileStream("loginid.lib", FileMode.Create);

                    StreamWriter sw = new StreamWriter(fs);
                    //开始写入
                    sw.Write(strloginID + ";" + strloginpassword + ";" + Intworkerthread.ToString() + ";" + Intiothread.ToString() + ";" + Inttimeout.ToString() + ";" + Intretry.ToString() + ";");
                    //清空缓冲区
                    sw.Flush();
                    //关闭流
                    sw.Close();
                    fs.Close();

                    if (MessageBox.Show("操作完成，是否退出", "确认", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                    {
                       this.Close(); this.Dispose();
                    }

                }
                catch (Exception er)
                {
                    MessageBox.Show("导入出错");

                    if (MessageBox.Show("操作失败，是否退出", "确认", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                    {
                         this.Close(); this.Dispose();

                    }

                }
            }        
        }

        private int STRin(string str)
        {
            int i=0;



            return i;
        }


        /// <summary>
        /// 正则表达式 返回解析后的match
        /// 减少代码量
        /// </summary>
        /// <param name="regex">正则表达式</param>
        /// <param name="str">需要解析的string</param>
        /// <returns>解析后的match</returns>
        private Match regexmatch(String regex, String str)
        {
            Regex reg = new Regex(regex);

            Match match = reg.Match(Convert.ToString(str).ToString());

            return match;
        }

        private String loginID = "";

        private String loginpassword = "";

        private String workerthread = "";

        private String iothread = "";

        private String timeout = "";

        private String retry = "";

        private void Seting_Load(object sender, EventArgs e)
        {
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

                    loginID = match.Groups[1].Value.Replace(" ", ""); ;

                    loginpassword = match.Groups[2].Value.Replace(" ", "");

                    workerthread = match.Groups[3].Value.Replace(" ", "");
                    
                    iothread = match.Groups[4].Value.Replace(" ", "");
                    
                    timeout = match.Groups[5].Value.Replace(" ", "");
                    
                    retry = match.Groups[6].Value.Replace(" ", "");

                    textBox1.Text = workerthread;

                    textBox2.Text = iothread;

                    //textBox3.Text = timeout;

                    textBox4.Text = retry;

                    textBox5.Text = loginID;

                    textBox6.Text = loginpassword;

                }
                else
                {
                    MessageBox.Show("配置文件损坏");      

                   
                }

                sr.Close();
            }
            catch (Exception er)
            {
                MessageBox.Show("无法打开配置文件");

                return;
            } 
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                FileStream fs = new FileStream("loginid.lib", FileMode.Create);

                StreamWriter sw = new StreamWriter(fs);
                //开始写入
                sw.Write("roousama;852963;10;10;1;2;");
                //清空缓冲区
                sw.Flush();
                //关闭流
                sw.Close();
                fs.Close();

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

                        loginID = match.Groups[1].Value.Replace(" ", ""); ;

                        loginpassword = match.Groups[2].Value.Replace(" ", "");

                        workerthread = match.Groups[3].Value.Replace(" ", "");

                        iothread = match.Groups[4].Value.Replace(" ", "");

                        timeout = match.Groups[5].Value.Replace(" ", "");

                        retry = match.Groups[6].Value.Replace(" ", "");

                        textBox1.Text = workerthread;

                        textBox2.Text = iothread;

                        //textBox3.Text = timeout;

                        textBox4.Text = retry;

                        textBox5.Text = loginID;

                        textBox6.Text = loginpassword;

                        MessageBox.Show("已恢复默认配置");

                    }
                    else
                    {
                        MessageBox.Show("配置文件损坏");

                    }

                    sr.Close();
                }
                catch (Exception er)
                {
                    MessageBox.Show("无法打开配置文件");                   
                } 




            }
            catch (Exception er)
            {
                MessageBox.Show("恢复失败"+er);

                return;
            } 
        }
    }
}
