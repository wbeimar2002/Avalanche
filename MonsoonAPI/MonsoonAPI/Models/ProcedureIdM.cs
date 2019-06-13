using IsmStateServer.Types;

namespace MonsoonAPI.models
{
    public class ProcedureIdM
    {
        // ReSharper disable once InconsistentNaming
        public string m_strLibId;
        // ReSharper disable once InconsistentNaming
        public string m_strLibName;
        // ReSharper disable once InconsistentNaming
        public string m_strVolName;

        public ProcedureId ToProcId()
        {
            return new ProcedureId(m_strLibId, m_strLibName, m_strVolName);
        }

        public static ProcedureIdM FromProcId(ProcedureId procId)
        {
            return new ProcedureIdM
            {
                m_strLibId = procId?.m_strLibId,
                m_strLibName = procId?.m_strLibName,
                m_strVolName = procId?.m_strVolName
            };
        }

        public override string ToString()
        {
            return $"{m_strLibId}~{m_strLibName}~{m_strVolName}";
        }
    }
}
