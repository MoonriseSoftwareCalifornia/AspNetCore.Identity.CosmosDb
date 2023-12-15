using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AspNetCore.Identity.CosmosDb.EntityConfigurations
{
    public class RoleEntityTypeConfiguration<TRoleEntity, TKey> : IEntityTypeConfiguration<TRoleEntity>
        where TRoleEntity : IdentityRole<TKey>
        where TKey : IEquatable<TKey>
    {
        private readonly string _tableName;

        public RoleEntityTypeConfiguration(string tableName = "Identity_Roles")
        {
            _tableName = tableName;
        }

        public void Configure(EntityTypeBuilder<TRoleEntity> builder)
        {
            builder.HasKey(_ => _.Id);
            builder.HasPartitionKey(_ => _.Id);
            builder.Property(_ => _.ConcurrencyStamp).IsETagConcurrency();

            builder.ToContainer(_tableName);
        }
    }
}