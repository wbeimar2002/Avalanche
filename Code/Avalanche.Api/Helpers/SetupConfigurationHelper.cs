using System.Collections.Generic;
using System.Reflection;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Infrastructure.Enumerations;

namespace Avalanche.Api.Helpers
{
    public static class SetupConfigurationHelper
    {
        /// <summary>
        /// Relationship between SetupConfiguration and PatientViewModel Api Properties
        /// </summary>
        /// <returns>PatienInfoHelper Dictionary</returns>
        public static Dictionary<SetupConfigurationInfo, PropertyInfo> PatientInfoHelper()
        {
            var setupInfo = new Dictionary<SetupConfigurationInfo, PropertyInfo>
            {                
                //Pair Key-Value: PropertyName - SetupConfigurationName(json file)
                { SetupConfigurationInfo.mrn, typeof(PatientViewModel).GetProperty(nameof(PatientViewModel.MRN)) },
                { SetupConfigurationInfo.firstName, typeof(PatientViewModel).GetProperty(nameof(PatientViewModel.FirstName)) },
                { SetupConfigurationInfo.lastName, typeof(PatientViewModel).GetProperty(nameof(PatientViewModel.LastName)) },
                { SetupConfigurationInfo.sex, typeof(PatientViewModel).GetProperty(nameof(PatientViewModel.Sex)) },
                { SetupConfigurationInfo.dateOfBirth, typeof(PatientViewModel).GetProperty(nameof(PatientViewModel.DateOfBirth)) },
                { SetupConfigurationInfo.physician, typeof(PatientViewModel).GetProperty(nameof(PatientViewModel.Physician)) },
                { SetupConfigurationInfo.department, typeof(PatientViewModel).GetProperty(nameof(PatientViewModel.Department)) },
                { SetupConfigurationInfo.procedureType, typeof(PatientViewModel).GetProperty(nameof(PatientViewModel.ProcedureType)) },
                //{ SetupConfigurationInfo.AccessionNumber, typeof(PatientViewModel).GetProperty(nameof(PatientViewModel.AccessionNumbrer)) },
                //{ SetupConfigurationInfo.ScopeSerialNumber, typeof(PatientViewModel).GetProperty(nameof(PatientViewModel.ScopeSerialNumber)) }
            };

            return setupInfo;
        }
    }
}
