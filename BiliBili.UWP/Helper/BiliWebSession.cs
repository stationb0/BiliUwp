using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

namespace BiliBili.UWP.Helper
{
    /// <summary>
    /// 全局共享的网页会话。
    ///
    /// 背景：网页端播放接口(x/player/playurl、pgc/player/web/playurl)是**认 cookie** 的，
    /// 只有请求里带上登录后的 SESSDATA，匿名 480P 的限制才会解除；
    /// 而 App 里保存的只有 access_key（客户端接口用），所以之前一直拿不到 1080P。
    ///
    /// 这个类负责：
    /// 1. 用一个固定的 HttpBaseProtocolFilter/HttpClient 作为全局会话（cookie 不会丢）；
    /// 2. 用 access_key 走 passport 的 sso 接口换取 SESSDATA 等 cookie 并持久化；
    /// 3. 之后所有网页端请求都复用这个会话，自动带上登录 cookie。
    /// </summary>
    public static class BiliWebSession
    {
        private static readonly object _lock = new object();
        private static HttpBaseProtocolFilter _filter;
        private static HttpClient _client;
        private static bool _ready;
        private static bool _ensuring;
        private static bool _buvidReady;

        public const string WebUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";

        /// <summary>
        /// 确保会话里有 buvid3/buvid4（B 站网页接口的风控标识，没有会被 -352 拦）
        /// 不登录也需要，排行榜、分区列表等接口都靠它
        /// </summary>
        public static async Task EnsureBuvidAsync()
        {
            if (_buvidReady)
            {
                return;
            }
            _buvidReady = true;
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, new Uri("https://api.bilibili.com/x/frontend/finger/spi"));
                request.Headers.UserAgent.TryParseAdd(WebUserAgent);
                request.Headers.Referer = new Uri("https://www.bilibili.com/");
                var response = await Client.SendRequestAsync(request);
                var text = await response.Content.ReadAsStringAsync();
                var obj = Newtonsoft.Json.Linq.JObject.Parse(text);
                if (obj["data"] != null)
                {
                    string buvid3 = obj["data"]["b_3"] == null ? null : obj["data"]["b_3"].ToString();
                    string buvid4 = obj["data"]["b_4"] == null ? null : obj["data"]["b_4"].ToString();
                    if (!string.IsNullOrEmpty(buvid3))
                    {
                        SetCookie("buvid3", buvid3);
                    }
                    if (!string.IsNullOrEmpty(buvid4))
                    {
                        SetCookie("buvid4", buvid4);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("获取 buvid 失败", LogType.ERROR, ex);
            }
        }

        public static HttpBaseProtocolFilter Filter
        {
            get
            {
                lock (_lock)
                {
                    if (_filter == null)
                    {
                        _filter = new HttpBaseProtocolFilter();
                        _filter.IgnorableServerCertificateErrors.Add(Windows.Security.Cryptography.Certificates.ChainValidationResult.Expired);
                    }
                    return _filter;
                }
            }
        }

        /// <summary>
        /// 全局共享的 HttpClient（复用同一个 filter，cookie 才不会丢）
        /// </summary>
        public static HttpClient Client
        {
            get
            {
                lock (_lock)
                {
                    if (_client == null)
                    {
                        _client = new HttpClient(Filter);
                    }
                    return _client;
                }
            }
        }

        /// <summary>
        /// 当前是否已经拿到登录 cookie
        /// </summary>
        public static bool HasLoginCookie
        {
            get { return !string.IsNullOrEmpty(SettingHelper.Get_SessData()); }
        }

        /// <summary>
        /// 把保存的 cookie 写进会话
        /// </summary>
        private static void ApplySavedCookies()
        {
            string sessdata = SettingHelper.Get_SessData();
            if (string.IsNullOrEmpty(sessdata))
            {
                return;
            }
            string dedeUserId = SettingHelper.Get_DedeUserID();
            string biliJct = SettingHelper.Get_BiliJct();
            SetCookie("SESSDATA", sessdata);
            if (!string.IsNullOrEmpty(dedeUserId))
            {
                SetCookie("DedeUserID", dedeUserId);
            }
            if (!string.IsNullOrEmpty(biliJct))
            {
                SetCookie("bili_jct", biliJct);
            }
        }

        private static void SetCookie(string name, string value)
        {
            // WinRT 的 HttpCookie 不接受带前导点的域名（会抛 0x800710DD），这里用不带点的域名
            foreach (var domain in new[] { "bilibili.com", "www.bilibili.com", "api.bilibili.com", "passport.bilibili.com" })
            {
                try
                {
                    Filter.CookieManager.SetCookie(new HttpCookie(name, domain, "/")
                    {
                        Value = value,
                        Expires = DateTimeOffset.Now.AddDays(30)
                    });
                }
                catch (Exception)
                {
                    // 某些域名不可写就跳过，其它域名仍会生效
                }
            }
            // 兜底再尝试一次带点的写法（老版本 WinRT 可能需要）
            try
            {
                Filter.CookieManager.SetCookie(new HttpCookie(name, ".bilibili.com", "/")
                {
                    Value = value,
                    Expires = DateTimeOffset.Now.AddDays(30)
                });
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// 用共享会话请求一个网页接口并返回文本（自动带 buvid / 登录 cookie）
        /// </summary>
        public static async Task<string> GetStringAsync(string url, string referer = "https://www.bilibili.com/")
        {
            await EnsureBuvidAsync();
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(url));
            request.Headers.UserAgent.TryParseAdd(WebUserAgent);
            request.Headers.Referer = new Uri(string.IsNullOrEmpty(referer) ? "https://www.bilibili.com/" : referer);
            request.Headers.Add("Origin", "https://www.bilibili.com");
            request.Headers.Add("Accept", "application/json, text/plain, */*");
            request.Headers.Add("Accept-Language", "zh-CN,zh;q=0.9");
            var response = await Client.SendRequestAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// 从会话里读出 cookie 并持久化（登录成功后调用）
        /// </summary>
        private static bool HarvestCookies()
        {
            bool found = false;
            foreach (var uri in new[] { "https://www.bilibili.com/", "https://api.bilibili.com/", "https://passport.bilibili.com/" })
            {
                try
                {
                    var cookies = Filter.CookieManager.GetCookies(new Uri(uri));
                    foreach (HttpCookie cookie in cookies)
                    {
                        if (string.Equals(cookie.Name, "SESSDATA", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(cookie.Value))
                        {
                            SettingHelper.Set_SessData(cookie.Value);
                            found = true;
                        }
                        else if (string.Equals(cookie.Name, "DedeUserID", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(cookie.Value))
                        {
                            SettingHelper.Set_DedeUserID(cookie.Value);
                        }
                        else if (string.Equals(cookie.Name, "bili_jct", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(cookie.Value))
                        {
                            SettingHelper.Set_BiliJct(cookie.Value);
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
            return found;
        }

        /// <summary>
        /// 用共享会话请求一下 nav 接口，确认 cookie 是否真的让网页端处于登录状态
        /// </summary>
        private static async Task<bool> CheckWebLoginAsync()
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, new Uri("https://api.bilibili.com/x/web-interface/nav"));
                request.Headers.UserAgent.TryParseAdd(WebUserAgent);
                var response = await Client.SendRequestAsync(request);
                var text = await response.Content.ReadAsStringAsync();
                var obj = Newtonsoft.Json.Linq.JObject.Parse(text);
                if (obj["data"] != null && obj["data"]["isLogin"] != null)
                {
                    return obj["data"]["isLogin"].ToObject<bool>();
                }
            }
            catch (Exception)
            {
            }
            return false;
        }

        /// <summary>
        /// 确保会话里已经有登录 cookie。
        /// 优先用本地保存的 SESSDATA；没有或者已失效的话，用 access_key 走 sso 接口换取。
        /// </summary>
        public static async Task<bool> EnsureLoginCookiesAsync(bool force = false)
        {
            // 先保证有 buvid（风控标识），不登录也需要
            await EnsureBuvidAsync();

            if (!force && _ready)
            {
                return HasLoginCookie;
            }
            if (_ensuring)
            {
                // 并发调用时直接等待一小会，避免重复请求
                for (int i = 0; i < 30; i++)
                {
                    await Task.Delay(100);
                    if (_ready)
                    {
                        return HasLoginCookie;
                    }
                }
                return HasLoginCookie;
            }
            _ensuring = true;
            try
            {
                _ready = true;

                // 1. 本地已有 SESSDATA，先塞进会话并验证是否还有效
                if (HasLoginCookie && !force)
                {
                    ApplySavedCookies();
                    if (await CheckWebLoginAsync())
                    {
                        return true;
                    }
                    // 保存的 cookie 已失效，继续走下面的重新换取流程
                    LogHelper.WriteLog("保存的网页 cookie 已失效，尝试重新获取", LogType.INFO);
                }

                if (!ApiHelper.IsLogin())
                {
                    return false;
                }

                // 2. 用 access_key 换 cookie（依次尝试几个 passport 的 sso 接口）
                string accessKey = ApiHelper.access_key;
                if (string.IsNullOrEmpty(accessKey))
                {
                    return false;
                }

                var query = $"access_key={accessKey}&appkey={ApiHelper.AndroidKey.Appkey}&build={ApiHelper.build}&mobi_app=android&platform=android&ts={ApiHelper.GetTimeSpan}";
                var sign = ApiHelper.GetSign(query);
                var ssoUrls = new[]
                {
                    $"https://passport.bilibili.com/api/login/sso?{query}&gourl=https%3A%2F%2Fwww.bilibili.com&sign={sign}",
                    $"https://passport.biligame.com/api/login/sso?{query}&gourl=https%3A%2F%2Fwww.bilibili.com&sign={sign}",
                    $"https://passport.bilibili.com/api/v2/sso?{query}&sign={sign}",
                };

                bool found = false;
                foreach (var ssoUrl in ssoUrls)
                {
                    if (found)
                    {
                        break;
                    }
                    try
                    {
                        using (var request = new HttpRequestMessage(HttpMethod.Get, new Uri(ssoUrl)))
                        {
                            request.Headers.UserAgent.TryParseAdd(WebUserAgent);
                            request.Headers.Referer = new Uri("https://www.bilibili.com/");
                            var response = await Client.SendRequestAsync(request);
                            await response.Content.ReadAsStringAsync();
                        }
                        found = HarvestCookies();
                    }
                    catch (Exception ex)
                    {
                        LogHelper.WriteLog("access_key 换取 cookie 失败:" + ssoUrl.Split('?')[0], LogType.ERROR, ex);
                    }
                }

                if (found)
                {
                    ApplySavedCookies();
                    bool isLogin = await CheckWebLoginAsync();
                    LogHelper.WriteLog("已获取网页登录 cookie，播放将使用登录清晰度（nav.isLogin=" + isLogin + "）", LogType.INFO);
                }
                else
                {
                    LogHelper.WriteLog("未能获取网页登录 cookie，播放清晰度将受限（匿名最高 720P）", LogType.INFO);
                }
                return found;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("初始化网页会话失败", LogType.ERROR, ex);
                return false;
            }
            finally
            {
                _ensuring = false;
            }
        }

        /// <summary>
        /// 从网页登录回调地址里解析 cookie（形如 ...?SESSDATA=xxx&amp;bili_jct=xxx&amp;DedeUserID=xxx）
        /// </summary>
        public static bool SaveCookiesFromUrl(string url)
        {
            try
            {
                if (string.IsNullOrEmpty(url))
                {
                    return false;
                }
                var uri = new Uri(url);
                var query = uri.Query.TrimStart('?');
                var dict = new Dictionary<string, string>();
                foreach (var pair in query.Split('&'))
                {
                    var index = pair.IndexOf('=');
                    if (index <= 0)
                    {
                        continue;
                    }
                    var key = pair.Substring(0, index);
                    var value = Uri.UnescapeDataString(pair.Substring(index + 1));
                    dict[key] = value;
                }
                string sessdata;
                if (!dict.TryGetValue("SESSDATA", out sessdata) || string.IsNullOrEmpty(sessdata))
                {
                    return false;
                }
                SettingHelper.Set_SessData(sessdata);
                string dedeUserId;
                if (dict.TryGetValue("DedeUserID", out dedeUserId))
                {
                    SettingHelper.Set_DedeUserID(dedeUserId);
                }
                string biliJct;
                if (dict.TryGetValue("bili_jct", out biliJct))
                {
                    SettingHelper.Set_BiliJct(biliJct);
                }
                ApplySavedCookies();
                _ready = true;
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("解析网页登录cookie失败", LogType.ERROR, ex);
                return false;
            }
        }

        /// <summary>
        /// 退出登录时清空会话
        /// </summary>
        public static void Clear()
        {
            try
            {
                foreach (var name in new[] { "SESSDATA", "DedeUserID", "DedeUserID__ckMd5", "bili_jct", "sid" })
                {
                    foreach (var domain in new[] { ".bilibili.com", "bilibili.com", "www.bilibili.com", "api.bilibili.com", "passport.bilibili.com" })
                    {
                        try
                        {
                            Filter.CookieManager.DeleteCookie(new HttpCookie(name, domain, "/"));
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            SettingHelper.Set_SessData("");
            SettingHelper.Set_DedeUserID("");
            SettingHelper.Set_BiliJct("");
            _ready = false;
        }
    }
}
