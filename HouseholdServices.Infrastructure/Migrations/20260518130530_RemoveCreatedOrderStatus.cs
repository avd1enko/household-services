using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HouseholdServices.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCreatedOrderStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "order_statuses",
                keyColumn: "order_status_id",
                keyValue: 4);

            migrationBuilder.UpdateData(
                table: "order_statuses",
                keyColumn: "order_status_id",
                keyValue: 1,
                column: "name",
                value: "in_progress");

            migrationBuilder.UpdateData(
                table: "order_statuses",
                keyColumn: "order_status_id",
                keyValue: 2,
                column: "name",
                value: "completed");

            migrationBuilder.UpdateData(
                table: "order_statuses",
                keyColumn: "order_status_id",
                keyValue: 3,
                column: "name",
                value: "cancelled");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "order_statuses",
                keyColumn: "order_status_id",
                keyValue: 1,
                column: "name",
                value: "created");

            migrationBuilder.UpdateData(
                table: "order_statuses",
                keyColumn: "order_status_id",
                keyValue: 2,
                column: "name",
                value: "in_progress");

            migrationBuilder.UpdateData(
                table: "order_statuses",
                keyColumn: "order_status_id",
                keyValue: 3,
                column: "name",
                value: "completed");

            migrationBuilder.InsertData(
                table: "order_statuses",
                columns: new[] { "order_status_id", "name" },
                values: new object[] { 4, "cancelled" });
        }
    }
}
