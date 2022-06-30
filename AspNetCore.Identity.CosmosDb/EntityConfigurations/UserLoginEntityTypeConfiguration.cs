using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AspNetCore.Identity.CosmosDb.EntityConfigurations
{
    public class UserLoginEntityTypeConfiguration : IEntityTypeConfiguration<IdentityUserLogin<string>>
    {
        private readonly string _container;

        public UserLoginEntityTypeConfiguration(string container = "Identity")
        {
            _container = container;
        }

        public void Configure(EntityTypeBuilder<IdentityUserLogin<string>> builder)
        {
            builder
                .UseETagConcurrency()
                .HasPartitionKey(_ => _.ProviderKey);
            
            builder.ToContainer(_container);
        }
    }
}