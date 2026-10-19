using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BiliBili.UWP.Helper;
using BiliBili.UWP.Models;

namespace BiliBili.UWP.Api
{
    /// <summary>
    /// 基于网页端公开接口(api.bilibili.com)的辅助类。
    /// 网页端接口不需要 appkey/sign，因而不会因为旧客户端 key 失效而不可用。
    /// 视频详情统一走 x/web-interface/view/detail，同时兼容 av号 / BV号。
    /// </summary>
    internal static class BiliWebApi
    {
        public const string WebUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";

        /// <summary>
        /// 网页端接口通用请求头
        /// </summary>
        public static IDictionary<string, string> WebHeaders()
        {
            return new Dictionary<string, string>
            {
                { "User-Agent", WebUserAgent },
                { "Referer", "https://www.bilibili.com/" },
                { "Origin", "https://www.bilibili.com" }
            };
        }

        /// <summary>
        /// 请求一个返回 JSON 的网页端接口，失败返回 null
        /// </summary>
        public static async Task<JObject> GetJson(string url)
        {
            try
            {
                var headers = WebHeaders();
                var dic = new Dictionary<string, string>(headers);
                string results = await WebClientClass.GetResults(new Uri(url), dic);
                if (string.IsNullOrWhiteSpace(results))
                {
                    return null;
                }
                return JObject.Parse(results);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("请求网页接口失败:" + url, LogType.ERROR, ex);
                return null;
            }
        }

        /// <summary>
        /// 尝试把用户输入/URL 解析成视频 id。
        /// </summary>
        /// <param name="raw">URL、av号、BV号或者一串数字</param>
        /// <param name="aid">解析出的 av 号(可能为空)</param>
        /// <param name="bvid">解析出的 BV 号(可能为空)</param>
        public static bool TryParseVideoId(string raw, out string aid, out string bvid)
        {
            aid = null;
            bvid = null;
            if (string.IsNullOrWhiteSpace(raw))
            {
                return false;
            }

            raw = raw.Trim();

            // 完整链接或裸 BV 号：真实 BV 号固定为 BV1 开头、共 12 位，
            // 这样能避免把 player_preload 之类参数里的加密串误当成 BV 号
            var bvMatch = Regex.Match(raw, @"[Bb][Vv](1[0-9A-Za-z]{9})(?![0-9A-Za-z])");
            if (bvMatch.Success)
            {
                bvid = "BV" + bvMatch.Groups[1].Value;
                return true;
            }

            // av 号
            var avMatch = Regex.Match(raw, @"(?:^|[^0-9A-Za-z])(?:av|AV)(\d+)");
            if (avMatch.Success)
            {
                aid = avMatch.Groups[1].Value;
                return true;
            }

            // 纯数字当作 av 号
            if (Regex.IsMatch(raw, @"^\d+$"))
            {
                aid = raw;
                return true;
            }

            // 只有 BV 号主体(11 位且以 1 开头、包含字母)的情况
            if (Regex.IsMatch(raw, @"^1[0-9A-Za-z]{9}$") && Regex.IsMatch(raw, @"[A-Za-z]"))
            {
                bvid = "BV" + raw;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 获取视频详情(包含 相关视频/标签/充电 等)，兼容 av 号与 BV 号。
        /// </summary>
        /// <param name="rawId">av号、BV号或者视频链接</param>
        /// <returns>失败返回 null</returns>
        public static async Task<VideoInfoModels> GetVideoInfo(string rawId)
        {
            string aid, bvid;
            if (!TryParseVideoId(rawId, out aid, out bvid))
            {
                return null;
            }

            string idQuery = !string.IsNullOrEmpty(bvid) ? "bvid=" + bvid : "aid=" + aid;

            // 优先使用 view/detail，可以一次拿到 视频信息 + 相关视频 + 标签
            var root = await GetJson("https://api.bilibili.com/x/web-interface/view/detail?" + idQuery);
            bool hasDetail = root != null && root["code"] != null && root["code"].ToObject<int>() == 0 && root["data"] != null;

            if (!hasDetail)
            {
                // 兼容旧行为：再试一次纯 view 接口
                root = await GetJson("https://api.bilibili.com/x/web-interface/view?" + idQuery);
                if (root == null || root["code"] == null || root["code"].ToObject<int>() != 0 || root["data"] == null)
                {
                    if (root != null)
                    {
                        LogHelper.WriteLog("获取视频信息失败:" + idQuery + " code=" + root["code"] + " msg=" + root["message"], LogType.ERROR);
                    }
                    return null;
                }

                var onlyView = root["data"].ToObject<VideoInfoModels>();
                NormalizeVideoInfo(onlyView);
                return onlyView;
            }

            var data = root["data"] as JObject;
            var viewToken = data["View"] as JObject ?? data;
            var info = viewToken.ToObject<VideoInfoModels>();
            if (info == null)
            {
                return null;
            }

            try
            {
                var related = data["Related"] as JArray;
                if (related != null && related.Count > 0)
                {
                    info.relates = related.ToObject<List<relatesModel>>();
                }
                var tags = data["Tags"] as JArray;
                if (tags != null && tags.Count > 0)
                {
                    info.tag = tags.ToObject<List<tagModel>>();
                }
                var elec = data["elec"];
                if (elec != null && elec.Type != JTokenType.Null)
                {
                    info.elec = elec.ToObject<elecModel>();
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("解析视频附加信息失败:" + idQuery, LogType.ERROR, ex);
            }

            // 争议内容提示
            var argue = viewToken["argue_info"]?["argue_msg"]?.ToString();
            if (!string.IsNullOrEmpty(argue))
            {
                info.argue_msg = argue;
            }

            NormalizeVideoInfo(info);
            return info;
        }

        /// <summary>
        /// 补齐网页接口没有直接返回、但界面需要的字段
        /// </summary>
        private static void NormalizeVideoInfo(VideoInfoModels info)
        {
            if (info == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(info.aid) && info.data != null)
            {
                info.aid = info.data.aid;
            }
            if (string.IsNullOrEmpty(info.bvid) && info.data != null)
            {
                info.bvid = info.data.bvid;
            }
            if (string.IsNullOrEmpty(info.short_link))
            {
                var shortLinkV2 = info.short_link_v2;
                if (!string.IsNullOrEmpty(shortLinkV2))
                {
                    info.short_link = shortLinkV2;
                }
                else if (!string.IsNullOrEmpty(info.bvid))
                {
                    info.short_link = "https://www.bilibili.com/video/" + info.bvid;
                }
                else if (!string.IsNullOrEmpty(info.aid))
                {
                    info.short_link = "https://www.bilibili.com/video/av" + info.aid;
                }
            }

            // 网页接口只对登录用户返回 req_user，未登录时给一个默认对象，
            // 避免界面上的关注按钮状态读取空引用
            if (info.req_user == null)
            {
                info.req_user = new req_userModel();
            }
        }

        /// <summary>
        /// 登录状态下尝试获取当前用户与该视频的关系(点赞/投币/收藏/关注)
        /// </summary>
        public static async Task TryFillUserRelation(VideoInfoModels info)
        {
            if (info == null || !ApiHelper.IsLogin() || string.IsNullOrEmpty(info.aid))
            {
                return;
            }
            try
            {
                var root = await GetJson("https://api.bilibili.com/x/web-interface/archive/relation?aid=" + info.aid + "&access_key=" + ApiHelper.access_key);
                if (root != null && root["code"] != null && root["code"].ToObject<int>() == 0 && root["data"] != null)
                {
                    var relation = root["data"].ToObject<req_userModel>();
                    if (relation != null)
                    {
                        info.req_user = relation;
                    }
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
