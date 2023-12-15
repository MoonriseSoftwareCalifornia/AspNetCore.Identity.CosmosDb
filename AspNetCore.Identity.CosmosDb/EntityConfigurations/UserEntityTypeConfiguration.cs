using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AspNetCore.Identity.CosmosDb.EntityConfigurations
{
    public class UserEntityTypeConfiguration<TUserEntity, TKey>
        : IEntityTypeConfiguration<TUserEntity>
        where TUserEntity : IdentityUser<TKey>
        where TKey : IEquatable<TKey>
    {
        private readonly string _tableName;

        public UserEntityTypeConfiguration(string tableName = "Identity")
        {
            _tableName = tableName;
        }

        public void Configure(EntityTypeBuilder<TUserEntity> builder)
        {
            builder.HasKey(_ => _.Id);
            builder.HasPartitionKey(_ => _.Id);
            builder.Property(_ => _.ConcurrencyStamp).IsETagConcurrency();

            builder.ToContainer(_tableName);
        }
    }
}