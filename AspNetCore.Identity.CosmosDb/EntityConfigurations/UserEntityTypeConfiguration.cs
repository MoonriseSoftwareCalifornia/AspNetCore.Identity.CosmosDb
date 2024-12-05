using System;
using System.Linq;
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
        private readonly PersonalDataConverter? _dataConverter;

        public UserEntityTypeConfiguration(PersonalDataConverter? dataConverter, string tableName = "Identity")
        {
            _tableName = tableName;
            _dataConverter = dataConverter;
        }

        public void Configure(EntityTypeBuilder<TUserEntity> builder)
        {
            builder.HasKey(_ => _.Id);
            builder.HasPartitionKey(_ => _.Id);
            builder.Property(_ => _.ConcurrencyStamp).IsETagConcurrency();

            builder.Property(u => u.ConcurrencyStamp).IsConcurrencyToken();
            builder.Property(u => u.UserName).HasMaxLength(256);
            builder.Property(u => u.NormalizedUserName).HasMaxLength(256);
            builder.Property(u => u.Email).HasMaxLength(256);
            builder.Property(u => u.NormalizedEmail).HasMaxLength(256);
            builder.Property(u => u.PhoneNumber).HasMaxLength(256);

            if (_dataConverter != null)
            {
                var personalDataProps = typeof(TUserEntity).GetProperties().Where(
                                prop => Attribute.IsDefined(prop, typeof(ProtectedPersonalDataAttribute)));
                foreach (var p in personalDataProps)
                {
                    if (p.PropertyType != typeof(string))
                    {
                        throw new InvalidOperationException("Can only protect strings.");
                    }
                    builder.Property(typeof(string), p.Name).HasConversion(_dataConverter);
                }
            }

#pragma warning disable S125 // Sections of code should not be commented out
            // dotnet/efcore#35264
            //b.HasIndex(u => u.NormalizedUserName).HasDatabaseName("UserNameIndex").IsUnique();
            //b.HasIndex(u => u.NormalizedEmail).HasDatabaseName("EmailIndex");
            //b.HasMany<TUserClaim>().WithOne().HasForeignKey(uc => uc.UserId).IsRequired();
            //b.HasMany<TUserLogin>().WithOne().HasForeignKey(ul => ul.UserId).IsRequired();
            //b.HasMany<TUserToken>().WithOne().HasForeignKey(ut => ut.UserId).IsRequired();
#pragma warning restore S125 // Sections of code should not be commented out

            builder.ToContainer(_tableName);
        }
    }
}