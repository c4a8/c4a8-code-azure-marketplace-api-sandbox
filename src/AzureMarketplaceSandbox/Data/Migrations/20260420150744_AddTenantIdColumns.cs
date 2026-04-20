using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AzureMarketplaceSandbox.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantIdColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Offers_OfferId",
                table: "Offers");

            migrationBuilder.DropIndex(
                name: "IX_MarketplaceTokens_Token",
                table: "MarketplaceTokens");

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "WebhookDeliveryLogs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "UsageEvents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Subscriptions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Plans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "PlanMeteringDimensions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Operations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Offers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "MeteringDimensions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "MarketplaceTokens",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_WebhookDeliveryLogs_TenantId",
                table: "WebhookDeliveryLogs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_UsageEvents_TenantId",
                table: "UsageEvents",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_TenantId",
                table: "Subscriptions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Plans_TenantId",
                table: "Plans",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanMeteringDimensions_TenantId",
                table: "PlanMeteringDimensions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Operations_TenantId",
                table: "Operations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Offers_TenantId_OfferId",
                table: "Offers",
                columns: new[] { "TenantId", "OfferId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MeteringDimensions_TenantId",
                table: "MeteringDimensions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceTokens_TenantId_Token",
                table: "MarketplaceTokens",
                columns: new[] { "TenantId", "Token" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MarketplaceTokens_Tenants_TenantId",
                table: "MarketplaceTokens",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MeteringDimensions_Tenants_TenantId",
                table: "MeteringDimensions",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Offers_Tenants_TenantId",
                table: "Offers",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Operations_Tenants_TenantId",
                table: "Operations",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PlanMeteringDimensions_Tenants_TenantId",
                table: "PlanMeteringDimensions",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Plans_Tenants_TenantId",
                table: "Plans",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_Tenants_TenantId",
                table: "Subscriptions",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UsageEvents_Tenants_TenantId",
                table: "UsageEvents",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WebhookDeliveryLogs_Tenants_TenantId",
                table: "WebhookDeliveryLogs",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MarketplaceTokens_Tenants_TenantId",
                table: "MarketplaceTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_MeteringDimensions_Tenants_TenantId",
                table: "MeteringDimensions");

            migrationBuilder.DropForeignKey(
                name: "FK_Offers_Tenants_TenantId",
                table: "Offers");

            migrationBuilder.DropForeignKey(
                name: "FK_Operations_Tenants_TenantId",
                table: "Operations");

            migrationBuilder.DropForeignKey(
                name: "FK_PlanMeteringDimensions_Tenants_TenantId",
                table: "PlanMeteringDimensions");

            migrationBuilder.DropForeignKey(
                name: "FK_Plans_Tenants_TenantId",
                table: "Plans");

            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_Tenants_TenantId",
                table: "Subscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_UsageEvents_Tenants_TenantId",
                table: "UsageEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_WebhookDeliveryLogs_Tenants_TenantId",
                table: "WebhookDeliveryLogs");

            migrationBuilder.DropIndex(
                name: "IX_WebhookDeliveryLogs_TenantId",
                table: "WebhookDeliveryLogs");

            migrationBuilder.DropIndex(
                name: "IX_UsageEvents_TenantId",
                table: "UsageEvents");

            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_TenantId",
                table: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_Plans_TenantId",
                table: "Plans");

            migrationBuilder.DropIndex(
                name: "IX_PlanMeteringDimensions_TenantId",
                table: "PlanMeteringDimensions");

            migrationBuilder.DropIndex(
                name: "IX_Operations_TenantId",
                table: "Operations");

            migrationBuilder.DropIndex(
                name: "IX_Offers_TenantId_OfferId",
                table: "Offers");

            migrationBuilder.DropIndex(
                name: "IX_MeteringDimensions_TenantId",
                table: "MeteringDimensions");

            migrationBuilder.DropIndex(
                name: "IX_MarketplaceTokens_TenantId_Token",
                table: "MarketplaceTokens");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "WebhookDeliveryLogs");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "UsageEvents");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Plans");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "PlanMeteringDimensions");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Operations");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Offers");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "MeteringDimensions");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "MarketplaceTokens");

            migrationBuilder.CreateIndex(
                name: "IX_Offers_OfferId",
                table: "Offers",
                column: "OfferId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceTokens_Token",
                table: "MarketplaceTokens",
                column: "Token",
                unique: true);
        }
    }
}
