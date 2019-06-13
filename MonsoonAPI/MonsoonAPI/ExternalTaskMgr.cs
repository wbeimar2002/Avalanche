using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PatInfoEngine.Types;

namespace MonsoonAPI
{
    public abstract class ExternalTaskMgr
    {
        public enum Tasks
        {
            DICOMExport,
            DICOMImport,
            HL7Export,
            HL7Import,
            CopyTask
        }

        public static Tasks Ism2MonsoonTask(EPieTask pieTask)
        {
            switch (pieTask)
            {
                case EPieTask.DICOMExport: return Tasks.DICOMExport;
                case EPieTask.DICOMImport: return Tasks.DICOMImport;
                case EPieTask.HL7Export: return Tasks.HL7Export;
                case EPieTask.HL7Import: return Tasks.HL7Import;
                default: throw new ArgumentOutOfRangeException(nameof(pieTask), pieTask, null);
            }
        }
    }
}
