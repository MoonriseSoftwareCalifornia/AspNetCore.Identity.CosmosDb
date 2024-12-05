using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AspNetCore.Identity.CosmosDb.EntityConfigurations
{
    public class UserRoleEntityTypeConfiguration<TKey> : IEntityTypeConfiguration<IdentityUserRole<TKey>>
        where TKey : IEquatable<TKey>
    {
        private readonly string _tableName;

        public UserRoleEntityTypeConfiguration(string tableName = "Identity_UserRoles")
        {
            _tableName = tableName;
        }

        public void Configure(EntityTypeBuilder<IdentityUserRole<TKey>> builder)
        {
            builder
                .UseETagConcurrency()
                .HasPartitionKey(_ => _.UserId);

            builder.HasKey(r => new { r.UserId, r.RoleId });

            builder.ToContainer(_tableName);
        }
    }
}