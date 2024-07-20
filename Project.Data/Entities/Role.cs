namespace Project.Data.Entities;
public enum RoleTypes {
    User = 0,
    Admin = 1,
}
public class Role {
    public Guid Id { get; set; }
    public RoleTypes RoleType { get; set; }
}
