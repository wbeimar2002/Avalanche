using System;
using ISM.Library.Types;

namespace MonsoonAPI.models
{
    public class PrintReportGenerateInfo
    {
        public enum EPrintOritentationM { Portrait, Landscape };
        public enum EPatientDataOptionsM { False, True, Never, Undefined };
        public class PrintLayoutInfoM
        {
            // ReSharper disable once InconsistentNaming
            public EPrintOritentationM m_Orientation;
            // ReSharper disable once InconsistentNaming
            public string m_strLayout;

            public PrintLayoutInfo ToPrintLayoutInfo()
            {
                return new PrintLayoutInfo
                {
                    m_strLayout = m_strLayout,
                    m_Orientation = (EPrintOritentation)Enum.Parse(typeof(EPrintOritentation), m_Orientation.ToString())
                };
            }
        }

        public class PrintSettingsM
        {
            // ReSharper disable once InconsistentNaming
            public PrintLayoutInfoM m_PrintLayout;
            // ReSharper disable once InconsistentNaming
            public EPatientDataOptionsM m_PatDataOptions;
            // ReSharper disable once InconsistentNaming
            public int m_nCopies;
            // ReSharper disable once InconsistentNaming
            public string m_strPrinter;
            // ReSharper disable once InconsistentNaming
            // TODO - this should not be here. UI should not have to pass it in, but printing causes presets to save, and otherwise autoprint will get wiped on manual print.
            //      - we need to able to specify a subset to save...revisit after release - too complicated to fix safely in the next day.
            public bool m_bAutoPrint;

            public PrintSettings ToPrintSettings()
            {
                var ps = new PrintSettings
                {
                    m_nCopies = m_nCopies,
                    m_strPrinter = m_strPrinter,
                    m_PatDataOptions = (EPatientDataOptions)Enum.Parse(typeof(EPatientDataOptions), m_PatDataOptions.ToString()),
                    m_PrintLayout = m_PrintLayout?.ToPrintLayoutInfo(),
                    m_bAutoPrint = m_bAutoPrint
                };
                if (ps.m_nCopies == 0)
                    ps.m_nCopies = 1;
                return ps;
            }
        }
        // ReSharper disable once InconsistentNaming
        public ProcedureIdM m_ProcId;
        // ReSharper disable once InconsistentNaming
        public PrintSettingsM m_Settings;// ReSharper disable once InconsistentNaming
        public string m_strImageList;
        // ReSharper disable once InconsistentNaming
        public bool m_bDoPrint;
        // ReSharper disable once InconsistentNaming
        public string m_strReportName;
        // ReSharper disable once InconsistentNaming
        public string m_strUser;
    }
}
