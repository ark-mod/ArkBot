﻿using System;

namespace ArkBot.Modules.Application.Services.Data
{
    public class BackupListEntity
    {
        private string[] _files;

        public string Path { get; set; }
        public string FullPath { get; set; }
        public long ByteSize { get; set; }
        public DateTime DateModified { get; set; }
        public string[] Files
        {
            get
            {
                return _files ?? LazyFiles.Value;
            }

            set
            {
                _files = value;
            }
        }
        public Lazy<string[]> LazyFiles { get; set; }
    }

    public class FromServerBackupListEntity : BackupListEntity
    {

    }
}
