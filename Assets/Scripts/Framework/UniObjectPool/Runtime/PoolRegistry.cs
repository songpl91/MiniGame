using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniFramework.ObjectPool
{
    /// <summary>
    /// 对象池注册器
    /// 提供更智能的对象池名称管理和查找机制
    /// </summary>
    public static class PoolRegistry
    {
        /// <summary>
        /// 对象池注册信息
        /// </summary>
        public class PoolRegistration
        {
            /// <summary>
            /// 对象池名称
            /// </summary>
            public string PoolName { get; set; }
            
            /// <summary>
            /// 预制体引用
            /// </summary>
            public GameObject Prefab { get; set; }
            
            /// <summary>
            /// 父对象引用
            /// </summary>
            public Transform Parent { get; set; }
            
            /// <summary>
            /// 对象类型
            /// </summary>
            public Type ObjectType { get; set; }
            
            /// <summary>
            /// 注册时间
            /// </summary>
            public DateTime RegisterTime { get; set; }
            
            /// <summary>
            /// 自定义标签
            /// </summary>
            public string[] Tags { get; set; }
        }

        private static readonly Dictionary<string, PoolRegistration> _registrations = new Dictionary<string, PoolRegistration>();
        private static readonly Dictionary<string, List<string>> _prefabPathToPoolNames = new Dictionary<string, List<string>>();
        private static readonly Dictionary<int, string> _instanceIdToPoolName = new Dictionary<int, string>(); // 保留作为快速查找的缓存
        private static readonly Dictionary<int, List<string>> _gameObjectToPoolNames = new Dictionary<int, List<string>>();
        private static readonly object _lockObject = new object();

        /// <summary>
        /// 获取预制体的唯一标识路径
        /// 优先使用 AssetDatabase 路径，运行时回退到名称+InstanceID
        /// </summary>
        /// <param name="prefab">预制体</param>
        /// <returns>唯一标识路径</returns>
        private static string GetPrefabIdentifier(GameObject prefab)
        {
            if (prefab == null)
                return null;

#if UNITY_EDITOR
            // 编辑器模式下使用资源路径作为稳定标识
            string assetPath = UnityEditor.AssetDatabase.GetAssetPath(prefab);
            if (!string.IsNullOrEmpty(assetPath))
            {
                return assetPath;
            }
#endif
            // 运行时回退方案：名称 + InstanceID
            return $"{prefab.name}_{prefab.GetInstanceID()}";
        }

        /// <summary>
        /// 注册对象池
        /// </summary>
        /// <param name="poolName">对象池名称</param>
        /// <param name="prefab">预制体</param>
        /// <param name="parent">父对象</param>
        /// <param name="objectType">对象类型</param>
        /// <param name="tags">自定义标签</param>
        public static void RegisterPool(string poolName, GameObject prefab, Transform parent = null, Type objectType = null, params string[] tags)
        {
            if (string.IsNullOrEmpty(poolName))
                throw new ArgumentException("对象池名称不能为空", nameof(poolName));

            lock (_lockObject)
            {
                var registration = new PoolRegistration
                {
                    PoolName = poolName,
                    Prefab = prefab,
                    Parent = parent,
                    ObjectType = objectType ?? typeof(GameObject),
                    RegisterTime = DateTime.Now,
                    Tags = tags ?? new string[0]
                };

                _registrations[poolName] = registration;

                // 建立预制体到对象池名称的映射
                if (prefab != null)
                {
                    // 使用稳定的预制体标识符
                    string prefabIdentifier = GetPrefabIdentifier(prefab);
                    if (!string.IsNullOrEmpty(prefabIdentifier))
                    {
                        if (!_prefabPathToPoolNames.ContainsKey(prefabIdentifier))
                        {
                            _prefabPathToPoolNames[prefabIdentifier] = new List<string>();
                        }
                        if (!_prefabPathToPoolNames[prefabIdentifier].Contains(poolName))
                        {
                            _prefabPathToPoolNames[prefabIdentifier].Add(poolName);
                        }
                    }

                    // 保留 InstanceID 映射作为快速查找缓存（仅用于运行时）
                    int prefabId = prefab.GetInstanceID();
                    _instanceIdToPoolName[prefabId] = poolName; // 如果有多个池，这里只记录最后一个
                }

                UniLogger.Log($"注册对象池: {poolName}, 预制体: {prefab?.name ?? "None"}");
            }
        }

        /// <summary>
        /// 注销对象池
        /// </summary>
        /// <param name="poolName">对象池名称</param>
        public static void UnregisterPool(string poolName)
        {
            if (string.IsNullOrEmpty(poolName))
                return;

            lock (_lockObject)
            {
                if (_registrations.TryGetValue(poolName, out var registration))
                {
                    _registrations.Remove(poolName);

                    // 移除预制体映射
                    if (registration.Prefab != null)
                    {
                        // 移除稳定标识符映射
                        string prefabIdentifier = GetPrefabIdentifier(registration.Prefab);
                        if (!string.IsNullOrEmpty(prefabIdentifier) && _prefabPathToPoolNames.ContainsKey(prefabIdentifier))
                        {
                            _prefabPathToPoolNames[prefabIdentifier].Remove(poolName);
                            if (_prefabPathToPoolNames[prefabIdentifier].Count == 0)
                            {
                                _prefabPathToPoolNames.Remove(prefabIdentifier);
                            }
                        }

                        // 移除 InstanceID 缓存
                        int prefabId = registration.Prefab.GetInstanceID();
                        _instanceIdToPoolName.Remove(prefabId);
                    }

                    UniLogger.Log($"注销对象池: {poolName}");
                }
            }
        }

        /// <summary>
        /// 根据预制体查找对象池名称（返回第一个匹配的）
        /// </summary>
        /// <param name="prefab">预制体</param>
        /// <param name="parent">父对象（可选，用于进一步筛选）</param>
        /// <returns>对象池名称，如果未找到则返回null</returns>
        public static string FindPoolNameByPrefab(GameObject prefab, Transform parent = null)
        {
            var poolNames = FindPoolNamesByPrefab(prefab, parent);
            return poolNames.Count > 0 ? poolNames[0] : null;
        }

        /// <summary>
        /// 根据预制体查找所有关联的对象池名称
        /// </summary>
        /// <param name="prefab">预制体</param>
        /// <param name="parent">父对象（可选，用于进一步筛选）</param>
        /// <returns>对象池名称列表</returns>
        public static List<string> FindPoolNamesByPrefab(GameObject prefab, Transform parent = null)
        {
            var result = new List<string>();
            if (prefab == null)
                return result;

            lock (_lockObject)
            {
                // 首先尝试使用稳定标识符查找
                string prefabIdentifier = GetPrefabIdentifier(prefab);
                if (!string.IsNullOrEmpty(prefabIdentifier) && _prefabPathToPoolNames.TryGetValue(prefabIdentifier, out var poolNames))
                {
                    foreach (var poolName in poolNames)
                    {
                        // 如果指定了父对象，进一步验证
                        if (parent != null && _registrations.TryGetValue(poolName, out var registration))
                        {
                            if (registration.Parent == parent)
                            {
                                result.Add(poolName);
                            }
                        }
                        else
                        {
                            result.Add(poolName);
                        }
                    }
                }

                // 如果稳定标识符查找失败，回退到 InstanceID 缓存
                if (result.Count == 0)
                {
                    int prefabId = prefab.GetInstanceID();
                    if (_instanceIdToPoolName.TryGetValue(prefabId, out string poolName))
                    {
                        // 如果指定了父对象，进一步验证
                        if (parent != null && _registrations.TryGetValue(poolName, out var registration))
                        {
                            if (registration.Parent == parent)
                            {
                                result.Add(poolName);
                            }
                        }
                        else
                        {
                            result.Add(poolName);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 根据游戏对象查找可能的对象池名称
        /// </summary>
        /// <param name="gameObject">游戏对象</param>
        /// <returns>可能的对象池名称列表</returns>
        public static List<string> FindPossiblePoolNames(GameObject gameObject)
        {
            var result = new List<string>();
            if (gameObject == null)
                return result;

            lock (_lockObject)
            {
                string cleanName = gameObject.name.Replace("(Clone)", "");
                
                // 查找名称匹配的对象池
                foreach (var kvp in _registrations)
                {
                    var registration = kvp.Value;
                    if (registration.Prefab != null && registration.Prefab.name == cleanName)
                    {
                        result.Add(kvp.Key);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 根据标签查找对象池
        /// </summary>
        /// <param name="tag">标签</param>
        /// <returns>匹配的对象池名称列表</returns>
        public static List<string> FindPoolsByTag(string tag)
        {
            var result = new List<string>();
            if (string.IsNullOrEmpty(tag))
                return result;

            lock (_lockObject)
            {
                foreach (var kvp in _registrations)
                {
                    var registration = kvp.Value;
                    if (registration.Tags != null)
                    {
                        for (int i = 0; i < registration.Tags.Length; i++)
                        {
                            if (registration.Tags[i] == tag)
                            {
                                result.Add(kvp.Key);
                                break;
                            }
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 获取对象池注册信息
        /// </summary>
        /// <param name="poolName">对象池名称</param>
        /// <returns>注册信息，如果未找到则返回null</returns>
        public static PoolRegistration GetRegistration(string poolName)
        {
            if (string.IsNullOrEmpty(poolName))
                return null;

            lock (_lockObject)
            {
                return _registrations.TryGetValue(poolName, out var registration) ? registration : null;
            }
        }

        /// <summary>
        /// 获取所有注册的对象池信息
        /// </summary>
        /// <returns>所有注册信息的副本</returns>
        public static Dictionary<string, PoolRegistration> GetAllRegistrations()
        {
            lock (_lockObject)
            {
                return new Dictionary<string, PoolRegistration>(_registrations);
            }
        }

        /// <summary>
        /// 清空所有注册信息
        /// </summary>
        public static void Clear()
        {
            lock (_lockObject)
            {
                _registrations.Clear();
                _prefabPathToPoolNames.Clear();
                _instanceIdToPoolName.Clear();
                _gameObjectToPoolNames.Clear();
                UniLogger.Log("清空所有对象池注册信息");
            }
        }

        /// <summary>
        /// 生成默认的对象池名称（简单规则：预制体名称 + Pool）
        /// </summary>
        /// <param name="prefab">预制体</param>
        /// <returns>默认的对象池名称</returns>
        public static string GenerateDefaultPoolName(GameObject prefab)
        {
            if (prefab == null)
                return "UnknownPool";
            
            string baseName = prefab.name;
            
            // 确保以Pool结尾
            if (!baseName.EndsWith("Pool"))
            {
                baseName += "Pool";
            }
            
            return baseName;
        }

        /// <summary>
        /// 检查对象池名称是否已存在
        /// </summary>
        /// <param name="poolName">对象池名称</param>
        /// <returns>是否已存在</returns>
        public static bool IsPoolNameExists(string poolName)
        {
            if (string.IsNullOrEmpty(poolName))
                return false;

            lock (_lockObject)
            {
                return _registrations.ContainsKey(poolName);
            }
        }
    }
}