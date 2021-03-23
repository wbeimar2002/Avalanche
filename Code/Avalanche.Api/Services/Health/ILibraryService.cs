using Ism.Library.V1.Protos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Health
{
    public interface ILibraryService
    {
        Task DiscardActiveProcedure(DiscardActiveProcedureRequest discardActiveProcedureRequest);
        Task CommitActiveProcedure(CommitActiveProcedureRequest commitActiveProcedureRequest);
        Task<AllocateNewProcedureResponse> AllocateNewProcedure(AllocateNewProcedureRequest allocateNewProcedureRequest);
    }
}
