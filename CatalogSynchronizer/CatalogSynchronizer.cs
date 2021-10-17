using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CatalogSynchronizer
{
    class CatalogSynchronizer
    {
        private string _sourceDirectory;
        private string _replicaDirectory;
        private int _syncDelay;
        private string _logFile;
        private Regex _lastPath = new Regex(@"\\[^\\]*$");

        public CatalogSynchronizer(string sourceDirectory, string replicaDirectory, int syncDelay, string logFile)
        {
            _sourceDirectory = sourceDirectory;
            _replicaDirectory = replicaDirectory;
            _syncDelay = syncDelay;
            _logFile = logFile;
        }

        public async Task SynchronizeAsync()
        {
            await Task.Factory.StartNew(() => Synchronize());
        }

        private void Synchronize()
        {
            while (true)
            {
                SynchronizeDirectorys(_sourceDirectory, _replicaDirectory);
                SynchronizeFiles(_sourceDirectory, _replicaDirectory);
                Thread.Sleep(1000 * 60 * _syncDelay);
            }
        }

        private void SynchronizeDirectorys(string sourceDirectory, string replicaDirectory)
        {
           
            var sourceDirectories = Directory.EnumerateDirectories(sourceDirectory);

            var replicaDirectories = Directory.EnumerateDirectories(replicaDirectory);

            var replicaDirectoriesSet = new HashSet<string>();

            foreach (var directory in replicaDirectories)
            {
                replicaDirectoriesSet.Add(directory);
            }

            foreach (var directory in sourceDirectories)
            {
                var replicaDir = replicaDirectory + _lastPath.Match(directory).Value;

                if (!replicaDirectoriesSet.Contains(replicaDir))
                {
                    Directory.CreateDirectory(replicaDir);

                    var message = $"{DateTime.Now} created directory {replicaDir}\n";
                    Console.Write(message);
                    File.AppendAllText(_logFile, message);
                }
                else
                {
                    replicaDirectoriesSet.Remove(replicaDir);
                }


                SynchronizeDirectorys(directory, replicaDir);

                SynchronizeFiles(directory, replicaDir);
            }

            foreach (var directory in replicaDirectoriesSet)
            {
                Directory.Delete(directory, true);

                var message = $"{DateTime.Now} deleted directory {directory} with all subdirectories and files\n";
                Console.Write(message);
                File.AppendAllText(_logFile, message);
            }
        }

        private void SynchronizeFiles(string sourceDirectory, string replicaDirectory)
        {
            var filesInSource = Directory.EnumerateFiles(sourceDirectory);
            var filesInReplica = Directory.EnumerateFiles(replicaDirectory);

            var filesInReplicaSet = new HashSet<string>();

            foreach (var file in filesInReplica)
            {
                filesInReplicaSet.Add(file);
            }

            foreach (var file in filesInSource)
            {
                var replicaFile = replicaDirectory + _lastPath.Match(file).Value;

                if (!filesInReplica.Contains(replicaFile))
                {
                    File.Copy(file, replicaFile);

                    var message = $"{DateTime.Now} copied file {replicaFile}\n";
                    Console.Write(message);
                    File.AppendAllText(_logFile, message);
                }
                else 
                {
                    if (!IsFilesEqualse(file, replicaFile))
                    {
                        File.Delete(replicaFile);
                        File.Copy(file, replicaFile);

                        var message = $"{DateTime.Now} copied file {replicaFile}\n";
                        Console.Write(message);
                        File.AppendAllText(_logFile, message);
                    }

                    filesInReplicaSet.Remove(replicaFile);
                }
            }

            foreach (var file in filesInReplicaSet)
            {
                File.Delete(file);

                var message = $"{DateTime.Now} deleted file {file}\n";
                Console.Write(message);
                File.AppendAllText(_logFile, message);
            }
        }

        private bool IsFilesEqualse(string firstPath, string secondPath)
        {
            var fs1 = new FileStream(firstPath, FileMode.Open);
            var fs2 = new FileStream(secondPath, FileMode.Open);
            int fs1Byte;
            int fs2Byte;
            
            if (fs1.Length != fs2.Length)
            {
                fs1.Close();
                fs2.Close();

                return false;
            }

            do
            {
                fs1Byte = fs1.ReadByte();
                fs2Byte = fs2.ReadByte();
            } while ((fs1Byte == fs2Byte) && (fs1Byte != -1));

            fs1.Close();
            fs2.Close();

            return (fs1Byte - fs2Byte == 0);
        }
    }
}
