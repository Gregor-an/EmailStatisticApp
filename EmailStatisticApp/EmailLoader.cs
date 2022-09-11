using System;
using System.IO;
using Microsoft.Exchange.Data.Mime;
using Microsoft.Exchange.Data.Transport.Email;

namespace EmailStatisticApp
{
    public class EmailMessageLoader : IDisposable
    {
        private readonly MimeDocument _mimeDocument;

        public EmailMessageLoader(string emailPath, Stream emailContent = null)
        {
            if(emailContent == null)
            {
                using(Stream input = File.OpenRead(emailPath))
                {
                    input.CopyTo(EmailMessageStream);
                }
                emailContent = EmailMessageStream;
            }

            _mimeDocument = new MimeDocument(DecodingOptions.Default, MimeLimits.Unlimited);
            _mimeDocument.Load(emailContent, CachingMode.SourceTakeOwnership);
            EmailMessage = EmailMessage.Create(_mimeDocument);

            var bodyDispositionHeader = EmailMessage.Body.MimePart?.Headers.FindFirst("Content-Disposition");
            if(bodyDispositionHeader != null && bodyDispositionHeader.Value == string.Empty)
                bodyDispositionHeader.Value = "inline";
        }

        public Stream EmailMessageStream { get; } = new MemoryStream();

        public EmailMessage EmailMessage { get; }

        public string ReturnPath
        {
            get
            {
                return _mimeDocument?.RootPart.Headers.FindFirst(HeaderId.ReturnPath).Value;
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; 

        protected virtual void Dispose(bool disposing)
        {
            if(!disposedValue)
            {
                if(disposing)
                {
                    EmailMessageStream.Dispose();
                }

                disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
