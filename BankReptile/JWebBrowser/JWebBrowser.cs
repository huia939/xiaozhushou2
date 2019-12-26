using JWebBrowser.LoginSpider;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Entity;
using Common;
using JWebBrowserCommon.Interface;
using Gecko;

namespace JWebBrowser
{
    public partial class JWebBrowser : Form
    {
      
        private ActionInvoke invoke;

        private TaskInfo TaskInfo
        {
            get; set;
        }

        private ILoginSpiderInterface spider;

        public JWebBrowser(TaskInfo info, string args)
        {
            InitializeComponent();
            invoke = new ActionInvoke(this);
            TaskInfo = info;

            txtAccount.Text = info.UserName;
            txtPassword.Text = info.PassWord;
            txtPassword.TextBox.PasswordChar = '*';
        }

        private void Init()
        {
            //if (!WBMain.Init())
            //{
            //    MessageBox.Show("加载浏览器异常！");
            //    Process.GetCurrentProcess().Kill();
            //}
        }

        GeckoWebBrowser WBMain
        {
            get; set;
        }

        private void toolStripLabel3_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(WBMain.Url?.ToString()) && WBMain.Url.ToString() != "about:blank")
            {
                WBMain.Navigate(WBMain.Url?.ToString());
            }
            //else if (!string.IsNullOrEmpty(WBMain?.LastUrl))
            //{
            //    WBMain.Navigate(WBMain.LastUrl);
            //}
        }

        Dictionary<int, bool> Pages = new Dictionary<int, bool>();

        private void Start()
        {
            new Thread(() =>
            {
                Thread.Sleep(3000);

                while (true)
                {
                    try
                    {
                        Upload();
                    }
                    catch { }
                    Thread.Sleep(30000);
                }
            }).Start();

            new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        if (spider.IsLogin)
                        {
                            invoke.Invoke(() =>
                            {
                                lblMessage.Text = "登录成功";
                                lblMessage.ForeColor = Color.Green;
                            });

                            SetMessage(MessageType.状态, "登录成功");

                            //上报心跳包并获取PageIndex   未实现
                            //获取到的PageIndex保存进入Pages
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
                    catch { }
                    Thread.Sleep(10000);
                }
            }).Start();

        }

        private void JWebBrowser_Load(object sender, EventArgs e)
        {
            try
            {
                GeckoWebBrowser gecko = new GeckoWebBrowser();
                gecko.NoDefaultContextMenu = true; //禁用右键菜单
                gecko.Dock = DockStyle.Fill;

                gecko.Navigate("www.baidu.com");
                panel1.Controls.Add(gecko); //添加到窗口中

                WBMain = gecko;//Activator.CreateInstance(typeof(GeckoWebBrowser), "银联对账小助手专用浏览器") as GeckoWebBrowser;

                Init();

                ////panel1.Controls.Add(WBMain as Control);

                spider = LoginSpiderFactory.Create(TaskInfo, WBMain);


                //Start();
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private void Upload()
        {
            if (!spider.IsLogin)
                return;

            int pageIndex = 0;
            lock (Pages)
            {
                if (Pages.Count > 0)
                    pageIndex = Pages.Keys.First();
            }
            pageIndex = 2;
            if (pageIndex == 0)
                return;
            var result = spider.GetBankRecordByParameters(new BackTaskParameters { PageIndex = pageIndex });
            if (result.Success)
            {
                //上报数据 未开发
                //string upReuslt = CallApi.PostAPI($"{AppConfig.WebApi}/api/device/userLogin", $"account={txtUserName.Text}&password={txtPassWord.Text}");


                SetMessage(MessageType.默认, $"上报第{pageIndex}页账单数据成功！");
            }
        }

        private void SetMessage(MessageType messageType, string message)
        {
            if (messageType != MessageType.状态)
                message = $"账户{TaskInfo.UserName}提醒您：{message}";
        }

        private void AutoKill()
        {
            if (TaskInfo.StarProccessId > 0)
            {
                new Thread(() =>
                {
                    while (true)
                    {
                        try
                        {
                            var processes = Process.GetProcesses();
                            bool isfind = false;
                            foreach (var p in processes)
                            {
                                if (p.Id == TaskInfo.StarProccessId)
                                {
                                    isfind = true;
                                    break;
                                }
                            }
                            if (!isfind)
                            {
                                break;
                            }
                        }
                        catch { }
                        Thread.Sleep(3000);
                    }
                    invoke.Invoke(() => { this.Close(); });
                }).Start();
            }
        }

        private void JWebBrowser_FormClosing(object sender, FormClosingEventArgs e)
        {
            Process.GetCurrentProcess().Kill();
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
    }
}