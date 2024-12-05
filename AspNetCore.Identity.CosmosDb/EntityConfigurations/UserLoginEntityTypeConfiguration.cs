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
        private readonly int _maxKeyLength;

        public UserLoginEntityTypeConfiguration(int maxKeyLength, string tableName = "Identity_Logins")
        {
            _tableName = tableName;
            _maxKeyLength = maxKeyLength;
        }

        public void Configure(EntityTypeBuilder<IdentityUserLogin<TKey>> builder)
        {
            builder.HasKey(l => new { l.LoginProvider, l.ProviderKey });

            builder
                .UseETagConcurrency()
                .HasPartitionKey(_ => _.ProviderKey);

            if (_maxKeyLength > 0)
            {
                builder.Property(l => l.LoginProvider).HasMaxLength(_maxKeyLength);
                builder.Property(l => l.ProviderKey).HasMaxLength(_maxKeyLength);
            }

            builder.ToContainer(_tableName);
        }
    }
}