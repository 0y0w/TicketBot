using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace TicketBot
{
    public class CaptchaService : IDisposable
    {
        private Process _serverProcess;
        private readonly HttpClient _httpClient = new HttpClient();
        private const string ApiUrl = "http://127.0.0.1:8000/predict";
        
        public void StartServer(string exePath)
        {
            _serverProcess = new Process();
            _serverProcess.StartInfo.FileName = exePath;
            _serverProcess.StartInfo.UseShellExecute = false;
            _serverProcess.StartInfo.CreateNoWindow = true;
            _serverProcess.Start();
        }
        public void Stop()
        {
            try
            {
                // 執行 CMD 命令，taskkill /F 是強制，/IM 是指定名稱，/T 是連同子進程一起殺
                Process.Start(new ProcessStartInfo
                {
                    FileName = "taskkill",
                    Arguments = "/F /IM ddddocr.exe /T",
                    CreateNoWindow = true,
                    UseShellExecute = false
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("無法執行 taskkill: " + ex.Message);
            }
        }
        // 檢查服務是否準備好的方法
        public async Task<bool> WaitForReadyAsync(int timeoutSeconds = 10)
        {
            var sw = Stopwatch.StartNew();
            while (sw.Elapsed.TotalSeconds < timeoutSeconds)
            {
                try
                {
                    // 簡單測試連線
                    await _httpClient.GetAsync("http://127.0.0.1:8000/");
                    return true;
                }
                catch
                {
                    await Task.Delay(500);
                }
            }
            return false;
        }

        public async Task<string> Predict(byte[] imageBytes)
        {
            using (var content = new MultipartFormDataContent())
            {
                var byteContent = new ByteArrayContent(imageBytes);
                content.Add(byteContent, "file", "captcha.jpg");

                var response = await _httpClient.PostAsync(ApiUrl, content);
                var jsonStr = await response.Content.ReadAsStringAsync();
                return JObject.Parse(jsonStr)["result"]?.ToString();
            }
        }

        public void Dispose()
        {
            if (_serverProcess != null && !_serverProcess.HasExited)
                _serverProcess.Kill();
            _httpClient.Dispose();
        }
    }
}