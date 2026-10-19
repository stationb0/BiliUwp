using BiliBili.UWP.Helper;
using BiliBili.UWP.Modules;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BiliBili.UWP.Api
{
    public class RankAPI
    {
        /// <summary>
        /// 排行榜（网页端 ranking/v2 接口，需要会话里的 buvid cookie 才不会被 -352 风控）
        /// </summary>
        /// <param name="rid">分区ID (0=全站)</param>
        /// <param name="type">1=全站，2=原创</param>
        /// <param name="day">日/周/月排行（网页端 v2 接口已不区分，保留参数兼容旧调用）</param>
        public async Task<List<RankItemModel>> GetRank(int rid, int type, int day = 1)
        {
            var list = new List<RankItemModel>();
            try
            {
                string typeStr = type == 2 ? "origin" : "all";
                string url = $"https://api.bilibili.com/x/web-interface/ranking/v2?rid={rid}&type={typeStr}";
                string text = await BiliWebSession.GetStringAsync(url, "https://www.bilibili.com/v/popular/rank/all");
                var obj = JObject.Parse(text);
                if (obj["code"] == null || obj["code"].ToObject<int>() != 0)
                {
                    LogHelper.WriteLog("排行榜接口返回失败:" + (obj["code"] ?? "") + " " + (obj["message"] ?? ""), LogType.ERROR);
                    return list;
                }
                var array = obj["data"]?["list"] as JArray;
                if (array == null)
                {
                    return list;
                }
                int i = 1;
                foreach (var item in array)
                {
                    var stat = item["stat"];
                    var owner = item["owner"];
                    var model = new RankItemModel()
                    {
                        rank = i++,
                        aid = item["aid"]?.ToString(),
                        title = item["title"]?.ToString(),
                        pic = item["pic"]?.ToString(),
                        author = owner?["name"]?.ToString(),
                        mid = owner?["mid"]?.ToString(),
                        duration = FormatDuration(item["duration"]),
                        play = stat?["view"]?.ToObject<int>() ?? 0,
                        video_review = stat?["danmaku"]?.ToObject<int>() ?? 0,
                        coins = stat?["coin"]?.ToObject<int>() ?? 0,
                        pts = (int)(item["score"]?.ToObject<double>() ?? 0)
                    };
                    list.Add(model);
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("读取排行榜失败", LogType.ERROR, ex);
            }
            return list;
        }

        private static string FormatDuration(JToken token)
        {
            try
            {
                if (token == null)
                {
                    return "";
                }
                int seconds = token.ToObject<int>();
                return TimeSpan.FromSeconds(seconds).ToString(@"mm\:ss");
            }
            catch (Exception)
            {
                return "";
            }
        }
    }
}
