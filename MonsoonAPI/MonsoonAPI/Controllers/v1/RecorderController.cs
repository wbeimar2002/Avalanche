using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MsntSi.Types;
using IsmStateServer;
using IsmRec.Types;

namespace MonsoonAPI.Controllers.v1
{
    public class RecorderController : ControllerBase
    {
        public RecorderController(IMonsoonResMgr resMgr, INodeMgr nodeMgr, IConfiguration cfg) 
            : base("recorder", resMgr,  nodeMgr, cfg)
        {
           
        }

        [HttpGet("{infoType}")]
        public async Task<IActionResult> GetRecSettings(string infoType)
        {
            const string method = "GetRecSettings";
            try
            {
                LogEnter(method);
                if (Rm.MonsoonCfg == EMonsoonConfig.EasyView)
                {
                    return await ReturnBadRequest(method, "Recorder not supported on EasyView");
                }

                switch (infoType)
                {
                    case "room_info":
                        return await GetRoomInfo(method);
                    case "input_lables":
                        return await GetInputLabels(method);
                    default:
                        return await ReturnError(method, $"Unrecognized command {infoType}");
                }

            }
            catch (Exception ex)
            {
                return await ReturnError(method, ex.Message);
            }
        }

        private async Task<IActionResult> GetInputLabels(string method)
        {
            try
            {
                using (var ismRecProxy = Rm.GetIsmRecProxy())
                {
                    if (ismRecProxy == null)
                        return await  ReturnOffline(eSystemType.recorder, method);

                    if (ismRecProxy.Proxy.Capture_GetSwitchLabels(null, out var bEnabled, out var labels, out var nChannelSelected, out var strError) != 0)
                        return await  ReturnError(method,$"Capture_GetSwitchLabels failed with err {strError}");

                    var output = new
                    {
                        m_bEnabled = bEnabled,
                        m_Labels = labels,
                        m_nChannelSelected = nChannelSelected
                    };
                    return await ReturnOk(method, output);
                }
            }
            catch (Exception ex)
            {
                return await ReturnError(method, ex.Message);
            }
        }

        private async Task<IActionResult> GetRoomInfo(string method)
        {
            try
            {
                IssDataCodes[] codesToGet =
                    {
                        IssDataCodes.recorder_session_state,
                        IssDataCodes.lib_safe_zone,
                        IssDataCodes.recorder_proc_info
                     };
                if (Rm.GetIssDataMap(codesToGet, out var dataValues) != ESuccess.Ok)
                    return await ReturnError(method, "GetIssDataMap failed.");

                // recorder state
                eRecordState recState = eRecordState.unknown;
                if (dataValues.TryGetValue(IssDataCodes.recorder_session_state, out var obj) && obj != null)
                    recState = (eRecordState)obj;
                // horrible logic to facilitate merging. Horrible!
                var recordState = models.DmagM.TranslateRecState(recState, out bool background_recording);

                // safe_zone
                bool safeZone = false;
                if (dataValues.TryGetValue(IssDataCodes.lib_safe_zone, out obj) && obj != null)
                    safeZone = (bool)obj;

                // procedure id
                ProcedureData procData = null;
                if (dataValues.TryGetValue(IssDataCodes.recorder_proc_info, out obj) && obj != null)
                    procData = (ProcedureData)obj;

                var output = new
                {
                    record_state = recordState,
                    background_recording,
                    safe_zone = safeZone,
                    proc_id = procData?.m_Id
                };
                return await ReturnOk(method, output);
            }
            catch (Exception ex)
            {
                return await ReturnError(method, ex.Message);
            }
        }



        [HttpPost]
        public async Task<IActionResult> EnableChannels([FromBody]Dictionary<string, bool> channelEnable)
        {
            const string method = "EnableChannels";
            try
            {
                LogEnter(method);
                if (Rm.MonsoonCfg == EMonsoonConfig.EasyView)
                {
                    return await ReturnBadRequest(method, "Recorder not supported on EasyView");
                }

                using (var ismRecProxy = Rm.GetIsmRecProxy())
                {
                    if (ismRecProxy == null)
                        return await ReturnOffline(eSystemType.recorder, method);

                    if (ismRecProxy.Proxy.Record_SetRecordChannelsEnabled(channelEnable, out var strErr) != 0)
                        return await ReturnError(method,
                            $"Record_SetRecordChannelEnabled failed with err {strErr}");
                }
                return await ReturnOk(method);
            }
            catch (Exception ex)
            {
                return await ReturnError(method, ex.Message);
            }
        }

        [HttpPut]
        public async Task<IActionResult> SetInputSettings([FromBody]int input)
        {
            var method = $"SetInputSettings - {input}";
            try
            {
                LogEnter(method);
                if (Rm.MonsoonCfg == EMonsoonConfig.EasyView)
                {
                    return await ReturnBadRequest(method, "Recorder not supported on EasyView");
                }

                using (var ismRecProxy = Rm.GetIsmRecProxy())
                {
                    if (ismRecProxy == null)
                        return await  ReturnOffline(eSystemType.recorder,method);

                    if (ismRecProxy.Proxy.Capture_SelectChannel(null, input, out var strErr) != 0)
                        return await ReturnError(method, $"Capture_SelectChannel faield for {input} with err {strErr}");
                }


                return await ReturnOk( method);
            }
            catch (Exception ex)
            {
                return await ReturnError(method, ex.Message);
            }
        }


    }
}