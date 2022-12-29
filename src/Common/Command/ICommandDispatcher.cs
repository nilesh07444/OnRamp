using Common.Command;

namespace Common.Command
{
    public interface ICommandDispatcher
    {
        /// <summary>
        ///     Dispatches a command to its handler
        /// </summary>
        /// <param name="command">The command to be passed to the handler</param>
        CommandResponse Dispatch<TCommand>(TCommand command);
    }
}