using System.Text;

namespace UniFramework.ObjectPool.Examples
{
    /// <summary>
    /// 可池化的 StringBuilder 示例
    /// 展示如何实现 IPoolable 接口
    /// </summary>
    public class PoolableStringBuilder : IPoolable
    {
        private StringBuilder _stringBuilder;

        /// <summary>
        /// StringBuilder 实例
        /// </summary>
        public StringBuilder StringBuilder => _stringBuilder;

        /// <summary>
        /// 构造函数
        /// </summary>
        public PoolableStringBuilder()
        {
            _stringBuilder = new StringBuilder();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="capacity">初始容量</param>
        public PoolableStringBuilder(int capacity)
        {
            _stringBuilder = new StringBuilder(capacity);
        }

        /// <summary>
        /// 从对象池取出时调用
        /// </summary>
        public void OnSpawn()
        {
            // StringBuilder 已经在构造函数中创建，这里不需要额外操作
        }

        /// <summary>
        /// 归还到对象池时调用
        /// </summary>
        public void OnDespawn()
        {
            // 清空 StringBuilder 内容，准备下次使用
            _stringBuilder?.Clear();
        }

        /// <summary>
        /// 追加字符串
        /// </summary>
        /// <param name="value">要追加的字符串</param>
        /// <returns>当前实例</returns>
        public PoolableStringBuilder Append(string value)
        {
            _stringBuilder?.Append(value);
            return this;
        }

        /// <summary>
        /// 追加字符
        /// </summary>
        /// <param name="value">要追加的字符</param>
        /// <returns>当前实例</returns>
        public PoolableStringBuilder Append(char value)
        {
            _stringBuilder?.Append(value);
            return this;
        }

        /// <summary>
        /// 追加整数
        /// </summary>
        /// <param name="value">要追加的整数</param>
        /// <returns>当前实例</returns>
        public PoolableStringBuilder Append(int value)
        {
            _stringBuilder?.Append(value);
            return this;
        }

        /// <summary>
        /// 追加浮点数
        /// </summary>
        /// <param name="value">要追加的浮点数</param>
        /// <returns>当前实例</returns>
        public PoolableStringBuilder Append(float value)
        {
            _stringBuilder?.Append(value);
            return this;
        }

        /// <summary>
        /// 插入字符串
        /// </summary>
        /// <param name="index">插入位置</param>
        /// <param name="value">要插入的字符串</param>
        /// <returns>当前实例</returns>
        public PoolableStringBuilder Insert(int index, string value)
        {
            _stringBuilder?.Insert(index, value);
            return this;
        }

        /// <summary>
        /// 移除指定范围的字符
        /// </summary>
        /// <param name="startIndex">开始位置</param>
        /// <param name="length">移除长度</param>
        /// <returns>当前实例</returns>
        public PoolableStringBuilder Remove(int startIndex, int length)
        {
            _stringBuilder?.Remove(startIndex, length);
            return this;
        }

        /// <summary>
        /// 替换字符串
        /// </summary>
        /// <param name="oldValue">要替换的字符串</param>
        /// <param name="newValue">新字符串</param>
        /// <returns>当前实例</returns>
        public PoolableStringBuilder Replace(string oldValue, string newValue)
        {
            _stringBuilder?.Replace(oldValue, newValue);
            return this;
        }

        /// <summary>
        /// 获取字符串长度
        /// </summary>
        public int Length => _stringBuilder?.Length ?? 0;

        /// <summary>
        /// 获取或设置容量
        /// </summary>
        public int Capacity
        {
            get => _stringBuilder?.Capacity ?? 0;
            set
            {
                if (_stringBuilder != null)
                    _stringBuilder.Capacity = value;
            }
        }

        /// <summary>
        /// 清空内容
        /// </summary>
        /// <returns>当前实例</returns>
        public PoolableStringBuilder Clear()
        {
            _stringBuilder?.Clear();
            return this;
        }

        /// <summary>
        /// 转换为字符串
        /// </summary>
        /// <returns>字符串表示</returns>
        public override string ToString()
        {
            return _stringBuilder?.ToString() ?? string.Empty;
        }

        /// <summary>
        /// 隐式转换为字符串
        /// </summary>
        /// <param name="poolableStringBuilder">可池化的 StringBuilder</param>
        public static implicit operator string(PoolableStringBuilder poolableStringBuilder)
        {
            return poolableStringBuilder?.ToString() ?? string.Empty;
        }

        /// <summary>
        /// 创建 PoolableStringBuilder 对象池的静态方法
        /// </summary>
        /// <param name="initialCapacity">初始容量</param>
        /// <param name="config">对象池配置</param>
        /// <returns>对象池实例</returns>
        public static UniObjectPool<PoolableStringBuilder> CreatePool(
            int initialCapacity = 256,
            PoolConfig config = null)
        {
            return PoolManager.CreatePool(
                createFunc: () => new PoolableStringBuilder(initialCapacity),
                resetAction: null, // 使用 IPoolable 接口的方法
                destroyAction: null,
                config: config ?? PoolConfig.CreateHighPerformance()
            );
        }
    }
}