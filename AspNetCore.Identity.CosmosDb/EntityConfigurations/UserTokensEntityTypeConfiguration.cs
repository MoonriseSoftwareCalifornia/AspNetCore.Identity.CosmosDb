using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AspNetCore.Identity.CosmosDb.EntityConfigurations
{
    public class UserTokensEntityTypeConfiguration : IEntityTypeConfiguration<IdentityUserToken<string>>
    {
        private readonly string _container;

        public UserTokensEntityTypeConfiguration(string container = "Identity")
        {
            _container = container;
        }

        public void Configure(EntityTypeBuilder<IdentityUserToken<string>> builder)
        {
            builder
                .UseETagConcurrency()
                .HasPartitionKey(_ => _.UserId);

            builder.ToContainer(_container);
        }
    }
}