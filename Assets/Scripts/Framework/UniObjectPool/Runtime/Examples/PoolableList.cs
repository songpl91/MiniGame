using System.Collections.Generic;

namespace UniFramework.ObjectPool.Examples
{
    /// <summary>
    /// 可池化的泛型列表示例
    /// 展示如何池化集合类型
    /// </summary>
    /// <typeparam name="T">列表元素类型</typeparam>
    public class PoolableList<T> : IPoolable
    {
        private List<T> _list;

        /// <summary>
        /// 内部列表实例
        /// </summary>
        public List<T> List => _list;

        /// <summary>
        /// 列表元素数量
        /// </summary>
        public int Count => _list?.Count ?? 0;

        /// <summary>
        /// 列表容量
        /// </summary>
        public int Capacity
        {
            get => _list?.Capacity ?? 0;
            set
            {
                if (_list != null)
                    _list.Capacity = value;
            }
        }

        /// <summary>
        /// 索引器
        /// </summary>
        /// <param name="index">索引</param>
        /// <returns>元素</returns>
        public T this[int index]
        {
            get => _list[index];
            set => _list[index] = value;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public PoolableList()
        {
            _list = new List<T>();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="capacity">初始容量</param>
        public PoolableList(int capacity)
        {
            _list = new List<T>(capacity);
        }

        /// <summary>
        /// 从对象池取出时调用
        /// </summary>
        public void OnSpawn()
        {
            // 列表已经在构造函数中创建，这里不需要额外操作
        }

        /// <summary>
        /// 归还到对象池时调用
        /// </summary>
        public void OnDespawn()
        {
            // 清空列表内容，准备下次使用
            _list?.Clear();
        }

        /// <summary>
        /// 添加元素
        /// </summary>
        /// <param name="item">要添加的元素</param>
        public void Add(T item)
        {
            _list?.Add(item);
        }

        /// <summary>
        /// 添加多个元素
        /// </summary>
        /// <param name="items">要添加的元素集合</param>
        public void AddRange(IEnumerable<T> items)
        {
            _list?.AddRange(items);
        }

        /// <summary>
        /// 插入元素
        /// </summary>
        /// <param name="index">插入位置</param>
        /// <param name="item">要插入的元素</param>
        public void Insert(int index, T item)
        {
            _list?.Insert(index, item);
        }

        /// <summary>
        /// 移除元素
        /// </summary>
        /// <param name="item">要移除的元素</param>
        /// <returns>是否成功移除</returns>
        public bool Remove(T item)
        {
            return _list?.Remove(item) ?? false;
        }

        /// <summary>
        /// 移除指定位置的元素
        /// </summary>
        /// <param name="index">要移除的位置</param>
        public void RemoveAt(int index)
        {
            _list?.RemoveAt(index);
        }

        /// <summary>
        /// 移除指定范围的元素
        /// </summary>
        /// <param name="index">开始位置</param>
        /// <param name="count">移除数量</param>
        public void RemoveRange(int index, int count)
        {
            _list?.RemoveRange(index, count);
        }

        /// <summary>
        /// 清空列表
        /// </summary>
        public void Clear()
        {
            _list?.Clear();
        }

        /// <summary>
        /// 检查是否包含指定元素
        /// </summary>
        /// <param name="item">要检查的元素</param>
        /// <returns>是否包含</returns>
        public bool Contains(T item)
        {
            return _list?.Contains(item) ?? false;
        }

        /// <summary>
        /// 查找元素的索引
        /// </summary>
        /// <param name="item">要查找的元素</param>
        /// <returns>元素索引，如果不存在则返回 -1</returns>
        public int IndexOf(T item)
        {
            return _list?.IndexOf(item) ?? -1;
        }

        /// <summary>
        /// 复制到数组
        /// </summary>
        /// <param name="array">目标数组</param>
        /// <param name="arrayIndex">开始位置</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            _list?.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// 转换为数组
        /// </summary>
        /// <returns>数组</returns>
        public T[] ToArray()
        {
            return _list?.ToArray() ?? new T[0];
        }

        /// <summary>
        /// 获取枚举器
        /// </summary>
        /// <returns>枚举器</returns>
        public List<T>.Enumerator GetEnumerator()
        {
            return _list?.GetEnumerator() ?? new List<T>().GetEnumerator();
        }

        /// <summary>
        /// 隐式转换为 List<T>
        /// </summary>
        /// <param name="poolableList">可池化列表</param>
        public static implicit operator List<T>(PoolableList<T> poolableList)
        {
            return poolableList?._list;
        }

        /// <summary>
        /// 创建 PoolableList<T> 对象池的静态方法
        /// </summary>
        /// <param name="initialCapacity">初始容量</param>
        /// <param name="config">对象池配置</param>
        /// <returns>对象池实例</returns>
        public static UniObjectPool<PoolableList<T>> CreatePool(
            int initialCapacity = 16,
            PoolConfig config = null)
        {
            return PoolManager.CreatePool(
                poolName: $"PoolableList_{typeof(T).Name}",
                createFunc: () => new PoolableList<T>(initialCapacity),
                resetAction: null, // 使用 IPoolable 接口的方法
                destroyAction: null,
                config: config ?? PoolConfig.CreateHighPerformance()
            );
        }
    }
}