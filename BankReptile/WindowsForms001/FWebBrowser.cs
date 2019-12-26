using CefSharp;
using CefSharp.WinForms;
using Common;
using Common.Types;
using Entity;
using Entity.WebApiResponse;
using Gecko;
using HtmlAgilityPack;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using static FWebBrowser.InternetSet;

namespace FWebBrowser
{

    public partial class FWebBrowser : Form
    {
        private bool IsLogin = false;
        private ActionInvoke invoke;

        private TaskInfo TaskInfo
        {
            get; set;
        }

        public FWebBrowser(TaskInfo info, string args)
        {

            InitializeComponent();
            InitBrowser();
            //this.webBrowser1 = new GeckoWebBrowser();
            GeckoWebBrowser gecko = new GeckoWebBrowser();
            gecko.CreateControl();
            gecko.NoDefaultContextMenu = true; //禁用右键菜单
            gecko.Dock = DockStyle.Fill;
            this.Controls.Add(gecko); //添加到窗口中

            invoke = new ActionInvoke(this);
            TaskInfo = info;
            TaskInfo.Channel = (Channel)info.bank_id;//根据银行bank_id 设置客户端类型

            txtAccount.Text = info.UserName;
            txtPassword.Text = info.PassWord;
            txtPassword.TextBox.PasswordChar = '*';


            WfmLogin();

        }
        public ChromiumWebBrowser browser;
        public void InitBrowser()
        {
            Cef.Initialize(new CefSettings());
            //browser = new ChromiumWebBrowser("http://www.baidu.com");
            switch (TaskInfo.Channel)
            {
                case Channel.支付宝:
                    browser = new ChromiumWebBrowser("https://auth.alipay.com/login/index.htm");
                    Thread.Sleep(5000);
                    //加载完毕后触发事件webBrowser1_DocumentCompleted
                    break;

                case Channel.苏宁云平台:
                    browser = new ChromiumWebBrowser("https://mpassport.suning.com/ids/login");
                    Thread.Sleep(5000);
                    //加载完毕后触发事件webBrowser1_DocumentCompleted
                    break;

                default:
                    throw new Exception("暂不支持该银行！");
            }

            Font font = new Font("微软雅黑", 10.5f);
            this.tabPage1.Controls.Add(browser);
            browser.Font = font;
            browser.Dock = DockStyle.Fill;
            browser.LoadingStateChanged += new EventHandler<LoadingStateChangedEventArgs>(LoadingStateChangeds);

        }
        //加载状态
        private void LoadingStateChangeds(object sender, EventArgs e)
        {


        }
        /// <summary>
        /// 登录操作
        /// </summary>
        private void WfmLogin()
        {
            //switch (TaskInfo.Channel)
            //{
            //    case Channel.支付宝:
            //        webBrowser1.Navigate("https://auth.alipay.com/login/index.htm");
            //        Thread.Sleep(5000);
            //        //加载完毕后触发事件webBrowser1_DocumentCompleted
            //        webBrowser1.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(webBrowser1_DocumentCompleted);
            //        break;

            //    case Channel.苏宁云平台:
            //        webBrowser1.Navigate("https://mpassport.suning.com/ids/login");
            //        Thread.Sleep(5000);
            //        //加载完毕后触发事件webBrowser1_DocumentCompleted
            //        webBrowser1.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(webBrowser1_DocumentCompleted);
            //        break;

            //    default:
            //        throw new Exception("暂不支持该银行！");
            //}

        }

        /// <summary>
        /// 页面登录成功
        /// 进行数据上传操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnUpadteDate_Click(object sender, EventArgs e)
        {
            UpdateJsonData();
        }
        private void Start()
        {
            new Thread(() =>
            {
                Thread.Sleep(3000);
                while (true)
                {
                    try
                    {
                        new Thread(() => WfmRefresh()).Start();
                    }
                    catch
                    {

                    }
                    //35秒刷新下页面。保持通讯状态
                    Thread.Sleep(35000);
                }
            }).Start();

            //心跳包通讯
            new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        string resul2t = CallApi.PostAPI($"{ConfigurationManager.AppSettings["webInterfaceApi"]}/api/device/ping", $"code_id={TaskInfo.code_id}&token={TaskInfo.token}");
                        var models = JsonHelper.ToObject<PingRespnse>(resul2t);

                        //判断是否登录
                        new Thread(() => WebBrowserToStandard()).Start();

                        //判断用户是否登录成功
                        if (IsLogin)
                        {
                            invoke.Invoke(() =>
                            {
                                lblMessage.Text = "登录成功";
                                lblMessage.ForeColor = Color.Green;
                            });

                            SetMessage(MessageType.状态, "登录成功");
                            //上报心跳包并获取PageIndex   未实现
                            //获取到的PageIndex保存进入Pages

                            ///api/device/ping
                            //心跳包请求ID
                            string result = CallApi.PostAPI($"{ConfigurationManager.AppSettings["webInterfaceApi"]}/api/device/ping", $"code_id={TaskInfo.code_id}&token={TaskInfo.token}");
                            var model = JsonHelper.ToObject<PingRespnse>(result);
                            if (model.status == 1)
                            {
                                if (model.result.need_data == 1)
                                {
                                    //等于1 则进行数据 抓取上传操作
                                    //new Thread(() => UpdateJsonData()).Start();
                                }
                            }
                        }
                        else
                        {
                            invoke.Invoke(() =>
                            {
                                lblMessage.Text = "未登录";
                                lblMessage.ForeColor = Color.Red;
                            });

                            SetMessage(MessageType.状态, "未登录");
                        }
                    }
                    catch
                    {

                    }
                    Thread.Sleep(15000);//15秒保持心跳通讯
                }
            }).Start();

        }

        /// <summary>
        /// 
        /// 上传解析
        /// </summary>
        private void UpdateJsonData()
        {
            switch (TaskInfo.Channel)
            {
                case Channel.支付宝:
                    //HtmlElement doc = webBrowser1.Document.GetElementById("container");

                    var task1 = browser.GetSourceAsync();
                    task1.Wait();
                    string html = task1.Result;

                    string docmaHtml = html;

                    GetDealPostDate(docmaHtml);
                    break;
                case Channel.苏宁云平台:

                    break;
                default:
                    throw new Exception("暂不支持该银行！");
            }


        }


        /// <summary>
        /// 跳转到查询交易明细页面
        /// </summary>
        private void WebBrowserToStandard()
        {
            try
            {
                switch (TaskInfo.Channel)
                {
                    case Channel.支付宝:
                        //获取页面内容。然后解析
                        var txtIsLoginConent = "";// webBrowser1.Document.GetElementById("container");
                        if (txtIsLoginConent == "")
                        {
                            IsLogin = false;
                        }
                        else
                        {
                            IsLogin = true;
                            try
                            {
                                var txtIsLoginTable = "";// webBrowser1.Document.GetElementById("tradeRecordsIndex");

                            }
                            catch (Exception)
                            {
                                // webBrowser1.Navigate("https://consumeprod.alipay.com/record/standard.htm");
                                browser = new ChromiumWebBrowser("https://consumeprod.alipay.com/record/standard.htm");
                                Thread.Sleep(5000);
                            }

                        }
                        break;
                    case Channel.苏宁云平台:

                        break;
                    default:
                        throw new Exception("暂不支持该银行！");
                }


            }
            catch (Exception ex)
            {
                IsLogin = false;
            }
        }

        /// <summary>
        /// 页面加载完毕执行操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            switch (TaskInfo.Channel)
            {
                case Channel.支付宝:
                    //设置账户和密码
                    //HtmlElement txtUserName = webBrowser1.Document.GetElementById("J-input-user");
                    //HtmlElement txtPassWord = webBrowser1.Document.GetElementById("password_rsainput");

                    //if (txtUserName == null || txtPassWord == null)
                    //    return;
                    //txtUserName.SetAttribute("value", TaskInfo.UserName);
                    //txtPassWord.SetAttribute("value", TaskInfo.PassWord);

                    //////获取按钮登录
                    //HtmlElement txtBtnLogin = webBrowser1.Document.GetElementById("J-login-btn");
                    //txtBtnLogin.InvokeMember("click");

                    browser.GetBrowser().MainFrame.ExecuteJavaScriptAsync("document.getElementById('J-input-user').value='" + TaskInfo.UserName + "'");
                    browser.GetBrowser().MainFrame.ExecuteJavaScriptAsync("document.getElementById('password_rsainput').value='" + TaskInfo.PassWord + "'");
                    Thread.Sleep(15000);
                    browser.GetBrowser().MainFrame.ExecuteJavaScriptAsync("document.getElementById('J-login-btn').click();");

                    Start();
                    break;
                case Channel.苏宁云平台:
                    browser.GetBrowser().MainFrame.ExecuteJavaScriptAsync("document.getElementById('userName').value='" + TaskInfo.UserName + "'");
                    browser.GetBrowser().MainFrame.ExecuteJavaScriptAsync("document.getElementById('password').value='" + TaskInfo.PassWord + "'");
                    Thread.Sleep(15000);
                    browser.GetBrowser().MainFrame.ExecuteJavaScriptAsync("document.getElementById('loginButton').click();");

                    //Start();
                    break;
                default:
                    throw new Exception("暂不支持该银行！");
            }


        }

        /// <summary>
        /// 手动刷新页面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnRefresh_Click(object sender, EventArgs e)
        {

            //WfmRefresh();
            switch (TaskInfo.Channel)
            {
                case Channel.支付宝:
                    //webBrowser1.Navigate("https://consumeprod.alipay.com/record/standard.htm");
                    browser.Load("https://consumeprod.alipay.com/record/standard.htm");
                    Thread.Sleep(5000);
                    IsLogin = true;
                    break;
                case Channel.苏宁云平台:
                    browser.Load("https://moms.suning.com/moms/delivery/toHaveSoldBabyMain.action?tabtype=3");
                    break;
                default:
                    throw new Exception("暂不支持该银行！");
            }


        }

        /// <summary>
        /// 刷新页面
        /// </summary>
        private void WfmRefresh()
        {
            try
            {
                //webBrowser1.Document.ExecCommand("Refresh", false, null);//真正意义上的F5刷新
                browser.GetBrowser().MainFrame.ExecuteJavaScriptAsync("window.location.reload();");
            }
            catch (Exception ex)
            {

            }

        }

        private void SetMessage(MessageType messageType, string message)
        {
            if (messageType != MessageType.状态)
            {
                message = $"账户{TaskInfo.UserName}提醒您：{message}";
                MessageBox.Show(message);
            }
        }


        private void lblShowPassWord_Click(object sender, EventArgs e)
        {
            if (lblShowPassWord.Text == "显示密码")
            {
                txtPassword.TextBox.PasswordChar = '\0';
                lblShowPassWord.Text = "隐藏密码";
            }
            else
            {
                txtPassword.TextBox.PasswordChar = '*';
                lblShowPassWord.Text = "显示密码";
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Process.GetCurrentProcess().Kill();
        }

        #region MyRegion

        /// <summary>
        /// 页面内容转化DataTable进行上传操作
        /// </summary>
        /// <param name="str"></param>
        private void GetDealPostDate(string str)
        {
            switch (TaskInfo.Channel)
            {
                case Channel.支付宝:
                    DataTable dtstr = GetQueryDataTable(str, "");
                    if (dtstr != null)
                    {  //有数据则进行数据上传操作
                        this.dataGridView1.DataSource = dtstr;
                        //上传操作
                        string strDataJson = Newtonsoft.Json.JsonConvert.SerializeObject(dtstr, Newtonsoft.Json.Formatting.None, new Newtonsoft.Json.Converters.DataTableConverter());
                        string strData = "{\"data\":" + strDataJson + "}";

                        string result = CallApi.PostAPI($"{ConfigurationManager.AppSettings["webInterfaceApi"]}/api/device/postData", $"code_id={TaskInfo.code_id}&bank_id={TaskInfo.bank_id}&page=1&token={TaskInfo.token}&data={strData}");
                        var model = JsonHelper.ToObject<DefaultReponse>(result);
                        if (model.status == 1)
                        {
                            SetMessage(MessageType.默认, TaskInfo.UserName + "交易记录上传成功");
                        }
                    }
                    break;
                case Channel.苏宁云平台:
                    DataTable dtSnData = GetSnYunataTable(str, "");
                    if (dtSnData != null)
                    {  //有数据则进行数据上传操作
                        this.dataGridView1.DataSource = dtSnData;
                        //上传操作
                        string strsnDataJson = Newtonsoft.Json.JsonConvert.SerializeObject(dtSnData, Newtonsoft.Json.Formatting.None, new Newtonsoft.Json.Converters.DataTableConverter());
                        string strsnData = "{\"data\":" + strsnDataJson + "}";

                        string result = CallApi.PostAPI($"{ConfigurationManager.AppSettings["webInterfaceApi"]}/api/device/postData", $"code_id={TaskInfo.code_id}&bank_id={TaskInfo.bank_id}&page=1&token={TaskInfo.token}&data={strsnData}");
                        var model = JsonHelper.ToObject<DefaultReponse>(result);
                        if (model.status == 1)
                        {
                            SetMessage(MessageType.默认, TaskInfo.UserName + "交易记录上传成功");
                        }
                    }

                    break;
                default:
                    throw new Exception("暂不支持该银行！");
            }


        }
        /// <summary>
        /// 支付宝:html解析转化为DataTable
        /// </summary>
        /// <param name="Text"></param>
        /// <param name="TableID"></param>
        /// <param name="IsSkipTitle"></param>
        /// <returns></returns>
        private static DataTable GetQueryDataTable(string Text, string TableID, bool IsSkipTitle = false)
        {
            try
            {
                DataTable dt = new DataTable();
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(Text);
                HtmlNodeCollection Table = doc.DocumentNode.SelectNodes("//*[@id='tradeRecordsIndex']");
                if (Table.Count > 0) //判断是否存在
                {
                    HtmlNodeCollection Th = null;
                    int index = 0;
                    HtmlNodeCollection HtmlTr = Table[0].SelectNodes("tbody")[0].SelectNodes("tr");
                    if (HtmlTr.Count > 1) //判断没有数据的
                    {
                        //HtmlNodeCollection Ths = Table[0].SelectNodes("thead")[0].SelectNodes("tr")[0].SelectNodes("th");
                        //if (Ths != null) //创建表字段
                        //{
                        //    Th = Ths;
                        //    for (int i = 0; i < Th.Count; i++)
                        //    {
                        //        dt.Columns.Add(Th[i].InnerText, System.Type.GetType("System.String")); //动态加列
                        //    }
                        //}
                        dt.Columns.Add("order_no", System.Type.GetType("System.String"));
                        dt.Columns.Add("trading_time", System.Type.GetType("System.String"));
                        dt.Columns.Add("order_money", System.Type.GetType("System.String"));
                        dt.Columns.Add("reciprocal_account", System.Type.GetType("System.String"));
                        dt.Columns.Add("reciprocal_name", System.Type.GetType("System.String"));
                        dt.Columns.Add("lei_6", System.Type.GetType("System.String"));

                        foreach (HtmlNode row in Table[0].SelectNodes("tbody")[0].SelectNodes("tr"))
                        {
                            if (IsSkipTitle && index == 0) //去掉表头
                            {
                                index++;
                                continue;
                            }

                            HtmlNodeCollection tds = row.SelectNodes("td");
                            if (tds != null)
                            {
                                DataRow dr = dt.NewRow();
                                for (int i = 0; i < tds.Count; i++)
                                {
                                    //自动匹配抓取
                                    //if (!string.IsNullOrEmpty(Th[i].InnerText))
                                    //    dr[Th[i].InnerText] = tds[i].InnerText;

                                    switch (i)
                                    {
                                        case 0:
                                            dr["trading_time"] = GetTimeStamp();
                                            dr["lei_6"] = 1;
                                            dr["reciprocal_account"] = "";
                                            break;
                                        case 1:

                                            break;
                                        case 2:
                                            dr["reciprocal_name"] = tds[i].SelectNodes("p")[0].InnerText.Trim() + "|" + tds[i].SelectNodes("p")[1].InnerText.Trim();
                                            dr["order_no"] = tds[i].SelectNodes("div")[0].SelectNodes("a")[0].Attributes["title"].Value;
                                            break;
                                        case 3:
                                            dr["order_money"] = tds[i].InnerText.Trim();
                                            break;
                                        default:
                                            break;
                                    }


                                }


                                dt.Rows.Add(dr);
                            }
                            index++;
                        }
                    }
                }
                return dt;
            }
            catch (Exception ex)
            {
                return null;

            }
        }

        /// <summary>
        /// 苏宁云平台:html解析转化为DataTable
        /// </summary>
        /// <param name="Text"></param>
        /// <param name="TableID"></param>
        /// <param name="IsSkipTitle"></param>
        /// <returns></returns>
        private static DataTable GetSnYunataTable(string Text, string TableID, bool IsSkipTitle = false)
        {
            try
            {
                DataTable dt = new DataTable();
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(Text);
                HtmlNodeCollection Table = doc.DocumentNode.SelectNodes("//*[@id='tabdiv']");
                if (Table.Count > 0) //判断是否存在
                {
                    HtmlNodeCollection Th = null;
                    int index = 0;
                    string HtmlType = Table[0].SelectNodes("div")[0].SelectNodes("span")[0].InnerText;
                    HtmlNodeCollection HtmlTr = Table[0].SelectNodes("table")[0].SelectNodes("tbody")[0].SelectNodes("tr");
                    if (HtmlTr.Count > 1) //判断没有数据的
                    {
                        //HtmlNodeCollection Ths = Table[0].SelectNodes("table")[0].SelectNodes("thead")[0].SelectNodes("tr")[0].SelectNodes("th");
                        //if (Ths != null) //创建表字段
                        //{
                        //    Th = Ths;
                        //    for (int i = 0; i < Th.Count; i++)
                        //    {
                        //        dt.Columns.Add(Th[i].InnerText, System.Type.GetType("System.String")); //动态加列
                        //    }
                        //}

                        dt.Columns.Add("shop_order_no", System.Type.GetType("System.String"));
                        dt.Columns.Add("create_time", System.Type.GetType("System.String"));
                        dt.Columns.Add("price", System.Type.GetType("System.String"));
                        dt.Columns.Add("status", System.Type.GetType("System.String"));
                        dt.Columns.Add("finish_time", System.Type.GetType("System.String"));
                        dt.Columns.Add("exprie_time", System.Type.GetType("System.String"));


                        // Table[0].SelectNodes("table")[0].SelectNodes("tbody")[0].SelectNodes("tr")[1].SelectNodes("td")[0].SelectNodes("table")[0].SelectNodes("tbody")[0].SelectNodes("tr")
                        foreach (HtmlNode row in Table[0].SelectNodes("table")[0].SelectNodes("tbody")[0].SelectNodes("tr"))
                        {
                            if (index == 0) //去掉表头IsSkipTitle && 
                            {
                                index++;
                                continue;
                            }
                            HtmlNodeCollection dxDdNum = Table[0].SelectNodes("table")[0].SelectNodes("tbody")[0].SelectNodes("tr")[index].SelectNodes("td")[0].SelectNodes("table")[0].SelectNodes("tbody")[0].SelectNodes("tr")[0].SelectNodes("td")[0].SelectNodes("div")[0].SelectNodes("div")[0].SelectNodes("span");
                            string strDdHm = dxDdNum[0].InnerText;
                            string strDdDate = dxDdNum[1].InnerText;
                            string strDateXdsj = strDdDate.Split('：')[1];

                            HtmlNodeCollection dxDdTdNum = Table[0].SelectNodes("table")[0].SelectNodes("tbody")[0].SelectNodes("tr")[index].SelectNodes("td")[0].SelectNodes("table")[0].SelectNodes("tbody")[0].SelectNodes("tr");
                            HtmlNodeCollection tds = dxDdTdNum[1].SelectNodes("td");

                            DataRow dr = dt.NewRow();
                            dr["shop_order_no"] = strDdHm.Split('：')[1];
                            dr["create_time"] = TimestampHelper.GetUtcTimeStamp(Convert.ToDateTime(strDateXdsj));
                            switch (HtmlType)
                            {
                                case "未付款：":
                                    dr["status"] = "0";//待支付
                                    dr["finish_time"] = "";
                                    dr["exprie_time"] = strDateXdsj;
                                    break;
                                case "未发货：":
                                    dr["status"] = "1";//已完成
                                    dr["finish_time"] = "";
                                    dr["exprie_time"] = strDateXdsj;
                                    break;
                                default:
                                    break;
                            }
                            if (tds != null)
                            {
                                for (int i = 0; i < tds.Count; i++)
                                {
                                    ////自动匹配抓取
                                    //if (!string.IsNullOrEmpty(Th[i].InnerText))
                                    //    dr[Th[i].InnerText] = tds[i].InnerText;

                                    switch (i)
                                    {
                                        case 0:
                                            break;
                                        case 6:
                                            //dr["reciprocal_name"] = tds[i].SelectNodes("p")[0].InnerText.Trim() + "|" + tds[i].SelectNodes("p")[1].InnerText.Trim();
                                            dr["price"] = tds[i].SelectNodes("b")[0].InnerText;//.SelectNodes("a")[0].Attributes["title"].Value;
                                            break;
                                        default:
                                            break;
                                    }


                                }


                                dt.Rows.Add(dr);
                            }
                            index++;
                        }
                    }
                }
                return dt;
            }
            catch (Exception ex)
            {
                return null;

            }
        }

        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        public static string GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalMilliseconds).ToString();
        }

        #endregion

        private void FWebBrowser_Load(object sender, EventArgs e)
        {
            this.Text = TaskInfo.code_name + "(" + TaskInfo.bank_name + ")";
            txtProxy.Text = "代理IP：" + TaskInfo.code_ip_proxy_ip;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string str = File.ReadAllText(@"D:\2019软件开发资源\抓取数据\BankReptile\WindowsForms001\snweifah.txt");
            GetDealPostDate(str);
        }
    }
}
