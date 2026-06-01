using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HouseholdServices.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddResponseAcceptedOrderTrigger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                                 CREATE OR REPLACE FUNCTION create_order_after_response_accepted()
                                 RETURNS trigger
                                 LANGUAGE plpgsql
                                 AS $$
                                 DECLARE 
                                    accepted_status_id integer;
                                    in_progress_status_id integer;
                                 BEGIN
                                    SELECT response_status_id
                                    INTO accepted_status_id
                                    FROM response_statuses
                                    WHERE name = 'accepted';
                                    
                                    SELECT order_status_id
                                    INTO in_progress_status_id
                                    FROM order_statuses
                                    WHERE name = 'in_progress';
                                    
                                    IF OLD.response_status_id <> NEW.response_status_id AND NEW.response_status_id = accepted_status_id
                                    THEN
                                        INSERT INTO orders(
                                        response_id,
                                        order_status_id,
                                        price,
                                        initial_meeting_at,
                                        created_at,
                                        completed_at)
                                        
                                        VALUES (
                                        NEW.response_id,
                                        in_progress_status_id,
                                        NEW.proposed_price,
                                        NULL,
                                        NOW(),
                                        NULL);
                                    END IF;
                                    RETURN NEW;
                                    
                                 END;
                                 $$;
                                 """);

            migrationBuilder.Sql("""
                                 CREATE TRIGGER trg_create_order_after_response_accepted
                                 AFTER UPDATE ON responses
                                 FOR EACH ROW
                                 EXECUTE FUNCTION create_order_after_response_accepted();
                                 """);

        }
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                                 DROP TRIGGER IF EXISTS trg_create_order_after_response_accepted ON responses;
                                 """);
            migrationBuilder.Sql("""
                                 DROP FUNCTION IF EXISTS create_order_after_response_accepted();
                                 """);
        }
    }
}
