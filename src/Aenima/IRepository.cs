using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Aenima
{
    public interface IRepository
    {
        Task<TAggregate> GetById<TAggregate>(string identity, int version) where TAggregate : class, IAggregate, new();
        Task Save<T>(T aggregate, IDictionary<string, string> headers = null) where T : class, IAggregate;
    }
}