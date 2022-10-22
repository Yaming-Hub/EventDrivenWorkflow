﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EventDrivenWorkflow.Persistence;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EventDrivenWorkflow.Runtime.Data;
using System.Collections;

namespace Microsoft.EventDrivenWorkflow.Memory.Persistence
{
    public class EntityStore<TEntity> : IEntityStore<TEntity>
        where TEntity : IEntity
    {
        private readonly object lockObject;
        private readonly Dictionary<(string, string), TEntity> dictionary;
        private long etag = 0;

        public EntityStore()
        {
            this.lockObject = new object();
            this.dictionary = new Dictionary<(string, string), TEntity>();
        }

        public Task<TEntity> Get(string partitionKey, string key)
        {
            lock (this.lockObject)
            {
                if (!TryGetEntity(partitionKey, key, out var entity))
                {
                    throw new StoreException(StoreErrorCode.NotFound, "Entity not found");
                }

                return Task.FromResult(this.dictionary[(partitionKey, key)]);
            }
        }

        public Task<IEnumerable<TEntity>> GetMany(string partitionKey, IEnumerable<string> keys)
        {
            lock (this.lockObject)
            {
                List<TEntity> entities = new List<TEntity>();
                foreach (var key in keys)
                {
                    if (TryGetEntity(partitionKey, key, out var entity))
                    {
                        entities.Add(entity);
                    }
                }

                return Task.FromResult<IEnumerable<TEntity>>(entities);
            }
        }

        public Task<TEntity> GetOrAdd(string partitionKey, string key, Func<TEntity> getEntity)
        {
            lock (this.lockObject)
            {
                if (TryGetEntity(partitionKey, key, out var entity))
                {
                    return Task.FromResult(entity);
                }

                entity = getEntity();
                if (entity == null)
                {
                    throw new InvalidOperationException("The getEntity callback should not return null entity.");
                }

                var copy = CopyAndUpdateEtag(entity);
                dictionary.Add((partitionKey, key), copy);

                return Task.FromResult(copy);
            }
        }

        public Task Update(string partitionKey, string key, TEntity entity)
        {
            lock (this.lockObject)
            {
                if (!TryGetEntity(partitionKey, key, out var existingEntity))
                {
                    throw new StoreException(StoreErrorCode.NotFound, "Entity not found");
                }

                if (existingEntity.ETag != entity.ETag)
                {
                    throw new StoreException(StoreErrorCode.EtagMismatch, "Entity was changed.");
                }

                dictionary[(partitionKey, key)] = CopyAndUpdateEtag(entity);
            }

            return Task.CompletedTask;
        }

        public Task Upsert(string partitionKey, string key, TEntity entity)
        {
            lock (this.lockObject)
            {
                dictionary[(partitionKey, key)] = CopyAndUpdateEtag(entity);
            }

            return Task.CompletedTask;
        }


        public Task Delete(string partitionKey, string key)
        {
            lock (this.lockObject)
            {
                if (dictionary.ContainsKey((partitionKey, key)))
                {
                    dictionary.Remove((partitionKey, key));
                }
            }

            return Task.CompletedTask;
        }

        private bool TryGetEntity(string partitionKey, string key, out TEntity entity)
        {
            return this.dictionary.TryGetValue((partitionKey, key), out entity) && entity.ExpireDateTime > DateTime.UtcNow;
        }

        private TEntity Copy(TEntity entity)
        {
            return JsonSerializer.Deserialize<TEntity>(JsonSerializer.Serialize(entity));
        }

        private TEntity CopyAndUpdateEtag(TEntity entity)
        {
            var copy = JsonSerializer.Deserialize<TEntity>(JsonSerializer.Serialize(entity));
            copy.ETag = this.etag.ToString();
            this.etag++;

            return copy;
        }
    }
}