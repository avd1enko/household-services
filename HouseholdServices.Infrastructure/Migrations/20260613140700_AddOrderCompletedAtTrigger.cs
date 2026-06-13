using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HouseholdServices.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderCompletedAtTrigger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                                 CREATE OR REPLACE FUNCTION set_order_completed_at_when_completed()
                                 RETURNS TRIGGER AS $$
                                 DECLARE
                                     completed_status_id INT;
                                 BEGIN
                                     SELECT order_status_id
                                     INTO completed_status_id
                                     FROM order_statuses
                                     WHERE name = 'completed';

                                     IF NEW.order_status_id = completed_status_id THEN
                                         NEW.completed_at = NOW();
                                     END IF;

                                     RETURN NEW;
                                 END;
                                 $$ LANGUAGE plpgsql;
                                 """);

            migrationBuilder.Sql("""
                                 CREATE TRIGGER trg_set_order_completed_at_when_completed
                                 BEFORE UPDATE OF order_status_id ON orders
                                 FOR EACH ROW
                                 WHEN (OLD.order_status_id IS DISTINCT FROM NEW.order_status_id)
                                 EXECUTE FUNCTION set_order_completed_at_when_completed();
                                 """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                                 DROP TRIGGER IF EXISTS trg_set_order_completed_at_when_completed ON orders;
                                 """);

            migrationBuilder.Sql("""
                                 DROP FUNCTION IF EXISTS set_order_completed_at_when_completed();
                                 """);
        }
    }
}
