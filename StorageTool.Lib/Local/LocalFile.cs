﻿using StorageTool.Lib.Interfaces;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace StorageTool.Lib.Local
{
    public class LocalFile : IDataObject
    {
        public string ShortName => Path.GetFileName(FullAddress);

        public string FullAddress { get; }

        public byte[] Hash => GetMd5();

        public long Size => GetSize();

        public DateTime UpdateTimestamp => File.GetLastWriteTimeUtc(FullAddress);

        public LocalFile(string path)
        {
            FullAddress = path;
        }

        public async Task<Stream> GetReadStreamAsync()
        {
            return await Task.Run(() => File.OpenRead(FullAddress)).ConfigureAwait(false);
        }

        public bool HashEquals(IDataObject other)
        {
            return Hash.SequenceEqual(other.Hash);
        }

        public async Task DeleteAsync(IUserFeedback feedback)
        {
            feedback.ObjectDeleteStarted(FullAddress);
            await Task.Run(() => File.Delete(FullAddress)).ConfigureAwait(false);
            feedback.ObjectDeleteFinished(FullAddress);
        }

        private byte[] GetMd5()
        {
            try
            {
                using (FileStream stream = File.OpenRead(FullAddress))
                using (MD5 hash = MD5.Create())
                {
                    hash.ComputeHash(stream);
                    return hash.Hash;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        private long GetSize()
        {
            try
            {
                FileInfo fileInfo = new FileInfo(FullAddress);
                return fileInfo.Length;
            }
            catch (Exception)
            {
                return -1;
            }
        }
    }
}
