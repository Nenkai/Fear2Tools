using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Syroot.BinaryData;

namespace Fear2Tools
{
    public class LTArchive
    {
        public int[] CRCFileHashName = new int[0x04];
        public List<LTFileEntry> Files = new List<LTFileEntry>();
        public Dictionary<string, LTFolderEntry> Folders = new Dictionary<string, LTFolderEntry>();

        public static LTArchive Init(string path)
        {
            using var fs = new FileStream(path, FileMode.Open);
            using var br = new BinaryStream(fs);

            if (br.ReadInt32() != 0x5241544C)
                throw new Exception("Not a LT Archive.");

            int version = br.ReadInt32();
            if (version != 3)
                Console.WriteLine($"Warning: Unexpected archive version {version}");

            LTArchive archive = new LTArchive();

            // Main header
            int nameTablesize = br.ReadInt32();
            int folderCount = br.ReadInt32();
            int fileCount = br.ReadInt32();
            br.ReadInt32();
            br.ReadInt32();
            br.ReadInt32();

            archive.CRCFileHashName = br.ReadInt32s(4);
            br.BaseStream.Position += nameTablesize;

            long fileEntryTableOffset = br.BaseStream.Position;
            for (var i = 0; i < fileCount; i++)
            {
                br.BaseStream.Position = fileEntryTableOffset + (i * 0x20);
                var file = new LTFileEntry();

                int nameOffset = br.ReadInt32();
                file.FileOffset = br.ReadInt64();
                file.CompressedFileSize = br.ReadInt64();
                file.RawFileSize = br.ReadInt64();
                file.CompressionMethod = br.ReadInt32();

                br.BaseStream.Position = 0x30 + nameOffset;
                file.Name = br.ReadString(StringCoding.ZeroTerminated);
                archive.Files.Add(file);
            }

            long folderEntryTableOffset = fileEntryTableOffset + (fileCount * 0x20);
            for (var i = 0; i < folderCount; i++)
            {
                br.BaseStream.Position = folderEntryTableOffset + (i * 0x10);
                var folder = new LTFolderEntry();

                int nameOffset = br.ReadInt32();
                folder.FirstChildIndex = br.ReadInt32();
                if (folder.FirstChildIndex == -1)
                    folder.FirstChildIndex = 0;

                folder.NextFolderIndexForParent = br.ReadInt32();
                if (folder.NextFolderIndexForParent == -1)
                    folder.NextFolderIndexForParent = 0;

                folder.FileCount = br.ReadInt32();

                br.BaseStream.Position = 0x30 + nameOffset;
                folder.Name = br.ReadString(StringCoding.ZeroTerminated);
                archive.Folders.Add(folder.Name, folder);
            }

            return archive;
        }

        public static void Pack(string path, string outputPath)
        {
            var archive = new LTArchive();
            string[] files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);

            archive.BuildFileList(path, files);
            archive.BuildPack(outputPath);
        }

        private void BuildPack(string output)
        {
            using var fs = new FileStream(output, FileMode.Create);
            using var bs = new BinaryStream(fs);

            OptimizedStringTable strTable = new OptimizedStringTable();
            strTable.Alignment = 0x04;
            strTable.IsRelativeOffsets = true;

            // Skip header for now
            bs.Position = 0x30;
            foreach (var folder in Folders)
                strTable.AddString(folder.Key);

            foreach (var file in Files)
                strTable.AddString(file.Name);

            strTable.SaveStream(bs);

            long stringTableSize = bs.Position - 0x30;

            long fileEntriesMapOffset = bs.Position;
            long folderEntriesMapOffset = fileEntriesMapOffset + (Files.Count * 0x20);
            long dataOffset = folderEntriesMapOffset + (Folders.Count * 0x10);

            for (int i = 0; i < Files.Count; i++)
            {
                bs.Position = dataOffset; 
                LTFileEntry file = Files[i];
                file.FileOffset = dataOffset;
                file.RawFileSize = WriteFile(file, bs);
                file.CompressedFileSize = file.RawFileSize;
                dataOffset = bs.Position;

                bs.Position = fileEntriesMapOffset + (i * 0x20);

                bs.WriteInt32(strTable.GetStringOffset(file.Name));
                bs.WriteInt64(file.FileOffset);
                bs.WriteInt64(file.CompressedFileSize);
                bs.WriteInt64(file.RawFileSize);
                bs.WriteInt32(file.CompressionMethod);
            }

            int j = 0;
            foreach (var folder in Folders)
            {
                bs.Position = folderEntriesMapOffset + (j * 0x10);

                bs.WriteInt32(strTable.GetStringOffset(folder.Key));
                bs.WriteInt32(folder.Value.FirstChildIndex);
                bs.WriteInt32(folder.Value.NextFolderIndexForParent);
                bs.WriteInt32(folder.Value.Files.Count);

                j++;
            }

            // Write header
            bs.Position = 0;
            bs.WriteInt32(0x5241544C);
            bs.WriteInt32(3);
            bs.WriteInt32((int)stringTableSize);
            bs.WriteInt32(Folders.Count);
            bs.WriteInt32(Files.Count);
            bs.WriteInt32(1);
            bs.WriteInt32(0);
            bs.WriteInt32(1);

            bs.Flush();
        }

        private long WriteFile(LTFileEntry entry, Stream packStream)
        {
            using var fs = new FileStream(entry.FullPath, FileMode.Open);
            fs.CopyTo(packStream);
            packStream.Align(0x04, grow: true);

            return fs.Length;
        }

        private void BuildFileList(string path, string[] files)
        {
            LTFolderEntry parentFolder = new LTFolderEntry();
            Folders.Add("", parentFolder); // Root

            files = files.OrderBy(e => e, InsensitiveAlphaNumStringComparer.Default).ToArray();

            foreach (var file in files)
            {
                string relPath = file.Substring(path.Length).Replace('\\', '/');
                if (relPath.StartsWith('/'))
                    relPath = relPath.Substring(1);

                for (var i = 0; i < relPath.Length; i++)
                {
                    if (relPath[i] == '/')
                    {
                        string currentFolder = relPath.Substring(0, i);
                        int lastSlash = currentFolder.LastIndexOf('/');

                        if (lastSlash == -1) // Root?
                            parentFolder = Folders[""];
                        else
                            parentFolder = Folders[currentFolder.Substring(0, lastSlash)];

                        if (!Folders.ContainsKey(currentFolder))
                        {
                            var newChildFolder = new LTFolderEntry();
                            newChildFolder.Name = currentFolder.Replace('\\', '/');
                            newChildFolder.ParentFolder = parentFolder;
                            Folders.Add(newChildFolder.Name, newChildFolder);

                            parentFolder.Folders.Add(newChildFolder);
                            parentFolder = newChildFolder;
                        }
                        else
                            parentFolder = Folders[currentFolder];
                    }
                }

                var fileEntry = new LTFileEntry();
                fileEntry.FullPath = file;
                fileEntry.Name = Path.GetFileName(relPath);
                parentFolder.Files.Add(fileEntry);
                Files.Add(fileEntry);
            }


            // Link all folders together
            var foldersEntryList = Folders.Values.ToList();
            LinkFolders(Folders[""]); // Start from root

            void LinkFolders(LTFolderEntry folder)
            {
                for (int i = 0; i < folder.Folders.Count; i++)
                {
                    LTFolderEntry? child = folder.Folders[i];
                    if (i < folder.Folders.Count - 1)
                    {
                        var nextForParent = folder.Folders[i + 1];
                        int idx = foldersEntryList.IndexOf(nextForParent);
                        child.NextFolderIndexForParent = idx;
                    }

                    if (child.Folders.Count > 0)
                        child.FirstChildIndex = foldersEntryList.IndexOf(child.Folders[0]);

                    LinkFolders(child);
                }
            }
        }


    }

    public class LTFileEntry
    {
        public string FullPath;

        public string Name { get; set; }
        public long FileOffset { get; set; }
        public long CompressedFileSize { get; set; }
        public long RawFileSize { get; set; }
        public int CompressionMethod { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

    public class LTFolderEntry
    {
        public string Name { get; set; }
        public int FirstChildIndex { get; set; } = -1;
        public int NextFolderIndexForParent { get; set; } = -1;
        public int FileCount { get; set; }

        public LTFolderEntry ParentFolder { get; set; }
        public List<LTFileEntry> Files { get; set; } = new List<LTFileEntry>();
        public List<LTFolderEntry> Folders { get; set; } = new List<LTFolderEntry>();

        public override string ToString()
        {
            return Name;
        }
    }
}
