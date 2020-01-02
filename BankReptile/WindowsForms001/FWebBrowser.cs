using CefSharp;
using CefSharp.WinForms;
using Common;
using Common.Log;
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
        private PingRespnse.PingResult PingInfo
        {
            get; set;
        }
        public FWebBrowser(TaskInfo info, string args)
        {

            InitializeComponent();

            invoke = new ActionInvoke(this);
            TaskInfo = info;
            TaskInfo.code_ip_proxy_ip = info.code_ip_proxy_ip != null ? info.code_ip_proxy_ip.Replace("@", ":") : "";
            TaskInfo.Channel = (Channel)info.bank_id;//根据银行bank_id 设置客户端类型

            txtAccount.Text = info.UserName;
            txtPassword.Text = info.PassWord;
            txtPassword.TextBox.PasswordChar = '*';

            InitBrowser();

        }
        public ChromiumWebBrowser browser;
        /// <summary>
        /// 采集初始化
        /// </summary>
        public void InitBrowser()
        {
            var settings = new CefSettings();
            if (TaskInfo.code_ip_proxy_ip != "")
            {
                settings.CachePath = "cache";
                string proxyIp = "http://" + TaskInfo.code_ip_proxy_ip;
                settings.CefCommandLineArgs.Add("proxy-server", proxyIp);// "http://123.169.165.161:9999"

            }

            Cef.Initialize(settings);

            //Cef.Initialize(new CefSettings());

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
                case Channel.衣联网:
                    browser = new ChromiumWebBrowser("https://accounts.eelly.com/login/account");
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

            Start();
        }
        //加载状态
        private void LoadingStateChangeds(object sender, EventArgs e)
        {


        }

        /// <summary>
        /// 设置平台账号密码
        /// </summary>
        private void SetPingtUserNamePwd()
        {
            switch (TaskInfo.Channel)
            {
                case Channel.支付宝:
                    //设置账户和密码
                    browser.GetBrowser().MainFrame.ExecuteJavaScriptAsync("document.getElementById('J-input-user').value='" + TaskInfo.UserName + "'");
                    browser.GetBrowser().MainFrame.ExecuteJavaScriptAsync("document.getElementById('password_rsainput').value='" + TaskInfo.PassWord + "'");
                    Thread.Sleep(15000);
                    browser.GetBrowser().MainFrame.ExecuteJavaScriptAsync("document.getElementById('J-login-btn').click();");


                    break;
                case Channel.苏宁云平台:
                    browser.GetBrowser().MainFrame.ExecuteJavaScriptAsync("document.getElementById('userName').value='" + TaskInfo.UserName + "'");
                    browser.GetBrowser().MainFrame.ExecuteJavaScriptAsync("document.getElementById('password').value='" + TaskInfo.PassWord + "'");
                    Thread.Sleep(15000);
                    browser.GetBrowser().MainFrame.ExecuteJavaScriptAsync("document.getElementById('loginButton').click();");


                    break;
                case Channel.衣联网:
                    browser.GetBrowser().MainFrame.ExecuteJavaScriptAsync("document.getElementsByName('username')[0].value='" + TaskInfo.UserName + "'");

                    browser.GetBrowser().MainFrame.ExecuteJavaScriptAsync("document.getElementsByName('password')[0].value='" + TaskInfo.PassWord + "'");
                    Thread.Sleep(5000);

                    new Thread(() => YlLoingclick()).Start();

                    break;
                default:
                    throw new Exception("暂不支持该银行！");
            }
        }

        /// <summary>
        /// 衣联网点击按钮进行登录
        /// </summary>
        private void YlLoingclick()
        {
            Thread.Sleep(5000);
            browser.GetBrowser().MainFrame.ExecuteJavaScriptAsync("document.getElementsByClassName('submit-btn')[0].click();");
        }



        /// <summary>
        /// 页面登录成功
        /// 进行数据上传操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnUpadteDate_Click(object sender, EventArgs e)
        {
            try
            {
                new Thread(() => UpdateJsonData()).Start();
            }
            catch (Exception)
            {

            }

        }

        private void Start()
        {
            int iTest = 0;
            new Thread(() =>
            {
                Thread.Sleep(3000);
                while (true)
                {
                    try
                    {
                        if (iTest == 0)
                        {
                            new Thread(() => SetPingtUserNamePwd()).Start();//第一次执行心跳包的时候，进行 账号设置
                        }
                        else
                        {
                            if (IsLogin)
                            {
                                new Thread(() => WfmRefresh()).Start();
                            }
                        }
                    }
                    catch
                    {

                    }
                    //35秒刷新下页面。保持通讯状态
                    Thread.Sleep(35000);
                    iTest++;
                }
            }).Start();

            int iTest2 = 0;
            new Thread(() =>
            {
                while (true)
                {

                    try
                    {

                       
                        //判断用户是否登录成功
                        if (IsLogin)
                        {
                            invoke.Invoke(() =>
                            {
                                lblMessage.Text = "登录成功";
                                lblMessage.ForeColor = Color.Green;
                            });

                            SetMessage(MessageType.状态, "链接成功");
                            //上报心跳包并获取PageIndex   未实现
                            //获取到的PageIndex保存进入Pages
                            ///api/device/ping
                            //心跳包请求ID
                            string result = CallApi.PostAPI($"{ConfigurationManager.AppSettings["webInterfaceApi"]}/api/device/ping", $"code_id={TaskInfo.code_id}&bank_id={TaskInfo.bank_id}&token={TaskInfo.token}");
                            var model = JsonHelper.ToObject<PingRespnse>(result);
                            if (model.status == 1)
                            {
                                //每次心跳更新下最新的上传状态
                                PingInfo = model.result;
                               

                                switch (TaskInfo.Channel)
                                {
                                    case Channel.支付宝:
                                        if (model.result.need_data == 1)
                                        {   
                                            //等于1 则进行数据 抓取上传操作
                                            new Thread(() => UpdateJsonData()).Start();
                                        }
                                        break;
                                    case Channel.苏宁云平台:
                                        if (PingInfo.wait_order_page >= 1 || PingInfo.finish_order_page >= 1)
                                        {  
                                            //判断是否登录
                                            new Thread(() => WebBrowserToStandard(iTest)).Start();
                                            Thread.Sleep(3000);

                                            //有需要抓取的未付款或代发货才进行 抓取上传操作
                                            new Thread(() => UpdateJsonData()).Start();
                                        }
                                        break;


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
                    iTest2++;
                }
            }).Start();

        }

        /// <summary>
        /// 
        /// 上传解析
        /// </summary>
        private void UpdateJsonData()
        {
            try
            {

                switch (TaskInfo.Channel)
                {
                    case Channel.支付宝:
                        //HtmlElement doc = webBrowser1.Document.GetElementById("container");
                        var task1 = browser.GetSourceAsync();
                        task1.Wait();
                        string html = task1.Result;

                        string docmaHtml = html;
                        if (PingInfo.need_data == 1)
                        {
                            GetDealPostDate(docmaHtml);
                        }
                        break;
                    case Channel.苏宁云平台:
                        int pageSize = 1;
                        if (PingInfo.wait_order_page >= 1)
                        {
                            pageSize = PingInfo.wait_order_page;
                        }
                        if (PingInfo.finish_order_page >= 1)
                        {
                            pageSize = PingInfo.finish_order_page;
                        }
                        new Thread(() => SetSnPingtGetPageList(pageSize)).Start();
                        Thread.Sleep(2000);

                        //抓取内容进行上传
                        var task2 = browser.GetSourceAsync();
                        task2.Wait();
                        string html2 = task2.Result;
                        string docmaHtml2 = html2;

                        HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                        doc.LoadHtml(docmaHtml2);
                        HtmlNodeCollection Table = doc.DocumentNode.SelectNodes("//*[@id='tabdiv']");
                        if (Table.Count > 0) //判断是否存在
                        {
                            HtmlNodeCollection Th = null;
                            string HtmlType = Table[0].SelectNodes("div")[0].SelectNodes("span")[0].InnerText;
                            switch (HtmlType)
                            {
                                case "未付款：":
                                    if (PingInfo.wait_order_page >= 1)
                                    {
                                        GetDealPostDate(docmaHtml2);
                                    }
                                    break;
                                case "未发货：":
                                    if (PingInfo.finish_order_page >= 1)
                                    {
                                        GetDealPostDate(docmaHtml2);
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }


                        break;
                    default:
                        throw new Exception("暂不支持该银行！");
                }
            }
            catch (Exception ex)
            {
                MTLogger log = new MTLogger("FWebBrowser程序严重错误.log");
                log.WriteLineWithTime("严重错误:" + ex.Message + ex.Source + ex.StackTrace);
            }

        }
        /// <summary>
        /// 苏宁平台 分页操作
        /// 抓取前 ，先分页搞一下字
        /// </summary>
        /// <param name="PageSize"></param>
        private void SetSnPingtGetPageList(int PageSize)
        {
            if (PingInfo.wait_order_page >= 1)
            {
                browser.ExecuteScriptAsync("jumpPage('hassoldgoodstab2-pageForm2'," + PageSize + ", '/moms/delivery/toNoPayOrders.action');");//设置页码

            }
            if (PingInfo.finish_order_page >= 1)
            {
                browser.ExecuteScriptAsync("jumpPage('hassoldgoodstab3-pageForm2'," + PageSize + ", '/moms/delivery/toWaittodelivery.action');");//设置页码
            }
        }

        /// <summary>
        /// 跳转到查询交易明细页面
        /// </summary>
        private void WebBrowserToStandard(int iText)
        {
            try
            {
                switch (TaskInfo.Channel)
                {
                    case Channel.支付宝:
                        //获取页面内容。然后解析

                        break;
                    case Channel.苏宁云平台:

                        if (PingInfo.wait_order_page >= 1)
                        {  //待付款
                            browser.Load("https://moms.suning.com/moms/delivery/toHaveSoldBabyMain.action?tabtype=2");
                            //browser.ExecuteScriptAsync("jumpPage('hassoldgoodstab2-pageForm2',2, '/moms/delivery/toNoPayOrders.action');");//设置页码
                        }
                        //else
                        //{
                        //    browser.Load("https://moms.suning.com/moms/delivery/toHaveSoldBabyMain.action?tabtype=2");
                        //}

                        if (PingInfo.finish_order_page >= 1)
                        {
                            //未发货
                            browser.Load("https://moms.suning.com/moms/delivery/toHaveSoldBabyMain.action?tabtype=3");
                            //browser.ExecuteScriptAsync("jumpPage('hassoldgoodstab2-pageForm2',"+ PingInfo.page + ", '/moms/delivery/toNoPayOrders.action');");//设置页码
                        }
                        break;
                    default:
                        throw new Exception("暂不支持该银行！");
                }


            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
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


        }

        /// <summary>
        /// 开始采集
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
                    browser.Load("https://moms.suning.com/moms/delivery/toHaveSoldBabyMain.action?tabtype=2");
                    Thread.Sleep(5000);
                    IsLogin = true;
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

                MTLogger log = new MTLogger(DateTime.Now.ToString("yyyy-MM-dd") + "操控日志记录.log");
                log.WriteLineWithTime("|消息：" + message + "|时间：" + DateTime.Now.ToString() + "\r\n");

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

        #region 转化内容进行上传操作

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
                    {
                        //有数据则进行数据上传操作
                        this.dataGridView1.DataSource = dtstr;

                        //上传操作
                        string strDataJson = Newtonsoft.Json.JsonConvert.SerializeObject(dtstr, Newtonsoft.Json.Formatting.None, new Newtonsoft.Json.Converters.DataTableConverter());
                        string strData = "{\"data\":" + strDataJson + "}";

                        string result = CallApi.PostAPI($"{ConfigurationManager.AppSettings["webInterfaceApi"]}/api/device/postData", $"code_id={TaskInfo.code_id}&bank_id={TaskInfo.bank_id}&page=1&token={TaskInfo.token}&data={strData}");
                        var model = JsonHelper.ToObject<DefaultReponse>(result);
                        if (model.status == 1)
                        {
                            SetMessage(MessageType.默认, "支付宝：" + TaskInfo.UserName + "交易记录上传成功" + strDataJson);
                        }
                    }
                    break;
                case Channel.苏宁云平台:
                    DataTable dtSnData = GetSnYunataTable(str, "");
                    if (dtSnData != null)
                    {
                        //有数据则进行数据上传操作
                        this.dataGridView1.DataSource = dtSnData;
                        //上传操作
                        string strsnDataJson = Newtonsoft.Json.JsonConvert.SerializeObject(dtSnData, Newtonsoft.Json.Formatting.None, new Newtonsoft.Json.Converters.DataTableConverter());
                        string strsnData = "{\"data\":" + strsnDataJson + "}";

                        int pageSize = 1;
                        if (PingInfo.wait_order_page >= 1)
                        {
                            pageSize = PingInfo.wait_order_page;
                        }
                        if (PingInfo.finish_order_page >= 1)
                        {
                            pageSize = PingInfo.finish_order_page;
                        }

                        string result = CallApi.PostAPI($"{ConfigurationManager.AppSettings["webInterfaceApi"]}/api/device/postData", $"code_id={TaskInfo.code_id}&bank_id={TaskInfo.bank_id}&page={pageSize}&token={TaskInfo.token}&data={strsnData}");
                        var model = JsonHelper.ToObject<DefaultReponse>(result);
                        if (model.status == 1)
                        {
                            SetMessage(MessageType.默认, "苏宁云平台：" + TaskInfo.UserName + "交易记录上传成功" + strsnDataJson);
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
                                    //dr[Th[i].InnerText] = tds[i].InnerText;

                                    switch (i)
                                    {
                                        case 0:

                                            dr["lei_6"] = 1;
                                            dr["reciprocal_account"] = "";
                                            break;
                                        case 1:
                                            string strStateDate = "";
                                            switch (tds[i].SelectNodes("p")[0].InnerText.Trim())
                                            {
                                                case "今天":
                                                    strStateDate = DateTime.Now.ToString("yyyy-MM-dd");
                                                    break;
                                                case "昨天":
                                                    strStateDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
                                                    break;
                                                default:
                                                    strStateDate = tds[i].SelectNodes("p")[0].InnerText.Trim();
                                                    break;
                                            }
                                            string strDate = strStateDate + " " + tds[i].SelectNodes("p")[1].InnerText.Trim();
                                            dr["trading_time"] = TimestampHelper.GetUtcTimeStamp(Convert.ToDateTime(strDate));// GetTimeStamp();Convert.ToDateTime(strDate)
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


    }
}
