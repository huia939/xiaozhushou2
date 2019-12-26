using Common;
using Common.Log;
using Common.Types;
using Entity.WebApiResponse;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BankReptile
{
    public partial class FrmLogin : Form
    {
        public FrmLogin()
        {
            InitializeComponent();

            // 账号默认记住
            this.txtUserName.Text = ConfigurationManager.AppSettings["userName"];
            //如果记住密码为true 那么把值赋给文本框
            if (ConfigurationManager.AppSettings["rememberMe"].Equals("true"))
            {
                this.txtPassWord.Text = ConfigurationManager.AppSettings["passWord"];
                loginCheckBoxUne.Checked = true;
            }
            else
            {
                this.txtPassWord.Text = "";
                loginCheckBoxUne.Checked = false;
            }
      
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string result = CallApi.PostAPI($"{ConfigurationManager.AppSettings["webInterfaceApi"]}/api/device/userLogin", $"account={txtUserName.Text}&password={txtPassWord.Text}");
            var model = JsonHelper.ToObject<UserLoginResponse>(result);
            if(model == null)
            {
                MessageBox.Show("登录异常！接口返回为空。");
                return;
            }
            if(model.status != 1)
            {
                MessageBox.Show(model.message);
                return;
            }

            Configuration cfa = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            cfa.AppSettings.Settings["userName"].Value = txtUserName.Text; // 账号（默认记住）
            if (this.loginCheckBoxUne.Checked)
            {
                cfa.AppSettings.Settings["rememberMe"].Value = "true"; // 自动赋值
                cfa.AppSettings.Settings["passWord"].Value = txtPassWord.Text; // 密码
            }
            else
            {
                cfa.AppSettings.Settings["rememberMe"].Value = "false"; // 自动赋值
                cfa.AppSettings.Settings["passWord"].Value = txtPassWord.Text; // 密码
            }

            TxtHelper.SetTextValue("", "TxtDataBase.txt"); ;//重新登录后则把已经打开的程序 置为可再次打开
            cfa.Save(); // 保存数据

            MTLogger log = new MTLogger("token.log");
            log.WriteAll(model.result.token);

            this.Hide();

            FrmMain frmMain = new FrmMain();
            frmMain.Show();
        }

        private void loginCheckBoxUne_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
