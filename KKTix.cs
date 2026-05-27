using OpenQA.Selenium.Support.UI;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TicketBot
{
    public class KKTix : IPlatform
    {
        private Form1 _ui;

        private bool _isRunning = false;
        private CancellationTokenSource _cts;
        private SemaphoreSlim _pauseSemaphore = new SemaphoreSlim(0, 1);
        private bool _isPaused = true;

        private IBrowser _browser;
        private IPage _page;
        private bool _disposed = false;

        private bool _isLoggedIn = false;

        public void Initialize(Form1 ui) => _ui = ui;

        public async Task btnPause_Click()
        {
            if (!_isRunning) return;

            _isPaused = !_isPaused;

            if (_isPaused)
            {
                await _pauseSemaphore.WaitAsync();
                _ui.btnPause.Text = "繼續";
                _ui.Log("⏸️ 程式已暫停");
            }
            else
            {
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
            if (!_isRunning) return;

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

            _isPaused = true;
            _ui.btnPause.Text = "開始搶票";

            _ui.Log("🚀 啟動程式...");
            _ui.btnStart.Enabled = false;
            _ui.btnStop.Enabled = true;

            _ui.cmbPlatform.Enabled = false;

            _isLoggedIn = false;

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

            _ui.Log("正在前往登入...");
            try 
            {
                string activityId = _ui.txtActivityId.Text;

                if (activityId.Length != 0)
                {
                    await _page.GoToAsync($"https://kktix.com/users/sign_in?back_to=https://kktix.com/events/{activityId}/registrations/new", WaitUntilNavigation.Networkidle2);
                }
                else
                {
                    await _page.GoToAsync($"https://kktix.com/users/sign_in?back_to=https://kktix.com", WaitUntilNavigation.Networkidle2);
                }
            }
            catch (Exception ex)
            {
                _ui.Log("前往首頁失敗 : " + ex.Message);
            }

            _ui.Log("⌛ 等待登入中...");

            while (_isRunning && !_isLoggedIn)
            {
                try
                {
                    var loginButton = await _page.QuerySelectorAsync("a[href*='/users/sign_in']");
                    if (loginButton == null)
                    {
                        _ui.Log("✅ 偵測到會員登入成功！");
                        _isLoggedIn = true;
                    }
                }
                catch 
                { 

                }
            }

            await StartBooking();
        }

        private async Task StartBooking()
        {
            if (!_isRunning) return;

            if (!_isPaused)
            {
                await _pauseSemaphore.WaitAsync();
                _isPaused = true;
            }

            while (_isRunning && _isLoggedIn)
            {
                await _pauseSemaphore.WaitAsync();
                _pauseSemaphore.Release();

                if (!_isRunning) break;

                IElementHandle atd_form = null;

                try
                {
                    if (_page.IsClosed) break;


                    string url = _page.Url;

                    atd_form = await _page.QuerySelectorAsync(".attendee-form");

                    if ((url.Contains("register_intents/new") || url.Contains("registrations/new")) && atd_form != null)
                    {
                        _ui.Log("搶票完成，自動暫停程緒");
                        _ui.btnPause.PerformClick();
                    }
                    else if (url.Contains("register_intents/new") || url.Contains("registrations/new"))
                    {
                        await HandleAreaSelection();
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
                var areaItems = await _page.QuerySelectorAllAsync(".ticket-unit");

                if (areaItems == null || areaItems.Length == 0)
                {
                    _ui.Log("🔎 搜尋不到區域，執行重整...");
                    await _page.ReloadAsync();
                    return;
                }

                var quantity = _ui.txtQuantity.Text != "" ? _ui.txtQuantity.Text : "1";
                IElementHandle plusBtn = null;
                string selectedName = "";

                foreach (var li in areaItems)
                {
                    var text = await li.EvaluateFunctionAsync<string>(@"el => {
                    // 抓取名稱，並移除不必要的換行與多餘空白
                        const name = el.querySelector('.ticket-name').innerText.split('\n')[0].trim();
    
                        // 抓取價格，只選取價格文字，過濾掉 mobile-fee 的手續費文字
                        const priceEl = el.querySelector('.ticket-price span.ng-binding');
                        const price = priceEl ? priceEl.innerText.split('\n')[0].trim() : '0';
    
                        return name + ' | ' + price;
                    }");
                    _ui.Log($"偵測到票券: {text}");
                    plusBtn = await li.QuerySelectorAsync("button.plus");

                    if (plusBtn != null)
                    {
                        if (blackList.Any(b => text.Contains(b))) continue;

                        if (whiteList.Length == 0 || whiteList.Any(w => text.Contains(w)))
                        {
                            selectedName = text;
                            break;
                        }
                    }
                }

                if (selectedName != null)
                {
                    _ui.Log($"🎯 成功鎖定: {selectedName.Trim()}");
                    for (int i = 0; i < Convert.ToInt32(quantity); i++)
                    {
                        await plusBtn.ClickAsync();
                        await Task.Delay(100); 
                    }

                    var agree = await _page.QuerySelectorAsync("#person_agree_terms");

                    if (agree != null)
                    {
                        bool isChecked = await _page.EvaluateFunctionAsync<bool>("el => el.checked", agree);
                        if (!isChecked)
                        {
                            await _page.EvaluateFunctionAsync("el => el.click()", agree);
                            _ui.Log("✅ 已自動勾選同意條款");
                        }
                    }
                    try
                    {
                        var rgstBtn = ".register-new-next-button-area button.btn-primary";
                        await _page.EvaluateExpressionAsync(@"
                            const btn = document.querySelector('.register-new-next-button-area button');
                            if (btn && !btn.disabled) {
                                btn.click();
                            } else {
                                throw new Error('按鈕雖然存在但仍處於停用狀態');
                            }
                        ");
                        await _page.ClickAsync(rgstBtn);

                        _ui.btnPause.PerformClick();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("等待購票按鈕時發生錯誤: " + ex.Message);
                    }
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
