using System;
using System.Collections.Generic;
using System.Text;

namespace yae.Framing
{
    public interface IDispatcher<TFrame, TMessage>
    {
        TMessage ToMessage(TFrame frame);
        void ToFrame(TMessage message); 

    }
}
