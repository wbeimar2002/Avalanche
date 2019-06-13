using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ISM.LibrarySi;
using IsmStateServer.Types;
using IsmUtility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MsntSi.Types;

namespace MonsoonAPI.Controllers.v1
{
    public class VideoMapController : ControllerBase
    {
        public VideoMapController(IMonsoonResMgr resMgr, INodeMgr nodeMgr, IConfiguration cfg) :
            base("VideoMap", resMgr, nodeMgr, cfg)
        { }

        [HttpGet("{libname}/{libid}/{filename}")]
        public async Task<IActionResult> GetLibVideoMap(string libname, string libid, string filename)
        {
            var method = $"GetLibVideoMap/{libname}/{libid}/{filename}";
            try
            {
                LogEnter(method);
                // get lib-view library
                using (var libProxy = Rm.GetLibProxy())
                {
                    if (libProxy == null)
                        return await ReturnOffline(eSystemType.library, method);
                    
                    // get the data
                    ProcedureId procId = new ProcedureId { m_strLibId = libid, m_strLibName = libname };
                    bool isLast = false;
                    uint startOffset = 0;
                    int startTimeStampMs = 0;
                    List<int> frameMap = new List<int>();
                    double averageRate = 0;

                    uint chunkSize = GetDesiredChunkSize(libProxy);

                    while (false == isLast)
                    {
                        if (libProxy.Proxy.GetSeekTableChunk(procId, filename, startOffset, startTimeStampMs, chunkSize, out SeekTableChunk curChunk, out string strErr) != 0)
                        {
                            return await ReturnError(method, "GetSeekTable err: " + strErr);
                        }
                        
                        ProcessChunk(curChunk, frameMap, ref averageRate, ref startOffset, ref startTimeStampMs, ref isLast);
                    }

                    // return
                    object retObj = new
                    {
                        frame_map = frameMap,
                        avg_rate = averageRate
                    };
                    return await ReturnOk(method, retObj);
                }
            }
            catch (Exception ex)
            {
                return await ReturnError(method, ex.Message);
            }
        }

        [HttpGet("{filename}")]
        public async Task<IActionResult> GetActiveVideoMap(string filename)
        {
            var method = $"GetActiveVideoMap/{filename}";
            try
            {
                LogEnter(method);
                // get LOCAL library
                using (var libProxy = Rm.GetLibProxy(true))
                {
                    if (libProxy == null)
                        return await ReturnOffline(eSystemType.library, method);
                    
                    bool isLast = false;
                    uint startOffset = 0;
                    int startTimeStampMs = 0;
                    List<int> frameMap = new List<int>();
                    double averageRate = 0;

                    uint chunkSize = GetDesiredChunkSize(libProxy);

                    while (false == isLast)
                    {
                        if (libProxy.Proxy.GetActiveSeekTableChunk(filename, startOffset, startTimeStampMs, chunkSize, out SeekTableChunk curChunk, out string strErr) != 0)
                        {
                            return await ReturnError(method, "GetSeekTable err: " + strErr);
                        }

                        ProcessChunk(curChunk, frameMap, ref averageRate, ref startOffset, ref startTimeStampMs, ref isLast);
                    }

                    // return
                    object retObj = new
                    {
                        frame_map = frameMap,
                        avg_rate = averageRate
                    };
                    return await ReturnOk(method, retObj);
                }
            }
            catch (Exception ex)
            {
                return await ReturnError(method, ex.Message);
            }
   
        }

        [HttpPut]
        public async Task<IActionResult> FocusLivePreview()
        {
            const string method = "FocusLivePreview";
            try
            {
                LogEnter(method);
                using (var previewProxy = Rm.GetIsmLivePreviewProxy())
                {
                    if (previewProxy == null)
                        return await ReturnOffline(eSystemType.room_light, method);

                    if (previewProxy.Proxy.Focus(out var strErr) != 0)
                        return await ReturnError(method, "Focus err: " + strErr);

                    return await ReturnOk(method);
                }
            }
            catch (Exception ex)
            {
                return await ReturnError(method, ex.Message);
            }
        }

        // gets number of frame map entries to request at a time
        private uint GetDesiredChunkSize(WcfOneTimeUseDuplexClientWrapper<ILibrary6Si, ResMgr> proxy)
        {
            uint chunkSize = 131072; // default roughly based on expected size of message and current default channel binding plus room for error...
            if (proxy.MaxReceivedMessageSize.HasValue)
            {
                long frameMapChunkSize = proxy.MaxReceivedMessageSize.Value / 8; // 4 bytes per frame_map entry and lets just divide by 2 again to be very safe.
                if (frameMapChunkSize > (long)uint.MaxValue)
                {
                    frameMapChunkSize = uint.MaxValue;
                }

                chunkSize = (uint)frameMapChunkSize;
            }
            return chunkSize;
        }

        private void ProcessChunk(SeekTableChunk curChunk, List<int> frameMap, ref double averageRate, ref uint startOffset, ref int startTimeStampMs, ref bool isLast)
        {
            isLast = curChunk.SeekTableComplete;
            if (0 == frameMap.Count)
            {
                averageRate = curChunk.AverageFrameDurationSeconds;
            }
            else
            {
                int totalCount = frameMap.Count + curChunk.FrameTimeStampsMs.Count;
                averageRate = (averageRate * (frameMap.Count / totalCount)) + (curChunk.AverageFrameDurationSeconds * (curChunk.FrameTimeStampsMs.Count / totalCount));
            }

            frameMap.AddRange(curChunk.FrameTimeStampsMs);
            startOffset += (uint)curChunk.FrameTimeStampsMs.Count;
            if (curChunk.NextStartTimeStampMs.HasValue)
            {
                startTimeStampMs = curChunk.NextStartTimeStampMs.Value;
            }
        }
    }
}