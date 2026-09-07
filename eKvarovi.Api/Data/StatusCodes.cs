namespace eKvarovi.Api.Data;

public static class FaultStatusCodes
{
    public const string Received = "ZAPRIMLJENO";
    public const string Reviewed = "PREGLEDANO";
    public const string Assigned = "DODIJELJENO";
    public const string InProgress = "U_RADU";
    public const string Resolved = "RIJESENO";
    public const string Closed = "ZATVORENO";
}

public static class InterventionStatusCodes
{
    public const string Planned = "PLANIRANA";
    public const string InProgress = "U_TIJEKU";
    public const string Completed = "ZAVRSENA";
    public const string Failed = "NEUSPJESNA";
}

public static class AttachmentPurposeCodes
{
    public const string PhotoBefore = "FOTO_PRIJE";
    public const string PhotoAfter = "FOTO_NAKON";
    public const string Document = "DOKUMENT";
}

public static class RoleNames
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string Technician = "Technician";
    public const string Reporter = "Reporter";
}