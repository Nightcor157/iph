using System;
using System.IO;

namespace OurIPH.Services
{
    public static class AppPaths
    {
        public static string WorkspaceRoot
        {
            get
            {
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                return Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", ".."));
            }
        }

        public static string EveIphDatabasePath
        {
            get
            {
                var latest = Path.Combine(WorkspaceRoot, "LatestFiles", "EVEIPH DB.sqlite");
                if (File.Exists(latest))
                {
                    return GetSqliteSafeDatabasePath(latest);
                }

                return GetSqliteSafeDatabasePath(Path.Combine(WorkspaceRoot, "Root Directory", "EVEIPH DB.sqlite"));
            }
        }

        public static string RuntimeDataDirectory
        {
            get
            {
                var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var path = Path.Combine(local, "OurIPH", "Data");
                Directory.CreateDirectory(path);
                return path;
            }
        }

        public static string SettingsDirectory
        {
            get
            {
                var path = Path.Combine(WorkspaceRoot, "OurIPH", "Settings");
                Directory.CreateDirectory(path);
                return path;
            }
        }

        public static string GetSettingsPath(string fileName)
        {
            return Path.Combine(SettingsDirectory, fileName);
        }

        private static string GetSqliteSafeDatabasePath(string sourcePath)
        {
            if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
            {
                return sourcePath;
            }

            if (!IsUncPath(sourcePath))
            {
                return sourcePath;
            }

            var localPath = Path.Combine(RuntimeDataDirectory, "EVEIPH DB.sqlite");
            CopyIfNewerOrDifferent(sourcePath, localPath);
            AppLogger.Info("Using local SQLite runtime copy: " + localPath);
            return localPath;
        }

        private static bool IsUncPath(string path)
        {
            return path.StartsWith(@"\\", StringComparison.Ordinal);
        }

        private static void CopyIfNewerOrDifferent(string sourcePath, string localPath)
        {
            var source = new FileInfo(sourcePath);
            var target = new FileInfo(localPath);
            if (target.Exists
                && target.Length == source.Length
                && target.LastWriteTimeUtc >= source.LastWriteTimeUtc)
            {
                return;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(localPath));
            File.Copy(sourcePath, localPath, true);
            File.SetLastWriteTimeUtc(localPath, source.LastWriteTimeUtc);
        }
    }
}
