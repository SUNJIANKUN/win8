using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.Resources.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using System.Collections.Specialized;

// 此文件定义的数据模型可充当在添加、移除或修改成员时
// 支持通知的强类型模型的代表性示例。所选
// 属性名称与标准项模板中的数据绑定一致。
//
// 应用程序可以使用此模型作为起始点并以它为基础构建，或完全放弃它并
// 替换为适合其需求的其他内容。

namespace App3.Data
{
    /// <summary>
    /// <see cref="SampleDataItem"/> 和 <see cref="SampleDataGroup"/> 的基类，
    /// 定义对两者通用的属性。
    /// </summary>
    [Windows.Foundation.Metadata.WebHostHidden]
    public abstract class SampleDataCommon : App3.Common.BindableBase
    {
        private static Uri _baseUri = new Uri("ms-appx:///");

        public SampleDataCommon(String uniqueId, String title, String subtitle, String imagePath, String description)
        {
            this._uniqueId = uniqueId;
            this._title = title;
            this._subtitle = subtitle;
            this._description = description;
            this._imagePath = imagePath;
        }

        private string _uniqueId = string.Empty;
        public string UniqueId
        {
            get { return this._uniqueId; }
            set { this.SetProperty(ref this._uniqueId, value); }
        }

        private string _title = string.Empty;
        public string Title
        {
            get { return this._title; }
            set { this.SetProperty(ref this._title, value); }
        }

        private string _subtitle = string.Empty;
        public string Subtitle
        {
            get { return this._subtitle; }
            set { this.SetProperty(ref this._subtitle, value); }
        }

        private string _description = string.Empty;
        public string Description
        {
            get { return this._description; }
            set { this.SetProperty(ref this._description, value); }
        }

        private ImageSource _image = null;
        private String _imagePath = null;
        public ImageSource Image
        {
            get
            {
                if (this._image == null && this._imagePath != null)
                {
                    this._image = new BitmapImage(new Uri(SampleDataCommon._baseUri, this._imagePath));
                }
                return this._image;
            }

            set
            {
                this._imagePath = null;
                this.SetProperty(ref this._image, value);
            }
        }

        public void SetImage(String path)
        {
            this._image = null;
            this._imagePath = path;
            this.OnPropertyChanged("Image");
        }

        public override string ToString()
        {
            return this.Title;
        }
    }

    /// <summary>
    /// 泛型项数据模型。
    /// </summary>
    public class SampleDataItem : SampleDataCommon
    {
        public SampleDataItem(String uniqueId, String title, String subtitle, String imagePath, String description, String content, SampleDataGroup group)
            : base(uniqueId, title, subtitle, imagePath, description)
        {
            this._content = content;
            this._group = group;
        }

        private string _content = string.Empty;
        public string Content
        {
            get { return this._content; }
            set { this.SetProperty(ref this._content, value); }
        }

        private SampleDataGroup _group;
        public SampleDataGroup Group
        {
            get { return this._group; }
            set { this.SetProperty(ref this._group, value); }
        }
    }

    /// <summary>
    /// 泛型组数据模型。
    /// </summary>
    public class SampleDataGroup : SampleDataCommon
    {
        public SampleDataGroup(String uniqueId, String title, String subtitle, String imagePath, String description)
            : base(uniqueId, title, subtitle, imagePath, description)
        {
            Items.CollectionChanged += ItemsCollectionChanged;
        }

        private void ItemsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // 由于两个原因提供要从 GroupedItemsPage 绑定到的完整
            // 项集合的子集: GridView 不会虚拟化大型项集合，并且它
            // 可在浏览包含大量项的组时改进用户
            // 体验。
            //
            // 最多显示 12 项，因为无论显示 1、2、3、4 还是 6 行，
            // 它都生成填充网格列

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewStartingIndex < 12)
                    {
                        TopItems.Insert(e.NewStartingIndex,Items[e.NewStartingIndex]);
                        if (TopItems.Count > 12)
                        {
                            TopItems.RemoveAt(12);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    if (e.OldStartingIndex < 12 && e.NewStartingIndex < 12)
                    {
                        TopItems.Move(e.OldStartingIndex, e.NewStartingIndex);
                    }
                    else if (e.OldStartingIndex < 12)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        TopItems.Add(Items[11]);
                    }
                    else if (e.NewStartingIndex < 12)
                    {
                        TopItems.Insert(e.NewStartingIndex, Items[e.NewStartingIndex]);
                        TopItems.RemoveAt(12);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldStartingIndex < 12)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        if (Items.Count >= 12)
                        {
                            TopItems.Add(Items[11]);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (e.OldStartingIndex < 12)
                    {
                        TopItems[e.OldStartingIndex] = Items[e.OldStartingIndex];
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    TopItems.Clear();
                    while (TopItems.Count < Items.Count && TopItems.Count < 12)
                    {
                        TopItems.Add(Items[TopItems.Count]);
                    }
                    break;
            }
        }

        private ObservableCollection<SampleDataItem> _items = new ObservableCollection<SampleDataItem>();
        public ObservableCollection<SampleDataItem> Items
        {
            get { return this._items; }
        }

        private ObservableCollection<SampleDataItem> _topItem = new ObservableCollection<SampleDataItem>();
        public ObservableCollection<SampleDataItem> TopItems
        {
            get {return this._topItem; }
        }
    }

    /// <summary>
    /// 创建包含硬编码内容的组和项的集合。
    /// 
    /// SampleDataSource 用占位符数据而不是实时生产数据
    /// 初始化，因此在设计时和运行时均需提供示例数据。
    /// </summary>
    public sealed class SampleDataSource
    {
        private static SampleDataSource _sampleDataSource = new SampleDataSource();

        private ObservableCollection<SampleDataGroup> _allGroups = new ObservableCollection<SampleDataGroup>();
        public ObservableCollection<SampleDataGroup> AllGroups
        {
            get { return this._allGroups; }
        }

        public static IEnumerable<SampleDataGroup> GetGroups(string uniqueId)
        {
            if (!uniqueId.Equals("AllGroups")) throw new ArgumentException("Only 'AllGroups' is supported as a collection of groups");
            
            return _sampleDataSource.AllGroups;
        }

        public static SampleDataGroup GetGroup(string uniqueId)
        {
            // 对于小型数据集可接受简单线性搜索
            var matches = _sampleDataSource.AllGroups.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public static SampleDataItem GetItem(string uniqueId)
        {
            // 对于小型数据集可接受简单线性搜索
            var matches = _sampleDataSource.AllGroups.SelectMany(group => group.Items).Where((item) => item.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public SampleDataSource()
        {
            String ITEM_CONTENT = String.Format("Item Content: {0}\n\n{0}\n\n{0}\n\n{0}\n\n{0}\n\n{0}\n\n{0}",
                        "Curabitur class aliquam vestibulum nam curae maecenas sed integer cras phasellus suspendisse quisque donec dis praesent accumsan bibendum pellentesque condimentum adipiscing etiam consequat vivamus dictumst aliquam duis convallis scelerisque est parturient ullamcorper aliquet fusce suspendisse nunc hac eleifend amet blandit facilisi condimentum commodo scelerisque faucibus aenean ullamcorper ante mauris dignissim consectetuer nullam lorem vestibulum habitant conubia elementum pellentesque morbi facilisis arcu sollicitudin diam cubilia aptent vestibulum auctor eget dapibus pellentesque inceptos leo egestas interdum nulla consectetuer suspendisse adipiscing pellentesque proin lobortis sollicitudin augue elit mus congue fermentum parturient fringilla euismod feugiat");

            var group1 = new SampleDataGroup("Group-1",
                    "美味寿司",
                    "好吃的寿司",
                    "Assets/0.jpg",
                    "寿司虽然是日本食品，但原本来自中国，古书上曾有过记载。我相信很多童鞋都很喜欢这漂亮的寿司吧，下面大家就和小编一起来学习各种口味寿司的做法！很美味滴哟！");
            group1.Items.Add(new SampleDataItem("Group-1-Item-1",
                    "蟹籽寿司",
                    "一",
                    "Assets/1.jpg",
                    "【材料】： 寿司饭、海苔、蟹籽、青芥末 ",
                    "\n\n\n\n【材料】： 寿司饭、海苔、蟹籽、青芥末 \n【做法】：1、用手将饭捏成一样大小的饭团，抹上少许青芥末 2、边上用海苔围起，海苔要比饭团高处1/3左右 3、小心地放入蟹籽 ",
                    group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-2",
                    "奇异果寿司",
                    "二",
                    "Assets/2.jpg",
                    "【材料】： 椰奶、泰国香米、香草豆(去籽)、奇异果去皮、菠萝去皮、糖、水",
                    "\n\n\n\n【材料】： 椰奶、泰国香米、香草豆(去籽)、奇异果去皮、菠萝去皮、糖、水\n【做法】： 1、椰奶、水、米、糖和香草豆放入锅中，用中高火煮沸，然后用小火炖25到30分钟，不断搅拌，直到米饭变软汁水全部被吸干2、去除香草豆和多余的油，冷却2个小时3、竹帘上依次放保鲜膜、米饭、奇异果和去皮菠萝4、卷成卷，去除保鲜膜，在寿司上滚上一层烤椰丝5、切成段",
                    group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-3",
                    "飞鱼籽寿司",
                    "三",
                    "Assets/3.jpg",
                    "【材料】： 米饭、飞鱼籽、黄瓜、芥末、醋、海鲜酱油 ",
                    "\n\n\n\n【材料】： 米饭、飞鱼籽、黄瓜、芥末、醋、海鲜酱油 \n【做法】：1、黄瓜用削土豆皮的刀竖着从黄瓜上削下来，一片可以包一个寿司2、把米饭握成小扁圆型，外边包上削下来的黄瓜长片，中间点上些芥末3、最后，在上面放上飞鱼籽4、可以蘸上海鲜酱油吃 ",
                    group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-4",
                    "拌饭寿司",
                    "四",
                    "Assets/4.jpg",
                    "【材料】： 米饭、紫菜、韩国辣酱、杏仁酱(吃起来也更香)、菠菜、芝麻、盐、麻油、甘蓝(切丝备用) ",
                    "\n\n\n\n【材料】： 米饭、紫菜、韩国辣酱、杏仁酱(吃起来也更香)、菠菜、芝麻、盐、麻油、甘蓝(切丝备用)\n【做法】：1、米饭用韩国辣酱和杏仁酱拌匀2、菠菜焯熟，加芝麻，盐，麻油拌匀3、海苔上铺米饭，在靠近自己胸前的这一端，摆上甘蓝和菠菜4、卷起，稍整形，用刀蘸水切段儿即可 ",
                    group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-5",
                    "心里美寿司",
                    "五",
                    "Assets/5.jpg",
                    "【材料】： 米饭、紫菜、心里美萝卜刨成丝、寿司醋 ",
                    "\n\n\n\n【材料】： 米饭、紫菜、心里美萝卜刨成丝、寿司醋 \n【做法】：1、心里美萝卜丝内加入2勺白糖，2勺白醋、1/2勺柠檬汁、1/3茶匙盐拌一下2、半小时后，把心里美里泡出的甜醋汁，倒入米饭里拌匀 3、卷成卷后切断 \n【点评】：心里美萝卜富含多种维生素以及钙、磷、铁等，所含的芥子油能促进胃肠蠕动，帮助消化，顺气解郁，还有改善血液循环，保护心脏，延缓衰老防癌的作用。",
                    group1));
           
            group1.Items.Add(new SampleDataItem("Group-1-Item-6",
                   "小熊寿司",
                   "六",
                   "Assets/6.jpg",
                   "【材料】： 海苔、米饭、酱油、肉松、火腿 ",
                   "\n\n\n\n【材料】： 海苔、米饭、酱油、肉松、火腿\n【做法】：1、一部分米饭用寿司醋拌匀待用(白色部分)，另一部分米饭用酱油和肉松拌匀待用(棕色部分) 2、用海苔将棕色米饭卷成小卷(做耳朵)，再用棕色米饭和火腿卷成卷(做脸) 3、把小卷卷放在白米饭里卷成大卷卷4、把海苔剪成小片做眼睛和鼻子",
                   group1));
          
            group1.Items.Add(new SampleDataItem("Group-1-Item-7",
                   "桃色寿司",
                   "七",
                   "Assets/7.jpg",
                   "【材料】： 大米、糯米、苋菜、蟹柳、黄瓜、烤海苔和醋汁",
                   "\n\n\n\n【材料】： 大米、糯米、苋菜、蟹柳、黄瓜、烤海苔和醋汁\n【做法】： 1、选苋菜调少量油盐爆香，菜盛出 2、选用余下的菜汁烩入白饭中即是天然的润色材料，备用 3、用寿司帘将双色米饭和蟹柳、黄瓜平铺于海苔上，卷紧压实 4、沾水冷切寿司卷就OK啦 ",
                   group1));
           
            group1.Items.Add(new SampleDataItem("Group-1-Item-8",
                   "蟹肉寿司",
                   "八",
                   "Assets/8.jpg",
                   "【材料】： 米饭、熟白芝麻、罐装蟹肉、黄瓜、干香菇、紫菜、葱花、糖、酱油、酒 ",
                   "\n\n\n\n【材料】： 米饭、熟白芝麻、罐装蟹肉、黄瓜、干香菇、紫菜、葱花、糖、酱油、酒\n【做法】 1、蟹肉除去软骨，用手撕碎 2、香菇用温水泡软，切成碎末加糖、酱油和酒煮熟 3、黄瓜切成薄片，紫菜切成4等分 4、寿司模型用醋水蘸湿，底部铺上叶片， 填入一半高度的寿司米饭，铺上紫菜、香菇、黄瓜，再填上一层寿司饭 5、最上层铺上蟹肉，加盖压紧，用刀切成小块，撒上葱花 ",
                   group1));
          
            group1.Items.Add(new SampleDataItem("Group-1-Item-9",
                   "心形寿司",
                   "九",
                   "Assets/9.jpg",
                   "【材料】： 白米饭、紫菜、寿司醋、腌黄瓜条、煎蛋丝、红萝卜丝、心形模具、蟹籽、炒香的黑芝麻 ",
                   "\n\n\n\n【材料】： 白米饭、紫菜、寿司醋、腌黄瓜条、煎蛋丝、红萝卜丝、心形模具、蟹籽、炒香的黑芝麻\n【做法】： 1、把煮好的饭放凉后，倒入适量的甜醋，拌均匀  2、模具内涂一点油，方便脱模 3、先放一层饭在模具底，压实压平 4、放上馅料 5、放一层饭上去封顶，压平、结实 6、倒扣脱模 7、紫菜剪成与心饭团的高度同高，然后捆着边包围饭团，再在上面放些蟹籽、黑芝麻，压实，切段",
                   group1));
            
            group1.Items.Add(new SampleDataItem("Group-1-Item-11",
                   "蛋卷寿司",
                   "十",
                   "Assets/11.jpg",
                   "【材料】： 寿司饭、紫菜、煎好的厚度均匀的蛋块、玉米粒、香肠粒、胡萝卜粒、陈皮粒",
                   "\n\n\n\n【材料】： 寿司饭、紫菜、煎好的厚度均匀的蛋块、玉米粒、香肠粒、胡萝卜粒、陈皮粒\n【做法】： 1、把玉米粒、香肠粒、胡萝卜粒、陈皮粒与饭拌匀 2、帘子上放一层保鲜膜，依次铺上蛋皮、饭、馅料 3、注意不能太大力，否则蛋会破 ",
                   group1));
            this.AllGroups.Add(group1);

        }     
    }
}
