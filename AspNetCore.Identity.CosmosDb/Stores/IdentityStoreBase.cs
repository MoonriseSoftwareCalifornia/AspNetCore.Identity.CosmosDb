using AspNetCore.Identity.CosmosDb.Contracts;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace AspNetCore.Identity.CosmosDb.Stores
{
    /// <summary>
    /// Identity store base
    /// </summary>
    public abstract class IdentityStoreBase
    {
        private readonly IRepository _repo;

        public IdentityStoreBase(IRepository repo)
        {
            _repo = repo;
        }

        /// <summary>
        /// Processes exceptions thrown by a store method
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        protected IdentityResult ProcessExceptions(Exception e)
        {
            var errors = new List<IdentityError>();

            errors.Add(new IdentityError()
            {
                Code = "500", Description = e.Message
            });

            return IdentityResult.Failed(errors.ToArray());
        }

        /// <summary>
        /// Gets an identity error
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        protected IdentityError Get(Exception e)
        {
            if (e.GetType() == typeof(Microsoft.Azure.Cosmos.CosmosException))
            {
                var error = (Microsoft.Azure.Cosmos.CosmosException)e;
                return new IdentityError()
                {
                    //; ; Code = error.
                };
            }
            return new IdentityError() { Code = "500", Description = e.Message };
        }
    }
}
