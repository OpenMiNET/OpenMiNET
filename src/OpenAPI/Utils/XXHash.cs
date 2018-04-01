/*
xxHashSharp - A pure C# implementation of xxhash
Copyright (C) 2014, Seok-Ju, Yun. (https://github.com/noricube/xxHashSharp)
Original C Implementation Copyright (C) 2012-2014, Yann Collet. (https://code.google.com/p/xxhash/)
BSD 2-Clause License (http://www.opensource.org/licenses/bsd-license.php)
Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are
met:
    * Redistributions of source code must retain the above copyright
      notice, this list of conditions and the following disclaimer.
    * Redistributions in binary form must reproduce the above
      copyright notice, this list of conditions and the following
      disclaimer in the documentation and/or other materials provided
      with the distribution.
THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
"AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System;

namespace OpenAPI.Utils
{
	public class XxHash
	{
		public struct XxhState
		{
			public ulong TotalLen;
			public uint Seed;
			public uint V1;
			public uint V2;
			public uint V3;
			public uint V4;
			public int Memsize;
			public byte[] Memory;
		};

		const uint Prime321 = 2654435761U;
		const uint Prime322 = 2246822519U;
		const uint Prime323 = 3266489917U;
		const uint Prime324 = 668265263U;
		const uint Prime325 = 374761393U;

		protected XxhState State;
		public XxHash()
		{

		}

		public static uint CalculateHash(byte[] buf, int len = -1, uint seed = 0)
		{
			uint h32;
			int index = 0;
			if (len == -1)
			{
				len = buf.Length;
			}


			if (len >= 16)
			{
				int limit = len - 16;
				uint v1 = seed + Prime321 + Prime322;
				uint v2 = seed + Prime322;
				uint v3 = seed + 0;
				uint v4 = seed - Prime321;

				do
				{
					v1 = CalcSubHash(v1, buf, index);
					index += 4;
					v2 = CalcSubHash(v2, buf, index);
					index += 4;
					v3 = CalcSubHash(v3, buf, index);
					index += 4;
					v4 = CalcSubHash(v4, buf, index);
					index += 4;
				} while (index <= limit);

				h32 = RotateLeft(v1, 1) + RotateLeft(v2, 7) + RotateLeft(v3, 12) + RotateLeft(v4, 18);
			}
			else
			{
				h32 = seed + Prime325;
			}

			h32 += (uint)len;

			while (index <= len - 4)
			{
				h32 += BitConverter.ToUInt32(buf, index) * Prime323;
				h32 = RotateLeft(h32, 17) * Prime324;
				index += 4;
			}

			while (index < len)
			{
				h32 += buf[index] * Prime325;
				h32 = RotateLeft(h32, 11) * Prime321;
				index++;
			}

			h32 ^= h32 >> 15;
			h32 *= Prime322;
			h32 ^= h32 >> 13;
			h32 *= Prime323;
			h32 ^= h32 >> 16;

			return h32;
		}

		public void Init(uint seed = 0)
		{
			State.Seed = seed;
			State.V1 = seed + Prime321 + Prime322;
			State.V2 = seed + Prime322;
			State.V3 = seed + 0;
			State.V4 = seed - Prime321;
			State.TotalLen = 0;
			State.Memsize = 0;
			State.Memory = new byte[16];
		}

		public bool Update(byte[] input, int len)
		{
			int index = 0;

			State.TotalLen += (uint)len;

			if (State.Memsize + len < 16)
			{
				Array.Copy(input, 0, State.Memory, State.Memsize, len);
				State.Memsize += len;

				return true;
			}

			if (State.Memsize > 0)
			{
				Array.Copy(input, 0, State.Memory, State.Memsize, 16 - State.Memsize);

				State.V1 = CalcSubHash(State.V1, State.Memory, index);
				index += 4;
				State.V2 = CalcSubHash(State.V2, State.Memory, index);
				index += 4;
				State.V3 = CalcSubHash(State.V3, State.Memory, index);
				index += 4;
				State.V4 = CalcSubHash(State.V4, State.Memory, index);
				index += 4;

				index = 0;
				State.Memsize = 0;
			}

			if (index <= len - 16)
			{
				int limit = len - 16;
				uint v1 = State.V1;
				uint v2 = State.V2;
				uint v3 = State.V3;
				uint v4 = State.V4;

				do
				{
					v1 = CalcSubHash(v1, input, index);
					index += 4;
					v2 = CalcSubHash(v2, input, index);
					index += 4;
					v3 = CalcSubHash(v3, input, index);
					index += 4;
					v4 = CalcSubHash(v4, input, index);
					index += 4;
				} while (index <= limit);

				State.V1 = v1;
				State.V2 = v2;
				State.V3 = v3;
				State.V4 = v4;
			}

			if (index < len)
			{
				Array.Copy(input, index, State.Memory, 0, len - index);
				State.Memsize = len - index;
			}
			return true;
		}

		public uint Digest()
		{
			uint h32;
			int index = 0;
			if (State.TotalLen >= 16)
			{
				h32 = RotateLeft(State.V1, 1) + RotateLeft(State.V2, 7) + RotateLeft(State.V3, 12) + RotateLeft(State.V4, 18);
			}
			else
			{
				h32 = State.Seed + Prime325;
			}

			h32 += (UInt32)State.TotalLen;

			while (index <= State.Memsize - 4)
			{
				h32 += BitConverter.ToUInt32(State.Memory, index) * Prime323;
				h32 = RotateLeft(h32, 17) * Prime324;
				index += 4;
			}

			while (index < State.Memsize)
			{
				h32 += State.Memory[index] * Prime325;
				h32 = RotateLeft(h32, 11) * Prime321;
				index++;
			}

			h32 ^= h32 >> 15;
			h32 *= Prime322;
			h32 ^= h32 >> 13;
			h32 *= Prime323;
			h32 ^= h32 >> 16;

			return h32;
		}

		private static uint CalcSubHash(uint value, byte[] buf, int index)
		{
			uint readValue = BitConverter.ToUInt32(buf, index);
			value += readValue * Prime322;
			value = RotateLeft(value, 13);
			value *= Prime321;
			return value;
		}

		private static uint RotateLeft(uint value, int count)
		{
			return (value << count) | (value >> (32 - count));
		}
	}
}
