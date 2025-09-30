using System.Text.Json;
using MiniGit.Utils;

namespace MiniGit.Core
{
    public class CommitTransaction : IDisposable
    {
        private const string RepoFolder = ".minigit";
        private const string CommitsFile = "commits.json";
        private const string TempSuffix = ".tmp";
        private const string BackupSuffix = ".backup";

        private readonly string _tempCommitsFile;
        private readonly string _backupCommitsFile;
        private readonly string _tempSnapshotDir;
        private readonly string _commitId;
        private readonly List<string> _createdTempFiles;
        private bool _committed = false;
        private bool _disposed = false;

        public CommitTransaction(string commitId)
        {
            _commitId = commitId;
            _tempCommitsFile = Path.Combine(RepoFolder, CommitsFile + TempSuffix);
            _backupCommitsFile = Path.Combine(RepoFolder, CommitsFile + BackupSuffix);
            _tempSnapshotDir = Path.Combine(RepoFolder, "snapshots", commitId + TempSuffix);
            _createdTempFiles = new List<string>();

            Logger.INFO($"Starting atomic commit transaction for ID: '{commitId}'");
        }

        public bool Prepare(List<Commit> commits, List<string> filesToSnapshot)
        {
            try
            {
                Logger.INFO("Phase 1: Preparing commit transaction...");

                if (!Directory.Exists(RepoFolder))
                    Directory.CreateDirectory(RepoFolder);

                string commitsPath = Path.Combine(RepoFolder, CommitsFile);
                if (File.Exists(commitsPath))
                {
                    File.Copy(commitsPath, _backupCommitsFile, overwrite: true);
                    _createdTempFiles.Add(_backupCommitsFile);
                    Logger.DEBUG($"Created backup: {_backupCommitsFile}");
                }

                var json = JsonSerializer.Serialize(commits, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_tempCommitsFile, json);
                _createdTempFiles.Add(_tempCommitsFile);
                Logger.DEBUG($"Prepared commits in temp file: {_tempCommitsFile}");

                if (filesToSnapshot.Count > 0)
                {
                    Directory.CreateDirectory(_tempSnapshotDir);
                    _createdTempFiles.Add(_tempSnapshotDir);

                    foreach (var file in filesToSnapshot)
                    {
                        var relativePath = Path.GetRelativePath(Directory.GetCurrentDirectory(), file);
                        var targetPath = Path.Combine(_tempSnapshotDir, relativePath);
                        var targetDir = Path.GetDirectoryName(targetPath);

                        if (!string.IsNullOrEmpty(targetDir) && !Directory.Exists(targetDir))
                        {
                            Directory.CreateDirectory(targetDir);
                        }

                        File.Copy(file, targetPath, overwrite: true);
                        Logger.DEBUG($"Prepared file snapshot: {relativePath}");
                    }
                }
                Logger.INFO("Phase 1 completed successfully - all data prepared in temp locations");
                return true;
            }
            catch (Exception ex)
            {
                Logger.ERROR($"Phase 1 failed: {ex.Message}");
                Rollback();
                return false;
            }
        }

        public bool Commit()
        {
            try
            {
                Logger.INFO("Phase 2: Committing transaction...");

                string finalCommitsPath = Path.Combine(RepoFolder, CommitsFile);
                if (File.Exists(_tempCommitsFile))
                {
                    File.Move(_tempCommitsFile, finalCommitsPath, overwrite: true);
                    Logger.DEBUG($"Committed commits file: {finalCommitsPath}");
                }

                string finalSnapshotDir = Path.Combine(RepoFolder, "snapshots", _commitId);
                if (Directory.Exists(_tempSnapshotDir))
                {
                    if (Directory.Exists(finalSnapshotDir))
                    {
                        Directory.Delete(finalSnapshotDir, recursive: true);
                    }
                    Directory.Move(_tempSnapshotDir, finalSnapshotDir);
                    Logger.DEBUG($"Committed snapshot directory: {finalSnapshotDir}");
                }

                if (File.Exists(_backupCommitsFile))
                {
                    File.Delete(_backupCommitsFile);
                    Logger.DEBUG("Cleaned up backup file");
                }

                _committed = true;
                Logger.INFO($"Atomic commit transaction completed successfully for ID: {_commitId}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.ERROR($"Phase 2 failed: {ex.Message}");
                RestoreFromBackup();
                return false;
            }
        }

        public void Rollback()
        {
            if (_committed)
                return;

            Logger.WARN($"Rolling back commit transaction for ID: '{_commitId}'");

            try
            {
                foreach (var tempPath in _createdTempFiles)
                {
                    try
                    {
                        if (File.Exists(tempPath))
                        {
                            File.Delete(tempPath);
                            Logger.DEBUG($"Cleaned up temp file: {tempPath}");
                        }
                        else if (Directory.Exists(tempPath))
                        {
                            Directory.Delete(tempPath);
                            Logger.DEBUG($"Cleaned up temp directory: {tempPath}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.WARN($"Failed to clean up {tempPath}: {ex.Message}");
                    }
                }
                Logger.INFO("Rollback completed");
            }
            catch (Exception ex)
            {
                Logger.ERROR($"Error during rollback: {ex.Message}");
            }
        }

        private void RestoreFromBackup()
        {
            try
            {
                string finalCommitsPath = Path.Combine(RepoFolder, CommitsFile);
                if (File.Exists(_backupCommitsFile))
                {
                    File.Copy(_backupCommitsFile, finalCommitsPath, overwrite: true);
                    Logger.INFO("Restored commits.json from backup");
                }
            }
            catch (Exception ex)
            {
                Logger.ERROR($"Failed to restore from backup: {ex.Message}");
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                if (_committed)
                {
                    Rollback();
                }
                _disposed = true;
            }
        }
    }
}