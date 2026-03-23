using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GigBoardBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddDirectForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Shifts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Expenses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ShiftId",
                table: "Deliveries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Deliveries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(@"
                UPDATE Deliveries 
                SET UserId = (
                    SELECT TOP 1 UserId 
                    FROM UserDeliveries 
                    WHERE UserDeliveries.DeliveryId = Deliveries.Id
                )
                WHERE EXISTS (
                    SELECT 1 FROM UserDeliveries WHERE UserDeliveries.DeliveryId = Deliveries.Id
                );
            ");

            migrationBuilder.Sql(@"
                UPDATE Deliveries 
                SET ShiftId = (
                    SELECT TOP 1 ShiftId 
                    FROM ShiftDeliveries 
                    WHERE ShiftDeliveries.DeliveryId = Deliveries.Id
                )
                WHERE EXISTS (
                    SELECT 1 FROM ShiftDeliveries WHERE ShiftDeliveries.DeliveryId = Deliveries.Id
                );
            ");

            migrationBuilder.Sql(@"
                UPDATE Shifts 
                SET UserId = (
                    SELECT TOP 1 UserId 
                    FROM UserShifts 
                    WHERE UserShifts.ShiftId = Shifts.Id
                )
                WHERE EXISTS (
                    SELECT 1 FROM UserShifts WHERE UserShifts.ShiftId = Shifts.Id
                );
            ");

            migrationBuilder.Sql(@"
                UPDATE Expenses 
                SET UserId = (
                    SELECT TOP 1 UserId 
                    FROM UserExpenses 
                    WHERE UserExpenses.ExpenseId = Expenses.Id
                )
                WHERE EXISTS (
                    SELECT 1 FROM UserExpenses WHERE UserExpenses.ExpenseId = Expenses.Id
                );
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_UserId",
                table: "Shifts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_UserId",
                table: "Expenses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Deliveries_ShiftId",
                table: "Deliveries",
                column: "ShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_Deliveries_UserId",
                table: "Deliveries",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Deliveries_Shifts_ShiftId",
                table: "Deliveries",
                column: "ShiftId",
                principalTable: "Shifts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Deliveries_Users_UserId",
                table: "Deliveries",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_Users_UserId",
                table: "Expenses",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Shifts_Users_UserId",
                table: "Shifts",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deliveries_Shifts_ShiftId",
                table: "Deliveries");

            migrationBuilder.DropForeignKey(
                name: "FK_Deliveries_Users_UserId",
                table: "Deliveries");

            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_Users_UserId",
                table: "Expenses");

            migrationBuilder.DropForeignKey(
                name: "FK_Shifts_Users_UserId",
                table: "Shifts");

            migrationBuilder.DropIndex(
                name: "IX_Shifts_UserId",
                table: "Shifts");

            migrationBuilder.DropIndex(
                name: "IX_Expenses_UserId",
                table: "Expenses");

            migrationBuilder.DropIndex(
                name: "IX_Deliveries_ShiftId",
                table: "Deliveries");

            migrationBuilder.DropIndex(
                name: "IX_Deliveries_UserId",
                table: "Deliveries");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "ShiftId",
                table: "Deliveries");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Deliveries");
        }
    }
}
