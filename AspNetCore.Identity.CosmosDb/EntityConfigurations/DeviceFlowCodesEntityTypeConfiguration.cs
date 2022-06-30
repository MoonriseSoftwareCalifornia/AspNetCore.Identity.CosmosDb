using Duende.IdentityServer.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AspNetCore.Identity.CosmosDb.EntityConfigurations
{
    public class DeviceFlowCodesEntityTypeConfiguration : IEntityTypeConfiguration<DeviceFlowCodes>
    {
        private readonly string _container;

        public DeviceFlowCodesEntityTypeConfiguration(string container = "Identity")
        {
            _container = container;
        }

        public void Configure(EntityTypeBuilder<DeviceFlowCodes> builder)
        {
            builder
                .HasKey(_ => new { _.ClientId, _.SessionId, _.DeviceCode });

            builder
                .UseETagConcurrency()
                .HasPartitionKey(_ => _.SessionId);

            builder.ToContainer(_container);
        }
    }
}