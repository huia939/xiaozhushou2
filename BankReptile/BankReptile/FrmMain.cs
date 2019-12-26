using Common;
using Common.Log;
using Common.Types;
using Entity;
using Entity.WebApiResponse;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BankReptile
{
    public partial class FrmMain : Form
    {
        public static GetBankListResponse model;
        public FrmMain()
        {
            InitializeComponent();

            MTLogger log = new MTLogger("token.log");
            Token = log.ReadContent();

            Bind();
        }

        private string Token
        {
            get; set;
        }

        private void Bind()
        {
            string result = CallApi.PostAPI($"{ConfigurationManager.AppSettings["webInterfaceApi"]}/api/device/getBankList", $"token={Token}");

            model = JsonHelper.ToObject<GetBankListResponse>(result);
            var modelGrid = JsonHelper.ToObject<GetBankGridView>(result);//DataGridView 显示指定列
            if (model == null)
            {
                MessageBox.Show("获取银行列表异常！接口返回为空。");
                this.Close();
                return;
            }

            if (model.status != 1)
            {
                MessageBox.Show(model.message);
                this.Close();
                return;
            }

            dgvBanks.DataSource = modelGrid.result;
            //for (int i = 3; i < dgvBanks.ColumnCount; i++)  //隐藏dataGridView1控件中不需要的列字段,从第3列开始隐藏
            //{
            //    dgvBanks.Columns[i].Visible = false;
            //}

        }

        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            Process.GetCurrentProcess().Kill();
        }

        private void dgvBanks_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            //MessageBox.Show("您单击的是第" + (e.RowIndex + 1) + "行第" + (e.ColumnIndex + 1) + "列！");
            //MessageBox.Show("单元格的内容是：" + dgvBanks.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString());
        }

        private void dgvBanks_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            //点击进行数据操作
            string rowIn = e.RowIndex.ToString();
            //如果打开了该行，则不再添加行数
            string strList = TxtHelper.GetTextValue("TxtDataBase.txt");
            string[] strLists = strList.Split('|');
            bool isOpen = false;
            for (int i = 0; i < strLists.Length; i++)
            {
                if (strLists[i] == rowIn)
                {   //存在已经打开过，则不再次打开
                    isOpen = true;
                    MessageBox.Show(dgvBanks.Rows[e.RowIndex].Cells[1].Value.ToString() + "(" + dgvBanks.Rows[e.RowIndex].Cells[0].Value.ToString() + ")" + "已经打开,暂不支持开启多窗口！");
                    return;
                }
            }

            if (!isOpen)
            {
                try
                {
                    string setNuaml = strList + e.RowIndex + "|"; // 更新已经打开过的数据
                    TxtHelper.SetTextValue(setNuaml, "TxtDataBase.txt");

                    string jsonTO = "{" +
                        "\"UserName\":\" " + model.result[e.RowIndex].code_login_username + "\"," +
                        "\"PassWord\":\"" + model.result[e.RowIndex].code_login_password + "\"," +
                        "\"code_id\":\"" + model.result[e.RowIndex].code_id + "\"," +
                        "\"code_name\":\"" + model.result[e.RowIndex].code_name + "\"," +
                        "\"bank_id\":\"" + model.result[e.RowIndex].bank_id + "\"," +
                        "\"bank_name\":\"" + model.result[e.RowIndex].bank_name + "\"," +
                        "\"code_ip_proxy_ip\":\"" + model.result[e.RowIndex].code_ip_proxy_ip + "\"," +
                        "\"token\":\"" + Token + "\"}";                     //"\"Channel\":\"Channel." + model.result[e.RowIndex].bank_name + "\"," +

                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.FileName = "FWebBrowser.exe"; //启动的应用程序名称
                    startInfo.Arguments = jsonTO;
                    startInfo.WindowStyle = ProcessWindowStyle.Normal;

                    Process p1 = new Process();
                    p1.StartInfo.UseShellExecute = false;

                    Process.Start(startInfo);


                }
                catch (Exception ex)
                {
                    throw;
                }
            }

        }
    }
}
