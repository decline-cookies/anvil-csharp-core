using System;
using System.IO;
using System.Threading;
using Anvil.CSharp.Command;

namespace Anvil.IO
{
    public class WriteToDiskCommand : AbstractCommand<WriteToDiskCommand>
    {
        public bool WasSuccessful
        {
            get;
            private set;
        }

        public Exception Exception
        {
            get;
            private set;
        }

        private readonly string m_Path;
        private readonly byte[] m_Data;
        private readonly CancellationTokenSource m_CancellationTokenSource;

        private FileStream m_FileStream;

        public WriteToDiskCommand(string path, byte[] data)
        {
            m_Path = path;
            m_Data = data;
            m_CancellationTokenSource = new CancellationTokenSource();
        }

        protected override void DisposeSelf()
        {
            m_CancellationTokenSource.Dispose();
            m_FileStream?.Dispose();
            
            base.DisposeSelf();
        }

        protected override void ExecuteCommand()
        {
            ExecuteAsync();
        }

        private async void ExecuteAsync()
        {
            string directoryPath = Path.GetDirectoryName(m_Path);
            if (!Directory.Exists(directoryPath))
            {
                //TODO: Handle null case
                Directory.CreateDirectory(directoryPath);
            }

            m_FileStream = File.Open(m_Path, FileMode.Create, FileAccess.Write, FileShare.Write);

            int totalBytes = m_Data.Length;

            try
            {
                await m_FileStream.WriteAsync(m_Data, 0, totalBytes, m_CancellationTokenSource.Token);
            }
            catch (Exception e)
            {
                WasSuccessful = false;
                Exception = e;
                CompleteCommand();
            }

            WasSuccessful = true;
            CompleteCommand();
        }
    }
}

