using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArbitrageApp.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ArbitrageOpportunities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Symbol = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BinancePrice = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CoinbasePrice = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CryptoComPrice = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KrakenPrice = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PercentDiff = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArbitrageOpportunities", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArbitrageOpportunities_Symbol",
                table: "ArbitrageOpportunities",
                column: "Symbol");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArbitrageOpportunities");
        }
    }
}
