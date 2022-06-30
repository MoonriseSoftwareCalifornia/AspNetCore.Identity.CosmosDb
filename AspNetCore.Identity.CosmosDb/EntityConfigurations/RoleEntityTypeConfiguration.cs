using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AspNetCore.Identity.CosmosDb.EntityConfigurations
{
    public class RoleEntityTypeConfiguration : IEntityTypeConfiguration<IdentityRole>
    {
        private readonly string _container;

        public RoleEntityTypeConfiguration(string container = "Identity")
        {
            _container = container;
        }

        public void Configure(EntityTypeBuilder<IdentityRole> builder)
        {
            builder.HasKey(_ => _.Id);
            builder.HasPartitionKey(_ => _.Id);
            builder.Property(_ => _.ConcurrencyStamp).IsETagConcurrency();

            builder.ToContainer(_container);
        }
    }
}