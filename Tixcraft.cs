using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TicketBot
{
    public class TixCraft : IPlatform
    {
        private Form1 _ui;

        private bool _isRunning = false;
        private CancellationTokenSource _cts;
        private SemaphoreSlim _pauseSemaphore = new SemaphoreSlim(0, 1);
        private bool _isPaused = true;
        private CaptchaService _captchaService = new CaptchaService();

        private IBrowser _browser;
        private IPage _page;
        private bool _isProcessingCaptcha = false;
        private bool _disposed = false;

        public void Initialize(Form1 ui) => _ui = ui;

        public async Task btnPause_Click()
        {
            if (!_isRunning) return;

            _isPaused = !_isPaused;

            if (_isPaused)
            {
                // 取得號誌，這會讓後續的 WaitAsync 進入等待
                await _pauseSemaphore.WaitAsync();
                _ui.btnPause.Text = "繼續";
                _ui.Log("⏸️ 程式已暫停");
            }
            else
            {
                // 釋放號誌，讓迴圈繼續跑
                try
                {
                    _pauseSemaphore.Release();
                }
                catch (SemaphoreFullException)
                {

                }
                catch (ObjectDisposedException)
                {

                }
                _ui.btnPause.Text = "暫停";
                _ui.Log("▶️ 程式恢復執行！");
            }
        }
        public void btnStop_Click()
        {
            if (!_isRunning) return ;

            _ui.Log("🛑 正在停止程式...");
            _isRunning = false;

            _cts?.Cancel();

            if (_browser != null)
            {
                _browser.Dispose();
            }

            _ui.btnStart.Enabled = true;
            _ui.btnStop.Enabled = false;

            _ui.cmbPlatform.Enabled = true;
        }

        public async Task btnStart_Click()
        {
            _isRunning = true;
            _cts = new CancellationTokenSource();

            _ui.btnPause.Text = "開始搶票";

            _ui.Log("🚀 啟動程式...");
            _ui.btnStart.Enabled = false;
            _ui.btnStop.Enabled = true;

            _ui.cmbPlatform.Enabled = false;

            await RunBot();
        }

        private async Task RunBot()
        {
            // 下載並啟動瀏覽器
            var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();

            if (!_isRunning) return;
            try
            {
                _browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = false,
                    DefaultViewport = null,
                    Args = new[] {
                    "--start-maximized",
                    "--disable-blink-features=AutomationControlled"
                }
                });
                var pages = await _browser.PagesAsync();
                _page = pages[0];

                AttachDialogHandler(_page);
                AttachPageEvents(_page);

                await _page.SetUserAgentAsync("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
                await _page.EvaluateFunctionOnNewDocumentAsync(@"() => {
                    Object.defineProperty(navigator, 'webdriver', { get: () => false });
                }");
            }
            catch 
            {

            }

            await LoginFlow();
        }

        private async Task LoginFlow()
        {
            if (!_isRunning) return;

            _ui.Log("正在前往首頁...");
            try
            {
                await _page.GoToAsync("https://tixcraft.com/", WaitUntilNavigation.Networkidle2);
                await _page.WaitForSelectorAsync("a[href='#login']", new WaitForSelectorOptions { Timeout = 10000 });
                var loginTrigger = await _page.QuerySelectorAsync("a[href='#login']");

                await Task.Delay(1000);
                if (loginTrigger != null)
                {
                    _ui.Log("自動觸發登入選單...");
                    await loginTrigger.ClickAsync();

                    await Task.Delay(500);
                }
            }
            catch (Exception ex)
            {
                _ui.Log("自動點擊登入失敗 : " + ex.Message);
            }


            _ui.Log("⌛ 等待登入中...");
            bool isLoggedIn = false;
            while (_isRunning && !isLoggedIn && _page.Url.Split('#')[0] == "https://tixcraft.com/")
            {
                try
                {
                    isLoggedIn = await _page.EvaluateFunctionAsync<bool>(@"() => {
                        const userLink = document.querySelector('a.user-name');
                        return userLink !== null && userLink.innerText.trim().length > 0;
                    }");
                }
                catch { }
                

                if (!isLoggedIn)
                {
                    await Task.Delay(2000);
                }
                else
                {
                    _ui.Log("✅ 偵測到「會員帳戶」，登入成功！");
                }
            }

            await StartBooking();
        }

        private async Task StartBooking()
        {
            if (!_isRunning) return;

            string activityId = _ui.txtActivityId.Text;

            if (activityId.Length != 0)
            {
                await _page.GoToAsync($"https://tixcraft.com/activity/detail/{activityId}", WaitUntilNavigation.Networkidle2);
            }

            if (!_isPaused)
            {
                await _pauseSemaphore.WaitAsync();
                _isPaused = true;
            }
            await _page.WaitForSelectorAsync("a[href*='activity/game/']", new WaitForSelectorOptions { Timeout = 0 });

            while (_isRunning)
            {
                await _pauseSemaphore.WaitAsync();
                _pauseSemaphore.Release();

                if (!_isRunning) break;

                try
                {
                    if (_page.IsClosed) break;

                    var buyBtn = await _page.QuerySelectorAsync("a[href*='activity/game/']");
                    var orderBtn = await _page.QuerySelectorAsync("button[data-href*='/ticket/area/']");

                    string url = _page.Url;

                    if (orderBtn != null && url.Contains("activity/detail"))
                    {
                        _ui.Log("偵測到訂購按鈕，導向選區！");
                        await orderBtn.ClickAsync();
                    }
                    else if (buyBtn != null && url.Contains("activity/detail"))
                    {
                        _ui.Log("點擊購票按鈕...");
                        await buyBtn.ClickAsync();
                        await Task.Delay(300);
                    }
                    else if (url.Contains("ticket/area"))
                    {
                        await HandleAreaSelection();
                    }
                    else if (url.Contains("ticket/ticket"))
                    {
                        await SolveCaptcha();
                    }
                    else if (url.Contains("activity/detail"))
                    {
                        _ui.Log("⏳ 尚未開放，重新整理中...");
                        await Task.Delay(500);
                        await _page.ReloadAsync();
                    }
                    else if (url.Contains("ticket/order"))
                    {
                        _ui.Log("搶票完成，自動暫停程緒");
                        _ui.btnPause.PerformClick();
                    }
                }
                catch (Exception ex)
                {
                    _ui.Log($"⚠️ 偵測異常: {ex.Message}");
                }
            }
        }
        private async Task HandleAreaSelection()
        {
            string[] whiteList = _ui.txtPreference.Text.Split(new[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();
            string[] blackList = _ui.txtBlacklist.Text.Split(new[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();

            try
            {
                var areaItems = await _page.QuerySelectorAllAsync(".area-list li");

                if (areaItems == null || areaItems.Length == 0)
                {
                    _ui.Log("🔎 搜尋不到區域，執行重整...");
                    await _page.ReloadAsync();
                    return;
                }

                IElementHandle targetLink = null;
                string selectedName = "";

                foreach (var li in areaItems)
                {
                    var link = await li.QuerySelectorAsync("a");
                    var text = await _page.EvaluateFunctionAsync<string>("el => el.innerText", li);

                    if (link == null) continue;

                    var opacity = await _page.EvaluateFunctionAsync<string>("el => window.getComputedStyle(el).opacity", link);

                    if (opacity == "1")
                    {
                        if (blackList.Any(b => text.Contains(b))) continue;

                        if (whiteList.Length == 0 || whiteList.Any(w => text.Contains(w)))
                        {
                            targetLink = link;
                            selectedName = text;
                            break;
                        }
                    }
                }

                if (targetLink != null)
                {
                    _ui.Log($"🎯 成功鎖定: {selectedName.Trim()}");
                    await _page.EvaluateFunctionAsync("el => el.click()", targetLink);
                }
                else
                {
                    _ui.Log("⌛ 目前無符合條件的可用區域，持續監控中...");
                    await Task.Delay(500);
                    await _page.ReloadAsync();
                }
            }
            catch (Exception ex)
            {
                _ui.Log($"⚠️ 選區偵測異常: {ex.Message}");
            }
        }
        private async Task SolveCaptcha()
        {
            _ui.Log("📝 偵測到填表頁，啟動自動化流程...");

            if (_isProcessingCaptcha) return;
            _isProcessingCaptcha = true;

            try
            {
                // 選擇張數
                var selectSelector = "select[id^='TicketForm_ticketPrice_']";
                var quantity = _ui.txtQuantity.Text != "" ? _ui.txtQuantity.Text : "1";
                await _page.WaitForSelectorAsync(selectSelector);
                await _page.SelectAsync(selectSelector, quantity);
                _ui.Log($"🔹 已選擇張數: {quantity}");

                // 勾選同意條款 
                var agree = await _page.QuerySelectorAsync("#TicketForm_agree");
                if (agree != null)
                {
                    bool isChecked = await _page.EvaluateFunctionAsync<bool>("el => el.checked", agree);
                    if (!isChecked)
                    {
                        await _page.EvaluateFunctionAsync("el => el.click()", agree);
                        _ui.Log("✅ 已自動勾選同意條款");
                    }
                }

                // 執行辨識
                await DoCaptchaRecognition();
            }
            catch (Exception ex)
            {
                _ui.Log($"❌ 自動填表發生錯誤: {ex.Message}");
            }
            finally
            {
                await Task.Delay(500);
                _isProcessingCaptcha = false;
            }
        }
        private async Task DoCaptchaRecognition()
        {
            var imageSelector = "#TicketForm_verifyCode-image";

            try
            {
                await _page.WaitForSelectorAsync(imageSelector);

                byte[] imageBytes = await GetCaptchaBufferAsync();

                if (imageBytes != null)
                {
                    string result = await _captchaService.Predict(imageBytes);

                    if (!string.IsNullOrEmpty(result))
                    {
                        _ui.Log($"🤖 AI 辨識成功: {result}");

                        var inputSelector = "#TicketForm_verifyCode";


                        // 清空並輸入
                        await _page.WaitForSelectorAsync(inputSelector);
                        await _page.FocusAsync(inputSelector);
                        await _page.Keyboard.DownAsync("Control");
                        await _page.Keyboard.PressAsync("A");
                        await _page.Keyboard.UpAsync("Control");
                        await _page.Keyboard.PressAsync("Backspace");
                        await _page.TypeAsync(inputSelector, result);

                        _ui.Log("✅ 驗證碼已填入");

                        var submitSelector = "button.btn-green";

                        await _page.ClickAsync(submitSelector);
                        _ui.Log("🚀 已自動提交驗證碼...");
                    }
                }
            }
            catch (Exception ex)
            {
                _ui.Log($"⚠️ 辨識流程失敗: {ex.Message}");
                _isProcessingCaptcha = false;
            }
        }
        private async Task<byte[]> GetCaptchaBufferAsync()
        {
            try
            {
                string imgSelector = "#TicketForm_verifyCode-image";

                string base64String = await _page.EvaluateFunctionAsync<string>(@"(selector) => {
                    return new Promise((resolve) => {
                        const img = document.querySelector(selector);
                        if (!img) { resolve(null); return; }
            
                        fetch(img.src)
                            .then(res => res.blob())
                            .then(blob => {
                                const reader = new FileReader();
                                reader.onload = () => resolve(reader.result.split(',')[1]);
                                reader.readAsDataURL(blob);
                            })
                            .catch(() => resolve(null));
                    });
                }", imgSelector);

                if (string.IsNullOrEmpty(base64String))
                    throw new Exception("抓取到的圖片資料為空。");

                string cleanBase64 = base64String.Trim().Replace("\r", "").Replace("\n", "").Replace(" ", "");

                return Convert.FromBase64String(cleanBase64);
            }
            catch (Exception ex)
            {
                _ui.Log($"❌ 直取圖片發生錯誤: {ex.Message}");
                return null;
            }
        }
        private void AttachDialogHandler(IPage page)
        {
            page.Dialog += async (s, args) =>
            {
                try
                {
                    var dialog = args.Dialog;
                    _ui.Log($"⚠️ 偵測到網頁彈窗: {dialog.Message}");

                    await dialog.Accept();
                    _ui.Log("✅ 已自動處理彈窗，準備重新辨識...");

                    _isProcessingCaptcha = false;
                }
                catch (InvalidOperationException)
                {
                    _ui.Log("ℹ️ 彈窗已被網頁自動關閉，跳過處理...");
                }
            };
        }
        private void AttachPageEvents(IPage page)
        {
            page.Close += (s, e) => {
                _ui.btnStop.Invoke(new Action(() => _ui.btnStop.PerformClick()));
                _ui.Log("🚨 偵測到頁面已關閉！");
            };

            // 當頁面崩潰（例如記憶體不足或網頁錯誤）時觸發
            page.Error += (s, e) => {
                _ui.btnStop.Invoke(new Action(() => _ui.btnStop.PerformClick()));
                _ui.Log($"🚨 偵測到頁面錯誤");
            };
        }
        public void Dispose()
        {
            Dispose(true); GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                _isRunning = false;            
                _cts?.Cancel();
                try            
                {                
                    if (_browser != null)                
                    {                    
                        _browser.Dispose();                     
                        _browser = null;                
                    }            
                }            
                catch 
                { 

                }                        
                _pauseSemaphore?.Dispose();        
            }
                _disposed = true;
        }
    }
}
