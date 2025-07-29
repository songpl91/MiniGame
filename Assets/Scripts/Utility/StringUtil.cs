using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

// 【正则表达式】
// 是否邮件:   \w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*
// 是否数字:   ^[1-9]*[0-9]*$
// 是否字符：  ^.[A-Za-z]+$
// 是否手机号: ^((\(\d{3}\))|(\d{3}\-))?13[0-9]\d{8}|15[0-9]\d{8}|18[0-9]\d{8}
// 身份证15位: ^(\d{6})(\d{2})(\d{2})(\d{2})(\d{3})_
// 身份证18位: ^(\d{6})(\d{4})(\d{2})(\d{2})(\d{3})([0-9Xx])_

// 字符串管理;
    public class StringUtil
    {
        /// <summary>
        /// 检查一个字符串是否纯数字构成
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsNumberID(string inputstr)
        {
            return QuickValidate("^[1-9]*[0-9]*$", inputstr);
        }

        /// <summary>
        /// 验证一个字符串是否符合制定的正则表达式;
        /// </summary>
        /// <param name="experss"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool QuickValidate(string experss, string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(experss);
            return regex.IsMatch(value);
        }

        /// <summary>
        /// 通过正则表达式保留字符;
        /// </summary>
        /// <param name="experss"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string FiltrationString(string experss, string value)
        {
            if (string.IsNullOrEmpty(experss) || string.IsNullOrEmpty(value))
                return "未知";

            System.Text.StringBuilder sb = new System.Text.StringBuilder("");
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(experss);
            System.Text.RegularExpressions.MatchCollection mc = regex.Matches(value);
            for (int idx = 0; idx < mc.Count; idx++)
                sb.Append(mc[idx].Value.ToString());

            return sb.ToString();
        }

        /// <summary>
        /// 替换制定字符串;
        /// </summary>
        /// <param name="strSource"></param>
        /// <param name="strOld"></param>
        /// <param name="strNew"></param>
        /// <returns></returns>
        public static string ReplaceString(string strSource, string strOld, string strNew)
        {
            string Result = System.Text.RegularExpressions.Regex.Replace(strSource, strOld, strNew);
            return Result;
        }

        /// <summary>
        /// 返回隐藏中间的字符串
        /// </summary>
        /// <param name="Input">输入</param>
        /// <returns>输出</returns>
        public static string GetRepleseStr(string Input, int Start, int Length, string ReplaceStr)
        {
            string Test = "";
            string Output = Input.Substring(Start, Length);
            for (int i = 0; i < Output.Length; i++)
            {
                Test += ReplaceStr;
            }

            return Input.Replace(Output, Test);
        }


        /// <summary>
        /// 获取字符长度，中文汉字为2字节定义;
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int StringLength(string inputstr)
        {
            int Len = 0;
            System.Text.ASCIIEncoding ascii = new System.Text.ASCIIEncoding();
            byte[] s = ascii.GetBytes(inputstr);
            for (int i = 0; i < s.Length; i++)
            {
                if ((int)s[i] == 63)
                    Len += 2;
                else
                    Len += 1;
            }

            return Len;
        }

        /// <summary>
        /// 截取指定长度字符串;
        /// </summary>
        /// <param name="value"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public static string ClipString(string inputString, int len)
        {
            System.Text.ASCIIEncoding ascii = new System.Text.ASCIIEncoding();
            int tempLen = 0;
            string tempString = "";
            byte[] s = ascii.GetBytes(inputString);
            for (int i = 0; i < s.Length; i++)
            {
                if ((int)s[i] == 63)
                {
                    tempLen += 2;
                }
                else
                {
                    tempLen += 1;
                }

                try
                {
                    tempString += inputString.Substring(i, 1);
                }
                catch
                {
                    break;
                }

                if (tempLen >= len)
                {
                    break;
                }
            }

            int Count = System.Text.Encoding.Unicode.GetByteCount(inputString);
            if (Count > len)
                tempString += "...";
            return tempString;
        }

        /// <summary>
        /// 获取某字符串在另个字符串出现的次数;
        /// </summary>
        /// <param name="strOriginal"></param>
        /// <param name="strSymbol"></param>
        /// <returns></returns>
        public static int GetStrCount(string strOriginal, string strSymbol)
        {
            int count = 0;
            for (int i = 0; i < (strOriginal.Length - strSymbol.Length + 1); i++)
            {
                if (strOriginal.Substring(i, strSymbol.Length) == strSymbol)
                {
                    count = count + 1;
                }
            }

            return count;
        }

        /// <summary>
        ///  获取一个字符在一个字符串最后位置位置索引
        /// </summary>
        /// <param name="strSource"></param>
        /// <param name="idxstr"></param>
        /// <returns></returns>
        public static int GetLastIndexOf(string strSource, string idxstr)
        {
            return strSource.LastIndexOf(idxstr, StringComparison.Ordinal);
        }

        /// <summary>
        /// 字符串转换
        /// </summary>
        /// <param name="data"></param>
        /// <param name="func"></param>
        public static string ByteToString(byte[] sByte)
        {
            string str = System.Text.Encoding.UTF8.GetString(sByte);
            if (string.IsNullOrEmpty(str))
                return "";
            str = str.Trim();
            return str;
        }

        /// <summary>
        /// 计算字符串16位的MD5值
        /// </summary>
        public static string Get16Md5Str(string ConvertString)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            string t2 = BitConverter.ToString(md5.ComputeHash(Encoding.Default.GetBytes(ConvertString)), 4, 8);
            t2 = t2.Replace("-", "");
            return t2;
        }

        /// <summary>
        /// 计算字符串的MD5值
        /// </summary>
        public static string md5(string source)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] data = System.Text.Encoding.UTF8.GetBytes(source);
            byte[] md5Data = md5.ComputeHash(data, 0, data.Length);
            md5.Clear();

            string destString =
                md5Data.Aggregate("", (current, t) => current + System.Convert.ToString(t, 16).PadLeft(2, '0'));

            destString = destString.PadLeft(32, '0');
            return destString;
        }

        /// <summary>
        /// 将大数转换成k m 形式
        /// </summary>
        /// <param name="Num"></param>
        /// <returns></returns>
        public static string GetNumKMStr(int Num)
        {
            if (Num >= 1000000)
            {
                float tempNum = (float)Num / 1000000f;
                return ((int)(tempNum * 10) / 10.0f) + "m";
            }

            if (Num >= 1000)
            {
                float tempNum = (float)Num / 1000f;
                return ((int)(tempNum * 10) / 10.0f) + "k";
            }

            return Num.ToString();
        }
    }
