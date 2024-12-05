using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AspNetCore.Identity.CosmosDb.EntityConfigurations
{
    public class UserTokensEntityTypeConfiguration<TKey> : IEntityTypeConfiguration<IdentityUserToken<TKey>>
        where TKey : IEquatable<TKey>
    {
        private readonly int _maxKeyLength;
        private readonly string _tableName;

        public UserTokensEntityTypeConfiguration(int maxKeyLength, string tableName = "Identity_Tokens")
        {
            _maxKeyLength = maxKeyLength;
            _tableName = tableName;
        }

        public void Configure(EntityTypeBuilder<IdentityUserToken<TKey>> builder)
        {
            builder.HasKey(t => new { t.UserId, t.LoginProvider, t.Name });

            builder
                .UseETagConcurrency()
                .HasPartitionKey(_ => _.UserId);

            if (_maxKeyLength > 0)
            {
                builder.Property(t => t.LoginProvider).HasMaxLength(_maxKeyLength);
                builder.Property(t => t.Name).HasMaxLength(_maxKeyLength);
            }


            builder.ToContainer(_tableName);
        }
    }
}