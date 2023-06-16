using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AspNetCore.Identity.CosmosDb.EntityConfigurations
{
    public class UserTokensEntityTypeConfiguration<TKey> : IEntityTypeConfiguration<IdentityUserToken<TKey>>
        where TKey : IEquatable<TKey>
    {
        private readonly string _tableName;

        public UserTokensEntityTypeConfiguration(string tableName = "Identity_Tokens")
        {
            _tableName = tableName;
        }

        public void Configure(EntityTypeBuilder<IdentityUserToken<TKey>> builder)
        {
            builder
                .UseETagConcurrency()
                .HasPartitionKey(_ => _.UserId);

            builder.ToContainer(_tableName);
        }
    }
}