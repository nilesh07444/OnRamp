namespace Common.Command
{
    public interface ICommandHandlerBase<in TParameter> 
    {
        /// <summary>
        ///     Executes a command handler
        /// </summary>
        /// <param name="command">The command to be used</param>
        CommandResponse Execute(TParameter command);
    }
}