using IsmDmag;
using IsmRec.Types;
using IsmStateServer;
using IsmUtility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MonsoonAPI.models;
using MsntSi.Types;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonsoonAPI.Controllers.v1
{
    public class ProcActiveVidController : ControllerBase
    {
        public ProcActiveVidController(IMonsoonResMgr resMgr, INodeMgr nodeMgr, IConfiguration cfg) :
            base("ProcActiveVid", resMgr, nodeMgr, cfg)
        {
        }

        [HttpGet("{channel}")]
        public async Task<IActionResult> GetVideoCount(string channel)
        {
            var method = $"GetVideoCount/{channel}";
            try
            {
                LogEnter(method);
                var channelDurations = ((Dictionary<string, double>)Rm.GetIssData(IssDataCodes.recorder_active_video_duration)).ToOrdinalIgnoreCaseDictionary();
                if (channelDurations == null)
                    return await ReturnError(method, "Failed to retrieve duration from state server");

                return await ReturnOk(method, channelDurations);
            }
            catch (Exception ex)
            {
                return await ReturnError(method, ex.Message);
            }

        }

        [HttpGet]
        public async Task<IActionResult> GetVideos()
        {
            const string method = "GetVideos";
            try
            {
                LogEnter(method);
                if (Rm.MonsoonCfg == EMonsoonConfig.EasyView)
                {
                    return await ReturnBadRequest(method, "ProcActiveVid not supported on EasyView");
                }

                List<VideoData> videos = Rm.GetVideoData();
                if (videos == null)
                    return await ReturnError(method, "Failed to retrieve videos from state server");

                return await ReturnOk(method, videos);
            }
            catch (Exception ex)
            {
                return await ReturnError(method, ex.Message);
            }

        }



        [HttpDelete]
        public async Task<IActionResult> DeleteVideo([FromBody] string video)
        {
            const string method = "DeleteVideo - {video}";
            try
            {
                var accessInfo = HttpContextUtilities.GetAccessInfoFromHttpContext(HttpContext);
                LogEnter(method);

                if (Rm.MonsoonCfg == EMonsoonConfig.EasyView)
                {
                    return await ReturnBadRequest(method, "ProcActiveVid not supported on EasyView");
                }

                using (var recProxy = Rm.GetIsmRecProxy())
                {
                    if (recProxy == null)
                        return await ReturnOffline(eSystemType.recorder, method);

                    if (recProxy.Proxy.Record_DeleteVideo(video, accessInfo, out var strErr) != 0)
                        return await ReturnError(method, "Record_DeleteVideo err: " + strErr);

                    return await ReturnOk(method);
                }

            }
            catch (Exception ex)
            {
                return await ReturnError(method, ex.Message);
            }
        }

        [HttpPut]
        public async Task<IActionResult> AddVideo([FromBody] MovieMonsoon movieInfo)
        {
            var method = $"AddVideo:'{movieInfo?.RelativePath}'";
            try
            {
                LogEnter(method);

                if (Rm.MonsoonCfg == EMonsoonConfig.EasyView)
                {
                    return await ReturnBadRequest(method, "ProcActiveVid not supported on EasyView");
                }

                if (string.IsNullOrEmpty(movieInfo?.RelativePath))
                {
                    return await ReturnBadRequest(method, "AddVideo must have video path specified");
                }

                using (var recProxy = Rm.GetIsmRecProxy())
                {
                    if (recProxy == null)
                        return await ReturnOffline(eSystemType.recorder, method);

                    clsMovie movie = new clsMovie(movieInfo.RelativePath)
                    {
                        Created = DateTime.UtcNow,
                        Stream = "M",
                        Length = movieInfo.Length,
                        ThumbName = clsPicture.GetThumbnailFileName(movieInfo.RelativePath)
                    };

                    if (recProxy.Proxy.Record_AddMobileCaptureVideo(movie, out var strErr) != 0)
                        return await ReturnError(method, "AddMobileCaptureVideo err: " + strErr);

                }
                return await ReturnOk(method);
            }
            catch (Exception ex)
            {
                return await ReturnError(method, ex.Message);
            }

        }

        [HttpPost]
        public async Task<IActionResult> ToggleRecording([FromBody]bool startRecording)
        {
            var method = $"ToggleRecording - {startRecording}";
            try
            {
                LogEnter(method);

                if (Rm.MonsoonCfg == EMonsoonConfig.EasyView)
                {
                    return await ReturnBadRequest(method, "ProcActiveVid not supported on EasyView");
                }

                using (var recProxy = Rm.GetIsmRecProxy())
                {
                    if (recProxy == null)
                        return await ReturnOffline(eSystemType.recorder, method);

                    string strErr;
                    var nRes = startRecording
                        ? recProxy.Proxy.Record_StartRecord(out strErr)
                        : recProxy.Proxy.Record_StopRecord(out strErr);

                    return await ReturnOk($"{method} - {nRes}", nRes);
                }
            }
            catch (Exception ex)
            {
                return await ReturnError(method, ex.Message);
            }

        }
    }
}
