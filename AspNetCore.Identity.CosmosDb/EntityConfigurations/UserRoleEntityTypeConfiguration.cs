using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AspNetCore.Identity.CosmosDb.EntityConfigurations
{
    public class UserRoleEntityTypeConfiguration : IEntityTypeConfiguration<IdentityUserRole<string>>
    {
        private readonly string _container;

        public UserRoleEntityTypeConfiguration(string container = "Identity")
        {
            _container = container;
        }

        public void Configure(EntityTypeBuilder<IdentityUserRole<string>> builder)
        {
            builder
                .UseETagConcurrency()
                .HasPartitionKey(_ => _.UserId);

            builder.ToContainer(_container);
        }
    }
}