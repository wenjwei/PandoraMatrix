using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using com.tencent.pandora;
using UnityEngine.UI;
using System;
using System.Text.RegularExpressions;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Reflection;

namespace com.tencent.pandora
{
    public enum AccountType
    {
        QQ,
        WX,
        TTPP,
    }
    [System.Serializable]
    public class UserInfo
    {
        public string name = "";
        [Header("选择登录平台")]
        public PlatformType loginPlatform = PlatformType.OpenPlatformWithSocket;
        [Header("是否连接正式服环境")]
        public bool isProductEnvironment = false;
        public string openId = "";
        [Header("【roleid：必须跟其他人使用的不同】")]
        public string roleId = ""; // 以roleid作为账号切换的唯一判定
        public string partition = "";
        public AccountType accountType = AccountType.QQ;
        public string accessToken = "";
        public string payToken = "";
        public string gameName = "";
        [Header("【大区：只有大区不符合idip要求时才填写】")]
        public string area = "";
        [Header("【UserData扩展字段：只有需要往UserData中添加新的数据时才填写】")]
        public UserInfoExtend[] userInfoExtend;
    }

    [System.Serializable]
    public struct UserInfoExtend
    {
        public string key;
        public string value;
    }
    public class GameSimulator : MonoBehaviour
    {
        #region 配置UserData
        public UserInfo[] userInfo;


        public string QQAppid = string.Empty;
        public string WXAppid = string.Empty;
        public string TTPPAppid = string.Empty;
        public Vector2 referenceResolution = new Vector2(1136, 640);
        public Font font;

        //根据平台赋值
        private string _appId = "";
        private string _platID = "";
        private string _area = "";// wx 为1  手q为2
        private string _loginAccountType = "";
        #endregion

        private UserInfo _cachedUserInfo;

        public enum TriggerType
        {
            ReceivedPandoraPopMsg,
            ReceivedPandoraClosedMsg,
        }

        private Queue<string> _actionQueue = new Queue<string>();
        private string _currentOpen;

        private void Start()
        {
            UserInfo configedUserInfo = GetUserdata();
            if (configedUserInfo == null)
            {
                return;
            }
            if (IsUserdataInvalid(configedUserInfo) == false)
            {
                return;
            }
            PandoraSettings.LoginPlatform = configedUserInfo.loginPlatform;
            Init(configedUserInfo);
            SetGameDelegates();
            Login(configedUserInfo);
            _cachedUserInfo = configedUserInfo;
        }

        private UserInfo GetUserdata()
        {
            int selectedId = PlayerPrefs.GetInt("selectedId");
            if (selectedId + 1 > userInfo.Length)
            {
                Debug.LogError("您选择的用户信息没有配置，请先配置");
                return null;
            }

            UserInfo configedUserInfo = userInfo[selectedId];

#if UNITY_IOS
            _platID = "0";
#elif UNITY_ANDROID
            _platID = "1";
#elif UNITY_STANDALONE_WIN
            _platID = "2";
#endif
            switch (configedUserInfo.accountType)
            {
                case AccountType.QQ:
                    _appId = QQAppid;
                    _area = "2";// wx 为1  手q为2
                    _loginAccountType = "qq";
                    break;
                case AccountType.WX:
                    _appId = WXAppid;
                    _area = "1";// wx 为1  手q为2
                    _loginAccountType = "wx";
                    break;
                case AccountType.TTPP:
                    _appId = TTPPAppid;
                    _area = "3";// 游客为3
                    _loginAccountType = "ttpp";
                    break;
                default:
                    Debug.LogError("登录类型错误");
                    break;
            }
            return configedUserInfo;
        }

        private bool IsUserdataInvalid(UserInfo userInfo)
        {

            if (userInfo.openId == "" || userInfo.roleId == "" || userInfo.partition == "" || _appId == "" || userInfo.gameName == "")
            {
                string userDataToPrint = string.Format("openId={0}&roleId={1}&partitionId={2}&gameName={3}&appid={4}", userInfo.openId, userInfo.roleId, userInfo.partition, userInfo.gameName, _appId);

                Debug.LogError("userdata 非法，请检查自己的配置：" + userDataToPrint);
                return false;
            }
            return true;
        }

        //进入游戏时的操作
        private void Init(UserInfo userInfo)
        {
#if USING_NGUI
            Pandora.Instance.Init(referenceResolution, userInfo.isProductEnvironment, "UI Root");
#else
            Pandora.Instance.Init(referenceResolution, userInfo.isProductEnvironment, "Canvas");
#endif
            if (font != null)
            {
                Pandora.Instance.SetFont(font);
            }
        }

        private void SetGameDelegates()
        {
            Pandora.Instance.SetGameCallback(OnPandoraEvent);
            Pandora.Instance.SetGetCurrencyDelegate(GetCurrency);
            Pandora.Instance.SetJumpDelegate(Jump);

        }

        private void OnPandoraEvent(Dictionary<string, string> dict)
        {
            string jsonMessage = MiniJSON.Json.Serialize(dict);
            Logger.Log("OnPandoraEvent:" + jsonMessage);
            if (dict["type"] == "pandoraPop")
            {
                _actionQueue.Enqueue(dict["content"]);
                TriggerPopPandoraAction(TriggerType.ReceivedPandoraPopMsg, dict);
                return;
            }

            if (dict["type"] == "pandoraClosed")
            {
                TriggerPopPandoraAction(TriggerType.ReceivedPandoraClosedMsg, dict);
                return;
            }
        }

        private void Login(UserInfo userInfo)
        {
            //这里只是用了最基本的字段，用于匹配规则
            Dictionary<string, string> userDataDict = new Dictionary<string, string>();
            userDataDict["sOpenId"] = userInfo.openId;
            userDataDict["sAcountType"] = _loginAccountType;
            userDataDict["sArea"] = _area;
            if (!String.IsNullOrEmpty(userInfo.area))
            {
                userDataDict["sArea"] = userInfo.area;
            }
            userDataDict["sPartition"] = userInfo.partition;
            userDataDict["sAppId"] = _appId;
            userDataDict["sRoleId"] = userInfo.roleId;
            userDataDict["sAccessToken"] = userInfo.accessToken;
            userDataDict["sPayToken"] = userInfo.payToken;
            userDataDict["sPlatID"] = _platID;  // 0是IOS、1是安卓
            userDataDict["sGameName"] = userInfo.gameName.ToLower();
            userDataDict["sGameVer"] = "1.3.0.0";  //游戏版本号
            foreach (UserInfoExtend item in userInfo.userInfoExtend)
            {
                userDataDict.Add(item.key, item.value);
            }
            Pandora.Instance.SetUserData(userDataDict);
        }


        #region 自定义的函数都放这里

        private void NoticeReady()
        {
            Debug.Log("调用了函数NoticeReady");
            Dictionary<string, string> request = new Dictionary<string, string>() { { "type", "notice" }, { "content", "ready" } };
            string jsonRequest = MiniJSON.Json.Serialize(request);
            Pandora.Instance.CallGame(jsonRequest);
        }

        private void Relogin()
        {
            if (_cachedUserInfo != null)
            {
                Login(_cachedUserInfo);
            }
        }
        private void Logout()
        {
            Debug.Log("<color=#00ff00>" + "调用了Logout" + "</color>");
        }

        Dictionary<string, string> GetCurrency()
        {
            Debug.Log("GetCurrency 被调用");
            return new Dictionary<string, string>() { { "gold", "2000" }, { "diamond", "1000" }, };
        }
        private void Jump(string type, string content)
        {
            Debug.Log(string.Format("Jump 被调用，type：{0},content:{1}", type, content));
        }

        private void SendBrokerMsg()
        {
            Debug.LogError("use extFunc SendBrokerMsg");
            //Pandora.Instance.BrokerWrapper.Send(10000, "testBackup", 10001);
        }

        public void OpenPixTest1()
        {
            Dictionary<string, string> cmd = new Dictionary<string, string>()
            {
                {"type","open" },
                { "module","hall"},
                { "tab","pixTest_0"},
                { "parentPath","Canvas"},
            };

            Pandora.Instance.Do(cmd);
        }

        public void OpenPixTest2()
        {
            Dictionary<string, string> cmd = new Dictionary<string, string>()
            {
                {"type","open" },
                { "module","hall"},
                { "tab","pixTest_1"},
                { "parentPath","Canvas"},
            };

            Pandora.Instance.Do(cmd);


        }

        private void TriggerPopPandoraAction(TriggerType triggerType, Dictionary<string, string> dict)
        {
            if (triggerType == TriggerType.ReceivedPandoraPopMsg)
            {
                if (string.IsNullOrEmpty(_currentOpen))
                {
                    PopPandoraAction();
                }

                return;
            }

            if (triggerType == TriggerType.ReceivedPandoraClosedMsg && dict["content"] == _currentOpen)
            {
                _currentOpen = "";
                if (_actionQueue.Count > 0)
                {
                    PopPandoraAction();
                }
            }
        }

        private void PopPandoraAction()
        {
            _currentOpen = _actionQueue.Dequeue();
            Dictionary<string, string> cmd = new Dictionary<string, string>() {
                        { "type","open"},
                        {"content",_currentOpen },
                        {"parentPath","Canvas" },
                    };
            Pandora.Instance.Do(cmd);
        }

        #endregion
    }
}



