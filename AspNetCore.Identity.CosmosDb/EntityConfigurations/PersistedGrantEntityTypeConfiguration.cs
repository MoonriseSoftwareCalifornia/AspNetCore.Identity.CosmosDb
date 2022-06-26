
using Duende.IdentityServer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AspNetCore.Identity.CosmosDb.EntityConfigurations
{
    public class PersistedGrantEntityTypeConfiguration : IEntityTypeConfiguration<PersistedGrant>
    {
        private readonly string _tableName;

        public PersistedGrantEntityTypeConfiguration(string tableName = "Identity_PersistedGrant")
        {
            _tableName = tableName;
        }

        public void Configure(EntityTypeBuilder<PersistedGrant> builder)
        {
            builder.HasKey(_ => new { _.Type, _.ClientId, _.SessionId });

            builder
                .UseETagConcurrency()
                .HasPartitionKey(_ => _.Key);

            builder.ToContainer(_tableName);
        }
    }
}
