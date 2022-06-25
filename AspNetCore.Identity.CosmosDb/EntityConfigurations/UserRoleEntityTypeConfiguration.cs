using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PieroDeTomi.EntityFrameworkCore.Identity.Cosmos.EntityConfigurations
{
    public class UserRoleEntityTypeConfiguration : IEntityTypeConfiguration<IdentityUserRole<string>>
    {
        private readonly string _tableName;

        public UserRoleEntityTypeConfiguration(string tableName = "Identity_UserRoles")
        {
            _tableName = tableName;
        }

        public void Configure(EntityTypeBuilder<IdentityUserRole<string>> builder)
        {
            builder
                .UseETagConcurrency()
                .HasPartitionKey(_ => _.UserId);

            builder.ToContainer(_tableName);
        }
    }
}