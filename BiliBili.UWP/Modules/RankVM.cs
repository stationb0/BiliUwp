using BiliBili.UWP.Api;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliBili.UWP.Modules
{
    public class RankVM : IModules
    {
        readonly Api.RankAPI rankAPI;
        public RankVM()
        {
            rankAPI = new Api.RankAPI();
            TypeFilter = new List<RankFilterItem>()
            {
                new RankFilterItem()
                {
                    id=1,
                    name="全站"
                },
                new RankFilterItem()
                {
                    id=2,
                    name="原创"
                }
            };
            SelectTypeFilter = TypeFilter[0];

            DayFilter = new List<RankFilterItem>() { 
                new RankFilterItem()
                {
                    id=1,
                    name="日排行"
                },
                new RankFilterItem()
                {
                    id=3,
                    name="3日排行"
                },
                new RankFilterItem()
                {
                    id=7,
                    name="周排行"
                },
                new RankFilterItem()
                {
                    id=30,
                    name="月排行"
                }
            };
            SelectDayFilter = DayFilter[0];

            List<RankRegionModel> regions = new List<RankRegionModel>() { 
                new RankRegionModel()
                {
                    name="全站",
                    rid=0,
                },
                new RankRegionModel()
                {
                    name="国创相关",
                    rid=168,
                }
            };
            if (ApiHelper.regions != null)
            {
                // 202(资讯)、85(剧情) 这两个分区网页端排行榜接口返回 -400，直接不展示
                var unsupported = new[] { 202, 85 };
                foreach (var item in ApiHelper.regions.Where(x => x.children != null && x.children.Count != 0
                    && x.tid != 13 && x.tid != 167 && x.name != "广告" && !unsupported.Contains(x.tid)))
                {
                    regions.Add(new RankRegionModel()
                    {
                        name = item.name,
                        rid = item.tid
                    });
                }
            }
            RegionItems = regions;
        }
        private bool _loading = true;
        public bool Loading
        {
            get { return _loading; }
            set { _loading = value; DoPropertyChanged("Loading"); }
        }
        private RankRegionModel _current;

        public RankRegionModel Current
        {
            get { return _current; }
            set { _current = value; DoPropertyChanged("Current"); }
        }
        private List<RankRegionModel> _RegionItems;
        public List<RankRegionModel> RegionItems
        {
            get { return _RegionItems; }
            set { _RegionItems = value; DoPropertyChanged("RegionItems"); }
        }

        private RankFilterItem _SelectDayFilter;
        public RankFilterItem SelectDayFilter
        {
            get { return _SelectDayFilter; }
            set { _SelectDayFilter = value; }
        }
        public List<RankFilterItem> DayFilter { get; set; }

        private RankFilterItem _SelectTypeFilter;
        public RankFilterItem SelectTypeFilter
        {
            get { return _SelectTypeFilter; }
            set { _SelectTypeFilter = value; }
        }
        public List<RankFilterItem> TypeFilter { get; set; }


        public async Task LoadRankDetail(RankRegionModel region)
        {
            try
            {
                Loading = true;
                var result = await rankAPI.GetRank(region.rid, SelectTypeFilter.id, SelectDayFilter.id);
                if (result != null && result.Count != 0)
                {
                    region.Items = result.Take(36).ToList();
                }
                else
                {
                    // 个别分区没有排行榜数据，静默显示空列表即可，不再弹提示
                    region.Items = new List<RankItemModel>();
                }
            }
            catch (Exception ex)
            {
                var handel = HandelError<ApiDataModel<List<RankRegionModel>>>(ex);
                Utils.ShowMessageToast(handel.message);
            }
            finally
            {
                Loading = false;
            }
        }
    }
    public class RankRegionModel : IModules
    {
        public string name { get; set; }
        public int rid { get; set; }

        private List<RankItemModel> _Items;
        public List<RankItemModel> Items
        {
            get { return _Items; }
            set { _Items = value; DoPropertyChanged("Items"); }
        }
    }
    public class RankFilterItem
    {
        public string name { get; set; }
        public int id { get; set; }
    }


    public class RankItemModel
    {
        public int rank { get; set; }
        public string aid { get; set; }
        public string author { get; set; }
        public string mid { get; set; }
        public int coins { get; set; }
        public int pts { get; set; }
        public string duration { get; set; }
        public string pic { get; set; }
        public string title { get; set; }
        public int video_review { get; set; }
        public int play { get; set; }
    }
}
