-- Folders can contain other folders. NULL ParentFolderId means the folder is at the user's root.
ALTER TABLE folders
    ADD COLUMN ParentFolderId UUID REFERENCES folders(Id) ON DELETE CASCADE;

CREATE INDEX ix_folders_user_parent ON folders (UserId, ParentFolderId);

-- Folder names are unique within their parent, so a path like "Skyblock/Dungeons" resolves to one folder.
-- Two partial indexes because NULL parents (root folders) are never equal in a normal unique index.
CREATE UNIQUE INDEX ux_folders_root_name ON folders (UserId, Name) WHERE ParentFolderId IS NULL;
CREATE UNIQUE INDEX ux_folders_child_name ON folders (ParentFolderId, Name) WHERE ParentFolderId IS NOT NULL;
