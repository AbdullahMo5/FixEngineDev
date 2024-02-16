using QuickFix;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public static class SessionSettingsFactory
    {
        public static SessionSettings GetSessionSettings(string lp, string host, int port, string senderCompId, string? senderSubId, string? targetSubId, string targetCompId, string resetOnLogin, string ssl)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("[DEFAULT]");
            stringBuilder.AppendLine("ConnectionType=initiator");
            stringBuilder.AppendLine("ReconnectInterval=10");
            stringBuilder.AppendLine("FileStorePath=store");
            stringBuilder.AppendLine("FileLogPath=log");
            stringBuilder.AppendLine("HeartBtInt=30");
            stringBuilder.AppendLine("StartTime=00:00:00");
            stringBuilder.AppendLine("EndTime=00:00:00");
            stringBuilder.AppendLine("UseDataDictionary=Y");
            if(lp=="CTRADER")stringBuilder.AppendLine("DataDictionary=./DataDictionary/FIX44-CSERVER.xml");
            if(lp=="CENTROID")stringBuilder.AppendLine("DataDictionary=./DataDictionary/FIX44-CENTROID.xml");
            if(ssl == "Y")
            {
                stringBuilder.AppendLine($"SSLEnable={ssl}");
                stringBuilder.AppendLine($"NeedClientAuth=N");
                stringBuilder.AppendLine($"SSLValidateCertificates=N");
            }
            //stringBuilder.AppendLine($"SocketUseSSL={ssl}");
            stringBuilder.AppendLine($"SocketConnectHost={host}");
            stringBuilder.AppendLine($"SocketConnectPort={port}");
            stringBuilder.AppendLine("LogoutTimeout=100");
            stringBuilder.AppendLine($"ResetOnLogon={resetOnLogin}");
            stringBuilder.AppendLine("ResetOnDisconnect=Y");
            stringBuilder.AppendLine("[SESSION]");
            stringBuilder.AppendLine("BeginString=FIX.4.4");
            stringBuilder.AppendLine($"SenderCompID={senderCompId}");
            if(senderSubId != null)
                stringBuilder.AppendLine($"SenderSubID={senderSubId}");
            if(targetSubId != null)
                stringBuilder.AppendLine($"TargetSubID={targetSubId}");
            stringBuilder.AppendLine($"TargetCompID={targetCompId}");

            var stringReader = new StringReader(stringBuilder.ToString());

            return new SessionSettings(stringReader);
        } 
    }
}
