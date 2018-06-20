#region Copyright notice and license
// Protocol Buffers - Google's data interchange format
// Copyright 2015 Google Inc.  All rights reserved.
// https://developers.google.com/protocol-buffers/
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are
// met:
//
//     * Redistributions of source code must retain the above copyright
// notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above
// copyright notice, this list of conditions and the following disclaimer
// in the documentation and/or other materials provided with the
// distribution.
//     * Neither the name of Google Inc. nor the names of its
// contributors may be used to endorse or promote products derived from
// this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
// THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion

#define POOL
using System.IO;

namespace Google.Protobuf
{
    /// <summary>
    /// Extension methods on <see cref="IMessage"/> and <see cref="IMessage{T}"/>.
    /// </summary>
    public static class MessageExtensions
    {
#if POOL
        private static CodedOutputStream _sharedOutSteam;

        private static CodedInputStream _sharedInputSteam;
#endif

        private static CodedInputStream GetInputSteam( Stream  input)
        {
#if POOL
            if (_sharedInputSteam == null)
            {
                byte[] result = new byte[input.Length];
                _sharedInputSteam = new CodedInputStream(input, result,0, 0, false);
            }
            else
            {
                long size = input.Length;
                if (_sharedInputSteam.Capacity == size)
                {
                    _sharedInputSteam.Reset(input,0, 0, false);
                }
                else
                {
                    _sharedInputSteam.Reset(input, new byte[size],0, 0, false);
                }
            }
            return _sharedInputSteam;
#else
            return new CodedInputStream(input);
#endif
        }

        private static CodedInputStream GetInputSteam( byte[] data, int offset, int length)
        {
#if POOL
            if (_sharedInputSteam == null)
            {
                _sharedInputSteam = new CodedInputStream(data,offset,length);
            }
            else
            {
                _sharedInputSteam.Reset(data, offset,length, true);
            }
            return _sharedInputSteam;
#else
            return new CodedInputStream(data,offset,length);
#endif
        }

        private static CodedOutputStream GetOutSteam(IMessage message,Stream steam)
        {
#if POOL
            if(_sharedOutSteam == null)
            {
                _sharedOutSteam = new CodedOutputStream(steam,message.CalculateSize());
            }
            else
            {
                int size = message.CalculateSize();
                if (_sharedOutSteam.Capacity == size)
                {
                    _sharedOutSteam.Reset(steam,0, size);
                }
                else
                {
                    _sharedOutSteam.Reset(steam,new byte[size], 0,false);
                }
            }
            return _sharedOutSteam;
#else
            return new CodedOutputStream(steam, message.CalculateSize());
#endif
        }

        private static CodedOutputStream GetOutSteam( IMessage message,out byte[] result)
        {
#if POOL
            if (_sharedOutSteam == null)
            {
                result = new byte[message.CalculateSize()];
                _sharedOutSteam = new CodedOutputStream(result);
            }
            else
            {
                result = new byte[message.CalculateSize()];
                _sharedOutSteam.Reset(result, 0, result.Length);
            }
            return _sharedOutSteam;
#else
            result = new byte[message.CalculateSize()];
            return new CodedOutputStream(result);
#endif
        }
        /// <summary>
        /// clear Pool
        /// </summary>
        public static void PoolDispose()
        {
#if POOL
            if(_sharedInputSteam != null)
            {
                _sharedInputSteam.Dispose();
                _sharedInputSteam = null;
            }

            if (_sharedOutSteam != null)
            {
                _sharedOutSteam.Dispose();
                _sharedOutSteam = null;
            }
#endif
        }

        /// <summary>
        /// Merges data from the given byte array into an existing message.
        /// </summary>
        /// <param name="message">The message to merge the data into.</param>
        /// <param name="data">The data to merge, which must be protobuf-encoded binary data.</param>
        public static void MergeFrom(this IMessage message, byte[] data) =>
            MergeFrom(message, data, false);

        /// <summary>
        /// Merges data from the given byte array slice into an existing message.
        /// </summary>
        /// <param name="message">The message to merge the data into.</param>
        /// <param name="data">The data containing the slice to merge, which must be protobuf-encoded binary data.</param>
        /// <param name="offset">The offset of the slice to merge.</param>
        /// <param name="length">The length of the slice to merge.</param>
        public static void MergeFrom(this IMessage message, byte[] data, int offset, int length) =>
            MergeFrom(message, data, offset, length, false);

        /// <summary>
        /// Merges data from the given byte string into an existing message.
        /// </summary>
        /// <param name="message">The message to merge the data into.</param>
        /// <param name="data">The data to merge, which must be protobuf-encoded binary data.</param>
        public static void MergeFrom(this IMessage message, ByteString data) =>
            MergeFrom(message, data, false);

        /// <summary>
        /// Merges data from the given stream into an existing message.
        /// </summary>
        /// <param name="message">The message to merge the data into.</param>
        /// <param name="input">Stream containing the data to merge, which must be protobuf-encoded binary data.</param>
        public static void MergeFrom(this IMessage message, Stream input) =>
            MergeFrom(message, input, false);

        /// <summary>
        /// Merges length-delimited data from the given stream into an existing message.
        /// </summary>
        /// <remarks>
        /// The stream is expected to contain a length and then the data. Only the amount of data
        /// specified by the length will be consumed.
        /// </remarks>
        /// <param name="message">The message to merge the data into.</param>
        /// <param name="input">Stream containing the data to merge, which must be protobuf-encoded binary data.</param>
        public static void MergeDelimitedFrom(this IMessage message, Stream input) =>
            MergeDelimitedFrom(message, input, false);

        /// <summary>
        /// Converts the given message into a byte array in protobuf encoding.
        /// </summary>
        /// <param name="message">The message to convert.</param>
        /// <returns>The message data as a byte array.</returns>
        public static byte[] ToByteArray(this IMessage message)
        {
            ProtoPreconditions.CheckNotNull(message, "message");

            byte[] result = null;
            CodedOutputStream output = GetOutSteam(message,out result);
            message.WriteTo(output);
            output.CheckNoSpaceLeft();
            return result;
        }

        /// <summary>
        /// Writes the given message data to the given stream in protobuf encoding.
        /// </summary>
        /// <param name="message">The message to write to the stream.</param>
        /// <param name="output">The stream to write to.</param>
        /// <param name="leaveopen">The stream leaveopen.</param>
        public static void WriteTo(this IMessage message, Stream output)
        {
            ProtoPreconditions.CheckNotNull(message, "message");
            ProtoPreconditions.CheckNotNull(output, "output");
            CodedOutputStream codedOutput = GetOutSteam(message,output);
            message.WriteTo(codedOutput);
            codedOutput.Flush();
        }

        /// <summary>
        /// Writes the length and then data of the given message to a stream.
        /// </summary>
        /// <param name="message">The message to write.</param>
        /// <param name="output">The output stream to write to.</param>
        public static void WriteDelimitedTo(this IMessage message, Stream output)
        {
            ProtoPreconditions.CheckNotNull(message, "message");
            ProtoPreconditions.CheckNotNull(output, "output");
            CodedOutputStream codedOutput = GetOutSteam(message, output);
            codedOutput.WriteRawVarint32((uint)message.CalculateSize());
            message.WriteTo(codedOutput);
            codedOutput.Flush();
        }

        /// <summary>
        /// Converts the given message into a byte string in protobuf encoding.
        /// </summary>
        /// <param name="message">The message to convert.</param>
        /// <returns>The message data as a byte string.</returns>
        public static ByteString ToByteString(this IMessage message)
        {
            ProtoPreconditions.CheckNotNull(message, "message");
            return ByteString.AttachBytes(message.ToByteArray());
        }

        // Implementations allowing unknown fields to be discarded.
        internal static void MergeFrom(this IMessage message, byte[] data, bool discardUnknownFields)
        {
            ProtoPreconditions.CheckNotNull(message, "message");
            ProtoPreconditions.CheckNotNull(data, "data");
            CodedInputStream input = GetInputSteam(data,0,data.Length);
            input.DiscardUnknownFields = discardUnknownFields;
            message.MergeFrom(input);
            input.CheckReadEndOfStreamTag();
        }

        internal static void MergeFrom(this IMessage message, byte[] data, int offset, int length, bool discardUnknownFields)
        {
            ProtoPreconditions.CheckNotNull(message, "message");
            ProtoPreconditions.CheckNotNull(data, "data");
            CodedInputStream input = GetInputSteam(data, offset, length);
            input.DiscardUnknownFields = discardUnknownFields;
            message.MergeFrom(input);
            input.CheckReadEndOfStreamTag();
        }

        internal static void MergeFrom(this IMessage message, ByteString data, bool discardUnknownFields)
        {
            ProtoPreconditions.CheckNotNull(message, "message");
            ProtoPreconditions.CheckNotNull(data, "data");
            CodedInputStream input = data.CreateCodedInput();
            input.DiscardUnknownFields = discardUnknownFields;
            message.MergeFrom(input);
            input.CheckReadEndOfStreamTag();
        }

        internal static void MergeFrom(this IMessage message, Stream input, bool discardUnknownFields)
        {
            ProtoPreconditions.CheckNotNull(message, "message");
            ProtoPreconditions.CheckNotNull(input, "input");
            CodedInputStream codedInput = GetInputSteam( input);
            codedInput.DiscardUnknownFields = discardUnknownFields;
            message.MergeFrom(codedInput);
            codedInput.CheckReadEndOfStreamTag();
        }

        internal static void MergeDelimitedFrom(this IMessage message, Stream input, bool discardUnknownFields)
        {
            ProtoPreconditions.CheckNotNull(message, "message");
            ProtoPreconditions.CheckNotNull(input, "input");
            int size = (int) CodedInputStream.ReadRawVarint32(input);
            Stream limitedStream = new LimitedInputStream(input, size);
            MergeFrom(message, limitedStream, discardUnknownFields);
        }
    }
}
