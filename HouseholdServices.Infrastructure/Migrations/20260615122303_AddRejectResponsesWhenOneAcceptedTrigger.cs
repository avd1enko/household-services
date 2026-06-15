using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HouseholdServices.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRejectResponsesWhenOneAcceptedTrigger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                                 CREATE OR REPLACE FUNCTION reject_responses_when_one_accepted()
                                 RETURNS TRIGGER 
                                 AS $$
                                 
                                 DECLARE
                                    accepted_status_id INT;
                                    rejected_status_id INT;
                                    pending_status_id INT;
                                 BEGIN
                                 SELECT response_status_id
                                 INTO accepted_status_id
                                 FROM response_statuses
                                 WHERE name = 'accepted';
                                 
                                 SELECT response_status_id
                                 INTO rejected_status_id
                                 FROM response_statuses
                                 WHERE name = 'rejected';
                                 
                                 SELECT response_status_id
                                 INTO pending_status_id
                                 FROM response_statuses
                                 WHERE name = 'pending';
                                 
                                 IF NEW.response_status_id = accepted_status_id THEN
                                    UPDATE responses
                                    SET response_status_id = rejected_status_id
                                    WHERE request_id = NEW.request_id
                                    AND response_id <> NEW.response_id
                                    AND response_status_id = pending_status_id;
                                 END IF;
                                 RETURN NEW;
                                 END;
                                 $$ LANGUAGE plpgsql;
                                 """

            );
            migrationBuilder.Sql(
                """
                CREATE TRIGGER trg_reject_responses_when_one_accepted
                AFTER UPDATE OF response_status_id ON responses
                FOR EACH ROW
                WHEN (OLD.response_status_id IS DISTINCT FROM NEW.response_status_id)
                EXECUTE FUNCTION reject_responses_when_one_accepted();
                """);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                                 DROP TRIGGER IF EXISTS trg_reject_responses_when_one_accepted ON responses;
                                 """);

            migrationBuilder.Sql("""
                                 DROP FUNCTION IF EXISTS reject_responses_when_one_accepted();
                                 """);
        }
    }
}
