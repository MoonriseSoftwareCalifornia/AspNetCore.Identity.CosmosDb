using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AspNetCore.Identity.CosmosDb.EntityConfigurations
{
    public class UserLoginEntityTypeConfiguration<TKey> : IEntityTypeConfiguration<IdentityUserLogin<TKey>>
        where TKey : IEquatable<TKey>
    {
        private readonly string _tableName;

        public UserLoginEntityTypeConfiguration(string tableName = "Identity_Logins")
        {
            _tableName = tableName;
        }

        public void Configure(EntityTypeBuilder<IdentityUserLogin<TKey>> builder)
        {
            builder
                .UseETagConcurrency()
                .HasPartitionKey(_ => _.ProviderKey);

            builder.ToContainer(_tableName);
        }
    }
}