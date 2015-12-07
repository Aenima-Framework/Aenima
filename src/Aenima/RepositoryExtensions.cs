using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Aenima
{
    public static class RepositoryExtensions
    {
        public static Task Update<TAggregate>(
            this IRepository repository, 
            TAggregate aggregate,
            Action<TAggregate> command, 
            IDictionary<string, string> headers = null)
            where TAggregate : class, IAggregate, new()
        {
            command(aggregate);
            return repository.Save(aggregate, headers);
        }

        public static Task Create<TAggregate>(
            this IRepository repository,
            Action<TAggregate> command,
            IDictionary<string, string> headers = null)
            where TAggregate : class, IAggregate, new()
        {
            return repository.Update(new TAggregate(), command, headers);
        }

        public static async Task Update<TAggregate>(
            this IRepository repository,
            string aggregateId,
            Action<TAggregate> command,
            IDictionary<string, string> headers = null)
            where TAggregate : class, IAggregate, new()
        {
            var aggregate = await repository
                .GetById<TAggregate>(aggregateId)
                .ConfigureAwait(false);

            command(aggregate);

            await repository
                .Save(aggregate, headers)
                .ConfigureAwait(false);
        }

        public static Task<TAggregate> GetById<TAggregate>(
            this IRepository repository, 
            string aggregateId)
            where TAggregate : class, IAggregate, new()
        {
            return repository.GetById<TAggregate>(aggregateId, int.MaxValue);
        }
    }
}