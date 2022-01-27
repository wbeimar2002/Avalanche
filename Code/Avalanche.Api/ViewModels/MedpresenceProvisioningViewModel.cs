using System.Collections.Generic;
using Ism.SystemState.Models.Medpresence;

namespace Avalanche.Api.ViewModels
{
    public class MedpresenceInviteViewModel
    {
        public List<MedpresenceSecureGuest>? Invitees { get; set; }
    }
}
