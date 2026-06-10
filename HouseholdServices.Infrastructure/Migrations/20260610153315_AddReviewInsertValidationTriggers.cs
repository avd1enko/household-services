using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HouseholdServices.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewInsertValidationTriggers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                                 CREATE OR REPLACE FUNCTION prevent_duplicate_review_per_order()
                                 RETURNS trigger
                                 LANGUAGE plpgsql
                                 AS $$
                                 BEGIN
                                    IF EXISTS (
                                        SELECT 1
                                        FROM reviews
                                        WHERE order_id = NEW.order_id
                                    )
                                    THEN
                                        RAISE EXCEPTION 'Review for order % already exists.', NEW.order_id
                                            USING ERRCODE = 'unique_violation';
                                    END IF;

                                    RETURN NEW;
                                 END;
                                 $$;
                                 """);

            migrationBuilder.Sql("""
                                 CREATE TRIGGER trg_reviews_prevent_duplicate_per_order
                                 BEFORE INSERT ON reviews
                                 FOR EACH ROW
                                 EXECUTE FUNCTION prevent_duplicate_review_per_order();
                                 """);

            migrationBuilder.Sql("""
                                 CREATE OR REPLACE FUNCTION ensure_review_order_completed()
                                 RETURNS trigger
                                 LANGUAGE plpgsql
                                 AS $$
                                 DECLARE
                                    order_status_name text;
                                 BEGIN
                                    SELECT os.name
                                    INTO order_status_name
                                    FROM orders o
                                    JOIN order_statuses os ON os.order_status_id = o.order_status_id
                                    WHERE o.order_id = NEW.order_id;

                                    IF order_status_name IS NULL
                                    THEN
                                        RAISE EXCEPTION 'Order % does not exist.', NEW.order_id
                                            USING ERRCODE = 'foreign_key_violation';
                                    END IF;

                                    IF order_status_name <> 'completed'
                                    THEN
                                        RAISE EXCEPTION 'Review can be created only for completed order %. Current status is %.', NEW.order_id, order_status_name
                                            USING ERRCODE = 'check_violation';
                                    END IF;

                                    RETURN NEW;
                                 END;
                                 $$;
                                 """);

            migrationBuilder.Sql("""
                                 CREATE TRIGGER trg_reviews_require_completed_order
                                 BEFORE INSERT ON reviews
                                 FOR EACH ROW
                                 EXECUTE FUNCTION ensure_review_order_completed();
                                 """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                                 DROP TRIGGER IF EXISTS trg_reviews_require_completed_order ON reviews;
                                 """);

            migrationBuilder.Sql("""
                                 DROP FUNCTION IF EXISTS ensure_review_order_completed();
                                 """);

            migrationBuilder.Sql("""
                                 DROP TRIGGER IF EXISTS trg_reviews_prevent_duplicate_per_order ON reviews;
                                 """);

            migrationBuilder.Sql("""
                                 DROP FUNCTION IF EXISTS prevent_duplicate_review_per_order();
                                 """);
        }
    }
}
