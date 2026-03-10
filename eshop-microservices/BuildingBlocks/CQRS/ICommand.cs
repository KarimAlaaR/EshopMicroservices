using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace BuildingBlocks.CQRS
{
    public interface ICommand : IRequest<Unit>
    {
    }
    public interface ICommand<out TResponse> : IRequest<TResponse>
            where TResponse : notnull
    {
    }
}
