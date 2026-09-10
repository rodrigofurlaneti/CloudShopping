CREATE TABLE profilepermissions (
 Id int NOT NULL AUTO_INCREMENT PRIMARY KEY,
 TenantId int NOT NULL,
 ProfileId int NOT NULL,
 Permission varchar(60) NOT NULL,
 UNIQUE KEY ux_profile_permission (TenantId,ProfileId,Permission),
 CONSTRAINT fk_permission_profile FOREIGN KEY (ProfileId) REFERENCES profiles(Id)
);
CREATE TABLE accesschanges (
 Id char(32) NOT NULL PRIMARY KEY,
 TenantId int NOT NULL,
 ProfileId int NOT NULL,
 ActorId int NOT NULL,
 BeforeJson text NOT NULL,
 AfterJson text NOT NULL,
 CreatedAt datetime(6) NOT NULL,
 KEY ix_access_changes (TenantId,ProfileId,CreatedAt)
);
