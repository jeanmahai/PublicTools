using System;
using System.Collections.Generic;

namespace Soho.Utility
{
    public interface ILogEmitter
    {
        void Init(Dictionary<string, string> param);

        void EmitLog(LogEntry log);
    }
}
