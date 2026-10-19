using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Windows.UI.Popups;
using System.Text;
using System.Text.RegularExpressions;


// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace BiliBili.UWP.Pages
{
    public enum PartOrderBy
    {
        danmaku,
        view,
        senddate,
        reply,
        favorite
    }
    public enum Parts
    {
        douga,
        bangumi,
        music,
        dance,
        game,
        technology,
        life,
        kichiku,
        fashion,
        ent,
        movie,
        tv,
        ad,
        cn
    }
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class PartsPage : Page
    {
        public PartsPage()
        {
            this.InitializeComponent();
            TestClass.d1 = this.Resources["HomeTemplate"] as DataTemplate;
            TestClass.d2 = this.Resources["ItemsTemplate"] as DataTemplate;
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }
        int Part_Id = 1;
        /// <summary>
        /// 是否隐藏排序菜单（分区页的数据来自排行榜接口，不支持排序）
        /// </summary>
        bool _hideOrderBar = false;
        private void btn_back_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
           
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            await Task.Delay(200);
            if (e.NavigationMode == NavigationMode.New)
            {
                pivot.ItemsSource = null;
                GetSetting();
                pr_Laod.Visibility = Visibility.Visible;
                if ((e.Parameter as object[])[0] is Parts)
                {
                    await LoadPart((Parts)(e.Parameter as object[])[0]);
                }
                else
                {
                    await LoadPart((RegionModel)(e.Parameter as object[])[0]);
                }
                pr_Laod.Visibility = Visibility.Collapsed;
            }
        }
        private void GetSetting()
        {
            defu_Order = "senddate";

        }
        string defu_Order = "senddate";
        private async Task LoadPart(Parts parts)
        {
           
            com_bar.Visibility = Visibility.Collapsed;
            _hideOrderBar = false;
            List<PartModel> l = new List<PartModel>();
            // defu_Order = "default";
            switch (parts)
            {
                case Parts.douga:
                    top_txt_Header.Text = "动画区";
                    Part_Id = 1;
                    #region
                    {
                        PartModel p0 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "首页",
                            isHome = true,
                            Banner = await GetBanner(Part_Id),
                            DTs = await GetTuiJianDT(Part_Id),
                            leftVisibility = Visibility.Visible,
                            rightVisibility = Visibility.Visible,
                            grid_c_left = new GridLength(1, GridUnitType.Star),
                            grid_c_right = new GridLength(1, GridUnitType.Star),
                            grid_c_center = new GridLength(0, GridUnitType.Auto)
                        };

                        if (p0.Banner.Count != 0)
                        {
                            p0.homeBanner = p0.Banner[0];
                            p0.leftBanner = p0.Banner[p0.Banner.Count - 1];
                            p0.rightBanner = p0.Banner[p0.Banner.IndexOf(p0.homeBanner) + 1];

                        }

                        //this.fvRight.SelectedIndex = this.home_flipView.SelectedIndex + 1;

                        PartModel p1 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "综合",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 27,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(27),
                            VideoList = await GetVideos(27, defu_Order, 1, "")
                        };
                        if (p1.TagsList.Count != 0)
                        {
                            p1.SelectTag = p1.TagsList[0];
                        }
                        PartModel p2 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "MAD·AMV",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 24,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(24),
                            VideoList = await GetVideos(24, defu_Order, 1, "")
                        };
                        if (p2.TagsList.Count != 0)
                        {
                            p2.SelectTag = p2.TagsList[0];
                        }
                        PartModel p3 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "MMD·3D",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 25,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(25),
                            VideoList = await GetVideos(25, defu_Order, 1, "")
                        };
                        if (p3.TagsList.Count != 0)
                        {
                            p3.SelectTag = p3.TagsList[0];
                        }
                        PartModel p4 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "短片·手书·配音",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 47,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(47),
                            VideoList = await GetVideos(47, defu_Order, 1, "")
                        };
                        if (p4.TagsList.Count != 0)
                        {
                            p4.SelectTag = p4.TagsList[0];
                        }
                        l.Add(p0);
                        l.Add(p1);
                        l.Add(p2);
                        l.Add(p3);
                        l.Add(p4);
                    }
                    #endregion
                    break;
                case Parts.bangumi:
                    top_txt_Header.Text = "番剧区";
                    Part_Id = 13;
                    #region
                    {
                        PartModel p0 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "首页",
                            isHome = true,
                            Banner = await GetBanner(Part_Id),
                            DTs = await GetTuiJianDT(Part_Id),
                            leftVisibility = Visibility.Visible,
                            rightVisibility = Visibility.Visible,
                            grid_c_left = new GridLength(1, GridUnitType.Star),
                            grid_c_right = new GridLength(1, GridUnitType.Star),
                            grid_c_center = new GridLength(0, GridUnitType.Auto)
                        };

                        if (p0.Banner.Count != 0)
                        {
                            p0.homeBanner = p0.Banner[0];
                            p0.leftBanner = p0.Banner[p0.Banner.Count - 1];
                            p0.rightBanner = p0.Banner[p0.Banner.IndexOf(p0.homeBanner) + 1];

                        }
                        PartModel p1 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "连载动画",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 33,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(33),
                            VideoList = await GetVideos(33, defu_Order, 1, "")
                        };

                        if (p1.TagsList.Count != 0)
                        {
                            p1.SelectTag = p1.TagsList[0];
                        }
                        PartModel p2 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "完结动画",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 32,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(32),
                            VideoList = await GetVideos(32, defu_Order, 1, "")
                        };
                        if (p2.TagsList.Count != 0)
                        {
                            p2.SelectTag = p2.TagsList[0];
                        }
                        
                        PartModel p4 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "资讯",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 51,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(51),
                            VideoList = await GetVideos(51, defu_Order, 1, "")
                        };
                        if (p4.TagsList.Count != 0)
                        {
                            p4.SelectTag = p4.TagsList[0];
                        }
                        PartModel p5 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "官方延伸",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 152,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(152),
                            VideoList = await GetVideos(152, defu_Order, 1, "")
                        };
                        if (p5.TagsList.Count != 0)
                        {
                            p5.SelectTag = p5.TagsList[0];
                        }
                        l.Add(p0);
                        l.Add(p1);
                        l.Add(p2);
                      
                        l.Add(p4);
                        l.Add(p5);
                    }
                    #endregion
                    break;
                case Parts.music:
                    top_txt_Header.Text = "音乐区";
                    Part_Id = 3;
                    #region
                    {
                        PartModel p0 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "首页",
                            isHome = true,
                            Banner = await GetBanner(Part_Id),
                            DTs = await GetTuiJianDT(Part_Id),
                            leftVisibility = Visibility.Visible,
                            rightVisibility = Visibility.Visible,
                            grid_c_left = new GridLength(1, GridUnitType.Star),
                            grid_c_right = new GridLength(1, GridUnitType.Star),
                            grid_c_center = new GridLength(0, GridUnitType.Auto)
                        };

                        if (p0.Banner.Count != 0)
                        {
                            p0.homeBanner = p0.Banner[0];
                            p0.leftBanner = p0.Banner[p0.Banner.Count - 1];
                            p0.rightBanner = p0.Banner[p0.Banner.IndexOf(p0.homeBanner) + 1];

                        }

                        PartModel p1 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "原创音乐",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 28,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(28),
                            VideoList = await GetVideos(28, defu_Order, 1, "")
                        };

                        if (p1.TagsList.Count != 0)
                        {
                            p1.SelectTag = p1.TagsList[0];
                        }
                        PartModel p2 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "翻唱",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 31,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(31),
                            VideoList = await GetVideos(31, defu_Order, 1, "")
                        };
                        if (p2.TagsList.Count != 0)
                        {
                            p2.SelectTag = p2.TagsList[0];
                        }
                        PartModel p3 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "VOCALOID·UTAU",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 30,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(30),
                            VideoList = await GetVideos(30, defu_Order, 1, "")
                        };
                        if (p3.TagsList.Count != 0)
                        {
                            p3.SelectTag = p3.TagsList[0];
                        }
                        PartModel p4 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "演奏",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 59,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(59),
                            VideoList = await GetVideos(59, defu_Order, 1, "")
                        };
                        if (p4.TagsList.Count != 0)
                        {
                            p4.SelectTag = p4.TagsList[0];
                        }
                        PartModel p5 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "三次元音乐",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 29,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(29),
                            VideoList = await GetVideos(29, defu_Order, 1, "")
                        };
                        if (p5.TagsList.Count != 0)
                        {
                            p5.SelectTag = p5.TagsList[0];
                        }
                        PartModel p6 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "OP/ED/OST",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 54,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(54),
                            VideoList = await GetVideos(54, defu_Order, 1, "")
                        };
                        if (p6.TagsList.Count != 0)
                        {
                            p6.SelectTag = p6.TagsList[0];
                        }
                        PartModel p7 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "音乐选集",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 130,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(130),
                            VideoList = await GetVideos(130, defu_Order, 1, "")
                        };
                        if (p7.TagsList.Count != 0)
                        {
                            p7.SelectTag = p7.TagsList[0];
                        }
                        l.Add(p0);
                        l.Add(p1);
                        l.Add(p2);
                        l.Add(p3);
                        l.Add(p4);
                        l.Add(p5);
                        l.Add(p6);
                        l.Add(p7);
                    }
                    #endregion
                    break;
                case Parts.dance:
                    top_txt_Header.Text = "舞蹈区";
                    Part_Id = 129;
                    #region
                    {
                        PartModel p0 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "首页",
                            isHome = true,
                            Banner = await GetBanner(Part_Id),
                            DTs = await GetTuiJianDT(Part_Id),
                            leftVisibility = Visibility.Visible,
                            rightVisibility = Visibility.Visible,
                            grid_c_left = new GridLength(1, GridUnitType.Star),
                            grid_c_right = new GridLength(1, GridUnitType.Star),
                            grid_c_center = new GridLength(0, GridUnitType.Auto)
                        };
                        if (p0.Banner.Count != 0)
                        {
                            p0.homeBanner = p0.Banner[0];
                            p0.leftBanner = p0.Banner[p0.Banner.Count - 1];
                            p0.rightBanner = p0.Banner[p0.Banner.IndexOf(p0.homeBanner) + 1];

                        }
                        PartModel p1 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "宅舞",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 20,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(20),
                            VideoList = await GetVideos(20, defu_Order, 1, "")
                        };

                        if (p1.TagsList.Count != 0)
                        {
                            p1.SelectTag = p1.TagsList[0];
                        }
                        PartModel p2 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "三次元舞蹈",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 154,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(154),
                            VideoList = await GetVideos(154, defu_Order, 1, "")
                        };
                        if (p2.TagsList.Count != 0)
                        {
                            p2.SelectTag = p2.TagsList[0];
                        }
                        PartModel p3 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "舞蹈教程",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 156,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(156),
                            VideoList = await GetVideos(156, defu_Order, 1, "")
                        };
                        if (p3.TagsList.Count != 0)
                        {
                            p3.SelectTag = p3.TagsList[0];
                        }
                        l.Add(p0);
                        l.Add(p1);
                        l.Add(p2);
                        l.Add(p3);
                    }
                    #endregion
                    break;
                case Parts.game:
                    top_txt_Header.Text = "游戏区";
                    Part_Id = 4;
                    #region
                    {
                        PartModel p0 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "首页",
                            isHome = true,
                            Banner = await GetBanner(Part_Id),
                            DTs = await GetTuiJianDT(Part_Id),
                            leftVisibility = Visibility.Visible,
                            rightVisibility = Visibility.Visible,
                            grid_c_left = new GridLength(1, GridUnitType.Star),
                            grid_c_right = new GridLength(1, GridUnitType.Star),
                            grid_c_center = new GridLength(0, GridUnitType.Auto)
                        };
                        if (p0.Banner.Count != 0)
                        {
                            p0.homeBanner = p0.Banner[0];
                            p0.leftBanner = p0.Banner[p0.Banner.Count - 1];
                            p0.rightBanner = p0.Banner[p0.Banner.IndexOf(p0.homeBanner) + 1];

                        }
                        PartModel p1 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "单机联机",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 17,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(17),
                            VideoList = await GetVideos(17, defu_Order, 1, "")
                        };

                        if (p1.TagsList.Count != 0)
                        {
                            p1.SelectTag = p1.TagsList[0];
                        }
                        PartModel p2 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "网游·电竞",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 65,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(65),
                            VideoList = await GetVideos(65, defu_Order, 1, "")
                        };
                        if (p2.TagsList.Count != 0)
                        {
                            p2.SelectTag = p2.TagsList[0];
                        }
                        PartModel p3 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "音游",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 136,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(136),
                            VideoList = await GetVideos(136, defu_Order, 1, "")
                        };
                        if (p3.TagsList.Count != 0)
                        {
                            p3.SelectTag = p3.TagsList[0];
                        }
                        PartModel p4 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "Mugen",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 19,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(19),
                            VideoList = await GetVideos(19, defu_Order, 1, "")
                        };
                        if (p4.TagsList.Count != 0)
                        {
                            p4.SelectTag = p4.TagsList[0];
                        }
                        PartModel p5 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "GMV",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 121,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(121),
                            VideoList = await GetVideos(121, defu_Order, 1, "")
                        };
                        l.Add(p0);
                        l.Add(p1);
                        l.Add(p2);
                        l.Add(p3);
                        l.Add(p4);
                        l.Add(p5);
                    }
                    #endregion
                    break;
                case Parts.technology:
                    top_txt_Header.Text = "科技区";
                    Part_Id = 36;
                    #region
                    {
                        PartModel p0 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "首页",
                            isHome = true,
                            Banner = await GetBanner(Part_Id),
                            DTs = await GetTuiJianDT(Part_Id),
                            leftVisibility = Visibility.Visible,
                            rightVisibility = Visibility.Visible,
                            grid_c_left = new GridLength(1, GridUnitType.Star),
                            grid_c_right = new GridLength(1, GridUnitType.Star),
                            grid_c_center = new GridLength(0, GridUnitType.Auto)
                        };
                        if (p0.Banner.Count != 0)
                        {
                            p0.homeBanner = p0.Banner[0];
                            p0.leftBanner = p0.Banner[p0.Banner.Count - 1];
                            p0.rightBanner = p0.Banner[p0.Banner.IndexOf(p0.homeBanner) + 1];

                        }
                        PartModel p1 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "纪录片",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 37,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(37),
                            VideoList = await GetVideos(37, defu_Order, 1, "")
                        };

                        if (p1.TagsList.Count != 0)
                        {
                            p1.SelectTag = p1.TagsList[0];
                        }
                        PartModel p2 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "趣味科普人文",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 124,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(124),
                            VideoList = await GetVideos(124, defu_Order, 1, "")
                        };
                        if (p2.TagsList.Count != 0)
                        {
                            p2.SelectTag = p2.TagsList[0];
                        }
                        PartModel p3 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "野生技术协会",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 122,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(122),
                            VideoList = await GetVideos(122, defu_Order, 1, "")
                        };
                        if (p3.TagsList.Count != 0)
                        {
                            p3.SelectTag = p3.TagsList[0];
                        }
                        PartModel p4 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "演讲•公开课",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 39,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(39),
                            VideoList = await GetVideos(39, defu_Order, 1, "")
                        };
                        if (p4.TagsList.Count != 0)
                        {
                            p4.SelectTag = p4.TagsList[0];
                        }
                        PartModel p5 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "星海",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 96,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(96),
                            VideoList = await GetVideos(96, defu_Order, 1, "")
                        };
                        if (p5.TagsList.Count != 0)
                        {
                            p5.SelectTag = p5.TagsList[0];
                        }
                        PartModel p6 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "数码",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 95,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(95),
                            VideoList = await GetVideos(95, defu_Order, 1, "")
                        };
                        if (p6.TagsList.Count != 0)
                        {
                            p6.SelectTag = p6.TagsList[0];
                        }
                        PartModel p7 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "机械",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 98,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(98),
                            VideoList = await GetVideos(98, defu_Order, 1, "")
                        };
                        if (p7.TagsList.Count != 0)
                        {
                            p7.SelectTag = p7.TagsList[0];
                        }
                        l.Add(p0);
                        l.Add(p1);
                        l.Add(p2);
                        l.Add(p3);
                        l.Add(p4);
                        l.Add(p5);
                        l.Add(p6);
                        l.Add(p7);
                    }
                    #endregion
                    break;
                case Parts.life:
                    top_txt_Header.Text = "生活区";
                    Part_Id = 160;
                    #region
                    {
                        PartModel p0 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "首页",
                            isHome = true,
                            Banner = await GetBanner(Part_Id),
                            DTs = await GetTuiJianDT(Part_Id),
                            leftVisibility = Visibility.Visible,
                            rightVisibility = Visibility.Visible,
                            grid_c_left = new GridLength(1, GridUnitType.Star),
                            grid_c_right = new GridLength(1, GridUnitType.Star),
                            grid_c_center = new GridLength(0, GridUnitType.Auto)
                        };
                        if (p0.Banner.Count != 0)
                        {
                            p0.homeBanner = p0.Banner[0];
                            p0.leftBanner = p0.Banner[p0.Banner.Count - 1];
                            p0.rightBanner = p0.Banner[p0.Banner.IndexOf(p0.homeBanner) + 1];

                        }
                        PartModel p1 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "搞笑",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 138,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(138),
                            VideoList = await GetVideos(138, defu_Order, 1, "")
                        };

                        if (p1.TagsList.Count != 0)
                        {
                            p1.SelectTag = p1.TagsList[0];
                        }
                        PartModel p2 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "日常",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 21,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(21),
                            VideoList = await GetVideos(21, defu_Order, 1, "")
                        };
                        if (p2.TagsList.Count != 0)
                        {
                            p2.SelectTag = p2.TagsList[0];
                        }
                        PartModel p3 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "美食圈",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 76,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(76),
                            VideoList = await GetVideos(76, defu_Order, 1, "")
                        };
                        if (p3.TagsList.Count != 0)
                        {
                            p3.SelectTag = p3.TagsList[0];
                        }
                        PartModel p4 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "动物圈",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 75,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(75),
                            VideoList = await GetVideos(75, defu_Order, 1, "")
                        };
                        if (p4.TagsList.Count != 0)
                        {
                            p4.SelectTag = p4.TagsList[0];
                        }
                        PartModel p5 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "手工",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 161,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(161),
                            VideoList = await GetVideos(161, defu_Order, 1, "")
                        };
                        if (p5.TagsList.Count != 0)
                        {
                            p5.SelectTag = p5.TagsList[0];
                        }
                        PartModel p6 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "绘画",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 162,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(162),
                            VideoList = await GetVideos(162, defu_Order, 1, "")
                        };
                        if (p6.TagsList.Count != 0)
                        {
                            p6.SelectTag = p6.TagsList[0];
                        }
                        PartModel p7 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "运动",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 163,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(163),
                            VideoList = await GetVideos(163, defu_Order, 1, "")
                        };
                        if (p7.TagsList.Count != 0)
                        {
                            p7.SelectTag = p7.TagsList[0];
                        }
                        l.Add(p0);
                        l.Add(p1);
                        l.Add(p2);
                        l.Add(p3);
                        l.Add(p4);
                        l.Add(p5);
                        l.Add(p6);
                        l.Add(p7);
                    }
                    #endregion
                    break;
                case Parts.kichiku:
                    top_txt_Header.Text = "鬼畜区";
                    Part_Id = 119;
                    #region
                    {
                        PartModel p0 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "首页",
                            isHome = true,
                            Banner = await GetBanner(Part_Id),
                            DTs = await GetTuiJianDT(Part_Id),
                            leftVisibility = Visibility.Visible,
                            rightVisibility = Visibility.Visible,
                            grid_c_left = new GridLength(1, GridUnitType.Star),
                            grid_c_right = new GridLength(1, GridUnitType.Star),
                            grid_c_center = new GridLength(0, GridUnitType.Auto)
                        };
                        if (p0.Banner.Count != 0)
                        {
                            p0.homeBanner = p0.Banner[0];
                            p0.leftBanner = p0.Banner[p0.Banner.Count - 1];
                            p0.rightBanner = p0.Banner[p0.Banner.IndexOf(p0.homeBanner) + 1];

                        }
                        PartModel p1 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "鬼畜调教",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 22,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(22),
                            VideoList = await GetVideos(22, defu_Order, 1, "")
                        };

                        if (p1.TagsList.Count != 0)
                        {
                            p1.SelectTag = p1.TagsList[0];
                        }
                        PartModel p2 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "音MAD",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 26,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(26),
                            VideoList = await GetVideos(26, defu_Order, 1, "")
                        };
                        if (p2.TagsList.Count != 0)
                        {
                            p2.SelectTag = p2.TagsList[0];
                        }
                        PartModel p3 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "人力VOCALOID",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 126,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(126),
                            VideoList = await GetVideos(126, defu_Order, 1, "")
                        };
                        if (p3.TagsList.Count != 0)
                        {
                            p3.SelectTag = p3.TagsList[0];
                        }
                        PartModel p4 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "教程演示",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 127,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(127),
                            VideoList = await GetVideos(127, defu_Order, 1, "")
                        };
                        if (p4.TagsList.Count != 0)
                        {
                            p4.SelectTag = p4.TagsList[0];
                        }
                        l.Add(p0);
                        l.Add(p1);
                        l.Add(p2);
                        l.Add(p3);
                        l.Add(p4);
                    }
                    #endregion
                    break;
                case Parts.fashion:
                    top_txt_Header.Text = "时尚区";
                    Part_Id = 155;
                    #region
                    {
                        PartModel p0 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "首页",
                            isHome = true,
                            Banner = await GetBanner(Part_Id),
                            DTs = await GetTuiJianDT(Part_Id),
                            leftVisibility = Visibility.Visible,
                            rightVisibility = Visibility.Visible,
                            grid_c_left = new GridLength(1, GridUnitType.Star),
                            grid_c_right = new GridLength(1, GridUnitType.Star),
                            grid_c_center = new GridLength(0, GridUnitType.Auto)
                        };
                        if (p0.Banner.Count != 0)
                        {
                            p0.homeBanner = p0.Banner[0];
                            p0.leftBanner = p0.Banner[p0.Banner.Count - 1];
                            p0.rightBanner = p0.Banner[p0.Banner.IndexOf(p0.homeBanner) + 1];

                        }
                        PartModel p1 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "美妆",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 157,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(157),
                            VideoList = await GetVideos(157, defu_Order, 1, "")
                        };

                        if (p1.TagsList.Count != 0)
                        {
                            p1.SelectTag = p1.TagsList[0];
                        }
                        PartModel p2 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "服饰",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 158,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(158),
                            VideoList = await GetVideos(158, defu_Order, 1, "")
                        };
                        if (p2.TagsList.Count != 0)
                        {
                            p2.SelectTag = p2.TagsList[0];
                        }
                        PartModel p3 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "健身",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 164,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(164),
                            VideoList = await GetVideos(164, defu_Order, 1, "")
                        };
                        if (p3.TagsList.Count != 0)
                        {
                            p3.SelectTag = p3.TagsList[0];
                        }
                        PartModel p4 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "资讯",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 159,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(159),
                            VideoList = await GetVideos(159, defu_Order, 1, "")
                        };
                        if (p4.TagsList.Count != 0)
                        {
                            p4.SelectTag = p4.TagsList[0];
                        }
                        l.Add(p0);
                        l.Add(p1);
                        l.Add(p2);
                        l.Add(p3);
                        l.Add(p4);
                    }
                    #endregion
                    break;
                case Parts.ent:
                    top_txt_Header.Text = "娱乐区";
                    Part_Id = 5;
                    #region
                    {
                        PartModel p0 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "首页",
                            isHome = true,
                            Banner = await GetBanner(Part_Id),
                            DTs = await GetTuiJianDT(Part_Id),
                            leftVisibility = Visibility.Visible,
                            rightVisibility = Visibility.Visible,
                            grid_c_left = new GridLength(1, GridUnitType.Star),
                            grid_c_right = new GridLength(1, GridUnitType.Star),
                            grid_c_center = new GridLength(0, GridUnitType.Auto)
                        };
                        if (p0.Banner.Count != 0)
                        {
                            p0.homeBanner = p0.Banner[0];
                            p0.leftBanner = p0.Banner[p0.Banner.Count - 1];
                            p0.rightBanner = p0.Banner[p0.Banner.IndexOf(p0.homeBanner) + 1];

                        }
                        PartModel p1 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "综艺",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 71,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(71),
                            VideoList = await GetVideos(71, defu_Order, 1, "")
                        };

                        if (p1.TagsList.Count != 0)
                        {
                            p1.SelectTag = p1.TagsList[0];
                        }
                        PartModel p2 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "明星",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 137,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(137),
                            VideoList = await GetVideos(137, defu_Order, 1, "")
                        };
                        if (p2.TagsList.Count != 0)
                        {
                            p2.SelectTag = p2.TagsList[0];
                        }
                        PartModel p3 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "Korea相关",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 131,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(131),
                            VideoList = await GetVideos(131, defu_Order, 1, "")
                        };
                        if (p3.TagsList.Count != 0)
                        {
                            p3.SelectTag = p3.TagsList[0];
                        }
                        l.Add(p0);
                        l.Add(p1);
                        l.Add(p2);
                        l.Add(p3);
                    }
                    #endregion
                    break;
                case Parts.movie:
                    top_txt_Header.Text = "电影区";
                    Part_Id = 23;
                    #region
                    {

                        PartModel p0 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "首页",
                            isHome = true,
                            Banner = await GetBanner(Part_Id),
                            DTs = await GetTuiJianDT(Part_Id),
                            leftVisibility = Visibility.Visible,
                            rightVisibility = Visibility.Visible,
                            grid_c_left = new GridLength(1, GridUnitType.Star),
                            grid_c_right = new GridLength(1, GridUnitType.Star),
                            grid_c_center = new GridLength(0, GridUnitType.Auto)
                        };
                        if (p0.Banner.Count != 0)
                        {
                            try
                            {
                                p0.homeBanner = p0.Banner[0];
                                p0.leftBanner = p0.Banner[p0.Banner.Count - 1];
                                p0.rightBanner = p0.Banner[p0.Banner.IndexOf(p0.homeBanner) + 1];

                            }
                            catch (Exception)
                            {
                            }
                          

                        }
                        PartModel p1 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "电影相关",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 82,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(82),
                            VideoList = await GetVideos(82, defu_Order, 1, "")
                        };

                        if (p1.TagsList.Count != 0)
                        {
                            p1.SelectTag = p1.TagsList[0];
                        }
                        PartModel p2 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "短片",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 85,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(85),
                            VideoList = await GetVideos(85, defu_Order, 1, "")
                        };
                        if (p2.TagsList.Count != 0)
                        {
                            p2.SelectTag = p2.TagsList[0];
                        }
                        PartModel p3 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "欧美电影",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 145,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(145),
                            VideoList = await GetVideos(145, defu_Order, 1, "")
                        };
                        if (p3.TagsList.Count != 0)
                        {
                            p3.SelectTag = p3.TagsList[0];
                        }
                        PartModel p4 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "日本电影",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 146,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(146),
                            VideoList = await GetVideos(146, defu_Order, 1, "")
                        };
                        if (p4.TagsList.Count != 0)
                        {
                            p4.SelectTag = p4.TagsList[0];
                        }
                        PartModel p5 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "国产电影",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 147,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(147),
                            VideoList = await GetVideos(147, defu_Order, 1, "")
                        };
                        if (p5.TagsList.Count != 0)
                        {
                            p5.SelectTag = p5.TagsList[0];
                        }
                        PartModel p6 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "其他国家",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 83,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(83),
                            VideoList = await GetVideos(83, defu_Order, 1, "")
                        };
                        if (p6.TagsList.Count != 0)
                        {
                            p6.SelectTag = p6.TagsList[0];
                        }
                        l.Add(p0);
                        l.Add(p1);
                        l.Add(p2);
                        l.Add(p3);
                        l.Add(p4);
                        l.Add(p5);
                        l.Add(p6);
                    }
                    #endregion
                    break;
                case Parts.tv:
                    top_txt_Header.Text = "电视剧区";
                    Part_Id = 11;
                    #region
                    {

                        PartModel p0 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "首页",
                            isHome = true,
                            Banner = await GetBanner(Part_Id),
                            DTs = await GetTuiJianDT(Part_Id),
                            leftVisibility = Visibility.Visible,
                            rightVisibility = Visibility.Visible,
                            grid_c_left = new GridLength(1, GridUnitType.Star),
                            grid_c_right = new GridLength(1, GridUnitType.Star),
                            grid_c_center = new GridLength(0, GridUnitType.Auto)
                        };
                        if (p0.Banner.Count != 0)
                        {
                            p0.homeBanner = p0.Banner[0];
                            p0.leftBanner = p0.Banner[p0.Banner.Count - 1];
                            p0.rightBanner = p0.Banner[p0.Banner.IndexOf(p0.homeBanner) + 1];

                        }
                        PartModel p1 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "连载剧集",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 15,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(15),
                            VideoList = await GetVideos(15, defu_Order, 1, "")
                        };

                        if (p1.TagsList.Count != 0)
                        {
                            p1.SelectTag = p1.TagsList[0];
                        }
                        PartModel p2 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "完结剧集",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 34,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(34),
                            VideoList = await GetVideos(34, defu_Order, 1, "")
                        };
                        if (p2.TagsList.Count != 0)
                        {
                            p2.SelectTag = p2.TagsList[0];
                        }
                        PartModel p3 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "特摄·布袋",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 86,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(86),
                            VideoList = await GetVideos(86, defu_Order, 1, "")
                        };
                        if (p3.TagsList.Count != 0)
                        {
                            p3.SelectTag = p3.TagsList[0];
                        }
                        PartModel p4 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "电视剧相关",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 128,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(128),
                            VideoList = await GetVideos(128, defu_Order, 1, "")
                        };
                        if (p4.TagsList.Count != 0)
                        {
                            p4.SelectTag = p4.TagsList[0];
                        }
                        l.Add(p0);
                        l.Add(p1);
                        l.Add(p2);
                        l.Add(p3);
                        l.Add(p4);
                    }
                    #endregion
                    break;
                case Parts.ad:
                    top_txt_Header.Text = "广告区";
                    Part_Id = 165;
                    #region
                    {

                        PartModel p0 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "首页",
                            isHome = true,
                            Banner = await GetBanner(Part_Id),
                            DTs = await GetTuiJianDT(Part_Id),
                            leftVisibility = Visibility.Visible,
                            rightVisibility = Visibility.Visible,
                            grid_c_left = new GridLength(1, GridUnitType.Star),
                            grid_c_right = new GridLength(1, GridUnitType.Star),
                            grid_c_center = new GridLength(0, GridUnitType.Auto)
                        };
                        if (p0.Banner.Count != 0)
                        {
                            p0.homeBanner = p0.Banner[0];
                            p0.leftBanner = p0.Banner[p0.Banner.Count - 1];
                            p0.rightBanner = p0.Banner[p0.Banner.IndexOf(p0.homeBanner) + 1];

                        }
                        PartModel p1 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "广告",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 165,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(165),
                            VideoList = await GetVideos(165, defu_Order, 1, "")
                        };

                        if (p1.TagsList.Count != 0)
                        {
                            p1.SelectTag = p1.TagsList[0];
                        }
                      
                        l.Add(p0);
                        l.Add(p1);
                    }
                    #endregion
                    break;
                case Parts.cn:
                    top_txt_Header.Text = "国创区";
                    Part_Id = 167;
                    #region
                    {
                        PartModel p0 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "首页",
                            isHome = true,
                            Banner = await GetBanner(Part_Id),
                            DTs = await GetTuiJianDT(Part_Id),
                            leftVisibility = Visibility.Visible,
                            rightVisibility = Visibility.Visible,
                            grid_c_left = new GridLength(1, GridUnitType.Star),
                            grid_c_right = new GridLength(1, GridUnitType.Star),
                            grid_c_center = new GridLength(0, GridUnitType.Auto)
                        };

                        if (p0.Banner.Count != 0)
                        {
                            p0.homeBanner = p0.Banner[0];
                            p0.leftBanner = p0.Banner[p0.Banner.Count - 1];
                            p0.rightBanner = p0.Banner[p0.Banner.IndexOf(p0.homeBanner) + 1];

                        }
                        PartModel p1 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "国产动画",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 153,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(153),
                            VideoList = await GetVideos(153, defu_Order, 1, "")
                        };

                        if (p1.TagsList.Count != 0)
                        {
                            p1.SelectTag = p1.TagsList[0];
                        }
                        PartModel p2 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "国产原创相关",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 168,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(168),
                            VideoList = await GetVideos(168, defu_Order, 1, "")
                        };
                        if (p2.TagsList.Count != 0)
                        {
                            p2.SelectTag = p2.TagsList[0];
                        }
                        PartModel p3 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "国产动画",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 153,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(153),
                            VideoList = await GetVideos(153, defu_Order, 1, "")
                        };
                        if (p3.TagsList.Count != 0)
                        {
                            p3.SelectTag = p3.TagsList[0];
                        }
                        PartModel p4 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "布袋戏",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 169,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(169),
                            VideoList = await GetVideos(169, defu_Order, 1, "")
                        };
                        if (p4.TagsList.Count != 0)
                        {
                            p4.SelectTag = p4.TagsList[0];
                        }
                        PartModel p5 = new PartModel()
                        {
                            fontWeight = FontWeights.Normal,
                            HanderText = "资讯",
                            isHome = false,
                            orderBy = PartOrderBy.senddate,
                            PartId = 170,
                            PageNum = 1,
                            ShowTags = Visibility.Collapsed,
                            TagsList = await GetTags(170),
                            VideoList = await GetVideos(170, defu_Order, 1, "")
                        };
                        if (p5.TagsList.Count != 0)
                        {
                            p5.SelectTag = p5.TagsList[0];
                        }
                        l.Add(p0);
                        l.Add(p1);
                        l.Add(p2);
                        l.Add(p3);
                        l.Add(p4);
                        l.Add(p5);
                    }
                    #endregion
                    break;
                default:
                    break;
            }

            // 首页(横幅/推荐)区块依赖的旧接口已失效，移除后只保留有视频列表的分区
            l.RemoveAll(x => x.isHome);
            if (l.Count == 0)
            {
                PartModel fallback = new PartModel()
                {
                    fontWeight = FontWeights.Normal,
                    HanderText = "视频",
                    isHome = false,
                    orderBy = PartOrderBy.senddate,
                    PartId = Part_Id,
                    PageNum = 1,
                    ShowTags = Visibility.Collapsed,
                    TagsList = await GetTags(Part_Id),
                    VideoList = await GetVideos(Part_Id, defu_Order, 1, "")
                };
                if (fallback.TagsList != null && fallback.TagsList.Count != 0)
                {
                    fallback.SelectTag = fallback.TagsList[0];
                }
                l.Add(fallback);
            }

            pivot.ItemsSource = l;
            UpdateBannerState();
          
        }

        private async Task LoadPart(RegionModel parts)
        {
            com_bar.Visibility = Visibility.Collapsed;
            _hideOrderBar = true;
            List<PartModel> l = new List<PartModel>();

            top_txt_Header.Text = parts.name;
            Part_Id = parts.tid;

            // 子分区接口(旧的 region/show/child/list、新的 newlist 对子分区)都已取不到数据，
            // 这里直接用「整个分区」的视频列表（newlist 对顶级分区可用，且会聚合子分区内容）
            PartModel p = new PartModel()
            {
                fontWeight = FontWeights.Normal,
                HanderText = parts.name,
                isHome = false,
                orderBy = PartOrderBy.senddate,
                PartId = parts.tid,
                PageNum = 1,
                ShowTags = Visibility.Collapsed,
                TagsList = await GetTags(parts.tid)
            };
            if (p.TagsList != null && p.TagsList.Count != 0)
            {
                p.SelectTag = p.TagsList[0];
            }
            p.VideoList = await GetVideos(parts.tid, defu_Order, 1, "");
            l.Add(p);

            if (p.VideoList == null || p.VideoList.Count == 0)
            {
                Utils.ShowMessageToast("该分区暂时没有可显示的视频");
            }

            pivot.ItemsSource = l;
            UpdateBannerState();
        }

        private Task<ObservableCollection<TagsModel>> GetTags(int id)
        {
            // 旧的标签接口(x/tag/hots)已失效，分区内不做标签筛选，只保留「全部」
            var list = new ObservableCollection<TagsModel>();
            list.Add(new TagsModel() { tag_name = "全部" });
            return Task.FromResult(list);
        }
        // 分区视频池（网页端 ranking/v2 一次最多返回 100 条，这里做本地分页，避免出现重复内容）
        private static readonly Dictionary<int, List<DHModel>> _regionPoolCache = new Dictionary<int, List<DHModel>>();

        private async Task<ObservableCollection<DHModel>> GetVideos(int id, string orderBy, int Num, string tag)
        {
            ObservableCollection<DHModel> list = new ObservableCollection<DHModel>();
            try
            {
                if (Num != 1)
                {
                    pr_Laod.Visibility = Visibility.Visible;
                }

                // 第一页或缓存缺失时，拉取该分区的视频池
                if (Num == 1 || !_regionPoolCache.ContainsKey(id))
                {
                    var pool = await FetchRegionVideos(id);
                    _regionPoolCache[id] = pool;
                }

                List<DHModel> cached;
                if (_regionPoolCache.TryGetValue(id, out cached) && cached.Count != 0)
                {
                    foreach (var item in cached.Skip((Num - 1) * 20).Take(20))
                    {
                        list.Add(item);
                    }
                }
                return list;
            }
            catch (Exception ex)
            {
                BiliBili.UWP.Helper.LogHelper.WriteLog("读取分区视频失败: rid=" + id, BiliBili.UWP.Helper.LogType.ERROR, ex);
                return list;
            }
            finally
            {
                if (Num != 1)
                {
                    pr_Laod.Visibility = Visibility.Collapsed;
                }
            }
        }

        /// <summary>
        /// 读取一个分区的视频池：优先用网页端分区排行榜(最多100条)，
        /// 失败时退回 x/web-interface/newlist（只有最新20条）。
        /// 注意 newlist 的 pn/order 参数服务端已忽略，翻页会返回同样的内容，不能用于加载更多。
        /// </summary>
        private async Task<List<DHModel>> FetchRegionVideos(int rid)
        {
            var list = new List<DHModel>();
            try
            {
                string url = $"https://api.bilibili.com/x/web-interface/ranking/v2?rid={rid}&type=all";
                string text = await BiliBili.UWP.Helper.BiliWebSession.GetStringAsync(url, "https://www.bilibili.com/v/popular/rank/all");
                JObject obj = JObject.Parse(text);
                var array = obj["data"] is JObject dataObj ? dataObj["list"] as JArray : null;
                if (array != null && array.Count != 0)
                {
                    foreach (var item in array)
                    {
                        list.Add(MapVideoItem(item));
                    }
                    return list;
                }
                BiliBili.UWP.Helper.LogHelper.WriteLog("分区排行榜不可用，退回最新列表: rid=" + rid + " code=" + (obj["code"] ?? ""), BiliBili.UWP.Helper.LogType.INFO);
            }
            catch (Exception ex)
            {
                BiliBili.UWP.Helper.LogHelper.WriteLog("读取分区排行榜失败: rid=" + rid, BiliBili.UWP.Helper.LogType.ERROR, ex);
            }

            // 兜底：最新视频（只有一页）
            try
            {
                string uri = $"https://api.bilibili.com/x/web-interface/newlist?rid={rid}&ps=20";
                string results = await BiliBili.UWP.Helper.BiliWebSession.GetStringAsync(uri, "https://www.bilibili.com/");
                JObject jObject = JObject.Parse(results);
                var archives = jObject["data"] is JObject dataObj ? dataObj["archives"] as JArray : null;
                if (archives != null)
                {
                    foreach (var item in archives)
                    {
                        list.Add(MapVideoItem(item));
                    }
                }
            }
            catch (Exception ex)
            {
                BiliBili.UWP.Helper.LogHelper.WriteLog("读取分区最新视频失败: rid=" + rid, BiliBili.UWP.Helper.LogType.ERROR, ex);
            }
            return list;
        }

        private static DHModel MapVideoItem(JToken item)
        {
            string pic = item["pic"]?.ToString();
            return new DHModel
            {
                aid = item["aid"]?.ToString(),
                title = item["title"]?.ToString(),
                pic = string.IsNullOrEmpty(pic) ? pic : pic + "@200w.jpg",
                author = item["owner"]?["name"]?.ToString(),
                mid = item["owner"]?["mid"]?.ToString(),
                play = FormatCount(item["stat"]?["view"]),
                video_review = FormatCount(item["stat"]?["danmaku"]),
            };
        }

        /// <summary>
        /// 播放量/弹幕格式化成「1.2万」这种显示
        /// </summary>
        private static string FormatCount(JToken token)
        {
            try
            {
                if (token == null)
                {
                    return "0";
                }
                long value = token.ToObject<long>();
                if (value >= 100000000)
                {
                    return (value / 100000000.0).ToString("0.0") + "亿";
                }
                if (value >= 10000)
                {
                    return (value / 10000.0).ToString("0.0") + "万";
                }
                return value.ToString();
            }
            catch (Exception)
            {
                return "0";
            }
        }

        private Task<List<DHModel>> GetBanner(int Id)
        {
            // 旧 Banner 接口(app.bilibili.com/api/region2/*.json)已失效
            return Task.FromResult(new List<DHModel>());
        }

        private Task<List<DHModel>> GetTuiJianDT(int Id)
        {
            // 旧推荐接口(bilibili.com/index/ding/*.json)已失效
            return Task.FromResult(new List<DHModel>());
        }


        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            int d = Convert.ToInt32(this.ActualWidth / 400);
            if (d > 3)
            {
                d = 3;
            }
            bor_Width.Width = this.ActualWidth / d - 22;
            if (this.ActualWidth <= 500)
            {
                ViewBox_num.Width = ActualWidth / 2 - 18;
                ViewBox2_num.Width = ActualWidth / 2 - 18;
                Grid.SetRow(com_bar, 2);
               
                com_bar.HorizontalAlignment = HorizontalAlignment.Stretch;
                com_bar.VerticalAlignment = VerticalAlignment.Bottom;

            }
            else
            {
                int i = Convert.ToInt32(ActualWidth / 200);
                ViewBox_num.Width = ActualWidth / i - 13;
                ViewBox2_num.Width = ActualWidth / i - 13;

                Grid.SetRow(com_bar, 0);
                Grid.SetRowSpan(com_bar, 2);
                com_bar.HorizontalAlignment = HorizontalAlignment.Right;
                com_bar.VerticalAlignment = VerticalAlignment.Top;
            }

           


            if (pivot.Items.Count == 0)
            {
                return;
            }
            var m = (pivot.ItemsSource as List<PartModel>)[0];
            if (this.ActualWidth <= 640)
            {

                m.leftVisibility = Visibility.Collapsed;
                m.rightVisibility = Visibility.Collapsed;
                m.grid_c_left = new GridLength(0, GridUnitType.Auto);
                m.grid_c_right = new GridLength(0, GridUnitType.Auto);
                m.grid_c_center = new GridLength(1, GridUnitType.Star);
            }
            else
            {
                m.leftVisibility = Visibility.Visible;
                m.rightVisibility = Visibility.Visible;
                m.grid_c_left = new GridLength(1, GridUnitType.Star);
                m.grid_c_right = new GridLength(1, GridUnitType.Star);
                m.grid_c_center = new GridLength(0, GridUnitType.Auto);
            }


        }
        private void UpdateBannerState()
        {
            if (pivot.Items.Count == 0)
            {
                return;
            }
            var m = (pivot.ItemsSource as List<PartModel>)[0];
            if (this.ActualWidth <= 640)
            {

                m.leftVisibility = Visibility.Collapsed;
                m.rightVisibility = Visibility.Collapsed;
                m.grid_c_left = new GridLength(0, GridUnitType.Auto);
                m.grid_c_right = new GridLength(0, GridUnitType.Auto);
                m.grid_c_center = new GridLength(1, GridUnitType.Star);
            }
            else
            {
                m.leftVisibility = Visibility.Visible;
                m.rightVisibility = Visibility.Visible;
                m.grid_c_left = new GridLength(1, GridUnitType.Star);
                m.grid_c_right = new GridLength(1, GridUnitType.Star);
                m.grid_c_center = new GridLength(0, GridUnitType.Auto);
            }

        }


        private void LZ_List_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as DHModel;
            if (item == null || string.IsNullOrEmpty(item.aid))
            {
                return;
            }
            // 列表里存的就是 av 号，直接打开视频详情
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(VideoViewPage), item.aid);
        }
        bool isLoading = false;
        private async void sv_LZ_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;
            var m = scrollViewer == null ? null : scrollViewer.DataContext as PartModel;
            if (m == null || m.NoMore)
            {
                return;
            }
            if (scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight)
            {
                if (!isLoading)
                {
                    isLoading = true;
                    try
                    {
                        m.PageNum++;
                        var more = await GetVideos(m.PartId, m.orderBy.ToString(), m.PageNum, m.SelectTag == null ? "全部" : m.SelectTag.tag_name);
                        if (more == null || more.Count == 0)
                        {
                            m.NoMore = true;
                            Utils.ShowMessageToast("没有更多了");
                        }
                        else
                        {
                            foreach (var item in more)
                            {
                                m.VideoList.Add(item);
                            }
                        }
                    }
                    finally
                    {
                        isLoading = false;
                    }
                }
            }
        }

        private async void grid_tag_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            (pivot.SelectedItem as PartModel).PageNum = 1;
            if ((pivot.SelectedItem as PartModel).VideoList!=null)
            {
                (pivot.SelectedItem as PartModel).VideoList.Clear();
            }
           
            var m = pivot.SelectedItem as PartModel;
            m.SelectTag = (sender as GridView).SelectedItem as TagsModel;
            m.VideoList = await GetVideos(m.PartId, m.orderBy.ToString(), m.PageNum, ((sender as GridView).SelectedItem as TagsModel).tag_name);

        }

        private void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pivot.Items.Count == 0)
            {

                return;
            }
            var current = pivot.SelectedItem as PartModel;
            if (current == null || current.isHome || _hideOrderBar)
            {
                com_bar.Visibility = Visibility.Collapsed;
            }
            else
            {
                com_bar.Visibility = Visibility.Visible;
            }
            btn_Type.IsChecked = false;
            
            foreach (ToggleMenuFlyoutItem item in menu.Items)
            {
                item.IsChecked = false;
            }
            switch ((pivot.SelectedItem as PartModel).orderBy)
            {
                case PartOrderBy.view:
                    btn_Play.IsChecked = true;
                    break;
                case PartOrderBy.danmaku:
                    btn_Danmaku.IsChecked = true;
                    break;
                case PartOrderBy.reply:
                    btn_Comment.IsChecked = true;
                    break;
                case PartOrderBy.favorite:
                    btn_Sc.IsChecked = true;
                    break;
                case PartOrderBy.senddate:
                    btn_New.IsChecked = true;
                    break;
                default:
                    break;
            }
            //(pivot.SelectedItem as PartModel).fontWeight = FontWeights.Bold;

        }

        private void btn_Type_Checked(object sender, RoutedEventArgs e)
        {
            (pivot.SelectedItem as PartModel).ShowTags = Visibility.Visible;
        }

        private void btn_Type_Unchecked(object sender, RoutedEventArgs e)
        {
            (pivot.SelectedItem as PartModel).ShowTags = Visibility.Collapsed;
        }

        private async void btn_Refresh_Click(object sender, RoutedEventArgs e)
        {
            var m = pivot.SelectedItem as PartModel;
            if (m == null)
            {
                return;
            }
            _regionPoolCache.Remove(m.PartId);
            m.PageNum = 1;
            m.NoMore = false;
            if (m.VideoList != null)
            {
                m.VideoList.Clear();
            }
            m.VideoList = await GetVideos(m.PartId, m.orderBy.ToString(), m.PageNum, m.SelectTag == null ? "全部" : m.SelectTag.tag_name);
        }

        private void btn_New_Click(object sender, RoutedEventArgs e)
        {
            foreach (ToggleMenuFlyoutItem item in menu.Items)
            {
                item.IsChecked = false;
            }
            int se = Convert.ToInt32((sender as ToggleMenuFlyoutItem).Tag);
            switch (se)
            {
                case 0:
                    (pivot.SelectedItem as PartModel).orderBy = PartOrderBy.senddate;
                    break;
                case 1:
                    (pivot.SelectedItem as PartModel).orderBy = PartOrderBy.danmaku;
                    break;
                case 2:
                    (pivot.SelectedItem as PartModel).orderBy = PartOrderBy.view;
                    break;
                case 3:
                    (pivot.SelectedItem as PartModel).orderBy = PartOrderBy.reply;
                    break;
                case 4:
                    (pivot.SelectedItem as PartModel).orderBy = PartOrderBy.favorite;
                    break;
                default:
                    break;
            }
            switch ((pivot.SelectedItem as PartModel).orderBy)
            {
                case PartOrderBy.view:
                    btn_Play.IsChecked = true;
                    break;
                case PartOrderBy.danmaku:
                    btn_Danmaku.IsChecked = true;
                    break;
                case PartOrderBy.reply:
                    btn_Comment.IsChecked = true;
                    break;
                case PartOrderBy.favorite:
                    btn_Sc.IsChecked = true;
                    break;
                case PartOrderBy.senddate:
                    btn_New.IsChecked = true;
                    break;
                default:
                    break;
            }
            btn_Refresh_Click(this, e);
        }

        private void home_flipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void btn_Banner_Ban_Click(object sender, RoutedEventArgs e)
        {
            var m = (DHModel)(sender as HyperlinkButton).DataContext;
            if (m.aid != "0")
            {
                MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(VideoViewPage), m.aid);
                //this.Frame.Navigate(typeof(VideoInfoPage), m.aid);
                return;
            }
            string ban = Regex.Match(m.link, @"^http://bangumi.bilibili.com/anime/(.*?)$").Groups[1].Value;
            if (ban.Length != 0)
            {
                MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(BanInfoPage), ban);
                //this.Frame.Navigate(typeof(BanInfoPage), ban);
                return;
            }
            string ban2 = Regex.Match(m.link, @"^http://www.bilibili.com/bangumi/i/(.*?)$").Groups[1].Value;
            if (ban2.Length != 0)
            {
                MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(BanInfoPage), ban2);
                //this.Frame.Navigate(typeof(BanInfoPage), ban2);
                return;
            }
            string aid = Regex.Match(m.link, @"^http://www.bilibili.com/video/av(.*?)/$").Groups[1].Value;
            if (aid.Length != 0)
            {
                MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(VideoViewPage),aid);
               // this.Frame.Navigate(typeof(VideoInfoPage), aid);
                return;
            }
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(WebPage), m.link);
            //this.Frame.Navigate(typeof(WebViewPage), m.link);
        }

        private async void PullToRefreshBox_RefreshInvoked(DependencyObject sender, object args)
        {
            var m = (pivot.ItemsSource as List<PartModel>)[0];
            m.Banner = await GetBanner(Part_Id);
            m.DTs = await GetTuiJianDT(Part_Id);

        }

        private async void btn_Refresh_DT_Click(object sender, RoutedEventArgs e)
        {
            var m = (pivot.ItemsSource as List<PartModel>)[0];
            m.DTs = await GetTuiJianDT(Part_Id);
        }

        private void GridView_DT_ItemClick(object sender, ItemClickEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(VideoViewPage), ((DHModel)e.ClickedItem).aid);
            //this.Frame.Navigate(typeof(VideoInfoPage), ((DHModel)e.ClickedItem).aid);
        }

        private async void btn_LoadMore_Click(object sender, RoutedEventArgs e)
        {
            if (!isLoading)
            {
                isLoading = true;
                var m = (sender as Button).DataContext as PartModel;
                m.PageNum++;
                foreach (var item in await GetVideos(m.PartId, m.orderBy.ToString(), m.PageNum, m.SelectTag.tag_name))
                {
                    m.VideoList.Add(item);
                }

                isLoading = false;
            }

        }
    }
    public static class TestClass
    {
        public static DataTemplate d1;
        public static DataTemplate d2;
    }
    public class MessageItemDataTemplateSelector2 : DataTemplateSelector
    {
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var s = item as PartModel;
            if (s.isHome)
            {
                return TestClass.d1;
                //return App.Current.Resources["HomeTemplate"] as DataTemplate;
            }
            else
            {
                return TestClass.d2;
                //return App.Current.Resources["ItemsTemplate"] as DataTemplate;
            }

        }
    }

    public class PartModel : INotifyPropertyChanged
    {
        private FontWeight _fontWeight;
        public FontWeight fontWeight
        {
            get { return _fontWeight; }
            set { _fontWeight = value; RaisePropertyChanged("fontWeight"); }
        }
        public string HanderText { get; set; }
        /// <summary>
        /// 是否已经没有更多视频（避免滑到底反复请求）
        /// </summary>
        public bool NoMore { get; set; }
        private ObservableCollection<DHModel> _VideoList;
        public ObservableCollection<DHModel> VideoList
        {
            get { return _VideoList; }
            set { _VideoList = value; RaisePropertyChanged("VideoList"); }
        }
        private List<DHModel> _Banner;
        public List<DHModel> Banner
        {
            get { return _Banner; }
            set { _Banner = value;RaisePropertyChanged("Banner"); }
        }
        private List<DHModel> _DTs;
        public List<DHModel> DTs
        {
            get { return _DTs; }
            set { _DTs = value; RaisePropertyChanged("DTs"); }
        }
        private DHModel _homeBanner;
        public DHModel homeBanner
        {
            get { return _homeBanner; }
            set
            {
                _homeBanner = value;
                RaisePropertyChanged("homeBanner");
                if (Banner.Count == 0)
                {
                    return;
                }
                if (leftVisibility == Visibility.Collapsed || rightVisibility == Visibility.Collapsed)
                {
                    return;
                }
                try
                {
                    if (Banner.IndexOf(value) == 0)
                    {
                        leftBanner = Banner[Banner.Count - 1];
                        //this.fvLeft.SelectedIndex = this.fvLeft.Items.Count - 1;
                        //this.fvRight.SelectedIndex = 1;
                        rightBanner = Banner[1];
                    }
                    else if (Banner.IndexOf(value) == 1)
                    {
                        leftBanner = Banner[0];
                        rightBanner = Banner[Banner.Count - 1];
                        //this.fvLeft.SelectedIndex = 0;
                        // this.fvRight.SelectedIndex = this.fvRight.Items.Count - 1;
                    }
                    else if (Banner.IndexOf(value) == Banner.Count - 1)
                    {
                        leftBanner = Banner[Banner.Count - 2];
                        rightBanner = Banner[0];
                        //this.fvLeft.SelectedIndex = this.fvLeft.Items.Count - 2;
                        // this.fvRight.SelectedIndex = 0;
                    }
                    else if ((Banner.IndexOf(value) < (Banner.Count - 1)) && Banner.IndexOf(value) > -1)
                    {
                        leftBanner = Banner[Banner.IndexOf(value) - 1];//  this.home_flipView.SelectedIndex - 1;
                        rightBanner = Banner[Banner.IndexOf(value) + 1];
                        //this.fvLeft.SelectedIndex = this.home_flipView.SelectedIndex - 1;
                        //this.fvRight.SelectedIndex = this.home_flipView.SelectedIndex + 1;
                    }
                    else
                    {
                        return;
                    }
                }
                catch (Exception)
                {
                }
                

            }
        }
        private DHModel _leftBanner;
        public DHModel leftBanner
        {
            get { return _leftBanner; }
            set { _leftBanner = value; RaisePropertyChanged("leftBanner"); }
        }
        private DHModel _rightBanner;
        public DHModel rightBanner
        {
            get { return _rightBanner; }
            set { _rightBanner = value; RaisePropertyChanged("rightBanner"); }
        }

        private Visibility _leftVisibility;
        public Visibility leftVisibility
        {
            get { return _leftVisibility; }
            set { _leftVisibility = value; RaisePropertyChanged("leftVisibility"); }
        }

        private Visibility _rightVisibility;
        public Visibility rightVisibility
        {
            get { return _rightVisibility; }
            set { _rightVisibility = value; RaisePropertyChanged("rightVisibility"); }
        }
        private GridLength _grid_c_left;
        public GridLength grid_c_left
        {
            get { return _grid_c_left; }
            set { _grid_c_left = value; RaisePropertyChanged("grid_c_left"); }
        }
        private GridLength _grid_c_right;
        public GridLength grid_c_right
        {
            get { return _grid_c_right; }
            set { _grid_c_right = value; RaisePropertyChanged("grid_c_right"); }
        }
        private GridLength _grid_c_center;
        public GridLength grid_c_center
        {
            get { return _grid_c_center; }
            set { _grid_c_center = value; RaisePropertyChanged("grid_c_center"); }
        }


        private ObservableCollection<TagsModel> _TagsList;
        public ObservableCollection<TagsModel> TagsList
        {
            get { return _TagsList; }
            set { _TagsList = value; RaisePropertyChanged("TagsList"); }
        }
        public bool isHome { get; set; }
        public int PartId { get; set; }
        public int PageNum { get; set; }
        public PartOrderBy orderBy { get; set; }

        private TagsModel _SelectTag;
        public TagsModel SelectTag
        {
            get { return _SelectTag; }
            set { _SelectTag = value; RaisePropertyChanged("SelectTag"); }
        }

        private Visibility _ShowTags;
        public Visibility ShowTags
        {
            get { return _ShowTags; }
            set { _ShowTags = value; RaisePropertyChanged("ShowTags"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
    public class TagsModel
    {
        public int code { get; set; }
        public object data { get; set; }
        public int rid { get; set; }
        public ObservableCollection<TagsModel> tags { get; set; }
        public int tag_id { get; set; }
        public string tag_name { get; set; }
        public int highlight { get; set; }
        public string message { get; set; }
    }
    public class DHModel
    {
        public object result { get; set; }
        public object list { get; set; }

        public object recommends { get; set; }
        public string aid { get; set; }
        public string title { get; set; }
        public string play { get; set; }
        public string video_review { get; set; }
        public string mid { get; set; }
        public string pic { get; set; }
        public string author { get; set; }

        public object banners { get; set; }
        public string img { get; set; }

        public object news { get; set; }
        public string link { get; set; }
    }

}
