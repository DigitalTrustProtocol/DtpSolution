﻿using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DtpCore.Notifications
{
    public class NotificationSegment : List<INotification>
    {
        private IMediator _mediator;

        public NotificationSegment(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Adds the Notification and return it self. Usefull when returning from a function.
        /// </summary>
        /// <param name="notification"></param>
        /// <returns>NotificationsResult</returns>
        public Task Publish(INotification notification)
        {
            Add(notification);
            return _mediator.Publish(notification);
        }

        /// <summary>
        /// Adds the Notification and return it self. Usefull when returning from a function.
        /// Waits for the task to complet.
        /// </summary>
        /// <param name="notification"></param>
        /// <returns>NotificationsResult</returns>
        public void PublishAndWait(INotification notification)
        {
            Add(notification);
            _mediator.Publish(notification).GetAwaiter().GetResult();
        }

        public T FindLast<T>()  where T: class, INotification
        {
            return FindLast(p => p is T) as T;
        }
    }
}
