using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using IsmUtility;
using Microsoft.AspNetCore.Http;

namespace MonsoonAPI.Controllers.Compatibility
{
    public class UploadUtil
    {
        const int kSubChunk = 16384;
        static int m_nX = 0;
        private IMonsoonResMgr m_Rm;
        private HttpContext m_Context;
        private CompatibilityGlobal m_Global;

        public UploadUtil(IMonsoonResMgr rm, CompatibilityGlobal global)
        {
            m_Rm = rm;
            m_Global = global;
        }

        public void SetContext(HttpContext context)
        {
            m_Context = context;
        }

        public HttpStatusCode Upload(HttpRequest request, out string strResponse)
        {
            m_Rm.LogEvent(EventLogEntryType.Information, 0, "Compatibility Upload Entered", 3);
            strResponse = string.Empty;
            try
            {
                string strCredentials;
                if (CheckSystemReadiness(false, false, out strCredentials) != HttpStatusCode.OK)
                {
                    m_Rm.LogEvent(EventLogEntryType.Error, 0, "Compatibility Upload Error: Unauthorized", 1);
                    return HttpStatusCode.Unauthorized;
                }
                m_Rm.LogEvent(EventLogEntryType.Information, 0, "Compatibility Upload: Credentials checked", 4);

                string strUploadFile = request.Headers["file_name"];
                if (string.IsNullOrEmpty(strUploadFile))
                {
                    m_Rm.LogEvent(EventLogEntryType.Error, 0, "Compatibility Upload Error: Missing Header file_name.", 1);
                    return HttpStatusCode.BadRequest;
                }
                m_Rm.LogEvent(EventLogEntryType.Information, 0, "Compatibility Upload: File present " + strUploadFile, 4);

                Int64 nOffset;
                if (!Int64.TryParse(request.Headers["offset"], out nOffset))
                    nOffset = 0;

                m_Rm.LogEvent(EventLogEntryType.Information, 0, string.Format("Upload {0} at {1}", strUploadFile, nOffset), 3);

                BinaryWriter writer = null;
                lock (m_Global.LockObj)
                {
                    try
                    {
                        var uploadingFiles = m_Global.UploadingFiles;
                        var fileTimes = m_Global.UploadingFileTimes;

                        if (uploadingFiles.ContainsKey(strUploadFile))
                            writer = uploadingFiles[strUploadFile];
                        else
                        {
                            string strDir = Path.GetDirectoryName(strUploadFile);
                            if (!Directory.Exists(strDir))
                            {
                                m_Rm.LogEvent(EventLogEntryType.Information, 0, "WebUtils.Upload created directory: " + strDir, 3);
                                Directory.CreateDirectory(strDir);
                            }

                            m_Rm.LogEvent(EventLogEntryType.Information, 0, "Upload is about to open file " + strUploadFile, 3);
                            FileMode mode = nOffset == 0 ? FileMode.Create : FileMode.OpenOrCreate;
                            int nVerbosity = (mode == FileMode.Create) ? 3 : 4;
                            uploadingFiles[strUploadFile] = writer = new BinaryWriter(File.Open(strUploadFile, mode, FileAccess.ReadWrite, FileShare.Read));
                            m_Rm.LogEvent(EventLogEntryType.Information, 0,
                                $"WebUtils.Upload opened file {strUploadFile} in mode {mode}", 3);
                        }
                        fileTimes[strUploadFile] = DateTime.UtcNow + new TimeSpan(0, 5, 0);
                    }
                    catch (Exception ex)
                    {
                        m_Rm.LogEvent(EventLogEntryType.Error, 0, "Upload failed to get file " + strUploadFile + " err - " + ex.Message, 1);
                        return HttpStatusCode.InternalServerError;
                    }
                }

                try
                {
                    if (writer.BaseStream.Position != nOffset)
                    {
                        if (writer.BaseStream.Length < nOffset)
                        {
                            m_Rm.LogEvent(EventLogEntryType.Warning, 0,
                                $"Failed to upload to {strUploadFile} because specified offset of {nOffset} exceeds file length of {writer.BaseStream.Length}", 1);
                            return HttpStatusCode.Conflict;
                        }
                        writer.BaseStream.Position = nOffset;
                        m_Rm.LogEvent(EventLogEntryType.Information, 0, $"Offset set to {nOffset} for file {strUploadFile}", 3);
                    }

                    // read stream & write
                    //using (var reader = new BinaryReader(m_Request.Body))
                    //{
                    request.Body.CopyTo(writer.BaseStream);
                    // }
                }
                catch (System.ObjectDisposedException)
                {
                    m_Rm.LogEvent(EventLogEntryType.Warning, 0, $"File {strUploadFile} has been closed. Will try again", 3);
                    lock (m_Global.LockObj)
                    {
                        Dictionary<string, BinaryWriter> uploadingFiles = m_Global.UploadingFiles;
                        Dictionary<string, DateTime> fileTimes = m_Global.UploadingFileTimes;
                        uploadingFiles[strUploadFile] = writer = new BinaryWriter(File.Open(strUploadFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read));
                        m_Rm.LogEvent(EventLogEntryType.Information, 0, string.Format("WebUtils.Upload opened file {0} in mode openOrCreate", strUploadFile), 3);
                        fileTimes[strUploadFile] = DateTime.UtcNow;
                    }

                    if (writer.BaseStream.Position != nOffset)
                    {
                        writer.BaseStream.Position = nOffset;
                        m_Rm.LogEvent(EventLogEntryType.Information, 0, $"Offset set to {nOffset} for file {strUploadFile}", 3);
                    }

                    // read stream & write
                    //using (BinaryReader reader = new BinaryReader(m_Request.InputStream))
                    //{
                    request.Body.CopyTo(writer.BaseStream);
                    // }
                }
                catch (Exception ex)
                {
                    m_Rm.LogEvent(EventLogEntryType.Error, 0, "Upload failed to write in " + strUploadFile + " with err " + ex.Message, 1);
                    lock (m_Global.LockObj)
                    {
                        try
                        {
                            writer.BaseStream.Close();
                        }
                        catch (Exception ex2)
                        {
                            m_Rm.LogEvent(EventLogEntryType.Warning, 0, "Upload failed to close file. Err - " + ex2.Message, 1);
                        }

                        if (m_Global.UploadingFiles.ContainsKey(strUploadFile))
                            m_Global.UploadingFiles.Remove(strUploadFile);
                    }
                }

                m_Global.UploadingFileTimes[strUploadFile] = DateTime.UtcNow;
                strResponse = writer.BaseStream.Position.ToString();
                m_Rm.LogEvent(EventLogEntryType.Information, 0, "Returning position " + strResponse, 3);

                return HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                m_Rm.LogEvent(EventLogEntryType.Error, 0, "Upload failed with err " + ex.Message, 1);
                return HttpStatusCode.InternalServerError;
            }
        }

        public HttpStatusCode FinishWrite(HttpRequest request, out string strChecksum)
        {
            strChecksum = string.Empty;
            BinaryWriter writer = null;
            string strUploadFile = "''";
            try
            {
                m_Rm.LogEvent(EventLogEntryType.Information, 0, "FinishWrite entered", 4);
                string strCredentials;
                if (CheckSystemReadiness(false, false, out strCredentials) != HttpStatusCode.OK)
                    return HttpStatusCode.Unauthorized;

                strUploadFile = request.Headers["file_name"];
                if (string.IsNullOrEmpty(strUploadFile))
                    return HttpStatusCode.BadRequest;
                if (!File.Exists(strUploadFile))
                    return HttpStatusCode.NotFound;

                m_Rm.LogEvent(EventLogEntryType.Information, 0, $"FinishWrite entered for {strUploadFile}", 3);

                lock (m_Global.LockObj)
                {
                    Dictionary<string, BinaryWriter> uploadingFiles = m_Global.UploadingFiles;
                    Dictionary<string, DateTime> file2DT = m_Global.UploadingFileTimes;
                    if (uploadingFiles.ContainsKey(strUploadFile))
                    {
                        writer = uploadingFiles[strUploadFile];
                        uploadingFiles.Remove(strUploadFile);
                        m_Rm.LogEvent(EventLogEntryType.Information, 0, "File removed from writer map " + strUploadFile, 3);
                    }
                    else
                    {
                        writer = new BinaryWriter(File.Open(strUploadFile, FileMode.Open));
                        m_Rm.LogEvent(EventLogEntryType.Information, 0, "File opened: " + strUploadFile, 3);
                    }

                    if (file2DT.ContainsKey(strUploadFile))
                    {
                        file2DT.Remove(strUploadFile);
                        m_Rm.LogEvent(EventLogEntryType.Information, 0, "File removed from date map " + strUploadFile, 3);
                    }
                }

                // checksum
                string strErr;
                strChecksum = DirManager.GetChecksum(writer.BaseStream, out strErr);
                m_Rm.LogEvent(EventLogEntryType.Information, 0, String.Format("Checksum for file {0} is {1} ", strUploadFile, strErr), 3);
                if (!string.IsNullOrEmpty(strErr))
                {
                    m_Rm.LogEvent(EventLogEntryType.Error, 0, "Checksum failed with err " + strErr, 1);
                    return HttpStatusCode.InternalServerError;
                }

                m_Rm.LogEvent(EventLogEntryType.Information, 0, "Finish write done with " + strUploadFile, 3);
                return HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                m_Rm.LogEvent(EventLogEntryType.Error, 0, "GetChecksum failed with err " + ex.Message, 1);
                return HttpStatusCode.InternalServerError;
            }
            finally
            {
                try
                {
                    if (writer != null)
                    {
                        writer.Close();
                        m_Rm.LogEvent(EventLogEntryType.Information, 0, "Writer closed: " + strUploadFile, 3);
                    }
                    else
                        m_Rm.LogEvent(EventLogEntryType.Information, 0, "Writer note closed because it is already null: " + strUploadFile, 3);
                }
                catch { }
            }
        }


        internal HttpStatusCode CheckSystemReadiness(bool bCheckLib, bool bCheckMw, out string strCredentials)
        {
            strCredentials = string.Empty;
            strCredentials = m_Context.Session.GetString(m_Global.SESSION_CREDENTIALS);
            if (string.IsNullOrEmpty(strCredentials))
            {
                m_Rm.LogEvent(EventLogEntryType.Warning, 0, "Requeast is not authenticated", 1);
                return HttpStatusCode.Unauthorized;
            }

            try
            {
                if (bCheckLib)
                {
                    using (var lib = m_Rm.GetLibProxy())
                    {
                    }
                }
                if (bCheckMw)
                {
                    using (var mw = m_Rm.GetMwProxy())
                    { }
                }

            }
            catch
            {
                m_Rm.LogEvent(EventLogEntryType.Warning, 0, "Services is not online", 1);
                return HttpStatusCode.ServiceUnavailable;
            }

            return HttpStatusCode.OK;
        }
        public HttpStatusCode CheckSpace(HttpRequest request, out string strResponse)
        {
            strResponse = string.Empty;
            try
            {
                if (CheckSystemReadiness(false, false, out string strCredentials) != HttpStatusCode.OK)
                    return HttpStatusCode.Unauthorized;

                var strBytes = request.Headers["space_required"];
                var nBytesRequired = long.Parse(strBytes);
                var strDestination = request.Headers["destination"];

                // destination may refer to a folder not yet created if so, go back a layer (or two, or three...)
                m_Rm.LogEvent(EventLogEntryType.Information, 0,
                    $"CheckSpace entered for {strDestination} for {nBytesRequired} bytes", 3);
                if (!Directory.Exists(strDestination))
                    strDestination = Path.GetPathRoot(strDestination);
                if (!Directory.Exists(strDestination))
                    throw new Exception("Destination does not exist: " + strDestination);

                var freeBytesAvailable = DirManager.GetDiskAvailableBytes(strDestination);
                m_Rm.LogEvent(EventLogEntryType.Information, 0,
                    $"CheckSpace finds, requiring {DirManager.BytesToMB(nBytesRequired):0.00} MB, finds {DirManager.BytesToMB(freeBytesAvailable):0.00} MB to be free on {strDestination}", 3);
                bool bSpaceAvailable = freeBytesAvailable - (ulong)nBytesRequired > DirManager.MBToBytes(1); // always leave 1 MB buffer
                strResponse = bSpaceAvailable.ToString();
                return HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                m_Rm.LogEvent(EventLogEntryType.Error, 0, "CheckSpace failed with err " + ex.Message, 1);
                return HttpStatusCode.InternalServerError;
            }
        }

        public HttpStatusCode GetChecksum(HttpRequest request, out string strChecksum)
        {
            strChecksum = string.Empty;
            BinaryWriter writer = null;
            bool bCloseWriter = false;
            string strUploadFile = string.Empty;
            try
            {
                string strCredentials;
                if (CheckSystemReadiness(false, false, out strCredentials) != HttpStatusCode.OK)
                    return HttpStatusCode.Unauthorized;

                strUploadFile = request.Headers["file_name"];
                if (string.IsNullOrEmpty(strUploadFile))
                    return HttpStatusCode.BadRequest;
                if (!File.Exists(strUploadFile))
                    return HttpStatusCode.NotFound;

                lock (m_Global.LockObj)
                {
                    Dictionary<string, BinaryWriter> uploadingFiles = m_Global.UploadingFiles;
                    if (uploadingFiles.ContainsKey(strUploadFile))
                        writer = uploadingFiles[strUploadFile];
                    else
                    {
                        writer = new BinaryWriter(File.Open(strUploadFile, FileMode.Open));
                        bCloseWriter = true;
                    }
                }

                // checksum
                string strOffset = request.Headers["offset"];
                if (string.IsNullOrEmpty(strOffset))
                    return HttpStatusCode.BadRequest;
                Int64 nOffset = Int64.Parse(strOffset);
                string strErr;
                strChecksum = DirManager.GetChecksum(writer.BaseStream, nOffset, out strErr);
                if (bCloseWriter)
                    writer.Close();
                if (!string.IsNullOrEmpty(strErr))
                {
                    m_Rm.LogEvent(EventLogEntryType.Error, 0, $"Checksum failed with err {strErr} for file {strUploadFile}", 1);
                    return HttpStatusCode.InternalServerError;
                }

                m_Rm.LogEvent(EventLogEntryType.Information, 0, $"Checksum computed to {strChecksum} for file {strUploadFile}", 3);
                return HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                m_Rm.LogEvent(EventLogEntryType.Error, 0, $"GetChecksum failed with err {ex.Message} for file {strUploadFile}", 1);
                return HttpStatusCode.InternalServerError;
            }
            finally
            {
                if (writer != null && bCloseWriter)
                {
                    try { writer.Close(); }
                    catch { }
                }
            }
        }
    }
}
