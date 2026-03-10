using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using GestionProduccion.Data;

#nullable disable

namespace GestionProduccion.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260308170000_AddAuditToOperationalTasks")]
    partial class AddAuditToOperationalTasks
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
        }
    }
}
