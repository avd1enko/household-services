using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HouseholdServices.Infrastructure.Migrations
{
    public partial class RemoveCreatedOrderStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                                 DELETE FROM order_statuses;
                                 INSERT INTO order_statuses (order_status_id, name) VALUES (1, 'in_progress');
                                 INSERT INTO order_statuses (order_status_id, name) VALUES (2, 'completed');
                                 INSERT INTO order_statuses (order_status_id, name) VALUES (3, 'cancelled');
                                 """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                                 DELETE FROM order_statuses;

                                 INSERT INTO order_statuses (order_status_id, name) VALUES (1, 'created');
                                 INSERT INTO order_statuses (order_status_id, name) VALUES (2, 'in_progress');
                                 INSERT INTO order_statuses (order_status_id, name) VALUES (3, 'completed');
                                 INSERT INTO order_statuses (order_status_id, name) VALUES (4, 'cancelled');
                                 """);
        }
    }
}