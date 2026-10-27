-- Tracker names are unique within their folder, so ingest can find a tracker by folder path + name
CREATE UNIQUE INDEX ux_trackers_folder_name ON trackers (FolderId, Name);
