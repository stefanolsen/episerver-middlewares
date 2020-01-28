using Microsoft.Owin.Security.DataHandler;
using Microsoft.Owin.Security.DataHandler.Encoder;
using Microsoft.Owin.Security.DataProtection;

namespace StefanOlsen.Episerver.Owin.AnonymousId
{
    public class AnonymousIdDataFormat : SecureDataFormat<AnonymousId>
    {
        public AnonymousIdDataFormat(IDataProtector protector)
            : base(new AnonymousIdSerializer(), protector, TextEncodings.Base64Url)
        {
        }
    }
}