using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

  public static partial class Utility
    {
        /// <summary>
        /// 格式化相关的实用函数。
        /// </summary>
        public static class Format
        {
            /// <summary>
            /// 格式化文件大小
            /// </summary>
            /// <param name="bytes">字节数</param>
            /// <returns>格式化的文件大小</returns>
            public static string FileSize(long bytes)
            {
                string[] sizes = { "B", "KB", "MB", "GB" };
                double len = bytes;
                int order = 0;
        
                while (len >= 1024 && order < sizes.Length - 1)
                {
                    order++;
                    len = len / 1024;
                }
        
                return $"{len:0.##} {sizes[order]}";
            }
            
            /// <summary>
            /// 格式化货币数量
            /// </summary>
            /// <param name="count">货币数量</param>
            /// <returns>格式化后的货币</returns>
            public static string Currency(int count)
            {
                if (count < 1000)
                {
                    return count.ToString();
                }

                return (count / 1000.0f).ToString("0.#") + "k";
            }
        }
    }