﻿/*
 * Copyright (c) 2018 Demerzel Solutions Limited
 * This file is part of the Nethermind library.
 *
 * The Nethermind library is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * The Nethermind library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with the Nethermind. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.IO;
using System.Numerics;
using Nethermind.Core.Crypto;
using Nethermind.Core.Extensions;
using Nethermind.Dirichlet.Numerics;

namespace Nethermind.Core.Encoding
{
    public class HeaderDecoder : IRlpValueDecoder<BlockHeader>, IRlpDecoder<BlockHeader>
    {
        public BlockHeader Decode(Rlp.ValueDecoderContext context, RlpBehaviors rlpBehaviors = RlpBehaviors.None)
        {
            if (context.IsNextItemNull())
            {
                return null;
            }

            var headerRlp = context.PeekNextItem();
            int headerSequenceLength = context.ReadSequenceLength();
            int headerCheck = context.Position + headerSequenceLength;

            Keccak parentHash = context.DecodeKeccak();
            Keccak ommersHash = context.DecodeKeccak();
            Address beneficiary = context.DecodeAddress();
            Keccak stateRoot = context.DecodeKeccak();
            Keccak transactionsRoot = context.DecodeKeccak();
            Keccak receiptsRoot = context.DecodeKeccak();
            Bloom bloom = context.DecodeBloom();
            UInt256 difficulty = context.DecodeUInt256();
            UInt256 number = context.DecodeUInt256();
            UInt256 gasLimit = context.DecodeUInt256();
            UInt256 gasUsed = context.DecodeUInt256();
            UInt256 timestamp = context.DecodeUInt256();
            byte[] extraData = context.DecodeByteArray();
            
            BlockHeader blockHeader = new BlockHeader(
                parentHash,
                ommersHash,
                beneficiary,
                difficulty,
                (long) number,
                (long) gasLimit,
                timestamp,
                extraData)
            {
                StateRoot = stateRoot,
                TxRoot = transactionsRoot,
                ReceiptsRoot = receiptsRoot,
                Bloom = bloom,
                GasUsed = (long) gasUsed,
                Hash = Keccak.Compute(headerRlp)
            };
            
            if (context.PeekPrefixAndContentLength().ContentLength == Keccak.Size)
            {
                blockHeader.MixHash = context.DecodeKeccak();
                blockHeader.Nonce = (ulong) context.DecodeUBigInt();
            }
            else
            {
                blockHeader.AuRaStep = (long) context.DecodeUInt256();
                blockHeader.AuRaSignature = context.DecodeByteArray();
            }
            
            if (!rlpBehaviors.HasFlag(RlpBehaviors.AllowExtraData))
            {
                context.Check(headerCheck);
            }

            return blockHeader;
        }

        public BlockHeader Decode(Rlp.DecoderContext context, RlpBehaviors rlpBehaviors = RlpBehaviors.None)
        {
            if (context.IsNextItemNull())
            {
                return null;
            }

            var headerRlp = context.PeekNextItem();
            int headerSequenceLength = context.ReadSequenceLength();
            int headerCheck = context.Position + headerSequenceLength;

            Keccak parentHash = context.DecodeKeccak();
            Keccak ommersHash = context.DecodeKeccak();
            Address beneficiary = context.DecodeAddress();
            Keccak stateRoot = context.DecodeKeccak();
            Keccak transactionsRoot = context.DecodeKeccak();
            Keccak receiptsRoot = context.DecodeKeccak();
            Bloom bloom = context.DecodeBloom();
            UInt256 difficulty = context.DecodeUInt256();
            UInt256 number = context.DecodeUInt256();
            UInt256 gasLimit = context.DecodeUInt256();
            UInt256 gasUsed = context.DecodeUInt256();
            UInt256 timestamp = context.DecodeUInt256();
            byte[] extraData = context.DecodeByteArray();
            
            BlockHeader blockHeader = new BlockHeader(
                parentHash,
                ommersHash,
                beneficiary,
                difficulty,
                (long) number,
                (long) gasLimit,
                timestamp,
                extraData)
            {
                StateRoot = stateRoot,
                TxRoot = transactionsRoot,
                ReceiptsRoot = receiptsRoot,
                Bloom = bloom,
                GasUsed = (long) gasUsed,
                Hash = Keccak.Compute(headerRlp)
            };
            
            if (context.PeekPrefixAndContentLength().ContentLength == Keccak.Size)
            {
                blockHeader.MixHash = context.DecodeKeccak();
                blockHeader.Nonce = (ulong) context.DecodeUBigInt();
            }
            else
            {
                blockHeader.AuRaStep = (long) context.DecodeUInt256();
                blockHeader.AuRaSignature = context.DecodeByteArray();
            }

            if (!rlpBehaviors.HasFlag(RlpBehaviors.AllowExtraData))
            {
                context.Check(headerCheck);
            }

            return blockHeader;
        }

        public Rlp Encode(BlockHeader item, RlpBehaviors rlpBehaviors = RlpBehaviors.None)
        {
            if (item == null)
            {
                return Rlp.OfEmptySequence;
            }

            bool isAuRa = item.AuRaSignature != null;
            bool withMixHashAndNonce = !rlpBehaviors.HasFlag(RlpBehaviors.ForSealing);
            int numberOfElements = isAuRa || withMixHashAndNonce ? 15 : 13;
            Rlp[] elements = new Rlp[numberOfElements];
            elements[0] = Rlp.Encode(item.ParentHash);
            elements[1] = Rlp.Encode(item.OmmersHash);
            elements[2] = Rlp.Encode(item.Beneficiary);
            elements[3] = Rlp.Encode(item.StateRoot);
            elements[4] = Rlp.Encode(item.TxRoot);
            elements[5] = Rlp.Encode(item.ReceiptsRoot);
            elements[6] = Rlp.Encode(item.Bloom);
            elements[7] = Rlp.Encode(item.Difficulty);
            elements[8] = Rlp.Encode((UInt256) item.Number);
            elements[9] = Rlp.Encode(item.GasLimit);
            elements[10] = Rlp.Encode(item.GasUsed);
            elements[11] = Rlp.Encode(item.Timestamp);
            elements[12] = Rlp.Encode(item.ExtraData);
            
            if (isAuRa)
            {
                elements[13] = Rlp.Encode(item.AuRaStep.Value);
                elements[14] = Rlp.Encode(item.AuRaSignature);                
            }
            else if (withMixHashAndNonce)
            {
                elements[13] = Rlp.Encode(item.MixHash);
                elements[14] = Rlp.Encode(item.Nonce);
            }

            Rlp rlp = Rlp.Encode(elements);
            
            return Rlp.Encode(elements);
        }

        public void Encode(MemoryStream stream, BlockHeader item, RlpBehaviors rlpBehaviors = RlpBehaviors.None)
        {
            if (item == null)
            {
                stream.Write(Rlp.OfEmptySequence.Bytes);
                return;
            }

            bool withMixHashAndNonce = !rlpBehaviors.HasFlag(RlpBehaviors.ForSealing);
            Rlp.StartSequence(stream, GetContentLength(item, rlpBehaviors));
            Rlp.Encode(stream, item.ParentHash);
            Rlp.Encode(stream, item.OmmersHash);
            Rlp.Encode(stream, item.Beneficiary);
            Rlp.Encode(stream, item.StateRoot);
            Rlp.Encode(stream, item.TxRoot);
            Rlp.Encode(stream, item.ReceiptsRoot);
            Rlp.Encode(stream, item.Bloom);
            Rlp.Encode(stream, item.Difficulty);
            Rlp.Encode(stream, item.Number);
            Rlp.Encode(stream, item.GasLimit);
            Rlp.Encode(stream, item.GasUsed);
            Rlp.Encode(stream, item.Timestamp);
            Rlp.Encode(stream, item.ExtraData);
            
            if (item.AuRaSignature != null)
            {
                Rlp.Encode(stream, item.AuRaStep.Value);
                Rlp.Encode(stream, item.AuRaSignature);
            }
            else if (withMixHashAndNonce)
            {
                Rlp.Encode(stream, item.MixHash);
                Rlp.Encode(stream, item.Nonce);
            }
        }

        private int GetContentLength(BlockHeader item, RlpBehaviors rlpBehaviors)
        {
            if (item == null)
            {
                return 0;
            }

            bool forSealing = (rlpBehaviors & RlpBehaviors.ForSealing) == RlpBehaviors.ForSealing;
            int contentLength = 0
                                + Rlp.LengthOf(item.ParentHash)
                                + Rlp.LengthOf(item.OmmersHash)
                                + Rlp.LengthOf(item.Beneficiary)
                                + Rlp.LengthOf(item.StateRoot)
                                + Rlp.LengthOf(item.TxRoot)
                                + Rlp.LengthOf(item.ReceiptsRoot)
                                + Rlp.LengthOf(item.Bloom)
                                + Rlp.LengthOf(item.Difficulty)
                                + Rlp.LengthOf(item.Number)
                                + Rlp.LengthOf(item.GasLimit)
                                + Rlp.LengthOf(item.GasUsed)
                                + Rlp.LengthOf(item.Timestamp)
                                + Rlp.LengthOf(item.ExtraData);
            
            if (item.AuRaSignature != null)
            {
                contentLength += Rlp.LengthOf(item.AuRaStep.Value);
                contentLength += Rlp.LengthOf(item.AuRaSignature);
            }
            else if (!forSealing)
            {
                contentLength += Rlp.LengthOf(item.MixHash) + Rlp.LengthOf(item.Nonce);
            }

            return contentLength;
        }

        public int GetLength(BlockHeader item, RlpBehaviors rlpBehaviors)
        {
            return Rlp.LengthOfSequence(GetContentLength(item, rlpBehaviors));
        }
    }
}