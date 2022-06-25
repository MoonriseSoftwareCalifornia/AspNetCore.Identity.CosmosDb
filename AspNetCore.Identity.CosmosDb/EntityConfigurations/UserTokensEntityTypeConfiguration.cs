using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PieroDeTomi.EntityFrameworkCore.Identity.Cosmos.EntityConfigurations
{
    public class UserTokensEntityTypeConfiguration : IEntityTypeConfiguration<IdentityUserToken<string>>
    {
        private readonly string _tableName;

        public UserTokensEntityTypeConfiguration(string tableName = "Identity_Tokens")
        {
            _tableName = tableName;
        }

        public void Configure(EntityTypeBuilder<IdentityUserToken<string>> builder)
        {
            builder
                .UseETagConcurrency()
                .HasPartitionKey(_ => _.UserId);

            builder.ToContainer(_tableName);
        }
    }
}