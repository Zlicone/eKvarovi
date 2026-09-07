using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eKvarovi.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AttachmentPurposes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttachmentPurposes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FaultPriorities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Rank = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaultPriorities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FaultStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Rank = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaultStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FaultTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaultTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InterventionStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterventionStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LocationTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MaterialUnits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Abbreviation = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialUnits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Address = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    City = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LocationTypeId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Locations_LocationTypes_LocationTypeId",
                        column: x => x.LocationTypeId,
                        principalTable: "LocationTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Materials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    UnitId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Materials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Materials_MaterialUnits_UnitId",
                        column: x => x.UnitId,
                        principalTable: "MaterialUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    LocationId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Employees_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    EmployeeId = table.Column<int>(type: "INTEGER", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppUsers_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FaultReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    LocationId = table.Column<int>(type: "INTEGER", nullable: false),
                    ReporterId = table.Column<int>(type: "INTEGER", nullable: false),
                    FaultTypeId = table.Column<int>(type: "INTEGER", nullable: true),
                    PriorityId = table.Column<int>(type: "INTEGER", nullable: true),
                    StatusId = table.Column<int>(type: "INTEGER", nullable: false),
                    DueDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ClosedByEmployeeId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaultReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FaultReports_Employees_ClosedByEmployeeId",
                        column: x => x.ClosedByEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FaultReports_Employees_ReporterId",
                        column: x => x.ReporterId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FaultReports_FaultPriorities_PriorityId",
                        column: x => x.PriorityId,
                        principalTable: "FaultPriorities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FaultReports_FaultStatuses_StatusId",
                        column: x => x.StatusId,
                        principalTable: "FaultStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FaultReports_FaultTypes_FaultTypeId",
                        column: x => x.FaultTypeId,
                        principalTable: "FaultTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FaultReports_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppUserRoles",
                columns: table => new
                {
                    AppUserId = table.Column<int>(type: "INTEGER", nullable: false),
                    AppRoleId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUserRoles", x => new { x.AppUserId, x.AppRoleId });
                    table.ForeignKey(
                        name: "FK_AppUserRoles_AppRoles_AppRoleId",
                        column: x => x.AppRoleId,
                        principalTable: "AppRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppUserRoles_AppUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FaultAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FaultReportId = table.Column<int>(type: "INTEGER", nullable: false),
                    TechnicianId = table.Column<int>(type: "INTEGER", nullable: false),
                    AssignedByEmployeeId = table.Column<int>(type: "INTEGER", nullable: true),
                    AssignedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UnassignedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Note = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaultAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FaultAssignments_Employees_AssignedByEmployeeId",
                        column: x => x.AssignedByEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FaultAssignments_Employees_TechnicianId",
                        column: x => x.TechnicianId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FaultAssignments_FaultReports_FaultReportId",
                        column: x => x.FaultReportId,
                        principalTable: "FaultReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FaultReportEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FaultReportId = table.Column<int>(type: "INTEGER", nullable: false),
                    EventType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    OldValue = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    NewValue = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    ChangedByAppUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaultReportEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FaultReportEvents_AppUsers_ChangedByAppUserId",
                        column: x => x.ChangedByAppUserId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FaultReportEvents_FaultReports_FaultReportId",
                        column: x => x.FaultReportId,
                        principalTable: "FaultReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Interventions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FaultAssignmentId = table.Column<int>(type: "INTEGER", nullable: false),
                    StatusId = table.Column<int>(type: "INTEGER", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FinishedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DurationMinutes = table.Column<int>(type: "INTEGER", nullable: true),
                    Note = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Interventions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Interventions_FaultAssignments_FaultAssignmentId",
                        column: x => x.FaultAssignmentId,
                        principalTable: "FaultAssignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Interventions_InterventionStatuses_StatusId",
                        column: x => x.StatusId,
                        principalTable: "InterventionStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FaultAttachments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FaultReportId = table.Column<int>(type: "INTEGER", nullable: false),
                    InterventionId = table.Column<int>(type: "INTEGER", nullable: true),
                    PurposeId = table.Column<int>(type: "INTEGER", nullable: false),
                    OriginalFileName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    StoredFileName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    SizeBytes = table.Column<long>(type: "INTEGER", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UploadedByAppUserId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaultAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FaultAttachments_AppUsers_UploadedByAppUserId",
                        column: x => x.UploadedByAppUserId,
                        principalTable: "AppUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FaultAttachments_AttachmentPurposes_PurposeId",
                        column: x => x.PurposeId,
                        principalTable: "AttachmentPurposes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FaultAttachments_FaultReports_FaultReportId",
                        column: x => x.FaultReportId,
                        principalTable: "FaultReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FaultAttachments_Interventions_InterventionId",
                        column: x => x.InterventionId,
                        principalTable: "Interventions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InterventionMaterials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    InterventionId = table.Column<int>(type: "INTEGER", nullable: false),
                    MaterialId = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantity = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterventionMaterials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InterventionMaterials_Interventions_InterventionId",
                        column: x => x.InterventionId,
                        principalTable: "Interventions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InterventionMaterials_Materials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppRoles_Name",
                table: "AppRoles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppUserRoles_AppRoleId",
                table: "AppUserRoles",
                column: "AppRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_AppUsers_Email",
                table: "AppUsers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppUsers_EmployeeId",
                table: "AppUsers",
                column: "EmployeeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppUsers_Username",
                table: "AppUsers",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AttachmentPurposes_Code",
                table: "AttachmentPurposes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Email",
                table: "Employees",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_LocationId",
                table: "Employees",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_FaultAssignments_AssignedByEmployeeId",
                table: "FaultAssignments",
                column: "AssignedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_FaultAssignments_FaultReportId",
                table: "FaultAssignments",
                column: "FaultReportId",
                unique: true,
                filter: "\"UnassignedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FaultAssignments_TechnicianId",
                table: "FaultAssignments",
                column: "TechnicianId");

            migrationBuilder.CreateIndex(
                name: "IX_FaultAttachments_FaultReportId",
                table: "FaultAttachments",
                column: "FaultReportId");

            migrationBuilder.CreateIndex(
                name: "IX_FaultAttachments_InterventionId",
                table: "FaultAttachments",
                column: "InterventionId");

            migrationBuilder.CreateIndex(
                name: "IX_FaultAttachments_PurposeId",
                table: "FaultAttachments",
                column: "PurposeId");

            migrationBuilder.CreateIndex(
                name: "IX_FaultAttachments_StoredFileName",
                table: "FaultAttachments",
                column: "StoredFileName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FaultAttachments_UploadedByAppUserId",
                table: "FaultAttachments",
                column: "UploadedByAppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FaultPriorities_Name",
                table: "FaultPriorities",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FaultPriorities_Rank",
                table: "FaultPriorities",
                column: "Rank",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FaultReportEvents_ChangedByAppUserId",
                table: "FaultReportEvents",
                column: "ChangedByAppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FaultReportEvents_FaultReportId",
                table: "FaultReportEvents",
                column: "FaultReportId");

            migrationBuilder.CreateIndex(
                name: "IX_FaultReports_ClosedByEmployeeId",
                table: "FaultReports",
                column: "ClosedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_FaultReports_CreatedAt",
                table: "FaultReports",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_FaultReports_FaultTypeId",
                table: "FaultReports",
                column: "FaultTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_FaultReports_LocationId",
                table: "FaultReports",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_FaultReports_PriorityId",
                table: "FaultReports",
                column: "PriorityId");

            migrationBuilder.CreateIndex(
                name: "IX_FaultReports_ReporterId",
                table: "FaultReports",
                column: "ReporterId");

            migrationBuilder.CreateIndex(
                name: "IX_FaultReports_StatusId",
                table: "FaultReports",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_FaultStatuses_Code",
                table: "FaultStatuses",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FaultStatuses_Name",
                table: "FaultStatuses",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FaultTypes_Name",
                table: "FaultTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InterventionMaterials_InterventionId_MaterialId",
                table: "InterventionMaterials",
                columns: new[] { "InterventionId", "MaterialId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InterventionMaterials_MaterialId",
                table: "InterventionMaterials",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_Interventions_FaultAssignmentId",
                table: "Interventions",
                column: "FaultAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Interventions_StatusId",
                table: "Interventions",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_InterventionStatuses_Code",
                table: "InterventionStatuses",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InterventionStatuses_Name",
                table: "InterventionStatuses",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Locations_LocationTypeId",
                table: "Locations",
                column: "LocationTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationTypes_Name",
                table: "LocationTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Materials_Name",
                table: "Materials",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Materials_UnitId",
                table: "Materials",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialUnits_Name",
                table: "MaterialUnits",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppUserRoles");

            migrationBuilder.DropTable(
                name: "FaultAttachments");

            migrationBuilder.DropTable(
                name: "FaultReportEvents");

            migrationBuilder.DropTable(
                name: "InterventionMaterials");

            migrationBuilder.DropTable(
                name: "AppRoles");

            migrationBuilder.DropTable(
                name: "AttachmentPurposes");

            migrationBuilder.DropTable(
                name: "AppUsers");

            migrationBuilder.DropTable(
                name: "Interventions");

            migrationBuilder.DropTable(
                name: "Materials");

            migrationBuilder.DropTable(
                name: "FaultAssignments");

            migrationBuilder.DropTable(
                name: "InterventionStatuses");

            migrationBuilder.DropTable(
                name: "MaterialUnits");

            migrationBuilder.DropTable(
                name: "FaultReports");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "FaultPriorities");

            migrationBuilder.DropTable(
                name: "FaultStatuses");

            migrationBuilder.DropTable(
                name: "FaultTypes");

            migrationBuilder.DropTable(
                name: "Locations");

            migrationBuilder.DropTable(
                name: "LocationTypes");
        }
    }
}
