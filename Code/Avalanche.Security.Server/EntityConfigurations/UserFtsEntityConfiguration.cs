using Avalanche.Security.Server.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Avalanche.Security.Server.EntityConfigurations
{
    public class UserFtsEntityConfiguration : IEntityTypeConfiguration<UserFtsEntity>
    {
        private const string FtsTableName = "UserFts";
        public void Configure(EntityTypeBuilder<UserFtsEntity> builder)
        {
            _ = builder.ToView(FtsTableName);
            _ = builder.HasKey(d => d.RowId);

            // Properties
            //For Sqlite FTS the Match column has to be the same name as the underlying table
            _ = builder.Property(p => p.Match).HasColumnName(FtsTableName);

            // Relationships
            _ = builder.HasOne(x => x.User)
            .WithOne()
            .HasForeignKey<UserFtsEntity>(fts => fts.RowId);
        }
    }
}
