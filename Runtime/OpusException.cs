using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Codecs.Opus.Enums;

namespace Unity.Codecs.Opus
{
    public class OpusException : Exception
    {
        private OpusStatusCode _statusCode = OpusStatusCode.OK;

        public OpusStatusCode StatusCode
        {
            get
            {
                return _statusCode;
            }
        }

        public OpusException(OpusStatusCode statusCode, string message)
            : base(message)
        {
            _statusCode = statusCode;
        }
    }
}
