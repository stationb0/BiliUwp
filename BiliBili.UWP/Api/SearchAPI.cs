using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliBili.UWP.Api
{
    /// <summary>
    /// 搜索接口。
    /// 统一走网页端 x/web-interface/search/type（需要 buvid/登录 cookie，走全局网页会话），
    /// 旧的 app.bilibili.com/x/v2/search/type 已失效，而且之前参数前面多拼了一个 '?' 导致 -400。
    /// </summary>
    public class SearchAPI
    {
        private ApiModel CreateSearchApi(string searchType, string keyword, int page, int pageSize, string extra = "")
        {
            var api = new ApiModel()
            {
                method = HttpMethod.GET,
                baseUrl = "https://api.bilibili.com/x/web-interface/search/type",
                parameter = $"search_type={searchType}&keyword={Uri.EscapeDataString(keyword ?? "")}&page={page}&page_size={pageSize}",
                useWebSession = true,
                webReferer = "https://search.bilibili.com/"
            };
            if (!string.IsNullOrEmpty(extra))
            {
                api.parameter += "&" + extra;
            }
            return api;
        }

        /// <summary>
        /// 视频搜索
        /// </summary>
        /// <param name="keyword">关键字</param>
        /// <param name="pn">页码</param>
        /// <param name="order">排序(totalrank/click/pubdate/dm/stow)</param>
        /// <param name="duration">时长(0全部/1&lt;10分钟/2 10-30/3 30-60/4 &gt;60)</param>
        /// <param name="region">分区tid，0=全部</param>
        public ApiModel WebSearchVideo(string keyword, int pn = 1, string order = "totalrank", string duration = "0", string region = "0")
        {
            string extra = $"order={(string.IsNullOrEmpty(order) ? "totalrank" : order)}&duration={(string.IsNullOrEmpty(duration) ? "0" : duration)}&tids={(string.IsNullOrEmpty(region) ? "0" : region)}";
            return CreateSearchApi("video", keyword, pn, 20, extra);
        }

        /// <summary>
        /// 番剧搜索
        /// </summary>
        public ApiModel WebSearchAnime(string keyword, int pn = 1)
        {
            return CreateSearchApi("media_bangumi", keyword, pn, 20);
        }

        /// <summary>
        /// 影视搜索
        /// </summary>
        public ApiModel WebSearchMovie(string keyword, int pn = 1)
        {
            return CreateSearchApi("media_ft", keyword, pn, 20);
        }

        /// <summary>
        /// 用户搜索
        /// </summary>
        public ApiModel WebSearchUser(string keyword, int pn = 1, string order = "&order=&order_sort=", string type = "&user_type=")
        {
            string extra = $"{order?.TrimStart('&')}{(string.IsNullOrEmpty(order) ? "" : "&")}{type?.TrimStart('&')}";
            return CreateSearchApi("bili_user", keyword, pn, 20, extra.TrimEnd('&'));
        }

        /// <summary>
        /// 直播搜索
        /// </summary>
        public ApiModel WebSearchLive(string keyword, int pn = 1)
        {
            return CreateSearchApi("live", keyword, pn, 20);
        }

        /// <summary>
        /// 专栏搜索
        /// </summary>
        public ApiModel WebSearchArticle(string keyword, int pn = 1, string order = "totalrank", string region = "0")
        {
            string extra = $"order={(string.IsNullOrEmpty(order) ? "totalrank" : order)}&category_id={(string.IsNullOrEmpty(region) ? "0" : region)}";
            return CreateSearchApi("article", keyword, pn, 20, extra);
        }

        /// <summary>
        /// 话题搜索
        /// </summary>
        public ApiModel WebSearchTopic(string keyword, int pn = 1)
        {
            return CreateSearchApi("topic", keyword, pn, 20);
        }
    }
}
