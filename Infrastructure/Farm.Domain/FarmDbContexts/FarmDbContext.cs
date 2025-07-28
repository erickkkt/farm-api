using Farm.Domain.Attibutes;
using Farm.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Globalization;

namespace Farm.Domain.FarmDbContexts
{
    public class FarmDbContext : DbContext
    {
        public FarmDbContext(DbContextOptions<FarmDbContext> options) : base(options)
        {
        }

        public DbSet<Animal> Animals { get; set; }
        public DbSet<Cage> Cages { get; set; }
        public DbSet<Entities.Farm> Farms { get; set; }
        public DbSet<Audit> Audits { get; set; }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Animal>().ToTable("Animal");
            modelBuilder.Entity<Cage>().ToTable("Cage");
            modelBuilder.Entity<Entities.Farm>().ToTable("Farm");
            modelBuilder.Entity<Audit>().ToTable("Audit");
            modelBuilder.Entity<User>().ToTable("User");
            modelBuilder.Entity<Role>().ToTable("Role");


            // create indexes for tables
            modelBuilder.Entity<Animal>()
                .HasIndex(p => new { p.Name, p.Code })
                .IsUnique(true);
            modelBuilder.Entity<Cage>()
                .HasIndex(a => new { a.Name })
                .IsUnique(true);
            modelBuilder.Entity<Audit>()
                .HasIndex(a => new { a.EntityName, a.PrimaryKeyValue });

            // default role data
            modelBuilder.Entity<Role>()
                .HasData(new Role()
                {
                    Id = new Guid("68DFFDA0-B650-44AC-A599-6CDBDFD641E4"),
                    Name = "HO.SYSADMIN",
                    Description = "System Administrator",
                    IsActive = true
                });
        }


        public override int SaveChanges()
        {
            KeepTrackChanges();

            return base.SaveChanges();
        }

        public async override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            KeepTrackChanges();

            return await base.SaveChangesAsync(cancellationToken);
        }

        private void KeepTrackChanges()
        {
            ChangeTracker.DetectChanges();
            var modifiedEntityEntries = ChangeTracker.Entries().Where(p => p.State == EntityState.Modified).ToList();
            var time = DateTime.UtcNow;

            foreach (var modifiedEntityEntry in modifiedEntityEntries)
            {
                var modifiedEntityEntryType = modifiedEntityEntry.Entity.GetType();

                var trackEntityChangesAttribute = modifiedEntityEntryType
                                                .GetCustomAttributes(false)
                                                .OfType<TrackAuditAttribute>()
                                                .SingleOrDefault();

                if (trackEntityChangesAttribute == null)
                {
                    continue;
                }

                var changedByUserId = modifiedEntityEntry.CurrentValues["ChangedByUserId"]?.ToString();
                if (!Guid.TryParse(changedByUserId, out var changedByUserIdAsGuid))
                {
                    throw new Exception("ChangedByUserId is not a valid GUID");
                }

                var changedByUserName = modifiedEntityEntry.CurrentValues["ChangedByUserName"]?.ToString() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(changedByUserName))
                {
                    throw new Exception("ChangedByUserName must be supplied");
                }

                //var tableName = modifiedEntityEntry.Metadata.Relational().TableName;
                var tableName = modifiedEntityEntry.Metadata.GetDefaultTableName();

                object primaryKey = null;

                modifiedEntityEntry.OriginalValues.SetValues(modifiedEntityEntry.GetDatabaseValues());

                foreach (var property in modifiedEntityEntry.Properties)
                {
                    var propertyEntityEntryType = modifiedEntityEntryType.GetProperty(property.Metadata.Name);

                    var dontTrackPropertyChangeAttribute = propertyEntityEntryType
                        ?.GetCustomAttributes(false)
                        .OfType<NoTrackAuditAttribute>()
                        .SingleOrDefault();

                    if (dontTrackPropertyChangeAttribute != null)
                    {
                        continue;
                    }

                    if (property.Metadata.IsPrimaryKey())
                    {
                        primaryKey = property.CurrentValue;
                        continue;
                    }

                    if (property.IsModified)
                    {
                        string originalValue;
                        string currentValue;
                        var propertyType = property.Metadata.PropertyInfo?.PropertyType;

                        if (propertyType == typeof(DateTime) ||
                            propertyType == typeof(DateTime?))
                        {
                            originalValue = property.OriginalValue?.ToString();
                            if (!string.IsNullOrWhiteSpace(originalValue))
                            {
                                if (DateTime.TryParseExact(
                                                            originalValue,
                                                            "MM/dd/yyyy HH:mm:ss",
                                                            CultureInfo.InvariantCulture,
                                                            DateTimeStyles.None,
                                                            out var intermediateDate))
                                {
                                    originalValue = intermediateDate.ToString("yyyy-MM-dd HH:mm:ss");
                                }
                            }

                            currentValue = property.CurrentValue?.ToString();
                            if (!string.IsNullOrWhiteSpace(currentValue))
                            {
                                if (DateTime.TryParseExact(
                                                            currentValue,
                                                            "MM/dd/yyyy HH:mm:ss",
                                                            CultureInfo.InvariantCulture,
                                                            DateTimeStyles.None,
                                                            out var intermediateDate))
                                {
                                    currentValue = intermediateDate.ToString("yyyy-MM-dd HH:mm:ss");
                                }
                            }
                        }
                        else
                        {
                            originalValue = property.OriginalValue?.ToString();
                            currentValue = property.CurrentValue?.ToString();
                        }

                        var decimalField = false;
                        var decimalsDiffer = false;
                        if (propertyType == typeof(decimal) ||
                            propertyType == typeof(decimal?))
                        {
                            decimalField = true;

                            if (property.CurrentValue != null &&
                                property.OriginalValue != null &&
                                decimal.Compare(decimal.Parse(originalValue), decimal.Parse(currentValue)) != 0)
                            {
                                decimalsDiffer = true;
                            }
                        }

                        if (decimalField && decimalsDiffer || !decimalField && originalValue != currentValue)
                        {
                            var audit = new Audit
                            {
                                EntityName = tableName,
                                PrimaryKeyValue = new Guid(primaryKey?.ToString()),
                                PropertyName = property.Metadata.Name,
                                OriginalValue = originalValue,
                                CurrentValue = currentValue,
                                ChangedByUserId = changedByUserIdAsGuid,
                                ChangedByUserName = changedByUserName,
                                ChangedDateUtc = time
                            };

                            Audits.Add(audit);
                        }
                    }
                }
            }
        }
    }
}
