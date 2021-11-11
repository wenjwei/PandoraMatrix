using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace com.tencent.pandora
{
    public partial class Pandora
    {
        //private string _gameCode = "PANDORA";
        private string _gameCode = "XZJ";
        private bool _forceGameCodeToUpper = true;
        //SDK 构建版本号，每次给游戏合入时+1
        //public int SdkVersion = 1;
        public int SdkBuildVersion = 0;
        private const string FontLoadDir = "Font/";

        #region Broker Setting

        /* private string  _brokerHost = "speedm.broker.tplay.qq.com";
        private int     _brokerPort = 5692;
        private string  _alternateIp1 = "182.254.42.60";
        private string  _alternateIp2 = "182.254.74.52"; */
        private string  _brokerHost = "xzj.broker.tplay.qq.com";
        private int     _brokerPort = 20297;
        private string  _alternateIp1 = "175.27.0.4";
        private string  _alternateIp2 = "175.27.0.108";

        /* private string  _brokerTestHost = "test4.broker.tplay.qq.com";
        private int     _brokerTestPort = 10023;
        private string  _alternateTestIp1 = "";
        private string  _alternateTestIp2 = ""; */
        private string  _brokerTestHost = "pdaty.broker.tplay.qq.com";
        private int     _brokerTestPort = 5041;
        private string  _alternateTestIp1 = "";
        private string  _alternateTestIp2 = "";

        #endregion

        #region Atm Setting

        private string  _atmHost = "jsonatm.broker.tplay.qq.com";
        private int     _atmPort = 5692;

        private string  _atmTestHost = "test.broker.tplay.qq.com";
        private int     _atmTestPort = 4567;

        #endregion

        #region LogUpload Setting

        private string _logUploadUrl = "https://pdrlog.game.qq.com/?c=PandoraSDKLogUpload&a=batch";
        private string _logUploadTestUrl = "https://pdrlog.game.qq.com/?c=PandoraSDKLogUpload&a=batch";

        #endregion
    }
}
