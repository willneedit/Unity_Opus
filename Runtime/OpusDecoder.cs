﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Codecs.Opus.Enums;

namespace Unity.Codecs.Opus
{
    public class OpusDecoder : IDisposable
    {
        private IntPtr _handle = IntPtr.Zero;
        //private string _version = string.Empty;
        private const int MaxFrameSize = 5760;
        private bool _previousPacketInvalid = false;
        private readonly int _channelCount = 2;

        /*public string Version
        {
            get
            {
                return _version;
            }
        }*/

        private Bandwidth? _previousPacketBandwidth = null;

        public Bandwidth? PreviousPacketBandwidth
        {
            get
            {
                return _previousPacketBandwidth;
            }
        }

        public OpusDecoder(SamplingRate outputSamplingRateHz, Channels numChannels)
        {
            if ((outputSamplingRateHz != SamplingRate.Sampling08000)
                && (outputSamplingRateHz != SamplingRate.Sampling12000)
                && (outputSamplingRateHz != SamplingRate.Sampling16000)
                && (outputSamplingRateHz != SamplingRate.Sampling24000)
                && (outputSamplingRateHz != SamplingRate.Sampling48000))
            {
                throw new ArgumentOutOfRangeException("outputSamplingRateHz", "Must use one of the pre-defined sampling rates");
            }
            if ((numChannels != Channels.Mono)
                && (numChannels != Channels.Stereo))
            {
                throw new ArgumentOutOfRangeException("numChannels", "Must be Mono or Stereo");
            }

            _channelCount = (int)numChannels;
            _handle = Wrapper.Opus_decoder_create(outputSamplingRateHz, numChannels);
            //_version = Wrapper.opus_get_version_string();

            if (_handle == IntPtr.Zero)
            {
                throw new OpusException(OpusStatusCode.AllocFail, "Memory was not allocated for the encoder");
            }
        }

        public short[] DecodePacketLost()
        {
            _previousPacketInvalid = true;

            short[] tempData = new short[MaxFrameSize * _channelCount];

            int numSamplesDecoded = Wrapper.Opus_decode(_handle, null, tempData, 0, _channelCount);

            if (numSamplesDecoded == 0)
                return new short[] { };

            short[] pcm = new short[numSamplesDecoded * _channelCount];
            Buffer.BlockCopy(tempData, 0, pcm, 0, pcm.Length * sizeof(short));

            return pcm;
        }

        public float[] DecodePacketLostFloat()
        {
            _previousPacketInvalid = true;

            float[] tempData = new float[MaxFrameSize * _channelCount];

            int numSamplesDecoded = Wrapper.Opus_decode(_handle, null, tempData, 0, _channelCount);

            if (numSamplesDecoded == 0)
                return new float[] { };

            float[] pcm = new float[numSamplesDecoded * _channelCount];
            Buffer.BlockCopy(tempData, 0, pcm, 0, pcm.Length * sizeof(float));

            return pcm;
        }

        private short[] tempDataShort = null;
        public short[] DecodePacket(byte[] packetData)
        {
            tempDataShort ??= new short[MaxFrameSize * _channelCount];

            int bandwidth = Wrapper.opus_packet_get_bandwidth(packetData);

            int numSamplesDecoded;
            if(bandwidth == (int)OpusStatusCode.InvalidPacket)
            {
                numSamplesDecoded = Wrapper.Opus_decode(_handle, null, tempDataShort, 0, _channelCount);
                _previousPacketInvalid = true;
            }
            else
            {
                _previousPacketBandwidth = (Bandwidth)bandwidth;
                numSamplesDecoded = Wrapper.Opus_decode(_handle, packetData, tempDataShort, _previousPacketInvalid ? 1 : 0, _channelCount);
                
                _previousPacketInvalid = false;
            }

            if (numSamplesDecoded == 0)
                return new short[] { };

            return tempDataShort[..(numSamplesDecoded * _channelCount)];
        }

        private float[] tempDataFloat = null;
        public float[] DecodePacketFloat(byte[] packetData)
        {
            tempDataFloat ??= new float[MaxFrameSize * _channelCount];

            int bandwidth = Wrapper.opus_packet_get_bandwidth(packetData);

            int numSamplesDecoded;
            if(bandwidth == (int)OpusStatusCode.InvalidPacket)
            {
                numSamplesDecoded = Wrapper.Opus_decode(_handle, null, tempDataFloat, 0, _channelCount);
                _previousPacketInvalid = true;
            }
            else
            {
                _previousPacketBandwidth = (Bandwidth)bandwidth;
                numSamplesDecoded = Wrapper.Opus_decode(_handle, packetData, tempDataFloat, _previousPacketInvalid ? 1 : 0, _channelCount);

                _previousPacketInvalid = false;
            }

            if (numSamplesDecoded == 0)
                return new float[] { };

            return tempDataFloat[..(numSamplesDecoded * _channelCount)];
        }

        public void Dispose()
        {
            if (_handle != IntPtr.Zero)
            {
                Wrapper.Opus_decoder_destroy(_handle);
                _handle = IntPtr.Zero;
            }
        }
    }
}
