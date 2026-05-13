using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HouseholdServices.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedLookupData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "order_statuses",
                columns: new[] { "order_status_id", "name" },
                values: new object[,]
                {
                    { 1, "created" },
                    { 2, "in_progress" },
                    { 3, "completed" },
                    { 4, "cancelled" }
                });

            migrationBuilder.InsertData(
                table: "request_statuses",
                columns: new[] { "request_status_id", "name" },
                values: new object[,]
                {
                    { 1, "open" },
                    { 2, "in_progress" },
                    { 3, "completed" },
                    { 4, "cancelled" }
                });

            migrationBuilder.InsertData(
                table: "response_statuses",
                columns: new[] { "response_status_id", "name" },
                values: new object[,]
                {
                    { 1, "pending" },
                    { 2, "accepted" },
                    { 3, "rejected" }
                });

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "role_id", "name" },
                values: new object[,]
                {
                    { 1, "client" },
                    { 2, "master" }
                });

            migrationBuilder.InsertData(
                table: "service_categories",
                columns: new[] { "category_id", "description", "name" },
                values: new object[,]
                {
                    { 1, "Plumbing installation, repairs, and maintenance.", "plumbing" },
                    { 2, "Electrical installation, diagnostics, and repairs.", "electrical" },
                    { 3, "Regular, deep, and post-renovation cleaning.", "cleaning" },
                    { 4, "Diagnostics and repair of household appliances.", "appliance_repair" },
                    { 5, "Furniture assembly, installation, and minor repairs.", "furniture_assembly" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "order_statuses",
                keyColumn: "order_status_id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "order_statuses",
                keyColumn: "order_status_id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "order_statuses",
                keyColumn: "order_status_id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "order_statuses",
                keyColumn: "order_status_id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "request_statuses",
                keyColumn: "request_status_id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "request_statuses",
                keyColumn: "request_status_id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "request_statuses",
                keyColumn: "request_status_id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "request_statuses",
                keyColumn: "request_status_id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "response_statuses",
                keyColumn: "response_status_id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "response_statuses",
                keyColumn: "response_status_id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "response_statuses",
                keyColumn: "response_status_id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "role_id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "role_id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "service_categories",
                keyColumn: "category_id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "service_categories",
                keyColumn: "category_id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "service_categories",
                keyColumn: "category_id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "service_categories",
                keyColumn: "category_id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "service_categories",
                keyColumn: "category_id",
                keyValue: 5);
        }
    }
}
