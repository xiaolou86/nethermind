// SPDX-FileCopyrightText: 2023 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only

using System.Threading.Tasks;
using Nethermind.Api;
using Nethermind.Blockchain.Services;
using Nethermind.Config;
using Nethermind.Consensus.Producers;
using Nethermind.Consensus.Validators;
using Nethermind.Evm;
using Nethermind.Evm.TransactionProcessing;
using Nethermind.Init.Steps;
using Nethermind.Merge.Plugin.InvalidChainTracker;

namespace Nethermind.Optimism;

public class InitializeBlockchainOptimism : InitializeBlockchain
{
    private readonly OptimismNethermindApi _api;
    private readonly IBlocksConfig _blocksConfig;

    public InitializeBlockchainOptimism(OptimismNethermindApi api) : base(api)
    {
        _api = api;
        _blocksConfig = api.Config<IBlocksConfig>();
    }

    protected override Task InitBlockchain()
    {
        _api.SpecHelper = new(
            _api.ChainSpec.Optimism.RegolithTimestamp,
            _api.ChainSpec.Optimism.BedrockBlockNumber,
            _api.ChainSpec.Optimism.L1FeeRecipient
        );
        _api.L1CostHelper = new(_api.SpecHelper, _api.ChainSpec.Optimism.L1BlockAddress);

        return base.InitBlockchain();
    }

    protected override ITransactionProcessor CreateTransactionProcessor()
    {
        if (_api.SpecProvider is null) throw new StepDependencyException(nameof(_api.SpecProvider));
        if (_api.WorldState is null) throw new StepDependencyException(nameof(_api.WorldState));
        if (_api.SpecHelper is null) throw new StepDependencyException(nameof(_api.SpecHelper));
        if (_api.L1CostHelper is null) throw new StepDependencyException(nameof(_api.L1CostHelper));

        VirtualMachine virtualMachine = CreateVirtualMachine();

        return new OptimismTransactionProcessor(
            _api.SpecProvider,
            _api.WorldState,
            virtualMachine,
            _api.LogManager,
            _api.L1CostHelper,
            _api.SpecHelper
        );
    }

    protected override IHeaderValidator CreateHeaderValidator()
    {
        if (_api.InvalidChainTracker is null) throw new StepDependencyException(nameof(_api.InvalidChainTracker));

        OptimismHeaderValidator opHeaderValidator = new(
            _api.BlockTree,
            _api.SealValidator,
            _api.SpecProvider,
            _api.LogManager);

        return new InvalidHeaderInterceptor(opHeaderValidator, _api.InvalidChainTracker, _api.LogManager);
    }

    protected override IBlockValidator CreateBlockValidator()
    {
        if (_api.InvalidChainTracker is null) throw new StepDependencyException(nameof(_api.InvalidChainTracker));
        if (_api.TxValidator is null) throw new StepDependencyException(nameof(_api.TxValidator));

        OptimismTxValidator txValidator = new(_api.TxValidator);
        BlockValidator blockValidator = new(
            txValidator,
            _api.HeaderValidator,
            _api.UnclesValidator,
            _api.SpecProvider,
            _api.LogManager);

        return new InvalidBlockInterceptor(blockValidator, _api.InvalidChainTracker, _api.LogManager);
    }

    protected override IUnclesValidator CreateUnclesValidator() => Always.Valid;

    protected override IHealthHintService CreateHealthHintService() =>
        new ManualHealthHintService(_blocksConfig.SecondsPerSlot * 6, HealthHintConstants.InfinityHint);

    protected override IBlockProductionPolicy CreateBlockProductionPolicy() => AlwaysStartBlockProductionPolicy.Instance;
}
