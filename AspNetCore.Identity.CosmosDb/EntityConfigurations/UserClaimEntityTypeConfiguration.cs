using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace AspNetCore.Identity.CosmosDb.EntityConfigurations
{
    public class UserClaimEntityTypeConfiguration<TKey> : IEntityTypeConfiguration<IdentityUserClaim<TKey>>
        where TKey : IEquatable<TKey>
    {
        private readonly string _tableName;

        public UserClaimEntityTypeConfiguration(string tableName = "Identity")
        {
            _tableName = tableName;
        }

        public void Configure(EntityTypeBuilder<IdentityUserClaim<TKey>> builder)
        {
            builder
                .HasDiscriminator().HasValue("IdentityUserClaim<string>"); // Backward compatibility.

            builder
                .Property(_ => _.Id)
                .HasConversion(_ => _.ToString(), _ => Convert.ToInt32(_));

            builder
                .UseETagConcurrency()
                .HasPartitionKey(_ => _.Id)
                .ToContainer(_tableName);
        }
    }
}