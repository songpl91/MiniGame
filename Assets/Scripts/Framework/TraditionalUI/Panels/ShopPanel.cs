using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Framework.TraditionalUI.Panels
{
    /// <summary>
    /// 商店面板
    /// 游戏内商店界面，支持多种商品类型和货币
    /// </summary>
    public class ShopPanel : TraditionalUIPanel
    {
        #region UI组件引用
        
        [Header("顶部信息")]
        [SerializeField] private Text coinText;
        [SerializeField] private Text gemText;
        [SerializeField] private Button refreshButton;
        
        [Header("分类标签")]
        [SerializeField] private Transform categoryTabParent;
        [SerializeField] private Button categoryTabPrefab;
        
        [Header("商品列表")]
        [SerializeField] private ScrollRect shopScrollRect;
        [SerializeField] private Transform shopItemParent;
        [SerializeField] private ShopItemUI shopItemPrefab;
        
        [Header("底部按钮")]
        [SerializeField] private Button backButton;
        
        #endregion
        
        #region 私有变量
        
        private List<ShopCategory> shopCategories;
        private List<Button> categoryTabs;
        private List<ShopItemUI> shopItemUIs;
        private int currentCategoryIndex = 0;
        
        #endregion
        
        #region 初始化方法
        
        protected override void OnInitialize()
        {
            base.OnInitialize();
            
            // 初始化数据
            InitializeShopData();
            
            // 设置按钮事件
            SetupButtonEvents();
            
            // 创建分类标签
            CreateCategoryTabs();
            
            // 初始化商品列表
            InitializeShopItems();
        }
        
        /// <summary>
        /// 设置按钮事件
        /// </summary>
        private void SetupButtonEvents()
        {
            if (refreshButton != null)
                refreshButton.onClick.AddListener(OnRefreshClick);
            
            if (backButton != null)
                backButton.onClick.AddListener(OnBackClick);
        }
        
        /// <summary>
        /// 初始化商店数据
        /// </summary>
        private void InitializeShopData()
        {
            // 这里应该从数据管理器加载商店数据
            // shopCategories = ShopDataManager.Instance.GetShopCategories();
            
            // 模拟数据
            shopCategories = new List<ShopCategory>
            {
                new ShopCategory
                {
                    id = "weapons",
                    name = "武器",
                    items = new List<ShopItem>
                    {
                        new ShopItem { id = "sword1", name = "铁剑", price = 100, currencyType = CurrencyType.Coin, description = "基础的铁制剑，攻击力+10" },
                        new ShopItem { id = "sword2", name = "银剑", price = 500, currencyType = CurrencyType.Coin, description = "精制的银剑，攻击力+25" },
                        new ShopItem { id = "sword3", name = "黄金剑", price = 50, currencyType = CurrencyType.Gem, description = "传说中的黄金剑，攻击力+50" }
                    }
                },
                new ShopCategory
                {
                    id = "armor",
                    name = "防具",
                    items = new List<ShopItem>
                    {
                        new ShopItem { id = "armor1", name = "皮甲", price = 80, currencyType = CurrencyType.Coin, description = "基础的皮制护甲，防御力+5" },
                        new ShopItem { id = "armor2", name = "铁甲", price = 300, currencyType = CurrencyType.Coin, description = "坚固的铁制护甲，防御力+15" },
                        new ShopItem { id = "armor3", name = "龙鳞甲", price = 100, currencyType = CurrencyType.Gem, description = "龙鳞制成的护甲，防御力+30" }
                    }
                },
                new ShopCategory
                {
                    id = "consumables",
                    name = "消耗品",
                    items = new List<ShopItem>
                    {
                        new ShopItem { id = "potion1", name = "生命药水", price = 20, currencyType = CurrencyType.Coin, description = "恢复50点生命值" },
                        new ShopItem { id = "potion2", name = "魔法药水", price = 30, currencyType = CurrencyType.Coin, description = "恢复30点魔法值" },
                        new ShopItem { id = "potion3", name = "经验药水", price = 10, currencyType = CurrencyType.Gem, description = "获得100点经验值" }
                    }
                }
            };
            
            categoryTabs = new List<Button>();
            shopItemUIs = new List<ShopItemUI>();
        }
        
        /// <summary>
        /// 创建分类标签
        /// </summary>
        private void CreateCategoryTabs()
        {
            if (categoryTabParent == null || categoryTabPrefab == null) return;
            
            // 清除现有标签
            foreach (Transform child in categoryTabParent)
            {
                if (child != categoryTabPrefab.transform)
                    DestroyImmediate(child.gameObject);
            }
            categoryTabs.Clear();
            
            // 创建新标签
            for (int i = 0; i < shopCategories.Count; i++)
            {
                var category = shopCategories[i];
                var tabButton = Instantiate(categoryTabPrefab, categoryTabParent);
                tabButton.gameObject.SetActive(true);
                
                // 设置标签文本
                var tabText = tabButton.GetComponentInChildren<Text>();
                if (tabText != null)
                    tabText.text = category.name;
                
                // 设置点击事件
                int categoryIndex = i;
                tabButton.onClick.AddListener(() => OnCategoryTabClick(categoryIndex));
                
                categoryTabs.Add(tabButton);
            }
            
            // 选中第一个标签
            if (categoryTabs.Count > 0)
            {
                SelectCategoryTab(0);
            }
        }
        
        /// <summary>
        /// 初始化商品列表
        /// </summary>
        private void InitializeShopItems()
        {
            if (shopItemParent == null || shopItemPrefab == null) return;
            
            // 清除现有商品UI
            foreach (Transform child in shopItemParent)
            {
                if (child != shopItemPrefab.transform)
                    DestroyImmediate(child.gameObject);
            }
            shopItemUIs.Clear();
            
            // 创建商品UI（创建足够的UI对象以供复用）
            int maxItemsPerCategory = 0;
            foreach (var category in shopCategories)
            {
                maxItemsPerCategory = Mathf.Max(maxItemsPerCategory, category.items.Count);
            }
            
            for (int i = 0; i < maxItemsPerCategory; i++)
            {
                var itemUI = Instantiate(shopItemPrefab, shopItemParent);
                itemUI.gameObject.SetActive(false);
                itemUI.OnPurchaseClick += OnItemPurchaseClick;
                shopItemUIs.Add(itemUI);
            }
            
            // 显示当前分类的商品
            ShowCategoryItems(currentCategoryIndex);
        }
        
        #endregion
        
        #region 生命周期回调
        
        protected override void OnAfterShow()
        {
            base.OnAfterShow();
            
            // 更新货币显示
            UpdateCurrencyDisplay();
        }
        
        #endregion
        
        #region UI更新方法
        
        /// <summary>
        /// 更新货币显示
        /// </summary>
        private void UpdateCurrencyDisplay()
        {
            if (coinText != null)
            {
                // int coins = GameDataManager.Instance.GetCoins();
                int coins = 1000; // 模拟数据
                coinText.text = coins.ToString();
            }
            
            if (gemText != null)
            {
                // int gems = GameDataManager.Instance.GetGems();
                int gems = 50; // 模拟数据
                gemText.text = gems.ToString();
            }
        }
        
        /// <summary>
        /// 选中分类标签
        /// </summary>
        /// <param name="index">分类索引</param>
        private void SelectCategoryTab(int index)
        {
            currentCategoryIndex = index;
            
            // 更新标签状态
            for (int i = 0; i < categoryTabs.Count; i++)
            {
                var tab = categoryTabs[i];
                var colors = tab.colors;
                
                if (i == index)
                {
                    // 选中状态
                    colors.normalColor = Color.yellow;
                }
                else
                {
                    // 未选中状态
                    colors.normalColor = Color.white;
                }
                
                tab.colors = colors;
            }
            
            // 显示对应分类的商品
            ShowCategoryItems(index);
        }
        
        /// <summary>
        /// 显示分类商品
        /// </summary>
        /// <param name="categoryIndex">分类索引</param>
        private void ShowCategoryItems(int categoryIndex)
        {
            if (categoryIndex < 0 || categoryIndex >= shopCategories.Count) return;
            
            var category = shopCategories[categoryIndex];
            
            // 隐藏所有商品UI
            foreach (var itemUI in shopItemUIs)
            {
                itemUI.gameObject.SetActive(false);
            }
            
            // 显示当前分类的商品
            for (int i = 0; i < category.items.Count && i < shopItemUIs.Count; i++)
            {
                var item = category.items[i];
                var itemUI = shopItemUIs[i];
                
                itemUI.SetData(item);
                itemUI.gameObject.SetActive(true);
            }
            
            // 重置滚动位置
            if (shopScrollRect != null)
            {
                shopScrollRect.verticalNormalizedPosition = 1f;
            }
        }
        
        #endregion
        
        #region 事件处理方法
        
        /// <summary>
        /// 分类标签点击
        /// </summary>
        /// <param name="categoryIndex">分类索引</param>
        private void OnCategoryTabClick(int categoryIndex)
        {
            Debug.Log($"[商店] 切换到分类: {shopCategories[categoryIndex].name}");
            SelectCategoryTab(categoryIndex);
        }
        
        /// <summary>
        /// 商品购买点击
        /// </summary>
        /// <param name="item">商品数据</param>
        private void OnItemPurchaseClick(ShopItem item)
        {
            Debug.Log($"[商店] 尝试购买商品: {item.name}");
            
            // 检查货币是否足够
            if (!CanAffordItem(item))
            {
                MessageBoxPanel.ShowWarning("购买失败", $"您的{GetCurrencyName(item.currencyType)}不足！");
                return;
            }
            
            // 显示购买确认对话框
            string currencyName = GetCurrencyName(item.currencyType);
            string message = $"确定要花费 {item.price} {currencyName} 购买 {item.name} 吗？";
            
            MessageBoxPanel.ShowConfirm("确认购买", message, 
                () => PurchaseItem(item),
                () => Debug.Log("[商店] 取消购买"));
        }
        
        /// <summary>
        /// 刷新按钮点击
        /// </summary>
        private void OnRefreshClick()
        {
            Debug.Log("[商店] 刷新商店");
            
            // 这里可以实现商店刷新逻辑
            // 比如重新加载商品数据、更新限时商品等
            
            MessageBoxPanel.ShowInfo("刷新完成", "商店已刷新！");
        }
        
        /// <summary>
        /// 返回按钮点击
        /// </summary>
        private void OnBackClick()
        {
            Debug.Log("[商店] 返回");
            // TraditionalUIManager.Instance.ClosePanel(this);
        }
        
        #endregion
        
        #region 购买逻辑
        
        /// <summary>
        /// 检查是否能够购买商品
        /// </summary>
        /// <param name="item">商品数据</param>
        /// <returns>是否能够购买</returns>
        private bool CanAffordItem(ShopItem item)
        {
            switch (item.currencyType)
            {
                case CurrencyType.Coin:
                    // return GameDataManager.Instance.GetCoins() >= item.price;
                    return 1000 >= item.price; // 模拟数据
                
                case CurrencyType.Gem:
                    // return GameDataManager.Instance.GetGems() >= item.price;
                    return 50 >= item.price; // 模拟数据
                
                default:
                    return false;
            }
        }
        
        /// <summary>
        /// 购买商品
        /// </summary>
        /// <param name="item">商品数据</param>
        private void PurchaseItem(ShopItem item)
        {
            Debug.Log($"[商店] 购买商品: {item.name}");
            
            // 扣除货币
            // switch (item.currencyType)
            // {
            //     case CurrencyType.Coin:
            //         GameDataManager.Instance.SpendCoins(item.price);
            //         break;
            //     case CurrencyType.Gem:
            //         GameDataManager.Instance.SpendGems(item.price);
            //         break;
            // }
            
            // 添加物品到背包
            // InventoryManager.Instance.AddItem(item.id, 1);
            
            // 更新UI显示
            UpdateCurrencyDisplay();
            
            // 显示购买成功消息
            MessageBoxPanel.ShowInfo("购买成功", $"成功购买 {item.name}！");
        }
        
        /// <summary>
        /// 获取货币名称
        /// </summary>
        /// <param name="currencyType">货币类型</param>
        /// <returns>货币名称</returns>
        private string GetCurrencyName(CurrencyType currencyType)
        {
            switch (currencyType)
            {
                case CurrencyType.Coin:
                    return "金币";
                case CurrencyType.Gem:
                    return "宝石";
                default:
                    return "未知货币";
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// 商店分类数据
    /// </summary>
    [System.Serializable]
    public class ShopCategory
    {
        public string id;
        public string name;
        public List<ShopItem> items;
    }
    
    /// <summary>
    /// 商店商品数据
    /// </summary>
    [System.Serializable]
    public class ShopItem
    {
        public string id;
        public string name;
        public string description;
        public int price;
        public CurrencyType currencyType;
        public string iconPath;
        public bool isLimited;
        public int limitCount;
        public int purchasedCount;
    }
    
    /// <summary>
    /// 货币类型枚举
    /// </summary>
    public enum CurrencyType
    {
        Coin,   // 金币
        Gem     // 宝石
    }
    
    /// <summary>
    /// 商店商品UI组件
    /// </summary>
    public class ShopItemUI : MonoBehaviour
    {
        [Header("UI组件")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Text nameText;
        [SerializeField] private Text descriptionText;
        [SerializeField] private Text priceText;
        [SerializeField] private Image currencyIcon;
        [SerializeField] private Button purchaseButton;
        
        [Header("货币图标")]
        [SerializeField] private Sprite coinIcon;
        [SerializeField] private Sprite gemIcon;
        
        private ShopItem itemData;
        
        public System.Action<ShopItem> OnPurchaseClick;
        
        private void Awake()
        {
            if (purchaseButton != null)
                purchaseButton.onClick.AddListener(OnPurchaseButtonClick);
        }
        
        /// <summary>
        /// 设置商品数据
        /// </summary>
        /// <param name="item">商品数据</param>
        public void SetData(ShopItem item)
        {
            itemData = item;
            UpdateUI();
        }
        
        /// <summary>
        /// 更新UI显示
        /// </summary>
        private void UpdateUI()
        {
            if (itemData == null) return;
            
            // 设置名称
            if (nameText != null)
                nameText.text = itemData.name;
            
            // 设置描述
            if (descriptionText != null)
                descriptionText.text = itemData.description;
            
            // 设置价格
            if (priceText != null)
                priceText.text = itemData.price.ToString();
            
            // 设置货币图标
            if (currencyIcon != null)
            {
                switch (itemData.currencyType)
                {
                    case CurrencyType.Coin:
                        currencyIcon.sprite = coinIcon;
                        break;
                    case CurrencyType.Gem:
                        currencyIcon.sprite = gemIcon;
                        break;
                }
            }
            
            // 设置商品图标
            if (iconImage != null && !string.IsNullOrEmpty(itemData.iconPath))
            {
                // 这里应该加载商品图标
                // var sprite = ResourceManager.Instance.LoadSprite(itemData.iconPath);
                // iconImage.sprite = sprite;
            }
        }
        
        /// <summary>
        /// 购买按钮点击
        /// </summary>
        private void OnPurchaseButtonClick()
        {
            OnPurchaseClick?.Invoke(itemData);
        }
    }
}