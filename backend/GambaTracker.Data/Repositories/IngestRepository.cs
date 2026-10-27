using Dapper;
using GambaTracker.Core.Interfaces;

namespace GambaTracker.Data.Repositories
{
    public class IngestRepository : IIngestRepository
    {
        private readonly DapperContext _configuration;
        public IngestRepository(DapperContext configuration)
        {
            _configuration = configuration;
        }

        public async Task<(Guid FolderId, Guid TrackerId)> IngestEventAsync(Guid userId, IReadOnlyList<string> folderPath, string trackerName, Event eventItem)
        {
            using var conn = _configuration.CreateConnection();
            conn.Open();
            using var transaction = conn.BeginTransaction();

            // Walk the path from the root. ON CONFLICT DO NOTHING hits the unique name indexes when the folder
            // already exists (including one created by a concurrent request), then the SELECT reads its id.
            Guid? parentId = null;
            foreach (var name in folderPath)
            {
                await conn.ExecuteAsync(
                    "INSERT INTO Folders (Id, Name, UserId, ParentFolderId) VALUES (@Id, @Name, @UserId, @ParentFolderId) ON CONFLICT DO NOTHING",
                    new { Id = Guid.NewGuid(), Name = name, UserId = userId, ParentFolderId = parentId }, transaction);

                parentId = await conn.QuerySingleAsync<Guid>(
                    "SELECT Id FROM Folders WHERE UserId = @UserId AND ParentFolderId IS NOT DISTINCT FROM @ParentFolderId AND Name = @Name",
                    new { UserId = userId, ParentFolderId = parentId, Name = name }, transaction);
            }
            var folderId = parentId!.Value;

            await conn.ExecuteAsync(
                "INSERT INTO Trackers (Id, Name, FolderId) VALUES (@Id, @Name, @FolderId) ON CONFLICT (FolderId, Name) DO NOTHING",
                new { Id = Guid.NewGuid(), Name = trackerName, FolderId = folderId }, transaction);

            var trackerId = await conn.QuerySingleAsync<Guid>(
                "SELECT Id FROM Trackers WHERE FolderId = @FolderId AND Name = @Name",
                new { FolderId = folderId, Name = trackerName }, transaction);

            eventItem.TrackerId = trackerId;
            await EventWriter.InsertWithDropsAsync(conn, transaction, eventItem);

            transaction.Commit();
            return (folderId, trackerId);
        }
    }
}
