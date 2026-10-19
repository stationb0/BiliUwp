using BiliBili.UWP.Controls;
using BiliBili.UWP.Models;
using BiliBili.UWP.Modules;
using BiliBili.UWP.Pages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SYEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Streaming.Adaptive;
using Windows.UI.Popups;
using Windows.Web.Http;

namespace BiliBili.UWP.Helper
{
    //TODO 需要重写此类


    /// <summary>
    /// 视频地址解析类
    /// </summary>
    public static class PlayurlHelper
    {
        #region 网页端播放接口（无需 appkey / sign）

        private static string WebUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";

        private static int _lastLoggedQuality = -1;

        /// <summary>
        /// 记录一次播放清晰度诊断信息（清晰度变化时写一条日志，方便排查）
        /// </summary>
        private static void LogQualityOnce(int actualQuality, int videoQn)
        {
            try
            {
                int key = actualQuality * 1000 + Math.Min(videoQn, 999);
                if (key == _lastLoggedQuality)
                {
                    return;
                }
                _lastLoggedQuality = key;
                LogHelper.WriteLog($"播放诊断: 网页登录cookie={BiliWebSession.HasLoginCookie}, 请求清晰度={videoQn}, 服务器实际给出={actualQuality}", LogType.INFO);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// 请求一个返回 JSON 的网页端接口，失败返回 null
        /// </summary>
        private static async Task<JObject> GetWebJson(string url, string referer)
        {
            try
            {
                // 复用全局网页会话（带登录 cookie），否则播放接口只能拿到匿名清晰度
                await BiliWebSession.EnsureLoginCookiesAsync();
                var request = new Windows.Web.Http.HttpRequestMessage(Windows.Web.Http.HttpMethod.Get, new Uri(url));
                request.Headers.UserAgent.TryParseAdd(WebUserAgent);
                request.Headers.Referer = new Uri(string.IsNullOrEmpty(referer) ? "https://www.bilibili.com/" : referer);
                request.Headers.Add("Origin", "https://www.bilibili.com");
                request.Headers.Add("Accept", "application/json, text/plain, */*");
                request.Headers.Add("Accept-Language", "zh-CN,zh;q=0.9");
                var response = await BiliWebSession.Client.SendRequestAsync(request);
                response.EnsureSuccessStatusCode();
                string text = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(text))
                {
                    return null;
                }
                return JObject.Parse(text);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("请求播放接口失败:" + url, LogType.ERROR, ex);
                return null;
            }
        }

        /// <summary>
        /// 普通视频播放地址(网页端，登录态走 cookie)
        /// </summary>
        private static string BuildVideoPlayUrl(string aid, string cid, int qn, int fnval)
        {
            return $"https://api.bilibili.com/x/player/playurl?avid={aid}&cid={cid}&qn={qn}&type=&otype=json&fourk=1&fnver=0&fnval={fnval}";
        }

        /// <summary>
        /// 番剧播放地址(网页端，登录态走 cookie)
        /// </summary>
        private static string BuildBangumiPlayUrl(PlayerModel model, int qn, int fnval)
        {
            var url = $"https://api.bilibili.com/pgc/player/web/playurl?avid={model.Aid}&cid={model.Mid}&qn={qn}&type=&otype=json&module=bangumi&season_type={model.season_type}&fourk=1&fnver=0&fnval={fnval}";
            if (!string.IsNullOrEmpty(model.episode_id))
            {
                url += "&ep_id=" + model.episode_id;
            }
            return url;
        }

        /// <summary>
        /// 解析清晰度列表：优先 support_formats，其次 accept_quality
        /// </summary>
        private static List<QualityModel> ParseQualities(JObject data)
        {
            var list = new List<QualityModel>();
            if (data == null)
            {
                return list;
            }

            var support = data["support_formats"] as JArray;
            if (support != null && support.Count != 0)
            {
                foreach (var item in support)
                {
                    int qn = item["quality"] == null ? 0 : item["quality"].ToObject<int>();
                    string desc = item["new_description"] == null ? null : item["new_description"].ToString();
                    if (string.IsNullOrEmpty(desc) && item["display_desc"] != null)
                    {
                        desc = item["display_desc"].ToString();
                    }
                    if (string.IsNullOrEmpty(desc) && item["format"] != null)
                    {
                        desc = item["format"].ToString();
                    }
                    if (string.IsNullOrEmpty(desc))
                    {
                        desc = qn + "P";
                    }
                    if (qn != 0 && list.All(x => x.qn != qn))
                    {
                        list.Add(new QualityModel() { qn = qn, description = desc });
                    }
                }
            }

            if (list.Count == 0 && data["accept_quality"] is JArray acceptQuality)
            {
                var acceptDescription = data["accept_description"] as JArray;
                for (int i = 0; i < acceptQuality.Count; i++)
                {
                    int qn = acceptQuality[i].ToObject<int>();
                    string desc = (acceptDescription != null && acceptDescription.Count > i)
                        ? acceptDescription[i].ToString()
                        : qn + "P";
                    list.Add(new QualityModel() { qn = qn, description = desc });
                }
            }

            return list.OrderBy(x => x.qn).ToList();
        }

        /// <summary>
        /// 根据登录/大会员状态给清晰度加提示（不隐藏，服务器会按权限自动降级）
        /// </summary>
        private static List<QualityModel> ApplyQualityLimits(List<QualityModel> qualities)
        {
            if (qualities == null)
            {
                return GetDefaultQualities();
            }
            if (!ApiHelper.IsLogin())
            {
                foreach (var item in qualities.Where(x => x.qn > 64))
                {
                    item.description += "(需登录)";
                }
            }
            else if (!SettingHelper.Get_UserIsVip())
            {
                foreach (var item in qualities.Where(x => x.qn > 80))
                {
                    item.description += "(需大会员)";
                }
            }
            return qualities.OrderBy(x => x.qn).ToList();
        }

        /// <summary>
        /// 从 DASH 列表里挑选视频轨道
        /// </summary>
        private static DashItem PickDashVideo(List<DashItem> videos, int qn, int codecid)
        {
            if (videos == null || videos.Count == 0)
            {
                return null;
            }
            var video = videos.FirstOrDefault(x => x.id == qn && x.codecid == codecid);
            if (video == null)
            {
                video = videos.Where(x => x.codecid == codecid).OrderByDescending(x => x.id).FirstOrDefault();
            }
            if (video == null)
            {
                // 默认优先 AVC，兼容性最好
                video = videos.Where(x => x.codecid == 7).OrderByDescending(x => x.id).FirstOrDefault();
            }
            if (video == null)
            {
                video = videos.OrderByDescending(x => x.id).FirstOrDefault();
            }
            return video;
        }

        /// <summary>
        /// 从 DASH 列表里挑选音频轨道（优先高码率 m4a）
        /// </summary>
        private static DashItem PickDashAudio(List<DashItem> audios)
        {
            if (audios == null || audios.Count == 0)
            {
                return null;
            }
            var audio = audios.Where(x => x.mimeType == "audio/mp4").OrderByDescending(x => x.id).FirstOrDefault();
            if (audio == null)
            {
                audio = audios.OrderByDescending(x => x.id).FirstOrDefault();
            }
            return audio;
        }

        #endregion

        /// <summary>
        /// 读取番剧播放地址
        /// </summary>
        /// <param name="model"></param>
        /// <param name="quality"></param>
        /// <returns></returns>
        public static async Task<ReturnPlayModel> GetBangumiUrl(PlayerModel model, int qn)
        {
            try
            {
                //兼容旧代码
                if (qn < 10)
                {
                    switch (qn)
                    {
                        case 1:
                            qn = 32;
                            break;
                        case 2:
                            qn = 64;
                            break;
                        case 3:
                            qn = 80;
                            break;
                        case 4:
                            qn = 116;
                            break;
                        default:
                            qn = 64;
                            break;
                    }
                }

                // ===== 优先走网页端接口（不需要 appkey / sign）=====
                // 登录用户优先 DASH（能拿到 1080P 及以上），未登录先走 durl(MP4)
                if (SettingHelper.Get_UseDASH() && ApiHelper.IsLogin())
                {
                    var webDash = await GetBilibiliBangumiUrlDash(model, qn);
                    if (webDash != null)
                    {
                        return webDash;
                    }
                }
                var webFlv = await GetBilibiliBangumiUrl(model, qn);
                if (webFlv != null)
                {
                    return webFlv;
                }
                if (SettingHelper.Get_UseDASH())
                {
                    var webDash2 = await GetBilibiliBangumiUrlDash(model, qn);
                    if (webDash2 != null)
                    {
                        return webDash2;
                    }
                }
                if (SettingHelper.Get_PriorityBiliPlus())
                {
                    //biliplus API
                    var biliplus = await GetBiliPlus(model, qn);
                    if (biliplus != null)
                    {
                        return biliplus;
                    }
                    var moe = await Get23MoeUrl(model, qn);
                    if (moe != null)
                    {
                        return moe;
                    }
                }
                else
                {
                    //23moe API
                    var moe = await Get23MoeUrl(model, qn);
                    if (moe != null)
                    {
                        return moe;
                    }
                    var biliplus = await GetBiliPlus(model, qn);
                    if (biliplus != null)
                    {
                        return biliplus;
                    }
                }

                return null;

            }
            catch (Exception)
            {

                return null;
            }

        }
        public static async Task<ReturnPlayModel> GetBiliPlus(PlayerModel model, int qn)
        {
            if (SettingHelper.Get_UseDASH())
            {
                var biliplusdash = await GetBiliPlusDashUrl(model.Mid, qn, "https://www.bilibili.com/bangumi/play/ep" + model.episode_id, model.season_type);
                if (biliplusdash != null)
                {
                    return biliplusdash;
                }
            }

            var biliplus = await GetBiliPlusUrl(model.Mid, qn, "https://www.bilibili.com/bangumi/play/ep" + model.episode_id, model.season_type);
            if (biliplus != null)
            {
                return biliplus;
            }

            //biliplus API
            var biliplus2 = await GetBiliPlusUrl2(model);
            if (biliplus2 != null)
            {
                return biliplus2;
            }
            return null;
        }
        /// <summary>
        /// 番剧播放地址(durl/MP4，网页端接口，无需签名)
        /// </summary>
        public static async Task<ReturnPlayModel> GetBilibiliBangumiUrl(PlayerModel model, int qn)
        {
            try
            {
                List<string> urls = new List<string>();
                var playList = new SYEngine.Playlist(SYEngine.PlaylistTypes.NetworkHttp);
                string referer = "https://www.bilibili.com/bangumi/play/ep" + model.episode_id;
                var obj = await GetWebJson(BuildBangumiPlayUrl(model, qn, 0), referer);
                if (obj != null && obj["code"] != null && obj["code"].ToObject<int>() == 0)
                {
                    var durl = obj["result"]?["durl"] as JArray;
                    if (durl != null && durl.Count != 0)
                    {
                        foreach (var item in durl)
                        {
                            string videoUrl = item["url"]?.ToString();
                            if (string.IsNullOrEmpty(videoUrl))
                            {
                                continue;
                            }
                            urls.Add(videoUrl);
                            playList.Append(videoUrl, 0, (item["length"]?.ToObject<int>() ?? 0) / 1000);
                        }
                        playList.NetworkConfigs = CreatePlaylistNetworkConfigs(referer);
                        return new ReturnPlayModel()
                        {
                            playlist = playList,
                            usePlayMode = UsePlayMode.SYEngine,
                            urls = urls,
                            from = "bilibili"
                        };
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("获取番剧播放地址失败", LogType.ERROR, ex);
                return null;
            }
        }

        /// <summary>
        /// 番剧播放地址(DASH，网页端接口，无需签名)
        /// </summary>
        public static async Task<ReturnPlayModel> GetBilibiliBangumiUrlDash(PlayerModel model, int qn)
        {
            try
            {
                string referer = "https://www.bilibili.com/bangumi/play/ep" + model.episode_id;
                var obj = await GetWebJson(BuildBangumiPlayUrl(model, qn, 16), referer);
                if (obj == null || obj["code"] == null || obj["code"].ToObject<int>() != 0)
                {
                    return null;
                }
                var dash = obj["result"]?["dash"];
                if (dash == null)
                {
                    return null;
                }
                int codecid = 7;
                if (SettingHelper.Get_DASHUseHEVC())
                {
                    codecid = 12;
                }
                var videos = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DashItem>>(dash["video"].ToString());
                var audios = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DashItem>>(dash["audio"].ToString());
                var video = PickDashVideo(videos, qn, codecid);
                var audio = PickDashAudio(audios);
                if (video == null || audio == null)
                {
                    return null;
                }
                return new ReturnPlayModel()
                {
                    usePlayMode = UsePlayMode.Dash,
                    mediaSource = await CreateAdaptiveMediaSource(video, audio),
                    from = "bilibili_dash_" + video.codecid
                };
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("获取番剧DASH地址失败", LogType.ERROR, ex);
                return null;
            }
        }
        public static async Task<ReturnPlayModel> GetBilibiliBangumiWebUrl(PlayerModel model, int qn)
        {
            try
            {
                List<string> urls = new List<string>();
                var playList = new SYEngine.Playlist(SYEngine.PlaylistTypes.NetworkHttp);
                string url2 = string.Format(
                    $"https://api.bilibili.com/pgc/player/web/playurl?avid={ model.Aid}&cid={model.Mid}&qn={qn}&type=&otype=json&ep_id={model.episode_id}&&module=bangumi&access_key={ApiHelper.access_key}");
                //url2 += "&sign=" + ApiHelper.GetSign_VIDEO(url2);
                var re = await WebClientClass.GetResultsUTF8Encode(new Uri(url2));
                JObject obj = JObject.Parse(re);

                //是否遇到了地区限制
                if (obj["code"].ToInt32() == 0 && !re.Contains("8986943"))
                {
                    foreach (var item in obj["result"]["durl"])
                    {
                        urls.Add(item["url"].ToString());
                        playList.Append(item["url"].ToString(), 0, 0);
                    }
                    playList.NetworkConfigs = CreatePlaylistNetworkConfigs("https://www.bilibili.com");
                    return new ReturnPlayModel()
                    {
                        playlist = playList,
                        usePlayMode = UsePlayMode.SYEngine,
                        urls = urls,
                        from = "bilibili"
                    };
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {

                return null;
            }
        }


        public static async Task<ReturnPlayModel> Get23MoeUrl(PlayerModel model, int qn)
        {
            try
            {
                var playList = new SYEngine.Playlist(SYEngine.PlaylistTypes.NetworkHttp);
                List<string> urls = new List<string>();
                string rnd = ApiHelper.GetTimeSpan.ToString();
               
                var re3 = await WebClientClass.GetResults(new Uri(string.Format("https://moe.nsapps.cn/api/v1/BiliAnimeUrl?animeid={0}&cid={1}&epid={2}&rnd={3}", model.banId, model.Mid, model.banInfo.episode_id, rnd)));
                JObject obj = JObject.Parse(re3);
                if (Convert.ToInt32(obj["code"].ToString()) == 0)
                {
                    if (obj["mode"].ToString() == "mp4")
                    {
                        urls.Add(obj["data"][0]["url"].ToString());
                        return new ReturnPlayModel()
                        {
                            usePlayMode = UsePlayMode.System,
                            url = obj["data"][0]["url"].ToString(),
                            urls = urls,
                            from = "server"
                        };
                    }
                    else
                    {
                        var returnPlayModel = new ReturnPlayModel()
                        {
                            usePlayMode = UsePlayMode.SYEngine,
                            from = "server"
                        };
                        foreach (var item in obj["data"])
                        {
                            playList.Append(item["url"].ToString(), item["size"].ToInt32(), item["length"].ToInt32() / 1000);
                            urls.Add(item["url"].ToString());
                        }
                        playList.NetworkConfigs = CreatePlaylistNetworkConfigs("https://www.bilibili.com/bangumi/play/ep" + model.episode_id);
                        returnPlayModel.playlist = playList;
                        returnPlayModel.urls = urls;
                        return returnPlayModel;
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {

                return null;
            }
        }
        /// <summary>
        /// 使用biliplus的接口，支持番剧/视频
        /// 需要授权
        /// </summary>
        /// <param name="model"></param>
        /// <param name="qn"></param>
        /// <returns></returns>
        public static async Task<ReturnPlayModel> GetBiliPlusUrl(string cid, int qn, string referer, int season_type, string cookie = "")
        {
            try
            {
                var season = "";
                if (season_type != 0)
                {
                    season = $"&module=bangumi&season_type={ season_type}";
                }
                string url = "https://www.biliplus.com/BPplayurl.php?cid=" + cid + $"&otype=json&type=&quality={qn}&qn={qn}{season}&access_key={ApiHelper.access_key}";
                Dictionary<string, string> header = new Dictionary<string, string>();
                //if (SettingHelper.Get_BiliplusCookie() != "")
                //{
                //    if (cookie == "")
                //    {
                //        cookie = SettingHelper.Get_BiliplusCookie();
                //    }
                //    header.Add("Cookie", cookie);
                //}

                string re = await WebClientClass.GetResults(new Uri(url));
                FlvPlyaerUrlModel m = JsonConvert.DeserializeObject<FlvPlyaerUrlModel>(re);
                if (m.code == 0)
                {
                    List<string> urls = new List<string>();
                    var playList = new SYEngine.Playlist(SYEngine.PlaylistTypes.NetworkHttp);
                    foreach (var item in m.durl)
                    {
                        urls.Add(item.url);
                        playList.Append(item.url, 0, item.length / 1000);
                    }
                    playList.NetworkConfigs = CreatePlaylistNetworkConfigs(referer);
                    return new ReturnPlayModel()
                    {
                        playlist = playList,
                        usePlayMode = UsePlayMode.SYEngine,
                        urls = urls,
                        from = "biliplus"
                    };
                }
                else if (m.code == -403)
                {
                    //if (ApiHelper.IsLogin())
                    //{
                    //    ReturnPlayModel returnPlayModel = null;
                    //    MessageDialog messageDialog = new MessageDialog("读取视频地址失败了，是否授权Biliplus后再试一次?");
                    //    messageDialog.Commands.Add(new UICommand("授权", async (e) =>
                    //    {
                    //        var _cookie = await Account.AuthBiliPlus();
                    //        if (_cookie != "")
                    //        {
                    //            returnPlayModel = await GetBiliPlusUrl(cid, qn, referer, season_type, _cookie);
                    //        }
                    //        else
                    //        {
                    //            Utils.ShowMessageToast("授权失败了");
                    //        }
                    //    }));
                    //    messageDialog.Commands.Add(new UICommand("取消"));
                    //    await messageDialog.ShowAsync();
                    //    return returnPlayModel;
                    //}
                    //else
                    //{
                    return null;
                    //}
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {

                return null;
            }



        }
        public static async Task<ReturnPlayModel> GetBiliPlusDashUrl(string cid, int qn, string referer, int season_type, string cookie = "")
        {
            try
            {
                var season = "";
                if (season_type != 0)
                {
                    season = $"&module=bangumi&season_type={ season_type}";
                }
                string url = "https://www.biliplus.com/BPplayurl.php?cid=" + cid + $"&otype=json&type=&quality={qn}&qn={qn}{season}&access_key={ApiHelper.access_key}&fourk=1&fnver=0&fnval=16&platfrom=android";
                Dictionary<string, string> header = new Dictionary<string, string>();
                if (SettingHelper.Get_BiliplusCookie() != "")
                {
                    if (cookie == "")
                    {
                        cookie = SettingHelper.Get_BiliplusCookie();
                    }
                    header.Add("Cookie", cookie.Replace("secure;", ""));
                }
                header.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.88 Safari/537.36 Edg/87.0.664.57");
                string re = await WebClientClass.GetResults(new Uri(url), header);
                var obj = JObject.Parse(re);
                if (obj["code"].ToInt32() == 0)
                {
                    if (obj["dash"] != null)
                    {
                        int codecid = 7;
                        if (SettingHelper.Get_DASHUseHEVC())
                        {
                            codecid = 12;
                        }
                        var videos = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DashItem>>(obj["dash"]["video"].ToString());
                        var audios = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DashItem>>(obj["dash"]["audio"].ToString());
                        var video = videos.FirstOrDefault(x => x.id == qn && x.codecid == codecid);
                        if (video == null && codecid == 12)
                        {
                            codecid = 7;
                            video = videos.FirstOrDefault(x => x.id == qn && x.codecid == codecid);
                        }
                        var audio = audios.Where(x => x.mimeType == "audio/mp4").FirstOrDefault();

                        return new ReturnPlayModel()
                        {
                            usePlayMode = UsePlayMode.Dash,
                            mediaSource = await CreateAdaptiveMediaSource(video, audio),
                            from = "bilibili_dash_" + codecid
                        };
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (obj["code"].ToInt32() == -403)
                {
                    //if (ApiHelper.IsLogin())
                    //{
                    //    ReturnPlayModel returnPlayModel = null;
                    //    MessageDialog messageDialog = new MessageDialog("读取视频地址失败了，是否授权Biliplus后再试一次?");
                    //    messageDialog.Commands.Add(new UICommand("授权", async (e) =>
                    //    {
                    //        var _cookie = await Account.AuthBiliPlus();
                    //        if (_cookie != "")
                    //        {
                    //            returnPlayModel = await GetBiliPlusDashUrl(cid, qn, referer, season_type, _cookie);
                    //        }
                    //        else
                    //        {
                    //            Utils.ShowMessageToast("授权失败了");
                    //        }
                    //    }));
                    //    messageDialog.Commands.Add(new UICommand("取消"));
                    //    await messageDialog.ShowAsync();
                    //    return returnPlayModel;
                    //}
                    //else
                    //{
                    return null;
                    //}
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {

                return null;
            }



        }

        public static async Task<ReturnPlayModel> GetBiliPlusUrl2(PlayerModel model)
        {
            try
            {
                List<string> urls = new List<string>();
                var playList = new SYEngine.Playlist(SYEngine.PlaylistTypes.NetworkHttp);
                string url2 = string.Format(
                    $"https://www.biliplus.com//BPplayurl.php?cid={model.Mid}&otype=json");
                //url2 += "&sign=" + ApiHelper.GetSign_VIDEO(url2);
                var re = await WebClientClass.GetResultsUTF8Encode(new Uri(url2));
                JObject obj = JObject.Parse(re);

                if (obj["code"].ToInt32() == 10004)
                {
                    if (ApiHelper.IsLogin())
                    {
                        ReturnPlayModel returnPlayModel = null;
                        MessageDialog messageDialog = new MessageDialog("读取视频地址失败了，是否授权Biliplus后再试一次?");
                        messageDialog.Commands.Add(new UICommand("授权", async (e) =>
                        {
                            var _cookie = await Account.AuthBiliPlus();
                            if (_cookie != "")
                            {
                                returnPlayModel = await GetBiliPlusUrl2(model);
                            }
                            else
                            {
                                Utils.ShowMessageToast("授权失败了");
                            }
                        }));
                        messageDialog.Commands.Add(new UICommand("取消"));
                        await messageDialog.ShowAsync();
                        return returnPlayModel;
                    }
                    else
                    {
                        return null;
                    }
                }

                //是否遇到了地区限制
                if (obj["code"].ToInt32() == 0 && !re.Contains("8986943"))
                {
                    foreach (var item in obj["durl"])
                    {
                        urls.Add(item["url"].ToString());
                        playList.Append(item["url"].ToString(), item["size"].ToInt32(), item["length"].ToInt32() / 1000);
                    }
                    playList.NetworkConfigs = CreatePlaylistNetworkConfigs("https://www.bilibili.com/bangumi/play/ep" + model.episode_id);
                    return new ReturnPlayModel()
                    {
                        playlist = playList,
                        usePlayMode = UsePlayMode.SYEngine,
                        urls = urls,
                        from = "bilibili"
                    };
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {

                return null;
            }
        }



        /// <summary>
        /// 读取视频播放地址
        /// </summary>
        /// <param name="model"></param>
        /// <param name="quality"></param>
        /// <returns></returns>
        public static async Task<ReturnPlayModel> GetVideoUrl(string aid, string cid, int qn)
        {
            try
            {
                if (qn < 10)
                {
                    switch (qn)
                    {
                        case 1:
                            qn = 32;
                            break;
                        case 2:
                            qn = 64;
                            break;
                        case 3:
                            qn = 80;
                            break;
                        case 4:
                            qn = 116;
                            break;
                        default:
                            qn = 64;
                            break;
                    }
                }

                // 登录用户优先 DASH（可到 1080P+），未登录优先 durl(MP4，匿名可到 720P)
                if (SettingHelper.Get_UseDASH() && ApiHelper.IsLogin())
                {
                    var dashFirst = await GetVideoUrlDASH(aid, cid, qn);
                    if (dashFirst != null)
                    {
                        return dashFirst;
                    }
                }

                var biliv1 = await GetVideoUrlV1(aid, cid, qn);
                if (biliv1 != null)
                {
                    return biliv1;
                }
                if (SettingHelper.Get_UseDASH())
                {
                    var dashFallback = await GetVideoUrlDASH(aid, cid, qn);
                    if (dashFallback != null)
                    {
                        return dashFallback;
                    }
                }
                var biliplus = await GetBiliPlusUrl(cid, qn, "https://www.bilibili.com/video/av" + aid + "/", 0);
                if (biliplus != null)
                {
                    return biliplus;
                }
                return null;

            }
            catch (Exception)
            {
                return null;
            }

        }
        public static async Task<ReturnPlayModel> GetVideoUrlDASH(string aid, string cid, int qn)
        {
            try
            {
                string referer = "https://www.bilibili.com/video/av" + aid + "/";
                var obj = await GetWebJson(BuildVideoPlayUrl(aid, cid, qn, 16), referer);
                if (obj == null || obj["code"] == null || obj["code"].ToObject<int>() != 0)
                {
                    return null;
                }
                var dash = obj["data"]?["dash"];
                if (dash == null)
                {
                    return null;
                }
                int codecid = 7;
                if (SettingHelper.Get_DASHUseHEVC())
                {
                    codecid = 12;
                }
                var videos = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DashItem>>(dash["video"].ToString());
                var audios = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DashItem>>(dash["audio"].ToString());
                var video = PickDashVideo(videos, qn, codecid);
                var audio = PickDashAudio(audios);
                if (video == null || audio == null)
                {
                    return null;
                }
                LogQualityOnce(obj["data"]?["quality"]?.ToObject<int>() ?? video.id, qn);
                return new ReturnPlayModel()
                {
                    usePlayMode = UsePlayMode.Dash,
                    mediaSource = await CreateAdaptiveMediaSource(video, audio),
                    from = "bilibili_dash_" + video.codecid
                };
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("获取视频DASH地址失败", LogType.ERROR, ex);
                return null;
            }
        }


        public static async Task<ReturnPlayModel> GetVideoUrlV1(string aid, string cid, int qn)
        {
            try
            {
                List<string> urls = new List<string>();
                string referer = "https://www.bilibili.com/video/av" + aid + "/";
                var obj = await GetWebJson(BuildVideoPlayUrl(aid, cid, qn, 1), referer);
                if (obj == null || obj["code"] == null || obj["code"].ToObject<int>() != 0)
                {
                    return null;
                }
                var durl = obj["data"]?["durl"] as JArray;
                if (durl == null || durl.Count == 0)
                {
                    return null;
                }
                foreach (var item in durl)
                {
                    string videoUrl = item["url"]?.ToString();
                    if (!string.IsNullOrEmpty(videoUrl))
                    {
                        urls.Add(videoUrl);
                    }
                }
                if (urls.Count == 0)
                {
                    return null;
                }
                LogQualityOnce(obj["data"]?["quality"]?.ToObject<int>() ?? 0, qn);
                return new ReturnPlayModel()
                {
                    usePlayMode = UsePlayMode.System,
                    url = urls.FirstOrDefault(),
                    urls = urls,
                    from = "bilibili"
                };
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("获取视频MP4地址失败", LogType.ERROR, ex);
                return null;
            }
        }
        public static async Task<string> GetSoHuPlayInfo(string mid, int quality)
        {
            try
            {
                string[] str = mid.Split('|');
                //http://bangumi.bilibili.com/player/web_api/playurl?cid=10506396&module=movie&player=1&quality=4&ts=1475587467&sign=12b256ad5510d558d07ddf5c4430cd56
                // string url = string.Format("http://bangumi.bilibili.com/player/web_api/playurl?cid={0}&module=movie&player=1&quality=4&ts={1}&appkey={2}", mid,ApiHelper.GetTimeSpen,ApiHelper._appkey_DONTNOT);

                string url = string.Format("http://api.tv.sohu.com/v4/video/info/{0}.json?api_key=1820c56b9d16bbe3381766192e134811&uid=ad99774cfadfe5ecf12457ec5085359a&poid=1&plat=12&sver=3.7.0&partner=419&sysver=10.0.10586.318&ts={1}&verify=43026f88247fcbe0c56411624bd1531e&passport=&aid={2}&program_id=", str[1], ApiHelper.GetTimeSpan_2, str[0]);

                string results = await WebClientClass.GetResults(new Uri(url));
                SohuModel model = JsonConvert.DeserializeObject<SohuModel>(results);

                if (model.status == 200)
                {

                    switch (quality)
                    {
                        case 1:
                            return model.data.url_nor + "&uid=1608111818273358&SOHUSVP=aaxZQgiYTy4uioObZPfLJCVK3BxYwluKsrZ-cpoyfEk&pt=1&prod=h5&pg=1&eye=0&cv=1.0.0&qd=68000&src=11050001&ca=4&cateCode=101&_c=1&appid=tv&oth=&cd=";
                        case 2:
                            return model.data.url_super + "&uid=1608111818273358&SOHUSVP=aaxZQgiYTy4uioObZPfLJCVK3BxYwluKsrZ-cpoyfEk&pt=1&prod=h5&pg=1&eye=0&cv=1.0.0&qd=68000&src=11050001&ca=4&cateCode=101&_c=1&appid=tv&oth=&cd=";
                        case 3:
                            return model.data.url_original + "&uid=1608111818273358&SOHUSVP=aaxZQgiYTy4uioObZPfLJCVK3BxYwluKsrZ-cpoyfEk&pt=1&prod=h5&pg=1&eye=0&cv=1.0.0&qd=68000&src=11050001&ca=4&cateCode=101&_c=1&appid=tv&oth=&cd=";
                        default:
                            return "";
                    }
                }
                else
                {
                    return "";
                }
            }
            catch (Exception)
            {
                return "";

            }
        }

        /// <summary>
        /// 读取下载地址
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static async Task<List<string>> GetVideoUrl_Download(DownloadTaskModel m)
        {
            try
            {
                List<string> urls = new List<string>();
                var qn = 32;
                switch (m.quality)
                {
                    case 1:
                        qn = 32;
                        break;
                    case 2:
                        qn = 64;
                        break;
                    case 3:
                        qn = 80;
                        break;
                    case 4:
                        qn = 112;
                        break;
                    default:
                        break;
                }

                //https://interface.bilibili.com/v2/playurl?cid=31452468&appkey=84956560bc028eb7&otype=json&type=&quality=112&qn=112&sign=38b1355368ee65d055c12b57bdb26e3a

                if (m.downloadMode == DownloadMode.Video)
                {
                    var videos = await GetVideoUrl(m.avid, m.cid, qn);
                    if (videos != null)
                    {
                        return videos.urls;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    var ban = await GetBangumiUrl(new PlayerModel()
                    {
                        Aid = m.avid,
                        banId = m.sid,
                        episode_id = m.epid,
                        banInfo = new Models.episodesModel()
                        {
                            episode_id = m.epid
                        },
                        index = m.epIndex,
                        Mid = m.cid
                    }, qn);
                    if (ban != null)
                    {
                        return ban.urls;
                    }
                    else
                    {
                        return null;
                    }
                }

            }
            catch (Exception)
            {
                return null;
            }





        }


        public static async Task<List<QualityModel>> GetVideoQualities(PlayerModel model, bool down = false)
        {
            try
            {
                int qn = 80;
                int fnval = 16;
                if (down && SettingHelper.Get_DownFLV())
                {
                    // 下载 FLV/MP4 时不请求 DASH
                    fnval = 0;
                }
                string referer = "https://www.bilibili.com/video/av" + model.Aid + "/";
                var obj = await GetWebJson(BuildVideoPlayUrl(model.Aid, model.Mid, qn, fnval), referer);
                if (obj != null && obj["code"] != null && obj["code"].ToObject<int>() == 0)
                {
                    var qualities = ParseQualities(obj["data"] as JObject);
                    if (qualities.Count != 0)
                    {
                        return ApplyQualityLimits(qualities);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("读取视频清晰度失败", LogType.ERROR, ex);
            }

            return GetDefaultQualities();
        }


        public static async Task<List<QualityModel>> GetAnimeQualities(PlayerModel model)
        {
            try
            {
                int qn = 80;
                int fnval = SettingHelper.Get_UseDASH() ? 16 : 0;
                string referer = "https://www.bilibili.com/bangumi/play/ep" + model.episode_id;
                var obj = await GetWebJson(BuildBangumiPlayUrl(model, qn, fnval), referer);
                if (obj != null && obj["code"] != null && obj["code"].ToObject<int>() == 0)
                {
                    var qualities = ParseQualities(obj["result"] as JObject);
                    if (qualities.Count != 0)
                    {
                        return ApplyQualityLimits(qualities);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("读取番剧清晰度失败", LogType.ERROR, ex);
            }

            return GetDefaultQualities();
        }

        public static List<QualityModel> GetDefaultQualities()
        {
            if (ApiHelper.IsLogin())
            {
                return new List<QualityModel>() {
                new QualityModel(){description="流畅", qn=16},
                new QualityModel(){description="清晰", qn=32},
                new QualityModel(){description="高清",qn=64},
                new QualityModel(){description="超清",qn=80}
            };
            }
            else
            {
                return new List<QualityModel>() {
                new QualityModel(){description="流畅(登录享受更多清晰度)", qn=16},
                new QualityModel(){description="清晰(登录享受更多清晰度)", qn=32},
            };
            }

        }

        public static async Task<HasSubtitleModel> GetHasSubTitle(string aid, string cid)
        {
            try
            {
                var url = $"https://api.bilibili.com/x/player.so?id=cid:{cid}&aid={aid}";
                var results = await WebClientClass.GetResults(new Uri(url));
                if (results.Contains("subtitle"))
                {
                    var json = Regex.Match(results, @"<subtitle>(.*?)</subtitle>").Groups[1].Value;
                    return JsonConvert.DeserializeObject<HasSubtitleModel>(json);
                }
                else
                {
                    return new HasSubtitleModel() { allow_submit = false };
                }
            }
            catch (Exception)
            {
                return new HasSubtitleModel() { allow_submit = false };
            }
        }
        public static async Task<SubtitleModel> GetSubtitle(string url)
        {
            try
            {
                if (!url.Contains("http:") || !url.Contains("https:"))
                {
                    url = "https:" + url;
                }
                var results = await WebClientClass.GetResults(new Uri(url));


                return JsonConvert.DeserializeObject<SubtitleModel>(results);

            }
            catch (Exception)
            {
                return null;
            }
        }

        private static async Task<IMediaSource> CreateAdaptiveMediaSource(DashItem video, DashItem audio)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Referer = new Uri("https://www.bilibili.com");
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/69.0.3497.100 Safari/537.36");
                var mpdStr = $@"<MPD xmlns=""urn:mpeg:DASH:schema:MPD:2011""  profiles=""urn:mpeg:dash:profile:isoff-on-demand:2011"" type=""static"">
  <Period  start=""PT0S"">
    <AdaptationSet>
      <ContentComponent contentType=""video"" id=""1"" />
      <Representation bandwidth=""{video.bandwidth}"" codecs=""{video.codecs}"" height=""{video.height}"" id=""{video.id}"" mimeType=""{video.mimeType}"" width=""{video.width}"">
        <BaseURL></BaseURL>
        <SegmentBase indexRange=""{video.SegmentBase.indexRange}"">
          <Initialization range=""{video.SegmentBase.Initialization}"" />
        </SegmentBase>
      </Representation>
    </AdaptationSet>
    <AdaptationSet>
      <ContentComponent contentType=""audio"" id=""2"" />
      <Representation bandwidth=""{audio.bandwidth}"" codecs=""{audio.codecs}"" id=""{audio.id}"" mimeType=""{audio.mimeType}"" >
        <BaseURL></BaseURL>
        <SegmentBase indexRange=""{audio.SegmentBase.indexRange}"">
          <Initialization range=""{audio.SegmentBase.Initialization}"" />
        </SegmentBase>
      </Representation>
    </AdaptationSet>
  </Period>
</MPD>
";

                var stream = new MemoryStream(Encoding.UTF8.GetBytes(mpdStr)).AsInputStream();
                var soure = await AdaptiveMediaSource.CreateFromStreamAsync(stream, new Uri(video.baseUrl), "application/dash+xml", httpClient);
                var s = soure.Status;
                soure.MediaSource.DownloadRequested += (sender, args) =>
                {
                    if (args.ResourceContentType == "audio/mp4")
                    {
                        args.Result.ResourceUri = new Uri(audio.baseUrl);
                    }
                };
                return soure.MediaSource;
            }
            catch (Exception)
            {
                return null;
            }

        }
        private static PlaylistNetworkConfigs CreatePlaylistNetworkConfigs(string referer)
        {
            SYEngine.PlaylistNetworkConfigs config = new SYEngine.PlaylistNetworkConfigs();
            config.DownloadRetryOnFail = true;
            config.HttpCookie = string.Empty;
            config.UniqueId = string.Empty;
            config.HttpUserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/69.0.3497.100 Safari/537.36";
            config.HttpReferer = referer;
            return config;
        }
    }
    class RUModel
    {
        public string url { get; set; }
        public long length { get; set; }
        public long size { get; set; }
    }


    public class H5PlyaerUrlModel
    {
        public string from { get; set; }

        public List<H5PlyaerUrlModel> durl { get; set; }
        public int order { get; set; }
        public string url { get; set; }




    }
    public class FlvPlyaerUrlModel
    {
        public string format { get; set; }
        public int code { get; set; }
        public int status { get; set; }
        public int vip_status { get; set; }
        public List<string> accept_description { get; set; }
        public List<int> accept_quality { get; set; }
        public FlvPlyaerUrlModel data { get; set; }
        public List<FlvPlyaerUrlModel> durl { get; set; }
        public int order { get; set; }
        public int length { get; set; }
        public long size { get; set; }
        public string url { get; set; }
        public string[] backup_url { get; set; }

    }
    public class DashModel
    {
        public string format { get; set; }
        public List<string> accept_description { get; set; }
        public List<int> accept_quality { get; set; }
        /// <summary>
        /// 时长，毫秒
        /// </summary>
        public int timelength { get; set; }
        public int video_codecid { get; set; }
        public DashDashModel dash { get; set; }
    }
    public class DashDashModel
    {
        public List<DashItemModel> video { get; set; }
        public List<DashItemModel> audio { get; set; }
        /// <summary>
        /// 时长，秒
        /// </summary>
        public int duration { get; set; }

    }
    public class DashItemModel
    {
        public int id { get; set; }
        public int bandwidth { get; set; }
        public string baseUrl { get; set; }
        public string base_url { get; set; }
        public List<string> backupUrl { get; set; }
        public List<string> backup_url { get; set; }
        public string mime_type { get; set; }
        public string mimeType { get; set; }
        public string codecs { get; set; }
        public int codecid { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public string frameRate { get; set; }
        public string frame_rate { get; set; }
        /// <summary>
        /// 计算平均帧数
        /// </summary>
        public string fps
        {
            get
            {
                try
                {
                    if (!string.IsNullOrEmpty(frameRate))
                    {
                        var values = frameRate.Split('/');
                        if (values.Length == 1)
                        {
                            return frameRate;
                        }
                        double r = Convert.ToDouble(values[0]);
                        double d = Convert.ToDouble(values[1]);
                        return (r / d).ToString("0.0");
                    }
                    else if (!string.IsNullOrEmpty(frame_rate))
                    {
                        var values = frame_rate.Split('/');
                        if (values.Length == 1)
                        {
                            return frame_rate;
                        }
                        double r = Convert.ToDouble(values[0]);
                        double d = Convert.ToDouble(values[1]);
                        return (r / d).ToString("0.0");
                    }
                    else
                    {
                        return "0";
                    }
                }
                catch (Exception)
                {
                    return "0";
                }

            }
        }

        public SegmentBase SegmentBase { get; set; }
        public SegmentBase segment_base { get; set; }
    }

    public class SegmentBase
    {
        public string Initialization { get; set; }
        public string indexRange { get; set; }

        public string initialization { get; set; }
        public string index_range { get; set; }
    }

    public class SetPlayMp4Model
    {
        public string type { get; set; }
        public int duration { get; set; }
        public string url { get; set; }
    }
    public class SetPlayModel
    {
        public string type { get; set; }
        public int duration { get; set; }
        public List<SetPlayUrlModel> segments { get; set; }
    }
    public class SetPlayUrlModel
    {
        public int duration { get; set; }
        public int filesize { get; set; }
        public string url { get; set; }
    }

    public class PlayParModel
    {
        public List<PlayerModel> viedeolist { get; set; }
        public int play { get; set; }
        public bool laod { get; set; }

    }
    public class SohuModel
    {
        public int status { get; set; }
        public string statusText { get; set; }
        public SohuModel data { get; set; }
        public string url_blue { get; set; }

        public string download_url { get; set; }
        public string url_high { get; set; }
        public string url_nor { get; set; }
        public string url_original { get; set; }
        public string url_super { get; set; }

        public string url_high_mp4 { get; set; }
        public string url_nor_mp4 { get; set; }
        public string url_original_mp4 { get; set; }
        public string url_super_mp4 { get; set; }
    }
    public class PlayerModel
    {
        public PlayMode Mode { get; set; }
        public string No { get; set; }
        public int index { get; set; }
        public string ImageSrc { get; set; }
        public string rich_vid { get; set; }
        public string Aid { get; set; }
        public string Mid { get; set; }
        public string Title { get; set; }
        public string VideoTitle { get; set; }
        public string episode_id { get; set; }
        public string Path { get; set; }
        public object Parameter { get; set; }

        public PlayMode playMode { get; set; }
        public List<string> videoList { get; set; }

        public string banId { get; set; }
        public episodesModel banInfo { get; set; }
        /// <summary>
        /// 是否互动视频
        /// </summary>
        public bool isInteraction { get; set; } = false;
        /// <summary>
        /// 互动视频分支ID
        /// </summary>
        public int node_id { get; set; } = 0;

        public int season_type { get; set; } = 1;

        public int? graph_version { get; set; } = 467;

        public bool is_dash { get; set; } = false;
    }

    public class DashItem
    {
        public string baseUrl { get; set; }
        public string bandwidth { get; set; }
        public string codecs { get; set; }
        public int codecid { get; set; }
        public string height { get; set; }
        public string width { get; set; }
        public int id { get; set; }
        public string mimeType { get; set; }
        public SegmentBase SegmentBase { get; set; }
    }

    public class HasSubtitleModel
    {
        public bool allow_submit { get; set; }
        public List<HasSubtitleItemModel> subtitles { get; set; }
    }

    public class HasSubtitleItemModel
    {
        public long id { get; set; }
        public string lan { get; set; }
        public string lan_doc { get; set; }
        public string subtitle_url { get; set; }
    }
    public class SubtitleModel
    {
        public double font_size { get; set; }
        public string font_color { get; set; }
        public double background_alpha { get; set; }
        public string background_color { get; set; }
        public string Stroke { get; set; }

        public List<SubtitleItemModel> body { get; set; }
    }
    public class SubtitleItemModel
    {
        public double from { get; set; }
        public double to { get; set; }
        public int location { get; set; }
        public string content { get; set; }
    }
    public class QualityModel
    {
        public int qn { get; set; }
        public string description { get; set; }
    }


    public enum UsePlayMode
    {
        /// <summary>
        /// 使用SYEngine播放
        /// </summary>
        SYEngine,
        /// <summary>
        /// 使用系统控件播放
        /// </summary>
        System,
        /// <summary>
        /// 使用Html5播放器播放，暂时无效
        /// </summary>
        Html5,
        /// <summary>
        /// 使用VLC播放
        /// </summary>
        VLC,
        Dash
    }
    public class ReturnPlayModel
    {
        public IMediaSource mediaSource { get; set; }
        public UsePlayMode usePlayMode { get; set; }
        public SYEngine.Playlist playlist { get; set; }
        public string url { get; set; }

        public string from { get; set; }

        /// <summary>
        /// 暂时用于测试
        /// </summary>
        public List<string> urls { get; set; }
    }
}
