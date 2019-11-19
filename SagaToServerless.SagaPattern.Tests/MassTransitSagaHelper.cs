using Automatonymous;
using Automatonymous.Contexts;
using System.Threading.Tasks;

namespace SagaToServerless.SagaPattern.Tests
{
    public class MassTransitSagaHelper
    {
        public static async Task<State> GetCurrentState<T, K>(T state, K saga) where T : class, SagaStateMachineInstance where K : MassTransitStateMachine<T>
        {
            var context = new StateMachineInstanceContext<T>(state);

            return await saga.GetState(context.Instance);
        }

    }
}
