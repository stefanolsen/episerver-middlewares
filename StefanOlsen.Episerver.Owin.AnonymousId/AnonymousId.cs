using System;

namespace StefanOlsen.Episerver.Owin.AnonymousId
{
    public class AnonymousId
    {
        public AnonymousId(string id, DateTime expireDate)
        {
            Id = id;
            ExpireDate = expireDate;
        }

        public string Id { get; }
        public DateTime ExpireDate { get; }
    }
}