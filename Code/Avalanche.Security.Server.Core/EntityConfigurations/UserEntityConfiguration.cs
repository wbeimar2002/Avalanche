using Avalanche.Security.Server.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Avalanche.Security.Server.Core.EntityConfigurations
{
    public class UserEntityConfiguration : IEntityTypeConfiguration<UserEntity>
    {
        public void Configure(EntityTypeBuilder<UserEntity> builder)
        {
            _ = builder.ToTable("Users");
            _ = builder.HasKey(x => x.Id);

            // Properties
            _ = builder.Property(n => n.Id)
                .ValueGeneratedOnAdd();

            _ = builder.Property(x => x.FirstName)
                .HasMaxLength(64)
                .IsUnicode()
                .IsRequired()
                .HasColumnType("TEXT COLLATE NOCASE");

            _ = builder.Property(x => x.LastName)
                .HasMaxLength(64)
                .IsUnicode()
                .IsRequired()
                .HasColumnType("TEXT COLLATE NOCASE");

            _ = builder.Property(x => x.UserName)
                .HasMaxLength(64)
                .IsUnicode()
                .IsRequired()
                .HasColumnType("TEXT COLLATE NOCASE");

            _ = builder.Property(x => x.Password)
                .HasMaxLength(64)
                .IsUnicode()
                .IsRequired()
                .HasColumnType("TEXT COLLATE NOCASE");

            // Indexes
            _ = builder.HasIndex(n => n.UserName)
                .IsUnique(true);
        }
    }
}
