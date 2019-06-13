using System.Linq;
using ISM.Library.Types;
using ISM.LibrarySi;
using IsmLogCommon;

namespace MonsoonAPI.models
{
    public class WorkItemExportM
    {
        public string[] m_ItemsToExport;
        public ProcedureIdM m_ProcId;
        public ePayloadType m_PayloadType;
        public string user;
        public string m_strDestination;

        public LibWorkItemNonPrint ToWorkItemExport(AccessInfo accessInfo)
        {
            return new LibWorkItemNonPrint(accessInfo)
            {
                Items = m_ItemsToExport?.Where(i => !string.IsNullOrEmpty(i)).Select(i => new FileUploadInfo{m_strFileName =  i}).ToList(),
                ProcId = m_ProcId.ToProcId(),
                PayloadType = m_PayloadType,
                Destination = m_strDestination
            };

        }
    }
}
