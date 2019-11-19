using System;
using System.Collections.Generic;
using System.Text;

namespace SagaToServerless.Common.Commands
{
    public interface ICommand
    {
        Guid CorrelationId { get; set; }
    }
}
