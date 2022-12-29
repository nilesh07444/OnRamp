using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Common.Command
{
    public sealed class CommandDispatcher : ICommandDispatcher
    {
        public CommandResponse Dispatch<TCommand>(TCommand command)
        {
            var commandHandlerAndValidator = ServiceLocator.Current.GetService(typeof(ICommandHandlerAndValidator<>).MakeGenericType(typeof(TCommand)));
            if (commandHandlerAndValidator == null || !(commandHandlerAndValidator is ICommandHandlerAndValidator<TCommand>))
            {
                var commandHandler = ServiceLocator.Current.GetService(typeof(ICommandHandlerBase<>).MakeGenericType(typeof(TCommand)));
                if (commandHandler == null || !(commandHandler is ICommandHandlerBase<TCommand>))
                    throw new InvalidOperationException("No ICommandHandler<" + command.GetType() + "> found for " +
                                                    command.GetType());
                var validator = ServiceLocator.Current.GetService(typeof(IValidator<>).MakeGenericType(command.GetType()));
                if (validator != null && validator is IValidator<TCommand>)
                {
                    var validation = (validator as IValidator<TCommand>).Validate(command);
                    if (validation.Any())
                        return new CommandResponse { Validation = validation };
                }
                (commandHandler as ICommandHandlerBase<TCommand>).Execute(command);
                return new CommandResponse();

            }
            else
            {
                var val = (commandHandlerAndValidator as ICommandHandlerAndValidator<TCommand>).Validate(command);
                if (val.Any())
                    return new CommandResponse { Validation = val };
                (commandHandlerAndValidator as ICommandHandlerAndValidator<TCommand>).Execute(command);
                return new CommandResponse();
            }
            throw new InvalidOperationException("No ICommandHandler<" + command.GetType() + "> found for " +
                                            command.GetType());
        }
    }
}