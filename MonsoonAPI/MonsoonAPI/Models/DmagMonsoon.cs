using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using IsmUtility;
using ISM.LibrarySi;
using IsmDmag;
using IsmStateServer.Types;
using IsmRec.Types;
using ISM.Library.Types;
using IsmRecSi;
using PatInfoEngineSi;

namespace MonsoonAPI.models
{
  
    public class DmagM
    {
        #region attributes
        public string HostName;
        public int proc_modifiable;
        public UlacInfo sharing;
        public Dictionary<ePayloadType, TaskInfo> tasks;
        public List<Dictionary<ESearchFields, string>> compare_list;
        public string RootPath;
        public ClinInfoDataExM.ClinInfoDataM Patient;
        public DmagContentM Content;
        public VideoEditSettingsM video_edit;
        public IEnumerable<ChannelInfo> channels;
        public bool IsClinical;
        public DateTimeOffset Created;
        public PersonNameDataM physician;
        public ProcedureIdM proc_id;
        public string Title;
        public string Description;
        public string ClinicalNote;
        public string record_state;
        public bool need_acknowledge;
        public bool background_recording;

        // not currently supported
        //public List<string> procTypeAutolabelOverrides;
        //public List<string> commonAutolabelOverrides;

        #endregion

        public void InitActive(IMonsoonResMgr rm, eRecordState recState, ClinInfoData clinInfo,
            ProcedureData procData, PersonNameData phys) //, List<string> procTypeAutolabelOverrides, List<string> commonAutolabelOverrides)
        {
            try
            {
                // horrible logic to accomodate merging
                record_state = TranslateRecState(recState, out  background_recording);
                HostName = rm.MyIp;
                IsClinical = true;
                Patient = ClinInfoDataExM.ClinInfoDataM.FromClinInfoData(rm, clinInfo);
                Created = DateTime.Today;
                proc_id = procData != null ? ProcedureIdM.FromProcId(procData.m_Id):null;
                ClinicalNote = procData?.m_strClinicalNote;
                need_acknowledge = procData?.m_bNeedAcknowledge ?? false;
                if (phys != null)
                    physician = PersonNameDataM.FromPersonNameData(phys);

                bool bProcModifiable = string.IsNullOrEmpty(clinInfo?.m_strScheduleId) ||
                                       clinInfo.m_strScheduleId.StartsWith(HL7Common.IsmPrefix, StringComparison.OrdinalIgnoreCase);
                proc_modifiable = bProcModifiable ? 1 : 0;
                sharing = new UlacInfo(EUlac.Public, null);

                //this.procTypeAutolabelOverrides = procTypeAutolabelOverrides;
                //this.commonAutolabelOverrides = commonAutolabelOverrides;
            }
            catch (Exception ex)
            {
                rm.LogEvent(EventLogEntryType.Error, 0, "DmagM.InitActive err: " + ex.Message, 3);
            }
        }

        public void SetActiveContent(IMonsoonResMgr rm, List<ChannelInfo> channelsInfo,
            List<VideoData> allVideo, List<ImageData> images,
            List<ProcMovieData> pmVideos)
        {
            try
            {
                channels = channelsInfo;
                Content = new DmagContentM
                {
                    Movies = new List<MovieMonsoon>(),
                    MovieActive = new List<MovieMonsoon>()
                };
                foreach (VideoData video in allVideo)
                {
                    MovieMonsoon movie = new MovieMonsoon(video);
                    List<MovieMonsoon> movieList = video.m_bFinished ? Content.Movies : Content.MovieActive;
                    movieList.Add(movie);
                }

                // pictures
                Content.Pictures = GetPictures(images);

                // proc movie(s)
                Content.ProcedureMovies = pmVideos.Select(vid => new DmagProcMovieM(vid)).ToList();
            }
            catch (Exception ex)
            {
                rm.LogEvent(EventLogEntryType.Error, 0, "SetActiveContent err: " + ex.Message, 3);
            }
        }
        

        public static string TranslateRecState(eRecordState recState, out bool backgroundRec)
        {
            backgroundRec = RsUtils.IsPMRecording(recState);

            if (!RsUtils.IsRecording(recState))
                return recState.ToString();
            else
                return RsUtils.IsMovRecording(recState) ? "proc_recording" : eRecordState.proc_not_recording.ToString();
        }


        public void InitLib(IMonsoonResMgr rm, DMAGFileMgr dmag, Dictionary<ePayloadType, TaskInfo> libTasks,
            EUlac ulac, bool bModify, /*string strXmlCompareList*/
            List<Dictionary<ESearchFields, SearchResultFieldBase>> compList = null)
        {
            try
            {
                HostName = rm.MonsoonLibHost;
                IsClinical = dmag.IsClinical;
                proc_id = ProcedureIdM.FromProcId(dmag.ProcId);
                Patient = ClinInfoDataExM.ClinInfoDataM.FromClinInfoData(rm, dmag.Procedure);
                Title = dmag.Title;
                Description = dmag.Description;
                ClinicalNote = dmag.ClinicalNote;
                physician = PersonNameDataM.FromPersonNameData(dmag.SafePhysician);
                Created = dmag.History.Created;
                Content = new DmagContentM(dmag.Content);
                video_edit = new VideoEditSettingsM(dmag.m_DmagData.AutoEditSettings);
                RootPath = LibCommon.GetCompleteLibPathFromDmag(string.Empty, dmag.ProcId.m_strLibId);

                channels = dmag.Content.Channels.Select(chan =>
                    new ChannelInfo(chan.Channel,
                        dmag.Content.Movies.Where(mov => mov.Stream.Equals(chan.Channel)).Select(mov => mov.Length)
                            .Sum(),
                        dmag.Content.Pictures.Count(pic => pic.Stream.Equals(chan.Channel))));

                // compare?
                compare_list = SearchResultsToWebFormat(compList);
                tasks = libTasks;
                sharing = new UlacInfo(ulac, dmag.m_DmagData.AdditionalUsers);
                proc_modifiable = bModify ? 1 : 0;
                need_acknowledge = false;
            }
            catch (Exception e)
            {
                rm.LogEvent(EventLogEntryType.Error, 0, "DmagMonsoon.InitLib failed with err " +e.Message, 2);
            }
        }

        public static List<Dictionary<ESearchFields, string>> SearchResultsToWebFormat(List<Dictionary<ESearchFields, SearchResultFieldBase>> searchResults)
        {
            if (null == searchResults)
            {
                return null;
            }

            List<Dictionary<ESearchFields, string>> results = new List<Dictionary<ESearchFields, string>>();
            foreach (Dictionary<ESearchFields, SearchResultFieldBase> dict in searchResults)
            {
                if (null == dict)
                {
                    // note/todo - don't expect this to happen, but preserving old behavior.  maybe address later
                    results.Add(null);
                }
                else
                {
                    Dictionary<ESearchFields, string> curResults = new Dictionary<ESearchFields, string>();
                    foreach (KeyValuePair<ESearchFields, SearchResultFieldBase> kvp in dict)
                    {
                        curResults.Add(kvp.Key, kvp.Value?.ToDataString());
                    }

                    results.Add(curResults);
                }
            }

            return results;
        }

        public static List<DmagPictureM> GetPictures(List<ImageData> images)
        {

            // pictures
            List < DmagPictureM >  picutures = new List<DmagPictureM>();
            foreach (ImageData img in images)
            {
                DmagPictureM pic = new DmagPictureM(img);
                picutures.Add(pic);
            }
            return picutures;
        }

        public class UlacInfo
        {
            public enum EUlacM
            {
                Private,
                Public,
                Department
            };

            // ReSharper disable once InconsistentNaming
            public EUlacM status;
            // ReSharper disable once InconsistentNaming
            public List<string> shared_with;

            public UlacInfo(EUlac stat, List<string> shared)
            {
                status = (EUlacM)Enum.Parse(typeof(EUlacM), stat.ToString());
                shared_with = shared;
            }
        }


        public class ChannelInfo
        {
            // ReSharper disable once InconsistentNaming
            public string channel;
            // ReSharper disable once InconsistentNaming
            public double length;
            // ReSharper disable once InconsistentNaming
            public int images;
            // ReSharper disable once InconsistentNaming
            public bool enabled;
            // ReSharper disable once InconsistentNaming
            public int m_nSelectedInput;

            public ChannelInfo()
            {

            }

            public ChannelInfo(string ch, double l, int i)
            {
                channel = ch;
                length = l;
                images = i;
                enabled = false;
            }
        }


        public class CompareInfo
        {
            // ReSharper disable once InconsistentNaming
            public string m_strLibId;
            // ReSharper disable once InconsistentNaming
            public string m_strLibName;
            // ReSharper disable once InconsistentNaming
            public string m_strMRN;
            // ReSharper disable once InconsistentNaming
            public DateTimeOffset m_dtCreated;

            public CompareInfo() { }
            public CompareInfo(XmlNode ndEntry)
            {
                m_strLibId = XmlUtils.El2String(ndEntry, ESearchFields.libid.ToString().ToLower());
                m_strLibName = XmlUtils.El2String(ndEntry, ESearchFields.libname.ToString().ToLower());
                m_strMRN = XmlUtils.El2String(ndEntry, ESearchFields.mrn.ToString().ToLower());
                m_dtCreated = XmlUtils.El2DT(ndEntry, ESearchFields.date.ToString().ToLower());
            }
        }

        public class DmagContentItem
        {
            public string RelativePath;
            public string Description;
            public DateTimeOffset CreatedTime;

            public DmagContentItem()
            {

            }

            public DmagContentItem(clsIsmDmagPath path)
            {
                RelativePath = path.RelativePath;
                Description = path.Description;
                CreatedTime = path.Created;
            }

            public DmagContentItem(ContentItem item)
            {
                RelativePath = System.IO.Path.GetFileName(item.m_strPath);
                Description = item.m_strLabel;
                CreatedTime = item.Timestamp;
            }
        }


        public class DmagPathM : DmagContentItem
        {
            public string Channel;

            public DmagPathM()
            {
            }

            public DmagPathM(clsIsmDmagPath path) : base(path)
            {
                Channel = path.Stream;
            }

            public DmagPathM(ContentItem item) : base(item)
            {
                Channel = item.m_strEncoderName;
            }

        }      

        public class DmagContentM
        {
            public List<DmagContentItem> Reports { get; }
            public List<DmagPictureM> Pictures { get; set; }
            public List<MovieMonsoon> Movies { get; set; }
            public List<MovieMonsoon> MovieActive { get;  set; }
            public List<DmagMarkerMonsoon> Markers { get; }
            public List<DmagProcMovieM> ProcedureMovies { get; set; }


            public DmagContentM()
            {

            }

            public DmagContentM(clsContent content)
            {
                try
                {
                    Pictures = content.Pictures.Select(pic => new DmagPictureM(pic)).ToList();
                    Movies = content.Movies.Select(mov => new MovieMonsoon(mov)).ToList();
                    ProcedureMovies = content.ProcMovies?.Select(pm => new DmagProcMovieM(pm)).ToList();
                    Reports = content.Reports.Select(rep => new DmagContentItem(rep)).ToList();

                    Markers = new List<DmagMarkerMonsoon>();
                    foreach (clsMarker marker in content.Markers) // this logic probably needs to change...
                    {
                        clsMovie mov = content.Movies.FirstOrDefault(movie => movie.Created <= marker.Created && movie.Created >= marker.Created);
                        if (mov != null) // do not add those without the movie
                            Markers.Add(new DmagMarkerMonsoon(marker, mov));
                    }
                }
                catch (Exception e)
                {
                    IsmLogCommon.IsmLog.LogEvent(EventLogEntryType.Error, 0, "DmagContentM constructor failed with err "+e.Message,2);
                }
            }
        }

        public class DmagPictureM : DmagPathM
        {
            public string Thumbnail;
            public EImgPrintState PrintState = EImgPrintState.ToBePrinted;
            public DmagPictureM(clsPicture pic) : base(pic)
            {
                Thumbnail = pic.ThumbName;
            }
            public DmagPictureM(ImageData img) : base(img)
            {
                Thumbnail = clsPicture.GetThumbnailFileName(RelativePath);
                PrintState = img.m_ImgPrintState;
            }

        }

        public enum EPMActionM
        {
             start_proc,
             //stop_proc,
             start_rec,
             //stop_rec,
             cap_img
        }

        public class ProcMovieActionM
        {
            public DateTimeOffset Start;
            public DateTimeOffset Stop;
            public string m_strFn = "";
            public EPMActionM m_ActionKind = EPMActionM.start_proc;
            public string Label = "";
            
            public ProcMovieActionM()
            {

            }

            public static ProcMovieActionM GetMonsoonProvMovieActions(ProcMovieAction action)
            {

                // first action - always
                ProcMovieActionM monsoonAction = new ProcMovieActionM
                {
                    Start = SimplifyDT(action.Start),
                    Stop = SimplifyDT(action.Stop),
                    m_strFn = action.RelativePath,
                    Label =  action.Label
                };
                switch (action.ActionKind)
                {
                    case EPMAction.image:
                        monsoonAction.m_ActionKind = EPMActionM.cap_img;
                        break;
                    case EPMAction.procedure:
                        monsoonAction.m_ActionKind = EPMActionM.start_proc;
                        break;
                    case EPMAction.recording:
                        monsoonAction.m_ActionKind = EPMActionM.start_rec;
                        break;
                }               

                return monsoonAction;
            }

            /// <summary>
            /// We get into trouble when we smetimes return DT with tz modifier, and sometimes /wout
            ///     Correcting to always return w/out
            /// </summary>
            /// <param name="dt"></param>
            /// <returns></returns>
            private static DateTimeOffset SimplifyDT(DateTimeOffset dt) 
            {
                var dtRet = dt;
                // we MUST have 3 digits for ms!
                if (dtRet.Millisecond < 1)
                    dtRet = dtRet.AddMilliseconds(1);

                return dtRet;
            }


            public ProcMovieAction Translate()
            {
                return new ProcMovieAction
                {
                    ActionKind = EPMAction.recording,
                    Start = Start,
                    Stop = Stop,
                    Label = Label
                };
            }
        }

        public class DmagProcMovieM  
        {
            public double Length;
            public List<ProcMovieActionM> Actions = new List<ProcMovieActionM>();
            public string Channel;
            public string RelativePath;
            public string Label;

            public DmagProcMovieM()
            {

            }

            /// <summary>
            /// Used for library procedures
            /// </summary>
            public DmagProcMovieM(clsProcMovie procMov)
            {
                if (procMov == null)
                    return;
                Length = procMov.Length;
                Channel = procMov.Stream;
                RelativePath = procMov.RelativePath;
                Label = procMov.Description;

                // translate actions & sort by time
                var monsoonActions = procMov.Actions.Select(ProcMovieActionM.GetMonsoonProvMovieActions);
                monsoonActions = monsoonActions.OrderBy(act => act.Start);
                Actions = monsoonActions.ToList();
            }
            
            /// <summary>
            /// Used for active procedure
            /// </summary>
            public DmagProcMovieM(ProcMovieData procMov)
            {
                if (procMov == null)
                    return;
                Length = procMov.m_dSeconds;
                Channel = procMov.m_strEncoderName;
                RelativePath = procMov.m_strPath;
                Label = procMov.m_strLabel;
                Actions = procMov.m_Actions.Select(ProcMovieActionM.GetMonsoonProvMovieActions).ToList();
            }
        }



        public class DmagMarkerMonsoon
        {
            public clsMarker.enumMarkerType MarkerType;
            public string Name;
            public string MovieName;
            public double OffsetSecs;

            public DmagMarkerMonsoon(clsMarker marker, clsMovie mov)
            {
                MarkerType = marker.MarkerType;
                Name = marker.Annotations;
                MovieName = mov.RelativePath;
                OffsetSecs = (marker.Created - mov.Created).TotalSeconds;

            }
        }

    }
}
 
 