using System.Runtime.InteropServices.Marshalling;
using System.Runtime.Versioning;
using System.Threading.Tasks.Dataflow;
using MiniGit.Utils;

namespace MiniGit.Utils
{
    public static class FileBackup
    {
        private const string BackupFolder = ".minigit/backups";

        public static (bool Success, string? BackupPath) SafeOverwrite(string sourcePath, string targetPath, bool createBackup = true)
        {
            try
            {
                Logger.INFO($"Starting safe overwrite: {sourcePath} -> {targetPath}");

                if (!File.Exists(sourcePath))
                {
                    Logger.ERROR($"Source file does not exist: {sourcePath}");
                    return (false, null);
                }

                string? backupPath = null;

                if (createBackup && File.Exists(targetPath))
                {
                    backupPath = CreateBackup(targetPath);
                    if (backupPath == null)
                    {
                        Logger.ERROR($"Failed to create backup, aborting overwrite for safety");
                        return (false, null);
                    }
                    Logger.INFO($"Created backup: {backupPath}");
                }

                var targetDir = Path.GetDirectoryName(targetPath);
                if (!string.IsNullOrEmpty(targetDir) && !Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                    Logger.DEBUG($"Created target directory: {targetDir}");
                }

                File.Copy(sourcePath, targetPath, overwrite: true);
                Logger.INFO($"Successfully overwrote file: {targetPath}");

                return (true, backupPath);
            }
            catch (Exception ex)
            {
                Logger.ERROR($"Safe overwrite failed: {ex.Message}");
                return (false, null);
            }
        }

        public static string? CreateBackup(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    Logger.WARN($"Cannot backup non-existent file: {filePath}");
                    return null;
                }

                if (!Directory.Exists(BackupFolder))
                {
                    Directory.CreateDirectory(BackupFolder);
                    Logger.DEBUG($"Created backup directory: {BackupFolder}");
                }

                var fileName = Path.GetFileName(filePath);
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var backupFileName = $"{fileName}.backup_{timestamp}";
                var backupPath = Path.Combine(BackupFolder, backupFileName);

                int counter = 1;
                var originalBackupPath = backupPath;
                while (File.Exists(backupPath))
                {
                    var nameWithoutExt = Path.GetFileNameWithoutExtension(originalBackupPath);
                    var ext = Path.GetExtension(originalBackupPath);
                    backupPath = Path.Combine(BackupFolder, $"{nameWithoutExt}_{counter}{ext}");
                    counter++;
                }

                File.Copy(filePath, backupPath);
                Logger.DEBUG($"Created backup file: {backupPath}");

                return backupPath;
            }
            catch (Exception ex)
            {
                Logger.ERROR($"Backup creation failed for {filePath}: {ex.Message}");
                return null;
            }
        }

        public static bool RestoreFromBackup(string backupPath, string targetPath)
        {
            try
            {
                if (!File.Exists(backupPath))
                {
                    Logger.ERROR($"Backup file does not exist: {backupPath}");
                    return false;
                }

                var targetDir = Path.GetDirectoryName(targetPath);
                if (!string.IsNullOrEmpty(targetDir) && !Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }

                File.Copy(backupPath, targetPath, overwrite: true);
                Logger.INFO($"Successfully restored from backup: {backupPath} -> {targetPath}");

                return true;
            }
            catch (Exception ex)
            {
                Logger.ERROR($"Restore from backup failed: {ex.Message}");
                return false;
            }
        }

        public static string[] ListBackups()
        {
            try
            {
                if (!Directory.Exists(BackupFolder))
                    return Array.Empty<string>();

                var backups = Directory.GetFiles(BackupFolder, "*.backup_*")
                    .OrderByDescending(f => File.GetCreationTime(f))
                    .ToArray();

                Logger.DEBUG($"Found {backups.Length} backup files");
                return backups;
            }
            catch (Exception ex)
            {
                Logger.ERROR($"Failed to list backups: {ex.Message}");
                return Array.Empty<string>();
            }
        }
    }
}