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

#pragma warning disable S125 // Sections of code should not be commented out
            // dotnet/efcore#35264
            //b.HasIndex(r => r.NormalizedName).HasDatabaseName("RoleNameIndex").IsUnique();
            //b.ToTable("AspNetRoles");
#pragma warning restore S125 // Sections of code should not be commented out
            builder.Property(r => r.ConcurrencyStamp).IsConcurrencyToken();

            builder.Property(u => u.Name).HasMaxLength(256);
            builder.Property(u => u.NormalizedName).HasMaxLength(256);

#pragma warning disable S125 // Sections of code should not be commented out
            // dotnet/efcore#35264
            //b.HasMany<TUserRole>().WithOne().HasForeignKey(ur => ur.RoleId).IsRequired();
            //b.HasMany<TRoleClaim>().WithOne().HasForeignKey(rc => rc.RoleId).IsRequired();
#pragma warning restore S125 // Sections of code should not be commented out

            builder.ToContainer(_tableName);
        }
    }
}