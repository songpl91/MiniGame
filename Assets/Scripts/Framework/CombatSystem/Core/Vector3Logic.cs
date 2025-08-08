using System;

namespace Framework.CombatSystem.Core
{
    /// <summary>
    /// 逻辑层的三维向量结构
    /// 完全独立于Unity，用于纯逻辑计算
    /// 体现：逻辑与表现分离的设计原则
    /// </summary>
    [Serializable]
    public struct Vector3Logic : IEquatable<Vector3Logic>
    {
        #region 字段
        
        public float X;
        public float Y;
        public float Z;
        
        #endregion
        
        #region 构造函数
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        /// <param name="z">Z坐标</param>
        public Vector3Logic(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        
        /// <summary>
        /// 构造函数（二维）
        /// </summary>
        /// <param name="x">X坐标</param>
        /// <param name="z">Z坐标</param>
        public Vector3Logic(float x, float z) : this(x, 0f, z)
        {
        }
        
        #endregion
        
        #region 静态属性
        
        /// <summary>
        /// 零向量
        /// </summary>
        public static Vector3Logic Zero => new Vector3Logic(0f, 0f, 0f);
        
        /// <summary>
        /// 单位向量
        /// </summary>
        public static Vector3Logic One => new Vector3Logic(1f, 1f, 1f);
        
        /// <summary>
        /// 前方向量
        /// </summary>
        public static Vector3Logic Forward => new Vector3Logic(0f, 0f, 1f);
        
        /// <summary>
        /// 后方向量
        /// </summary>
        public static Vector3Logic Back => new Vector3Logic(0f, 0f, -1f);
        
        /// <summary>
        /// 左方向量
        /// </summary>
        public static Vector3Logic Left => new Vector3Logic(-1f, 0f, 0f);
        
        /// <summary>
        /// 右方向量
        /// </summary>
        public static Vector3Logic Right => new Vector3Logic(1f, 0f, 0f);
        
        /// <summary>
        /// 上方向量
        /// </summary>
        public static Vector3Logic Up => new Vector3Logic(0f, 1f, 0f);
        
        /// <summary>
        /// 下方向量
        /// </summary>
        public static Vector3Logic Down => new Vector3Logic(0f, -1f, 0f);
        
        #endregion
        
        #region 属性
        
        /// <summary>
        /// 向量长度的平方
        /// </summary>
        public float SqrMagnitude => X * X + Y * Y + Z * Z;
        
        /// <summary>
        /// 向量长度
        /// </summary>
        public float Magnitude => (float)Math.Sqrt(SqrMagnitude);
        
        /// <summary>
        /// 归一化向量
        /// </summary>
        public Vector3Logic Normalized
        {
            get
            {
                float magnitude = Magnitude;
                if (magnitude > 0.00001f)
                {
                    return this / magnitude;
                }
                return Zero;
            }
        }
        
        #endregion
        
        #region 运算符重载
        
        /// <summary>
        /// 加法运算符
        /// </summary>
        public static Vector3Logic operator +(Vector3Logic a, Vector3Logic b)
        {
            return new Vector3Logic(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }
        
        /// <summary>
        /// 减法运算符
        /// </summary>
        public static Vector3Logic operator -(Vector3Logic a, Vector3Logic b)
        {
            return new Vector3Logic(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }
        
        /// <summary>
        /// 乘法运算符（标量）
        /// </summary>
        public static Vector3Logic operator *(Vector3Logic a, float scalar)
        {
            return new Vector3Logic(a.X * scalar, a.Y * scalar, a.Z * scalar);
        }
        
        /// <summary>
        /// 乘法运算符（标量）
        /// </summary>
        public static Vector3Logic operator *(float scalar, Vector3Logic a)
        {
            return a * scalar;
        }
        
        /// <summary>
        /// 除法运算符
        /// </summary>
        public static Vector3Logic operator /(Vector3Logic a, float scalar)
        {
            return new Vector3Logic(a.X / scalar, a.Y / scalar, a.Z / scalar);
        }
        
        /// <summary>
        /// 取反运算符
        /// </summary>
        public static Vector3Logic operator -(Vector3Logic a)
        {
            return new Vector3Logic(-a.X, -a.Y, -a.Z);
        }
        
        /// <summary>
        /// 相等运算符
        /// </summary>
        public static bool operator ==(Vector3Logic a, Vector3Logic b)
        {
            return a.Equals(b);
        }
        
        /// <summary>
        /// 不等运算符
        /// </summary>
        public static bool operator !=(Vector3Logic a, Vector3Logic b)
        {
            return !a.Equals(b);
        }
        
        #endregion
        
        #region 静态方法
        
        /// <summary>
        /// 计算两点间距离
        /// </summary>
        /// <param name="a">点A</param>
        /// <param name="b">点B</param>
        /// <returns>距离</returns>
        public static float Distance(Vector3Logic a, Vector3Logic b)
        {
            return (a - b).Magnitude;
        }
        
        /// <summary>
        /// 计算两点间距离的平方
        /// </summary>
        /// <param name="a">点A</param>
        /// <param name="b">点B</param>
        /// <returns>距离的平方</returns>
        public static float SqrDistance(Vector3Logic a, Vector3Logic b)
        {
            return (a - b).SqrMagnitude;
        }
        
        /// <summary>
        /// 点积
        /// </summary>
        /// <param name="a">向量A</param>
        /// <param name="b">向量B</param>
        /// <returns>点积结果</returns>
        public static float Dot(Vector3Logic a, Vector3Logic b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }
        
        /// <summary>
        /// 叉积
        /// </summary>
        /// <param name="a">向量A</param>
        /// <param name="b">向量B</param>
        /// <returns>叉积结果</returns>
        public static Vector3Logic Cross(Vector3Logic a, Vector3Logic b)
        {
            return new Vector3Logic(
                a.Y * b.Z - a.Z * b.Y,
                a.Z * b.X - a.X * b.Z,
                a.X * b.Y - a.Y * b.X
            );
        }
        
        /// <summary>
        /// 线性插值
        /// </summary>
        /// <param name="a">起始向量</param>
        /// <param name="b">目标向量</param>
        /// <param name="t">插值参数（0-1）</param>
        /// <returns>插值结果</returns>
        public static Vector3Logic Lerp(Vector3Logic a, Vector3Logic b, float t)
        {
            t = Math.Max(0f, Math.Min(1f, t)); // 限制在0-1范围内
            return a + (b - a) * t;
        }
        
        #endregion
        
        #region IEquatable实现
        
        /// <summary>
        /// 判断是否相等
        /// </summary>
        /// <param name="other">另一个向量</param>
        /// <returns>是否相等</returns>
        public bool Equals(Vector3Logic other)
        {
            const float epsilon = 0.00001f;
            return Math.Abs(X - other.X) < epsilon &&
                   Math.Abs(Y - other.Y) < epsilon &&
                   Math.Abs(Z - other.Z) < epsilon;
        }
        
        /// <summary>
        /// 重写Equals方法
        /// </summary>
        /// <param name="obj">比较对象</param>
        /// <returns>是否相等</returns>
        public override bool Equals(object obj)
        {
            return obj is Vector3Logic other && Equals(other);
        }
        
        /// <summary>
        /// 重写GetHashCode方法
        /// </summary>
        /// <returns>哈希码</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + X.GetHashCode();
                hash = hash * 23 + Y.GetHashCode();
                hash = hash * 23 + Z.GetHashCode();
                return hash;
            }
        }
        
        #endregion
        
        #region 字符串表示
        
        /// <summary>
        /// 转换为字符串
        /// </summary>
        /// <returns>字符串表示</returns>
        public override string ToString()
        {
            return $"({X:F2}, {Y:F2}, {Z:F2})";
        }
        
        /// <summary>
        /// 转换为字符串（指定精度）
        /// </summary>
        /// <param name="format">格式字符串</param>
        /// <returns>字符串表示</returns>
        public string ToString(string format)
        {
            return $"({X.ToString(format)}, {Y.ToString(format)}, {Z.ToString(format)})";
        }
        
        #endregion
    }
}