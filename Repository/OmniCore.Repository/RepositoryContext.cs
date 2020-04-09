﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Innofactor.EfCoreJsonValueConverter;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OmniCore.Model.Entities;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Internal;

namespace OmniCore.Repository
{
    public class RepositoryContext : DbContext, IRepositoryContextReadWrite
    {
        private readonly string ConnectionString;
        private IDisposable ReaderWriterLock;
        private ICoreLoggingFunctions Logging;

        // for migrations tool
        public RepositoryContext()
        {
            ConnectionString = "Data Source=:memory:";
        }

        public RepositoryContext(
            ICoreApplicationFunctions applicationFunctions,
            ICoreLoggingFunctions logging)
        {
            var path = Path.Combine(applicationFunctions.DataPath, "oc.db3");
            ConnectionString = $"Data Source={path}";
        }

        public DbSet<MedicationEntity> Medications { get; set; }
        public DbSet<UserEntity> Users { get; set; }
        public DbSet<RadioEntity> Radios { get; set; }
        public DbSet<RadioEventEntity> RadioEvents { get; set; }
        public DbSet<PodEntity> Pods { get; set; }
        public DbSet<PodRequestEntity> PodRequests { get; set; }

        public DbSet<MedicationDeliveryEntity> MedicationDeliveries { get; set; }

        public DbSet<PodResponseEntity> PodResponses { get; set; }

        public async Task InitializeDatabase(CancellationToken cancellationToken, bool createNew = false)
        {
            Logging.Debug("Initializing database");
            if (createNew)
            {
                Logging.Debug("Deleting existing database");
                Database.EnsureDeleted();
            }
            Logging.Debug("Migrating database structure");
            await Database.MigrateAsync(cancellationToken);
            Logging.Debug("Seeding default data");
            await SeedData();
            Logging.Debug("Database initialization complete.");
        }

        public override void Dispose()
        {
            base.Dispose();
            ReaderWriterLock?.Dispose();
            ReaderWriterLock = null;
        }

        public IRepositoryContextReadWrite WithExisting(params IEntity[] entities)
        {
            foreach(var entity in entities)
                Attach(entity).State = EntityState.Unchanged;
            return this;
        }

        public IRepositoryContextReadWrite WithExisting<T>(ICollection<T> entities)
            where T : IEntity
        {
            foreach (var entity in entities)
                Attach(entity).State = EntityState.Unchanged;
            return this;
        }

        public async Task Save(CancellationToken cancellationToken)
        {
            try
            {
                await SaveChangesAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                Logging.Warning("Save operation canceled");
                throw;
            }
            catch (Exception e)
            {
                throw new OmniCoreRepositoryException(FailureType.RepositoryGeneralError, "Error while saving", e);
            }
        }

        public void SetLock(IDisposable readerWriterLock, bool tracking)
        {
            ReaderWriterLock = readerWriterLock;
            ChangeTracker.QueryTrackingBehavior =
                tracking ? QueryTrackingBehavior.TrackAll : QueryTrackingBehavior.NoTracking;
        }

        private async Task SeedData()
        {
            try
            {
                if (!Medications.Any())
                    Medications.Add(new MedicationEntity
                    {
                        Hormone = HormoneType.Unknown,
                        Name = "Unspecified",
                        UnitName = "microLiters",
                        UnitNameShort = "µL",
                        UnitsPerMilliliter = 1000
                    });

                if (!Users.Any())
                    Users.Add(new UserEntity
                    {
                        Name = "Default User"
                    });
                await SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw new OmniCoreRepositoryException(FailureType.RepositoryGeneralError, "Error while initializing default data", e);
            }
        }

        //public static readonly ILoggerFactory DebugLoggerFactory
        //    = LoggerFactory.Create(builder => { builder.AddDebug() ; });

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(ConnectionString);
#if DEBUG
            optionsBuilder.EnableDetailedErrors();
            //optionsBuilder.UseLoggerFactory(DebugLoggerFactory);
#endif
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.AddJsonFields();
        }
    }
}