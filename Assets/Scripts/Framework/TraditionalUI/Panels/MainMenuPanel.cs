using UnityEngine;
using UnityEngine.UI;

namespace Framework.TraditionalUI.Panels
{
    /// <summary>
    /// 主菜单面板
    /// 游戏的主要入口界面
    /// </summary>
    public class MainMenuPanel : TraditionalUIPanel
    {
        #region UI组件引用
        
        [Header("主菜单按钮")]
        [SerializeField] private Button startGameButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button achievementsButton;
        [SerializeField] private Button shopButton;
        [SerializeField] private Button exitButton;
        
        [Header("信息显示")]
        [SerializeField] private Text playerNameText;
        [SerializeField] private Text playerLevelText;
        [SerializeField] private Text coinText;
        [SerializeField] private Text gemText;
        
        [Header("版本信息")]
        [SerializeField] private Text versionText;
        
        #endregion
        
        #region 初始化方法
        
        protected override void OnInitialize()
        {
            base.OnInitialize();
            
            // 设置按钮事件
            SetupButtonEvents();
            
            // 初始化UI显示
            InitializeUI();
        }
        
        /// <summary>
        /// 设置按钮事件
        /// </summary>
        private void SetupButtonEvents()
        {
            if (startGameButton != null)
                startGameButton.onClick.AddListener(OnStartGameClick);
            
            if (continueButton != null)
                continueButton.onClick.AddListener(OnContinueClick);
            
            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsClick);
            
            if (achievementsButton != null)
                achievementsButton.onClick.AddListener(OnAchievementsClick);
            
            if (shopButton != null)
                shopButton.onClick.AddListener(OnShopClick);
            
            if (exitButton != null)
                exitButton.onClick.AddListener(OnExitClick);
        }
        
        /// <summary>
        /// 初始化UI显示
        /// </summary>
        private void InitializeUI()
        {
            // 设置版本信息
            if (versionText != null)
            {
                versionText.text = $"版本 {Application.version}";
            }
            
            // 更新玩家信息
            UpdatePlayerInfo();
        }
        
        #endregion
        
        #region 生命周期回调
        
        protected override void OnAfterShow()
        {
            base.OnAfterShow();
            
            // 每次显示时更新玩家信息
            UpdatePlayerInfo();
            
            // 检查继续游戏按钮状态
            UpdateContinueButtonState();
        }
        
        #endregion
        
        #region UI更新方法
        
        /// <summary>
        /// 更新玩家信息显示
        /// </summary>
        private void UpdatePlayerInfo()
        {
            // 这里应该从游戏数据管理器获取玩家信息
            // 为了演示，使用模拟数据
            
            if (playerNameText != null)
            {
                // string playerName = GameDataManager.Instance.GetPlayerName();
                string playerName = "玩家"; // 模拟数据
                playerNameText.text = playerName;
            }
            
            if (playerLevelText != null)
            {
                // int playerLevel = GameDataManager.Instance.GetPlayerLevel();
                int playerLevel = 1; // 模拟数据
                playerLevelText.text = $"等级 {playerLevel}";
            }
            
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
        /// 更新继续游戏按钮状态
        /// </summary>
        private void UpdateContinueButtonState()
        {
            if (continueButton != null)
            {
                // bool hasSaveData = SaveManager.Instance.HasSaveData();
                bool hasSaveData = true; // 模拟数据
                continueButton.interactable = hasSaveData;
            }
        }
        
        #endregion
        
        #region 按钮事件处理
        
        /// <summary>
        /// 开始游戏按钮点击
        /// </summary>
        private void OnStartGameClick()
        {
            Debug.Log("[主菜单] 开始新游戏");
            
            // 显示确认对话框（如果有存档的话）
            // if (SaveManager.Instance.HasSaveData())
            // {
            //     TraditionalUIManager.Instance.OpenPanel("ConfirmDialog", new ConfirmDialogData
            //     {
            //         title = "确认",
            //         message = "开始新游戏将覆盖当前存档，是否继续？",
            //         onConfirm = StartNewGame
            //     });
            // }
            // else
            // {
            //     StartNewGame();
            // }
            
            StartNewGame(); // 直接开始新游戏
        }
        
        /// <summary>
        /// 继续游戏按钮点击
        /// </summary>
        private void OnContinueClick()
        {
            Debug.Log("[主菜单] 继续游戏");
            
            // 加载存档并进入游戏
            // SaveManager.Instance.LoadGame();
            // SceneManager.LoadScene("GameScene");
        }
        
        /// <summary>
        /// 设置按钮点击
        /// </summary>
        private void OnSettingsClick()
        {
            Debug.Log("[主菜单] 打开设置");
            TraditionalUIManager.Instance.OpenPanel("Settings");
        }
        
        /// <summary>
        /// 成就按钮点击
        /// </summary>
        private void OnAchievementsClick()
        {
            Debug.Log("[主菜单] 打开成就");
            // TraditionalUIManager.Instance.OpenPanel("Achievements");
        }
        
        /// <summary>
        /// 商店按钮点击
        /// </summary>
        private void OnShopClick()
        {
            Debug.Log("[主菜单] 打开商店");
            TraditionalUIManager.Instance.OpenPanel("Shop");
        }
        
        /// <summary>
        /// 退出按钮点击
        /// </summary>
        private void OnExitClick()
        {
            Debug.Log("[主菜单] 退出游戏");
            
            // 显示退出确认对话框
            TraditionalUIManager.Instance.OpenPanel("MessageBox", new MessageBoxData
            {
                title = "退出游戏",
                message = "确定要退出游戏吗？",
                showCancelButton = true,
                onConfirm = () => {
                    #if UNITY_EDITOR
                        UnityEditor.EditorApplication.isPlaying = false;
                    #else
                        Application.Quit();
                    #endif
                }
            });
        }
        
        #endregion
        
        #region 私有方法
        
        /// <summary>
        /// 开始新游戏
        /// </summary>
        private void StartNewGame()
        {
            Debug.Log("[主菜单] 开始新游戏");
            
            // 初始化游戏数据
            // GameDataManager.Instance.InitializeNewGame();
            
            // 显示加载界面
            TraditionalUIManager.Instance.OpenPanel("Loading");
            
            // 加载游戏场景
            // SceneManager.LoadScene("GameScene");
        }
        
        #endregion
    }
    
    /// <summary>
    /// 消息框数据类
    /// </summary>
    public class MessageBoxData
    {
        public string title;
        public string message;
        public bool showCancelButton = false;
        public System.Action onConfirm;
        public System.Action onCancel;
    }
}