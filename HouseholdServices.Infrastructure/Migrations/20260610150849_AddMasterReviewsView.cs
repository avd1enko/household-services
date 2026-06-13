using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HouseholdServices.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMasterReviewsView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                                 CREATE OR REPLACE VIEW master_reviews_view AS
                                 SELECT
                                    rev.review_id,
                                    rev.order_id,
                                    rev.rating,
                                    rev.comment,
                                    rev.created_at AS review_created_at,
                                    o.completed_at AS order_completed_at,

                                    req.request_id,
                                    req.title AS request_title,
                                    req.category_id,
                                    sc.name AS category_name,

                                    req.client_id,
                                    client.first_name AS client_first_name,
                                    client.last_name AS client_last_name,

                                    r.master_id,
                                    master.first_name AS master_first_name,
                                    master.last_name AS master_last_name

                                 FROM reviews rev
                                 JOIN orders o ON o.order_id = rev.order_id
                                 JOIN responses r ON r.response_id = o.response_id
                                 JOIN requests req ON req.request_id = r.request_id
                                 JOIN service_categories sc ON sc.category_id = req.category_id
                                 JOIN users client ON client.user_id = req.client_id
                                 JOIN users master ON master.user_id = r.master_id;
                                 """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                                 DROP VIEW IF EXISTS master_reviews_view;
                                 """);
        }
    }
}
