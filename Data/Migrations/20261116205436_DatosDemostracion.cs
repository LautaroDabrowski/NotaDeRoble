using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InstitutoBlazor.Data.Migrations
{
    /// <inheritdoc />
    public partial class DatosDemostracion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "departamento",
                columns: new[] { "id_Departamento", "Nombre" },
                values: new object[,]
                {
                    { 1, "Ventas" },
                    { 2, "Producción" },
                    { 3, "Marketing" },
                    { 4, "Administración" },
                    { 5, "Gerencia" }
                });

            migrationBuilder.InsertData(
                table: "localidad",
                columns: new[] { "CodPostal", "Nombre", "Provincia" },
                values: new object[,]
                {
                    { 1804, "Canning", "Buenos Aires" },
                    { 1842, "Monte Grande", "Buenos Aires" },
                    { 1865, "San Vicente", "Buenos Aires" },
                    { 1980, "Brandsen", "Buenos Aires" },
                    { 1984, "Domselaar", "Buenos Aires" }
                });

            migrationBuilder.InsertData(
                table: "empleados",
                columns: new[] { "id", "apellido", "CodPostal", "Direccion", "dni", "id_Departamento", "nombre", "telefono" },
                values: new object[,]
                {
                    { 1, "Álvarez", 1865, "Calle de demostración 1", "90000001", 1, "Ana", null },
                    { 2, "Benítez", 1984, "Calle de demostración 2", "90000002", 2, "Bruno", null },
                    { 3, "Castro", 1980, "Calle de demostración 3", "90000003", 3, "Carla", null },
                    { 4, "Díaz", 1804, "Calle de demostración 4", "90000004", 4, "Diego", null },
                    { 5, "Estévez", 1842, "Calle de demostración 5", "90000005", 5, "Elena", null },
                    { 6, "Fernández", 1865, "Calle de demostración 6", "90000006", 1, "Fabio", null },
                    { 7, "García", 1984, "Calle de demostración 7", "90000007", 2, "Gabriela", null },
                    { 8, "Herrera", 1980, "Calle de demostración 8", "90000008", 3, "Hugo", null },
                    { 9, "Ibarra", 1804, "Calle de demostración 9", "90000009", 4, "Inés", null },
                    { 10, "Juárez", 1842, "Calle de demostración 10", "90000010", 5, "Javier", null },
                    { 11, "López", 1865, "Calle de demostración 11", "90000011", 1, "Lucía", null },
                    { 12, "Molina", 1984, "Calle de demostración 12", "90000012", 2, "Martín", null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "empleados",
                keyColumn: "id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "empleados",
                keyColumn: "id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "empleados",
                keyColumn: "id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "empleados",
                keyColumn: "id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "empleados",
                keyColumn: "id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "empleados",
                keyColumn: "id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "empleados",
                keyColumn: "id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "empleados",
                keyColumn: "id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "empleados",
                keyColumn: "id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "empleados",
                keyColumn: "id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "empleados",
                keyColumn: "id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "empleados",
                keyColumn: "id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "departamento",
                keyColumn: "id_Departamento",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "departamento",
                keyColumn: "id_Departamento",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "departamento",
                keyColumn: "id_Departamento",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "departamento",
                keyColumn: "id_Departamento",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "departamento",
                keyColumn: "id_Departamento",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "localidad",
                keyColumn: "CodPostal",
                keyValue: 1804);

            migrationBuilder.DeleteData(
                table: "localidad",
                keyColumn: "CodPostal",
                keyValue: 1842);

            migrationBuilder.DeleteData(
                table: "localidad",
                keyColumn: "CodPostal",
                keyValue: 1865);

            migrationBuilder.DeleteData(
                table: "localidad",
                keyColumn: "CodPostal",
                keyValue: 1980);

            migrationBuilder.DeleteData(
                table: "localidad",
                keyColumn: "CodPostal",
                keyValue: 1984);
        }
    }
}
