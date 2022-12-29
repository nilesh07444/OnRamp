using System;
namespace Common.Command
{
    public abstract class CommandHandlerBase<T> : ICommandHandlerBase<T>
    {
        public abstract CommandResponse Execute(T command);
    }
     
}