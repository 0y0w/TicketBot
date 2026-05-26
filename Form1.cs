using OpenQA.Selenium;
using OpenQA.Selenium.DevTools.V146.Storage;
using PuppeteerSharp;
using PuppeteerSharp.Input;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace TicketBot
{
    public partial class Form1 : Form
    {
        private CaptchaService _captchaService = new CaptchaService();
        private IPlatform _currentPlatform;

        public Form1()
        {
            InitializeComponent();
            UpdatePlatform();
        }

        private void cmbPlatform_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_currentPlatform != null)    
            {        
                _currentPlatform.Dispose();       
            }
            UpdatePlatform();
        }
        private void UpdatePlatform()
        {
            _currentPlatform?.Dispose();
            if (cmbPlatform.Text == "拓元購票 TixCraft") _currentPlatform = new TixCraft();
            else if (cmbPlatform.Text == "KKTix")  _currentPlatform = new KKTix();
            _currentPlatform?.Initialize(this);
        }
        private async void Form1_Load(object sender, EventArgs e)
        {
            _captchaService.StartServer(Path.Combine(Application.StartupPath, "ddddocr.exe"));

            // 等待服務就緒
            bool isReady = await _captchaService.WaitForReadyAsync();
            if (isReady)
                Log("OCR 服務已就緒！");
            else
                Log("OCR 服務啟動超時，請檢查防火牆或錯誤。");

        }
        private async void btnPause_Click(object sender, EventArgs e)
        {
            if (_currentPlatform == null) return;
            await _currentPlatform.btnPause_Click();
        }
        private void btnStop_Click(object sender, EventArgs e)
        {
            if (_currentPlatform == null) return;
            _currentPlatform.btnStop_Click();
        }

        private async void btnStart_Click(object sender, EventArgs e)
        {
            if (_currentPlatform != null)
            {
                await _currentPlatform.btnStart_Click();
            }
            else
            {
                Log("請先選擇購票平台!");
            }
        }
        public void Log(string message)
        {
            // 檢查視窗控制項是否已經建立，以及視窗是否正在關閉
            if (this.IsDisposed || !this.IsHandleCreated)
            {
                // 如果視窗已經關閉或還沒準備好，改用 Console 輸出，避免崩潰
                Console.WriteLine($"[Log Error]: {message}");
                return;
            }

            if (this.InvokeRequired)
            {
                try
                {
                    this.Invoke(new Action(() => Log(message)));
                }
                catch (ObjectDisposedException)
                {

                }
            }
            else
            {
                rtbLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\n");
                rtbLog.ScrollToCaret();
            }
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            // 1. 確保停止 OCR 服務
            _captchaService.Stop();
        }
    }
}