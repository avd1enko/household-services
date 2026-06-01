using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HouseholdServices.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrdersView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                                 CREATE OR REPLACE VIEW order_details_view AS
                                 SELECT
                                    o.order_id,
                                    os.name AS status,
                                    o.price,
                                    o.initial_meeting_at,
                                    o.created_at,
                                    o.completed_at,
                                    
                                    req.request_id,
                                    req.title AS request_title,
                                    req.description AS request_description,
                                    req.address AS request_address,
                                    req.desired_date,
                                    req.category_id,
                                    req.client_id,
                                    
                                    sc.name AS category_name,
                                    
                                    client.first_name AS client_first_name,
                                    client.last_name AS client_last_name,
                                    client.phone AS client_phone,
                                    
                                    r.master_id,
                                    master.first_name AS master_first_name,
                                    master.last_name AS master_last_name,
                                    master.phone AS master_phone
                                    
                                    FROM orders o
                                    JOIN order_statuses os ON o.order_status_id = os.order_status_id
                                    JOIN responses r ON r.response_id = o.response_id
                                    JOIN requests req ON req.request_id = r.request_id
                                    JOIN service_categories sc ON req.category_id = sc.category_id
                                    JOIN users client ON req.client_id = client.user_id
                                    JOIN users master ON r.master_id = master.user_id
                                    
                                 """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                                 DROP VIEW IF EXISTS order_details_view;
                                 """);
        }
    }
}
