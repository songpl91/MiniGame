using UnityEngine;
using Framework.SplUI.Core;
using Framework.SplUI.Examples;

namespace Framework.SplUI.Test
{
    /// <summary>
    /// 动画系统测试脚本
    /// 用于验证SplUI动画系统的各种动画类型是否正常工作
    /// </summary>
    public class AnimationTest : MonoBehaviour
    {
        [Header("测试配置")]
        [SerializeField] private ExamplePanel testPanel;
        [SerializeField] private float testInterval = 2.0f;
        
        [Header("动画类型测试")]
        [SerializeField] private SplUIAnimationType[] animationTypes = new SplUIAnimationType[]
        {
            SplUIAnimationType.None,
            SplUIAnimationType.Fade,
            SplUIAnimationType.Scale,
            SplUIAnimationType.FadeScale,
            SplUIAnimationType.Slide,
            SplUIAnimationType.SlideFromBottom,
            SplUIAnimationType.SlideFromTop,
            SplUIAnimationType.SlideFromLeft,
            SplUIAnimationType.SlideFromRight
        };
        
        private int currentAnimationIndex = 0;
        private bool isTestRunning = false;
        
        /// <summary>
        /// Unity Start方法
        /// </summary>
        private void Start()
        {
            // 如果没有指定测试面板，尝试查找
            if (testPanel == null)
            {
                testPanel = FindObjectOfType<ExamplePanel>();
            }
            
            if (testPanel == null)
            {
                Debug.LogError("[AnimationTest] 未找到ExamplePanel，无法进行动画测试");
                return;
            }
            
            Debug.Log("[AnimationTest] 动画测试系统已准备就绪");
            Debug.Log($"[AnimationTest] 可测试的动画类型数量: {animationTypes.Length}");
        }
        
        /// <summary>
        /// Unity Update方法
        /// </summary>
        private void Update()
        {
            // 按空格键开始/停止测试
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (isTestRunning)
                {
                    StopAnimationTest();
                }
                else
                {
                    StartAnimationTest();
                }
            }
            
            // 按数字键1-9测试特定动画
            for (int i = 1; i <= 9; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha0 + i))
                {
                    int animationIndex = i - 1;
                    if (animationIndex < animationTypes.Length)
                    {
                        TestSpecificAnimation(animationTypes[animationIndex]);
                    }
                }
            }
            
            // 按R键重置面板
            if (Input.GetKeyDown(KeyCode.R))
            {
                ResetPanel();
            }
        }
        
        /// <summary>
        /// 开始动画测试
        /// </summary>
        public void StartAnimationTest()
        {
            if (testPanel == null)
            {
                Debug.LogError("[AnimationTest] 测试面板为空，无法开始测试");
                return;
            }
            
            isTestRunning = true;
            currentAnimationIndex = 0;
            
            Debug.Log("[AnimationTest] 开始动画测试循环");
            InvokeRepeating(nameof(TestNextAnimation), 0f, testInterval);
        }
        
        /// <summary>
        /// 停止动画测试
        /// </summary>
        public void StopAnimationTest()
        {
            isTestRunning = false;
            CancelInvoke(nameof(TestNextAnimation));
            
            Debug.Log("[AnimationTest] 停止动画测试循环");
        }
        
        /// <summary>
        /// 测试下一个动画
        /// </summary>
        private void TestNextAnimation()
        {
            if (currentAnimationIndex >= animationTypes.Length)
            {
                currentAnimationIndex = 0;
                Debug.Log("[AnimationTest] 动画测试循环完成，重新开始");
            }
            
            SplUIAnimationType animationType = animationTypes[currentAnimationIndex];
            TestSpecificAnimation(animationType);
            
            currentAnimationIndex++;
        }
        
        /// <summary>
        /// 测试特定动画类型
        /// </summary>
        /// <param name="animationType">要测试的动画类型</param>
        public void TestSpecificAnimation(SplUIAnimationType animationType)
        {
            if (testPanel == null)
            {
                Debug.LogError("[AnimationTest] 测试面板为空，无法测试动画");
                return;
            }
            
            Debug.Log($"[AnimationTest] 测试动画类型: {animationType}");
            
            // 设置动画类型
            testPanel.ShowAnimation = animationType;
            testPanel.HideAnimation = GetCorrespondingHideAnimation(animationType);
            
            // 如果面板当前是显示状态，先隐藏再显示
            if (testPanel.IsShowing)
            {
                testPanel.Hide(() =>
                {
                    // 延迟一点时间再显示，确保隐藏动画完成
                    Invoke(nameof(ShowTestPanel), 0.1f);
                });
            }
            else
            {
                ShowTestPanel();
            }
        }
        
        /// <summary>
        /// 显示测试面板
        /// </summary>
        private void ShowTestPanel()
        {
            if (testPanel != null)
            {
                testPanel.Show();
            }
        }
        
        /// <summary>
        /// 获取对应的隐藏动画类型
        /// </summary>
        /// <param name="showAnimation">显示动画类型</param>
        /// <returns>对应的隐藏动画类型</returns>
        private SplUIAnimationType GetCorrespondingHideAnimation(SplUIAnimationType showAnimation)
        {
            switch (showAnimation)
            {
                case SplUIAnimationType.SlideFromBottom:
                    return SplUIAnimationType.SlideToBottom;
                case SplUIAnimationType.SlideFromTop:
                    return SplUIAnimationType.SlideToTop;
                case SplUIAnimationType.SlideFromLeft:
                    return SplUIAnimationType.SlideToLeft;
                case SplUIAnimationType.SlideFromRight:
                    return SplUIAnimationType.SlideToRight;
                default:
                    return showAnimation; // 其他动画类型显示和隐藏使用相同类型
            }
        }
        
        /// <summary>
        /// 重置面板状态
        /// </summary>
        public void ResetPanel()
        {
            if (testPanel == null)
                return;
            
            Debug.Log("[AnimationTest] 重置面板状态");
            
            // 停止当前动画
            testPanel.StopCurrentAnimation();
            
            // 隐藏面板
            testPanel.Hide();
            
            // 重置动画类型为默认值
            testPanel.ShowAnimation = SplUIAnimationType.FadeScale;
            testPanel.HideAnimation = SplUIAnimationType.FadeScale;
        }
        
        /// <summary>
        /// Unity OnGUI方法 - 显示测试说明
        /// </summary>
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("SplUI 动画测试", GUI.skin.box);
            GUILayout.Label("空格键: 开始/停止自动测试");
            GUILayout.Label("数字键1-9: 测试特定动画");
            GUILayout.Label("R键: 重置面板");
            GUILayout.Label($"当前状态: {(isTestRunning ? "测试中" : "已停止")}");
            
            if (testPanel != null)
            {
                GUILayout.Label($"面板状态: {(testPanel.IsShowing ? "显示" : "隐藏")}");
                GUILayout.Label($"当前动画: {testPanel.ShowAnimation}");
            }
            
            GUILayout.EndArea();
        }
    }
}