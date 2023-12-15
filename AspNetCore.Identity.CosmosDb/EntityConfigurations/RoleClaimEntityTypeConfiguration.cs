using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace AspNetCore.Identity.CosmosDb.EntityConfigurations
{
    public class RoleClaimEntityTypeConfiguration<TKey> : IEntityTypeConfiguration<IdentityRoleClaim<TKey>>
        where TKey : IEquatable<TKey>
    {
        private readonly string _tableName;

        public RoleClaimEntityTypeConfiguration(string tableName = "Identity_Roles")
        {
            _tableName = tableName;
        }

        public void Configure(EntityTypeBuilder<IdentityRoleClaim<TKey>> builder)
        {
            builder
                .HasDiscriminator().HasValue("IdentityRoleClaim<string>"); // Backward compatibility.

            builder
                .Property(_ => _.Id)
                .HasConversion(_ => _.ToString(), _ => Convert.ToInt32(_));

            builder
                .UseETagConcurrency()
                .HasPartitionKey(_ => _.Id);

            builder.ToContainer(_tableName);
        }
    }
}