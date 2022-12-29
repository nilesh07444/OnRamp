using Domain.Customer.Models;
using Domain.Models;
using Ramp.Services.Implementations;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Ramp.Services
{
    public interface INotificationService
    {
        void Send(params INotification[] notifications);
    }

    public interface INotification
    {
        NotificationType Type { get; }
        IEnumerable<User> Recipients { get; }
        IEnumerable<StandardUser> StandardUserRecipients { get; }
        string Reference { get; }
        string Title { get; }
        Guid Id { get; }
    }
}