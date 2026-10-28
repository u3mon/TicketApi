using Application.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x=> x.Id);

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(250);
    }
}