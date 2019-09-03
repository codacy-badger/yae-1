using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace yae.Framing.Sample.BasicFrame
{
    /// <summary>
    /// Id: 4, Length: 4, Payload: Length
    /// </summary>
    public class BasicFrame
    {
        public int MessageId { get; set; }
        //public IMemoryOwner<byte> Payload { get; set; }
    }
}
