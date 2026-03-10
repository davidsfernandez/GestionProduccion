using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using GestionProduccion.Data;

#nullable disable

namespace GestionProduccion.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260308160000_AddEffectiveMinutesToOrder")]
    partial class AddEffectiveMinutesToOrder
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            // This is a simplified designer file, but EF Core only needs the partial class 
            // association to recognize the migration.
        }
    }
}
