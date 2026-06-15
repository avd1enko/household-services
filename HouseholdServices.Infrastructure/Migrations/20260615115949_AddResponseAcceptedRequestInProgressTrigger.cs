using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HouseholdServices.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddResponseAcceptedRequestInProgressTrigger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                                 CREATE OR REPLACE FUNCTION set_request_in_progress_when_response_accepted()
                                 RETURNS TRIGGER AS $$
                                 DECLARE
                                     accepted_status_id INT;
                                     in_progress_status_id INT;
                                 BEGIN
                                     SELECT response_status_id
                                     INTO accepted_status_id
                                     FROM response_statuses
                                     WHERE name = 'accepted';

                                     SELECT request_status_id
                                     INTO in_progress_status_id
                                     FROM request_statuses
                                     WHERE name = 'in_progress';

                                     IF NEW.response_status_id = accepted_status_id THEN
                                         UPDATE requests
                                         SET request_status_id = in_progress_status_id
                                         WHERE request_id = NEW.request_id;
                                     END IF;

                                     RETURN NEW;
                                 END;
                                 $$ LANGUAGE plpgsql;
                                 """);

            migrationBuilder.Sql("""
                                 CREATE TRIGGER trg_set_request_in_progress_when_response_accepted
                                 AFTER UPDATE OF response_status_id ON responses
                                 FOR EACH ROW
                                 WHEN (OLD.response_status_id IS DISTINCT FROM NEW.response_status_id)
                                 EXECUTE FUNCTION set_request_in_progress_when_response_accepted();
                                 """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                                 DROP TRIGGER IF EXISTS trg_set_request_in_progress_when_response_accepted ON responses;
                                 """);
            migrationBuilder.Sql("""
                                 DROP FUNCTION IF EXISTS set_request_in_progress_when_response_accepted();
                                 """);
        }
    }
}