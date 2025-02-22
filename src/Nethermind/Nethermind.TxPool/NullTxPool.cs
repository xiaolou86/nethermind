// SPDX-FileCopyrightText: 2022 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Nethermind.Core;
using Nethermind.Core.Crypto;
using Nethermind.Int256;

namespace Nethermind.TxPool
{
    public class NullTxPool : ITxPool
    {
        private NullTxPool() { }

        public static NullTxPool Instance { get; } = new();

        public int GetPendingTransactionsCount() => 0;
        public int GetPendingBlobTransactionsCount() => 0;
        public Transaction[] GetPendingTransactions() => Array.Empty<Transaction>();

        public Transaction[] GetPendingTransactionsBySender(Address address) => Array.Empty<Transaction>();

        public IDictionary<Address, Transaction[]> GetPendingTransactionsBySender()
            => new Dictionary<Address, Transaction[]>();

        public IDictionary<Address, Transaction[]> GetPendingLightBlobTransactionsBySender()
            => new Dictionary<Address, Transaction[]>();

        public IEnumerable<Transaction> GetPendingBlobTransactions() => Array.Empty<Transaction>();

        public void AddPeer(ITxPoolPeer peer) { }

        public void RemovePeer(PublicKey nodeId) { }

        public bool ContainsTx(Keccak hash, TxType txType) => false;

        public AcceptTxResult SubmitTx(Transaction tx, TxHandlingOptions txHandlingOptions) => AcceptTxResult.Accepted;

        public bool RemoveTransaction(Keccak? hash) => false;

        public bool IsKnown(Keccak hash) => false;

        public bool TryGetPendingTransaction(Keccak hash, out Transaction? transaction)
        {
            transaction = null;
            return false;
        }

        public bool TryGetPendingBlobTransaction(Keccak hash, [NotNullWhen(true)] out Transaction? blobTransaction)
        {
            blobTransaction = null;
            return false;
        }

        public UInt256 GetLatestPendingNonce(Address address) => 0;


        public event EventHandler<TxEventArgs> NewDiscovered
        {
            add { }
            remove { }
        }

        public event EventHandler<TxEventArgs> NewPending
        {
            add { }
            remove { }
        }

        public event EventHandler<TxEventArgs> RemovedPending
        {
            add { }
            remove { }
        }

        public event EventHandler<TxEventArgs>? EvictedPending
        {
            add { }
            remove { }
        }
    }
}
