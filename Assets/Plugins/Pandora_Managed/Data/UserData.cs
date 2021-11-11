using UnityEngine;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace com.tencent.pandora
{
    public class UserData : Dictionary<string, string>
    {
        public UserData()
        {
            this["sRoleId"] = string.Empty; 
            this["sOpenId"] = string.Empty;
            this["sServiceType"] = string.Empty;
            this["sAcountType"] = string.Empty;
            this["sArea"] = string.Empty;
            this["sPartition"] = string.Empty;
            this["sAppId"] = string.Empty;
            this["sAccessToken"] = string.Empty;
            this["sPayToken"] = string.Empty;
            this["sGameVer"] = string.Empty;
            this["sPlatID"] = string.Empty;
            this["sQQInstalled"] = string.Empty;
            this["sWXInstalled"] = string.Empty;
            this["sGameName"] = string.Empty;
            this["sMSDKGameID"] = string.Empty;//游戏在MSDK平台注册的应用ID
            this["sSdkVersion"] = string.Empty;
            this["sLevel"] = string.Empty;
            this["sVender"] = string.Empty;
            this["sSign"] = string.Empty;  //传递给业务层做完整性校验
            this["sBrokerHost"] = string.Empty;
            this["sBrokerIp"] = string.Empty;
            this["sBrokerPort"] = string.Empty;
        }

        //以roleid作为账号切换的唯一判定
        public string sRoleId
        {
            get { return this["sRoleId"]; }
            set { this["sRoleId"] = value; }
        }

        public string sOpenId
        {
            get { return this["sOpenId"]; }
            set { this["sOpenId"] = value; }
        }

        public string sServiceType
        {
            get { return this["sServiceType"]; }
            set { this["sServiceType"] = value; }
        }

        public string sAcountType
        {
            get { return this["sAcountType"]; }
            set { this["sAcountType"] = value; }
        }

        public string sArea
        {
            get { return this["sArea"]; }
            set { this["sArea"] = value; }
        }

        public string sPartition
        {
            get { return this["sPartition"]; }
            set { this["sPartition"] = value; }
        }

        public string sAppId
        {
            get { return this["sAppId"]; }
            set { this["sAppId"] = value; }
        }

        public string sAccessToken
        {
            get { return this["sAccessToken"]; }
            set { this["sAccessToken"] = value; }
        }

        public string sPayToken
        {
            get { return this["sPayToken"]; }
            set { this["sPayToken"] = value; }
        }

        public string sGameVer
        {
            get { return this["sGameVer"]; }
            set { this["sGameVer"] = value; }
        }

        public string sPlatID
        {
            get { return this["sPlatID"]; }
            set { this["sPlatID"] = value; }
        }

        public string sQQInstalled
        {
            get { return this["sQQInstalled"]; }
            set { this["sQQInstalled"] = value; }
        }

        public string sWXInstalled
        {
            get { return this["sWXInstalled"]; }
            set { this["sWXInstalled"] = value; }
        }

        public string sGameName
        {
            get { return this["sGameName"]; }
            set { this["sGameName"] = value; }
        }

        public string sMSDKGameID
        {
            get { return this["sMSDKGameID"]; }
            set { this["sMSDKGameID"] = value; }
        }

        public string sSdkVersion
        {
            get { return this["sSdkVersion"]; }
            set { this["sSdkVersion"] = value; }
        }

        public string sLevel
        {
            get { return this["sLevel"]; }
            set { this["sLevel"] = value; }
        }

        public string sVender
        {
            get { return this["sVender"]; }
            set { this["sVender"] = value; }
        }

        public string sSign
        {
            get { return this["sSign"]; }
            set { this["sSign"] = value; }
        }

        public string sBrokerHost
        {
            get { return this["sBrokerHost"]; }
            set { this["sBrokerHost"] = value; }
        }

        public string sBrokerIp
        {
            get { return this["sBrokerIp"]; }
            set { this["sBrokerIp"] = value; }
        }

        public string sBrokerPort
        {
            get { return this["sBrokerPort"]; }
            set { this["sBrokerPort"] = value; }
        }

        /// <summary>
        /// sOS 表示操作系统类型，在此将平台类型转换为操作系统类型
        /// ITOP约定的操作系统类型： 1 - Android, 2 - iOS, 3 - Web，4 - Linux, 5 - windows
        /// IDIP约定的平台类型：0 -iOS  1 - Android, 我们对接SDK时遵循这一规则
        /// </summary>
        public string sOS
        {
            get
            {
                if (this.sPlatID == "0")
                {
                    return "2";
                }

                if (this.sPlatID == "1")
                {
                    return "1";
                }

                return "";
            }
        }

        /// <summary>
        /// sChannelID 表示登录渠道，ITOP的渠道约定和IDIP的大区约定一致，直接返回大区字段
        /// ITOP约定的登录渠道： 1 - 微信(WeChat)，2 - 手Q(mqq)，3 - 游客(Guest)，4 - Facebook，5 - GameCenter, 6 - GooglePlay, 100 - 自建账号体系
        /// </summary>
        public string sChannelID
        {
            get
            {
                return this.sArea;
            }
        }

        // 是否为空
        public bool IsRoleEmpty()
        {
            return string.IsNullOrEmpty(this.sRoleId);
        }

        public void Clean()
        {
            List<string> keyList = new List<string>(this.Keys);
            for(int i = 0; i < keyList.Count; i++)
            {
                this[keyList[i]] = string.Empty;
            }
        }

        public void Assign(Dictionary<string, string> data)
        {
            var enumerator = data.GetEnumerator();
            while(enumerator.MoveNext())
            {
                this[enumerator.Current.Key] = enumerator.Current.Value;
            }
        }

        /// <summary>
        /// 计算签名的算法：
        /// val1 = md5(openid+area+partition+platid+roleid)
        /// val2 = md5(val1前8位+appid+"pandorasign")
        /// </summary>
        public void CalculateSign()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(this.sOpenId);
            sb.Append(this.sArea);
            sb.Append(this.sPartition);
            sb.Append(this.sPlatID);
            sb.Append(this.sRoleId);
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] mediumBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
            sb.Remove(0, sb.Length);
            for(int i = 0; i < mediumBytes.Length; i++)
            {
                sb.Append(mediumBytes[i].ToString("X2"));
            }
            string medium = sb.ToString();
            sb.Remove(0, sb.Length);
            sb.Append(medium.Substring(0, 8));
            sb.Append(this.sAppId);
            sb.Append("pandorasign");
            byte[] finalBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
            sb.Remove(0, sb.Length);
            for(int i = 0; i < finalBytes.Length; i++)
            {
                sb.Append(finalBytes[i].ToString("X2"));
            }
            this.sSign = sb.ToString();
        }

        /// <summary>
        /// 检查玩家登录态中必须字段是否为空
        /// </summary>
        /// <returns></returns>
        public bool IsValidate()
        {
            if (VerifyField("sAppId") == true
                && VerifyField("sOpenId") == true
                && VerifyField("sAcountType") == true
                && VerifyField("sArea") == true
                && VerifyField("sRoleId") == true
                && VerifyField("sAccessToken") == true
                && VerifyField("sPlatID") == true)
            {
                return true;
            }
            return false;
        }

        private bool VerifyField(string fieldName)
        {
            if (string.IsNullOrEmpty(this[fieldName]) == true)
            {
                Logger.LogError("UserData中 " + fieldName + " 字段内容为空，请检查游戏和Pandora的对接代码！", PandoraSettings.SDKTag);
                return false;
            }
            return true;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<color=#00ffff>UserData: ");
            var enumerator = this.GetEnumerator();
            while(enumerator.MoveNext())
            {
                sb.Append(enumerator.Current.Key);
                sb.Append("=");
                sb.Append(enumerator.Current.Value);
                sb.Append("&");
            }
            sb.Append("</color>");
            return sb.ToString();
        }
    }
}

