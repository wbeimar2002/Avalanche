using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using IsmLogCommon;
using IsmUtility;
using IsmStateServer;
using ISM.Middleware2Si;
using ISM.Library.Types;
using ISM.LibrarySi;
using IsmRec.Types;
using IsmRecSi;
using IsmStateServer.Types;
using PatInfoEngineSi;
using IIsmLivePreview;
using LssSi;

namespace MonsoonAPI {
    public interface IMonsoonResMgr
    {

        #region initialization
        void Init(IConfiguration cfg, INodeMgr nodeMgr);
        void RefreshStateCache();
        string GetLogVersion();
        #endregion

        #region logging
        void LogEvent(EventLogEntryType et, int nCode, string strMsg, int nLogVerbosityLevel);

        void LogEventWeb(EventLogEntryType et, int nCode, string strMsg, int nLogVerbosityLevel);

        void LogAccessEvent<TDetails>(TDetails details, AccessInfo accessInfo, string extraDetails) where TDetails : AccessDetailsBase;

        //TODO may not be needed
        void LogAccessEventWeb<TDetails>(TDetails details, AccessInfo accessInfo, string extraDetails) where TDetails : AccessDetailsBase;

        #endregion

            #region proxy retrieval
        WcfOneTimeUseClientWrapper<IMiddleware2Si> GetMwProxy();

        WcfOneTimeUseDuplexClientWrapper<ILibrary6Si, ResMgr> GetLibProxy(bool bCapture = false);

        WcfOneTimeUseClientWrapper<IFileRepositoryService> GetLibFileRepProxy(bool bCapture = false);

        WcfOneTimeUseClientWrapper<IIsmRecSi> GetIsmRecProxy();

        WcfOneTimeUseClientWrapper<ILivePreview> GetIsmLivePreviewProxy();

        WcfOneTimeUseDuplexClientWrapper<IIss, ResMgr> GetIssProxy();

        WcfOneTimeUseDuplexClientWrapper<IPatInfoEngineSi, ResMgr> GetPieProxy();

        WcfOneTimeUseDuplexClientWrapper<ILogService, ResMgr> GetLogProxy();

        WcfOneTimeUseClientWrapper<ILssMainSi> GetLssProxy();

        string LogVersion { get; }

        object GetIssData(IssDataCodes code, bool bLogging = true);
        ESuccess GetIssDataMap(IssDataCodes[] codesToGet, out Dictionary<IssDataCodes, object> dataValues);
        bool PublishPhysician(PersonNameData user);
        List<VideoData> GetVideoData();


            #endregion

        #region config retrieval
        EMonsoonConfig MonsoonCfg { get; }
        string MonsoonLibHost { get; } // host name of the Monsoon configured with symlinks to library results are coming from
        List<PrintLayoutInfo> PrintLayouts { get; }
        string RoomId { get; }
        RecorderSettings RecorderSettings { get; }
        int LogVerbosity { get; set; }
        IConfiguration Cfg { get; }
        string MyIp { get; }
        bool IsDev { get; }
        string LibHost { get; }
        string CultureName { get; }
        bool IsPieLocalEnabled { get; }
        bool IsPieConfigured { get; }

        bool IsLssEnabled { get; }

        #endregion

    }
}
