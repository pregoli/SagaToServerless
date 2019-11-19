using System;
using System.Collections.Generic;
using System.Text;

namespace SagaToServerless.Durable.Dto
{
    public class WorkflowStepResult
    {
        public WorkflowStepResult(
            string actionName,
            Guid outputId,
            bool successfull = true,
            string reason = "")
        {
            ActionName = actionName;
            OutputId = outputId;
            Successfull = successfull;
            Reason = reason;
        }

        public string ActionName { get; set; }
        public Guid OutputId { get; set; }
        public bool Successfull { get; set; }
        public string Reason { get; set; }
    }
}
