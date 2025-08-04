using UnityEngine;
using UnityEngine.UI;
using Framework.EnhanceUI.Core;

namespace Framework.EnhanceUI.Examples
{
    /// <summary>
    /// 泛型数据面板示例
    /// 演示如何在面板中处理泛型数据
    /// </summary>
    public class GenericDataPanel : EnhanceUIPanel
    {
        [Header("UI组件")]
        [SerializeField] private Text titleText;
        [SerializeField] private Text contentText;
        [SerializeField] private Text dataTypeText;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;
        
        // 存储强类型数据
        private object currentData;
        private System.Type currentDataType;
        
        protected override void OnInitialize(object data)
        {
            base.OnInitialize(data);
            
            // 设置按钮事件
            if (confirmButton != null)
                confirmButton.onClick.AddListener(OnConfirmClick);
            
            if (cancelButton != null)
                cancelButton.onClick.AddListener(OnCancelClick);
            
            // 处理传入的数据
            ProcessData(data);
        }
        
        /// <summary>
        /// 处理泛型数据（重写基类方法）
        /// </summary>
        protected override void OnInitialize<T>(T data)
        {
            // 存储数据和类型信息
            currentData = data;
            currentDataType = typeof(T);
            
            // 调用基类方法
            base.OnInitialize(data);
            
            Debug.Log($"[GenericDataPanel] 接收到泛型数据: {typeof(T).Name}");
        }
        
        /// <summary>
        /// 显示前处理数据（重写基类方法）
        /// </summary>
        protected override void OnBeforeShow<T>(T data)
        {
            base.OnBeforeShow(data);
            
            // 更新UI显示
            UpdateUI(data);
            
            Debug.Log($"[GenericDataPanel] 显示前处理泛型数据: {typeof(T).Name}");
        }
        
        /// <summary>
        /// 处理数据
        /// </summary>
        private void ProcessData(object data)
        {
            if (data == null)
            {
                ShowEmptyData();
                return;
            }
            
            // 根据数据类型进行不同处理
            switch (data)
            {
                case int intValue:
                    ShowIntData(intValue);
                    break;
                case float floatValue:
                    ShowFloatData(floatValue);
                    break;
                case string stringValue:
                    ShowStringData(stringValue);
                    break;
                case bool boolValue:
                    ShowBoolData(boolValue);
                    break;
                case GenericUIExample.PlayerData playerData:
                    ShowPlayerData(playerData);
                    break;
                case GenericUIExample.GameSettings gameSettings:
                    ShowGameSettings(gameSettings);
                    break;
                default:
                    ShowGenericData(data);
                    break;
            }
        }
        
        /// <summary>
        /// 更新UI显示
        /// </summary>
        private void UpdateUI<T>(T data)
        {
            if (dataTypeText != null)
            {
                dataTypeText.text = $"数据类型: {typeof(T).Name}";
            }
            
            ProcessData(data);
        }
        
        /// <summary>
        /// 显示空数据
        /// </summary>
        private void ShowEmptyData()
        {
            SetTitle("空数据面板");
            SetContent("没有传递任何数据");
            SetDataType("null");
        }
        
        /// <summary>
        /// 显示整数数据
        /// </summary>
        private void ShowIntData(int value)
        {
            SetTitle("整数数据面板");
            SetContent($"数值: {value}\n" +
                      $"十六进制: 0x{value:X}\n" +
                      $"二进制: {System.Convert.ToString(value, 2)}\n" +
                      $"是否为偶数: {value % 2 == 0}");
            SetDataType("int (值类型)");
        }
        
        /// <summary>
        /// 显示浮点数据
        /// </summary>
        private void ShowFloatData(float value)
        {
            SetTitle("浮点数据面板");
            SetContent($"数值: {value}\n" +
                      $"四舍五入: {Mathf.Round(value)}\n" +
                      $"向上取整: {Mathf.Ceil(value)}\n" +
                      $"向下取整: {Mathf.Floor(value)}");
            SetDataType("float (值类型)");
        }
        
        /// <summary>
        /// 显示字符串数据
        /// </summary>
        private void ShowStringData(string value)
        {
            SetTitle("字符串数据面板");
            SetContent($"内容: {value}\n" +
                      $"长度: {value.Length}\n" +
                      $"大写: {value.ToUpper()}\n" +
                      $"小写: {value.ToLower()}");
            SetDataType("string (引用类型)");
        }
        
        /// <summary>
        /// 显示布尔数据
        /// </summary>
        private void ShowBoolData(bool value)
        {
            SetTitle("布尔数据面板");
            SetContent($"值: {value}\n" +
                      $"字符串: {value.ToString()}\n" +
                      $"取反: {!value}\n" +
                      $"状态: {(value ? "真" : "假")}");
            SetDataType("bool (值类型)");
        }
        
        /// <summary>
        /// 显示玩家数据
        /// </summary>
        private void ShowPlayerData(GenericUIExample.PlayerData playerData)
        {
            SetTitle("玩家数据面板");
            SetContent($"玩家ID: {playerData.playerId}\n" +
                      $"玩家名称: {playerData.playerName}\n" +
                      $"等级: {playerData.level}\n" +
                      $"经验值: {playerData.experience:N0}\n" +
                      $"VIP状态: {(playerData.isVip ? "是" : "否")}");
            SetDataType("PlayerData (结构体)");
        }
        
        /// <summary>
        /// 显示游戏设置数据
        /// </summary>
        private void ShowGameSettings(GenericUIExample.GameSettings gameSettings)
        {
            SetTitle("游戏设置面板");
            SetContent($"音乐音量: {gameSettings.musicVolume:P0}\n" +
                      $"音效音量: {gameSettings.soundVolume:P0}\n" +
                      $"语言: {gameSettings.language}\n" +
                      $"画质: {gameSettings.quality}\n" +
                      $"通知: {(gameSettings.enableNotifications ? "开启" : "关闭")}");
            SetDataType("GameSettings (类)");
        }
        
        /// <summary>
        /// 显示通用数据
        /// </summary>
        private void ShowGenericData(object data)
        {
            SetTitle("通用数据面板");
            SetContent($"数据: {data}\n" +
                      $"类型: {data.GetType().Name}\n" +
                      $"命名空间: {data.GetType().Namespace}\n" +
                      $"ToString(): {data.ToString()}");
            SetDataType($"{data.GetType().Name} (未知类型)");
        }
        
        /// <summary>
        /// 设置标题
        /// </summary>
        private void SetTitle(string title)
        {
            if (titleText != null)
                titleText.text = title;
        }
        
        /// <summary>
        /// 设置内容
        /// </summary>
        private void SetContent(string content)
        {
            if (contentText != null)
                contentText.text = content;
        }
        
        /// <summary>
        /// 设置数据类型
        /// </summary>
        private void SetDataType(string dataType)
        {
            if (dataTypeText != null)
                dataTypeText.text = $"数据类型: {dataType}";
        }
        
        /// <summary>
        /// 确认按钮点击
        /// </summary>
        private void OnConfirmClick()
        {
            Debug.Log($"[GenericDataPanel] 确认操作，当前数据: {currentData}");
            
            // 可以在这里处理确认逻辑
            // 例如：保存数据、执行操作等
            
            Hide();
        }
        
        /// <summary>
        /// 取消按钮点击
        /// </summary>
        private void OnCancelClick()
        {
            Debug.Log($"[GenericDataPanel] 取消操作");
            Hide();
        }
        
        /// <summary>
        /// 获取当前数据
        /// </summary>
        public T GetCurrentData<T>()
        {
            if (currentData is T data)
                return data;
            
            return default(T);
        }
        
        /// <summary>
        /// 获取当前数据类型
        /// </summary>
        public System.Type GetCurrentDataType()
        {
            return currentDataType;
        }
        
        /// <summary>
        /// 检查数据类型
        /// </summary>
        public bool IsDataType<T>()
        {
            return currentDataType == typeof(T);
        }
        
        /// <summary>
        /// 重置到对象池状态
        /// </summary>
        protected override void OnResetToPool()
        {
            base.OnResetToPool();
            
            // 清理数据
            currentData = null;
            currentDataType = null;
            
            // 重置UI
            SetTitle("");
            SetContent("");
            SetDataType("");
        }
    }
}