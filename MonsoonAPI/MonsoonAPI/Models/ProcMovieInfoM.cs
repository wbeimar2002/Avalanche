using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using IsmLogCommon;
using IsmRec.Types;

namespace MonsoonAPI.models
{
    public class ProcMovieInfoM
    {

        public class PmEditInfo
        {
            public string RelativePath;
            public string Label;
            public List<DmagM.ProcMovieActionM> Actions;

            public override string ToString()
            {
                var ret = $"{RelativePath}: {(string.IsNullOrEmpty(Label) ? "" : Label + ": ")}";

                if (Actions?.Any() == true)
                {
                    var actions = Actions.Select(act => act.ToString());
                    ret += $"{string.Join("; ", actions)}";
                }
                return ret;
            }
        }

        public enum EMovieModeM
        {
            // ReSharper disable once InconsistentNaming
            clips,
            // ReSharper disable once InconsistentNaming
            movies
        }

        public ProcedureIdM ProcId;
        //    public List<DmagM.DmagProcMovieM> ProcedureMovies;
        public Dictionary<string, PmEditInfo> ProcedureMovies;
        public bool EditNow = false;
        // ReSharper disable once InconsistentNaming
        public EMovieModeM generate_clips_or_movies = EMovieModeM.clips;

        public string user;

        public static eRecording_Mode ToRecMode(EMovieModeM mode)
        {
            return mode == EMovieModeM.clips ? eRecording_Mode.clip : eRecording_Mode.movie;
        }
        public static EMovieModeM FromRecMode(eRecording_Mode mode)
        {
            return mode == eRecording_Mode.clip ? EMovieModeM.clips : EMovieModeM.movies;
        }

        public List<ProcMovieData> TranslateProcMovies()
        {
            try
            {
                List<ProcMovieData> pms = new List<ProcMovieData>();
                foreach (var chan2Pm in ProcedureMovies)
                {
                    IsmLog.LogEvent(EventLogEntryType.Information, 0, "Edit received: " + chan2Pm.Value, 4);
                    var pm = new ProcMovieData
                    {
                        m_strEncoderName = chan2Pm.Key,
                        m_strLabel = chan2Pm.Value.Label,
                        m_strPath = chan2Pm.Value.RelativePath,
                        m_Actions = chan2Pm.Value.Actions?.Select(act => act.Translate()).ToList() ?? new List<ProcMovieAction>()
                    };
                    pms.Add(pm);
                }
                return pms;
            }
            catch (Exception e)
            {
                IsmLog.LogEvent(EventLogEntryType.Error, 0, "TranslateProcMovies failed with err " + e.Message, 3);
                return null;
            }
          
        }
    }
}
