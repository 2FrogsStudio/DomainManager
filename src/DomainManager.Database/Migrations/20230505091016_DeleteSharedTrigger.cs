using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DomainManager.Migrations
{
    /// <inheritdoc />
    public partial class DeleteSharedTrigger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.Sql("DELETE FROM qrtz_triggers\n" +
                                 "WHERE trigger_name = 'Recurring.Trigger.DomainManager.Jobs.UpdateAndNotifyJobSystemSchedule'\n" +
                                 "  AND trigger_group = 'DomainManager.Bussines'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
