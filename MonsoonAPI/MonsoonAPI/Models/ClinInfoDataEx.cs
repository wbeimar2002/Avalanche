using System;
using IsmStateServer.Types;
using IsmRec.Types;
using ISM.Middleware2Si;

namespace MonsoonAPI.models
{
    public class ClinInfoDataExM
    {
        public enum eSexM { M, F, U, O };

        public class ClinInfoDataM
        {
            public PersonNameDataM m_PatName;
            public DateTime m_dtDob;
            public eSexM m_Sex;
            public string m_strAccession;
            public string m_strScheduleId;
            public string m_strExternalProcId;
            
            
            private string _procType; 

            /// <summary>
            /// Procedure type of this clinical information object. Empty for no procedure selected
            /// </summary>
            public string m_strProcType //this getter/setter is used such that this will never return null
            {
                get => string.IsNullOrWhiteSpace(_procType) ? string.Empty : _procType;
                set => _procType = string.IsNullOrWhiteSpace(value) ? string.Empty : value;
            }

            public static ClinInfoDataM FromClinInfoData(IMonsoonResMgr rm, ClinInfoData clinInfo)
            {
                try
                {
                    if (clinInfo == null)
                        return new ClinInfoDataM();

                    return new ClinInfoDataM
                    {
                        m_PatName = PersonNameDataM.FromPersonNameData(clinInfo.m_PatName),
                        m_dtDob = clinInfo.m_dtDob.ToDate(),
                        m_Sex = (eSexM)Enum.Parse(typeof(eSexM), clinInfo.m_Sex.ToString(), true),
                        m_strAccession = clinInfo.m_strAccession,
                        m_strExternalProcId = clinInfo.m_strExternalProcId,
                        m_strProcType = clinInfo.m_strProcType,
                        m_strScheduleId = clinInfo.m_strScheduleId
                    };
                }
                catch (Exception ex)
                {
                    rm.LogEvent(System.Diagnostics.EventLogEntryType.Error, 0, "FromClinInfoData err: " + ex.Message, 3);
                    return null;
                }
            }
            public ClinInfoData ToClinInfoData()
            {
                return new ClinInfoData
                {
                    m_PatName = m_PatName.ToPersonNameData(),
                    m_dtDob = m_dtDob.Date,
                    m_Sex = (eSex)Enum.Parse(typeof(eSex), m_Sex.ToString()),
                    m_strAccession = m_strAccession,
                    m_strExternalProcId = m_strExternalProcId,
                    m_strProcType = m_strProcType,
                    m_strScheduleId = m_strScheduleId
                };
            }
        }

        public ClinInfoDataM m_ClinInfo = null;
        public string m_strPhysician = string.Empty;
        public string m_strSharing = string.Empty;
        public string m_strClinicalNote = string.Empty;
        public string m_strUserId = string.Empty;
        public ProcedureIdM m_ProcId = null;
        public bool startPM = false;
        public string OriginatorIp;
        public bool skipAccessionSearch = false;
        public bool skipExternalIdSearch = false;

        public MwUtils.CredentialData Originator =>
            new MwUtils.CredentialData { m_strMachineName = OriginatorIp, m_strUserId = m_strUserId };
    }
}