using System;
using System.IO;
using Anvil.CSharp.Command;

namespace Anvil.IO
{
    public class LoadFromDiskCommand : AbstractCommand<LoadFromDiskCommand>, ISuccessfulCommand
    {
        public bool WasSuccessful
        {
            get;
            private set;
        }

        public AggregateException Exception
        {
            get;
            private set;
        }

        public byte[] Data
        {
            get;
            private set;
        }
        
        private readonly string m_Path;

        private FileStream m_FileStream;

        public LoadFromDiskCommand(string path)
        {
            m_Path = path;
        }

        protected override void DisposeSelf()
        {
            m_FileStream?.Dispose();
            
            base.DisposeSelf();
        }

        protected override void ExecuteCommand()
        {
            ExecuteAsync();
        }

        private async void ExecuteAsync()
        {
            if (!File.Exists(m_Path))
            {
                WasSuccessful = false;
                Exception = new AggregateException(new FileNotFoundException($"File at path {m_Path} doesn't exist!"));
                CompleteCommand();
            }

            m_FileStream = File.Open(m_Path, FileMode.Open, FileAccess.Read, FileShare.Read);

            int totalBytes = (int)m_FileStream.Length;
            
            Data = new byte[totalBytes];

            try
            {
                await m_FileStream.ReadAsync(Data, 0, totalBytes);
            }
            catch (Exception e)
            {
                WasSuccessful = false;
                Exception = new AggregateException(e);
                CompleteCommand();
            }

            WasSuccessful = true;
            CompleteCommand();
        }
    }
}

