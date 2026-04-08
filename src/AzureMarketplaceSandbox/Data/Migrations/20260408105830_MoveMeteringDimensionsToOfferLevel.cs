using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AzureMarketplaceSandbox.Data.Migrations
{
    /// <inheritdoc />
    public partial class MoveMeteringDimensionsToOfferLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MeteringDimensions_Plans_PlanId",
                table: "MeteringDimensions");

            migrationBuilder.DropIndex(
                name: "IX_MeteringDimensions_PlanId",
                table: "MeteringDimensions");

            migrationBuilder.DropColumn(
                name: "PlanId",
                table: "MeteringDimensions");

            migrationBuilder.RenameColumn(
                name: "Currency",
                table: "MeteringDimensions",
                newName: "OfferId");

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Offers",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "PlanMeteringDimensions",
                columns: table => new
                {
                    PlanId = table.Column<int>(type: "INTEGER", nullable: false),
                    MeteringDimensionId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanMeteringDimensions", x => new { x.PlanId, x.MeteringDimensionId });
                    table.ForeignKey(
                        name: "FK_PlanMeteringDimensions_MeteringDimensions_MeteringDimensionId",
                        column: x => x.MeteringDimensionId,
                        principalTable: "MeteringDimensions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlanMeteringDimensions_Plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "Plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MeteringDimensions_OfferId",
                table: "MeteringDimensions",
                column: "OfferId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanMeteringDimensions_MeteringDimensionId",
                table: "PlanMeteringDimensions",
                column: "MeteringDimensionId");

            migrationBuilder.AddForeignKey(
                name: "FK_MeteringDimensions_Offers_OfferId",
                table: "MeteringDimensions",
                column: "OfferId",
                principalTable: "Offers",
                principalColumn: "OfferId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MeteringDimensions_Offers_OfferId",
                table: "MeteringDimensions");

            migrationBuilder.DropTable(
                name: "PlanMeteringDimensions");

            migrationBuilder.DropIndex(
                name: "IX_MeteringDimensions_OfferId",
                table: "MeteringDimensions");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Offers");

            migrationBuilder.RenameColumn(
                name: "OfferId",
                table: "MeteringDimensions",
                newName: "Currency");

            migrationBuilder.AddColumn<int>(
                name: "PlanId",
                table: "MeteringDimensions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_MeteringDimensions_PlanId",
                table: "MeteringDimensions",
                column: "PlanId");

            migrationBuilder.AddForeignKey(
                name: "FK_MeteringDimensions_Plans_PlanId",
                table: "MeteringDimensions",
                column: "PlanId",
                principalTable: "Plans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
